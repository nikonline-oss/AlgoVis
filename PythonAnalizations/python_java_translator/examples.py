"""
Примеры использования транслятора Python -> ЯВА
"""

from .visualization_config import VisualizationConfig, create_preset_configs
from .java_translator import JavaTranslator

def test_visualization_configs():
    """Тестирование разных конфигураций визуализации"""
    
    python_code = '''
result = x + y

if condition:
    result = result * 2

for i in range(3):
    result = result + i

message = "Результат: " + str(result)
'''
    
    # 1. Конфигурация по умолчанию (все визуализируется)
    print("1. Конфигурация по умолчанию (все визуализируется):")
    config_default = VisualizationConfig()
    translator = JavaTranslator(config_default)
    result_default = translator.translate_python_code(python_code, "DefaultConfig")
    
    # Подсчитываем визуализируемые шаги
    visualized_steps = [s for s in result_default['steps'] if s.get('visualize', True)]
    print(f"   Всего шагов: {len(result_default['steps'])}")
    print(f"   Визуализируется: {len(visualized_steps)}")
    
    # 2. Только условия и вызовы
    print("\n2. Только условия и вызовы:")
    config_minimal = VisualizationConfig(
        visualize_assign=False,
        visualize_condition=True,
        visualize_generic=False,
        visualize_loop_init=False,
        visualize_loop_increment=False,
        visualize_start_end=True
    )
    translator = JavaTranslator(config_minimal)
    result_minimal = translator.translate_python_code(python_code, "MinimalConfig")
    
    visualized_steps = [s for s in result_minimal['steps'] if s.get('visualize', True)]
    print(f"   Всего шагов: {len(result_minimal['steps'])}")
    print(f"   Визуализируется: {len(visualized_steps)}")
    
    # 3. Только присваивания и конец
    print("\n3. Только присваивания и конец:")
    config_assign_only = VisualizationConfig(
        visualize_assign=True,
        visualize_condition=False,
        visualize_generic=False,
        visualize_loop_init=False,
        visualize_loop_increment=False,
        visualize_start_end=True
    )
    translator = JavaTranslator(config_assign_only)
    result_assign = translator.translate_python_code(python_code, "AssignOnly")
    
    visualized_steps = [s for s in result_assign['steps'] if s.get('visualize', True)]
    print(f"   Всего шагов: {len(result_assign['steps'])}")
    print(f"   Визуализируется: {len(visualized_steps)}")
    
    # 4. Визуализация с подсветкой разных цветов
    print("\n4. Визуализация с цветной подсветкой:")
    config_colored = VisualizationConfig(
        visualize_assign=True,
        visualize_condition=True,
        visualize_generic=True,
        highlight_enabled=True,
        highlight_color_assign="blue",
        highlight_color_condition="orange",
        highlight_color_swap="red",
        highlight_color_call="green"
    )
    translator = JavaTranslator(config_colored)
    result_colored = translator.translate_python_code(python_code, "Colored")
    
    # Проверяем цвета подсветки
    colored_steps = [s for s in result_colored['steps'] if s.get('highlightColor')]
    print(f"   Шагов с подсветкой: {len(colored_steps)}")
    
    return {
        "default": result_default,
        "minimal": result_minimal,
        "assign_only": result_assign,
        "colored": result_colored
    }


