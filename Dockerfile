FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY BankAccountsApi.csproj ./
RUN dotnet restore BankAccountsApi.csproj

COPY . ./
RUN dotnet publish BankAccountsApi.csproj -c Release -o /publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Устанавливаем curl и ping
RUN apt-get update && apt-get install -y curl iputils-ping && rm -rf /var/lib/apt/lists/*

COPY --from=build /publish ./
COPY app/appsettings.LocalDocker.json ./

ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

ENTRYPOINT ["dotnet", "BankAccountsApi.dll"]
