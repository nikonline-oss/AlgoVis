# YAWA — схема форматов

Документ описывает два формата:
- **YAWA** — входной формат программы (генерируется транспайлером)
- **Trace** — выходной формат исполнения (отдаётся клиенту)

## YAWA — формат программы

### Верхний уровень

```json
{
  "yawa_version": "1.0",
  "metadata": {
    "name": "Python → YAWA",
    "description": null,
    "generator": "algovis-transpiler@0.1"
  },
  "globals": {
    "N": { "value": { "lit": 10 } }
  },
  "functions": [ /* YawaFunction[] */ ],
  "entry": {
    "function": "main",
    "args": []
  },
  "limits": {
    "max_steps": 1000000,
    "max_depth": 500,
    "max_seconds": 10,
    "snapshot_every": 0
  }
}
```

### `YawaFunction`

```json
{
  "name": "bubble_sort",
  "params": [
    { "name": "A" },
    { "name": "args", "variadic": true },
    { "name": "kwargs", "kwvariadic": true }
  ],
  "returns": null,
  "body": [ /* YawaStatement[] */ ]
}
```

Флаги:
- `variadic: true` — `*args`
- `kwvariadic: true` — `**kwargs`

### Statements

Каждый statement — объект с полем `op`.

| `op` | Описание | Поля |
|---|---|---|
| `assign` | Присваивание | `target`, `value` |
| `declare` | Объявление | `name`, `type?`, `value?` |
| `tuple_assign` | Распаковка | `targets[]`, `values[]`, `splatIndex` |
| `if` | Ветвление | `cond`, `then[]`, `else[]?` |
| `while` | Цикл | `cond`, `body[]` |
| `for` | Range-цикл | `var`, `from`, `to`, `step?`, `body[]` |
| `foreach` | Обход коллекции | `var`, `in`, `body[]` |
| `return` | Возврат | `value?` |
| `break`, `continue` | Управление | — |
| `expr` | Выражение | `value` |
| `swap` | Обмен | `a`, `b` |
| `compare` | Сравнение с подсветкой | `a`, `b`, `result`, `label?` |
| `mark` | Подсветка | `target`, `color`, `label?` |
| `unmark` | Снять подсветку | `target` |
| `annotate` | Комментарий | `text` |
| `count` | Счётчик | `name`, `delta` |
| `assert` | Проверка | `cond`, `message?` |
| `try` | Try/except | `body[]`, `handlers[]`, `finally[]?` |
| `delete` | del | `targets[]` |
| `snapshot` | Явный snapshot | `label?` |

### Expressions

| Форма | Пример | Описание |
|---|---|---|
| `lit` | `{"lit": 5}` | Литерал (int, float, string, bool, null, array) |
| `ref` | `{"ref": "x"}` | Переменная |
| `index` | `{"index": [target, idx]}` | `A[i]` |
| `field` | `{"field": [target, "name"]}` | `obj.name` |
| `bin` | `{"bin": "+", "a": ..., "b": ...}` | Бинарная операция |
| `un` | `{"un": "-", "a": ...}` | Унарная |
| `call` | `{"call": "f", "args": [...]}` | Вызов функции |
| `call_method` | `{"call_method": recv, "name": "m", "args": [...]}` | `obj.m()` |
| `instantiate` | `{"instantiate": "Point", "args": [...]}` | Создание объекта |
| `new_object` | `{"new_object": {"x": 0}}` | Анонимный объект |
| `array` | `{"array": [expr...]}` | Массив с выражениями |
| `dict` | `{"dict": [{"k": ..., "v": ...}, ...]}` | Словарь |
| `set` | `{"set": [expr...]}` | Множество |
| `tuple` | `{"tuple": [expr...]}` | Кортеж |
| `slice` | `{"slice": [target, start, stop, step]}` | Срез (любой компонент = null) |
| `ternary` | `{"ternary": {"cond": ..., "then": ..., "else": ...}}` | Тернарный |
| `len` | `{"len": target}` | Длина |
| `funcref` | `{"funcref": "name"}` | Ссылка на функцию |
| `comp_body`, `comp_clauses` | см. ниже | List comprehension |
| `setc_body`, `setc_clauses` | см. ниже | Set comprehension |
| `dictk_key`, `dictk_value`, `dictk_clauses` | см. ниже | Dict comprehension |

### Comprehensions

```json
{
  "comp_body": { "bin": "*", "a": { "ref": "x" }, "b": { "lit": 2 } },
  "comp_clauses": [
    {
      "var": "x",
      "source": { "ref": "A" },
      "filter": { "bin": ">", "a": { "ref": "x" }, "b": { "lit": 0 } }
    }
  ]
}
```

Вложенные comprehension — несколько clauses:

```json
{
  "comp_body": { "bin": "*", "a": { "ref": "a" }, "b": { "ref": "b" } },
  "comp_clauses": [
    { "var": "a", "source": { "ref": "A" } },
    { "var": "b", "source": { "ref": "B" } }
  ]
}
```

### Операторы

**Бинарные:** `+ - * / // % ** == != < <= > >= and or in not_in`

**Унарные:** `- + not`

## Trace — формат выходной трассы

