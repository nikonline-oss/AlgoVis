# REST API

Base URL: `/api`. Формат: JSON. Аутентификация: `Authorization: Bearer <accessToken>`.

## Формат ответов

Успех — обычный JSON с полями. Ошибка:

```json
{
  "error": "Текст ошибки",
  "kind": "validation" | "auth" | "runtime" | "transpiler" | "not_found" | "internal",
  "detail": "опционально",
  "line": 5,
  "column": 12
}
```

Коды HTTP: `200`, `400`, `401`, `403`, `404`, `429`, `500`.

## Auth — `/api/auth`

### POST `/register`

```json
{ "email": "user@example.com", "username": "user", "password": "secret123" }
```

Response:
```json
{
  "accessToken": "eyJ...",
  "refreshToken": "PGmCM...",
  "expiresInSeconds": 900,
  "user": {
    "id": 1,
    "email": "user@example.com",
    "username": "user",
    "role": "student",
    "defaultLanguage": "python",
    "createdAt": "2026-10-07T...",
    "lastLoginAt": null
  }
}
```

### POST `/login`

```json
{ "email": "user@example.com", "password": "secret123" }
```

Response — как у register.

### POST `/refresh`

```json
{ "refreshToken": "PGmCM..." }
```

Response — новые accessToken и refreshToken (rotation).

### POST `/logout`

```json
{ "refreshToken": "PGmCM..." }
```

Response: `{ "ok": true }`.

### GET `/me`

Требует авторизации. Response:

```json
{
  "id": 1,
  "email": "user@example.com",
  "username": "user",
  "role": "student",
  "defaultLanguage": "python",
  "createdAt": "...",
  "lastLoginAt": "..."
}
```

### GET `/profile/{username}`

Публичный профиль:

```json
{
  "id": 1,
  "username": "user",
  "role": "student",
  "createdAt": "...",
  "rating": {
    "playerXp": 50,
    "playerLevel": 1,
    "completedCount": 1,
    "teacherRating": 0,
    "teacherLevel": 1,
    "createdCount": 0,
    "totalAssignmentsCompleted": 0
  },
  "publicProjectsCount": 2,
  "publishedAssignmentsCount": 0
}
```

## Projects — `/api/projects`

Требует авторизации.

### GET `/`

Список своих проектов.

### GET `/{id}`

Детали проекта (только владелец).

### POST `/`

```json
{ "name": "My Project", "description": "...", "pythonCode": "...", "yawaJson": "..." }
```

### PATCH `/{id}`

Любое подмножество полей.

### DELETE `/{id}`

Удаление.

### POST `/{id}/publish`

Публикация → возвращает `publicSlug`.

### DELETE `/{id}/publish`

Снять с публикации.

## Public projects — `/api/public/projects`

### GET `/{slug}`

Публичный доступ без авторизации:

```json
{
  "name": "My Project",
  "description": "...",
  "pythonCode": "...",
  "yawaJson": "...",
  "ownerUsername": "user",
  "updatedAt": "..."
}
```

## Assignments — `/api/assignments`

Требует авторизации.

### POST `/preview`

Прогон эталона. Body:

```json
{
  "referenceSolution": "def main(): ...",
  "language": "python",
  "compareTarget": "A",
  "maxSteps": 100000,
  "maxSeconds": 5
}
```

Response:
```json
{
  "success": true,
  "actualResult": [1,2,3,5,8,9],
  "stats": {
    "totalSteps": 52,
    "comparisons": 15,
    "swaps": 7,
    "memoryAccesses": 60,
    "userCounters": {}
  },
  "finalState": { "A": [1,2,3,5,8,9] }
}
```

### GET `/` — список публичных заданий (для ученика)

### GET `/mine` — список своих заданий (для автора)

### GET `/{id}` — детали для ученика (без эталона)

### GET `/{id}/mine` — детали для автора (с эталоном)

### POST `/`

```json
{
  "title": "Сортировка пузырьком",
  "description": "...",
  "mode": "visual",
  "allowedLanguages": [],
  "referenceSolution": "def bubble_sort(A): ...",
  "referenceLanguage": "python",
  "compareTarget": "A",
  "gradingRules": "{\"good\":{\"maxComparisons\":100},\"excellent\":{\"maxComparisons\":30},\"perfect\":{\"maxComparisons\":16,\"maxSwaps\":12}}",
  "templateCode": "...",
  "maxSteps": 100000,
  "maxSeconds": 5,
  "isPublished": false,
  "isPublic": true,
  "deadline": null
}
```

