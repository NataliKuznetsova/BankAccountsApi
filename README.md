BankAccountsApi

Реализация REST API для управления банковскими счетами и транзакциями

Что в проекте
- CRUD для счетов
- Регистрация транзакций, перевод между счетами
- Хранение в PostgreSQL (через EF Core)
- Аутентификация JWT (Keycloak)
- Документация Swagger

Swagger будет доступен по адресу: `http://localhost/swagger/index.html`

Запуск Docker:
docker compose up --build
прогон миграций
dotnet ef database update

для авторизации:
user - user
password - password
client-secret - secret

при прохождении интеграционного теста сначала запустить докер. потом тест