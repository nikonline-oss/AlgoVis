"""Конфигурация визуализации шагов"""
from typing import Callable, List, Set
from ..core.intermediate import Step, StepType


class VisualizationConfig:
    def __init__(self):
        self._filters: List[Callable[[Step], bool]] = []
        self._enabled_types: Set[StepType] = set()
        self._disabled_types: Set[StepType] = set()
    
    def enable_type(self, step_type: StepType):
        """Включает визуализацию для типа шага"""
        self._enabled_types.add(step_type)
        if step_type in self._disabled_types:
            self._disabled_types.remove(step_type)
    
    def disable_type(self, step_type: StepType):
        """Отключает визуализацию для типа шага"""
        self._disabled_types.add(step_type)
        if step_type in self._enabled_types:
            self._enabled_types.remove(step_type)
    
    def add_filter(self, filter_func: Callable[[Step], bool]):
        """Добавляет пользовательский фильтр"""
        self._filters.append(filter_func)
    
    def clear_filters(self):
        """Очищает все фильтры"""
        self._filters.clear()
        self._enabled_types.clear()
        self._disabled_types.clear()
    
    def should_visualize(self, step: Step) -> bool:
        """Определяет, нужно ли визуализировать шаг"""
        # Проверяем включенные типы
        if self._enabled_types and step.type not in self._enabled_types:
            return False
        
        # Проверяем отключенные типы
        if self._disabled_types and step.type in self._disabled_types:
            return False
        
        # Применяем пользовательские фильтры
        for filter_func in self._filters:
            if not filter_func(step):
                return False
        
        return True
    
    def apply_to_algorithm(self, algorithm):
        """Применяет конфигурацию к алгоритму"""
        # Основные шаги
        for step in algorithm.steps:
            step.visualize = self.should_visualize(step)
        
        # Шаги в функциях
        for func in algorithm.functions:
            for step in func.steps:
                step.visualize = self.should_visualize(step)
        
        return algorithm