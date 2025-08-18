using BankAccountsApi.Behaviors;
using BankAccountsApi.Hangfire;
using BankAccountsApi.Infrastructure.Bus;
using BankAccountsApi.Storage;
using BankAccountsApi.Storage.Interfaces;
using FluentValidation;
using Hangfire;
using Hangfire.PostgreSql;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RabbitMQ.Client;
using System.Reflection;
using System.Text.Json.Serialization;

namespace BankAccountsApi
{
    public class Program
    {
        private static readonly string[] value = ["openid"];

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Configuration
                   .SetBasePath(Directory.GetCurrentDirectory())
                   .AddJsonFile("app/appsettings.json", optional: false, reloadOnChange: true)
                   .AddJsonFile($"app/appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
                   .AddEnvironmentVariables();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.SetIsOriginAllowed(_ => true)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Swagger с Keycloak
            builder.Services.AddSwaggerGen(c =>
            {
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);

                c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Password = new OpenApiOAuthFlow
                        {
                            TokenUrl = new Uri("http://localhost:8080/realms/bank-accounts-realm/protocol/openid-connect/token"),
                            Scopes = new Dictionary<string, string> { { "openid", "OpenID" } }
                        }
                    }
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "oauth2"
                            }
                        },
                        value }
                });
            });

            // Аутентификация
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = builder.Configuration["Authentication:Authority"];
                    options.Audience = builder.Configuration["Authentication:Audience"];
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        ValidateLifetime = false,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                });

            // Регистрация контроллеров
            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            // Подключение к базе с правильным ключом строки подключения
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnection")));

            builder.Services.AddScoped<IAccountsRepository, AccountsRepository>();
            builder.Services.AddScoped<IClientsRepository, ClientsRepository>();
            builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
            builder.Services.AddScoped<ITransactionsRepository, TransactionsRepository>();
            builder.Services.AddScoped<IOutboxRepository, OutboxRepository>();

            builder.Services.AddMediatR(typeof(Program));
            builder.Services.AddAuthorization();

            // Hangfire
            builder.Services.AddHangfire(config =>
            {
                config.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DbConnection"));
            });
            builder.Services.AddHangfireServer();
            builder.Services.AddHttpClient();

            var rabbitConfig = builder.Configuration.GetSection("RabbitMQ");
            var amqpUri = $"amqp://{rabbitConfig["UserName"]}:{rabbitConfig["Password"]}@{rabbitConfig["Endpoint"]}";

            builder.Services.AddSingleton<RabbitMQ.Client.IConnectionFactory>(sp =>
                 new ConnectionFactory { Uri = new Uri(amqpUri) });

            builder.Services.AddSingleton(rabbitConfig.GetSection("EventRouting")
                                               .Get<Dictionary<string, string>>()!);

            builder.Services.AddSingleton<MessageBus>();
            builder.Services.AddHostedService<AntifraudConsumer>();
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            var app = builder.Build();

            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();

            app.UseHangfireDashboard();

            app.UseMiddleware<ValidationExceptionMiddleware>();

            app.UseRouting();
            app.UseCors("AllowAll");

            RecurringJob.AddOrUpdate<InterestService>(
                "CalculateInterestJob",
                service => service.CalculateInterestAsync(),
                Cron.Minutely
            );

            RecurringJob.AddOrUpdate<MessageBus>(
                "PublishOutboxEvents",
                x => x.PublishPendingEventsAsync(),
                Cron.Minutely);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bank API v1");
                c.OAuthClientId("bank-accounts-api");
                c.OAuthAppName("Bank API");
                c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
                {
                    { "scope", "openid" }
                });
            });

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
