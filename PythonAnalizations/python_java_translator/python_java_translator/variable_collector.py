"""
Сборщик переменных из AST Python кода
"""

import ast
from typing import Set

class VariableCollector(ast.NodeVisitor):
    """Сборщик переменных из AST Python кода"""
    
    def __init__(self):
        self.assigned_vars: Set[str] = set()  # Переменные, которым что-то присваивается
        self.used_vars: Set[str] = set()  # Переменные, которые используются
        self.function_calls: Set[str] = set()  # Вызовы функций
        
    def visit_Assign(self, node):
        """Обработка присваивания"""
        for target in node.targets:
            if isinstance(target, ast.Name):
                self.assigned_vars.add(target.id)
        
        # Рекурсивно обходим значение
        self.visit(node.value)
    
    def visit_Name(self, node):
        """Обработка использования имени переменной"""
        # Добавляем в использованные переменные, если это не присваивание
        if isinstance(node.ctx, ast.Load):
            self.used_vars.add(node.id)
    
    def visit_Call(self, node):
        """Обработка вызова функции"""
        if isinstance(node.func, ast.Name):
            self.function_calls.add(node.func.id)
        self.generic_visit(node)
    
    def visit_For(self, node):
        """Обработка цикла for"""
        # Переменная цикла
        if isinstance(node.target, ast.Name):
            self.assigned_vars.add(node.target.id)
        self.generic_visit(node)
    
    def visit_FunctionDef(self, node):
        """Обработка определения функции - пропускаем, не собираем параметры"""
        pass  # Не собираем параметры функций в глобальные переменные