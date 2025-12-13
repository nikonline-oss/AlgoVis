"""Валидатор JSON в формате ЯВА"""
import json
from typing import Dict, List, Any, Tuple


class JAVAValidator:
    def __init__(self):
        # Обязательные поля для алгоритма
        self.required_algorithm_fields = {
            "name": str,
            "description": str,
            "structureType": str,
            "variables": list,
            "steps": list
        }
        
        # Допустимые типы структур
        self.valid_structure_types = {"array", "binarytree", "graph", "linkedlist"}
        
        # Допустимые типы переменных
        self.valid_variable_types = {"int", "float", "bool", "string", "array", "object"}
        
        # Допустимые типы шагов
        self.valid_step_types = {
            "assign", "compare", "swap", "condition", 
            "call_function", "return", "generic"
        }
        
    def validate_json(self, java_json: str) -> Dict[str, Any]:
        """
        Валидирует JSON на соответствие спецификации ЯВА.
        
        Args:
            java_json: JSON строка в формате ЯВА
            
        Returns:
            Словарь с результатами валидации:
            {
                "valid": bool,
                "errors": List[str],
                "warnings": List[str]
            }
        """
        try:
            data = json.loads(java_json)
        except json.JSONDecodeError as e:
            return {
                "valid": False,
                "errors": [f"Ошибка декодирования JSON: {str(e)}"],
                "warnings": []
            }
        
        errors = []
        warnings = []
        
        # Проверка обязательных полей
        for field, expected_type in self.required_algorithm_fields.items():
            if field not in data:
                errors.append(f"Отсутствует обязательное поле: {field}")
            elif not isinstance(data[field], expected_type):
                errors.append(f"Поле {field} должно быть типа {expected_type.__name__}")
        
        # Если есть ошибки, дальше не проверяем
        if errors:
            return {"valid": False, "errors": errors, "warnings": warnings}
        
        # Проверка structureType
        if data["structureType"] not in self.valid_structure_types:
            errors.append(f"Недопустимый structureType: {data['structureType']}. "
                         f"Допустимые значения: {', '.join(self.valid_structure_types)}")
        
        # Валидация переменных
        for i, var in enumerate(data["variables"]):
            var_errors = self._validate_variable(var, i)
            errors.extend(var_errors)
        
        # Валидация шагов
        step_ids = set()
        for i, step in enumerate(data["steps"]):
            step_errors, step_warnings = self._validate_step(step, i, "main")
            errors.extend(step_errors)
            warnings.extend(step_warnings)
            
            # Проверка уникальности ID
            if step["id"] in step_ids:
                errors.append(f"Дублирующийся ID шага: {step['id']}")
            step_ids.add(step["id"])
        
        # Валидация функций
        if "functions" in data:
            if not isinstance(data["functions"], list):
                errors.append("Поле functions должно быть массивом")
            else:
                for i, func in enumerate(data["functions"]):
                    func_errors, func_warnings = self._validate_function(func, i)
                    errors.extend(func_errors)
                    warnings.extend(func_warnings)
        
        # Проверка наличия start и end шагов
        if "start" not in step_ids:
            errors.append("Отсутствует шаг с id 'start'")
        if "end" not in step_ids:
            errors.append("Отсутствует шаг с id 'end'")
        
        # Проверка корректности ссылок между шагами
        if not errors:
            link_errors = self._validate_step_links(data)
            errors.extend(link_errors)
        
        return {
            "valid": len(errors) == 0,
            "errors": errors,
            "warnings": warnings
        }
    
    def _validate_variable(self, var: Dict, index: int) -> List[str]:
        """Валидирует одну переменную"""
        errors = []
        
        # Проверка обязательных полей переменной
        required_fields = ["name", "type", "initialValue"]
        for field in required_fields:
            if field not in var:
                errors.append(f"Переменная #{index}: отсутствует поле '{field}'")
        
        if errors:
            return errors
        
        # Проверка типа переменной
        if var["type"] not in self.valid_variable_types:
            errors.append(f"Переменная {var['name']}: недопустимый тип '{var['type']}'. "
                         f"Допустимые типы: {', '.join(self.valid_variable_types)}")
        
        # Проверка начального значения в зависимости от типа
        if var["type"] == "array":
            if not isinstance(var["initialValue"], list):
                errors.append(f"Переменная {var['name']}: для типа 'array' "
                             f"initialValue должен быть массивом")
        elif var["type"] == "object":
            if not isinstance(var["initialValue"], dict):
                errors.append(f"Переменная {var['name']}: для типа 'object' "
                             f"initialValue должен быть объектом")
        elif var["type"] == "int":
            if not isinstance(var["initialValue"], int):
                try:
                    int(var["initialValue"])
                except (ValueError, TypeError):
                    errors.append(f"Переменная {var['name']}: для типа 'int' "
                                 f"initialValue должно быть целым числом")
        elif var["type"] == "float":
            if not isinstance(var["initialValue"], (int, float)):
                try:
                    float(var["initialValue"])
                except (ValueError, TypeError):
                    errors.append(f"Переменная {var['name']}: для типа 'float' "
                                 f"initialValue должно быть числом")
        elif var["type"] == "bool":
            if not isinstance(var["initialValue"], bool):
                if var["initialValue"] not in ["true", "false", "True", "False"]:
                    errors.append(f"Переменная {var['name']}: для типа 'bool' "
                                 f"initialValue должно быть true или false")
        
        return errors
    
    def _validate_step(self, step: Dict, index: int, context: str) -> Tuple[List[str], List[str]]:
        """Валидирует один шаг"""
        errors = []
        warnings = []
        
        # Проверка обязательных полей
        required_fields = ["id", "type", "description"]
        for field in required_fields:
            if field not in step:
                errors.append(f"Шаг #{index} ({context}): отсутствует поле '{field}'")
        
        if errors:
            return errors, warnings
        
        # Проверка типа шага
        if step["type"] not in self.valid_step_types:
            errors.append(f"Шаг {step['id']}: недопустимый тип шага '{step['type']}'. "
                         f"Допустимые типы: {', '.join(self.valid_step_types)}")
        
        # Проверка параметров
        if "parameters" not in step:
            warnings.append(f"Шаг {step['id']}: отсутствует поле 'parameters'")
        elif not isinstance(step["parameters"], list):
            errors.append(f"Шаг {step['id']}: parameters должен быть массивом")
        
        # Проверка conditionCases для condition шагов
        if step["type"] == "condition":
            if "conditionCases" not in step:
                errors.append(f"Шаг {step['id']}: для condition шага отсутствует conditionCases")
            elif not isinstance(step["conditionCases"], list):
                errors.append(f"Шаг {step['id']}: conditionCases должен быть массивом")
            elif len(step["conditionCases"]) != 2:
                errors.append(f"Шаг {step['id']}: conditionCases должен содержать "
                             "ровно 2 случая (true и false)")
            else:
                for i, case in enumerate(step["conditionCases"]):
                    if "condition" not in case:
                        errors.append(f"Шаг {step['id']}: conditionCases[{i}] "
                                     "отсутствует поле 'condition'")
                    elif case["condition"] not in ["true", "false"]:
                        errors.append(f"Шаг {step['id']}: conditionCases[{i}] "
                                     f"condition должен быть 'true' или 'false', "
                                     f"получено: {case['condition']}")
                    if "nextStep" not in case:
                        errors.append(f"Шаг {step['id']}: conditionCases[{i}] "
                                     "отсутствует поле 'nextStep'")
        
        # Проверка functionName для call_function шагов
        if step["type"] == "call_function":
            if "functionName" not in step:
                errors.append(f"Шаг {step['id']}: для call_function шага "
                             "отсутствует functionName")
            if "returnToStep" not in step:
                warnings.append(f"Шаг {step['id']}: для call_function шага "
                              "рекомендуется указать returnToStep")
        
        # Проверка nextStep для всех шагов кроме end
        if step["type"] != "generic" or step["id"] != "end":
            if "nextStep" not in step and step["type"] not in ["condition", "call_function"]:
                warnings.append(f"Шаг {step['id']}: отсутствует nextStep")
        
        # Проверка visualize
        if "visualize" not in step:
            warnings.append(f"Шаг {step['id']}: отсутствует поле 'visualize'")
        elif not isinstance(step["visualize"], bool):
            warnings.append(f"Шаг {step['id']}: visualize должен быть boolean")
        
        return errors, warnings
    
    def _validate_function(self, func: Dict, index: int) -> Tuple[List[str], List[str]]:
        """Валидирует функцию"""
        errors = []
        warnings = []
        
        # Проверка обязательных полей функции
        required_fields = ["name", "description", "parameters", 
                          "entryPoint", "steps"]
        for field in required_fields:
            if field not in func:
                errors.append(f"Функция #{index}: отсутствует поле '{field}'")
        
        if errors:
            return errors, warnings
        
        # Проверка имени функции
        if not isinstance(func["name"], str) or not func["name"]:
            errors.append(f"Функция #{index}: имя функции должно быть непустой строкой")
        
        # Проверка parameters
        if not isinstance(func["parameters"], list):
            errors.append(f"Функция {func['name']}: parameters должен быть массивом")
        
        # Проверка entryPoint
        if not isinstance(func["entryPoint"], str):
            errors.append(f"Функция {func['name']}: entryPoint должен быть строкой")
        
        # Валидация шагов функции
        step_ids = set()
        for i, step in enumerate(func["steps"]):
            step_errors, step_warnings = self._validate_step(step, i, f"функция {func['name']}")
            errors.extend(step_errors)
            warnings.extend(step_warnings)
            
            # Проверка уникальности ID
            if step["id"] in step_ids:
                errors.append(f"Функция {func['name']}: дублирующийся ID шага: {step['id']}")
            step_ids.add(step["id"])
        
        # Проверка что entryPoint существует
        if func["entryPoint"] not in step_ids:
            errors.append(f"Функция {func['name']}: entryPoint '{func['entryPoint']}' "
                         "не найден среди шагов функции")
        
        return errors, warnings
    
    def _validate_step_links(self, data: Dict) -> List[str]:
        """Проверяет корректность ссылок между шагами"""
        errors = []
        
        # Собираем все ID шагов
        all_step_ids = set()
        all_step_ids.add("start")
        all_step_ids.add("end")
        
        # ID шагов в основных шагах
        for step in data["steps"]:
            all_step_ids.add(step["id"])
        
        # ID шагов в функциях
        if "functions" in data:
            for func in data["functions"]:
                for step in func["steps"]:
                    all_step_ids.add(step["id"])
        
        # Проверка ссылок в основных шагах
        for step in data["steps"]:
            errors.extend(self._check_step_links(step, all_step_ids, "main"))
        
        # Проверка ссылок в функциях
        if "functions" in data:
            for func in data["functions"]:
                for step in func["steps"]:
                    errors.extend(self._check_step_links(step, all_step_ids, func["name"]))
        
        return errors
    
    def _check_step_links(self, step: Dict, all_step_ids: set, context: str) -> List[str]:
        """Проверяет ссылки для одного шага"""
        errors = []
        
        # Проверка nextStep
        if "nextStep" in step and step["nextStep"]:
            if step["nextStep"] not in all_step_ids:
                errors.append(f"Шаг {step['id']} ({context}): nextStep "
                            f"'{step['nextStep']}' ссылается на несуществующий шаг")
        
        # Проверка conditionCases
        if step["type"] == "condition" and "conditionCases" in step:
            for case in step["conditionCases"]:
                if "nextStep" in case and case["nextStep"]:
                    if case["nextStep"] not in all_step_ids:
                        errors.append(f"Шаг {step['id']} ({context}): conditionCases "
                                    f"nextStep '{case['nextStep']}' ссылается на несуществующий шаг")
        
        # Проверка returnToStep
        if step["type"] == "call_function" and "returnToStep" in step and step["returnToStep"]:
            if step["returnToStep"] not in all_step_ids:
                errors.append(f"Шаг {step['id']} ({context}): returnToStep "
                            f"'{step['returnToStep']}' ссылается на несуществующий шаг")
        
        return errors
    
    def validate_file(self, file_path: str) -> Dict[str, Any]:
        """Валидирует JSON файл"""
        try:
            with open(file_path, 'r', encoding='utf-8') as f:
                java_json = f.read()
            return self.validate_json(java_json)
        except Exception as e:
            return {
                "valid": False,
                "errors": [f"Ошибка чтения файла: {str(e)}"],
                "warnings": []
            }