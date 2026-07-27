# AccessFlow

AccessFlow — сервис для управления пользовательскими подключениями на VPS и отправки строк подключения пользователям по электронной почте.

Проект находится в разработке.

## Планируемый функционал

1. CRUD клиентов в базе данных.
2. CRUD подключений через API VPS.
3. Отправка уведомлений пользователям.
4. Массовое пересоздание подключений.
5. Хранение статусов и ошибок операций.

## Архитектура

Проект построен по принципам Clean Architecture:

- `AccessFlow.Domain` — сущности и бизнес-модели;
- `AccessFlow.Application` — сценарии использования и интерфейсы;
- `AccessFlow.Infrastructure` — PostgreSQL, EF Core и внешние интеграции;
- `AccessFlow.Api` — ASP.NET Core Web API.

## Структура проекта

```text
AccessFlow/
├── backend/
│   ├── AccessFlow.Api/
│   ├── AccessFlow.Application/
│   ├── AccessFlow.Domain/
│   ├── AccessFlow.Infrastructure/
│   ├── AccessFlow.sln
│   ├── Dockerfile
│   └── .dockerignore
├── pg/
│   └── data/
├── docker-compose.yml
├── .env.backend
├── .env.db
└── README.md
```

Каталог `pg/data` используется для хранения данных PostgreSQL на локальной машине и не должен добавляться в Git.

## Технологии

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Docker
- Docker Compose

Данные PostgreSQL сохраняются в локальном каталоге `pg/data`.

## Внешняя интеграция

Позднее AccessFlow будет подключён к API VPS. Через него сервис сможет:

- создавать подключения;
- удалять подключения;
- пересоздавать подключения;
- получать строки подключения;
- выполнять массовые операции.