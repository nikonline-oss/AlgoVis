# Архитектура AlgoVis

## Поток данных

```
┌────────────────┐    ┌──────────────────┐    ┌────────────────┐
│ Python-код     │───►│ AlgoVis.Parser   │───►│ CST tree-sitter│
│ (text)         │    │ (P/Invoke)       │    │ (нативное)     │
└────────────────┘    └──────────────────┘    └────────┬───────┘
                                                       │
                                                       ▼
┌────────────────┐    ┌──────────────────┐    ┌────────────────┐
│ YAWA-программа │◄───│ AlgoVis.Transpiler│◄──│ Обход CST      │
│ (YawaProgram)  │    │                  │    │                │
└────────┬───────┘    └──────────────────┘    └────────────────┘
         │
         ▼
┌────────────────┐    ┌──────────────────┐    ┌────────────────┐
│ AlgoVis.Yawa   │───►│ Trace            │───►│ Viewer / CLI   │
│ Interpreter    │    │ (steps, diff,    │    │                │
│                │    │  snapshot)       │    │                │
└────────────────┘    └──────────────────┘    └────────────────┘
```

## Модули

### AlgoVis.Parser

Низкоуровневая обёртка над tree-sitter. Только парсинг Python → CST.
Не знает ничего про YAWA.

**Файлы:**
- `Native/TsNative.cs` — P/Invoke сигнатуры
- `Native/LibLoader.cs` — резолвер путей к `.so`
- `Parsing/TsParser.cs`, `TsTree.cs`, `TsNode.cs` — managed-обёртки
- `Parsing/PythonParser.cs` — фасад

**Технические детали:**
- Использует tree-sitter v0.22.6 (ABI 14)
- Грамматика Python: tree-sitter-python v0.21.0 (ABI 14)
- Несовместимость версий ABI = segfault (см. `docs/install-native.md`)

### AlgoVis.Transpiler

Превращает CST Python в YAWA AST. Основной конвертер — `PythonToYawa.cs`
(~1500 строк).

**Принципы:**
- Не падать там, где можно обойтись — эмитить warning и продолжать
- Падать только на действительно неподдерживаемых конструкциях
- Все ошибки содержат `line:column` для UX
- Имена функций и классов нормализуются: `ClassName.method` — разделитель точка

**Ключевые механизмы:**
- `_renameStack` — стек карт переименования для nested functions и замыканий
- `_classMethodMap` — карта методов класса для эмуляции наследования
- `_pendingNestedFunctions` — функции, найденные внутри тела (включая lambda)

### AlgoVis.Yawa

Всё про YAWA: модель, runtime, интерпретатор, trace.

**Структура:**
- `Yawa/` — DTO YAWA-программы (`YawaProgram`, `YawaFunction`, `YawaStatement`, `YawaExpression`)
- `Trace/` — DTO выходной трассы (`TraceSession`, `TraceStep`, `StateChange`)
- `Runtime/RuntimeValue.cs` и наследники — значения во время исполнения
  (`IntValue`, `FloatValue`, `StringValue`, `BoolValue`, `NullValue`,
  `ArrayValue`, `ObjectValue`, `SetValue`, `TupleValue`, `TreeValue`,
  `GraphValue`, `FunctionRefValue`)
- `Runtime/Interpreter.cs` — главный цикл исполнения
- `Runtime/Evaluator.cs` — вычисление выражений
- `Runtime/Builtins.cs` — встроенные функции
- `Runtime/TraceRecorder.cs` — запись шагов
- `Runtime/Frame.cs` — области видимости

**Ключевые решения:**

1. **Управление потоком через исключения** — `ReturnException`, `BreakException`,
   `ContinueException`, `YawaRuntimeException`. Позволяет не таскать флаги.

2. **Семантика присваивания Python** — `Frame.Set` всегда пишет в `Locals`
   текущего фрейма. Shadowing работает как в Python: локальная переменная
   перекрывает родительскую.

3. **Замыкания** — `FunctionRefValue` захватывает `Frame` в момент создания.
   При вызове функции с `CapturedFrame` родителем нового фрейма становится
   захваченный фрейм, а не вызывающий.

