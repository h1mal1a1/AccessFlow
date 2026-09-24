# AccessFlow — памятка по Client и Connection

## Client
CREATE
→ создать Client в БД

READ
→ читать из БД

UPDATE
→ обновлять в БД

DELETE
→ одна DB-транзакция:
   получить Connections клиента
   → для каждого создать Outbox на удаление из VPS
   → удалить Connections из БД
   → удалить Client
   → COMMIT

Worker:
Outbox → DELETE client в VPS
→ success или not found = операция выполнена
→ Outbox Completed

Важно: в Outbox заранее сохранить `Connection.Name`, потому что после удаления Connection из БД получить его уже будет нельзя.

---

## Connection
Статусы:
Pending   — создан в БД, но ещё не создан в VPS
Active    — существует и в БД, и в VPS
Deleting  — удаление запрошено, но VPS ещё может существовать
Deleted   — в VPS уже удалён

### Create
DB transaction:
Connection(Status = Pending)
→ Outbox(Create)
→ COMMIT

Worker:
проверить, существует ли client в VPS
если нет
→ создать
если уже существует
→ получить существующий
→ получить IdExternal / ConnectionString / SubUrl
→ обновить Connection
→ Status = Active
→ Outbox Completed

Пока `Pending`:
IdExternal = null
ConnectionString = null
SubUrl = null

### Read
→ только PostgreSQL
В обычном GET в VPS не ходим.

### Update
Сейчас меняется только `Name`.

DB transaction:
создать Outbox:
CurrentName
NewName
ConnectionId
IdExternal
→ COMMIT

Worker:
обновить client в VPS
CurrentName → NewName

→ после успеха изменить Connection.Name в БД
→ Outbox Completed

До успешного VPS update в БД остаётся старый `Name`.
Если worker уже обновил VPS, но упал до обновления БД:
старого Name в VPS нет
→ проверить NewName
→ если UUID == IdExternal нашего Connection значит update уже произошёл
→ обновить БД

### Delete
DB transaction:
Status = Deleting
→ Outbox(Delete)
→ COMMIT

Worker:
DELETE client в VPS

success или not found
→ Status = Deleted
→ Outbox Completed
---

## Главная идея
Не откатываем изменения во внешнем VPS. Через Outbox + retry доводим операцию до требуемого конечного состояния.