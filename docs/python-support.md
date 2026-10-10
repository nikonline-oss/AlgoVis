# Поддерживаемый Python

Транспайлер `AlgoVis.Transpiler` поддерживает широкое подмножество Python 3,
достаточное для студенческих алгоритмов любой сложности. Документ описывает
возможности и ограничения.

## ✅ Полностью поддерживается

### Функции

```python
def add(a, b):
    return a + b

def greet(name="мир"):
    return "привет, " + name

def log(x: int) -> int:    # аннотации типов игнорируются
    return x

def total(*nums):          # *args
    s = 0
    for n in nums:
        s = s + n
    return s

def describe(**info):      # **kwargs
    return len(info)
```

- Рекурсия
- Множественные return: `return a, b`
- Nested functions и **замыкания** (nested function читает переменные родителя)
- Lambda-функции
- Декораторы (игнорируются, не ломают)

### Ветвления

- `if` / `elif` / `else` (цепочки любой длины)
- Цепочки сравнений: `0 <= x <= 100` → `(0 <= x) and (x <= 100)`
- `is None`, `is not None`
- `in` / `not in`
- Логические `and` / `or` / `not` с short-circuit

### Циклы

- `for i in range(n)` — 1, 2, 3 аргумента
- `for x in collection` — array, string, set, dict (по ключам), tuple
- Распаковка: `for k, v in pairs`
- `while`, `break`, `continue`
- Обратный range: `range(n, 0, -1)`

### Исключения

```python
try:
    risky_operation()
except:
    handle_error()
finally:
    cleanup()
```

Ловится **любое** исключение. `except X as e` — тип игнорируется, `e` получает
текст ошибки.

### Assert

```python
assert condition, "message"
```

При `False` бросает runtime-ошибку с указанным сообщением.

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
| dict | `{"a": 1}` | `d[k]`, `d[k] = v`, `del d[k]`, `in`, `len`, `k in d` |
| object | `ClassName()` | атрибуты, методы |

**Отрицательные индексы**: `A[-1]`, `A[-2]` — работает для array, string, tuple.

**Срезы**: `A[lo:hi]`, `A[:hi]`, `A[lo:]`, `A[::2]`, `A[::-1]`.

### Классы

```python
class Animal:
    def __init__(self, name):
        self.name = name

    def speak(self):
        return "..."

    @staticmethod
    def kind():
        return "animal"

class Dog(Animal):
    def speak(self):
        return "Woof"

d = Dog("Rex")
d.speak()                       # "Woof"
Animal.kind()                   # вызов статического метода
```

- Наследование через `class Child(Parent)`
- Переопределение методов
- `__init__` вызывается автоматически
- `@staticmethod` — вызывается как `ClassName.method()`
- **Множественное наследование не поддерживается**

### Comprehensions

```python
[x * 2 for x in A]
[x for x in A if x > 0]
{a: a*a for a in A}
{x * x for x in A if x > 0}
[a * b for a in A for b in B]     # вложенные
```

Все типы: list, set, dict. Вложенные `for` поддерживаются. Один `if` на каждый `for`.

### Генераторные выражения

```python
sum(x * x for x in A)
any(x < 0 for x in A)
all(x > 0 for x in A)
```

Внутри функций `sum`, `any`, `all` — превращаются в list comprehension.

### Встроенные функции

| Функция | Описание |
|---|---|
| `len(x)` / `length(x)` | длина array/string/set/dict/tuple |
| `range(...)` | 1-3 аргумента |
| `min(...)`, `max(...)` | от массива или списка аргументов |
| `abs(x)` | модуль |
| `sorted(A)` | сортировка |
| `sorted(A, key=abs)` | по модулю |
| `sorted(A, key=len)` | по длине |
| `sorted(A, key=func)` | по произвольной функции |
| `enumerate(A)` | пары (индекс, элемент) |
| `zip(A, B, ...)` | кортежи параллельных элементов |
| `sum(A)` | сумма чисел |
| `any(A)`, `all(A)` | логические свёртки |
| `chr(code)` | символ по коду |
| `ord(ch)` | код символа |
| `int(x)`, `str(x)`, `float(x)`, `bool(x)` | преобразования |
| `list(x)`, `set(x)`, `tuple(x)` | преобразования коллекций |
| `filter(func, A)`, `map(func, A)` | высшего порядка |

