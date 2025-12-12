"""
Основной транслятор Python -> ЯВА с настройками визуализации
"""

import ast
import json
import re
import time
from typing import Dict, List, Any, Optional, Callable, Union, Set
import importlib.util
import sys
from datetime import datetime

try:
    # Попробуем относительные импорты (работает в пакете)
    from .visualization_config import VisualizationConfig
    from .translation_config import TranslationConfig
    from .structure_registry import StructureRegistry
    from .error_handler import ErrorHandler, ErrorType, TranslationError
    from .data_structures import VariableInfo, StepInfo
    from .variable_collector import VariableCollector
except ImportError:
    # Если не работает, используем абсолютные (для запуска как скрипта)
    from visualization_config import VisualizationConfig
    from translation_config import TranslationConfig
    from structure_registry import StructureRegistry
    from error_handler import ErrorHandler, ErrorType, TranslationError
    from data_structures import VariableInfo, StepInfo
    from variable_collector import VariableCollector

class JavaTranslator:
    """Транслятор Python -> ЯВА с настройками визуализации"""
    
    def __init__(self, 
                 visualization_config: Optional[VisualizationConfig] = None,
                 translation_config: Optional[TranslationConfig] = None):
        self.variables: Dict[str, VariableInfo] = {}
        self.steps: List[StepInfo] = []
        self.functions: Dict[str, Any] = {}
        self.current_function: Optional[str] = None
        self.step_counter = 0
        self.step_map: Dict[str, StepInfo] = {}
        self.mods: Dict[str, Dict[str, Callable]] = {}
        self.structure_type = "array"
        self.custom_mods_loaded = False
        
        # Конфигурации
        self.visualization_config = visualization_config or VisualizationConfig()
        self.translation_config = translation_config or TranslationConfig()
        
        # Реестр структур
        self.structure_registry = StructureRegistry()
        
        # Отслеживание состояния
        self.current_recursion_depth = 0
        self.start_time = time.time()
        
        # Загрузка стандартных модов
        self._load_standard_mods()
    
    def _load_standard_mods(self):
        """Загрузка стандартных модов"""
        # Базовые математические функции
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
    
    def _safe_parse(self, code: str) -> ast.Module:
        """
        Безопасный парсинг Python кода с проверками
        
        Args:
            code: Исходный код Python
            
        Returns:
            AST дерево
            
        Raises:
            TranslationError: При ошибках парсинга или безопасности
        """
        # Проверка размера
        if len(code) > self.translation_config.max_code_size:
            raise TranslationError(
                f"Код слишком большой ({len(code)} > {self.translation_config.max_code_size} символов)",
                ErrorType.SIZE_LIMIT_ERROR
            )
        
        # Проверка на пустой код
        if not code.strip():
            raise TranslationError("Пустой код", ErrorType.SYNTAX_ERROR)
        
        # Проверка безопасности
        if self.translation_config.safe_mode:
            code_lower = code.lower()
            for keyword in self.translation_config.blacklisted_keywords:
                if keyword in code_lower:
                    raise TranslationError(
                        f"Запрещенная конструкция: {keyword}",
                        ErrorType.SECURITY_ERROR
                    )
        
        try:
            return ast.parse(code)
        except SyntaxError as e:
            raise TranslationError(
                f"Синтаксическая ошибка: {str(e)}",
                ErrorType.SYNTAX_ERROR,
                {"lineno": e.lineno, "offset": e.offset}
            )
        except Exception as e:
            raise TranslationError(
                f"Ошибка парсинга: {str(e)}",
                ErrorType.UNKNOWN_ERROR
            )
    
    def set_visualization_config(self, config: VisualizationConfig):
        """Установка конфигурации визуализации"""
        self.visualization_config = config
    
    def set_translation_config(self, config: TranslationConfig):
        """Установка конфигурации трансляции"""
        self.translation_config = config
    
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
        
        # Проверка лимита шагов
        if self.step_counter > self.translation_config.max_steps:
            raise TranslationError(
                f"Превышено максимальное количество шагов ({self.translation_config.max_steps})",
                ErrorType.SIZE_LIMIT_ERROR
            )
        
        return f"{prefix}_{self.step_counter}"
    
    def _check_recursion_limit(self):
        """Проверка лимита рекурсии"""
        self.current_recursion_depth += 1
        if self.current_recursion_depth > self.translation_config.max_recursion_depth:
            raise TranslationError(
                f"Превышена максимальная глубина рекурсии ({self.translation_config.max_recursion_depth})",
                ErrorType.RECURSION_ERROR
            )
    
    def _release_recursion(self):
        """Освобождение уровня рекурсии"""
        self.current_recursion_depth -= 1
    
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
        struct не добавляется в список переменных!
        """
        self._check_recursion_limit()
    
        try:
            tree = ast.parse(code)
        
            collector = VariableCollector()
            collector.visit(tree)
        
            # Все переменные, которые используются или присваиваются
            all_vars = collector.assigned_vars.union(collector.used_vars)
        
            # Убираем struct из списка переменных - она всегда существует в ЯВА
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
        finally:
            self._release_recursion()

    def _process_ast_node(self, node: ast.AST) -> List[StepInfo]:
        """Обработка узла AST и генерация шагов ЯВА"""
        self._check_recursion_limit()
    
        try:
            steps = []
        
            if isinstance(node, ast.Module):
                for stmt in node.body:
                    steps.extend(self._process_ast_node(stmt))
                
            elif isinstance(node, ast.Assign):
                for target in node.targets:
                    if isinstance(target, ast.Name):
                        # Не создаем переменную для struct
                        if target.id == 'struct':
                            continue  # Пропускаем присваивания struct
                    
                        value_str = ast.unparse(node.value)
                        step = self._translate_assignment(
                            target.id,
                            value_str,
                            f"Присвоение {target.id} = {value_str}"
                        )
                        steps.append(step)
                    if isinstance(target, ast.Subscript):
                        value_str = ast.unparse(node.value)
                        target_name = ast.unparse(target)
                        step = self._translate_assignment(
                            target_name,
                            value_str,
                            f"Присвоение {target_name} = {value_str}"
                        )
                        steps.append(step)
                    
            elif isinstance(node, ast.If):
                test_expr = ast.unparse(node.test)
            
                true_steps = []
                for stmt in node.body:
                    true_steps.extend(self._process_ast_node(stmt))
            
                false_steps = []
                for stmt in node.orelse:
                    false_steps.extend(self._process_ast_node(stmt))
            
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
                if isinstance(node.iter, ast.Call) and hasattr(node.iter.func, 'id') and node.iter.func.id == 'range':
                    args = node.iter.args
                    if len(args) == 1:
                        iter_expr = ast.unparse(args[0])
                    elif len(args) == 2:
                        iter_expr = ast.unparse(args[1])
                        var_name = node.target.id
                        init_step = self._translate_assignment(
                            var_name,
                            ast.unparse(args[0]),
                            f"Инициализация {var_name}"
                        )
                        steps.append(init_step)
                    else:
                        iter_expr = "10"
                    
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
                    steps.append(self._create_step(
                        step_type="generic",
                        description=f"Цикл for по {ast.unparse(node.iter)}",
                        parameters=[],
                        visualize=True
                    ))
                    
            elif isinstance(node, ast.Expr) and isinstance(node.value, ast.Call):
                func_name = ast.unparse(node.value.func)
                args = [ast.unparse(arg) for arg in node.value.args]
            
                mod_steps = self._handle_mod_function(func_name, args)
            
                if mod_steps:
                    steps.extend(mod_steps)
                else:
                    step = self._create_step(
                        step_type="generic",
                        description=f"Вызов функции {func_name}",
                        parameters=[f"{func_name}({', '.join(args)})"],
                        visualize=True
                    )
                    steps.append(step)

            elif isinstance(node, ast.FunctionDef):
                # Обработка определения функции
                function_name = node.name
                self.current_function = function_name
        
                # Сохраняем информацию о функции
                self.functions[function_name] = {
                    'args': [arg.arg for arg in node.args.args],
                    'body': node.body
                }
        
                # Создаем шаг для определения функции
                step = self._create_step(
                    step_type="function_def",
                    description=f"Определение функции {function_name}",
                    parameters=[function_name, len(node.args.args)],
                    visualize=False  # Обычно не визуализируем определение
                )
                steps.append(step)
        
                # Обрабатываем тело функции
                for stmt in node.body:
                    steps.extend(self._process_ast_node(stmt))
        
                self.current_function = None
    
            elif isinstance(node, ast.Return):
                # Обработка return
                if node.value:
                    return_expr = ast.unparse(node.value)
                    step = self._create_step(
                        step_type="return",
                        description=f"Возврат значения: {return_expr}",
                        parameters=[self._translate_expression(return_expr)],
                        visualize=True
                    )
                else:
                    step = self._create_step(
                        step_type="return",
                        description="Возврат из функции",
                        parameters=[],
                        visualize=True
                    )
                steps.append(step)
        
            return steps
        
        finally:
            self._release_recursion()
    
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
        def replace_str(match):
            content = match.group(1)
            content = self._remove_str_calls(content)
            return content
        
        pattern = r'str\(([^()]*(?:\([^()]*(?:\([^()]*\)[^()]*)*\)[^()]*)*)\)'
        
        while 'str(' in expr:
            new_expr = re.sub(pattern, replace_str, expr, flags=re.DOTALL)
            if new_expr == expr:
                break
            expr = new_expr
        
        return expr
    
    def _convert_string_literals_to_java(self, expr: str) -> str:
        """Конвертация строковых литералов Python в формат ЯВА (одинарные кавычки)"""
        # Паттерн для поиска строк в двойных кавычках
        double_quote_pattern = r'"(?:[^"\\]|\\.)*"'
        
        def replace_double(match):
            content = match.group(0)[1:-1]
            content = content.replace("'", "\\'")
            content = content.replace('"', '\\"')
            return f"'{content}'"
        
        # Паттерн для поиска строк в одинарных кавычках Python
        single_quote_pattern = r"'(?:[^'\\]|\\.)*'"
        
        def replace_single(match):
            content = match.group(0)[1:-1]
            if "'" in content and "\\'" not in content:
                content = content.replace("'", "\\'")
            return f"'{content}'"
        
        expr = re.sub(double_quote_pattern, replace_double, expr)
        expr = re.sub(single_quote_pattern, replace_single, expr)
        
        return expr
    
    def _create_step(self, step_type: str, description: str, parameters: List[Any], 
                    nextStep: Optional[str] = None, **kwargs) -> StepInfo:
        """Создание шага с учетом конфигурации визуализации"""
        
        visualize_explicit = kwargs.pop('visualize', None)
        
        if visualize_explicit is not None:
            visualize = visualize_explicit
        else:
            visualize = self.visualization_config.should_visualize(step_type, description)
        
        highlight_color = self.visualization_config.get_highlight_color(step_type)
        
        highlight_elements = None
        if step_type == "assign" and parameters and len(parameters) > 0:
            highlight_elements = [parameters[0]]
        
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
        
        check_step = self._create_step(
            step_type="condition",
            description=description or f"Проверка условия цикла: {condition_expr}",
            parameters=[java_condition],
            conditionCases=[
                {"condition": "true", "nextStep": ""},
                {"condition": "false", "nextStep": "end"}
            ]
        )
        
        if body_steps:
            check_step.conditionCases[0]["nextStep"] = body_steps[0].id
            body_steps[-1].nextStep = check_step.id
        else:
            check_step.conditionCases[0]["nextStep"] = check_step.id
        
        return [check_step] + body_steps
    
    def _translate_for_loop(self, var_name: str, iterable_expr: str,
                          body_steps: List[StepInfo],
                          description: str = "") -> List[StepInfo]:
        """Трансляция цикла for (только для range)"""
        init_step = self._translate_assignment(
            var_name, 
            "0", 
            f"Инициализация счетчика цикла {var_name}"
        )
        
        condition_step = self._create_step(
            step_type="condition",
            description=f"Проверка {var_name} < {iterable_expr}",
            parameters=[f"{var_name} < {iterable_expr}"],
            conditionCases=[
                {"condition": "true", "nextStep": ""},
                {"condition": "false", "nextStep": "end"}
            ]
        )
        
        increment_step = self._translate_assignment(
            var_name,
            f"{var_name} + 1",
            f"Инкремент {var_name}"
        )
        
        if body_steps:
            condition_step.conditionCases[0]["nextStep"] = body_steps[0].id
            body_steps[-1].nextStep = increment_step.id
        else:
            condition_step.conditionCases[0]["nextStep"] = increment_step.id
        
        increment_step.nextStep = condition_step.id
        
        return [init_step, condition_step] + body_steps + [increment_step]
    
    def _add_start_and_end_steps(self, steps: List[StepInfo]) -> List[StepInfo]:
        """Добавление обязательных шагов start и end как новых шагов"""
    
        # Создаем шаг start
        start_step = StepInfo(
            id="start",
            type="generic",
            description="Начало алгоритма",
            parameters=[],
            nextStep=None,  # Будет установлено позже
            visualize=True  # Обязательно визуализируем
        )
    
        # Создаем шаг end
        end_step = StepInfo(
            id="end",
            type="generic",
            description="Алгоритм завершен",
            parameters=[],
            visualize=True  # Обязательно визуализируем
        )
    
        # Если нет реальных шагов (кроме start и end)
        if not steps:
            start_step.nextStep = "end"
            return [start_step, end_step]
    
        # Если есть реальные шаги
        # 1. Start ведет на первый реальный шаг
        start_step.nextStep = steps[0].id
    
        # 2. Находим последний реальный шаг и направляем его на end
        # Ищем шаг, который не ведет на другой существующий шаг
        last_real_step = None
        step_ids = {step.id for step in steps}
    
        for step in reversed(steps):
            # Пропускаем condition шаги, так как у них свои переходы
            if step.type == "condition":
                continue
        
            # Если у шага нет nextStep или nextStep ведет на несуществующий шаг
            if not step.nextStep or step.nextStep not in step_ids:
                last_real_step = step
                break
    
        if last_real_step:
            last_real_step.nextStep = "end"
        else:
            # Если не нашли подходящий шаг, последний шаг ведет на end
            steps[-1].nextStep = "end"
    
        # Возвращаем: start + реальные шаги + end
        return [start_step] + steps + [end_step]
    
    def _ensure_proper_links(self, steps: List[StepInfo]):
        """Обеспечение правильных связей между шагами"""
        step_ids = {step.id for step in steps}
    
        for step in steps:
            # Пропускаем start и end
            if step.id in ["start", "end"]:
                continue
            
            # Проверяем nextStep
            if step.nextStep and step.nextStep not in step_ids:
                # Если ссылка ведет на несуществующий шаг, перенаправляем на end
                step.nextStep = "end"
        
            # Проверяем conditionCases
            if step.conditionCases:
                for case in step.conditionCases:
                    if case.get("nextStep") and case["nextStep"] not in step_ids:
                        case["nextStep"] = "end"
    
    def translate_python_code(self, code: str, algorithm_name: str = "Algorithm", 
                          structure_type: str = None) -> Dict:
        """
        Основной метод трансляции кода Python в ЯВА
        """
        # Сброс состояния
        self._reset_translation_state()
    
        # Установка типа структуры
        self.structure_type = structure_type or self.translation_config.default_structure_type
    
        try:
            # Безопасный парсинг
            tree = self._safe_parse(code)
        
            # Извлекаем и инициализируем переменные (struct не добавляется!)
            self._extract_and_initialize_variables(code)
        
            # Обрабатываем AST
            translated_steps = self._process_ast_node(tree)
        
            # Связываем шаги
            self._link_steps(translated_steps)
        
            # Добавляем обязательные шаги start и end как новые шаги
            translated_steps = self._add_start_and_end_steps(translated_steps)
        
            # Обеспечиваем правильные связи
            self._ensure_proper_links(translated_steps)
        
            # Создаем финальный результат (struct не в переменных!)
            result = self._create_final_result(translated_steps, algorithm_name)
        
            # Валидация результата
            if self.translation_config.validate_output:
                validation_errors = ErrorHandler.validate_java_output(result)
                if validation_errors:
                    raise TranslationError(
                        f"Ошибки валидации: {', '.join(validation_errors)}",
                        ErrorType.VALIDATION_ERROR,
                        {"validation_errors": validation_errors}
                    )
        
            return result
        
        except TranslationError as e:
            return ErrorHandler.create_error_result(
                e.args[0],
                e.error_type,
                code
            )
        except Exception as e:
            return ErrorHandler.create_error_result(
                f"Непредвиденная ошибка: {str(e)}",
                ErrorType.UNKNOWN_ERROR,
                code
            )
    
    def _reset_translation_state(self):
        """Сброс состояния транслятора"""
        self.variables.clear()
        self.steps.clear()
        self.step_counter = 0
        self.step_map.clear()
        self.current_recursion_depth = 0
        self.start_time = time.time()
    
    def _process_ast_node(self, node: ast.AST) -> List[StepInfo]:
        """Обработка узла AST и генерация шагов ЯВА"""
        self._check_recursion_limit()
        
        try:
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
                    if isinstance(target, ast.Subscript):
                        value_str = ast.unparse(node.value)
                        target_name = ast.unparse(target)
                        step = self._translate_assignment(
                            target_name,
                            value_str,
                            f"Присвоение {target_name} = {value_str}"
                        )
                        steps.append(step)
                        
            elif isinstance(node, ast.If):
                test_expr = ast.unparse(node.test)
                
                true_steps = []
                for stmt in node.body:
                    true_steps.extend(self._process_ast_node(stmt))
                
                false_steps = []
                for stmt in node.orelse:
                    false_steps.extend(self._process_ast_node(stmt))
                
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
                if isinstance(node.iter, ast.Call) and hasattr(node.iter.func, 'id') and node.iter.func.id == 'range':
                    args = node.iter.args
                    if len(args) == 1:
                        iter_expr = ast.unparse(args[0])
                    elif len(args) == 2:
                        iter_expr = ast.unparse(args[1])
                        var_name = node.target.id
                        init_step = self._translate_assignment(
                            var_name,
                            ast.unparse(args[0]),
                            f"Инициализация {var_name}"
                        )
                        steps.append(init_step)
                    else:
                        iter_expr = "10"
                        
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
                    steps.append(self._create_step(
                        step_type="generic",
                        description=f"Цикл for по {ast.unparse(node.iter)}",
                        parameters=[],
                        visualize=True
                    ))
                    
            elif isinstance(node, ast.Expr) and isinstance(node.value, ast.Call):
                func_name = ast.unparse(node.value.func)
                args = [ast.unparse(arg) for arg in node.value.args]
                
                mod_steps = self._handle_mod_function(func_name, args)
                
                if mod_steps:
                    steps.extend(mod_steps)
                else:
                    step = self._create_step(
                        step_type="generic",
                        description=f"Вызов функции {func_name}",
                        parameters=[f"{func_name}({', '.join(args)})"],
                        visualize=True
                    )
                    steps.append(step)

            elif isinstance(node, ast.FunctionDef):
                # Обработка определения функции
                function_name = node.name
                self.current_function = function_name

                # Сохраняем информацию о функции
                self.functions[function_name] = {
                    'args': [arg.arg for arg in node.args.args],
                    'body': node.body
                }

                # Создаем шаг для определения функции
                step = self._create_step(
                    step_type="function_def",
                    description=f"Определение функции {function_name}",
                    parameters=[function_name, len(node.args.args)],
                    visualize=False  # Обычно не визуализируем определение
                )
                steps.append(step)

                # Обрабатываем тело функции
                for stmt in node.body:
                    steps.extend(self._process_ast_node(stmt))

                self.current_function = None

            elif isinstance(node, ast.Return):
                # Обработка return
                if node.value:
                    return_expr = ast.unparse(node.value)
                    step = self._create_step(
                        step_type="return",
                        description=f"Возврат значения: {return_expr}",
                        parameters=[self._translate_expression(return_expr)],
                        visualize=True
                    )
                else:
                    step = self._create_step(
                        step_type="return",
                        description="Возврат из функции",
                        parameters=[],
                        visualize=True
                    )
                steps.append(step)
            
            return steps
            
        finally:
            self._release_recursion()
    
    def _handle_mod_function(self, func_name: str, args: List[str]) -> Optional[List[StepInfo]]:
        """Обработка вызова функции через моды"""
        for mod_name, mod_handlers in self.mods.items():
            if func_name in mod_handlers:
                handler = mod_handlers[func_name]
                try:
                    result = handler(*args)
                    if isinstance(result, dict) and result.get('type') == 'expression':
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
            
            if (not current_step.nextStep and 
                current_step.type != 'condition' and 
                current_step.id != "end"):
                current_step.nextStep = next_step.id
            
            self.step_map[current_step.id] = current_step
        
        if steps:
            last_step = steps[-1]
            if (not last_step.nextStep and 
                last_step.type != 'condition' and 
                last_step.id != "end"):
                pass
                
            self.step_map[last_step.id] = last_step
    
    def _create_final_result(self, steps: List[StepInfo], algorithm_name: str) -> Dict:
        """Создание финального результата трансляции"""
        # struct не включается в список переменных!
        return {
            "name": algorithm_name,
            "description": f"Алгоритм, сгенерированный из Python кода",
            "structureType": self.structure_type,
            "variables": [  # Только пользовательские переменные, без struct
                {
                    "name": var.name,
                    "type": var.type,
                    "initialValue": var.initial_value
                }
                for var in self.variables.values()
                if var.name != "struct"  # На всякий случай
            ],
            "functions": [],
            "steps": [step.to_dict() for step in steps],
            "translationInfo": {
                "translationTime": time.time() - self.start_time,
                "stepCount": len(steps),
                "variableCount": len(self.variables),
                "timestamp": datetime.now().isoformat()
            }
        }
    
    def save_to_file(self, java_data: Dict, filename: str):
        """Сохранение результата в файл"""
        with open(filename, 'w', encoding='utf-8') as f:
            json.dump(java_data, f, ensure_ascii=False, indent=2)
    
    def add_custom_mod(self, mod_name: str, functions: Dict[str, Callable]):
        """Добавление пользовательского мода"""
        if mod_name not in self.mods:
            self.mods[mod_name] = {}
        
        self.mods[mod_name].update(functions)
    
    def register_structure_type(self, name: str, properties: Dict):
        """Регистрация нового типа структуры"""
        self.structure_registry.register_structure(name, properties)