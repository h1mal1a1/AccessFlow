# AccessFlow — интеграция с 3X-UI/VPS

Дата актуализации: 2026-09-12

## 1. Общая идея

AccessFlow хранит наших пользователей и связь между пользователями и VPN-подключениями.

3X-UI не знает про пользователей AccessFlow. В 3X-UI есть один постоянный inbound `main`, внутри которого создаются отдельные clients.

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

При интеграции AccessFlow должен находить inbound `main` через API 3X-UI и использовать его numeric ID при создании клиентов.

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
      (например pc, phone, laptop и т.п.)

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

`Name` можно генерировать по-разному.

Его основное назначение — чтобы даже без AccessFlow по панели 3X-UI было понятно, кому или какому устройству принадлежит соединение.

Информацию о владельце при необходимости можно также хранить в `Comment`, но обязательного правила пока нет.

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

## 7. Создание Connection

На уровне `IVpsClient` уже реализован сценарий создания клиента в 3X-UI:

```text
1. Найти inbound по `remark = main`.
2. Создать client в найденном inbound.
3. Получить созданного client через API 3X-UI.
4. Получить connection link.
5. Построить subscription URL.
6. Вернуть `VpsConnectionInfo`.
```

Следующий этап — встроить этот вызов в создание `Connection` в Application и сохранить в PostgreSQL:

```text
IdExternal
Name
ConnectionString
SubUrl
Status
```

Если 3X-UI успешно создал client, но сохранение в PostgreSQL упало:

```text
→ выполнить компенсирующее действие
→ попытаться удалить только что созданного client из 3X-UI
```

То есть не оставлять "висячий" client во внешней системе.

---

## 8. Удаление Connection

При удалении нужно будет синхронизировать обе стороны.

Базовая идея:

```text
удалить/отключить client в 3X-UI
→ после успешного результата
→ пометить Connection как Deleted в AccessFlow
```

Точный порядок и поведение при частичных ошибках нужно будет окончательно определить при реализации.

---

## 9. Синхронизация AccessFlow ↔ 3X-UI

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

## 10. Правило для unknown-клиентов

Если клиент был автоматически импортирован из 3X-UI и имеет:

```text
Email = "unknown"
```

то уведомления ему не отправляются.

Это позволяет импортировать неизвестные подключения из 3X-UI, но не пытаться делать рассылку без реального email.

---

## 11. API 3X-UI

Авторизация:

```text
Authorization: Bearer <API_TOKEN>
```

API token создаётся в панели 3X-UI.

Токен хранить только в секретах/env и никогда не коммитить в Git.

Проверено на реальном VPS, что работают:

```text
GET  /panel/api/inbounds/list
GET  /panel/api/clients/get/{email}
GET  /panel/api/clients/links/{email}
POST /panel/api/clients/add
```

Нужны далее:

```text
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

Числовой ID inbound не должен храниться в `Connection`, так как логически используется постоянный inbound `main`.

Реальный ответ `GET /panel/api/clients/get/{email}` содержит client внутри `obj.client`, поэтому Infrastructure использует отдельные DTO для внешнего контракта 3X-UI. `obj` в generic response является nullable, так как при `success = false` 3X-UI может вернуть `obj = null`.

---

## 12. Конфигурация AccessFlow

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

Числовой inbound ID лучше получать по `remark = main`, а не жёстко хранить в БД.

---

## 13. Текущее состояние реализации

На текущем этапе реализовано:

```text
Application
→ IVpsClient.GetConnectionAsync(...)
→ IVpsClient.CreateConnectionAsync(...)
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
→ обработка network/timeout/5xx
→ обработка 4xx
→ обработка некорректного JSON
→ обработка business errors 3X-UI
```

`GetConnectionAsync` и `CreateConnectionAsync` проверены вручную на реальном VPS. Повторное создание client с тем же `email` обрабатывается как conflict.

HTTP-ошибки VPS маппятся через `VpsErrorType`:

```text
Conflict        → 409
NotFound        → 404
InvalidResponse → 502
OperationFailed → 502
Unavailable     → 503
Configuration   → 502
```

Реальный VPS используется только для smoke/manual checks. Автоматические тесты не должны зависеть от его доступности.

---

## 14. Следующий этап разработки

```text
1. Встроить IVpsClient.CreateConnectionAsync(...) в Application-сценарий создания Connection.
2. Сохранять IdExternal / Name / ConnectionString / SubUrl в PostgreSQL.
3. Реализовать удаление client в 3X-UI.
4. Добавить компенсирующее удаление, если сохранение Connection в БД завершилось ошибкой.
5. Реализовать update/delete для Connection.
6. Реализовать синхронизацию 3X-UI → AccessFlow.
7. Добавить интеграционные тесты через fake HTTP server.
```

После завершения VPS integration вернуться к архитектуре Notifications и вынести отправку уведомлений в отдельный независимо запускаемый сервис через RabbitMQ.
