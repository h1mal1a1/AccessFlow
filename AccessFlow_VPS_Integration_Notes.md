# AccessFlow — интеграция с 3X-UI/VPS

Дата актуализации: 2026-09-13

## 1. Общая идея

AccessFlow хранит пользователей и связь между пользователями и VPN-подключениями.

3X-UI не знает про пользователей AccessFlow. В 3X-UI используется один постоянный inbound `main`, внутри которого создаются отдельные clients.

Соответствие:

```text
AccessFlow Client
    ├── Connection #1 → 3X-UI client
    ├── Connection #2 → 3X-UI client
    └── Connection #3 → 3X-UI client
```

Один `Connection` в AccessFlow соответствует одному client в 3X-UI.

---

## 2. Inbound

В 3X-UI используется один постоянный inbound:

```text
remark = main
```

Все подключения создаются внутри него.

Поэтому `InboundId` в сущности `Connection` хранить не нужно.

AccessFlow находит inbound `main` через API 3X-UI и использует его numeric ID при создании клиентов.

---

## 3. Сущность Client

`Client` в AccessFlow — это реальный человек.

Текущая модель:

```csharp
public class Client
{
    public long Id { get; set; }
    public required string Email { get; set; }
    public required ClientStatus Status { get; set; }
    public required string PhoneNumber { get; set; }
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public ICollection<Connection> Connections { get; set; } = [];
    public ICollection<Notification> Notifications { get; set; } = [];
    public ICollection<BulkOperationItem> BulkOperationItems { get; set; } = [];
}
```

`Client.Email` — настоящий email пользователя для будущей рассылки уведомлений.

---

## 4. Сущность Connection

Текущая модель:

```csharp
public class Connection
{
    public long Id { get; set; }
    public long IdClient { get; set; }

    public required string IdExternal { get; set; }
    public required string Name { get; set; }
    public required string ConnectionString { get; set; }
    public required string SubUrl { get; set; }
    public required ConnectionStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Client Client { get; set; } = null!;
    public ICollection<Notification> Notifications { get; set; } = [];
}
```

Принятое соответствие:

```text
Connection.Name
    = 3X-UI client.email
    = человекочитаемое обозначение подключения

Connection.IdExternal
    = UUID клиента в 3X-UI

Connection.ConnectionString
    = готовая строка подключения
    = хранится в БД AccessFlow

Connection.SubUrl
    = subscription URL
    = хранится в БД AccessFlow
```

`IdExternal` оставляем.

`ConnectionString` и `SubUrl` тоже оставляем, потому что уведомления должны брать данные из нашей БД и не зависеть от доступности API 3X-UI во время рассылки.

---

## 5. Name / 3X-UI email

Поле `email` в 3X-UI используется как техническое имя клиента и ключ для API.

Например:

```text
GET  /panel/api/clients/get/{email}
POST /panel/api/clients/update/{email}
POST /panel/api/clients/del/{email}
GET  /panel/api/clients/links/{email}
```

В AccessFlow этому соответствует:

```text
Connection.Name
```

При Update 3X-UI требует текущее значение `email` в path. UUID в path для update не поддерживается.

При переименовании client его UUID сохраняется.

---

## 6. Источник истины

Ответственность разделена.

### AccessFlow DB хранит

- реального пользователя;
- связь `Client → Connection`;
- `Connection.Status`;
- `ConnectionString`;
- `SubUrl`;
- `CreatedAt`;
- `UpdatedAt`;
- данные, необходимые для уведомлений.

### 3X-UI является источником истины по техническому состоянию VPN

- существует ли client фактически;
- UUID;
- технические параметры;
- актуальные connection links;
- состояние VPN client.

Если AccessFlow и 3X-UI расходятся по техническим данным, БД AccessFlow должна актуализироваться на основе 3X-UI.

---

## 7. Создание Connection в VPS

На уровне `IVpsClient` реализован сценарий создания клиента в 3X-UI:

```text
1. Найти inbound по `remark = main`.
2. Создать client в найденном inbound.
3. Получить созданного client через API 3X-UI.
4. Получить connection link.
5. Построить subscription URL.
6. Вернуть `VpsConnectionInfo`.
```

`CreateConnectionAsync(...)` возвращает:

```text
IdExternal
Name
ConnectionString
SubUrl
```

