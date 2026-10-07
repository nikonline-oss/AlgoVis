Отлично, большой блок. Разобью на **документацию (сейчас)** и **обновление viewer (следующим сообщением)**. Документацию пишу так, чтобы её можно было положить в git и показывать новым разработчикам/пользователям.

## 📖 Шаг 1. README.md в корне проекта

```bash
cat > ~/projects/AlgoVis/README.md <<'MD'
# AlgoVis

Пошаговая визуализация алгоритмов: пользователь пишет Python-код, система
транспилирует его в промежуточный формат YAWA, исполняет с трассировкой и
показывает каждый шаг в браузере.

```
Python-код  ──►  YAWA (JSON)  ──►  Trace (JSON)  ──►  Viewer
              транспайлер      интерпретатор        браузер
```

## Быстрый старт

### Требования

- .NET 8 SDK
- Нативные библиотеки `libtree-sitter.so` и `libtree-sitter-python.so` в `/usr/local/lib`
  (собираются один раз, см. `docs/install-native.md`)

### Сборка

```bash
dotnet build AlgoVis.Server/AlgoVis.Server.csproj
```

### Запуск сервера

```bash
ASPNETCORE_HOSTINGSTARTUPASSEMBLIES="" \
ASPNETCORE_URLS=http://0.0.0.0:5000 \
dotnet run --project AlgoVis.Server
```

Открыть в браузере:

```
http://localhost:5000/api/yawa/viewer
```

### CLI — запуск без сервера

```bash
# Транспиляция Python в YAWA
dotnet run --project AlgoVis.Yawa.Cli -- from-python path/to/code.py --out=out.yawa.json

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

Вставьте во вкладку **Python** в viewer → **Запустить** → справа появится
визуализация с подсветкой сравниваемых элементов, счётчиками и пошаговой
историей.

## Структура решения

| Проект | Назначение |
|---|---|
| `AlgoVis.Parser` | Парсер Python через tree-sitter (P/Invoke). Возвращает CST. |
| `AlgoVis.Transpiler` | Python → YAWA. Поддерживает подмножество языка. |
| `AlgoVis.Yawa` | Модель YAWA, runtime, интерпретатор, trace-рекордер. |
| `AlgoVis.Yawa.Cli` | CLI для отладки: `load`, `run`, `inspect`, `from-python`. |
| `AlgoVis.Server` | ASP.NET Core: REST API + HTML-viewer. |
| `algovis.client` | React-фронт (в разработке, отдельная команда). |
| `tests/python_smoke` | 20 smoke-тестов транспайлера и интерпретатора. |

## Документация

- [`docs/yawa-schema.md`](docs/yawa-schema.md) — формат YAWA и Trace JSON
- [`docs/python-support.md`](docs/python-support.md) — что поддерживает транспайлер
- [`docs/install-native.md`](docs/install-native.md) — сборка tree-sitter
- [`docs/architecture.md`](docs/architecture.md) — обзор модулей и потока данных

## Smoke-тесты

```bash
tests/python_smoke/generate.sh          # создать 20 .py файлов
tests/python_smoke/run_all.sh           # прогнать через транспайлер и интерпретатор
```

Ожидаемый результат — **20/20** (все тесты проходят транспиляцию и исполнение).

## Лицензия

Внутренний проект. Все права принадлежат автору.
MD
```

## 📖 Шаг 2. `docs/python-support.md`

```bash
cat > ~/projects/AlgoVis/docs/python-support.md <<'MD'
# Поддерживаемое подмножество Python

Транспайлер принимает **подмножество** Python, достаточное для
студенческих алгоритмов. Ниже — что работает, что нет, и как обходить
ограничения.

## ✅ Что поддерживается

### Функции

```python
def add(a, b):
    return a + b

def greet(name="мир"):     # default value
    return "привет, " + name

def log(x: int) -> int:    # аннотации типов (игнорируются)
    return x
```

