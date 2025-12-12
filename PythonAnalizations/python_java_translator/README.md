markdown
# Python → ЯВА Транслятор с Настройками Визуализации

Модуль для трансляции Python-кода в формат ЯВА с гибкими настройками визуализации операций.

## Структура проекта
python_java_translator/
├── init.py # Инициализация пакета
├── visualization_config.py # Конфигурация визуализации
├── data_structures.py # Структуры данных
├── variable_collector.py # Сборщик переменных
├── java_translator.py # Основной транслятор
├── examples.py # Примеры использования
└── README.md # Документация

text

## Установка

1. Убедитесь, что у вас установлен Python 3.7 или выше
2. Скопируйте все файлы в одну директорию
3. Используйте как модуль в вашем проекте

## Быстрый старт

# Python → ЯВА Транслятор с Безопасностью и Валидацией

Улучшенный модуль для трансляции Python-кода в формат ЯВА с:
- Настройками визуализации операций
- Проверкой безопасности
- Валидацией результата
- Защитой от бесконечных циклов

## 🚀 Быстрый старт

```python
from python_java_translator import JavaTranslator, TranslationConfig

# Конфигурация безопасности
config = TranslationConfig(
    max_code_size=5000,
    max_steps=1000,
    max_recursion_depth=30,
    safe_mode=True
)

# Создание транслятора
translator = JavaTranslator(translation_config=config)

# Трансляция кода
python_code = """
sum = 0
for i in range(10):
    sum = sum + i
result = sum
"""

result = translator.translate_python_code(
    python_code, 
    "Сумма чисел",
    structure_type="array"
)

# Проверка на ошибки
if result.get("error"):
    print(f"Ошибка: {result['errorMessage']}")
else:
    translator.save_to_file(result, "алгоритм.java.json")
    print("Алгоритм успешно сгенерирован!")

```python
from python_java_translator import JavaTranslator, VisualizationConfig

# Создаем кастомную конфигурацию
config = VisualizationConfig(
    visualize_assign=True,
    visualize_condition=True,
    visualize_generic=False,
    highlight_enabled=True,
    highlight_color_assign="blue"
)

# Создаем транслятор
translator = JavaTranslator(config)

# Транслируем Python код
python_code = """
sum = 0
for i in range(10):
    sum = sum + i
result = sum
"""

# Получаем результат в формате ЯВА
java_algorithm = translator.translate_python_code(python_code, "Пример алгоритма")

# Сохраняем в файл
translator.save_to_file(java_algorithm, "алгоритм.java.json")
Основные компоненты
1. VisualizationConfig
Класс для настройки визуализации операций:

python
config = VisualizationConfig(
    visualize_assign=True,          # Присваивания
    visualize_condition=True,       # Условия
    visualize_compare=True,         # Сравнения
    visualize_swap=True,            # Обмены
    visualize_call=True,            # Вызовы функций
    visualize_generic=True,         # Универсальные операции
    visualize_loop_init=False,      # Инициализация циклов
    visualize_loop_increment=False, # Инкременты в циклах
    visualize_start_end=True,       # Старт и конец
    highlight_enabled=True,         # Подсветка
    highlight_color_assign="blue"   # Цвет для присваиваний
)
2. JavaTranslator
Основной класс транслятора:

python
translator = JavaTranslator(config)
result = translator.translate_python_code(code, "Название алгоритма")
3. Предустановленные конфигурации
python
from python_java_translator import create_preset_configs

presets = create_preset_configs()

# Доступные пресеты:
# - "full" - полная визуализация
# - "minimal" - только ключевые шаги
# - "debug" - для отладки
# - "performance" - минимум визуализации
# - "educational" - для обучения

config = presets["educational"]
Примеры использования
Запуск демонстрации:
bash
python examples.py
Интерактивная настройка:
python
from python_java_translator import interactive_visualization_config

config, result = interactive_visualization_config()
Тестирование с пресетом:
python
from python_java_translator import test_with_preset

test_code = """
n = struct.len
for i in range(n):
    for j in range(n - i - 1):
        if struct[j] > struct[j + 1]:
            temp = struct[j]
            struct[j] = struct[j + 1]
            struct[j + 1] = temp
"""

result = test_with_preset("educational", test_code, "Сортировка пузырьком")
Особенности трансляции
Автоматическое определение типов переменных - система анализирует контекст использования переменных

Удаление вызовов str() - в ЯВА преобразование в строку происходит автоматически

