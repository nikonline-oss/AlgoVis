# Подробная инструкция по написанию алгоритмов для системы визуализации

## 1. Структура алгоритма

### Базовая структура JSON
```json
{
  "algorithm": {
    "name": "Название алгоритма",
    "description": "Описание алгоритма",
    "structureType": "array|binarytree|linkedlist|graph",
    "variables": [...],
    "functions": [...],
    "steps": [...]
  },
  "data": [...]  // Начальные данные
}
```

## 2. Переменные (Variables)

### Типы переменных:
- `int` - целые числа
- `double` - дробные числа  
- `bool` - логические значения
- `string` - строки
- `array` - массивы
- `object` - объекты

### Пример объявления переменных:
```json
"variables": [
  {
    "name": "i",
    "type": "int",
    "initialValue": "0"
  },
  {
    "name": "high",
    "type": "int", 
    "initialValue": "struct.len - 1"  // Выражения поддерживаются!
  },
  {
    "name": "stack",
    "type": "array",
    "initialValue": "[]",
    "arraySize": 100
  },
  {
    "name": "found",
    "type": "bool",
    "initialValue": "false"
  }
]
```

## 3. Функции (Functions)

### Структура функции:
```json
{
  "name": "имя_функции",
  "description": "Описание функции",
  "parameters": ["param1", "param2"],
  "entryPoint": "первый_шаг",
  "steps": [...]
}
```

## 4. Типы шагов (Steps)

### 4.1. assign - присваивание
```json
{
  "id": "уникальный_id",
  "type": "assign",
  "description": "Описание операции",
  "parameters": ["переменная", "выражение"],
  "nextStep": "следующий_шаг",
  "visualize": true
}
```

**Примеры:**
```json
{
  "id": "init_i",
  "type": "assign", 
  "description": "Инициализация счетчика",
  "parameters": ["i", "0"],
  "nextStep": "loop_start",
  "visualize": true
}
```

```json
{
  "id": "calculate_mid",
  "type": "assign",
  "description": "Вычисление середины",
  "parameters": ["mid", "(low + high) / 2"],
  "nextStep": "compare",
  "visualize": true
}
```

### 4.2. compare - сравнение
```json
{
  "id": "compare_step",
  "type": "compare",
  "description": "Сравнение элементов",
  "parameters": ["выражение1", "выражение2"],
  "nextStep": "check_result",
  "visualize": true,
  "highlightElements": ["index1", "index2"],
  "highlightColor": "yellow"
}
```

**Результат сохраняется в переменную `last_comparison`:**
- `< 0` - первый меньше второго
- `= 0` - равны  
- `> 0` - первый больше второго

### 4.3. swap - обмен элементов
```json
{
  "id": "swap_step", 
  "type": "swap",
  "description": "Обмен элементов",
  "parameters": ["struct", "index1", "index2"],
  "nextStep": "next_step",
  "visualize": true,
  "highlightElements": ["i", "j"],
  "highlightColor": "red"
}
```

### 4.4. condition - условный переход
```json
{
  "id": "condition_step",
  "type": "condition",
  "description": "Проверка условия",
  "parameters": ["условие"],
  "conditionCases": [
    {
      "condition": "true",
      "nextStep": "step_if_true"
    },
    {
      "condition": "false", 
      "nextStep": "step_if_false"
    }
  ],
  "visualize": true
}
```

### 4.5. call_function - вызов функции
```json
{
  "id": "call_func",
  "type": "call_function", 
  "description": "Вызов функции",
  "functionName": "имя_функции",
  "functionParameters": {
    "param1": "значение1",
    "param2": "значение2"
  },
  "returnToStep": "шаг_после_возврата",
  "visualize": true
}
```

### 4.6. return - возврат из функции
```json
{
  "id": "return_step",
  "type": "return",
  "description": "Возврат из функции",
  "visualize": false
}
```

### 4.7. generic - общая операция
```json
{
  "id": "end_step",
  "type": "generic",
  "operation": "complete|error",
  "description": "Завершение алгоритма",
  "visualize": true,
  "highlightColor": "green"
}
```

## 5. Выражения и операции

### Математические операции:
```javascript
"i + 1"
"(low + high) / 2" 
"struct.len - 1"
"stack_size * 2"
```

### Логические операции:
```javascript
"i < struct.len"
"!found"
"left <= right && array[left] <= pivot"
"stack_size > 0 || !finished"
```

### Функции:
```javascript
"length(text)"
"substring(text, start, length)"
"min(a, b)"
"max(a, b)"
"contains(array, value)"
```

### Доступ к элементам:
```javascript
"array[i]"           // Элемент массива
"struct.len"         // Свойство структуры
"stack[top]"         // Элемент массива-переменной
```

## 6. Специальные переменные

### struct - структура данных
```javascript
struct.len           // Длина массива
struct.length        // Альтернативное имя длины
struct.first         // Первый элемент
struct.last          // Последний элемент  
struct.isEmpty       // Пуст ли массив
```

### Системные переменные:
```javascript
last_comparison      // Результат последнего сравнения
```

