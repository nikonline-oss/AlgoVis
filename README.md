
# ЯВА (Язык Визуализации Алгоритмов) - Полная документация

## 📖 Оглавление
1. [Введение](#введение)
2. [Базовые концепции](#базовые-концепции)
3. [Структура алгоритма](#структура-алгоритма)
4. [Типы данных и переменные](#типы-данных-и-переменные)
5. [Система выражений](#система-выражений)
6. [Операции и шаги](#операции-и-шаги)
7. [Функции](#функции)
8. [Визуализация](#визуализация)
9. [Структуры данных](#структуры-данных)
10. [Примеры алгоритмов](#примеры-алгоритмов)
11. [Лучшие практики](#лучшие-практики)

## 🎯 Введение

**ЯВА** (Язык Визуализации Алгоритмов) - это декларативный JSON-подобный язык для описания алгоритмов с поддержкой пошаговой визуализации. ЯВА компилируется в исполняемый код интерпретатором алгоритмов.

### Основные возможности:
- ✅ **Полноценная система типов** - числа, строки, булевы значения, массивы, объекты
- ✅ **Расширенная система выражений** - арифметика, логика, строковые операции, функции
- ✅ **Поддержка структур данных** - массивы, деревья, графы, связные списки
- ✅ **Визуализация** - подсветка, анимация, пошаговое выполнение
- ✅ **Функции и рекурсия** - модульность и повторное использование кода
- ✅ **Управление потоком** - условия, циклы, переходы

## 🧩 Базовые концепции

### Структура проекта
```
algorithm.json
├── Метаданные (название, описание)
├── Переменные (объявления)
├── Функции (вспомогательные)
└── Шаги (последовательность операций)
```

### Принцип выполнения
1. **Инициализация** - создание переменных и структур данных
2. **Последовательное выполнение** - шаги выполняются по порядку
3. **Визуализация** - каждый шаг может генерировать визуальное представление
4. **Управление потоком** - переходы между шагами через условия

## 🏗️ Структура алгоритма

### Базовая структура
```json
{
  "name": "Название алгоритма",
  "description": "Подробное описание что делает алгоритм",
  "structureType": "array|binarytree|graph|linkedlist",
  "variables": [
    // Определения переменных
  ],
  "functions": [
    // Определения функций  
  ],
  "steps": [
    // Последовательность шагов выполнения
  ]
}
```

### Пример минимального алгоритма
```json
{
  "name": "SimpleCounter",
  "description": "Простой счетчик от 0 до 5",
  "structureType": "array",
  "variables": [
    {
      "name": "counter",
      "type": "int",
      "initialValue": 0
    }
  ],
  "steps": [
    {
      "id": "start",
      "type": "assign",
      "description": "Инициализация счетчика",
      "parameters": ["counter", "0"],
      "nextStep": "increment"
    },
    {
      "id": "increment",
      "type": "assign",
      "description": "Увеличение счетчика",
      "parameters": ["counter", "counter + 1"],
      "nextStep": "check"
    },
    {
      "id": "check",
      "type": "condition",
      "description": "Проверка условия",
      "parameters": ["counter < 5"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "increment"
        },
        {
          "condition": "false",
          "nextStep": "end"
        }
      ]
    },
    {
      "id": "end",
      "type": "generic",
      "description": "Алгоритм завершен",
      "parameters": []
    }
  ]
}
```

## 📊 Типы данных и переменные

### Базовые типы данных

```json
{
  "variables": [
    {
      "name": "age",
      "type": "int",
      "initialValue": 25,
      "description": "Целое число"
    },
    {
      "name": "temperature",
      "type": "double", 
      "initialValue": 36.6,
      "description": "Число с плавающей точкой"
    },
    {
      "name": "is_active",
      "type": "bool",
      "initialValue": true,
      "description": "Логическое значение"
    },
    {
      "name": "username",
      "type": "string",
      "initialValue": "john_doe",
      "description": "Строка текста"
    }
  ]
}
```

### Строковые литералы
```json
{
  "variables": [
    {
      "name": "simple_string",
      "type": "string",
      "initialValue": "Hello World"
    },
    {
      "name": "quoted_string", 
      "type": "string",
      "initialValue": "\"Вложенные кавычки\""
    },
    {
      "name": "escape_string",
      "type": "string",
      "initialValue": "Первая строка\nВторая строка\tТабуляция"
    }
  ]
}
```

### Массивы
```json
{
  "variables": [
    {
      "name": "numbers",
      "type": "array",
      "initialValue": [1, 2, 3, 4, 5],
      "arraySize": 10,
      "description": "Массив чисел с начальной инициализацией"
    },
    {
      "name": "empty_array",
      "type": "array", 
      "initialValue": [],
      "arraySize": 5,
      "description": "Пустой массив фиксированного размера"
    },
    {
      "name": "strings_array",
      "type": "array",
      "initialValue": ["apple", "banana", "cherry"],
      "description": "Массив строк"
    }
  ]
}
```

### Объекты
```json
{
  "variables": [
    {
      "name": "user",
      "type": "object",
      "ObjectProperties": {
        "name": "Алексей",
        "age": 30,
        "active": true,
        "tags": ["admin", "moderator"]
      },
      "description": "Объект с различными свойствами"
    },
    {
      "name": "node",
      "type": "object",
      "ObjectProperties": {
        "value": 10,
        "visited": false,
        "children": []
      },
      "description": "Узел дерева"
    }
  ]
}
```

### Специализированные переменные для алгоритмов
```json
{
  "variables": [
    {
      "name": "stack",
      "type": "array",
      "initialValue": [],
      "description": "Стек для алгоритмов обхода"
    },
    {
      "name": "visited",
      "type": "array",
      "initialValue": [],
      "description": "Посещенные узлы/элементы"
    },
    {
      "name": "result",
      "type": "array", 
      "initialValue": [],
      "description": "Результирующая коллекция"
    },
    {
      "name": "temp",
      "type": "int",
      "initialValue": 0,
      "description": "Временная переменная для обменов"
    }
  ]
}
```

## 🧮 Система выражений

### Арифметические операции
```json
{
  "expressions": [
    {"example": "5 + 3 * 2", "result": "11", "description": "Приоритет операций"},
    {"example": "(10 - 4) / 2", "result": "3", "description": "Скобки изменяют порядок"},
    {"example": "2 ^ 3", "result": "8", "description": "Возведение в степень"},
    {"example": "7 % 3", "result": "1", "description": "Остаток от деления"},
    {"example": "-5 + 3", "result": "-2", "description": "Унарный минус"}
  ]
}
```

### Строковые операции
```json
{
  "expressions": [
    {
      "example": "\"Hello\" + \" \" + \"World\"", 
      "result": "\"Hello World\"", 
      "description": "Конкатенация строк"
    },
    {
      "example": "\"Number: \" + 42", 
      "result": "\"Number: 42\"", 
      "description": "Автоматическое преобразование чисел в строки"
    },
    {
      "example": "name + \" - \" + age", 
      "result": "\"John - 25\"", 
      "description": "Конкатенация переменных"
    }
  ]
}
```

### Логические операции
```json
{
  "expressions": [
    {"example": "a > b && c < d", "description": "Логическое И"},
    {"example": "x == y || z != w", "description": "Логическое ИЛИ"}, 
    {"example": "!is_visited", "description": "Логическое НЕ"},
    {"example": "(a > b) && (c < d || e == f)", "description": "Комбинированные условия"}
  ]
}
```

### Операции сравнения
```json
{
  "expressions": [
    {"example": "a == b", "description": "Равенство (числа, строки, булевы значения)"},
    {"example": "a != b", "description": "Неравенство"},
    {"example": "a > b", "description": "Больше (числа)"},
    {"example": "a < b", "description": "Меньше (числа)"},
    {"example": "a >= b", "description": "Больше или равно"},
    {"example": "a <= b", "description": "Меньше или равно"},
    {"example": "\"apple\" < \"banana\"", "result": "true", "description": "Лексикографическое сравнение строк"}
  ]
}
```

### Функции в выражениях
```json
{
  "expressions": [
    {"example": "sqrt(16)", "result": "4", "description": "Квадратный корень"},
    {"example": "min(5, 10)", "result": "5", "description": "Минимум из двух чисел"},
    {"example": "max(array[0], array[1])", "description": "Максимум элементов массива"},
    {"example": "abs(-5)", "result": "5", "description": "Модуль числа"},
    {"example": "pow(2, 3)", "result": "8", "description": "Возведение в степень"},
    {"example": "length(\"Hello\")", "result": "5", "description": "Длина строки"},
    {"example": "substring(\"Hello\", 1, 3)", "result": "\"ell\"", "description": "Подстрока"},
    {"example": "concat(\"A\", \"B\", \"C\")", "result": "\"ABC\"", "description": "Объединение строк"},
    {"example": "toupper(\"hello\")", "result": "\"HELLO\"", "description": "Верхний регистр"},
    {"example": "tolower(\"WORLD\")", "result": "\"world\"", "description": "Нижний регистр"}
  ]
}
```

### Доступ к элементам структур данных
```json
{
  "expressions": [
    {"example": "numbers[i]", "description": "Элемент массива по индексу"},
    {"example": "user.name", "description": "Свойство объекта"},
    {"example": "graph.nodes[0].connections", "description": "Вложенный доступ"},
    {"example": "array[i + 1]", "description": "Выражение в качестве индекса"},
    {"example": "matrix[i][j]", "description": "Многомерный массив"},
    {"example": "tree.root.left.value", "description": "Цепочка свойств"}
  ]
}
```

### Специальные выражения
```json
{
  "expressions": [
    {"example": "array_length", "description": "Длина массива (автоматическая переменная)"},
    {"example": "last_comparison", "description": "Результат последнего сравнения (-1, 0, 1)"},
    {"example": "i < array_length - 1", "description": "Типичное условие цикла для массивов"},
    {"example": "step_count", "description": "Текущий номер шага выполнения"},
    {"example": "recursion_depth", "description": "Глубина рекурсии"}
  ]
}
```

## ⚙️ Операции и шаги

### 1. Присваивание (assign)
```json
{
  "id": "assign_example",
  "type": "assign",
  "description": "Присвоение значения переменной",
  "parameters": ["variable_name", "expression"],
  "nextStep": "next_step_id",
  "visualize": true,
  "highlightElements": ["variable_name"],
  "highlightColor": "blue",
  "metadata": {
    "operation_type": "assignment",
    "importance": "high"
  }
}
```

**Примеры присваивания:**
```json
{"parameters": ["i", "0"], "description": "Присвоение константы"}
{"parameters": ["j", "i + 1"], "description": "Присвоение результата выражения"}
{"parameters": ["array[i]", "temp"], "description": "Присвоение элементу массива"}
{"parameters": ["node.visited", "true"], "description": "Присвоение свойству объекта"}
{"parameters": ["message", "'Hello' + ' World'"], "description": "Присвоение строки"}
{"parameters": ["counter", "counter + 1"], "description": "Инкремент переменной"}
```

### 2. Сравнение (compare)
```json
{
  "id": "compare_step",
  "type": "compare", 
  "description": "Сравнение двух элементов массива",
  "parameters": ["index1", "index2"],
  "nextStep": "after_compare",
  "visualize": true,
  "highlightElements": ["index1", "index2"],
  "highlightColor": "yellow",
  "metadata": {
    "operation": "comparison",
    "visualization": "highlight_both"
  }
}
```

### 3. Обмен (swap)
```json
{
  "id": "swap_step",
  "type": "swap",
  "description": "Обмен элементов местами",
  "parameters": ["index1", "index2"],
  "nextStep": "next_step",
  "visualize": true,
  "highlightElements": ["index1", "index2"],
  "highlightColor": "red",
  "metadata": {
    "animation": "swap",
    "duration": 500
  }
}
```

### 4. Условие (condition)
```json
{
  "id": "conditional_check",
  "type": "condition",
  "description": "Проверка условия и переход",
  "parameters": ["condition_expression"],
  "conditionCases": [
    {
      "condition": "true",
      "nextStep": "if_true_step",
      "description": "Выполняется если условие истинно"
    },
    {
      "condition": "false", 
      "nextStep": "if_false_step",
      "description": "Выполняется если условие ложно"
    }
  ],
  "visualize": true,
  "metadata": {
    "branch_type": "conditional"
  }
}
```

**Примеры условий:**
```json
{"parameters": ["array[i] > array[j]"], "description": "Сравнение элементов массива"}
{"parameters": ["user.name == 'Алексей'"], "description": "Сравнение строк"}
{"parameters": ["length(message) > 10"], "description": "Проверка длины строки"}
{"parameters": ["is_sorted || i >= array_length"], "description": "Комбинированное условие"}
{"parameters": ["!visited.includes(current.id)"], "description": "Проверка в массиве"}
{"parameters": ["counter % 2 == 0"], "description": "Проверка четности"}
```

### 5. Вызов функции (call_function)
```json
{
  "id": "function_call",
  "type": "call_function",
  "description": "Вызов пользовательской функции",
  "functionName": "function_name",
  "functionParameters": {
    "param1": "value1",
    "param2": "value2"
  },
  "returnToStep": "step_after_return",
  "visualize": true,
  "metadata": {
    "call_depth": "current_depth + 1",
    "function_type": "user_defined"
  }
}
```

### 6. Возврат из функции (return)
```json
{
  "id": "return_from_function",
  "type": "return", 
  "description": "Возврат из функции",
  "nextStep": "step_after_return",
  "visualize": false,
  "metadata": {
    "operation": "function_return"
  }
}
```

### 7. Универсальная операция (generic)
```json
{
  "id": "custom_operation",
  "type": "generic",
  "description": "Произвольная операция с метаданными",
  "parameters": ["param1", "param2"],
  "nextStep": "next_step",
  "visualize": true,
  "metadata": {
    "custom_field": "value",
    "operation_type": "custom",
    "importance": "medium"
  }
}
```

## 🔧 Функции

### Определение функции
```json
{
  "functions": [
    {
      "name": "partition",
      "description": "Разделение массива для быстрой сортировки",
      "parameters": ["low", "high"],
      "entryPoint": "choose_pivot",
      "steps": [
        {
          "id": "choose_pivot",
          "type": "assign",
          "description": "Выбор опорного элемента",
          "parameters": ["pivot", "array[high]"],
          "nextStep": "init_i",
          "visualize": true,
          "highlightElements": ["high"],
          "highlightColor": "orange"
        },
        {
          "id": "init_i",
          "type": "assign",
          "description": "Инициализация указателя",
          "parameters": ["i", "low - 1"],
          "nextStep": "start_loop",
          "visualize": false
        }
      ]
    }
  ]
}
```

### Вызов функции
```json
{
  "id": "call_partition",
  "type": "call_function",
  "functionName": "partition", 
  "functionParameters": {
    "low": "0",
    "high": "array_length - 1"
  },
  "returnToStep": "after_partition",
  "description": "Вызов функции разделения массива",
  "visualize": true,
  "metadata": {
    "algorithm_phase": "divide"
  }
}
```

### Рекурсивные функции
```json
{
  "functions": [
    {
      "name": "factorial",
      "description": "Вычисление факториала числа",
      "parameters": ["n"],
      "entryPoint": "check_base_case",
      "steps": [
        {
          "id": "check_base_case",
          "type": "condition",
          "description": "Проверка базового случая",
          "parameters": ["n <= 1"],
          "conditionCases": [
            {
              "condition": "true",
              "nextStep": "return_one"
            },
            {
              "condition": "false",
              "nextStep": "recursive_call"
            }
          ]
        },
        {
          "id": "return_one",
          "type": "assign",
          "description": "Базовый случай - возврат 1",
          "parameters": ["result", "1"],
          "nextStep": "return_from_factorial"
        },
        {
          "id": "recursive_call",
          "type": "call_function",
          "functionName": "factorial",
          "functionParameters": {
            "n": "n - 1"
          },
          "returnToStep": "compute_result",
          "description": "Рекурсивный вызов"
        }
      ]
    }
  ]
}
```

## 🎨 Визуализация

### Настройки визуализации шага
```json
{
  "id": "visualization_example",
  "type": "compare",
  "description": "Сравнение с расширенной визуализацией",
  "parameters": ["i", "j"],
  "visualize": true,
  "highlightElements": ["i", "j", "pivot"],
  "highlightColor": "yellow",
  "visualizationType": "comparison",
  "metadata": {
    "animation": "highlight",
    "duration": 1000,
    "sound": "compare",
    "display_mode": "side_by_side",
    "show_values": true,
    "show_indices": true
  }
}
```

### Цветовая палитра для подсветки
```json
{
  "colors": [
    {"name": "red", "usage": "Обмены, ошибки, важные операции"},
    {"name": "blue", "usage": "Текущий элемент, указатели"},
    {"name": "green", "usage": "Отсортированные элементы, успешные операции"},
    {"name": "yellow", "usage": "Сравнения, активные элементы"},
    {"name": "orange", "usage": "Опорные элементы, ключевые точки"},
    {"name": "purple", "usage": "Рекурсивные вызовы, специальные состояния"},
    {"name": "lightblue", "usage": "Вспомогательные элементы, диапазоны"},
    {"name": "lightgreen", "usage": "Промежуточные состояния"},
    {"name": "#FF5733", "usage": "Пользовательские цвета (HEX)"},
    {"name": "bg-red-500", "usage": "Tailwind классы для веб-интерфейса"}
  ]
}
```

### Типы визуализации
```json
{
  "visualization_types": [
    {"type": "default", "description": "Стандартная подсветка элементов"},
    {"type": "comparison", "description": "Визуализация сравнения двух элементов"},
    {"type": "swap", "description": "Анимация обмена элементов"},
    {"type": "traversal", "description": "Визуализация обхода структуры данных"},
    {"type": "recursion", "description": "Отображение рекурсивных вызовов"},
    {"type": "insertion", "description": "Вставка элементов"},
    {"type": "deletion", "description": "Удаление элементов"},
    {"type": "custom", "description": "Пользовательская визуализация"}
  ]
}
```

### Метаданные для расширенной визуализации
```json
{
  "metadata_examples": [
    {
      "purpose": "Анимации",
      "fields": {
        "animation": "type_of_animation",
        "duration": 1000,
        "easing": "ease-in-out"
      }
    },
    {
      "purpose": "Звуковые эффекты",
      "fields": {
        "sound": "sound_effect_name",
        "volume": 0.7
      }
    },
    {
      "purpose": "Отображение данных",
      "fields": {
        "show_values": true,
        "show_indices": true,
        "display_mode": "compact"
      }
    },
    {
      "purpose": "Статистика",
      "fields": {
        "operation_count": "count_value",
        "time_complexity": "O(n^2)"
      }
    }
  ]
}
```

## 🏗️ Структуры данных

### Массивы (Array)
```json
{
  "structureType": "array",
  "variables": [
    {
      "name": "arr",
      "type": "array",
      "initialValue": [64, 34, 25, 12, 22, 11, 90],
      "description": "Основной массив для сортировки"
    }
  ],
  "steps": [
    {
      "id": "access_element",
      "type": "assign",
      "description": "Доступ к элементу массива",
      "parameters": ["temp", "arr[i]"],
      "visualize": true,
      "highlightElements": ["i"]
    }
  ]
}
```

### Бинарные деревья (Binary Tree)
```json
{
  "structureType": "binarytree",
  "variables": [
    {
      "name": "stack",
      "type": "array", 
      "initialValue": [],
      "description": "Стек для обхода дерева"
    },
    {
      "name": "current",
      "type": "object",
      "initialValue": "root",
      "description": "Текущий узел"
    }
  ],
  "steps": [
    {
      "id": "traverse_node",
      "type": "generic",
      "description": "Обработка узла дерева",
      "parameters": ["process_node(current)"],
      "visualize": true,
      "highlightElements": ["current"],
      "metadata": {
        "tree_operation": "node_visit"
      }
    }
  ]
}
```

### Графы (Graph)
```json
{
  "structureType": "graph", 
  "variables": [
    {
      "name": "visited",
      "type": "array",
      "initialValue": [],
      "description": "Посещенные узлы"
    },
    {
      "name": "queue",
      "type": "array",
      "initialValue": [],
      "description": "Очередь для BFS"
    }
  ],
  "steps": [
    {
      "id": "visit_node",
      "type": "generic", 
      "description": "Посещение узла графа",
      "parameters": ["visited.push(current.id)"],
      "visualize": true,
      "highlightElements": ["current"],
      "connections": [
        {
          "from": "previous",
          "to": "current", 
          "type": "traversal"
        }
      ]
    }
  ]
}
```

### Связные списки (Linked List)
```json
{
  "structureType": "linkedlist",
  "variables": [
    {
      "name": "current",
      "type": "object", 
      "initialValue": "head",
      "description": "Текущий узел списка"
    }
  ],
  "steps": [
    {
      "id": "traverse_list",
      "type": "assign",
      "description": "Переход к следующему узлу",
      "parameters": ["current", "current.next"],
      "visualize": true,
      "highlightElements": ["current"],
      "connections": [
        {
          "from": "previous",
          "to": "current",
          "type": "next_pointer"
        }
      ]
    }
  ]
}
```

## 📋 Примеры алгоритмов

### 1. Сортировка пузырьком (Bubble Sort)
```json
{
  "name": "BubbleSort",
  "description": "Сортировка пузырьком с визуализацией каждого шага",
  "structureType": "array",
  "variables": [
    {
      "name": "i",
      "type": "int", 
      "initialValue": 0
    },
    {
      "name": "j",
      "type": "int",
      "initialValue": 0
    },
    {
      "name": "swapped",
      "type": "bool",
      "initialValue": false
    },
    {
      "name": "temp",
      "type": "int",
      "initialValue": 0
    }
  ],
  "steps": [
    {
      "id": "start",
      "type": "assign",
      "description": "Инициализация внешнего цикла",
      "parameters": ["i", "0"],
      "nextStep": "outer_loop_check",
      "visualize": false
    },
    {
      "id": "outer_loop_check", 
      "type": "condition",
      "description": "Проверка внешнего цикла",
      "parameters": ["i < array_length - 1"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "init_inner_loop"
        },
        {
          "condition": "false", 
          "nextStep": "end"
        }
      ],
      "visualize": true,
      "highlightElements": ["i"],
      "highlightColor": "blue"
    },
    {
      "id": "init_inner_loop",
      "type": "assign",
      "description": "Инициализация внутреннего цикла", 
      "parameters": ["j", "0"],
      "nextStep": "inner_loop_check",
      "visualize": false
    },
    {
      "id": "inner_loop_check",
      "type": "condition",
      "description": "Проверка внутреннего цикла",
      "parameters": ["j < array_length - i - 1"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "compare_elements"
        },
        {
          "condition": "false",
          "nextStep": "check_swapped" 
        }
      ],
      "visualize": true
    },
    {
      "id": "compare_elements",
      "type": "compare",
      "description": "Сравнение соседних элементов",
      "parameters": ["j", "j + 1"],
      "nextStep": "check_swap",
      "visualize": true,
      "highlightElements": ["j", "j + 1"],
      "highlightColor": "yellow"
    },
    {
      "id": "check_swap",
      "type": "condition", 
      "description": "Проверка необходимости обмена",
      "parameters": ["array[j] > array[j + 1]"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "perform_swap"
        },
        {
          "condition": "false",
          "nextStep": "increment_j"
        }
      ],
      "visualize": true
    },
    {
      "id": "perform_swap",
      "type": "swap",
      "description": "Обмен элементов местами",
      "parameters": ["j", "j + 1"],
      "nextStep": "mark_swapped",
      "visualize": true,
      "highlightElements": ["j", "j + 1"],
      "highlightColor": "red"
    },
    {
      "id": "mark_swapped",
      "type": "assign",
      "description": "Отметка выполнения обмена",
      "parameters": ["swapped", "true"],
      "nextStep": "increment_j",
      "visualize": false
    },
    {
      "id": "increment_j",
      "type": "assign",
      "description": "Увеличение счетчика внутреннего цикла",
      "parameters": ["j", "j + 1"],
      "nextStep": "inner_loop_check",
      "visualize": false
    },
    {
      "id": "check_swapped",
      "type": "condition",
      "description": "Проверка были ли обмены",
      "parameters": ["swapped == true"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "reset_swapped"
        },
        {
          "condition": "false",
          "nextStep": "end"
        }
      ],
      "visualize": true
    },
    {
      "id": "reset_swapped",
      "type": "assign",
      "description": "Сброс флага обменов", 
      "parameters": ["swapped", "false"],
      "nextStep": "increment_i",
      "visualize": false
    },
    {
      "id": "increment_i",
      "type": "assign",
      "description": "Увеличение счетчика внешнего цикла",
      "parameters": ["i", "i + 1"],
      "nextStep": "outer_loop_check",
      "visualize": false
    },
    {
      "id": "end",
      "type": "generic",
      "description": "Сортировка завершена",
      "parameters": [],
      "visualize": true,
      "highlightElements": ["all"],
      "highlightColor": "green"
    }
  ]
}
```

### 2. Поиск в глубину (DFS) для графов
```json
{
  "name": "GraphDFS",
  "description": "Поиск в глубину с визуализацией обхода",
  "structureType": "graph",
  "variables": [
    {
      "name": "stack",
      "type": "array",
      "initialValue": [],
      "description": "Стек для DFS"
    },
    {
      "name": "visited",
      "type": "array", 
      "initialValue": [],
      "description": "Посещенные узлы"
    },
    {
      "name": "current",
      "type": "object",
      "initialValue": "null",
      "description": "Текущий узел"
    }
  ],
  "steps": [
    {
      "id": "start",
      "type": "assign",
      "description": "Инициализация стека начальным узлом",
      "parameters": ["stack", "[start_node]"],
      "nextStep": "dfs_loop",
      "visualize": true,
      "highlightElements": ["start_node"],
      "highlightColor": "blue"
    },
    {
      "id": "dfs_loop",
      "type": "condition",
      "description": "Проверка пустоты стека",
      "parameters": ["stack.length > 0"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "pop_node"
        },
        {
          "condition": "false",
          "nextStep": "end"
        }
      ],
      "visualize": true
    },
    {
      "id": "pop_node",
      "type": "assign",
      "description": "Извлечение узла из стека",
      "parameters": ["current", "stack.pop()"],
      "nextStep": "check_visited",
      "visualize": true,
      "highlightElements": ["current"],
      "highlightColor": "orange"
    },
    {
      "id": "check_visited",
      "type": "condition",
      "description": "Проверка посещения узла",
      "parameters": ["visited.includes(current.id)"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "dfs_loop"
        },
        {
          "condition": "false",
          "nextStep": "mark_visited"
        }
      ],
      "visualize": true
    },
    {
      "id": "mark_visited",
      "type": "generic",
      "description": "Помечаем узел как посещенный",
      "parameters": ["visited.push(current.id)"],
      "nextStep": "process_node",
      "visualize": true,
      "highlightElements": ["current"],
      "highlightColor": "green"
    },
    {
      "id": "process_node",
      "type": "generic",
      "description": "Обработка текущего узла",
      "parameters": [],
      "nextStep": "get_neighbors",
      "visualize": true,
      "metadata": {
        "operation": "visit_node",
        "node_value": "current.value"
      }
    },
    {
      "id": "get_neighbors",
      "type": "assign",
      "description": "Получение соседей текущего узла",
      "parameters": ["neighbors", "graph.getNeighbors(current.id)"],
      "nextStep": "process_neighbors",
      "visualize": true
    },
    {
      "id": "process_neighbors",
      "type": "condition",
      "description": "Обработка всех соседей",
      "parameters": ["neighbors.length > 0"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "process_next_neighbor"
        },
        {
          "condition": "false",
          "nextStep": "dfs_loop"
        }
      ],
      "visualize": true
    },
    {
      "id": "process_next_neighbor",
      "type": "assign",
      "description": "Обработка следующего соседа",
      "parameters": ["neighbor", "neighbors.shift()"],
      "nextStep": "check_neighbor_visited",
      "visualize": true,
      "highlightElements": ["neighbor"],
      "highlightColor": "yellow"
    },
    {
      "id": "check_neighbor_visited",
      "type": "condition",
      "description": "Проверка посещения соседа",
      "parameters": ["!visited.includes(neighbor.id)"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "push_neighbor"
        },
        {
          "condition": "false",
          "nextStep": "process_neighbors"
        }
      ],
      "visualize": true
    },
    {
      "id": "push_neighbor",
      "type": "generic",
      "description": "Добавление непосещенного соседа в стек",
      "parameters": ["stack.push(neighbor)"],
      "nextStep": "process_neighbors",
      "visualize": true,
      "highlightElements": ["neighbor"],
      "highlightColor": "lightblue"
    },
    {
      "id": "end",
      "type": "generic",
      "description": "DFS завершен",
      "parameters": [],
      "visualize": true,
      "highlightElements": ["all_visited"],
      "highlightColor": "purple"
    }
  ]
}
```

### 3. Строковый алгоритм - Поиск подстроки
```json
{
  "name": "StringSearch", 
  "description": "Алгоритм поиска подстроки в строке",
  "structureType": "array",
  "variables": [
    {
      "name": "text",
      "type": "string",
      "initialValue": "Hello World, welcome to algorithm visualization!"
    },
    {
      "name": "pattern", 
      "type": "string",
      "initialValue": "algorithm"
    },
    {
      "name": "i",
      "type": "int",
      "initialValue": 0
    },
    {
      "name": "j", 
      "type": "int",
      "initialValue": 0
    },
    {
      "name": "found",
      "type": "bool",
      "initialValue": false
    },
    {
      "name": "position",
      "type": "int", 
      "initialValue": -1
    }
  ],
  "steps": [
    {
      "id": "start",
      "type": "assign",
      "description": "Инициализация переменных поиска",
      "parameters": ["i", "0"],
      "nextStep": "search_loop",
      "visualize": true
    },
    {
      "id": "search_loop",
      "type": "condition",
      "description": "Проверка условий продолжения поиска",
      "parameters": ["i <= length(text) - length(pattern) && !found"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "init_pattern_check"
        },
        {
          "condition": "false",
          "nextStep": "end_search"
        }
      ],
      "visualize": true
    },
    {
      "id": "init_pattern_check",
      "type": "assign",
      "description": "Начало проверки совпадения pattern",
      "parameters": ["j", "0"],
      "nextStep": "pattern_loop",
      "visualize": false
    },
    {
      "id": "pattern_loop",
      "type": "condition",
      "description": "Проверка совпадения символов",
      "parameters": ["j < length(pattern) && substring(text, i + j, 1) == substring(pattern, j, 1)"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "increment_j"
        },
        {
          "condition": "false",
          "nextStep": "check_full_match"
        }
      ],
      "visualize": true,
      "highlightElements": ["i", "j"],
      "highlightColor": "yellow"
    },
    {
      "id": "increment_j",
      "type": "assign",
      "description": "Переход к следующему символу pattern",
      "parameters": ["j", "j + 1"],
      "nextStep": "pattern_loop",
      "visualize": false
    },
    {
      "id": "check_full_match",
      "type": "condition",
      "description": "Проверка полного совпадения pattern",
      "parameters": ["j == length(pattern)"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "found_match"
        },
        {
          "condition": "false",
          "nextStep": "increment_i"
        }
      ],
      "visualize": true
    },
    {
      "id": "found_match",
      "type": "assign",
      "description": "Подстрока найдена",
      "parameters": ["found", "true"],
      "nextStep": "set_position",
      "visualize": true,
      "highlightElements": ["i"],
      "highlightColor": "green"
    },
    {
      "id": "set_position",
      "type": "assign",
      "description": "Запись позиции найденной подстроки",
      "parameters": ["position", "i"],
      "nextStep": "end_search",
      "visualize": true
    },
    {
      "id": "increment_i",
      "type": "assign",
      "description": "Переход к следующей позиции в text",
      "parameters": ["i", "i + 1"],
      "nextStep": "search_loop",
      "visualize": false
    },
    {
      "id": "end_search",
      "type": "generic",
      "description": "Поиск завершен",
      "parameters": [],
      "visualize": true,
      "metadata": {
        "result_found": "found",
        "position": "position",
        "search_result": "found ? 'Найдено в позиции ' + position : 'Не найдено'"
      }
    }
  ]
}
```

## 💡 Лучшие практики

### 1. Организация кода и именования
```json
{
  "naming_conventions": [
    {"rule": "id шагов", "example": "тип_действие_объект", "description": "sort_compare_elements, tree_visit_node"},
    {"rule": "переменные", "example": "snake_case", "description": "current_node, temp_value, is_sorted"},
    {"rule": "функции", "example": "camelCase", "description": "partitionArray, traverseTree"},
    {"rule": "группировка", "example": "префиксы", "description": "sort_init, sort_compare, sort_swap"}
  ]
}
```

### 2. Эффективная визуализация
```json
{
  "visualization_guidelines": [
    {"rule": "Подсвечивайте ключевые элементы", "example": "только изменяемые элементы на шаге"},
    {"rule": "Используйте семантические цвета", "example": "красный для обменов, желтый для сравнений"},
    {"rule": "Отключайте визуализацию служебных шагов", "example": "visualize: false для инкрементов счетчиков"},
    {"rule": "Добавляйте метаданные для сложных анимаций", "example": "длительность, тип анимации"}
  ]
}
```

### 3. Производительность и ограничения
```json
{
  "performance_rules": [
    {"limit": "Максимум шагов", "value": 10000, "description": "Защита от бесконечных циклов"},
    {"limit": "Глубина рекурсии", "value": 100, "description": "Максимальная глубина вызовов функций"},
    {"limit": "Размер массивов", "value": 1000, "description": "Для эффективной визуализации"},
    {"limit": "Длина строк", "value": 1000, "description": "Разумные ограничения для отображения"}
  ]
}
```

### 4. Отладка и тестирование
```json
{
  "debugging_tips": [
    {"tip": "Используйте простые тестовые данные", "example": "массивы из 5-10 элементов"},
    {"tip": "Проверяйте выражения в изоляции", "example": "тестируйте сложные выражения отдельно"},
    {"tip": "Добавляйте описательные message", "example": "шаги с ясными описаниями на русском"},
    {"tip": "Используйте metadata для отладочной информации", "example": "значения переменных, промежуточные результаты"}
  ]
}
```

## 🚀 Быстрый старт

### Шаблон для начала работы
```json
{
  "name": "YourAlgorithmName",
  "description": "Описание вашего алгоритма",
  "structureType": "array",
  "variables": [
    {
      "name": "i",
      "type": "int",
      "initialValue": 0
    },
    {
      "name": "result",
      "type": "int", 
      "initialValue": 0
    }
  ],
  "steps": [
    {
      "id": "start",
      "type": "assign",
      "description": "Начало алгоритма",
      "parameters": ["i", "0"],
      "nextStep": "main_loop",
      "visualize": true
    },
    {
      "id": "main_loop",
      "type": "condition",
      "description": "Основной цикл алгоритма",
      "parameters": ["i < 10"],
      "conditionCases": [
        {
          "condition": "true",
          "nextStep": "process_element"
        },
        {
          "condition": "false",
          "nextStep": "end"
        }
      ],
      "visualize": true
    },
    {
      "id": "process_element",
      "type": "generic",
      "description": "Обработка элемента",
      "parameters": ["result", "result + i"],
      "nextStep": "increment",
      "visualize": true,
      "highlightElements": ["i"],
      "highlightColor": "blue"
    },
    {
      "id": "increment",
      "type": "assign",
      "description": "Увеличение счетчика",
      "parameters": ["i", "i + 1"],
      "nextStep": "main_loop",
      "visualize": false
    },
    {
      "id": "end",
      "type": "generic",
      "description": "Алгоритм завершен",
      "parameters": [],
      "visualize": true,
      "metadata": {
        "final_result": "result"
      }
    }
  ]
}
```

## 📞 Поддержка и ресурсы

### Полезные шаблоны
- **Сортировки**: Bubble Sort, Quick Sort, Merge Sort
- **Поиск**: Linear Search, Binary Search, String Search  
- **Обходы**: Tree DFS/BFS, Graph DFS/BFS
- **Строковые алгоритмы**: Pattern Matching, String Transformation

### Отладка выражений
```json
{
  "debug_expression": {
    "expression": "your_complex_expression",
    "variables": {
      "var1": "value1",
      "var2": "value2"
    },
    "expected_result": "expected_value"
  }
}
```

**ЯВА** предоставляет мощный и гибкий способ описания алгоритмов с богатой визуализацией. Начните с простых примеров и постепенно переходите к сложным алгоритмам! 🚀

# AlgoVis
