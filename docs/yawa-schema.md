# YAWA — Yet Another Visualization Abstraction

Версия схемы: **1.0**
Формат: JSON (UTF-8)

## Назначение

YAWA — внутренний формат программ-алгоритмов, которые интерпретируются
для построения пошаговой визуализации. Не пишется руками: генерируется
транспайлером из Python или визуальным конструктором.

## Структура верхнего уровня

```json
{
  "yawa_version": "1.0",
  "metadata": {
    "name": "Bubble Sort",
    "description": "Сортировка пузырьком",
    "generator": "hand-written"
  },
  "globals": {
    "N": { "type": "int", "value": {"lit": 10} }
  },
  "functions": [ /* FunctionDef[] */ ],
  "entry": {
    "function": "bubble_sort",
    "args": [ {"lit": [5, 2, 8, 1, 9, 3]} ]
  },
  "limits": {
    "max_steps": 100000,
    "max_depth": 1000,
    "max_seconds": 10,
    "snapshot_every": 100
  }
}
```

## FunctionDef

```json
{
  "name": "bubble_sort",
  "params": [ {"name": "A", "type": "array<int>"} ],
  "returns": "array<int>",
  "body": [ /* Statement[] */ ]
}
```

## Statement — список операций

Каждый statement — объект с полем `"op"`.

| `op` | Описание | Поля |
|---|---|---|
| `assign` | Присваивание | `target` (LValue), `value` (Expr) |
| `declare` | Объявление переменной | `name`, `type` (опц.), `value` (опц.) |
| `if` | Ветвление | `cond`, `then` (Statement[]), `else` (опц., Statement[]) |
| `while` | Цикл с условием | `cond`, `body` |
| `for` | Цикл с числом | `var`, `from`, `to`, `step` (опц., по умолч. 1), `body` |
| `foreach` | Обход коллекции | `var`, `in`, `body` |
| `return` | Возврат из функции | `value` (опц.) |
| `break` | Прервать цикл | — |
| `continue` | Следующая итерация | — |
| `expr` | Выражение-инструкция | `value` (Expr) |
| `swap` | Обмен значений | `a` (LValue), `b` (LValue) |
| `compare` | Сравнение с подсветкой | `a`, `b`, `result` (`<`, `>`, `==` и т.д.), `label` (опц.) |
| `mark` | Подсветка целевого узла | `target` (LValue), `color`, `label` (опц.) |
| `unmark` | Снять подсветку | `target` (LValue) |
| `annotate` | Комментарий к текущему шагу | `text` |
| `count` | Пользовательский счётчик | `name`, `delta` (опц., по умолч. 1) |
| `snapshot` | Явно сделать snapshot | `label` (опц.) |

### Примеры statements

```json
{"op": "assign",
 "target": {"ref": "x"},
 "value": {"bin": "+", "a": {"lit": 1}, "b": {"lit": 2}}}

{"op": "declare", "name": "n", "type": "int",
 "value": {"call": "length", "args": [{"ref": "A"}]}}

{"op": "swap",
 "a": {"index": ["A", {"ref": "i"}]},
 "b": {"index": ["A", {"ref": "j"}]}}

{"op": "compare",
 "a": {"index": ["A", {"ref": "i"}]},
 "b": {"index": ["A", {"bin": "+", "a": {"ref": "i"}, "b": {"lit": 1}}]},
 "result": ">",
 "label": "сравниваем пару"}

{"op": "mark", "target": {"index": ["A", {"ref": "i"}]},
 "color": "red", "label": "минимум"}

{"op": "annotate", "text": "нашли новый минимум"}
```

## Expression — выражения

| Форма | Описание | Пример |
|---|---|---|
| `lit` | Литерал | `{"lit": 5}`, `{"lit": "hi"}`, `{"lit": true}`, `{"lit": null}`, `{"lit": [1,2,3]}` |
| `ref` | Ссылка на переменную | `{"ref": "x"}` |
| `index` | Доступ по индексу | `{"index": ["A", {"ref": "i"}]}` |
| `field` | Доступ к полю объекта | `{"field": ["obj", "x"]}` |
| `bin` | Бинарная операция | `{"bin": "+", "a": ..., "b": ...}` |
| `un` | Унарная операция | `{"un": "-", "a": ...}` |
| `call` | Вызов функции | `{"call": "min", "args": [...]}` |
| `call_method` | Вызов метода (ресивер → первый аргумент) | `{"call_method": {"ref": "obj"}, "name": "area", "args": []}` |
| `new_object` | Создание объекта | `{"new_object": {"x": 0, "y": 0}}` |
| `len` | Длина | `{"len": {"ref": "A"}}` |
| `ternary` | Тернарный оператор | `{"ternary": {"cond": ..., "then": ..., "else": ...}}` |

### Бинарные операторы

`+`, `-`, `*`, `/`, `%`, `**`, `==`, `!=`, `<`, `<=`, `>`, `>=`, `and`, `or`,
а также `in`, `not_in` для проверки вхождения.

### Унарные

`-` (минус), `not`, `+`.

