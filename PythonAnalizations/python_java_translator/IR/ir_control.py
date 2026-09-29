# ir_control.py (УЛУЧШЕННАЯ ВЕРИСИЯ)

from .ir_nodes import IRStatement


class IRFor:
    """
    Представление цикла for в промежуточном представлении.
    
    Attributes:
        var (str): Имя переменной цикла
        start (str): Начальное значение
        end (str): Конечное значение (не включается)
        step (str): Шаг инкремента
        body (list): Тело цикла
    """
    
    def __init__(self, var: str, start: str, end: str, step: str = "1"):
        """
        Инициализирует цикл for.
        
        Args:
            var (str): Имя переменной цикла
            start (str): Начальное значение
            end (str): Конечное значение
            step (str): Шаг инкремента
        """
        self.var = var
        self.start = start
        self.end = end
        self.step = step
        self.body = []
    
    def __repr__(self):
        return f"IRFor({self.var} from {self.start} to {self.end} step {self.step})"


class IRWhile:
    """
    Представление цикла while в промежуточном представлении.
    
    Attributes:
        condition (str): Условие продолжения цикла
        body (list): Тело цикла
    """
    
    def __init__(self, condition: str):
        """
        Инициализирует цикл while.
        
        Args:
            condition (str): Условие продолжения
        """
        self.condition = condition
        self.body = []
    
    def __repr__(self):
        return f"IRWhile({self.condition})"


class IRBreak(IRStatement):
    """
    Представление оператора break в промежуточном представлении.
    """
    
    def __repr__(self):
        return "IRBreak()"


class IRContinue(IRStatement):
    """
    Представление оператора continue в промежуточном представлении.
    """
    
    def __repr__(self):
        return "IRContinue()"


class IRIf:
    """
    Представление условного оператора if в промежуточном представлении.
    
    Attributes:
        condition (str): Условие проверки
        true_body (list): Тело при истинном условии
        false_body (list): Тело при ложном условии
    """
    
    def __init__(self, condition: str):
        """
        Инициализирует условный оператор if.
        
        Args:
            condition (str): Условие проверки
        """
        self.condition = condition
        self.true_body = []
        self.false_body = []
    
    def __repr__(self):
        return f"IRIf({self.condition})"


class IRSwap(IRStatement):
    """
    Представление обмена двух элементов массива в промежуточном представлении.
    
    Attributes:
        array_name (str): Имя массива
        idx1 (str): Индекс первого элемента
        idx2 (str): Индекс второго элемента
        highlight_elements (list): Индексы элементов для подсветки при визуализации
    """
    
    def __init__(self, array_name: str, idx1: str, idx2: str):
        """
        Инициализирует оператор обмена.
        
        Args:
            array_name (str): Имя массива
            idx1 (str): Индекс первого элемента
            idx2 (str): Индекс второго элемента
        """
        self.array_name = array_name
        self.idx1 = idx1
        self.idx2 = idx2
        self.highlight_elements = [idx1, idx2]
    
    def __repr__(self):
        return f"IRSwap({self.array_name}[{self.idx1}], {self.array_name}[{self.idx2}])"