### PATCH `/{id}` — частичное обновление

### DELETE `/{id}` — удаление

### POST `/{id}/publish` — публикация

### DELETE `/{id}/publish` — снять с публикации

### GET `/{id}/leaderboard` — рейтинг по заданию

## Submissions — `/api`

### POST `/assignments/{id}/submit`

```json
{ "language": "python", "code": "def bubble_sort(A): ..." }
```

Response:
```json
{
  "id": 1,
  "assignmentId": 1,
  "assignmentTitle": "...",
  "userId": 2,
  "username": "student",
  "language": "python",
  "code": "...",
  "status": "passed",
  "grade": "perfect",
  "xpAwarded": 50,
  "result": {
    "passed": true,
    "actualResult": [1,2,3,5,8,9],
    "expectedResult": [1,2,3,5,8,9],
    "stats": { "totalSteps": 52, "comparisons": 15, "swaps": 7, "memoryAccesses": 60 },
    "criteriaChecked": { "result": true, "good": true, "excellent": true, "perfect": true },
    "reason": null
  },
  "errorMessage": null,
  "createdAt": "...",
  "commentsCount": 0
}
```

### GET `/submissions/mine` — мои отправки

### GET `/assignments/{id}/submissions` — отправки на моё задание (автор)

### GET `/submissions/{id}` — детали отправки

## Comments — `/api`

### POST `/submissions/{id}/comments`

```json
{ "text": "Отличное решение!" }
```

### GET `/submissions/{id}/comments` — список

### DELETE `/comments/{id}` — удалить свой

## Leaderboard — `/api`

### GET `/leaderboard/players?limit=50`

### GET `/leaderboard/teachers?limit=50`

Публичные. Response — массив:

```json
[
  { "rank": 1, "userId": 2, "username": "student", "score": 50, "level": 1, "completedOrCreatedCount": 1 }
]
```

### GET `/assignments/{id}/leaderboard` — требует авторизации

## Admin — `/api/admin`

Требует роль `admin`.

### GET `/stats`

Сводка по платформе.

### GET `/users?page=1&pageSize=20&search=&role=`

### PATCH `/users/{id}/role`

```json
{ "role": "teacher" }
```

Допустимые роли: `student`, `teacher`, `admin`. Нельзя понизить последнего админа.

### PATCH `/users/{id}/active`

```json
{ "isActive": false }
```

При блокировке отзываются все refresh-токены.

### DELETE `/users/{id}`

Удаление. Нельзя удалить себя. Нельзя удалить последнего админа.
Нельзя удалить пользователя с созданными заданиями.

### GET `/projects?page=1&pageSize=20&search=`

### DELETE `/projects/{id}`

### GET `/assignments?page=1&pageSize=20&search=`

### DELETE `/assignments/{id}`

### GET `/submissions?page=1&pageSize=20&status=`

## YAWA runtime — `/api/yawa`

### GET `/health` — health check

### GET `/samples` — список примеров

### GET `/samples/{name}` — содержимое примера

### POST `/run` — YAWA-JSON → Trace

### POST `/run-sample?name=X` — sample → Trace

### POST `/run-python` — Python-код → Trace

**Публичный** (для анонимных тоже). Rate limit: 10/мин анонимным, 30/мин авторизованным.

```json
{ "code": "def main(): ..." }
```

Response: Trace JSON (см. `docs/yawa-schema.md`).

### GET `/viewer` — HTML-страница визуализатора (dev)

### GET `/assignments-viewer` — HTML-страница заданий (dev)

### GET `/admin-viewer` — HTML-страница админки (dev)

## Rate limits

| Endpoint | Лимит |
|---|---|
| `/api/yawa/run-python` (авториз.) | 30/мин |
| `/api/yawa/run-python` (аноним) | 10/мин |
| `/api/auth/register` | 5/час с IP |
| `/api/auth/login` | 10/мин с IP |
| `/api/assignments/preview` | 30/мин |
| `/api/assignments/{id}/submit` | 30/мин |

При превышении — **429** без тела. Нужен backoff на клиенте.
