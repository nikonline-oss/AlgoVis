# yava_translator_service.py

import json
import ast
import sys
import os
from typing import Dict, Any, Optional, Union, List
from pathlib import Path

# Добавляем родительскую директорию в путь Python для импорта
sys.path.append(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

try:
    from IR.ast_to_ir import ASTtoIR
    from IR.ir_to_yava import IRToYAVA
    from IR.ir_nodes import *
    from IR.ir_control import *
except ImportError as e:
    print(f"Ошибка импорта: {e}")
    print("Убедитесь, что в папке IR есть файл __init__.py")
    raise

class YAVATranslatorService:
    """
    Сервис для трансляции Python кода в формат ЯВА.
    Обеспечивает удобный интерфейс для всего процесса трансляции.
    """
    
    def __init__(self, 
                 default_program_name: str = "Algorithm",
                 default_description: str = "Сгенерированный алгоритм",
                 default_structure_type: str = "array"):
        """
        Инициализация сервиса трансляции.
        
        Args:
            default_program_name: Название алгоритма по умолчанию
            default_description: Описание алгоритма по умолчанию
            default_structure_type: Тип структуры данных по умолчанию
        """
        self.default_program_name = default_program_name
        self.default_description = default_description
        self.default_structure_type = default_structure_type
        
        self._ir_converter = ASTtoIR()
        self._translator = None
        self._last_result = None
        
    def translate_from_python(self,
                              python_code: str,
                              program_name: Optional[str] = None,
                              description: Optional[str] = None,
                              structure_type: Optional[str] = None) -> Dict[str, Any]:
        """
        Трансляция Python кода в формат ЯВА.
        
        Args:
            python_code: Исходный код на Python
            program_name: Название алгоритма (если None, используется значение по умолчанию)
            description: Описание алгоритма (если None, используется значение по умолчанию)
            structure_type: Тип структуры данных (если None, используется значение по умолчанию)
            
        Returns:
            Словарь с алгоритмом в формате ЯВА
        """
        # Устанавливаем значения по умолчанию
        program_name = program_name or self.default_program_name
        description = description or self.default_description
        structure_type = structure_type or self.default_structure_type
        
        # Шаг 1: Python → AST → IR
        try:
            ast_tree = ast.parse(python_code)
            self._ir_converter = ASTtoIR()
            self._ir_converter.visit(ast_tree)
            ir_program = self._ir_converter.program
        except Exception as e:
            raise ValueError(f"Ошибка при разборе Python кода: {e}")
        
        # Шаг 2: IR → ЯВА
        try:
            self._translator = IRToYAVA(
                program_name=program_name,
                description=description,
                structure_type=structure_type
            )
            
            self._last_result = self._translator.translate(ir_program)
            return self._last_result
        except Exception as e:
            raise ValueError(f"Ошибка при трансляции в формат ЯВА: {e}")
    
    def translate_from_ir(self,
                          ir_program: Dict[str, Any],
                          program_name: Optional[str] = None,
                          description: Optional[str] = None,
                          structure_type: Optional[str] = None) -> Dict[str, Any]:
        """
        Трансляция уже сгенерированного IR в формат ЯВА.
        
        Args:
            ir_program: IR программа в виде словаря
            program_name: Название алгоритма
            description: Описание алгоритма
            structure_type: Тип структуры данных
            
        Returns:
            Словарь с алгоритмом в формате ЯВА
        """
        program_name = program_name or self.default_program_name
        description = description or self.default_description
        structure_type = structure_type or self.default_structure_type
        
        try:
            self._translator = IRToYAVA(
                program_name=program_name,
                description=description,
                structure_type=structure_type
            )
            
            self._last_result = self._translator.translate(ir_program)
            return self._last_result
        except Exception as e:
            raise ValueError(f"Ошибка при трансляции IR в формат ЯВА: {e}")
    
    def save_to_file(self, 
                     yava_data: Optional[Dict[str, Any]] = None,
                     filepath: Union[str, Path] = "algorithm.yava.json",
                     indent: int = 2) -> Path:
        """
        Сохранение результата трансляции в файл.
        
        Args:
            yava_data: Данные в формате ЯВА (если None, используется последний результат)
            filepath: Путь для сохранения файла
            indent: Отступ для форматирования JSON
            
        Returns:
            Путь к сохраненному файлу
        """
        data = yava_data or self._last_result
        if data is None:
            raise ValueError("Нет данных для сохранения. Сначала выполните трансляцию.")
        
        filepath = Path(filepath)
        
        with open(filepath, 'w', encoding='utf-8') as f:
            json.dump(data, f, ensure_ascii=False, indent=indent)
        
        return filepath
    
    def load_from_file(self, filepath: Union[str, Path]) -> Dict[str, Any]:
        """
        Загрузка алгоритма ЯВА из файла.
        
        Args:
            filepath: Путь к файлу с алгоритмом
            
        Returns:
            Данные алгоритма в формате ЯВА
        """
        filepath = Path(filepath)
        
        if not filepath.exists():
            raise FileNotFoundError(f"Файл не найден: {filepath}")
        
        with open(filepath, 'r', encoding='utf-8') as f:
            return json.load(f)
    
    def validate_yava(self, yava_data: Dict[str, Any]) -> tuple[bool, List[str]]:
        """
        Проверка валидности данных в формате ЯВА.
        
        Args:
            yava_data: Данные для проверки
            
        Returns:
            Кортеж (валиден ли, список ошибок)
        """
        errors = []
        
        # Проверка обязательных полей
        required_fields = ["name", "description", "structureType", "variables", "steps"]
        for field in required_fields:
            if field not in yava_data:
                errors.append(f"Отсутствует обязательное поле: {field}")
        
        # Проверка наличия start и end шагов
        if "steps" in yava_data:
            step_ids = [step.get("id") for step in yava_data["steps"]]
            if "start" not in step_ids:
                errors.append("Отсутствует шаг с id='start'")
            if "end" not in step_ids:
                errors.append("Отсутствует шаг с id='end'")
            
            # Проверка связей шагов
            for i, step in enumerate(yava_data["steps"]):
                step_id = step.get("id", f"unknown_{i}")
                
                # Проверка conditions
                if step.get("type") == "condition":
                    if "conditionCases" not in step:
                        errors.append(f"Шаг {step_id}: отсутствует conditionCases для условия")
                    else:
                        for case in step["conditionCases"]:
                            if "condition" not in case or "nextStep" not in case:
                                errors.append(f"Шаг {step_id}: неполный conditionCase")
                
                # Проверка обычных шагов
                elif step.get("type") not in ["return"]:
                    if "nextStep" not in step and step_id != "end":
                        errors.append(f"Шаг {step_id}: отсутствует nextStep")
        
        return len(errors) == 0, errors
    
    def get_statistics(self, yava_data: Optional[Dict[str, Any]] = None) -> Dict[str, Any]:
        """
        Получение статистики по алгоритму.
        
        Args:
            yava_data: Данные алгоритма (если None, используется последний результат)
            
        Returns:
            Словарь со статистикой
        """
        data = yava_data or self._last_result
        if data is None:
            return {}
        
        stats = {
            "name": data.get("name"),
            "description": data.get("description"),
            "structure_type": data.get("structureType"),
            "total_steps": len(data.get("steps", [])),
            "total_variables": len(data.get("variables", [])),
            "total_functions": len(data.get("functions", [])),
            "step_types": {},
            "has_start": False,
            "has_end": False
        }
        
        # Анализ типов шагов
        for step in data.get("steps", []):
            step_type = step.get("type", "unknown")
            stats["step_types"][step_type] = stats["step_types"].get(step_type, 0) + 1
            
            if step.get("id") == "start":
                stats["has_start"] = True
            if step.get("id") == "end":
                stats["has_end"] = True
        
        return stats
    
    def create_example(self, algorithm_type: str = "bubble_sort") -> Dict[str, Any]:
        """
        Создание примеров алгоритмов.
        
        Args:
            algorithm_type: Тип алгоритма (bubble_sort, linear_search, factorial, etc.)
            
        Returns:
            Алгоритм в формате ЯВА
        """
        examples = {
            "bubble_sort": """
arr = [5, 2, 8, 1, 9, 3]
n = len(arr)
for i in range(n):
    for j in range(0, n - i - 1):
        if arr[j] > arr[j + 1]:
            temp = arr[j]
            arr[j] = arr[j + 1]
            arr[j + 1] = temp
""",
            "linear_search": """
arr = [10, 20, 30, 40, 50]
target = 30
found = False
index = -1

for i in range(len(arr)):
    if arr[i] == target:
        found = True
        index = i
        break
""",
            "factorial": """
n = 5
result = 1

for i in range(1, n + 1):
    result = result * i
""",
            "sum_array": """
arr = [1, 2, 3, 4, 5]
total = 0

for i in range(len(arr)):
    total = total + arr[i]
""",
            "find_max": """
arr = [3, 7, 2, 9, 4]
max_value = arr[0]

for i in range(1, len(arr)):
    if arr[i] > max_value:
        max_value = arr[i]
"""
        }
        
        if algorithm_type not in examples:
            raise ValueError(f"Неизвестный тип алгоритма. Доступные: {list(examples.keys())}")
        
        return self.translate_from_python(
            python_code=examples[algorithm_type],
            program_name=algorithm_type.replace("_", " ").title(),
            description=f"Пример алгоритма: {algorithm_type.replace('_', ' ')}"
        )
    
    def export_for_web(self, 
                       yava_data: Optional[Dict[str, Any]] = None,
                       include_html: bool = True) -> str:
        """
        Экспорт алгоритма в формат для веб-визуализации.
        
        Args:
            yava_data: Данные алгоритма
            include_html: Включать ли HTML обертку
            
        Returns:
            HTML код с алгоритмом
        """
        data = yava_data or self._last_result
        if data is None:
            return ""
        
        json_str = json.dumps(data, ensure_ascii=False, indent=2)
        
        if not include_html:
            return json_str
        
        html_template = f"""
<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Алгоритм: {data.get('name', 'Без названия')}</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 20px;
            background-color: #f5f5f5;
        }}
        .algorithm-info {{
            background-color: white;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .algorithm-name {{
            color: #2c3e50;
            margin-top: 0;
        }}
        .algorithm-description {{
            color: #7f8c8d;
            line-height: 1.6;
        }}
        .json-viewer {{
            background-color: #2c3e50;
            color: #ecf0f1;
            padding: 20px;
            border-radius: 8px;
            font-family: 'Courier New', monospace;
            white-space: pre-wrap;
            overflow-x: auto;
        }}
        .stats {{
            background-color: white;
            padding: 15px;
            border-radius: 8px;
            margin-top: 20px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }}
        .stat-item {{
            margin: 5px 0;
        }}
    </style>
</head>
<body>
    <div class="algorithm-info">
        <h1 class="algorithm-name">Алгоритм: {data.get('name', 'Без названия')}</h1>
        <p class="algorithm-description">{data.get('description', 'Без описания')}</p>
    </div>
    
    <div class="json-viewer">
{json_str}
    </div>
    
    <div class="stats">
        <h3>Статистика:</h3>
        <div class="stat-item">Тип структуры: {data.get('structureType', 'Не указан')}</div>
        <div class="stat-item">Всего шагов: {len(data.get('steps', []))}</div>
        <div class="stat-item">Всего переменных: {len(data.get('variables', []))}</div>
    </div>
</body>
</html>
        """
        
        return html_template
    
    def get_last_result(self) -> Optional[Dict[str, Any]]:
        """
        Получение последнего результата трансляции.
        
        Returns:
            Последний результат трансляции или None
        """
        return self._last_result
    
    def get_ir_program(self) -> Optional[Dict[str, Any]]:
        """
        Получение промежуточного IR представления.
        
        Returns:
            IR программа или None
        """
        return getattr(self._ir_converter, 'program', None)
    
    def reset(self):
        """
        Сброс состояния сервиса.
        """
        self._ir_converter = ASTtoIR()
        self._translator = None
        self._last_result = None


class YAVATranslatorCLI:
    """
    Командный интерфейс для сервиса трансляции.
    """
    
    def __init__(self):
        self.service = YAVATranslatorService()
        
    def run(self):
        """
        Запуск интерактивного режима.
        """
        print("=== ЯВА Транслятор (Python → Визуализация алгоритмов) ===")
        
        while True:
            print("\nДоступные команды:")
            print("1. Трансляция из Python кода")
            print("2. Загрузка из файла Python")
            print("3. Создать пример алгоритма")
            print("4. Проверить валидность")
            print("5. Показать статистику")
            print("6. Экспорт в HTML")
            print("7. Сохранить в файл")
            print("8. Выход")
            
            choice = input("\nВыберите действие (1-8): ").strip()
            
            if choice == "1":
                self._translate_from_input()
            elif choice == "2":
                self._load_from_file()
            elif choice == "3":
                self._create_example()
            elif choice == "4":
                self._validate()
            elif choice == "5":
                self._show_stats()
            elif choice == "6":
                self._export_html()
            elif choice == "7":
                self._save_to_file()
            elif choice == "8":
                print("Выход...")
                break
            else:
                print("Неизвестная команда. Попробуйте снова.")
    
    def _translate_from_input(self):
        """Трансляция из введенного Python кода"""
        print("\nВведите Python код (введите 'END' на новой строке для завершения):")
        
        lines = []
        while True:
            line = input()
            if line.strip() == "END":
                break
            lines.append(line)
        
        python_code = "\n".join(lines)
        
        if not python_code.strip():
            print("Код не был введен.")
            return
        
        name = input("Название алгоритма (Enter для значения по умолчанию): ").strip()
        description = input("Описание алгоритма (Enter для значения по умолчанию): ").strip()
        
        try:
            result = self.service.translate_from_python(
                python_code=python_code,
                program_name=name if name else None,
                description=description if description else None
            )
            print(f"\n✅ Трансляция успешно завершена!")
            print(f"   Создано шагов: {len(result.get('steps', []))}")
        except Exception as e:
            print(f"\n❌ Ошибка: {e}")
    
    def _load_from_file(self):
        """Загрузка Python кода из файла"""
        filename = input("Введите путь к файлу Python: ").strip()
        
        try:
            with open(filename, 'r', encoding='utf-8') as f:
                python_code = f.read()
            
            result = self.service.translate_from_python(python_code)
            print(f"\n✅ Файл загружен и трансляция завершена!")
            print(f"   Создано шагов: {len(result.get('steps', []))}")
        except Exception as e:
            print(f"\n❌ Ошибка: {e}")
    
    def _create_example(self):
        """Создание примера алгоритма"""
        print("Доступные примеры:")
        examples = ["bubble_sort", "linear_search", "factorial", "sum_array", "find_max"]
        for i, example in enumerate(examples, 1):
            print(f"{i}. {example.replace('_', ' ').title()}")
        
        choice = input("Выберите пример (1-5): ").strip()
        
        try:
            idx = int(choice) - 1
            if 0 <= idx < len(examples):
                result = self.service.create_example(examples[idx])
                print(f"\n✅ Пример создан: {examples[idx]}")
                print(f"   Создано шагов: {len(result.get('steps', []))}")
            else:
                print("Неверный выбор.")
        except Exception as e:
            print(f"\n❌ Ошибка: {e}")
    
    def _validate(self):
        """Проверка валидности"""
        if self.service._last_result is None:
            print("Сначала выполните трансляцию.")
            return
        
        is_valid, errors = self.service.validate_yava(self.service._last_result)
        
        if is_valid:
            print("✅ Алгоритм валиден!")
        else:
            print("❌ Найдены ошибки:")
            for error in errors:
                print(f"   - {error}")
    
    def _show_stats(self):
        """Показать статистику"""
        if self.service._last_result is None:
            print("Сначала выполните трансляцию.")
            return
        
        stats = self.service.get_statistics()
        
        print("\n📊 Статистика алгоритма:")
        print(f"   Название: {stats.get('name')}")
        print(f"   Тип структуры: {stats.get('structure_type')}")
        print(f"   Всего шагов: {stats.get('total_steps')}")
        print(f"   Всего переменных: {stats.get('total_variables')}")
        print(f"   Всего функций: {stats.get('total_functions')}")
        
        if stats.get('step_types'):
            print("   Типы шагов:")
            for step_type, count in stats['step_types'].items():
                print(f"     - {step_type}: {count}")
    
    def _export_html(self):
        """Экспорт в HTML"""
        if self.service._last_result is None:
            print("Сначала выполните трансляцию.")
            return
        
        filename = input("Введите имя файла для сохранения HTML (Enter для стандартного): ").strip()
        if not filename:
            filename = f"algorithm_{self.service._last_result.get('name', 'unnamed')}.html"
        
        html = self.service.export_for_web()
        
        with open(filename, 'w', encoding='utf-8') as f:
            f.write(html)
        
        print(f"✅ HTML файл сохранен: {filename}")
    
    def _save_to_file(self):
        """Сохранение в JSON файл"""
        if self.service._last_result is None:
            print("Сначала выполните трансляцию.")
            return
        
        filename = input("Введите имя файла для сохранения JSON (Enter для стандартного): ").strip()
        if not filename:
            filename = f"algorithm_{self.service._last_result.get('name', 'unnamed')}.yava.json"
        
        try:
            self.service.save_to_file(filepath=filename)
            print(f"✅ JSON файл сохранен: {filename}")
        except Exception as e:
            print(f"❌ Ошибка: {e}")


def create_batch_translator(source_dir: str, 
                           output_dir: str,
                           structure_type: str = "array") -> List[Dict[str, Any]]:
    """
    Пакетная трансляция всех Python файлов в директории.
    
    Args:
        source_dir: Директория с Python файлами
        output_dir: Директория для сохранения результатов
        structure_type: Тип структуры данных
    
    Returns:
        Список результатов трансляции
    """
    import os
    from pathlib import Path
    
    source_path = Path(source_dir)
    output_path = Path(output_dir)
    output_path.mkdir(parents=True, exist_ok=True)
    
    service = YAVATranslatorService()
    results = []
    
    # Ищем все .py файлы
    python_files = list(source_path.glob("*.py"))
    
    print(f"Найдено {len(python_files)} Python файлов для трансляции")
    
    for py_file in python_files:
        print(f"Обработка: {py_file.name}")
        
        try:
            # Читаем Python код
            with open(py_file, 'r', encoding='utf-8') as f:
                python_code = f.read()
            
            # Транслируем
            yava_data = service.translate_from_python(
                python_code=python_code,
                program_name=py_file.stem,
                description=f"Транслировано из {py_file.name}",
                structure_type=structure_type
            )
            
            # Сохраняем результат
            output_file = output_path / f"{py_file.stem}.yava.json"
            service.save_to_file(yava_data, output_file)
            
            results.append({
                "file": py_file.name,
                "success": True,
                "output": output_file,
                "stats": service.get_statistics(yava_data)
            })
            
            print(f"  ✅ Успешно: {output_file.name}")
            
        except Exception as e:
            print(f"  ❌ Ошибка: {e}")
            results.append({
                "file": py_file.name,
                "success": False,
                "error": str(e)
            })
    
    return results


# Примеры использования
if __name__ == "__main__":
    # Пример 1: Быстрое использование
    # Пример 2: Запуск CLI
    cli = YAVATranslatorCLI()
    cli.run()
    
    # Пример 3: Пакетная обработка
    results = create_batch_translator(
        source_dir="./python_algorithms",
        output_dir="./yava_algorithms"
    )