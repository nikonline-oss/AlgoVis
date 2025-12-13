"""Утилиты для определения типов переменных"""
import ast
import re
from typing import Dict, Any, Optional, Union, Set


class TypeResolver:
    """Определяет типы переменных в Python коде"""
    
    def __init__(self):
        self.type_mapping = {
            int: "int",
            float: "float",
            bool: "bool",
            str: "string",
            list: "array",
            dict: "object",
            tuple: "array"  # Кортежи считаем массивами
        }
        
        # Типы по умолчанию для распространенных функций
        self.function_return_types = {
            "len": "int",
            "range": "array",
            "list": "array",
            "dict": "object",
            "str": "string",
            "int": "int",
            "float": "float",
            "bool": "bool",
            "abs": "int",
            "min": "int",
            "max": "int",
            "sum": "int",
            "sorted": "array",
            "reversed": "array",
        }
    
    def infer_type_from_value(self, value: Any) -> str:
        """Определяет тип по значению"""
        if isinstance(value, (list, tuple)):
            return "array"
        elif isinstance(value, dict):
            return "object"
        elif isinstance(value, bool):
            return "bool"
        elif isinstance(value, int):
            return "int"
        elif isinstance(value, float):
            return "float"
        elif isinstance(value, str):
            return "string"
        else:
            return "int"  # По умолчанию
    
    def infer_type_from_expression(self, expr: ast.expr, 
                                  context: Dict[str, str] = None) -> str:
        """Определяет тип выражения AST"""
        if context is None:
            context = {}
        
        if isinstance(expr, ast.Constant):
            return self.infer_type_from_value(expr.value)
        
        elif isinstance(expr, ast.Name):
            # Ищем тип в контексте
            var_name = expr.id
            if var_name in context:
                return context[var_name]
            return "int"  # По умолчанию
        
        elif isinstance(expr, ast.List):
            return "array"
        
        elif isinstance(expr, ast.Dict):
            return "object"
        
        elif isinstance(expr, ast.Call):
            # Определяем тип по имени функции
            if isinstance(expr.func, ast.Name):
                func_name = expr.func.id
                if func_name in self.function_return_types:
                    return self.function_return_types[func_name]
            
            # Для методов объектов
            elif isinstance(expr.func, ast.Attribute):
                # Пока возвращаем string для методов строк и т.д.
                return "string"
            
            return "int"  # По умолчанию
        
        elif isinstance(expr, ast.BinOp):
            # Для арифметических операций
            if isinstance(expr.op, (ast.Add, ast.Sub, ast.Mult, ast.Div, ast.Mod, ast.Pow)):
                # Пытаемся определить тип операндов
                left_type = self.infer_type_from_expression(expr.left, context)
                right_type = self.infer_type_from_expression(expr.right, context)
                
                # Если оба int -> int, иначе float
                if left_type == "int" and right_type == "int":
                    return "int"
                elif left_type in ["int", "float"] and right_type in ["int", "float"]:
                    return "float"
                elif left_type == "string" or right_type == "string":
                    return "string"  # Для конкатенации
            
            return "int"  # По умолчанию
        
        elif isinstance(expr, ast.Compare):
            # Операции сравнения возвращают bool
            return "bool"
        
        elif isinstance(expr, ast.BoolOp):
            # Логические операции возвращают bool
            return "bool"
        
        elif isinstance(expr, ast.UnaryOp):
            # Унарные операции
            operand_type = self.infer_type_from_expression(expr.operand, context)
            if isinstance(expr.op, ast.USub) and operand_type in ["int", "float"]:
                return operand_type
            elif isinstance(expr.op, ast.Not):
                return "bool"
            
            return operand_type
        
        elif isinstance(expr, ast.Subscript):
            # Для индексации массива
            value_type = self.infer_type_from_expression(expr.value, context)
            if value_type == "array":
                # Пытаемся определить тип элементов массива
                return "int"  # По умолчанию для элементов массива
            return "int"  # По умолчанию
        
        elif isinstance(expr, ast.Attribute):
            # Для атрибутов объектов
            return "int"  # По умолчанию
        
        return "int"  # Дефолтный тип
    
    def analyze_variables(self, python_code: str) -> Dict[str, str]:
        """Анализирует код и определяет типы переменных"""
        try:
            tree = ast.parse(python_code)
        except SyntaxError:
            return {}
        
        # Собираем информацию о присваиваниях
        variable_types = {}
        
        for node in ast.walk(tree):
            if isinstance(node, ast.Assign):
                for target in node.targets:
                    if isinstance(target, ast.Name):
                        var_name = target.id
                        # Определяем тип значения
                        var_type = self.infer_type_from_expression(node.value, variable_types)
                        variable_types[var_name] = var_type
        
        return variable_types
    
    def get_initial_value(self, expr: ast.expr, default_type: str = "int") -> Any:
        """Получает начальное значение из выражения"""
        if isinstance(expr, ast.Constant):
            return expr.value
        
        elif isinstance(expr, ast.List):
            return [self.get_initial_value(e) for e in expr.elts]
        
        elif isinstance(expr, ast.Dict):
            result = {}
            for key, value in zip(expr.keys, expr.values):
                if key is not None:
                    key_value = self.get_initial_value(key)
                    value_value = self.get_initial_value(value)
                    result[key_value] = value_value
            return result
        
        elif isinstance(expr, ast.Name):
            # Для переменных возвращаем имя
            return expr.id
        
        elif isinstance(expr, ast.Call):
            # Для вызовов функций возвращаем строку
            return self._expression_to_string(expr)
        
        else:
            # Для других выражений возвращаем строковое представление
            expr_str = self._expression_to_string(expr)
            
            # Пытаемся вычислить простые выражения
            try:
                # Безопасное вычисление только для простых выражений
                if re.match(r'^[0-9+\-*/%\s()]+$', expr_str):
                    return eval(expr_str)
            except:
                pass
            
            return expr_str
    
    def _expression_to_string(self, expr: ast.expr) -> str:
        """Конвертирует выражение AST в строку (базовый вариант)"""
        if isinstance(expr, ast.Constant):
            return repr(expr.value)
        elif isinstance(expr, ast.Name):
            return expr.id
        elif isinstance(expr, ast.BinOp):
            left = self._expression_to_string(expr.left)
            right = self._expression_to_string(expr.right)
            op_map = {
                ast.Add: '+',
                ast.Sub: '-',
                ast.Mult: '*',
                ast.Div: '/',
                ast.Mod: '%',
                ast.Pow: '^',
            }
            op = op_map.get(type(expr.op), '?')
            return f"({left} {op} {right})"
        elif isinstance(expr, ast.Call):
            func = self._expression_to_string(expr.func)
            args = [self._expression_to_string(arg) for arg in expr.args]
            return f"{func}({', '.join(args)})"
        else:
            return str(ast.dump(expr, indent=2))