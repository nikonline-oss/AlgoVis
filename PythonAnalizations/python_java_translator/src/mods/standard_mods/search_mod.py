"""Стандартный мод для алгоритмов поиска"""
from ...mods.base_mod import BaseMod
from ...core.intermediate import Algorithm, Step, StepType, Variable, Function


class SearchMod(BaseMod):
    """Мод для оптимизации визуализации алгоритмов поиска"""
    
    def __init__(self):
        super().__init__()
        self.name = "SearchMod"
        self.priority = 5
        self.search_keywords = ["search", "find", "lookup", "binary", "linear", 
                               "bfs", "dfs", "traverse", "path"]
    
    def process(self, algorithm: Algorithm, **kwargs) -> Algorithm:
        """Обрабатывает алгоритм поиска"""
        # Проверяем, является ли алгоритм поиском
        is_search = self._is_search_algorithm(algorithm)
        
        if not is_search:
            return algorithm
        
        print(f"[SearchMod] Обработка алгоритма поиска: {algorithm.name}")
        
        # 1. Добавляем стандартные переменные для поиска
        algorithm = self._add_search_variables(algorithm)
        
        # 2. Оптимизируем шаги для лучшей визуализации
        algorithm = self._optimize_search_steps(algorithm)
        
        # 3. Добавляем специальные метаданные
        algorithm.metadata["search_algorithm"] = True
        algorithm.metadata["optimized_by"] = "SearchMod"
        
        return algorithm
    
    def _is_search_algorithm(self, algorithm: Algorithm) -> bool:
        """Определяет, является ли алгоритм поиском"""
        # Проверяем название алгоритма
        name_lower = algorithm.name.lower()
        for keyword in self.search_keywords:
            if keyword in name_lower:
                return True
        
        # Проверяем описание
        desc_lower = algorithm.description.lower()
        for keyword in self.search_keywords:
            if keyword in desc_lower:
                return True
        
        # Проверяем имена функций
        for func in algorithm.functions:
            func_name_lower = func.name.lower()
            for keyword in self.search_keywords:
                if keyword in func_name_lower:
                    return True
        
        return False
    
    def _add_search_variables(self, algorithm: Algorithm) -> Algorithm:
        """Добавляет стандартные переменные для поиска"""
        standard_vars = {
            "left": Variable("left", "int", 0, "Левая граница поиска"),
            "right": Variable("right", "int", 0, "Правая граница поиска"),
            "mid": Variable("mid", "int", 0, "Середина диапазона поиска"),
            "found": Variable("found", "bool", False, "Флаг найденного элемента"),
            "position": Variable("position", "int", -1, "Позиция найденного элемента"),
            "target": Variable("target", "int", 0, "Искомый элемент"),
            "current": Variable("current", "int", 0, "Текущий элемент")
        }
        
        # Добавляем только те переменные, которых еще нет
        existing_names = {v.name for v in algorithm.variables}
        for var_name, var in standard_vars.items():
            if var_name not in existing_names:
                algorithm.variables.append(var)
        
        return algorithm
    
    def _optimize_search_steps(self, algorithm: Algorithm) -> Algorithm:
        """Оптимизирует шаги поиска для лучшей визуализации"""
        
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
            
            # Для условных шагов добавляем описания
            elif step.type == StepType.CONDITION:
                optimized_step = self._enhance_condition_step(step, context)
            
            # Для присваиваний границ поиска
            elif step.type == StepType.ASSIGN:
                optimized_step = self._adjust_search_assignment(step)
            
            optimized_steps.append(optimized_step)
        
        return optimized_steps
    
    def _enhance_compare_step(self, step: Step) -> Step:
        """Улучшает шаг сравнения для поиска"""
        if len(step.parameters) >= 2:
            # Для поиска обычно сравниваем с целевым элементом
            # Подсвечиваем только проверяемый элемент
            if not step.highlight_elements:
                step.highlight_elements = [step.parameters[1]]
            
            if not step.highlight_color:
                step.highlight_color = "blue"
            
            # Улучшаем описание
            if not step.description or step.description.startswith("Сравнение"):
                param1 = step.parameters[0] if len(step.parameters) > 0 else "?"
                param2 = step.parameters[1] if len(step.parameters) > 1 else "?"
                step.description = f"Сравнение {param1} с {param2}"
        
        return step
    
    def _enhance_condition_step(self, step: Step, context: str) -> Step:
        """Улучшает условный шаг для поиска"""
        if not step.description or step.description.startswith("Проверка"):
            if step.parameters and len(step.parameters) > 0:
                condition = step.parameters[0]
                
                # Определяем тип проверки
                if "==" in condition or "!=" in condition:
                    step.description = f"Проверка совпадения элемента: {condition}"
                elif "<" in condition or ">" in condition or "<=" in condition or ">=" in condition:
                    step.description = f"Проверка границ поиска: {condition}"
                else:
                    step.description = f"Проверка условия поиска: {condition}"
        
        return step
    
    def _adjust_search_assignment(self, step: Step) -> Step:
        """Настраивает визуализацию присваиваний для поиска"""
        # Переменные границ поиска обычно нужно визуализировать
        search_vars = {"left", "right", "mid", "position", "found"}
        
        if step.parameters and len(step.parameters) > 0:
            var_name = step.parameters[0]
            if var_name in search_vars:
                step.visualize = True
                # Добавляем подсветку
                if not step.highlight_elements:
                    step.highlight_elements = [var_name]
                if not step.highlight_color:
                    step.highlight_color = "green"
        
        return step
    
    def should_handle_function(self, func_name: str) -> bool:
        """Определяет, должен ли мод обрабатывать функцию"""
        func_name_lower = func_name.lower()
        return any(keyword in func_name_lower for keyword in self.search_keywords)