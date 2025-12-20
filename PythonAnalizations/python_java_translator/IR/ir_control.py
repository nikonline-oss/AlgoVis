# ir_control.py (УЛУЧШЕННАЯ ВЕРИСИЯ)

from .ir_nodes import IRStatement


class IRFor:
    """Цикл for"""
    def __init__(self, var: str, start: str, end: str, step: str = "1"):
        self.var = var
        self.start = start
        self.end = end
        self.step = step
        self.body = []
    
    def __repr__(self):
        return f"IRFor({self.var} from {self.start} to {self.end} step {self.step})"


class IRWhile:
    """Цикл while"""
    def __init__(self, condition: str):
        self.condition = condition
        self.body = []
    
    def __repr__(self):
        return f"IRWhile({self.condition})"


class IRBreak(IRStatement):
    """Прерывание цикла (break)"""
    def __repr__(self):
        return "IRBreak()"


class IRContinue(IRStatement):
    """Продолжение цикла (continue)"""
    def __repr__(self):
        return "IRContinue()"


class IRIf:
    """Условный оператор if"""
    def __init__(self, condition: str):
        self.condition = condition
        self.true_body = []
        self.false_body = []
    
    def __repr__(self):
        return f"IRIf({self.condition})"


class IRSwap(IRStatement):
    """
    Swap двух элементов массива.
    """
    def __init__(self, array_name: str, idx1: str, idx2: str):
        self.array_name = array_name
        self.idx1 = idx1
        self.idx2 = idx2
        self.highlight_elements = [idx1, idx2]
    
    def __repr__(self):
        return f"IRSwap({self.array_name}[{self.idx1}], {self.array_name}[{self.idx2}])"