- Рекурсия
- Множественные return
- Nested functions (автоматически выносятся на top-level)
- Декораторы (игнорируются, но не ломают)

### Ветвления

```python
if x > 0:
    ...
elif x < 0:
    ...
else:
    ...
```

- Цепочки сравнений: `if 0 <= x <= 100:` (превращаются в `and`)
- `is None` / `is not None`
- `in` / `not in`
- Логические `and` / `or` / `not` (с short-circuit)

### Циклы

```python
for i in range(n):           # 1 аргумент
for i in range(lo, hi):      # 2 аргумента
for i in range(lo, hi, step) # 3 аргумента
for x in A:                  # обход коллекции
while cond:                  # while
```

Поддерживаются `break`, `continue`.

### Типы данных

| Тип | Литерал | Операции |
|---|---|---|
| int | `42` | `+ - * / // % **`, сравнения |
| float | `3.14` | то же |
| string | `"hi"` | `+`, `in`, индексация, срезы |
| bool | `True` / `False` | логические |
| None | `None` | `is None` |
| list | `[1, 2, 3]` | индексация, срезы, `append`, `pop`, `insert`, `remove`, `len`, `in`, `*` (повтор) |
| tuple | `(1, 2)` | индексация, распаковка |
| set | `{1, 2}` | `add`, `discard`, `in`, `len` |
| dict | `{"a": 1}` | `d[k]`, `d[k] = v`, `in`, `len` |
| object | `ClassName()` | атрибуты, методы |

**Отрицательные индексы**: `A[-1]`, `A[-2]` работают.

### Классы

```python
class Point:
    def __init__(self, x, y):
        self.x = x
        self.y = y

    def length_sq(self):
        return self.x * self.x + self.y * self.y

p = Point(3, 4)
d = p.length_sq()
```

- Поддерживаются классы без наследования
- `__init__` вызывается автоматически
- Методы через `self.method()`
- Поля через `self.field = value`

### List comprehension

```python
[x * 2 for x in A]
[x for x in A if x > 0]
[[0] * n for _ in range(n)]   # матрица
```

Один `for`, опционально один `if`.

### Работа со списком

```python
A.append(x)     # → push
A.pop()         # → pop
A.insert(i, x)  # → insert
A.remove(i)     # → remove (по индексу, как в Python для списков)
A * 3           # повтор списка: [0] * 5
```

### Специальные функции визуализации

```python
annotate("шаг алгоритма")          # подпись к текущему шагу
swap(A, i, j)                      # обмен (эквивалент A[i], A[j] = A[j], A[i])
compare(A[i], A[j])                # явное сравнение с подсветкой
mark(A, i, "red")                  # подсветка элемента
```

### Встроенные функции

`len` (или `length`), `min`, `max`, `abs`, `print`, `range`, `set()`

## ❌ Что НЕ поддерживается

### Множественные операторы в одном выражении — нужно разбивать

```python
# НЕ работает:
a, b, c = (1, 2, 3)         # распаковка tuple-литерала (левая часть = один identifier)
                            # ВМЕСТО ЭТОГО:
x = (1, 2, 3)
a = x[0]
b = x[1]
c = x[2]
```

### Вложенные comprehension в одном выражении

```python
# НЕ работает:
[x + y for x in A for y in B]

# Разбить:
result = []
for x in A:
    for y in B:
        result.append(x + y)
```

### Наследование классов

```python
class Animal: ...
class Dog(Animal): ...   # НЕ поддерживается
```

Используйте композицию или один класс.

### Генераторы и yield

```python
def gen():
    yield 1  # НЕ поддерживается
```

### Лямбда-функции

```python
f = lambda x: x * 2  # НЕ поддерживается
```

Определите обычную функцию.

### Асинхронность, threading

Никаких `async`, `await`, `threading`.

### Множественное наследование, метаклассы

Не поддерживается.

### Оператор распаковки

```python
def f(*args, **kwargs): ...   # НЕ поддерживается
a, *rest = [1, 2, 3]          # НЕ поддерживается
```