### Методы строк

- `strip()`, `lstrip()`, `rstrip()`
- `lower()`, `upper()`, `capitalize()`, `title()`
- `split()`, `split(sep)`
- `join(A)`
- `replace(old, new)`
- `startswith(prefix)`, `endswith(suffix)`
- `find(sub)`, `count(sub)`
- `isdigit()`, `isalpha()`, `isspace()`, `isupper()`, `islower()`

### Множественное присваивание

```python
a = b = c = 0
x, y = 1, 2
x, y = y, x                  # swap
head, *rest = [1, 2, 3, 4]   # splat
```

### F-strings

```python
f"Hello, {name}! You have {count * 2} items"
```

### Импорты

```python
import math
from typing import List

math.pi, math.e, math.tau    # константы поддерживаются
```

Другие модули игнорируются (кроме поддержанных констант math).

### Операции визуализации

Специальные функции (не из Python, добавлены в YAWA для UX):

```python
annotate("текст шага")     # подпись к текущему шагу
mark(A, i, "red")          # подсветить элемент массива
swap(A, i, j)              # поменять местами
compare(A[i], A[j])        # явное сравнение с подсветкой
```

### Dict-специфика

```python
d = {}
d["key"] = value
x = d["key"]
if "key" in d:
    ...
del d["key"]
for key in d:                # обход по ключам
    ...
```

## ⚠️ Ограничения

### Не поддерживается

| Конструкция | Комментарий |
|---|---|
| `match` / `case` | Планируется позже |
| `async` / `await` | Не нужно для визуализации алгоритмов |
| Многопоточность | Не поддерживается |
| Множественное наследование | `class C(A, B)` не работает |
| `super()` | Планируется |
| `@property`, `@classmethod` | Планируется |
| `yield` (генераторы как функции) | Не поддерживается |
| Метаклассы | Не поддерживается |
| `global` / `nonlocal` | **Игнорируются** — присваивание всегда локальное |
| `import` любых модулей, кроме math | Модули кроме math игнорируются |
| f-string с format-spec: `f"{x:.2f}"` | Только простые выражения |
| Множественная распаковка: `a, *b, *c = ...` | Один `*` максимум |

### Ослабленные места

- **`global` и `nonlocal`** молча игнорируются — код не сломается, но
  изменение глобальной переменной из функции не сработает.
- **`import` не-math модулей** — игнорируется, вызовы `random.randint` и
  подобные упадут с `Unknown function`. Для воспроизводимости это скорее плюс.
- **`try/except`** не различает типы исключений. `except ValueError` поймает
  любое исключение.

### Что делаем, если Python не транспилируется

1. **Проверьте строку в ошибке** — она указывает точное место.
2. **Попробуйте упростить** — большие вложенные expressions иногда
   не разбираются. Разбейте на несколько операторов.
3. **Сообщите разработчику** — если конструкция важна, её добавят.

## Тестовое покрытие

Три набора тестов в `tests/`:

| Набор | Файлов | Что покрывает |
|---|---|---|
| `python_smoke/` | 20 | Базовые алгоритмы: сортировки, поиски, рекурсии, BFS/DFS, dict, set, классы |
| `python_advanced/` | 30 | Сложные конструкции: comprehensions, assert, try/except, замыкания, lambda, *args, **kwargs, наследование, staticmethod, генераторы |
| `python_real/` | 10 | Реальные алгоритмы: Ханойские башни, Дейкстра, LRU-кэш, BFS на сетке, matrix ops |

Запуск:

```bash
tests/python_smoke/run_all.sh
tests/python_advanced/run_all.sh
tests/python_real/run_all.sh
```

Все три набора должны показывать **100%**.

## Известные ограничения производительности

- Интерпретатор — tree-walking. Медленнее, чем CPython, но для визуализации
  приемлемо.
- Лимит шагов: 1M (настраивается в YAWA).
- Лимит времени: 10 секунд.
- Большие массивы (>10k элементов) будут тормозить из-за размера trace.

## Ссылки

- [YAWA схема](yawa-schema.md)
- [API](api.md)
- [Архитектура](architecture.md)
