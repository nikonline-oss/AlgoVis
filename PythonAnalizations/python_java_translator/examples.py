"""
Примеры использования транслятора Python -> ЯВА
"""

import sys
import os

# Добавляем родительскую директорию в путь для импорта
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from python_java_translator.visualization_config import VisualizationConfig, create_preset_configs
from python_java_translator.translation_config import TranslationConfig
from python_java_translator.error_handler import ErrorType
from python_java_translator.java_translator import JavaTranslator

def test_safe_translation():
    """Тестирование безопасной трансляции с обработкой ошибок"""
    
    print("Тестирование безопасной трансляции")
    print("=" * 60)
    
    # Конфигурация с ограничениями
    safe_config = TranslationConfig(
        max_code_size=1000,
        max_steps=100,
        max_recursion_depth=10,
        safe_mode=True
    )
    
    translator = JavaTranslator(translation_config=safe_config)
    
    # 1. Корректный код
    print("\n1. Корректный код:")
    good_code = '''
sum = 0
for i in range(5):
    sum = sum + i
result = sum
'''
    result = translator.translate_python_code(good_code, "Сумма")
    if result.get("error"):
        print(f"   Ошибка: {result['errorMessage']}")
    else:
        print(f"   Успешно! Шагов: {len(result['steps'])}")
    
    # 2. Опасный код
    print("\n2. Опасный код:")
    dangerous_code = '''
import os
os.system("rm -rf /")
'''
    result = translator.translate_python_code(dangerous_code, "Опасный")
    if result.get("error"):
        print(f"   Обнаружена угроза: {result['errorType']}")
    else:
        print("   Пропущена опасность!")
    
    # 3. Слишком большой код
    print("\n3. Слишком большой код:")
    big_code = "x = 0\n" * 1000
    result = translator.translate_python_code(big_code, "Большой")
    if result.get("error"):
        print(f"   Превышен лимит: {result['errorMessage']}")
    
    # 4. Синтаксическая ошибка
    print("\n4. Синтаксическая ошибка:")
    bad_syntax = '''
x = 
if x > 0
    print("ok")
'''
    result = translator.translate_python_code(bad_syntax, "Синтаксис")
    if result.get("error"):
        print(f"   Синтаксическая ошибка: {result['errorMessage']}")
    
    return result

def test_structure_types():
    """Тестирование разных типов структур"""
    
    print("\n\nТестирование типов структур")
    print("=" * 60)
    
    # Код для работы с массивом
    array_code = '''
for i in range(struct.len):
    struct[i] = struct[i] * 2
'''
    
    # Код для работы с деревом
    tree_code = '''
if struct.hasLeft:
    left_value = struct.left.value
if struct.hasRight:
    right_value = struct.right.value
'''
    
    translator = JavaTranslator()
    
    print("1. Массив:")
    result = translator.translate_python_code(array_code, "Обработка массива", "array")
    print(f"   Переменные: {[v['name'] for v in result['variables']]}")
    
    print("\n2. Бинарное дерево:")
    result = translator.translate_python_code(tree_code, "Обход дерева", "binarytree")
    print(f"   Переменные: {[v['name'] for v in result['variables']]}")
    
    print("\n3. Неподдерживаемая структура:")
    try:
        result = translator.translate_python_code("x=1", "Тест", "unknown")
    except Exception as e:
        print(f"   Ошибка: {e}")

def test_custom_mods():
    """Тестирование пользовательских модов"""
    
    print("\n\nТестирование пользовательских модов")
    print("=" * 60)
    
    translator = JavaTranslator()
    
    # Добавляем пользовательскую функцию
    def custom_sum_handler(*args):
        expr = " + ".join(args)
        return translator._create_expression_result(f"custom_sum({expr})")
    
    translator.add_custom_mod("custom", {
        "custom_sum": custom_sum_handler,
        "average": lambda *args: translator._create_expression_result(f"average({', '.join(args)})")
    })
    
    # Код с пользовательской функцией
    code = '''
result = custom_sum(1, 2, 3, 4, 5)
avg = average(10, 20, 30)
'''
    
    result = translator.translate_python_code(code, "Пользовательские функции")
    
    # Ищем вызовы custom функций
    custom_calls = [s for s in result['steps'] if "custom" in str(s.get('parameters'))]
    print(f"   Найдено вызовов custom функций: {len(custom_calls)}")
    
    return result

