"""
Основной транслятор Python -> ЯВА с настройками визуализации
"""

import ast
import json
import re
from typing import Dict, List, Any, Optional, Callable, Union, Set
import importlib.util
import sys

from .visualization_config import VisualizationConfig
from .data_structures import VariableInfo, StepInfo
from .variable_collector import VariableCollector

class JavaTranslator:
    """Транслятор Python -> ЯВА с настройки визуализации"""
    
    def __init__(self, visualization_config: Optional[VisualizationConfig] = None):
        self.variables: Dict[str, VariableInfo] = {}
        self.steps: List[StepInfo] = []
        self.functions: Dict[str, Any] = {}
        self.current_function: Optional[str] = None
        self.step_counter = 0
        self.step_map: Dict[str, StepInfo] = {}
        self.mods: Dict[str, Dict[str, Callable]] = {}
        self.structure_type = "array"
        self.custom_mods_loaded = False
        
        # Конфигурация визуализации
        self.visualization_config = visualization_config or VisualizationConfig()
        
        # Загрузка стандартных модов
        self._load_standard_mods()
    
    def _load_standard_mods(self):
        """Загрузка стандартных модов"""
        # Базовые математические функции (возвращают выражения)
        math_mod = {
            'sqrt': lambda *args: self._create_expression_result(f'sqrt({self._process_args_for_java(args)})'),
            'abs': lambda *args: self._create_expression_result(f'abs({self._process_args_for_java(args)})'),
            'min': lambda *args: self._create_expression_result(f'min({self._process_args_for_java(args, sep=", ")})'),
            'max': lambda *args: self._create_expression_result(f'max({self._process_args_for_java(args, sep=", ")})'),
            'pow': lambda x, y: self._create_expression_result(f'pow({x}, {y})'),
            'len': lambda x: self._create_expression_result(f'length({x})')
        }
        
        # Строковые функции
        string_mod = {
            'substring': lambda s, start, length: self._create_expression_result(f'substring({s}, {start}, {length})'),
            'toupper': lambda s: self._create_expression_result(f'toupper({s})'),
            'tolower': lambda s: self._create_expression_result(f'tolower({s})'),
            'concat': lambda *args: self._create_expression_result(f'concat({self._process_args_for_java(args, sep=", ")})')
        }
        
        self.mods['math'] = math_mod
        self.mods['string'] = string_mod
    
    def set_visualization_config(self, config: VisualizationConfig):
        """Установка конфигурации визуализации"""
        self.visualization_config = config
    
    def update_visualization_flags(self, **kwargs):
        """Обновление флагов визуализации"""
        for key, value in kwargs.items():
            if hasattr(self.visualization_config, key):
                setattr(self.visualization_config, key, value)
    
    def _create_expression_result(self, expr: str):
        """Создание результата в виде выражения"""
        return {'type': 'expression', 'data': expr}
    
    def _process_args_for_java(self, args, sep=", "):
        """Обработка аргументов для ЯВА"""
        processed = []
        for arg in args:
            if isinstance(arg, str):
                if (arg.startswith('"') and arg.endswith('"')) or (arg.startswith("'") and arg.endswith("'")):
                    content = arg[1:-1]
                    content = content.replace("'", "\\'")
                    processed.append(f"'{content}'")
                else:
                    processed.append(arg)
            else:
                processed.append(str(arg))
        return sep.join(processed)
    
    def _generate_step_id(self, prefix: str = "step") -> str:
        """Генерация уникального ID шага"""
        self.step_counter += 1
        return f"{prefix}_{self.step_counter}"
    
    def _python_type_to_java(self, py_type: type) -> str:
        """Преобразование типа Python в тип ЯВА"""
        type_map = {
            int: 'int',
            float: 'float',
            bool: 'bool',
            str: 'string',
            list: 'array',
            dict: 'object',
            tuple: 'array'
        }
        return type_map.get(py_type, 'object')
    
    def _extract_and_initialize_variables(self, code: str):
        """
        Извлечение и автоматическая инициализация переменных из кода Python
        """
        tree = ast.parse(code)
        
        collector = VariableCollector()
        collector.visit(tree)
        
        # Все переменные, которые используются или присваиваются
        all_vars = collector.assigned_vars.union(collector.used_vars)
        
        # Удаляем системные переменные и struct
        if 'struct' in all_vars:
            all_vars.remove('struct')
        
        # Для каждой переменной определяем тип и значение по умолчанию
        for var_name in all_vars:
            if var_name not in self.variables:
                var_type = self._guess_variable_type(var_name, code, collector)
                initial_value = self._get_default_value_for_type(var_type)
                
                self.variables[var_name] = VariableInfo(
                    name=var_name,
                    type=var_type,
                    initial_value=initial_value
                )
    
    def _guess_variable_type(self, var_name: str, code: str, collector: VariableCollector) -> str:
        """
        Попытка определить тип переменной по контексту использования
        """
        # Если переменная используется в строковых операциях
        if self._is_used_in_string_context(var_name, code):
            return 'string'
        
        # Если переменная используется в логических операциях
        if self._is_used_in_boolean_context(var_name, code):
            return 'bool'
        
        # Если имя переменной содержит hint о типе
        name_lower = var_name.lower()
        if any(hint in name_lower for hint in ['str', 'text', 'name', 'message', 'word', 'char', 'output']):
            return 'string'
        elif any(hint in name_lower for hint in ['count', 'num', 'index', 'idx', 'size', 'len', 'i', 'j', 'k']):
            return 'int'
        elif any(hint in name_lower for hint in ['flag', 'is_', 'has_', 'enable', 'disable', 'active']):
            return 'bool'
        elif any(hint in name_lower for hint in ['arr', 'list', 'array', 'items', 'elements']):
            return 'array'
        elif any(hint in name_lower for hint in ['obj', 'dict', 'map', 'node', 'graph', 'tree']):
            return 'object'
        
        # По умолчанию - int
        return 'int'
    
    def _is_used_in_string_context(self, var_name: str, code: str) -> bool:
        """Проверяет, используется ли переменная в строковом контексте"""
        patterns = [
            f"['\"].*?['\"]\\s*\\+\\s*{var_name}",
            f"{var_name}\\s*\\+\\s*['\"].*?['\"]",
            f"concat\\(.*?{var_name}.*?\\)",
        ]
        
        for pattern in patterns:
            if re.search(pattern, code):
                return True
        
        return False
    
    def _is_used_in_boolean_context(self, var_name: str, code: str) -> bool:
        """Проверяет, используется ли переменная в логическом контексте"""
        patterns = [
            f"{var_name}\\s*(==|!=|>|<|>=|<=)\\s*",
            f"\\s*(==|!=|>|<|>=|<=)\\s*{var_name}",
            f"{var_name}\\s*(&&|\\|\\|)",
            f"(&&|\\|\\|)\\s*{var_name}",
            f"!{var_name}",
            f"if\\s*\\(.*?{var_name}.*?\\)",
            f"while\\s*\\(.*?{var_name}.*?\\)",
        ]
        
        for pattern in patterns:
            if re.search(pattern, code):
                return True
        
        return False
    
    def _get_default_value_for_type(self, var_type: str) -> Any:
        """Возвращает значение по умолчанию для типа ЯВА"""
        defaults = {
            'int': 0,
            'float': 0.0,
            'bool': False,
            'string': '',
            'array': [],
            'object': {}
        }
        return defaults.get(var_type, 0)
    
    def _translate_expression(self, expr: str) -> str:
        """
        Трансляция выражения Python в выражение ЯВА
        Важно: удаляет все вызовы str(), так как в ЯВА преобразование автоматическое
        """
        if not expr:
            return expr
        
        # Шаг 1: Удаляем все вызовы str() - они не нужны в ЯВА
        expr = self._remove_str_calls(expr)
        
        # Шаг 2: Замена операторов
        replacements = {
            '**': '^',
            ' and ': ' && ',
            ' or ': ' || ',
            ' not ': '!',
            'True': 'true',
            'False': 'false',
            'None': 'null',
            '#': '//'  # Комментарии в Python
        }
        
        for py_op, java_op in replacements.items():
            expr = expr.replace(py_op, java_op)
        
        # Шаг 3: Обработка строковых литералов для ЯВА (одинарные кавычки)
        expr = self._convert_string_literals_to_java(expr)
        
        return expr
    
    def _remove_str_calls(self, expr: str) -> str:
        """
        Удаляет все вызовы str() из выражения
        В ЯВА преобразование в строку происходит автоматически при конкатенации
        """
        # Удаляем str(аргумент) -> оставляем только аргумент
        # Обрабатываем вложенные вызовы рекурсивно
        
        def replace_str(match):
            # Найден вызов str(...)
            content = match.group(1)  # Содержимое внутри скобок
            # Рекурсивно обрабатываем вложенные вызовы str
            content = self._remove_str_calls(content)
            return content
        
        # Регулярное выражение для поиска str(...)
        # Обрабатывает вложенные скобки до 3 уровней (достаточно для большинства случаев)
        pattern = r'str\(([^()]*(?:\([^()]*(?:\([^()]*\)[^()]*)*\)[^()]*)*)\)'
        
        # Применяем замену пока есть вызовы str
        while 'str(' in expr:
            new_expr = re.sub(pattern, replace_str, expr)
            if new_expr == expr:
                break
            expr = new_expr
        
        return expr
    
    def _convert_string_literals_to_java(self, expr: str) -> str:
        """Конвертация строковых литералов Python в формат ЯВА (одинарные кавычки)"""
        # Паттерн для поиска строк в двойных кавычках
        double_quote_pattern = r'"(?:[^"\\]|\\.)*"'
        
        def replace_double(match):
            content = match.group(0)[1:-1]  # Убираем внешние кавычки
            # Экранируем одинарные кавычки внутри
            content = content.replace("'", "\\'")
            return f"'{content}'"
        
        # Паттерн для поиска строк в одинарных кавычках Python
        single_quote_pattern = r"'(?:[^'\\]|\\.)*'"
        
        def replace_single(match):
            content = match.group(0)[1:-1]  # Убираем внешние кавычки
            # Уже в одинарных, но нужно проверить экранирование
            if "'" in content and "\\'" not in content:
                # Если есть неэкранированные одинарные кавычки
                content = content.replace("'", "\\'")
            return f"'{content}'"
        
        # Сначала заменяем двойные кавычки
        expr = re.sub(double_quote_pattern, replace_double, expr)
        # Затем одинарные (уже конвертированные и оригинальные)
        expr = re.sub(single_quote_pattern, replace_single, expr)
        
        return expr
    
    def _create_step(self, step_type: str, description: str, parameters: List[Any], 
                    nextStep: Optional[str] = None, **kwargs) -> StepInfo:
        """Создание шага с учетом конфигурации визуализации"""
        
        # Извлекаем visualize из kwargs, если передан явно
        visualize_explicit = kwargs.pop('visualize', None)
        
        # Определяем, нужно ли визуализировать этот шаг
        if visualize_explicit is not None:
            visualize = visualize_explicit
        else:
            visualize = self.visualization_config.should_visualize(step_type, description)
        
        # Получаем цвет подсветки
        highlight_color = self.visualization_config.get_highlight_color(step_type)
        
        # Определяем элементы для подсветки (для assign - имя переменной)
        highlight_elements = None
        if step_type == "assign" and parameters and len(parameters) > 0:
            highlight_elements = [parameters[0]]
        
        # Извлекаем другие специальные параметры
        condition_cases = kwargs.pop('conditionCases', None)
        function_name = kwargs.pop('functionName', None)
        function_parameters = kwargs.pop('functionParameters', None)
        return_to_step = kwargs.pop('returnToStep', None)
        
        return StepInfo(
            id=self._generate_step_id(step_type),
            type=step_type,
            description=description,
            parameters=parameters,
            nextStep=nextStep,
            conditionCases=condition_cases,
            functionName=function_name,
            functionParameters=function_parameters,
            returnToStep=return_to_step,
            visualize=visualize,
            highlightElements=highlight_elements,
            highlightColor=highlight_color
        )
    
    def _translate_assignment(self, var_name: str, value_expr: str, description: str = "") -> StepInfo:
        """Трансляция операции присваивания"""
        java_expr = self._translate_expression(value_expr)
        
        return self._create_step(
            step_type="assign",
            description=description or f"Присвоение {var_name} = {value_expr}",
            parameters=[var_name, java_expr]
        )
    
    def _translate_condition(self, condition_expr: str, 
                           true_steps: List[StepInfo], 
                           false_steps: List[StepInfo],
                           description: str = "") -> StepInfo:
        """Трансляция условия"""
        java_condition = self._translate_expression(condition_expr)
        
        # Генерация ID для веток
        true_branch_id = true_steps[0].id if true_steps else "end"
        false_branch_id = false_steps[0].id if false_steps else "end"
        
        return self._create_step(
            step_type="condition",
            description=description or f"Проверка условия: {condition_expr}",
            parameters=[java_condition],
            conditionCases=[
                {"condition": "true", "nextStep": true_branch_id},
                {"condition": "false", "nextStep": false_branch_id}
            ]
        )
    
    def _translate_while_loop(self, condition_expr: str, 
                            body_steps: List[StepInfo],
                            description: str = "") -> List[StepInfo]:
        """Трансляция цикла while"""
        java_condition = self._translate_expression(condition_expr)
        
        # Шаг проверки условия
        check_step = self._create_step(
            step_type="condition",
            description=description or f"Проверка условия цикла: {condition_expr}",
            parameters=[java_condition],
            conditionCases=[
                {"condition": "true", "nextStep": ""},  # Заполнится позже
                {"condition": "false", "nextStep": "end"}
            ]
        )
        
        # Связывание шагов
        if body_steps:
            check_step.conditionCases[0]["nextStep"] = body_steps[0].id
            # Последний шаг тела цикла должен вернуться к проверке условия
            body_steps[-1].nextStep = check_step.id
        else:
            # Пустое тело цикла - бесконечный цикл
            check_step.conditionCases[0]["nextStep"] = check_step.id
        
        return [check_step] + body_steps
    
    def _translate_for_loop(self, var_name: str, iterable_expr: str,
                          body_steps: List[StepInfo],
                          description: str = "") -> List[StepInfo]:
        """Трансляция цикла for (только для range)"""
        # Инициализация переменной цикла
        init_step = self._translate_assignment(
            var_name, 
            "0", 
            f"Инициализация счетчика цикла {var_name}"
        )
        
        # Шаг проверки условия
        condition_step = self._create_step(
            step_type="condition",
            description=f"Проверка {var_name} < {iterable_expr}",
            parameters=[f"{var_name} < {iterable_expr}"],
            conditionCases=[
                {"condition": "true", "nextStep": ""},  # Заполнится позже
                {"condition": "false", "nextStep": "end"}
            ]
        )
        
        # Шаг инкремента
        increment_step = self._translate_assignment(
            var_name,
            f"{var_name} + 1",
            f"Инкремент {var_name}"
        )
        
        # Связывание шагов
        if body_steps:
            condition_step.conditionCases[0]["nextStep"] = body_steps[0].id
            # Последний шаг тела цикла ведет к инкременту
            body_steps[-1].nextStep = increment_step.id
        else:
            condition_step.conditionCases[0]["nextStep"] = increment_step.id
        
        # Инкремент ведет обратно к проверке условия
        increment_step.nextStep = condition_step.id
        
        return [init_step, condition_step] + body_steps + [increment_step]
    
    def _add_start_and_end_steps(self, steps: List[StepInfo]) -> List[StepInfo]:
        """Добавление обязательных шагов start и end"""
        if not steps:
            # Минимальный алгоритм с start и end
            return [
                self._create_step(
                    step_type="generic",
                    description="Начало алгоритма",
                    parameters=[],
                    nextStep="end"
                ),
                self._create_step(
                    step_type="generic",
                    description="Алгоритм завершен",
                    parameters=[]
                )
            ]
        
        # Переименовываем первый шаг в start
        first_step = steps[0]
        first_step.id = "start"
        
        # Удаляем nextStep у последнего шага, если он есть
        for step in reversed(steps):
            if step.id != "start":  # Пропускаем начальный шаг
                # Находим последний шаг (не condition и не имеющий nextStep, который ведет на существующий шаг)
                if step.type != "condition":
                    # Проверяем, ведет ли nextStep на существующий шаг
                    if step.nextStep and step.nextStep in [s.id for s in steps]:
                        # Этот шаг не последний, продолжаем искать
                        continue
                
                # Это последний шаг, заменяем его id на end и убираем nextStep
                if step.id != "end":
                    # Сохраняем старый ID для перелинковки
                    old_id = step.id
                    step.id = "end"
                    
                    # Обновляем все ссылки на старый ID
                    for s in steps:
                        if s.nextStep == old_id:
                            s.nextStep = "end"
                        if s.conditionCases:
                            for case in s.conditionCases:
                                if case.get("nextStep") == old_id:
                                    case["nextStep"] = "end"
                
                # Убираем nextStep у end
                step.nextStep = None
                break
        
        # Если end не найден, добавляем его
        if not any(step.id == "end" for step in steps):
            end_step = self._create_step(
                step_type="generic",
                description="Алгоритм завершен",
                parameters=[]
            )
            end_step.id = "end"
            
            # Находим последний шаг и связываем его с end
            for step in reversed(steps):
                if step.type != "condition" and step.id != "start":
                    if not step.nextStep:
                        step.nextStep = "end"
                    break
            
            steps.append(end_step)
        
        return steps
    
    def _ensure_proper_links(self, steps: List[StepInfo]):
        """Обеспечение правильных связей между шагами"""
        step_ids = {step.id for step in steps}
        
        for step in steps:
            # Проверяем nextStep
            if step.nextStep and step.nextStep not in step_ids:
                # Если ссылка ведет на несуществующий шаг, перенаправляем на end
                step.nextStep = "end"
            
            # Проверяем conditionCases
            if step.conditionCases:
                for case in step.conditionCases:
                    if case.get("nextStep") and case["nextStep"] not in step_ids:
                        case["nextStep"] = "end"
    
    def translate_python_code(self, code: str, algorithm_name: str = "Algorithm") -> Dict:
        """
        Основной метод трансляции кода Python в ЯВА
        
        Args:
            code: Код Python для трансляции
            algorithm_name: Название алгоритма
            
        Returns:
            Словарь с описанием алгоритма в формате ЯВА
        """
        # Извлекаем и инициализируем переменные
        self._extract_and_initialize_variables(code)
        
        # Парсим код для трансляции операций
        tree = ast.parse(code)
        
        # Обрабатываем AST
        translated_steps = self._process_ast_node(tree)
        
        # Связываем шаги
        self._link_steps(translated_steps)
        
        # Добавляем обязательные шаги start и end
        translated_steps = self._add_start_and_end_steps(translated_steps)
        
        # Обеспечиваем правильные связи
        self._ensure_proper_links(translated_steps)
        
        # Создаем финальный JSON
        return {
            "name": algorithm_name,
            "description": f"Алгоритм, сгенерированный из Python кода",
            "structureType": self.structure_type,
            "variables": [
                {
                    "name": var.name,
                    "type": var.type,
                    "initialValue": var.initial_value
                }
                for var in self.variables.values()
            ],
            "functions": [],
            "steps": [step.to_dict() for step in translated_steps]
        }
    
    def _process_ast_node(self, node: ast.AST) -> List[StepInfo]:
        """Обработка узла AST и генерация шагов ЯВА"""
        steps = []
        
        if isinstance(node, ast.Module):
            for stmt in node.body:
                steps.extend(self._process_ast_node(stmt))
                
        elif isinstance(node, ast.Assign):
            for target in node.targets:
                if isinstance(target, ast.Name):
                    value_str = ast.unparse(node.value)
                    step = self._translate_assignment(
                        target.id,
                        value_str,
                        f"Присвоение {target.id} = {value_str}"
                    )
                    steps.append(step)
                    
        elif isinstance(node, ast.If):
            test_expr = ast.unparse(node.test)
            
            # Обрабатываем then-ветку
            true_steps = []
            for stmt in node.body:
                true_steps.extend(self._process_ast_node(stmt))
            
            # Обрабатываем else-ветку
            false_steps = []
            for stmt in node.orelse:
                false_steps.extend(self._process_ast_node(stmt))
            
            # Если else-ветка пустая, добавляем переход на следующий шаг
            if not false_steps:
                false_steps = [self._create_step(
                    step_type="generic",
                    description="Переход к следующему шагу",
                    parameters=[],
                    visualize=False
                )]
            
            condition_step = self._translate_condition(
                test_expr,
                true_steps,
                false_steps,
                f"Условие: {test_expr}"
            )
            
            steps.append(condition_step)
            steps.extend(true_steps)
            steps.extend(false_steps)
            
        elif isinstance(node, ast.While):
            test_expr = ast.unparse(node.test)
            
            # Обрабатываем тело цикла
            body_steps = []
            for stmt in node.body:
                body_steps.extend(self._process_ast_node(stmt))
            
            loop_steps = self._translate_while_loop(
                test_expr,
                body_steps,
                f"Цикл while: {test_expr}"
            )
            
            steps.extend(loop_steps)
            
        elif isinstance(node, ast.For):
            # Упрощенная обработка for (только для range)
            if isinstance(node.iter, ast.Call) and hasattr(node.iter.func, 'id') and node.iter.func.id == 'range':
                args = node.iter.args
                if len(args) == 1:
                    iter_expr = ast.unparse(args[0])
                elif len(args) == 2:
                    # Если range(a, b), то цикл от a до b-1
                    iter_expr = ast.unparse(args[1])
                    # Нужно инициализировать переменную значением a
                    var_name = node.target.id
                    init_step = self._translate_assignment(
                        var_name,
                        ast.unparse(args[0]),
                        f"Инициализация {var_name}"
                    )
                    steps.append(init_step)
                else:
                    iter_expr = "10"  # Значение по умолчанию
                    
                var_name = node.target.id
                
                body_steps = []
                for stmt in node.body:
                    body_steps.extend(self._process_ast_node(stmt))
                
                loop_steps = self._translate_for_loop(
                    var_name,
                    iter_expr,
                    body_steps,
                    f"Цикл for {var_name} in range({iter_expr})"
                )
                
                steps.extend(loop_steps)
            else:
                # Для других итераторов создаем общий цикл
                steps.append(self._create_step(
                    step_type="generic",
                    description=f"Цикл for по {ast.unparse(node.iter)}",
                    parameters=[],
                    visualize=True
                ))
                
        elif isinstance(node, ast.Expr) and isinstance(node.value, ast.Call):
            # Вызов функции
            func_name = ast.unparse(node.value.func)
            args = [ast.unparse(arg) for arg in node.value.args]
            
            # Пытаемся обработать через моды
            mod_steps = self._handle_mod_function(func_name, args)
            
            if mod_steps:
                # Мод обработал вызов функции
                steps.extend(mod_steps)
            else:
                # Стандартная обработка
                step = self._create_step(
                    step_type="generic",
                    description=f"Вызов функции {func_name}",
                    parameters=[f"{func_name}({', '.join(args)})"],
                    visualize=True
                )
                steps.append(step)
        
        return steps
    
    def _handle_mod_function(self, func_name: str, args: List[str]) -> Optional[List[StepInfo]]:
        """Обработка вызова функции через моды"""
        # Ищем функцию во всех модах
        for mod_name, mod_handlers in self.mods.items():
            if func_name in mod_handlers:
                handler = mod_handlers[func_name]
                try:
                    result = handler(*args)
                    if isinstance(result, dict) and result.get('type') == 'expression':
                        # Создаем шаг с выражением
                        step = self._create_step(
                            step_type="generic",
                            description=f"Вызов функции {func_name}",
                            parameters=[result['data']],
                            visualize=True
                        )
                        return [step]
                except Exception as e:
                    print(f"Ошибка в обработчике мода {func_name}: {e}")
                    return None
        
        return None
    
    def _link_steps(self, steps: List[StepInfo]):
        """Связывание шагов через nextStep"""
        for i in range(len(steps) - 1):
            current_step = steps[i]
            next_step = steps[i + 1]
            
            # Если у текущего шага нет явного nextStep и он не является условием
            if (not current_step.nextStep and 
                current_step.type != 'condition' and 
                current_step.id != "end"):
                current_step.nextStep = next_step.id
            
            # Сохраняем в мап
            self.step_map[current_step.id] = current_step
        
        # Обрабатываем последний шаг
        if steps:
            last_step = steps[-1]
            if (not last_step.nextStep and 
                last_step.type != 'condition' and 
                last_step.id != "end"):
                # Последний шаг будет перенаправлен на end позже
                pass
                
            self.step_map[last_step.id] = last_step
    
    def save_to_file(self, java_data: Dict, filename: str):
        """Сохранение результата в файл"""
        with open(filename, 'w', encoding='utf-8') as f:
            json.dump(java_data, f, ensure_ascii=False, indent=2)