Конвертация строковых литералов - из двойных кавычек в одинарные

Поддержка циклов - for (range), while

Модульная система - возможность добавления обработчиков функций

Формат вывода
Результат трансляции сохраняется в JSON формате, который включает:

Название алгоритма

Описание

Тип структуры данных

Список переменных с типами и начальными значениями

Последовательность шагов с настройками визуализации

Ограничения
Поддерживается ограниченное подмножество Python

Циклы for поддерживаются только для range()

Функции обрабатываются через систему модов

Типы переменных определяются эвристически

Расширение функциональности
Добавление нового обработчика функций:
python
translator = JavaTranslator()
translator.mods['new_mod'] = {
    'custom_func': lambda *args: translator._create_expression_result(f'custom({args})')
}
Изменение конфигурации визуализации:
python
translator.update_visualization_flags(
    visualize_assign=False,
    visualize_condition=True
)
Требования
Python 3.7+

Стандартные библиотеки: ast, json, re, dataclasses

Лицензия
Проект доступен для использования и модификации.

text

## Файл 8: `setup.py` (опционально)
```python
from setuptools import setup, find_packages

setup(
    name="python-java-translator",
    version="1.0.0",
    author="Ваше Имя",
    description="Модуль трансляции Python-кода в ЯВА с настройками визуализации",
    packages=find_packages(),
    python_requires=">=3.7",
    install_requires=[],
    classifiers=[
        "Programming Language :: Python :: 3",
        "License :: OSI Approved :: MIT License",
        "Operating System :: OS Independent",
    ],
)
Подробная инструкция по использованию:
1. Установка
Создайте папку python_java_translator

Поместите все 6 файлов в эту папку

Убедитесь, что Python 3.7 или выше установлен

2. Быстрый старт
Создайте файл test.py в той же директории:

python
from python_java_translator import JavaTranslator, VisualizationConfig

# Создаем конфигурацию
config = VisualizationConfig(
    visualize_assign=True,
    visualize_condition=True,
    visualize_generic=False,
    highlight_enabled=True
)

# Создаем транслятор
translator = JavaTranslator(config)

# Python код для трансляции
python_code = """
# Простой алгоритм суммирования
sum = 0
for i in range(1, 11):
    sum = sum + i
result = sum
print("Сумма: " + str(result))
"""

# Транслируем
result = translator.translate_python_code(python_code, "Сумма чисел от 1 до 10")

# Сохраняем результат
translator.save_to_file(result, "сумма.java.json")
print("Файл сохранен: сумма.java.json")
3. Запуск демонстрации
bash
python examples.py
4. Использование предустановленных конфигураций
python
from python_java_translator import JavaTranslator, create_preset_configs

# Получаем все пресеты
presets = create_preset_configs()

# Используем пресет для обучения
translator = JavaTranslator(presets["educational"])

# Или для отладки
translator = JavaTranslator(presets["debug"])
5. Интерактивная настройка
python
from python_java_translator import interactive_visualization_config

# Запускает интерактивный режим настройки
config, result = interactive_visualization_config()
6. Анализ результатов
python
# После трансляции можно анализировать результат
result = translator.translate_python_code(code, "Алгоритм")

print(f"Всего шагов: {len(result['steps'])}")
print(f"Переменные: {[v['name'] for v in result['variables']]}")

# Фильтрация визуализируемых шагов
visualized_steps = [s for s in result['steps'] if s.get('visualize', True)]
print(f"Визуализируется шагов: {len(visualized_steps)}")
7. Добавление своих обработчиков функций
python
translator = JavaTranslator()

# Добавляем новый мод
translator.mods['custom'] = {
    'my_function': lambda x, y: translator._create_expression_result(f'my_func({x}, {y})')
}

# Теперь вызовы my_function будут обрабатываться
Основные возможности:
Гибкая настройка визуализации - включайте/выключайте разные типы операций

Цветовая подсветка - разные типы операций выделяются разными цветами

Автоматическое определение типов - система сама определяет типы переменных

Предустановленные профили - готовые конфигурации для разных задач

Сохранение в JSON - совместимость с различными системами визуализации

Поддержка основных конструкций - циклы, условия, присваивания

Рекомендации по использованию:
Для отладки алгоритмов используйте пресет "debug"

Для обучения программированию - пресет "educational"

Для минимального отображения - пресет "minimal"

Для максимальной производительности - пресет "performance"