### Срезы со step

```python
A[::2]     # НЕ поддерживается
A[::-1]    # НЕ поддерживается (реверс делайте через цикл)
```

Поддерживаются только `A[lo:hi]`, `A[:hi]`, `A[lo:]`.

### Matplotlib, numpy, другие библиотеки

Только стандартные типы и наши встроенные.

## 🚧 Частые ошибки и решения

### Ошибка: «Классы не поддерживаются»

**Было до недавнего времени.** Теперь поддерживаются. Если видите эту
ошибку — обновите проект (`git pull`).

### Ошибка: «Списки из не-литералов пока не поддерживаются»

Используйте `ArrayExpr` — теперь работает. Если видите ошибку — значит,
устаревшая версия.

### Ошибка: «Неподдерживаемое выражение: tuple»

Tuple-литералы теперь работают. Проверьте версию.

### Ошибка: «Вложенные for в одном comprehension»

Разбейте на несколько comprehension или на обычные циклы.

### Ошибка: «Неподдерживаемое выражение: lambda»

Определите именованную функцию:

```python
# Не:
sorted(A, key=lambda x: -x)

# Лучше сортировать вручную или использовать свой comparator
```

### Runtime: «Max steps exceeded»

Программа выполняется слишком долго. Скорее всего бесконечный цикл.
Проверьте условия выхода. Лимит по умолчанию — 1 000 000 шагов.

### Runtime: «Unknown function: X»

Функция `X` не объявлена **до момента вызова**. В Python функции видны
друг из друга в любом порядке, у нас — тоже, но если что-то не так,
объявите функцию раньше.

## 🔧 Советы

- **Пишите проще.** Чем меньше конструкций, тем надёжнее транспиляция.
- **Именуйте функции по-английски.** `bubble_sort`, `binary_search` — норм.
- **Используйте `annotate(...)` для пояснений** — они появятся в шагах.
- **Комментируйте** код: комментарии игнорируются транспайлером, но
  полезны для читателя.
- **Проверяйте на маленьких данных** сначала.

## 📊 Покрытие

Smoke-набор из 20 тестов (`tests/python_smoke/`) покрывает:

- 5 сортировок (bubble, selection, insertion, quick, merge)
- 2 поиска (linear, binary)
- 3 рекурсии (factorial, fibonacci, gcd)
- 2 обхода графа (BFS, DFS)
- 1 BST insert
- dict, set, stack
- tuple unpacking, chained comparison, f-strings, классы

Все 20 проходят транспиляцию и исполнение без ошибок.
MD
```

## 📖 Шаг 3. `docs/install-native.md`

```bash
cat > ~/projects/AlgoVis/docs/install-native.md <<'MD'
# Установка нативных библиотек tree-sitter

AlgoVis использует tree-sitter через P/Invoke. Нужны две `.so`:

- `libtree-sitter.so` — ядро
- `libtree-sitter-python.so` — грамматика Python

**Версии важны.** Мы проверяли на паре:

- `tree-sitter` **v0.22.6** (ABI 14)
- `tree-sitter-python` **v0.21.0** (ABI 14)

Более новые версии грамматики используют ABI 15 и вызовут segfault.

## Сборка

```bash
sudo apt update
sudo apt install -y build-essential git

# Ядро
cd /tmp
git clone --depth 1 --branch v0.22.6 https://github.com/tree-sitter/tree-sitter.git
cd tree-sitter
make
sudo make install

# Грамматика Python
cd /tmp
git clone --depth 1 --branch v0.21.0 https://github.com/tree-sitter/tree-sitter-python.git
cd tree-sitter-python
gcc -shared -fPIC -o libtree-sitter-python.so src/parser.c src/scanner.c -Isrc
sudo cp libtree-sitter-python.so /usr/local/lib/

# Регистрация
echo "/usr/local/lib" | sudo tee /etc/ld.so.conf.d/local.conf
sudo ldconfig
```

