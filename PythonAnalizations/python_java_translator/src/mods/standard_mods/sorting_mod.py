"""Стандартный мод для алгоритмов сортировки"""
from ...mods.base_mod import BaseMod
from ...core.intermediate import Algorithm, Step, StepType, Variable, Function


class SortingMod(BaseMod):
    """Мод для оптимизации визуализации алгоритмов сортировки"""
    
    def __init__(self):
        super().__init__()
        self.name = "SortingMod"
        self.priority = 5
        self.sorting_keywords = ["sort", "sorted", "bubble", "quick", "merge", 
                                "insertion", "selection", "heap", "shell"]
    
    def process(self, algorithm: Algorithm, **kwargs) -> Algorithm:
        """Обрабатывает алгоритм сортировки"""
        # Проверяем, является ли алгоритм сортировкой
        is_sorting = self._is_sorting_algorithm(algorithm)
        
        if not is_sorting:
            return algorithm
        
        print(f"[SortingMod] Обработка алгоритма сортировки: {algorithm.name}")
        
        # 1. Добавляем стандартные переменные для сортировок
        algorithm = self._add_sorting_variables(algorithm)
        
        # 2. Оптимизируем шаги для лучшей визуализации
        algorithm = self._optimize_sorting_steps(algorithm)
        
        # 3. Добавляем специальные метаданные
        algorithm.metadata["sorting_algorithm"] = True
        algorithm.metadata["optimized_by"] = "SortingMod"
        
        return algorithm
    
    def _is_sorting_algorithm(self, algorithm: Algorithm) -> bool:
        """Определяет, является ли алгоритм сортировкой"""
        # Проверяем название алгоритма
        name_lower = algorithm.name.lower()
        for keyword in self.sorting_keywords:
            if keyword in name_lower:
                return True
        
        # Проверяем описание
        desc_lower = algorithm.description.lower()
        for keyword in self.sorting_keywords:
            if keyword in desc_lower:
                return True
        
        # Проверяем имена функций
        for func in algorithm.functions:
            func_name_lower = func.name.lower()
            for keyword in self.sorting_keywords:
                if keyword in func_name_lower:
                    return True
        
        return False
    
    def _add_sorting_variables(self, algorithm: Algorithm) -> Algorithm:
        """Добавляет стандартные переменные для сортировок"""
        standard_vars = {
            "i": Variable("i", "int", 0, "Счетчик внешнего цикла"),
            "j": Variable("j", "int", 0, "Счетчик внутреннего цикла"),
            "temp": Variable("temp", "int", 0, "Временная переменная для обмена"),
            "swapped": Variable("swapped", "bool", False, "Флаг выполнения обмена"),
            "min_idx": Variable("min_idx", "int", 0, "Индекс минимального элемента"),
            "key": Variable("key", "int", 0, "Ключевой элемент для вставки")
        }
        
        # Добавляем только те переменные, которых еще нет
        existing_names = {v.name for v in algorithm.variables}
        for var_name, var in standard_vars.items():
            if var_name not in existing_names:
                algorithm.variables.append(var)
        
        return algorithm
    
    def _optimize_sorting_steps(self, algorithm: Algorithm) -> Algorithm:
        """Оптимизирует шаги сортировки для лучшей визуализации"""
        
        # Обрабатываем основные шаги
        algorithm.steps = self._optimize_step_list(algorithm.steps, "main")
        
        # Обрабатываем шаги в функциях
        for func in algorithm.functions:
            func.steps = self._optimize_step_list(func.steps, func.name)
        
        return algorithm
    
    def _optimize_step_list(self, steps: list, context: str) -> list:
        """Оптимизирует список шагов"""
        optimized_steps = []
        
        for step in steps:
            optimized_step = step
            
            # Для шагов сравнения добавляем подсветку
            if step.type == StepType.COMPARE:
                optimized_step = self._enhance_compare_step(step)
            
            # Для шагов обмена добавляем подсветку
            elif step.type == StepType.SWAP:
                optimized_step = self._enhance_swap_step(step)
            
            # Для условных шагов добавляем описания
            elif step.type == StepType.CONDITION:
                optimized_step = self._enhance_condition_step(step, context)
            
            # Для присваиваний счетчиков уменьшаем визуализацию
            elif step.type == StepType.ASSIGN:
                optimized_step = self._adjust_assignment_visualization(step)
            
            optimized_steps.append(optimized_step)
        
        return optimized_steps
    
    def _enhance_compare_step(self, step: Step) -> Step:
        """Улучшает шаг сравнения для визуализации"""
        if len(step.parameters) >= 2:
            # Добавляем подсветку сравниваемых элементов
            if not step.highlight_elements:
                step.highlight_elements = step.parameters[:2]
            
            if not step.highlight_color:
                step.highlight_color = "yellow"
        
        return step
    
    def _enhance_swap_step(self, step: Step) -> Step:
        """Улучшает шаг обмена для визуализации"""
        if len(step.parameters) >= 2:
            # Добавляем подсветку обмениваемых элементов
            if not step.highlight_elements:
                step.highlight_elements = step.parameters[:2]
            
            if not step.highlight_color:
                step.highlight_color = "red"
        
        return step
    
    def _enhance_condition_step(self, step: Step, context: str) -> Step:
        """Улучшает условный шаг"""
        return step
    
    def _adjust_assignment_visualization(self, step: Step) -> Step:
        """Настраивает визуализацию присваиваний"""
        # Счетчики циклов обычно не нужно визуализировать
        counter_vars = {"i", "j", "k", "counter", "index", "idx"}
        
        if step.parameters and len(step.parameters) > 0:
            var_name = step.parameters[0]
            if var_name in counter_vars:
                step.visualize = False
        
        return step
    
    def should_handle_function(self, func_name: str) -> bool:
        """Определяет, должен ли мод обрабатывать функцию"""
        func_name_lower = func_name.lower()
        return any(keyword in func_name_lower for keyword in self.sorting_keywords)