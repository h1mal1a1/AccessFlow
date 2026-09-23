# AccessFlow

AccessFlow — backend-сервис для управления клиентами и пользовательскими подключениями на VPS.

Сервис построен на .NET 8 и использует PostgreSQL, Entity Framework Core и интеграцию с 3X-UI API.

Проект находится в разработке.

## Реализовано

- CRUD клиентов;
- CRUD подключений;
- интеграция с 3X-UI API;
- создание, получение, переименование и удаление подключений на VPS;
- soft delete клиентов и подключений;
- хранение статусов подключений;
- Outbox для операций с VPS;
- транзакционное создание Outbox-сообщений вместе с изменениями в БД;
- пагинация;
- обработка ошибок через Problem Details;
- integration tests;
- запуск PostgreSQL и приложения через Docker Compose.

## В разработке

- Outbox worker для обработки операций:
  - создания подключения;
  - обновления подключения;
  - удаления подключения;
- retry-механизм для временных ошибок VPS;
- отправка строк подключения пользователям по электронной почте;
- массовое пересоздание подключений;
- расширение integration tests под асинхронную обработку через Outbox.

## Архитектура

Проект разделён на четыре основных слоя:

- `AccessFlow.Domain` — сущности, enum'ы и доменные модели;
- `AccessFlow.Application` — бизнес-сценарии, DTO и абстракции;
- `AccessFlow.Infrastructure` — PostgreSQL, EF Core, репозитории и интеграция с 3X-UI;
- `AccessFlow.Api` — ASP.NET Core Web API и HTTP-контракты.

```text
API
 ↓
Application
 ↓
Domain

Infrastructure
 ↳ PostgreSQL / EF Core
 ↳ 3X-UI API
