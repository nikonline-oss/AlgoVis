# ir_nodes.py (УЛУЧШЕННАЯ ВЕРИСИЯ)

from typing import Any

class IRNode:
    """Базовый класс для всех узлов IR"""
    pass


class IRStatement(IRNode):
    """Базовый класс для всех операторов IR"""
    pass


class IRExpression(IRNode):
    """Базовый класс для всех выражений IR"""
    pass


class IRAssign(IRStatement):
    """Присваивание значения переменной"""
    def __init__(self, target: str, value: Any):
        self.target = target
        self.value = value
    
    def __repr__(self):
        return f"IRAssign({self.target} = {self.value})"


class IRCompare(IRStatement):
    """
    Сравнение двух значений
    результат всегда пишется в last_comparison
    """
    def __init__(self, left: str, right: str):
        self.left = left
        self.right = right
        self.result = "last_comparison"
    
    def __repr__(self):
        return f"IRCompare({self.left}, {self.right})"


class IRReturn(IRStatement):
    """Возврат из функции"""
    def __init__(self, value: Any = None):
        self.value = value
    
    def __repr__(self):
        return f"IRReturn({self.value})"