def interactive_visualization_config():
    """Интерактивная настройка визуализации"""
    
    print("Интерактивная настройка визуализации операций ЯВА")
    print("=" * 60)
    
    # Создаем конфигурацию по умолчанию
    config = VisualizationConfig()
    
    # Список опций для настройки
    options = [
        ("visualize_assign", "Визуализация присваиваний (x = y)", config.visualize_assign),
        ("visualize_condition", "Визуализация условий (if, while)", config.visualize_condition),
        ("visualize_compare", "Визуализация сравнений", config.visualize_compare),
        ("visualize_swap", "Визуализация обменов", config.visualize_swap),
        ("visualize_call", "Визуализация вызовов функций", config.visualize_call),
        ("visualize_generic", "Визуализация универсальных операций", config.visualize_generic),
        ("visualize_loop_init", "Визуализация инициализации циклов", config.visualize_loop_init),
        ("visualize_loop_increment", "Визуализация инкрементов в циклах", config.visualize_loop_increment),
        ("visualize_start_end", "Визуализация старта и конца алгоритма", config.visualize_start_end),
        ("highlight_enabled", "Включить подсветку элементов", config.highlight_enabled),
    ]
    
    # Отображаем текущие настройки
    print("Текущие настройки визуализации:")
    for i, (key, description, value) in enumerate(options, 1):
        status = "✓" if value else "✗"
        print(f"{i:2}. {status} {description}")
    
    # Пример кода для тестирования
    test_code = '''
# Пример алгоритма для тестирования
sum = 0
for i in range(5):
    if i % 2 == 0:
        sum = sum + i
result = sum
'''
    
    # Создаем транслятор с текущей конфигурацией
    translator = JavaTranslator(config)
    result = translator.translate_python_code(test_code, "InteractiveTest")
    
    # Анализируем результат
    print(f"\nПример алгоритма сгенерирован:")
    print(f"Всего шагов: {len(result['steps'])}")
    
    visualized_by_type = {}
    for step in result['steps']:
        step_type = step['type']
        if step.get('visualize', True):
            visualized_by_type[step_type] = visualized_by_type.get(step_type, 0) + 1
    
    print("Визуализированные шаги по типам:")
    for step_type, count in visualized_by_type.items():
        print(f"  - {step_type}: {count}")
    
    # Сохраняем результат
    translator.save_to_file(result, "interactive_visualization_test.java.json")
    print(f"\nРезультат сохранен в 'interactive_visualization_test.java.json'")
    
    return config, result


def test_with_preset(preset_name: str, python_code: str, algorithm_name: str = None):
    """Тестирование с предустановленной конфигурацией"""
    
    presets = create_preset_configs()
    
    if preset_name not in presets:
        print(f"Пресет '{preset_name}' не найден. Доступные пресеты: {list(presets.keys())}")
        return None
    
    config = presets[preset_name]
    algorithm_name = algorithm_name or f"{preset_name.capitalize()}Preset"
    
    print(f"\nТестирование пресета '{preset_name}':")
    print(f"Алгоритм: {algorithm_name}")
    
    translator = JavaTranslator(config)
    result = translator.translate_python_code(python_code, algorithm_name)
    
    # Анализ
    total_steps = len(result['steps'])
    visualized_steps = [s for s in result['steps'] if s.get('visualize', True)]
    visualization_percentage = (len(visualized_steps) / total_steps * 100) if total_steps > 0 else 0
    
    print(f"Всего шагов: {total_steps}")
    print(f"Визуализируется: {len(visualized_steps)} ({visualization_percentage:.1f}%)")
    
    # Сохраняем результат
    filename = f"{algorithm_name.lower().replace(' ', '_')}.java.json"
    translator.save_to_file(result, filename)
    print(f"Файл сохранен: {filename}")
    
    return result


if __name__ == "__main__":
    print("Система настройки визуализации операций ЯВА")
    print("=" * 60)
    
    # Тестируем разные конфигурации
    test_results = test_visualization_configs()
    
    print("\n" + "=" * 60)
    
    # Интерактивная настройка
    config, result = interactive_visualization_config()
    
    print("\n" + "=" * 60)
    print("Тестирование предустановленных конфигураций:")
    
    # Пример кода для тестирования пресетов
    test_algorithm = '''
# Алгоритм сортировки пузырьком (упрощенный)
n = struct.len
for i in range(n):
    for j in range(n - i - 1):
        if struct[j] > struct[j + 1]:
            # Обмен элементов
            temp = struct[j]
            struct[j] = struct[j + 1]
            struct[j + 1] = temp
'''
    
    # Тестируем все пресеты
    for preset_name in ["full", "minimal", "debug", "performance", "educational"]:
        test_with_preset(preset_name, test_algorithm)
    
    print("\n" + "=" * 60)
    print("Использование системы в коде:")
    
    usage_example = '''
# Пример использования в вашем коде
from python_java_translator import JavaTranslator, VisualizationConfig

# Создаем кастомную конфигурацию
config = VisualizationConfig(
    visualize_assign=True,
    visualize_condition=True,
    visualize_generic=False,
    visualize_loop_init=False,
    visualize_loop_increment=False,
    highlight_enabled=True,
    highlight_color_assign="blue"
)

# Создаем транслятор с этой конфигурацией
translator = JavaTranslator(config)

# Транслируем Python код
python_code = """
sum = 0
for i in range(10):
    sum = sum + i
result = sum
"""

java_algorithm = translator.translate_python_code(python_code, "CustomVisualization")
translator.save_to_file(java_algorithm, "custom_visualization.java.json")
'''
    
    print(usage_example)