4. **Гибридный trace** — diff на каждом шаге + snapshot при входе/выходе
   из функции. Позволяет эффективно восстанавливать состояние на любом шаге.

5. **Рекурсивный сбор comparison** — `RecordComparisonIfApplicable` обходит
   `and`/`or`/`not` в условиях `if` и `while` с short-circuit. Автоматически
   считает comparisons для Python-кода без явного `compare()`.

### AlgoVis.Data

Модели БД, EF Core, PostgreSQL.

**Сущности:**
- `User` — email, username, password_hash, role, default_language
- `RefreshToken` — токен обновления сессии с rotation
- `Project` — сохранённый код пользователя (python_code + yawa_json)
- `Assignment` — задание (режим visual/code, эталон, критерии, дедлайн)
- `Submission` — попытка решения (код, статус, грейд, XP, result_json)
- `Comment` — комментарий к отправке
- `UserRating` — PlayerXp/Level, TeacherRating/Level

### AlgoVis.Server

ASP.NET Core: REST API + встроенные HTML-страницы для разработчика.

**Файлы:**
- `Controllers/AuthController.cs` — регистрация, логин, refresh, logout, me
- `Controllers/ProjectsController.cs` — CRUD проектов
- `Controllers/PublicProjectsController.cs` — публичный доступ по slug
- `Controllers/AssignmentsController.cs` — CRUD заданий, preview
- `Controllers/SubmissionsController.cs` — отправка решений
- `Controllers/CommentsController.cs` — комментарии
- `Controllers/LeaderboardController.cs` — рейтинги
- `Controllers/AdminController.cs` — админ-функции
- `Controllers/Yawa/YawaController.cs` — runtime API + отдача HTML-страниц
- `Controllers/Yawa/YawaViewerHtml.cs` — визуализатор (dev)
- `Controllers/Yawa/AssignmentsViewerHtml.cs` — среда заданий (dev)
- `Controllers/Yawa/AdminViewerHtml.cs` — админ-панель (dev)

**HTML-страницы** — это не продакшн-фронт, а инструмент бэкендера для
проверки функционала. Продакшн-UI пишется отдельно на React (см. `algovis.client`).

### AlgoVis.Server.Tests

Интеграционные тесты API через `WebApplicationFactory<Program>`.

**Структура:**
- `Infrastructure/TestAppFactory.cs` — уникальная БД на класс тестов
- `Infrastructure/ApiClient.cs` — обёртка над HttpClient с токенами
- `Infrastructure/TestBase.cs` — TRUNCATE таблиц перед каждым тестом
- `AuthTests.cs` (15 тестов)
- `ProjectsTests.cs` (15)
- `AssignmentsTests.cs` (15)
- `SubmissionsTests.cs` (10)
- `CommentsTests.cs` (8)
- `LeaderboardTests.cs` (6)
- `AdminTests.cs` (14)
- `YawaTests.cs` (7)

**Технические детали:**
- Отключён параллелизм — тесты используют глобальные env-vars
- Каждый класс тестов получает свежую БД
- Перед каждым тестом таблицы очищаются

## Безопасность

**Аутентификация:**
- Access token — JWT, 15 минут, HMAC-SHA256
- Refresh token — случайная строка 512 бит, 30 дней, хранится в БД
- Rotation: при обновлении старый refresh отзывается
- Детект переиспользования: если отозванный refresh приходит → отзыв всех сессий

**Авторизация:**
- Роли: `student`, `teacher`, `admin`
- Проверка владельца: чужой ресурс → 404 (не 403, чтобы не палить существование)

**Rate limiting:**
- Запуск кода: 30/мин авторизованным, 10/мин анонимным
- Регистрация: 5/час с IP
- Логин: 10/мин с IP
- В тестовом окружении отключён

## Дальнейшее развитие

- **SignalR** — real-time обновления комментариев, лидербордов, статистики
- **Мультиязычность** — транспорт YAWA не привязан к Python. Можно добавить C#, JS
- **On-premise версия** — лицензионный сервер + локальные инсталляции в школах