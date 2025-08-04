using System.Reflection;
using System.Text.Json.Serialization;
using BankAccountsApi.Behaviors;
using BankAccountsApi.Storage;
using BankAccountsApi.Storage.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

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

// Настройка Swagger с Keycloak
builder.Services.AddSwaggerGen(c =>
{
    // XML документация
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    // Конфигурация OAuth2
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
            new[] { "openid" }
        }
    });
});

// Настройка аутентификации
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://keycloak:8080/realms/bank-accounts-realm";
        options.Audience = "bank-accounts-api";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Регистрация сервисов
builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddSingleton<IInMemoryTransactionStorage, InMemoryTransactionStorage>();
builder.Services.AddSingleton<IInMemoryClientStorage, InMemoryClientStorage>();
builder.Services.AddSingleton<IInMemoryAccountStorage, InMemoryAccountStorage>();
builder.Services.AddSingleton<IInMemoryCurrencyStorage, InMemoryCurrencyStorage>();
builder.Services.AddMediatR(typeof(Program));
builder.Services.AddAuthorization();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ValidationExceptionMiddleware>();

app.UseRouting();
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bank API v1");
    c.OAuthClientId("bank-accounts-api");
    c.OAuthClientSecret("secret");
    c.OAuthAppName("Bank API"); c.OAuthConfigObject = new OAuthConfigObject
    {
        ClientId = "bank-accounts-api",
        ClientSecret = "secret",
        AdditionalQueryStringParams = new Dictionary<string, string>
        {
            { "grant_type", "password" },
        }
    };
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();