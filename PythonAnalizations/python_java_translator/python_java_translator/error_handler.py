"""
Обработка ошибок трансляции
"""

import json
from typing import Dict, Any, List
from enum import Enum

class ErrorType(Enum):
    """Типы ошибок трансляции"""
    SYNTAX_ERROR = "SYNTAX_ERROR"
    SECURITY_ERROR = "SECURITY_ERROR"
    TRANSLATION_ERROR = "TRANSLATION_ERROR"
    VALIDATION_ERROR = "VALIDATION_ERROR"
    RECURSION_ERROR = "RECURSION_ERROR"
    SIZE_LIMIT_ERROR = "SIZE_LIMIT_ERROR"
    STRUCTURE_ERROR = "STRUCTURE_ERROR"
    UNKNOWN_ERROR = "UNKNOWN_ERROR"

class TranslationError(Exception):
    """Базовое исключение трансляции"""
    def __init__(self, message: str, error_type: ErrorType, details: Dict = None):
        super().__init__(message)
        self.error_type = error_type
        self.details = details or {}

class ErrorHandler:
    """Обработчик ошибок трансляции"""
    
    @staticmethod
    def create_error_result(error_message: str, 
                           error_type: ErrorType,
                           original_code: str = None) -> Dict[str, Any]:
        """
        Создание результата с ошибкой в формате ЯВА
        
        Args:
            error_message: Сообщение об ошибке
            error_type: Тип ошибки
            original_code: Исходный код (опционально)
            
        Returns:
            Словарь с ошибкой в формате ЯВА
        """
        
        # Создаем информативные шаги для визуализации ошибки
        steps = [
            {
                "id": "start",
                "type": "generic",
                "description": "Начало алгоритма",
                "parameters": [],
                "nextStep": "error_detected",
                "visualize": True
            },
            {
                "id": "error_detected",
                "type": "generic",
                "description": f"Обнаружена ошибка: {error_type.value}",
                "parameters": [],
                "nextStep": "error_details",
                "visualize": True,
                "highlightColor": "red"
            },
            {
                "id": "error_details",
                "type": "generic",
                "description": error_message[:100] + ("..." if len(error_message) > 100 else ""),
                "parameters": [],
                "nextStep": "end",
                "visualize": True
            },
            {
                "id": "end",
                "type": "generic",
                "description": "Алгоритм завершен с ошибкой",
                "parameters": [],
                "visualize": True
            }
        ]
        
        # Если есть исходный код, добавляем шаг с ним
        if original_code:
            steps.insert(1, {
                "id": "code_loaded",
                "type": "generic",
                "description": "Загружен исходный код",
                "parameters": [original_code[:50] + "..." if len(original_code) > 50 else original_code],
                "nextStep": "error_detected",
                "visualize": False
            })
        
        return {
            "name": "ОШИБКА ТРАНСЛЯЦИИ",
            "description": f"{error_type.value}: {error_message}",
            "structureType": "array",
            "variables": [
                {
                    "name": "errorType",
                    "type": "string",
                    "initialValue": error_type.value
                },
                {
                    "name": "errorMessage",
                    "type": "string",
                    "initialValue": error_message
                }
            ],
            "functions": [],
            "steps": steps,
            "error": True,
            "errorType": error_type.value,
            "errorMessage": error_message,
            "timestamp": "2024-01-01T00:00:00Z"  # Будет заменено на реальное время
        }
    
    @staticmethod
    def validate_java_output(java_data: Dict[str, Any]) -> List[str]:
        """
        Валидация сгенерированного ЯВА алгоритма
        """
        errors = []
    
        # Проверка обязательных полей
        required_fields = ["name", "description", "structureType", "variables", "steps"]
        for field in required_fields:
            if field not in java_data:
                errors.append(f"Отсутствует обязательное поле: {field}")
    
        if "steps" not in java_data:
            return errors
    
        steps = java_data["steps"]
    
        # Проверка уникальности ID шагов
        step_ids = [s.get("id") for s in steps if s.get("id")]
        if len(step_ids) != len(set(step_ids)):
            errors.append("Найдены дублирующиеся ID шагов")
    
        # Проверка обязательных шагов start и end
        start_exists = any(s.get("id") == "start" for s in steps)
        end_exists = any(s.get("id") == "end" for s in steps)
    
        if not start_exists:
            errors.append("Отсутствует шаг с id='start'")
    
        if not end_exists:
            errors.append("Отсутствует шаг с id='end'")
    
        # Находим start и end для дальнейшей проверки
        start_step = next((s for s in steps if s.get("id") == "start"), None)
        end_step = next((s for s in steps if s.get("id") == "end"), None)
    
        # Проверка что start и end - это отдельные шаги
        if start_step:
            # Проверка типа
            if start_step.get("type") != "generic":
                errors.append("Шаг start должен быть типа 'generic'")
        
            # Проверка визуализации
            if not start_step.get("visualize", True):
                errors.append("Шаг start должен быть визуализируемым (visualize=true)")
        
            # Проверка что start - первый шаг
            if steps[0].get("id") != "start":
                errors.append("Шаг start должен быть первым шагом в алгоритме")
    
        if end_step:
            # Проверка типа
            if end_step.get("type") != "generic":
                errors.append("Шаг end должен быть типа 'generic'")
        
            # Проверка визуализации
            if not end_step.get("visualize", True):
                errors.append("Шаг end должен быть визуализируемым (visualize=true)")
        
            # Проверка что end - последний шаг
            if steps[-1].get("id") != "end":
                errors.append("Шаг end должен быть последним шагом в алгоритме")
        
            # Проверка что у end нет nextStep
            if end_step.get("nextStep"):
                errors.append("Шаг end не должен иметь nextStep")
    
        # Проверка связей между шагами
        graph_errors = ErrorHandler._validate_step_graph(steps)
        errors.extend(graph_errors)
    
        # Проверка переменных
        var_errors = ErrorHandler._validate_variables(java_data.get("variables", []))
        errors.extend(var_errors)
    
        return errors
    
    @staticmethod
    def _validate_step_graph(steps: List[Dict]) -> List[str]:
        """Валидация графа шагов"""
        errors = []
        step_ids = {s.get("id") for s in steps}
        
        # Построение графа
        graph = {}
        for step in steps:
            step_id = step.get("id")
            if not step_id:
                continue
            
            connections = []
            
            # Прямые связи
            if "nextStep" in step and step["nextStep"]:
                connections.append(step["nextStep"])
            
            # Условные переходы
            if "conditionCases" in step:
                for case in step["conditionCases"]:
                    if "nextStep" in case and case["nextStep"]:
                        connections.append(case["nextStep"])
            
            # Убираем несуществующие связи
            valid_connections = [conn for conn in connections if conn in step_ids or conn == "end"]
            if connections != valid_connections:
                errors.append(f"Шаг '{step_id}' имеет ссылки на несуществующие шаги")
            
            graph[step_id] = valid_connections
        
        # Проверка достижимости end из start
        if "start" in step_ids and "end" in step_ids:
            visited = set()
            stack = ["start"]
            
            while stack:
                node = stack.pop()
                if node == "end":
                    visited.add(node)
                    break
                if node in visited:
                    continue
                visited.add(node)
                if node in graph:
                    stack.extend(graph[node])
            
            if "end" not in visited:
                errors.append("Шаг 'end' недостижим из 'start'")
        
        # Поиск циклов без выхода
        if ErrorHandler._has_dangerous_loops(graph, steps):
            errors.append("Обнаружены потенциально бесконечные циклы")
        
        return errors
    
    @staticmethod
    def _has_dangerous_loops(graph: Dict[str, List[str]], steps: List[Dict]) -> bool:
        """Поиск опасных циклов без выхода"""
        # Упрощенная проверка: циклы без условий выхода
        for step in steps:
            if step.get("type") == "condition":
                cases = step.get("conditionCases", [])
                if not cases:
                    continue
            
                # Проверяем, есть ли ветка, ведущая из цикла
                has_exit = any(
                    case.get("nextStep") == "end" or 
                    (case.get("nextStep") and case["nextStep"] not in graph.get(step.get("id"), []))
                    for case in cases
                )
                if not has_exit:
                    # Проверяем, что это действительно цикл, а не просто условие
                    step_id = step.get("id")
                    if step_id in graph:
                        # Проверяем, ссылается ли кто-то на этот шаг
                        has_incoming = any(step_id in graph.get(other_id, []) for other_id in graph)
                        if has_incoming:
                            return True
        return False
    
    @staticmethod
    def _validate_variables(variables: List[Dict]) -> List[str]:
        """Валидация переменных"""
        errors = []
        var_names = set()
        
        for var in variables:
            name = var.get("name", "")
            
            if not name:
                errors.append("Найдена переменная без имени")
                continue
            
            if name in var_names:
                errors.append(f"Дублирующееся имя переменной: {name}")
            
            var_names.add(name)
            
            # Проверка зарезервированных имен
            reserved_names = {"struct", "start", "end", "true", "false", "null"}
            if name in reserved_names:
                errors.append(f"Использование зарезервированного имени: {name}")
            
            # Проверка типа
            var_type = var.get("type", "")
            valid_types = {"int", "float", "bool", "string", "array", "object"}
            if var_type not in valid_types:
                errors.append(f"Неверный тип переменной '{name}': {var_type}")
        
        return errors