## Проверка

```bash
ldconfig -p | grep tree-sitter
```

Ожидаемый вывод:

```
libtree-sitter-python.so (libc6,x86-64) => /usr/local/lib/libtree-sitter-python.so
libtree-sitter.so.0 (libc6,x86-64) => /usr/local/lib/libtree-sitter.so.0
libtree-sitter.so (libc6,x86-64) => /usr/local/lib/libtree-sitter.so
```

## Проверка ABI

```bash
cat > /tmp/ts_abi.c <<'EOF'
#include <stdio.h>
#include <tree_sitter/api.h>
extern const TSLanguage *tree_sitter_python(void);
int main() {
    const TSLanguage *lang = tree_sitter_python();
    printf("grammar ABI: %u\n", ts_language_version(lang));
    printf("core   ABI: %u\n", TREE_SITTER_LANGUAGE_VERSION);
    return 0;
}
EOF

gcc /tmp/ts_abi.c -o /tmp/ts_abi -L/usr/local/lib -ltree-sitter -ltree-sitter-python
/tmp/ts_abi
```

Должно вывести **оба ABI = 14**. Если grammar > core — пересоберите
грамматику из v0.21.0.

## Где ищутся библиотеки

`AlgoVis.Parser/Native/LibLoader.cs` ищет `.so` в порядке:

1. `/usr/local/lib/`
2. `<директория приложения>/native/linux-x64/`
3. `<рабочая директория>/native/linux-x64/`

Если библиотеки в другом месте — поправьте `LibLoader.cs` или создайте
симлинк в `/usr/local/lib/`.

## Docker

Если разработка идёт внутри контейнера — пропишите сборку tree-sitter
в `Dockerfile` (см. пример в `docs/docker-example.md`).
MD
```

## 📖 Шаг 4. `docs/architecture.md`

```bash
cat > ~/projects/AlgoVis/docs/architecture.md <<'MD'
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

- `Native/TsNative.cs` — P/Invoke сигнатуры
- `Native/LibLoader.cs` — резолвер путей к `.so`
- `Parsing/TsParser.cs`, `TsTree.cs`, `TsNode.cs` — managed-обёртки
- `Parsing/PythonParser.cs` — фасад

### AlgoVis.Transpiler

Превращает CST Python в YAWA AST.

- `PythonToYawa.cs` — основной конвертер (800+ строк)
- `UnsupportedFeatureException.cs` — ошибка с координатами

**Принципы:**
- Не падать там, где можно обойтись — эмитить warning и продолжать
- Падать только на действительно неподдерживаемых конструкциях
- Все ошибки содержат `line:column` для UX

### AlgoVis.Yawa

Всё про YAWA: модель, runtime, интерпретатор, trace.

- `Yawa/` — DTO YAWA-программы
- `Trace/` — DTO выходной трассы
- `Runtime/RuntimeValue.cs` и наследники — значения во время исполнения
- `Runtime/Interpreter.cs` — главный цикл исполнения
- `Runtime/Evaluator.cs` — вычисление выражений
- `Runtime/Builtins.cs` — встроенные функции
- `Runtime/TraceRecorder.cs` — запись шагов
- `Runtime/Frame.cs` — области видимости

### AlgoVis.Server

ASP.NET Core:

- `Controllers/Yawa/YawaController.cs` — REST API
- `Controllers/Yawa/YawaViewerHtml.cs` — HTML viewer (встроен)

Endpoints:

| Метод | Путь | Назначение |
|---|---|---|
| GET | `/api/yawa/health` | проверка что сервер жив |
| GET | `/api/yawa/samples` | список готовых YAWA-файлов |
| GET | `/api/yawa/samples/{name}` | содержимое YAWA |
| POST | `/api/yawa/run` | YAWA-JSON → Trace |
| POST | `/api/yawa/run-sample?name=X` | sample → Trace |
| POST | `/api/yawa/run-python` | Python-код → Trace |
| GET | `/api/yawa/viewer` | HTML-визуализатор |

