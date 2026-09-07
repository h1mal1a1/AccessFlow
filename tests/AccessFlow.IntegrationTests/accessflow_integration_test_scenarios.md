# AccessFlow — сценарии интеграционных тестов

## 1. Client — Create
- Корректный запрос → `201 Created`.
- В ответе возвращается `id` созданного клиента.
- Заголовок `Location` указывает на `/api/clients/{id}`.
- После создания `GET /api/clients/{id}` → `200 OK`.
- Сохранённые `Email`, `PhoneNumber`, `Comment` совпадают с отправленными.
- Статус нового клиента — `Active`.

## 2. Client — Get
- Существующий клиент → `200 OK`.
- Несуществующий `id` → `404 Not Found`.
- Удалённый клиент через обычный `GET /api/clients/{id}` → `404 Not Found`.

## 3. Client — Update
- Обновление существующего клиента → `204 No Content`.
- После обновления `GET` возвращает новые значения.
- Обновление несуществующего клиента → `404 Not Found`.
- Обновление удалённого клиента → `404 Not Found`.

## 4. Client — Delete
- Удаление существующего клиента → `204 No Content`.
- После удаления обычный `GET /api/clients/{id}` → `404 Not Found`.
- Удалённый клиент появляется в `/api/clients/deleted`.
- Статус удалённого клиента — `Deleted`.
- Удаление несуществующего клиента → `404 Not Found`.

## 5. Client — списки и пагинация
- Обычный список содержит активных клиентов.
- Удалённые клиенты не попадают в обычный список.
- `/api/clients/deleted` содержит удалённых клиентов.
- `page/pageSize` ограничивают количество записей.
- Разные страницы возвращают разные записи.
- Порядок записей соответствует сортировке по `Id`.

## 6. Connection — Create
- Создание для активного клиента → `201 Created`.
- В ответе возвращается `id` созданной connection.
- Заголовок `Location` указывает на `/api/connections/{id}`.
- После создания `GET /api/connections/{id}` → `200 OK`.
- Все поля сохранены корректно.
- Статус новой connection — `Active`.
- Создание connection для несуществующего клиента → `404 Not Found`.
- Создание connection для удалённого клиента → `404 Not Found`.

## 7. Connection — Get
- Существующая connection → `200 OK`.
- Несуществующий `id` → `404 Not Found`.
- Удалённая connection через обычный `GET` → `404 Not Found`.

## 8. Connection — Update
- Обновление существующей connection → `204 No Content`.
- После обновления `GET` возвращает новые значения.
- Обновление несуществующей connection → `404 Not Found`.
- Обновление удалённой connection → `404 Not Found`.

## 9. Connection — Delete
- Удаление существующей connection → `204 No Content`.
- После удаления обычный `GET` → `404 Not Found`.
- Удалённая connection появляется в `/api/connections/deleted`.
- Статус удалённой connection — `Deleted`.
- Удаление несуществующей connection → `404 Not Found`.

## 10. Connection — уникальные ограничения / 409
Проверить отдельно каждый partial unique index:
- Дублирующий `IdExternal` при создании → `409 Conflict`.
- Дублирующий `Name` при создании → `409 Conflict`.
- Дублирующий `SubUrl` при создании → `409 Conflict`.
- При `UPDATE` попытка занять `IdExternal` другой активной connection → `409 Conflict`.
- При `UPDATE` попытка занять `Name` другой активной connection → `409 Conflict`.
- При `UPDATE` попытка занять `SubUrl` другой активной connection → `409 Conflict`.

## 11. Partial unique indexes после soft delete
- Создать connection, удалить её, затем создать новую с тем же `IdExternal` → разрешено.
- Создать connection, удалить её, затем создать новую с тем же `Name` → разрешено.
- Создать connection, удалить её, затем создать новую с тем же `SubUrl` → разрешено.

## 12. Connection — списки и пагинация
- Обычный список содержит активные connections.
- Удалённые connections не попадают в обычный список.
- `/api/connections/deleted` содержит удалённые connections.
- `page/pageSize` ограничивают количество записей.
- Разные страницы возвращают разные записи.
- Порядок записей соответствует сортировке по `Id`.

## 13. Connection — чувствительные данные
Для `GET /api/connections`:
- Есть `Id`.
- Есть `IdClient`.
- Есть `IdExternal`.
- Есть `Name`.
- Есть `Status`.
- Нет `ConnectionString`.
- Нет `SubUrl`.

Для `GET /api/connections/deleted`:
- Нет `ConnectionString`.
- Нет `SubUrl`.

Для `GET /api/connections/{id}`:
- `ConnectionString` присутствует.
- `SubUrl` присутствует.

## 14. Удаление Client → soft delete Connections
- Создать клиента.
- Создать ему несколько активных connections.
- Удалить клиента.
- Клиент получает статус `Deleted`.
- Все его активные connections получают статус `Deleted`.
- Клиент не виден через обычные endpoints.
- Connections не видны через обычные endpoints.
- Клиент виден через `/api/clients/deleted`.
- Connections видны через `/api/connections/deleted`.

## 15. У клиента уже есть удалённая Connection
- Создать клиента.
- Создать две connections.
- Одну connection удалить заранее.
- Затем удалить клиента.
- Операция удаления клиента проходит успешно.
- Обе connections в итоге имеют статус `Deleted`.

## 16. Global Query Filters
- Client со статусом `Deleted` не виден через обычные запросы.
- Connection со статусом `Deleted` не видна через обычные запросы.
- Connection клиента со статусом `Deleted` не видна через обычные запросы.

## 17. ProblemDetails
- Для `404` проверить не только status code, но и тело `ProblemDetails`.
- Для `409` проверить не только status code, но и тело `ProblemDetails`.
- Для `409` проверить ожидаемый title: `Connection conflict`.
- Для `404` проверить ожидаемый title: `Resource not found`.

## 18. Concurrency: CreateConnection ↔ DeleteClient
Одновременно выполнить:
- создание connection для клиента;
- удаление этого же клиента.

Допустимые результаты:
- Create успел первым → Delete затем удаляет клиента и connection.
- Delete успел первым → Create получает `404`.

Главный invariant после завершения обеих операций:

`Deleted Client` не может иметь `Active Connection`.

Недопустимое состояние:
- `Client = Deleted`
- `Connection = Active`

## 19. Изоляция интеграционных тестов
- Тест не должен зависеть от порядка запуска других тестов.
- Тест не должен зависеть от данных, созданных другим тестом.
- Значения `IdExternal`, `Name`, `SubUrl`, используемые для проверки конфликтов, должны создаваться осознанно.
- Одинаковые тесты должны стабильно проходить при повторном запуске.
- Concurrency-тест не должен проходить случайно из-за старых данных.

## Рекомендуемый порядок реализации
1. Client CRUD
2. Client 404
3. Client pagination
4. Connection CRUD
5. Connection 404
6. Connection 409
7. Connection pagination
8. Проверка безопасных list DTO
9. Cascade soft-delete Client → Connections
10. Partial unique indexes после soft delete
11. ProblemDetails
12. Concurrency `CreateConnection ↔ DeleteClient`
13. Проверка изоляции тестов
