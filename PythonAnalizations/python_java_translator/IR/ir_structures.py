# ir_structures.py

class IRClass:
    """
    Представление класса в промежуточном представлении (IR).
    
    Attributes:
        name (str): Название класса
        fields (set): Множество полей класса
        methods (dict): Словарь методов класса, где ключ - имя метода
    """
    
    def __init__(self, name):
        """
        Инициализирует класс IR.
        
        Args:
            name (str): Название класса
        """
        self.name = name
        self.fields = set()
        self.methods = {}


class IRMethod:
    """
    Представление метода/функции в промежуточном представлении (IR).
    
    Attributes:
        name (str): Название метода
        params (list): Список параметров метода
        body (list): Список операторов в теле метода
    """
    
    def __init__(self, name, params):
        """
        Инициализирует метод IR.
        
        Args:
            name (str): Название метода
            params (list): Список параметров метода
        """
        self.name = name
        self.params = params
        self.body = []