## Ключевые модели данных

### YAWA (YawaProgram)

Структурное представление алгоритма. Не пишется руками — генерируется
транспайлером или визуальным конструктором. См. `docs/yawa-schema.md`.

### Trace (TraceSession)

Результат исполнения: список шагов, каждый со своим diff, highlight,
аннотацией и (периодически) полным snapshot.

```json
{
  "steps": [
    { "n": 5, "kind": "compare", "highlight": ["A[3]","A[4]"],
      "diff": [{"target":"compare.a","old":null,"new":7}],
      "vars": {"A": [...], "i": 2} },
    ...
  ]
}
```

## Управление потоком в интерпретаторе

Используются исключения для `return`, `break`, `continue`. Это позволяет
не таскать флаги через каждый вызов.

- `ReturnException` — из `return`
- `BreakException` — из `break`
- `ContinueException` — из `continue`
- `YawaRuntimeException` — ошибки исполнения

## Дальнейшее развитие

- **Классы**: сейчас через top-level функции `ClassName.method` +
  объекты с полем `__type__`. Наследование не поддерживается.
- **Визуализация**: `viewer` — минимальный HTML+JS, полный фронт
  разрабатывается отдельно в `algovis.client`.
- **Мультиязычность**: транспорт YAWA не привязан к Python. Можно
  написать транспайлер из C#, JS и т.д.
MD
```

## 📖 Шаг 5. `docs/docker-example.md`

```bash
cat > ~/projects/AlgoVis/docs/docker-example.md <<'MD'
# Docker: пример для разработки

Пример `Dockerfile` для контейнера, в котором работает сервер AlgoVis
с собранными `.so` tree-sitter и .NET 8.

```dockerfile
FROM gitpod/openvscode-server:latest

USER root

RUN apt-get update && apt-get install -y --no-install-recommends \
        curl wget unzip git ca-certificates \
        libicu70 libssl3 build-essential \
    && rm -rf /var/lib/apt/lists/*

# .NET 8
RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm /tmp/dotnet-install.sh

ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="$PATH:/usr/share/dotnet"

# tree-sitter core
RUN cd /tmp \
    && git clone --depth 1 --branch v0.22.6 https://github.com/tree-sitter/tree-sitter.git \
    && cd tree-sitter && make && make install \
    && cd /tmp && rm -rf tree-sitter

# tree-sitter-python
RUN cd /tmp \
    && git clone --depth 1 --branch v0.21.0 https://github.com/tree-sitter/tree-sitter-python.git \
    && cd tree-sitter-python \
    && gcc -shared -fPIC -o libtree-sitter-python.so src/parser.c src/scanner.c -Isrc \
    && cp libtree-sitter-python.so /usr/local/lib/ \
    && cd /tmp && rm -rf tree-sitter-python \
    && echo "/usr/local/lib" > /etc/ld.so.conf.d/local.conf \
    && ldconfig

USER 0
```

Сборка и запуск:

```bash
docker compose build
docker compose up -d
```

`docker-compose.yml`:

```yaml
services:
  algovis:
    build: .
    container_name: algovis-dev
    restart: unless-stopped
    ports:
      - "127.0.0.1:3000:3000"   # OpenVSCode
      - "127.0.0.1:5000:5000"   # AlgoVis.Server
    volumes:
      - ./workspace:/home/workspace:cached
    command: >
      --host 0.0.0.0 --port 3000 --without-connection-token
      --server-base-path /vscode
```

Nginx проксирует наружу:

```nginx
location /api/ {
    proxy_pass http://127.0.0.1:5000;
    proxy_set_header Host $host;
    proxy_read_timeout 300s;
}
location /vscode/ {
    auth_basic "VSCode";
    auth_basic_user_file /etc/nginx/.htpasswd;
    proxy_pass http://127.0.0.1:3000/;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection "upgrade";
}
```
MD
```
