"""
Тестирование транслятора с учётом безопасности
"""

import sys
import os

# Добавляем родительскую директорию в путь для импорта
sys.path.insert(0, os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

from python_java_translator import JavaTranslator, TranslationConfig, ErrorHandler

def run_security_tests():
    """Запуск тестов безопасности"""
    
    print("Тесты безопасности транслятора")
    print("=" * 60)
    
    # Конфигурация с максимальной безопасностью
    secure_config = TranslationConfig(
        max_code_size=1000,
        max_steps=100,
        max_recursion_depth=10,
        safe_mode=True,
        validate_output=True
    )
    
    translator = JavaTranslator(translation_config=secure_config)
    
    tests = [
        # (код, ожидаемый результат, описание)
        ("x = 1", "SUCCESS", "Простейший корректный код"),
        ("__import__('os').system('ls')", "SECURITY_ERROR", "Попытка импорта"),
        ("eval('1+1')", "SECURITY_ERROR", "Использование eval"),
        ("x = " + "a" * 2000, "SIZE_LIMIT_ERROR", "Превышение размера кода"),
        ("def recursive():\n    recursive()", "RECURSION_ERROR", "Глубокая рекурсия"),
        ("if True:\n    if True:\n        if True:\n            x=1", "SUCCESS", "Вложенные условия"),
        ("for i in range(1000):\n    x=i", "SIZE_LIMIT_ERROR", "Много шагов"),
        ("import sys\nsys.exit()", "SECURITY_ERROR", "Импорт системных модулей"),
        ("x = open('/etc/passwd')", "SECURITY_ERROR", "Открытие файлов"),
        ("exec('print(1)')", "SECURITY_ERROR", "Использование exec"),
    ]
    
    passed = 0
    failed = 0
    
    for code, expected, description in tests:
        print(f"\nТест: {description}")
        print(f"Код: {code[:50]}{'...' if len(code) > 50 else ''}")
        
        result = translator.translate_python_code(code, "Тест")
        
        if result.get("error"):
            actual = result["errorType"]
        else:
            actual = "SUCCESS"
        
        if actual == expected:
            print(f"✓ Пройден ({actual})")
            passed += 1
        else:
            print(f"✗ Провален: ожидалось {expected}, получено {actual}")
            if result.get("errorMessage"):
                print(f"  Сообщение: {result['errorMessage']}")
            failed += 1
    
    print(f"\n\nИтог: {passed} пройдено, {failed} провалено")
    
    return passed, failed

def run_validation_tests():
    """Тестирование валидации"""
    
    print("\n\nТесты валидации ЯВА алгоритмов")
    print("=" * 60)
    
    from python_java_translator.error_handler import ErrorHandler
    
    test_algorithms = [
        {
            "name": "Корректный",
            "data": {
                "name": "Test",
                "description": "Test",
                "structureType": "array",
                "variables": [{"name": "x", "type": "int", "initialValue": 0}],
                "steps": [
                    {"id": "start", "type": "generic", "description": "Start", 
                     "parameters": [], "nextStep": "end", "visualize": True},
                    {"id": "end", "type": "generic", "description": "End", 
                     "parameters": [], "visualize": True}
                ]
            },
            "should_pass": True
        },
        {
            "name": "Без start",
            "data": {
                "name": "Test",
                "description": "Test",
                "structureType": "array",
                "variables": [],
                "steps": [
                    {"id": "middle", "type": "generic", "description": "Middle", 
                     "parameters": []},
                    {"id": "end", "type": "generic", "description": "End", 
                     "parameters": []}
                ]
            },
            "should_pass": False
        },
        {
            "name": "Start не generic",
            "data": {
                "name": "Test",
                "description": "Test",
                "structureType": "array",
                "variables": [],
                "steps": [
                    {"id": "start", "type": "assign", "description": "Start", 
                     "parameters": ["x", "0"], "nextStep": "end", "visualize": True},
                    {"id": "end", "type": "generic", "description": "End", 
                     "parameters": [], "visualize": True}
                ]
            },
            "should_pass": False
        },
        {
            "name": "Дублирующиеся ID",
            "data": {
                "name": "Test",
                "description": "Test",
                "structureType": "array",
                "variables": [],
                "steps": [
                    {"id": "start", "type": "generic", "description": "Start", 
                     "parameters": [], "nextStep": "step1", "visualize": True},
                    {"id": "step1", "type": "generic", "description": "Step1", 
                     "parameters": [], "nextStep": "step1", "visualize": True},
                    {"id": "end", "type": "generic", "description": "End", 
                     "parameters": [], "visualize": True}
                ]
            },
            "should_pass": False
        }
    ]
    
    passed = 0
    failed = 0
    
    for test in test_algorithms:
        print(f"\nТест: {test['name']}")
        
        errors = ErrorHandler.validate_java_output(test['data'])
        has_errors = len(errors) > 0
        
        if has_errors != (not test['should_pass']):
            print(f"✓ Пройден")
            passed += 1
        else:
            print(f"✗ Провален")
            print(f"  Ошибки: {errors}")
            failed += 1
    
    print(f"\nИтог: {passed} пройдено, {failed} провалено")
    
    return passed, failed

def test_complex_algorithms():
    """Тестирование сложных алгоритмов"""
    
    print("\n\nТестирование сложных алгоритмов")
    print("=" * 60)
    
    translator = JavaTranslator()
    
    algorithms = [
        ("Сортировка пузырьком", '''
n = struct.len
for i in range(n):
    for j in range(n - i - 1):
        if struct[j] > struct[j + 1]:
            temp = struct[j]
            struct[j] = struct[j + 1]
            struct[j + 1] = temp
'''),
        ("Поиск в глубину", '''
stack = []
visited = []
stack.append(start_node)

while len(stack) > 0:
    current = stack.pop()
    if current not in visited:
        visited.append(current)
        for neighbor in current.neighbors:
            stack.append(neighbor)
'''),
        ("Бинарный поиск", '''
left = 0
right = struct.len - 1
found = False

while left <= right and not found:
    mid = (left + right) // 2
    if struct[mid] == target:
        found = True
    elif struct[mid] < target:
        left = mid + 1
    else:
        right = mid - 1
'''),
        ("Фибоначчи", '''
if n <= 1:
    return n
else:
    return fibonacci(n-1) + fibonacci(n-2)
''')
    ]
    
    for name, code in algorithms:
        print(f"\nАлгоритм: {name}")
        
        try:
            result = translator.translate_python_code(code, name)
            
            if result.get("error"):
                print(f"  Ошибка: {result['errorType']}")
            else:
                print(f"  Шагов: {len(result['steps'])}")
                print(f"  Переменных: {len(result['variables'])}")
                
                # Проверяем валидацию
                errors = ErrorHandler.validate_java_output(result)
                if errors:
                    print(f"  Валидация: {len(errors)} ошибок")
                    for err in errors[:2]:
                        print(f"    - {err}")
                else:
                    print(f"  Валидация: OK")
        
        except Exception as e:
            print(f"  Исключение: {e}")

def main():
    """Основная функция тестирования"""
    
    print("Полное тестирование транслятора Python -> ЯВА")
    print("=" * 60)
    
    # Запуск тестов безопасности
    security_passed, security_failed = run_security_tests()
    
    # Запуск тестов валидации
    validation_passed, validation_failed = run_validation_tests()
    
    # Тестирование сложных алгоритмов
    test_complex_algorithms()
    
    print("\n" + "=" * 60)
    print("Итоги тестирования:")
    print(f"Безопасность: {security_passed} пройдено, {security_failed} провалено")
    print(f"Валидация: {validation_passed} пройдено, {validation_failed} провалено")
    
    if security_failed == 0 and validation_failed == 0:
        print("\n✅ Все тесты пройдены успешно!")
    else:
        print("\n⚠️  Некоторые тесты провалены")

if __name__ == "__main__":
    main()