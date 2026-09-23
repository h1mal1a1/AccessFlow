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
```

## Работа с подключениями

PostgreSQL является источником истины для состояния подключений.

Подключение может находиться в одном из состояний:

- `Pending` — запись создана в БД, но подключение ещё не подтверждено на VPS;
- `Active` — подключение существует в БД и на VPS;
- `Deleting` — удаление запрошено, но операция на VPS ещё не завершена;
- `Deleted` — подключение считается удалённым.

Для операций, затрагивающих БД и VPS, используется Outbox Pattern.

Пример создания подключения:

```text
HTTP POST
   ↓
Connection (Pending)
   ↓
Outbox (ConnectionCreate)
   ↓
COMMIT
   ↓
Worker
   ↓
3X-UI
   ↓
Connection (Active)
```

Это позволяет не выполнять внешние запросы к VPS внутри транзакции PostgreSQL и повторять операцию при временных ошибках.

## Интеграция с VPS

Для управления подключениями используется API 3X-UI.

Интеграция поддерживает:

- получение подключения;
- создание подключения;
- переименование подключения;
- удаление подключения;
- получение UUID;
- получение строки подключения;
- получение subscription URL.

Один `Connection` в AccessFlow соответствует одному client в 3X-UI.

## Outbox

В таблице `outbox_messages` хранятся операции, которые должны быть выполнены во внешней системе.

Поддерживаемые типы сообщений:

```text
ConnectionCreate
ConnectionUpdate
ConnectionDelete
```

Сообщение содержит:

- тип операции;
- JSON payload;
- статус;
- количество попыток;
- время создания и обработки;
- последнюю ошибку.

Worker для обработки Outbox находится в разработке.

## Структура проекта

```text
AccessFlow/
├── backend/
│   ├── AccessFlow.Api/
│   ├── AccessFlow.Application/
│   ├── AccessFlow.Domain/
│   ├── AccessFlow.Infrastructure/
│   ├── AccessFlow.sln
│   └── Dockerfile
├── tests/
│   └── AccessFlow.IntegrationTests/
├── pg/
│   └── data/
├── docker-compose.yml
└── README.md
```

Каталог `pg/data` используется для локального хранения данных PostgreSQL и не должен добавляться в Git.

Файлы с секретами и локальной конфигурацией также не должны добавляться в репозиторий.

## Технологии

- C#
- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- 3X-UI API
- Docker
- Docker Compose
- xUnit

## Следующие этапы

Ближайшая задача проекта — реализация Outbox worker'а и надёжной синхронизации состояния подключений между PostgreSQL и VPS.