```json
{
  "yawa_version": "1.0",
  "session_id": "abc123...",
  "metadata": { "name": "Python → YAWA" },
  "structure": {
    "variables": [
      { "name": "A", "type": "array", "visual": "array" }
    ]
  },
  "steps": [ /* TraceStep[] */ ],
  "final_state": {
    "A": [1, 2, 3, 5, 8, 9],
    "__return__": [1, 2, 3, 5, 8, 9]
  },
  "statistics": {
    "total_steps": 100,
    "comparisons": 28,
    "swaps": 13,
    "memory_accesses": 112,
    "user_counters": { "comparisons": 28 },
    "structure_sizes": { "A": 8 }
  }
}
```

### `TraceStep`

```json
{
  "n": 5,
  "kind": "compare",
  "node_id": null,
  "diff": [
    { "target": "compare.a", "old": null, "new": 5 },
    { "target": "compare.b", "old": null, "new": 2 }
  ],
  "highlight": ["A[0]", "A[1]"],
  "annotation": "сравниваем пару",
  "stats": {
    "comparisons": 1,
    "swaps": 0,
    "memoryAccesses": 2,
    "stepsTotal": 6,
    "userCounters": {}
  },
  "vars": {
    "A": [5, 2, 8, 1],
    "i": 0,
    "j": 1
  },
  "snapshot": null
}
```

### Виды `step.kind`

| Kind | Что значит |
|---|---|
| `init` | Старт программы |
| `end` | Конец программы |
| `assign` | Присваивание |
| `declare` | Объявление переменной |
| `call` | Вызов функции |
| `return` | Возврат из функции |
| `compare` | Сравнение (с подсветкой) |
| `swap` | Обмен значений |
| `mark` / `unmark` | Подсветка элемента |
| `for` / `foreach` | Итерация цикла |
| `delete` | Удаление переменной/элемента |
| `annotate` | Пользовательский комментарий |
| `count` | Пользовательский счётчик |
| `snapshot` | Полное состояние (для быстрого перехода) |
| `error` | Программа упала |

### Snapshot vs diff

**diff** — список изменений на шаге:

```json
{ "target": "A[3]", "old": 5, "new": 2 }
```

Форматы `target`:
- `"A[3]"` — элемент массива
- `"A[1][2]"` — элемент матрицы
- `"obj.field"` — поле объекта
- `"i"` — переменная
- `"compare.a"` — служебное (можно игнорировать)

**snapshot** — полное состояние всех видимых переменных. Записывается при
входе/выходе из функции и периодически (по `snapshot_every`).

### Восстановление состояния на шаге N

```javascript
function stateAt(stepIdx, steps, snapshots) {
  // Найти ближайший snapshot до stepIdx
  let snapIdx = -1;
  for (const i of snapshots) {
    if (i <= stepIdx && i > snapIdx) snapIdx = i;
  }

  // Начальное состояние
  let state = {};
  if (snapIdx >= 0) state = JSON.parse(JSON.stringify(steps[snapIdx].snapshot));

  // Применить diff'ы от snapshot до stepIdx
  for (let i = snapIdx + 1; i <= stepIdx; i++) {
    const st = steps[i];
    if (!st.diff) continue;
    for (const d of st.diff) {
      const t = String(d.target);
      if (t.startsWith('compare.') || t.startsWith('__')) continue;

      let m = t.match(/^([A-Za-z_]\w*)\[(\d+)\]\[(\d+)\]$/);
      if (m) {
        const arr = state[m[1]];
        if (Array.isArray(arr) && Array.isArray(arr[+m[2]]))
          arr[+m[2]][+m[3]] = d.new;
        continue;
      }

      m = t.match(/^([A-Za-z_]\w*)\[(\d+)\]$/);
      if (m) {
        const arr = state[m[1]];
        if (Array.isArray(arr)) arr[+m[2]] = d.new;
        continue;
      }

      m = t.match(/^([A-Za-z_]\w*)\.(\w+)$/);
      if (m && m[1] !== 'object') {
        const obj = state[m[1]];
        if (obj && typeof obj === 'object') obj[m[2]] = d.new;
        continue;
      }

      if (/^[A-Za-z_]\w*$/.test(t)) state[t] = d.new;
    }
  }
  return state;
}
```

### Значения в snapshot и vars

| YAWA-тип | JSON-форма |
|---|---|
| int | `42` |
| float | `3.14` |
| string | `"hi"` |
| bool | `true` / `false` |
| null | `null` |
| array | `[1, 2, 3]` |
| object | `{"__type": "Point", "x": 0, "y": 0}` |
| set | `{"__type": "set", "items": [1, 2]}` |
| tuple | `{"__type": "tuple", "items": [1, 2]}` |
| tree_node | `{"value": 5, "left": {...}, "right": {...}}` |

### Специальные переменные

- `__return__` — возвращаемое значение entry-функции (появляется в конце)
- `__tup_N` — временные переменные для распаковки for-цикла (можно игнорировать)
- `__lambda_N` — сгенерированные имена для лямбда-функций

## Лимиты безопасности

При превышении любого лимита исполнение прерывается с `step.kind = "error"`.

- `max_steps` — количество шагов (по умолчанию 1M)
- `max_depth` — глубина рекурсии
- `max_seconds` — реальное время
- `max_array_size` — размер массива
