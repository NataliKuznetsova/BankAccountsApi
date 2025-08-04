FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src

COPY BankAccountsApi.csproj . 

RUN dotnet restore BankAccountsApi.csproj

COPY . .

RUN dotnet publish BankAccountsApi.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS runtime
WORKDIR /app

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "BankAccountsApi.dll"]
