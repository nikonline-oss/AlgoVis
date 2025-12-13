"""Базовый класс для модов"""
from typing import Dict, Any, List
from ..core.intermediate import Algorithm, Step, Function, Variable


class BaseMod:
    """Базовый класс для всех модов"""
    
    def __init__(self):
        self.name = self.__class__.__name__
        self.priority = 0  # Приоритет выполнения (меньше = раньше)
    
    def process(self, algorithm: Algorithm, **kwargs) -> Algorithm:
        """
        Основной метод обработки алгоритма.
        Может модифицировать algorithm in-place.
        """
        return algorithm
    
    def modify_variables(self, variables: List[Variable]) -> List[Variable]:
        """Модифицирует переменные"""
        return variables
    
    def modify_functions(self, functions: List[Function]) -> List[Function]:
        """Модифицирует функции"""
        return functions
    
    def modify_steps(self, steps: List[Step], context: str = "main") -> List[Step]:
        """Модифицирует шаги"""
        return steps
    
    def before_generation(self, algorithm: Algorithm) -> Algorithm:
        """Вызывается перед генерацией JSON"""
        return algorithm
    
    def after_generation(self, java_json: Dict[str, Any]) -> Dict[str, Any]:
        """Вызывается после генерации JSON"""
        return java_json
    
    def should_handle_function(self, func_name: str) -> bool:
        """Определяет, должен ли мод обрабатывать функцию"""
        return False
    
    def get_dependencies(self) -> List[str]:
        """Возвращает список зависимостей (имен других модов)"""
        return []