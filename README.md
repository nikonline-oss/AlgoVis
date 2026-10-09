# AlgoVis

Платформа для пошаговой визуализации алгоритмов. Пользователь пишет код на Python,
система транспилирует его в промежуточный формат YAWA, исполняет с трассировкой
и показывает каждый шаг в браузере.

```
Python-код  ──►  YAWA (JSON)  ──►  Trace (JSON)  ──►  Viewer
              транспайлер      интерпретатор        браузер / UI
```

## Возможности

**Для ученика:**
- Писать код в редакторе, видеть визуализацию массива, матрицы, дерева, set, dict
- Сохранять проекты в личном кабинете
- Публиковать проекты по короткой ссылке
- Решать задания преподавателей, получать XP и грейды
- Обсуждать решения в комментариях

**Для преподавателя:**
- Создавать задания с эталонным решением и критериями оценки
- Видеть preview эталона: результат, статистику, шаги
- Просматривать отправки учеников, ставить оценки, оставлять комментарии
- Видеть рейтинги по каждому заданию

**Для администратора:**
- Управлять пользователями: назначать роли, блокировать, удалять
- Просматривать статистику платформы
- Модерировать проекты и задания

## Быстрый старт

### Требования

- .NET 8 SDK
- PostgreSQL 16+
- Нативные библиотеки tree-sitter (см. [`docs/install-native.md`](docs/install-native.md))

### Установка

```bash
git clone <repo> AlgoVis
cd AlgoVis

# 1. Установить tree-sitter (один раз)
# См. docs/install-native.md

# 2. Настроить appsettings.json
cp AlgoVis.Server/appsettings.example.json AlgoVis.Server/appsettings.json
# Заменить секреты (см. ниже)

# 3. Восстановить пакеты
dotnet restore

# 4. Собрать
dotnet build AlgoVis.Server/AlgoVis.Server.csproj
```

### Секреты в appsettings.json

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=algovis;Username=algovis;Password=YOUR_PASSWORD"
  },
  "Jwt": {
    "Secret": "GENERATE_WITH: openssl rand -base64 48"
  }
}
```

### Запуск

```bash
ASPNETCORE_HOSTINGSTARTUPASSEMBLIES="" \
ASPNETCORE_URLS=http://0.0.0.0:5000 \
dotnet run --project AlgoVis.Server
```

Открыть в браузере:
- **Визуализатор**: `http://localhost:5000/api/yawa/viewer`
- **Среда заданий**: `http://localhost:5000/api/yawa/assignments-viewer`
- **Админ-панель**: `http://localhost:5000/api/yawa/admin-viewer` (нужна роль admin)

### CLI — запуск без сервера

```bash
# Транспиляция Python в YAWA
dotnet run --project AlgoVis.Yawa.Cli -- from-python code.py --out=out.yawa.json

# Исполнение YAWA с трассировкой
dotnet run --project AlgoVis.Yawa.Cli -- run out.yawa.json

# Просмотр сохранённой трассы
dotnet run --project AlgoVis.Yawa.Cli -- inspect trace.json --head=30
```

## Пример

```python
def bubble_sort(A):
    n = len(A)
    for i in range(n):
        for j in range(n - i - 1):
            if A[j] > A[j + 1]:
                A[j], A[j + 1] = A[j + 1], A[j]
                annotate("поменяли местами")
    return A

def main():
    A = [5, 2, 8, 1, 9, 3, 7, 4]
    bubble_sort(A)
```

Результат: 86 шагов, 28 сравнений, 13 обменов. Массив визуализируется с подсветкой
сравниваемых и обмениваемых элементов. На каждом шаге видно состояние переменных.

## Структура решения

| Проект | Назначение |
|---|---|
| `AlgoVis.Parser` | Парсер Python через tree-sitter (P/Invoke). Возвращает CST. |
| `AlgoVis.Transpiler` | Python → YAWA. Поддерживает широкое подмножество языка. |
| `AlgoVis.Yawa` | Модель YAWA, runtime, интерпретатор, trace-рекордер. |
| `AlgoVis.Yawa.Cli` | CLI: `load`, `run`, `inspect`, `from-python`. |
| `AlgoVis.Data` | Модели БД, EF Core, PostgreSQL. |
| `AlgoVis.Server` | ASP.NET Core: REST API + HTML viewer'ы. |
| `AlgoVis.Server.Tests` | 88 интеграционных тестов API. |
| `algovis.client` | React-фронт (разрабатывается отдельно). |

## Документация

- [`docs/architecture.md`](docs/architecture.md) — модули, поток данных, ключевые решения
- [`docs/python-support.md`](docs/python-support.md) — что поддерживает транспайлер Python
- [`docs/yawa-schema.md`](docs/yawa-schema.md) — формат YAWA и Trace JSON
- [`docs/api.md`](docs/api.md) — REST API контракты
- [`docs/install-native.md`](docs/install-native.md) — сборка tree-sitter
- [`docs/development.md`](docs/development.md) — тесты, отладка, деплой

## Тесты

Три набора в `tests/`:

```bash
tests/python_smoke/run_all.sh      # 20 базовых тестов транспайлера
tests/python_advanced/run_all.sh   # 30 тестов сложного Python
tests/python_real/run_all.sh       # 10 реальных алгоритмов
```

Все три набора должны показывать **100% прохождения**.

Интеграционные тесты API:

```bash
dotnet test AlgoVis.Server.Tests/AlgoVis.Server.Tests.csproj
```

Ожидаемо: **88/88** зелёных.

## Поддерживаемый Python

Транспайлер поддерживает широкое подмножество Python 3, включая:

- Все базовые конструкции (if/elif/else, for, while, break, continue, return)
- Comprehensions: list, set, dict, вложенные
- Классы с наследованием, `__init__`, методами, `@staticmethod`
- Замыкания и nested functions
- Lambda, `*args`, `**kwargs`
- Try/except/finally, assert
- Строковые методы: `strip`, `lower`, `split`, `join`, `replace`, и др.
- Встроенные: `len`, `range`, `min`, `max`, `sorted`, `enumerate`, `zip`, `sum`, `any`, `all`, `abs`, `chr`, `ord`, `int`, `str`, `list`, `set`, `tuple`
- Отрицательные индексы, срезы (включая `A[::2]`, `A[::-1]`)
- Множественное присваивание (`a = b = c`), распаковка кортежей, `*rest`
- F-strings с выражениями
- Импорты (`import math`)

Полный список см. в [`docs/python-support.md`](docs/python-support.md).

## Лицензия

Внутренний проект.
