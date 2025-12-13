"""Пример пользовательского мода для сортировок"""
from src.mods.base_mod import BaseMod
from src.core.intermediate import Algorithm, Step, StepType, Variable


class CustomSortMod(BaseMod):
    """Мод для улучшения визуализации сортировок"""
    
    def __init__(self):
        super().__init__()
        self.priority = 10
    
    def process(self, algorithm: Algorithm, **kwargs) -> Algorithm:
        # Добавляем специальные переменные для сортировок
        if any("sort" in func.name.lower() for func in algorithm.functions):
            algorithm.variables.extend([
                Variable(
                    name="swapped",
                    type="bool",
                    initial_value=False,
                    description="Флаг обмена элементов"
                ),
                Variable(
                    name="temp",
                    type="int",
                    initial_value=0,
                    description="Временная переменная для обмена"
                )
            ])
        
        return algorithm
    
    def modify_steps(self, steps, context="main"):
        """Добавляем подсветку элементов при сравнении"""
        modified_steps = []
        
        for step in steps:
            if step.type == StepType.COMPARE:
                # Добавляем подсветку сравниваемых элементов
                if len(step.parameters) >= 2:
                    step.highlight_elements = step.parameters[:2]
                    step.highlight_color = "yellow"
            
            modified_steps.append(step)
        
        return modified_steps