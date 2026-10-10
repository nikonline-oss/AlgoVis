# C++ — поддержка

Транспайлер: `AlgoVis.Transpiler.Cpp/CppToYawa.cs`.
Наборы: `tests/cpp_smoke/` (20), `tests/cpp_advanced/` (30), `tests/cpp_real/` (10).

**Итого: 60/60.**

## Поддерживается

**Типы:** `int`, `long`, `short`, `char`, `bool`, `float`, `double`, `string`, `vector`, `set`, `map`, `unordered_map`, `queue`, `stack`, `deque`, `list`, `array`, `pair`, пользовательские `struct`/`class`.

**Литералы:** числа (int/float), строки, `char`, `true`/`false`, `nullptr`, `NULL`, инициализаторы `{...}`.

**Операторы:** `+ - * / %`, сравнения, `&&`/`||`/`!`, `&`/`|` (упрощены до `and`/`or`), тернарник, составные `+= -= *= /= %=`.

**Управление:** `if`/`else`, `while`, `for` (классический и range-based), `break`, `continue`, `return`, `swap(a, b)`.

**Функции:** параметры (`int A[]`, `int *p`, `int &x`), рекурсия, `template<typename T>` для функций и классов.

**Классы:** конструкторы, методы, поля, `public`/`private`, эмуляция наследования. Поля инициализируются в конструкторе по типу (`int`→0, `bool`→false, `vector/set/map`→пустой контейнер).

**STL:** `push_back`/`pop_back`/`size`/`empty`/`front`/`back`/`insert`/`erase`, `begin`/`end` (no-op), `sort`/`reverse`/`min_element`/`max_element`/`accumulate`, `make_pair`, `.first`/`.second`, `map[k]` (auto-vivification: `map[k]` создаёт пустой элемент).

**Итераторы:** `for (auto it : v)`, `for (auto it = m.begin(); it != m.end(); it++)`, `it->first`/`it->second`, `*it`.

**Ввод-вывод:** `cout << ... << endl` → annotate.

**Массивы:** одномерные, многомерные, инициализаторы `int A[] = {1, 2, 3}`.

**Векторы с размером:** `vector<int> v(n)`, `vector<vector<bool>> visited(rows, vector<bool>(cols, false))` — через builtin `make_vector`, который создаёт массив с глубокими копиями шаблона.

**Константы:** `const`, `static`.

## Известные ограничения

- **`priority_queue`** — эмулируется как `ArrayValue`. `push`/`pop`/`top`/`size`/`empty` работают, но **порядок извлечения ≠ C++** (не по приоритету). Для визуализации достаточно.
- **Лямбды** `[](){}` — только с пустым capture. `[=]`, `[&]`, `[x, &y]` → `UnsupportedFeatureException`.
- **Компараторы** в `sort(v.begin(), v.end(), greater<int>())` — не поддержаны (в разработке).
- **Вложенные шаблоны** `map<int, vector<int>>` — работают через auto-vivification.
- **Указатели** как значения, арифметика указателей — не поддержаны.
- **`new`/`delete`** — не поддержаны.
- **`#include`, `using`, `namespace`** — игнорируются.

## Ключевые технические решения

- **`UnwrapConditionClause`** — в tree-sitter-cpp `if`/`while` оборачивают условие в `condition_clause`, извлекаем последнее named-child.
- **`TryConvertIteratorFor`** — распознаёт `for (auto it = m.begin(); ...)` по тексту `.begin()`/`.end()`. Для `it->first/second` → `ForeachPairStatement`, для `*it` → `ForeachStatement`.
- **`function_declarator` в `ConvertDeclaration`** — C++ "most vexing parse": `vector<bool> visited(n, false)` парсится как `function_declarator`. Разворачивается через `UnwrapFakeParameter`, который умеет:
  - `type_identifier`/`identifier` → `RefExpr` (с распознаванием `true`/`false`/`nullptr`);
  - вложенный `abstract_function_declarator` → рекурсивный `make_vector(...)`.
- **`ExtractClassMethods`** — методы имеют `field_identifier`, конструктор — `identifier`. Поля класса `_currentClassFields` заменяют `value` на `self.value`.
- **`template_declaration` на top-level** — раскрывается: `class_specifier` → `ExtractClassMethods`, `function_definition` → `ConvertFunction`.
- **`make_vector(n, init)`** — builtin, создающий массив длины `n` с **глубокими копиями** `init` (скаляры — по значению; массивы/сеты/объекты/кортежи — рекурсивный `DeepClone`). Без клонирования `visited[0]` и `visited[1]` были бы одной ссылкой.

## Грабли (не сломать)

- **`Frame.Set`** — всегда пишет в `Locals` текущего фрейма. Python-семантика shadowing. НЕ возвращать старое поведение с поиском в Parent.
- **`Frame.Get/Set` fallback на `self`** — если `self` есть в `Locals` и это `ObjectValue` с полем — читаем/пишем в поле. Это для C++-методов.
- **Auto-vivification `map[k]`** возвращает **пустой `ArrayValue`**, а не `IntValue(0)`. Так работает `map[k].push_back(v)`.
