# Разработка

## Окружение

- .NET 8 SDK
- PostgreSQL 16
- tree-sitter (см. `install-native.md`)
- Опционально: OpenVSCode Server для работы в браузере

## Первый запуск

```bash
git clone <repo>
cd AlgoVis
cp AlgoVis.Server/appsettings.example.json AlgoVis.Server/appsettings.json
# Отредактировать appsettings.json — пароль БД, JWT secret

dotnet restore
dotnet build AlgoVis.Server/AlgoVis.Server.csproj
```

## Запуск сервера

```bash
ASPNETCORE_HOSTINGSTARTUPASSEMBLIES="" \
ASPNETCORE_URLS=http://0.0.0.0:5000 \
dotnet run --project AlgoVis.Server
```

При первом запуске `EnsureCreated()` создаст все таблицы в БД.

### Админ

После регистрации любого пользователя можно вручную повысить его роль:

```bash
PGPASSWORD=... psql -h postgres -U algovis -d algovis -c \
  "UPDATE users SET \"Role\" = 'admin' WHERE \"Email\" = 'admin@example.com';"
```

Или войти под существующим админом и в админ-панели назначить роль.

## Сборка и тесты

```bash
# Сборка всего
dotnet build

# Тесты API
dotnet test AlgoVis.Server.Tests/AlgoVis.Server.Tests.csproj

# Скрипты тестирования транспайлера
tests/python_smoke/run_all.sh
tests/python_advanced/run_all.sh
tests/python_real/run_all.sh
```

## Структура

```
AlgoVis/
├── AlgoVis.Parser/              Парсер Python (tree-sitter)
├── AlgoVis.Transpiler/          Python → YAWA
├── AlgoVis.Yawa/                Модель + runtime + интерпретатор
├── AlgoVis.Yawa.Cli/            CLI для отладки
├── AlgoVis.Data/                Модели БД, EF Core
├── AlgoVis.Server/              ASP.NET Core API
├── AlgoVis.Server.Tests/        Интеграционные тесты
├── algovis.client/              React-фронт (отдельно)
├── docs/                        Документация
└── tests/
    ├── python_smoke/            20 базовых тестов
    ├── python_advanced/         30 тестов сложного Python
    └── python_real/             10 реальных алгоритмов
```

## CLI — быстрая отладка

```bash
# Транспиляция без сервера
dotnet run --project AlgoVis.Yawa.Cli -- from-python code.py --out=/tmp/c.yawa.json
cat /tmp/c.yawa.json | jq

# Исполнение
dotnet run --project AlgoVis.Yawa.Cli -- run /tmp/c.yawa.json
dotnet run --project AlgoVis.Yawa.Cli -- run /tmp/c.yawa.json --verbose

# Просмотр шагов
dotnet run --project AlgoVis.Yawa.Cli -- inspect /tmp/c.yawa.json --head=20
```

## Добавление поддержки новой конструкции Python

1. **Создайте тест** в `tests/python_advanced/`. Запустите — увидите ошибку.
2. **Транспайлер** — добавьте case в `PythonToYawa.cs` (`ConvertStatement` или `ConvertExpr`).
3. **Модель** — если нужен новый statement/expression, добавьте в `AlgoVis.Yawa/Yawa/Statements` или `Expressions`.
4. **Интерпретатор** — обработайте новый statement/expression в `Interpreter.cs` / `Evaluator.cs`.
5. **Запустите smoke + advanced** — не сломали ли существующее.
6. **Запустите новый тест** — должен пройти.

## Работа с БД

```bash
# Подключение
PGPASSWORD=... psql -h postgres -U algovis -d algovis

# Список таблиц
\dt

# Пользователи
SELECT "Id", "Email", "Role", "IsActive" FROM users;

# Промотреть индексы
\di
```

## Обновление схемы БД

Проект использует `EnsureCreated()` — схема создаётся один раз при старте и
не обновляется. Если меняли модели:

```bash
# Остановить сервер
# Дропнуть схему и создать заново (dev-only!)
PGPASSWORD=... psql -h postgres -U algovis -d algovis -c "DROP SCHEMA public CASCADE; CREATE SCHEMA public;"

# Запустить сервер — таблицы создадутся заново
```

Для production нужны миграции EF Core (`dotnet ef migrations add`).

## Тестирование API вручную

```bash
# Регистрация
curl -s -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","username":"tester","password":"secret123"}'

# Токен
ACCESS=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"secret123"}' \
  | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)

# Создать проект
curl -s -X POST http://localhost:5000/api/projects \
  -H "Authorization: Bearer $ACCESS" \
  -H "Content-Type: application/json" \
  -d '{"name":"Test","pythonCode":"def main(): pass"}'

# Запустить код
curl -s -X POST http://localhost:5000/api/yawa/run-python \
  -H "Content-Type: application/json" \
  -d '{"code":"def main():\n    A = [1,2,3]\n    A[0] = 99"}'
```

## Деплой на VPS

### Требования

- Ubuntu 22.04+
- 2+ vCPU, 4 ГБ RAM (для разработки + десятки пользователей)
- PostgreSQL (в Docker или на хосте)
- Nginx (для HTTPS и проксирования)

### Сборка для production

```bash
dotnet publish AlgoVis.Server/AlgoVis.Server.csproj \
  -c Release -o /var/www/algovis
```

### systemd

```ini
# /etc/systemd/system/algovis.service
[Unit]
Description=AlgoVis Server
After=network.target postgresql.service

[Service]
WorkingDirectory=/var/www/algovis
ExecStart=/usr/bin/dotnet /var/www/algovis/AlgoVis.Server.dll
Restart=always
RestartSec=5
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=ASPNETCORE_URLS=http://127.0.0.1:5000
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl enable --now algovis
sudo systemctl status algovis
```

### Nginx

```nginx
location /api/ {
    proxy_pass http://127.0.0.1:5000;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
    proxy_set_header Host $host;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_read_timeout 300s;
}

location / {
    root /var/www/frontend;   # React-фронт
    try_files $uri $uri/ /index.html;
}
```

## Ресурсы

- **CPU** — интерпретатор CPU-bound. Один запрос ~50 мс - 3 сек.
- **RAM** — trace 100k шагов занимает 20-100 МБ в сессии.
- **Rate limit** — обязателен, чтобы один пользователь не положил сервер.
- **Timeout** — 5-10 сек на запрос.

## Траблшутинг

**`DllNotFoundException: tree-sitter`** — библиотеки не установлены или
не в `LD_LIBRARY_PATH`. Проверьте `ldconfig -p | grep tree-sitter`.

**`Segmentation fault`** при парсинге — ABI mismatch. Проверьте `ts_abi`,
см. `install-native.md`.

**`Npgsql.PostgresException: 42P01`** — таблицы не созданы. Проверьте, что
`Program.cs` вызывает `EnsureCreated()` при старте.

**`429 Too Many Requests`** — rate limit. В тестах отключён автоматически
(env `ASPNETCORE_ENVIRONMENT=Testing`).

**`TypeLoadException` EF Core** — конфликт версий пакетов. Все EF-пакеты
должны быть одной версии (8.0.10).

## Полезные ссылки

- [tree-sitter](https://tree-sitter.github.io/tree-sitter/)
- [EF Core PostgreSQL](https://www.npgsql.org/efcore/)
- [ASP.NET Core 8](https://learn.microsoft.com/aspnet/core/)
