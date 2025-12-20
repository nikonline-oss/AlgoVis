# test_fixed_pipeline.py

import ast
import json
from .ast_to_ir import ASTtoIR
from .ir_to_yava import IRToYAVA

python_code = """
arr = [5, 2, 3]
n = len(arr)
for i in range(n):
    for j in range(0, n - i - 1):
        if arr[j] > arr[j + 1]:
            # Обмен элементов
            temp = arr[j]
            arr[j] = arr[j + 1]
            arr[j + 1] = temp
"""

# Python → AST → IR
ast_tree = ast.parse(python_code)
ir_converter = ASTtoIR()
ir_converter.visit(ast_tree)
ir_program = ir_converter.program

print("✅ IR программа:")
for stmt in ir_program["statements"][:3]:
    print(f"  - {type(stmt).__name__}")

# IR → ЯВА JSON
yava_translator = IRToYAVA(
    program_name="BubbleSort",
    description="Сортировка пузырьком с визуализацией",
    structure_type="array"
)

yava_json = yava_translator.translate(ir_program)

print("\n✅ ЯВА JSON сгенерирован:")
print(f"   Всего шагов: {len(yava_json['steps'])}")
print(f"   Переменных: {len(yava_json['variables'])}")

# Проверяем связи
print("\n🔗 Проверка связей шагов:")
for step in yava_json["steps"][:10]:
    if step["type"] == "condition":
        print(f"  {step['id']} ({step['type']}) -> {step.get('conditionCases', [])}")
    else:
        print(f"  {step['id']} ({step['type']}) -> {step.get('nextStep', 'END')}")

# Сохраняем
with open("bubble_sort_fixed.json", "w", encoding="utf-8") as f:
    json.dump(yava_json, f, ensure_ascii=False, indent=2)
print("\n💾 Файл сохранен: bubble_sort_fixed.json")