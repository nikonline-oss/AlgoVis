"""Генератор JSON в формате ЯВА"""
import json
from typing import Dict, Any
from .intermediate import Algorithm, StructureType, StepType


class JAVAGenerator:
    def __init__(self):
        self.indent = 2
    
    def generate(self, algorithm: Algorithm) -> str:
        """Генерирует JSON в формате ЯВА"""
        result = {
            "name": algorithm.name,
            "description": algorithm.description,
            "structureType": algorithm.structure_type.value,
            "variables": self._serialize_variables(algorithm.variables),
            "functions": self._serialize_functions(algorithm.functions),
            "steps": self._serialize_steps(algorithm.steps)
        }
        
        # Добавляем metadata если есть
        if algorithm.metadata:
            result["metadata"] = algorithm.metadata
            
        return json.dumps(result, indent=self.indent, ensure_ascii=False, default=self._json_default)
    
    def _json_default(self, obj):
        """Обработка объектов для JSON сериализации"""
        if isinstance(obj, (bool, int, float, str)):
            return obj
        elif obj is None:
            return None
        else:
            return str(obj)
    
    def _serialize_variables(self, variables):
        """Сериализует переменные"""
        result = []
        for var in variables:
            var_dict = {
                "name": var.name,
                "type": var.type,
                "initialValue": var.initial_value
            }
            if var.description:
                var_dict["description"] = var.description
            result.append(var_dict)
        return result
    
    def _serialize_functions(self, functions):
        """Сериализует функции"""
        result = []
        for func in functions:
            func_dict = {
                "name": func.name,
                "description": func.description,
                "parameters": func.parameters,
                "entryPoint": func.entry_point,
                "steps": self._serialize_steps(func.steps)
            }
            if func.return_type != "void":
                func_dict["returnType"] = func.return_type
            result.append(func_dict)
        return result
    
    def _serialize_steps(self, steps):
        """Сериализует шаги"""
        result = []
        for step in steps:
            step_dict = {
                "id": step.id,
                "type": step.type.value,
                "description": step.description,
                "parameters": step.parameters,
                "visualize": step.visualize
            }
            
            # Добавляем опциональные поля
            if step.next_step:
                step_dict["nextStep"] = step.next_step
            
            if step.condition_cases:
                step_dict["conditionCases"] = [
                    {
                        "condition": case.condition, 
                        "nextStep": case.next_step
                    }
                    for case in step.condition_cases
                ]
            
            if step.function_name:
                step_dict["functionName"] = step.function_name
            
            if step.function_parameters:
                step_dict["functionParameters"] = step.function_parameters
            
            if step.return_to_step:
                step_dict["returnToStep"] = step.return_to_step
            
            if step.highlight_elements:
                step_dict["highlightElements"] = step.highlight_elements
            
            if step.highlight_color:
                step_dict["highlightColor"] = step.highlight_color
            
            if step.metadata:
                for key, value in step.metadata.items():
                    step_dict[key] = value
            
            result.append(step_dict)
        return result