Если client с таким `email` уже существует, ошибка 3X-UI преобразуется в `VpsErrorType.Conflict`.

Если 3X-UI успешно создал client, но сохранение в PostgreSQL в будущем завершится ошибкой, потребуется компенсирующее удаление созданного client из 3X-UI.

---

## 8. Чтение Connection из VPS

Реализован:

```text
IVpsClient.GetConnectionAsync(name, ...)
```

Сценарий:

```text
GET client по email
→ получить UUID / subId
→ GET connection links
→ взять первый link как ConnectionString
→ построить SubUrl
→ вернуть VpsConnectionInfo
```

Если client отсутствует, метод возвращает `null`.

`null` используется только для реального отсутствия client. Ошибки интеграции не скрываются как `null`.

---

## 9. Обновление Connection в VPS

Реализован:

```text
IVpsClient.UpdateConnectionAsync(currentName, newName, ...)
```

3X-UI обновляет client через:

```text
POST /panel/api/clients/update/{currentName}
```

В текущей бизнес-логике меняется только `email`.

Тело update:

```json
{
  "email": "newName"
}
```

Для update используется отдельный DTO:

```text
ThreeXUiUpdateClientDto
```

Другие параметры client не передаются и не изменяются.

После успешного update выполняется `GetConnectionAsync(newName, ...)`, чтобы вернуть актуальный `VpsConnectionInfo`.

Проверено:

```text
currentName отсутствует
→ VpsErrorType.NotFound

newName уже занят другим client
→ VpsErrorType.Conflict

успешное переименование
→ UUID client сохраняется
```

---

## 10. Удаление Connection в VPS

Реализован:

```text
IVpsClient.DeleteConnectionAsync(name, ...)
```

3X-UI удаляет client через:

```text
POST /panel/api/clients/del/{email}
```

Проверено:

```text
успешное удаление
→ success = true

client отсутствует
→ success = false
→ VpsErrorType.NotFound
```

На уровне будущего Application-сценария нужно будет синхронизировать удаление в VPS и изменение состояния в PostgreSQL.

Точный порядок и поведение при частичных ошибках будет определён отдельно при проектировании взаимодействия БД и VPS.

---

## 11. HTTP API AccessFlow для VPS

Добавлен отдельный контроллер:

```text
VpsConnectionsController
```

Ручки:

```text
GET    /api/vps/connections/{name}
POST   /api/vps/connections
PUT    /api/vps/connections/{name}
DELETE /api/vps/connections/{name}
```

Поддерживаемые операции:

```text
Create
Read
Update
Delete
```

`GET all` и `GET deleted` для VPS сейчас не реализуются.

3X-UI физически удаляет client, поэтому отдельного VPS-endpoint для soft-deleted clients нет.

---

## 12. Синхронизация AccessFlow ↔ 3X-UI

Нужна отдельная синхронизация состояния.

### Случай 1

```text
Connection есть в AccessFlow,
но соответствующего client уже нет в 3X-UI
```

Действие:

```text
Connection.Status = Deleted
```

### Случай 2

```text
client есть в 3X-UI,
но Connection отсутствует в AccessFlow
```

Действие:

```text
создать нового Client в AccessFlow
→ создать ему Connection
```

Для автоматически импортированного Client:

```text
Email       = "unknown"
PhoneNumber = "unknown"
Comment     = "Imported from 3X-UI"
Status      = Active
```

Для Connection:

```text
Name             = 3X-UI client.email
IdExternal       = UUID клиента 3X-UI
ConnectionString = строка подключения
SubUrl           = subscription URL
Status           = Active
```

`IdExternal` используется, чтобы при следующих синхронизациях узнавать уже импортированный connection и не создавать дубликаты.

---

## 13. Правило для unknown-клиентов

Если клиент был автоматически импортирован из 3X-UI и имеет:

```text
Email = "unknown"
```

то уведомления ему не отправляются.

---

## 14. API 3X-UI

Авторизация:

```text
Authorization: Bearer <API_TOKEN>
```

API token хранится только в секретах/env и никогда не коммитится в Git.

Проверено на реальном VPS, что работают:

```text
GET  /panel/api/inbounds/list
GET  /panel/api/clients/get/{email}
GET  /panel/api/clients/links/{email}
POST /panel/api/clients/add
POST /panel/api/clients/update/{email}
POST /panel/api/clients/del/{email}
```