## 7. Работа с массивами

### Инициализация:
```json
{
  "name": "stack",
  "type": "array", 
  "initialValue": "[]",
  "arraySize": 100
}
```

### Операции с массивами:
```json
{
  "type": "assign",
  "parameters": ["stack[i]", "value"]
}
```

```json
{
  "type": "assign", 
  "parameters": ["value", "stack[i]"]
}
```

## 8. Визуализация

### Настройки визуализации:
```json
"visualize": true,
"highlightElements": ["i", "j", "pivot"],
"highlightColor": "red|green|blue|yellow|purple",
"visualizationType": "comparison|swap|pivot_placement"
```

## 9. Практические примеры

### Простой цикл:
```json
{
  "id": "loop_start",
  "type": "condition",
  "description": "Проверка условия цикла",
  "parameters": ["i < struct.len"],
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
}
```

### Условная логика:
```json
{
  "id": "check_condition",
  "type": "condition", 
  "description": "Проверка найден ли элемент",
  "parameters": ["found"],
  "conditionCases": [
    {
      "condition": "true",
      "nextStep": "element_found"
    },
    {
      "condition": "false",
      "nextStep": "element_not_found"
    }
  ],
  "visualize": true
}
```

### Работа с функциями:
```json
{
  "id": "call_partition",
  "type": "call_function",
  "description": "Разделение массива",
  "functionName": "partition",
  "functionParameters": {
    "low_param": "low",
    "high_param": "high"
  },
  "returnToStep": "next_operation",
  "visualize": true
}
```

## 10. Советы по разработке

### Планирование:
1. **Разбейте алгоритм** на логические блоки
2. **Определите переменные** которые понадобятся
3. **Выделите повторяющиеся операции** в функции
4. **Продумайте визуализацию** ключевых шагов

### Отладка:
1. **Начинайте с простых случаев**
2. **Добавляйте подробные описания**
3. **Используйте console.log для отладки** выражений
4. **Тестируйте на разных наборах данных**

### Оптимизация:
1. **Используйте функции** для повторяющейся логики
2. **Минимизируйте количество шагов** где это возможно
3. **Используйте эффективные выражения**
4. **Учитывайте особенности структур данных**

## 11. Пример полного алгоритма (линейный поиск)

```json
{
  "algorithm": {
    "name": "LinearSearch",
    "description": "Линейный поиск элемента в массиве",
    "structureType": "array",
    "variables": [
      {
        "name": "i",
        "type": "int",
        "initialValue": "0"
      },
      {
        "name": "target",
        "type": "int", 
        "initialValue": "5"
      },
      {
        "name": "found",
        "type": "bool",
        "initialValue": "false"
      }
    ],
    "steps": [
      {
        "id": "start",
        "type": "condition",
        "description": "Проверка условий продолжения поиска",
        "parameters": ["i < struct.len && !found"],
        "conditionCases": [
          {
            "condition": "true",
            "nextStep": "compare_element"
          },
          {
            "condition": "false", 
            "nextStep": "check_result"
          }
        ],
        "visualize": true
      },
      {
        "id": "compare_element",
        "type": "compare",
        "description": "Сравнение текущего элемента с искомым",
        "parameters": ["struct[i]", "target"],
        "nextStep": "check_match",
        "visualize": true,
        "highlightElements": ["i"],
        "highlightColor": "yellow"
      },
      {
        "id": "check_match",
        "type": "condition",
        "description": "Проверка совпадения элементов",
        "parameters": ["last_comparison == 0"],
        "conditionCases": [
          {
            "condition": "true",
            "nextStep": "set_found"
          },
          {
            "condition": "false",
            "nextStep": "increment_i"
          }
        ],
        "visualize": true
      },
      {
        "id": "set_found",
        "type": "assign",
        "description": "Элемент найден",
        "parameters": ["found", "true"],
        "nextStep": "start",
        "visualize": true
      },
      {
        "id": "increment_i", 
        "type": "assign",
        "description": "Переход к следующему элементу",
        "parameters": ["i", "i + 1"],
        "nextStep": "start",
        "visualize": true
      },
      {
        "id": "check_result",
        "type": "condition",
        "description": "Проверка результата поиска",
        "parameters": ["found"],
        "conditionCases": [
          {
            "condition": "true",
            "nextStep": "found"
          },
          {
            "condition": "false",
            "nextStep": "not_found" 
          }
        ],
        "visualize": true
      },
      {
        "id": "found",
        "type": "generic",
        "operation": "complete",
        "description": "Элемент найден на позиции i",
        "visualize": true,
        "highlightColor": "green"
      },
      {
        "id": "not_found",
        "type": "generic", 
        "operation": "complete",
        "description": "Элемент не найден",
        "visualize": true,
        "highlightColor": "red"
      }
    ]
  },
  "data": [1, 3, 5, 7, 9, 2, 4, 6, 8]
}
```

Эта инструкция покрывает все основные аспекты создания алгоритмов для системы визуализации. Начните с простых алгоритмов и постепенно переходите к более сложным!
