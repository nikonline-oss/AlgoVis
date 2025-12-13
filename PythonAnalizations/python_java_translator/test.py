from python_java_translator.main import JAVATranslator
# Пример Python кода с сортировкой пузырьком
python_code = """
def bubble_sort():
    n = struct.len
    
    for i in range(n):
        for j in range(0, n - i - 1):
            if struct.values[j] > struct.values[j + 1]:
                # Обмен элементов
                temp = struct.values[j]
                struct.values[j] = struct.values[j + 1]
                struct.values[j + 1] = temp
"""

# Создаем транслятор
translator = JAVATranslator()

# Транслируем
try:
    java_json = translator.translate(python_code)
    
    # Выводим результат
    print(java_json)
    
    # Сохраняем в файл
    with open("output.json", "w", encoding="utf-8") as f:
        java_json
    print("\nРезультат сохранен в output.json")
    
except Exception as e:
    print(f"Ошибка: {e}")
    import traceback
    traceback.print_exc()