def test_validation():
    """Тестирование валидации результата"""
    
    print("\n\nТестирование валидации результата")
    print("=" * 60)
    
    from python_java_translator.error_handler import ErrorHandler
    
    # 1. Корректный алгоритм
    correct_algo = {
        "name": "Test",
        "description": "Test",
        "structureType": "array",
        "variables": [{"name": "x", "type": "int", "initialValue": 0}],
        "steps": [
            {"id": "start", "type": "generic", "description": "Start", "parameters": [], "nextStep": "end", "visualize": True},
            {"id": "end", "type": "generic", "description": "End", "parameters": [], "visualize": True}
        ]
    }
    
    errors = ErrorHandler.validate_java_output(correct_algo)
    print(f"1. Корректный алгоритм: {len(errors)} ошибок")
    if errors:
        for err in errors:
            print(f"   - {err}")
    
    # 2. Алгоритм без start
    no_start = {
        "name": "Test",
        "description": "Test",
        "structureType": "array",
        "variables": [],
        "steps": [
            {"id": "middle", "type": "generic", "description": "Middle", "parameters": []},
            {"id": "end", "type": "generic", "description": "End", "parameters": []}
        ]
    }
    
    errors = ErrorHandler.validate_java_output(no_start)
    print(f"\n2. Алгоритм без start: {len(errors)} ошибок")
    for err in errors:
        print(f"   - {err}")
    
    # 3. Алгоритм с циклом без выхода
    infinite_loop = {
        "name": "Test",
        "description": "Test",
        "structureType": "array",
        "variables": [{"name": "x", "type": "int", "initialValue": 0}],
        "steps": [
            {"id": "start", "type": "generic", "description": "Start", "parameters": [], "nextStep": "loop", "visualize": True},
            {"id": "loop", "type": "condition", "description": "Loop", "parameters": ["true"], 
             "conditionCases": [{"condition": "true", "nextStep": "loop"}, {"condition": "false", "nextStep": "end"}]},
            {"id": "end", "type": "generic", "description": "End", "parameters": [], "visualize": True}
        ]
    }
    
    errors = ErrorHandler.validate_java_output(infinite_loop)
    print(f"\n3. Алгоритм с потенциальным бесконечным циклом: {len(errors)} ошибок")
    for err in errors:
        print(f"   - {err}")

def test_performance():
    """Тестирование производительности"""
    
    print("\n\nТестирование производительности")
    print("=" * 60)
    
    import time
    
    # Создаем большой код
    code_lines = []
    for i in range(100):
        code_lines.append(f"x{i} = {i}")
        code_lines.append(f"if x{i} % 2 == 0:")
        code_lines.append(f"    result = result + x{i}")
    
    big_code = "\n".join(code_lines)
    
    # Конфигурация с лимитами
    perf_config = TranslationConfig(
        max_code_size=5000,
        max_steps=500,
        max_recursion_depth=20
    )
    
    translator = JavaTranslator(translation_config=perf_config)
    
    start_time = time.time()
    result = translator.translate_python_code(big_code, "Производительность")
    elapsed = time.time() - start_time
    
    if result.get("error"):
        print(f"   Ошибка: {result['errorMessage']}")
    else:
        print(f"   Время трансляции: {elapsed:.2f} сек")
        print(f"   Количество шагов: {len(result['steps'])}")
        print(f"   Количество переменных: {len(result['variables'])}")

def main():
    """Основная функция для запуска примеров"""
    print("Система трансляции Python -> ЯВА с безопасностью и валидацией")
    print("=" * 60)
    
    # Тестируем безопасную трансляцию
    test_safe_translation()
    
    # Тестируем типы структур
    test_structure_types()
    
    # Тестируем пользовательские моды
    test_custom_mods()
    
    # Тестируем валидацию
    test_validation()
    
    # Тестируем производительность
    test_performance()
    
    print("\n" + "=" * 60)
    print("Пример использования в продакшене:")
    
    usage_example = '''
# Пример использования с безопасностью
from python_java_translator import JavaTranslator, TranslationConfig

# Конфигурация безопасности
config = TranslationConfig(
    max_code_size=5000,
    max_steps=1000,
    max_recursion_depth=30,
    safe_mode=True,
    validate_output=True
)

# Создание транслятора
translator = JavaTranslator(translation_config=config)

# Трансляция пользовательского кода
user_code = input("Введите алгоритм на Python: ")

try:
    result = translator.translate_python_code(
        user_code, 
        "Пользовательский алгоритм",
        structure_type="array"
    )
    
    if result.get("error"):
        print(f"Ошибка трансляции: {result['errorMessage']}")
    else:
        translator.save_to_file(result, "algorithm.java.json")
        print("Алгоритм успешно сгенерирован!")
        
except Exception as e:
    print(f"Критическая ошибка: {e}")
'''
    
    print(usage_example)

if __name__ == "__main__":
    main()