Дополнительный endpoint для inbound:

```text
GET /panel/api/inbounds/get/{id}
```

При создании клиента 3X-UI используется структура примерно такого вида:

```json
{
  "client": {
    "email": "pc",
    "totalGB": 0,
    "expiryTime": 0,
    "tgId": 0,
    "limitIp": 0,
    "enable": true
  },
  "inboundIds": [3]
}
```

При update текущая реализация отправляет только:

```json
{
  "email": "newName"
}
```

Числовой ID inbound не должен храниться в `Connection`, так как используется постоянный inbound `main`.

Реальный ответ `GET /panel/api/clients/get/{email}` содержит client внутри `obj.client`, поэтому Infrastructure использует отдельные DTO для внешнего контракта 3X-UI.

`obj` в generic response является nullable, так как при `success = false` 3X-UI может вернуть `obj = null`.

---

## 15. Конфигурация AccessFlow

Используемые настройки:

```text
Vps__BaseUrl
Vps__ApiToken
Vps__InboundRemark=main
```

Если понадобится:

```text
Vps__TimeoutSeconds
```

Числовой inbound ID получается по `remark = main`, а не хранится жёстко в БД.

---

## 16. Обработка ошибок VPS

Используются:

```text
VpsException
VpsErrorType
```

Типы ошибок:

```text
Conflict
NotFound
InvalidResponse
OperationFailed
Unavailable
Configuration
```

Маппинг в HTTP:

```text
Conflict        → 409
NotFound        → 404
InvalidResponse → 502
OperationFailed → 502
Unavailable     → 503
Configuration   → 502
```

`ThreeXUiHelper` централизует:

```text
отправку HTTP-запросов
→ network / timeout / 5xx

проверку HTTP status
→ 4xx

десериализацию JSON
→ empty / invalid / unsupported response
```

Business errors 3X-UI обрабатываются в конкретных CRUD-методах.

---

## 17. Текущее состояние реализации

На текущем этапе реализовано:

```text
Application
→ IVpsClient.GetConnectionAsync(...)
→ IVpsClient.CreateConnectionAsync(...)
→ IVpsClient.UpdateConnectionAsync(...)
→ IVpsClient.DeleteConnectionAsync(...)
→ VpsConnectionInfo
→ VpsException + VpsErrorType

Infrastructure
→ ThreeXUiClient
→ ThreeXUiHelper
→ typed HttpClient с BaseAddress и Bearer token
→ поиск inbound по Vps__InboundRemark
→ получение client
→ получение connection links
→ построение SubUrl
→ создание client
→ изменение email client
→ удаление client
→ обработка network/timeout/5xx
→ обработка 4xx
→ обработка некорректного JSON
→ обработка business errors 3X-UI

API
→ VpsConnectionsController
→ GET /api/vps/connections/{name}
→ POST /api/vps/connections
→ PUT /api/vps/connections/{name}
→ DELETE /api/vps/connections/{name}
```

Полный VPS CRUD проверен вручную на реальном VPS:

```text
Create
→ Get
→ Update
→ Get по новому имени
→ Delete
→ Get после удаления
```

При Update UUID клиента сохраняется.

Update с телом только `{ "email": "newName" }` также проверен отдельно.

Реальный VPS используется только для smoke/manual checks. Автоматические тесты не должны зависеть от его доступности.

---

## 18. Следующий этап разработки

VPS CRUD завершён.

Следующий отдельный архитектурный блок — продумать взаимодействие VPS и PostgreSQL.

Предварительные задачи:

```text
1. Встроить IVpsClient.CreateConnectionAsync(...) в Application-сценарий создания Connection.
2. Сохранять IdExternal / Name / ConnectionString / SubUrl в PostgreSQL.
3. Определить порядок Update между VPS и PostgreSQL.
4. Определить порядок Delete между VPS и PostgreSQL.
5. Добавить компенсацию, если создание client в VPS прошло успешно, а сохранение Connection в БД завершилось ошибкой.
6. Продумать поведение при частичных ошибках Update/Delete.
7. Реализовать синхронизацию 3X-UI → AccessFlow.
8. Добавить интеграционные тесты через fake HTTP server.
```

После завершения VPS integration вернуться к архитектуре Notifications и вынести отправку уведомлений в отдельный независимо запускаемый сервис через RabbitMQ.