## LValue — то, куда можно присвоить

| Форма | Пример |
|---|---|
| `ref` | `{"ref": "x"}` — переменная |
| `index` | `{"index": ["A", {"ref": "i"}]}` — элемент массива |
| `field` | `{"field": ["obj", "x"}]` — поле объекта |

> LValue отличается от Expr тем, что интерпретатор умеет его *изменять*.
> Синтаксически совпадает с соответствующими Expr.

## Классы и объекты

Классов в YAWA **нет**. Есть объекты — анонимные словари полей.

- Создание: `{"new_object": {"x": 0, "y": 0}}`
- Доступ: `{"field": ["obj", "x"]}`
- Присваивание: `{"op": "assign", "target": {"field": ["obj", "x"]}, "value": {"lit": 5}}`

Метод вызывается через `call_method`. Интерпретатор ищет **глобальную функцию**
с таким именем и подставляет `obj` первым аргументом:

```json
{"call_method": {"ref": "obj"}, "name": "area", "args": []}
```

эквивалентно

```json
{"call": "area", "args": [{"ref": "obj"}]}
```

Поле `__type__` в объекте — необязательная метка для рендера:

```json
{"new_object": {"__type__": "Point", "x": 0, "y": 0}}
```

Интерпретатор его игнорирует, фронт может использовать для отрисовки.

## Встроенные функции

| Имя | Аргументы | Возвращает |
|---|---|---|
| `length` / `len` | `array` \| `string` | `int` |
| `min`, `max` | `array` \| список аргументов | значение |
| `abs` | число | число |
| `print` | произвольное число аргументов | `null` (пишет в annotation) |
| `range` | `(n)` \| `(start, stop)` \| `(start, stop, step)` | `array<int>` |
| `push` | `(array, value)` | новый размер |
| `pop` | `(array)` | значение |
| `insert` | `(array, index, value)` | `null` |
| `remove` | `(array, index)` | значение |
| `contains` | `(array, value)` | `bool` |
| `find` | `(array, value)` | индекс \| `-1` |
| `insert_node` | `(tree, value)` | `null` |
| `delete_node` | `(tree, value)` | `bool` |
| `find_node` | `(tree, value)` | узел \| `null` |
| `add_edge` | `(graph, a, b, weight?)` | `null` |
| `remove_edge` | `(graph, a, b)` | `bool` |

## Структуры данных (встроенные)

Значения YAWA бывают:

- `int`, `float`, `string`, `bool`, `null`
- `array<T>` — упорядоченный список
- `object` — словарь полей
- `tree` — бинарное дерево (корень + левый/правый)
- `graph` — граф (узлы + рёбра)
- `stack`, `queue`, `hash_table` — по мере необходимости

Литерал массива: `{"lit": [1, 2, 3]}`.

## Trace — выходной формат

```json
{
  "yawa_version": "1.0",
  "session_id": "...",
  "metadata": { /* из YAWA */ },
  "structure": { /* описание начального состояния */ },
  "steps": [ /* Step[] */ ],
  "final_state": { /* состояние после завершения */ },
  "statistics": {
    "total_steps": 47,
    "comparisons": 12,
    "swaps": 6,
    "memory_accesses": 30,
    "user_counters": { "comparisons": 12, "swaps": 6 },
    "structure_sizes": { "A": 6 },
    "big_o_hint": null
  }
}
```

### Step

```json
{
  "n": 1,
  "kind": "compare",
  "node_id": "n17",
  "diff": [
    {"target": "compare.a", "old": null, "new": "A[0]"},
    {"target": "compare.b", "old": null, "new": "A[1]"}
  ],
  "highlight": ["A[0]", "A[1]"],
  "annotation": "сравниваем пару",
  "stats": {
    "comparisons": 1,
    "swaps": 0,
    "steps_total": 1
  },
  "snapshot": null
}
```

### Виды Step.kind

- `init` — старт программы
- `assign`, `declare`, `call`, `return`, `eval` — обычные операции
- `compare`, `swap`, `mark`, `unmark` — визуальные операции
- `snapshot` — полный snapshot (для быстрого отката)
- `error` — ошибка (переполнение лимитов и т.п.)
- `end` — завершение

### Периодичность snapshot

- При входе в функцию → `snapshot` со `kind = "snapshot"`
- При выходе из функции → `snapshot`
- Каждые `snapshot_every` шагов (если задано в YAWA) → `snapshot`
- Явно через `{"op": "snapshot"}`

Фронт может использовать snapshot-ы для быстрого перехода к произвольному шагу
(последний snapshot до нужного шага + проигрывание diff-ов).

## Лимиты безопасности

- `max_steps` — при превышении: `Step.kind = "error"`, интерпретация прерывается.
- `max_depth` — лимит глубины рекурсии.
- `max_seconds` — реальное время.
- `max_array_size` — лимит размера массива (по умолчанию 10 000).
- `max_object_fields` — лимит числа полей объекта (по умолчанию 1 000).

Лимиты могут быть переопределены на уровне `entry.limits`.