# ir_nodes.py (УЛУЧШЕННАЯ ВЕРИСИЯ)

from typing import Any


class IRNode:
    """
    Базовый класс для всех узлов промежуточного представления (IR).
    """
    pass


class IRStatement(IRNode):
    """
    Базовый класс для всех операторов промежуточного представления (IR).
    """
    pass


class IRExpression(IRNode):
    """
    Базовый класс для всех выражений промежуточного представления (IR).
    """
    pass


class IRAssign(IRStatement):
    """
    Представление присваивания значения переменной в IR.
    
    Attributes:
        target (str): Целевая переменная для присваивания
        value (Any): Присваиваемое значение
    """
    
    def __init__(self, target: str, value: Any):
        """
        Инициализирует оператор присваивания.
        
        Args:
            target (str): Целевая переменная
            value (Any): Значение для присваивания
        """
        self.target = target
        self.value = value
    
    def __repr__(self):
        return f"IRAssign({self.target} = {self.value})"


class IRCompare(IRStatement):
    """
    Представление сравнения двух значений в IR.
    
    Результат сравнения всегда записывается в переменную 'last_comparison'.
    
    Attributes:
        left (str): Левый операнд сравнения
        right (str): Правый операнд сравнения
        result (str): Имя переменной для результата (всегда 'last_comparison')
    """
    
    def __init__(self, left: str, right: str):
        """
        Инициализирует оператор сравнения.
        
        Args:
            left (str): Левый операнд
            right (str): Правый операнд
        """
        self.left = left
        self.right = right
        self.result = "last_comparison"
    
    def __repr__(self):
        return f"IRCompare({self.left}, {self.right})"


class IRReturn(IRStatement):
    """
    Представление возврата из функции в IR.
    
    Attributes:
        value (Any): Возвращаемое значение (может быть None)
    """
    
    def __init__(self, value: Any = None):
        """
        Инициализирует оператор возврата.
        
        Args:
            value (Any): Возвращаемое значение
        """
        self.value = value
    
    def __repr__(self):
        return f"IRReturn({self.value})"