"""Парсер Python AST в промежуточное представление ЯВА"""
import ast
from typing import Dict, List, Set, Tuple, Optional, Any
from .intermediate import *


class ASTParser:
    def __init__(self):
        self.algorithm = None
        self.current_function = None
        self.step_counter = 0
        self.variable_types = {}
        self.imported_modules = set()
        self.struct_name = "struct"
        self.label_stack = []  # Стек для отслеживания контекста (циклы, условия)
        
    def parse(self, python_code: str) -> Algorithm:
        """Парсит Python код в промежуточное представление"""
        tree = ast.parse(python_code)
        
        # Собираем информацию о модулях
        self._collect_imports(tree)
        
        # Ищем главную функцию алгоритма
        main_func = self._find_main_function(tree)
        
        if not main_func:
            raise ValueError("Не найдена основная функция алгоритма")
            
        # Создаем алгоритм
        self.algorithm = Algorithm(
            name=main_func.name,
            description=self._extract_docstring(main_func) or f"Алгоритм {main_func.name}",
            structure_type=self._detect_structure_type(tree),
            variables=[],
            functions=[],
            steps=[]
        )
        
        # Обрабатываем все функции
        for node in ast.walk(tree):
            if isinstance(node, ast.FunctionDef):
                self._process_function(node)
        
        # Обрабатываем глобальные переменные
        self._process_global_variables(tree)
        
        # Создаем начальный и конечный шаги
        self._create_start_end_steps()
        
        # Устанавливаем связи между шагами
        self._connect_steps()
        
        return self.algorithm
    
    def _process_function(self, func_node: ast.FunctionDef):
        """Обрабатывает функцию Python"""
        # Сбрасываем счетчик шагов для каждой функции
        self.step_counter = 0
        
        func = Function(
            name=func_node.name,
            description=self._extract_docstring(func_node) or f"Функция {func_node.name}",
            parameters=[arg.arg for arg in func_node.args.args],
            entry_point="",  # Будет установлен позже
            steps=[],
            return_type="void"
        )
        
        # Сохраняем текущую функцию
        prev_function = self.current_function
        self.current_function = func
        
        # Создаем начальный шаг функции
        start_step = Step(
            id=f"{func.name}_start",
            type=StepType.GENERIC,
            description=f"Начало функции {func.name}",
            visualize=False
        )
        func.steps.append(start_step)
        func.entry_point = start_step.id
        
        # Обрабатываем тело функции
        self._process_statements(func_node.body)
        
        # Добавляем return если нужно
        if not any(s.type == StepType.RETURN for s in func.steps):
            return_step = Step(
                id=f"{func.name}_return",
                type=StepType.RETURN,
                description=f"Возврат из функции {func.name}",
                visualize=False
            )
            func.steps.append(return_step)
        
        self.algorithm.functions.append(func)
        self.current_function = prev_function
        self.step_counter = 0
    
    def _process_statements(self, statements: List[ast.stmt]):
        """Обрабатывает список statements"""
        for i, stmt in enumerate(statements):
            # Пропускаем docstring
            if i == 0 and isinstance(stmt, ast.Expr) and isinstance(stmt.value, ast.Constant):
                continue
            self._process_statement(stmt)
    
    def _process_statement(self, stmt: ast.stmt):
        """Обрабатывает один statement"""
        if isinstance(stmt, ast.Assign):
            self._process_assignment(stmt)
        elif isinstance(stmt, ast.If):
            self._process_if(stmt)
        elif isinstance(stmt, ast.While):
            self._process_while(stmt)
        elif isinstance(stmt, ast.For):
            self._process_for(stmt)
        elif isinstance(stmt, ast.Expr):
            self._process_expression(stmt.value)
        elif isinstance(stmt, ast.Return):
            self._process_return(stmt)
        elif isinstance(stmt, ast.AugAssign):
            self._process_augmented_assignment(stmt)
        elif isinstance(stmt, ast.Pass):
            pass
        elif isinstance(stmt, ast.Break):
            self._process_break(stmt)
        elif isinstance(stmt, ast.Continue):
            self._process_continue(stmt)
        else:
            # Для неизвестных конструкций создаем generic step
            self._create_generic_step(f"Выполнение: {type(stmt).__name__}")
    
    def _process_assignment(self, assign: ast.Assign):
        """Обрабатывает присваивание"""
        for target in assign.targets:
            if isinstance(target, ast.Name):
                var_name = target.id
                value_expr = self._expression_to_string(assign.value)
                
                # Определяем тип переменной
                var_type = self._infer_type(assign.value)
                
                # Проверяем, не является ли это структурной переменной
                if var_name == self.struct_name:
                    continue  # struct обрабатывается отдельно
                
                # Добавляем переменную если еще не добавлена
                if not any(v.name == var_name for v in self.algorithm.variables):
                    # Получаем начальное значение
                    initial_value = self._get_initial_value(assign.value)
                    
                    # Форматируем для JSON
                    if var_type == "string":
                        if isinstance(initial_value, str):
                            if not (initial_value.startswith('"') and initial_value.endswith('"')):
                                initial_value = f'"{initial_value}"'
                        else:
                            initial_value = f'"{str(initial_value)}"'
                    elif var_type == "bool":
                        if isinstance(initial_value, str):
                            if initial_value.lower() == "true":
                                initial_value = True
                            elif initial_value.lower() == "false":
                                initial_value = False
                            else:
                                initial_value = False
                        elif not isinstance(initial_value, bool):
                            initial_value = False
                    
                    self.algorithm.variables.append(Variable(
                        name=var_name,
                        type=var_type,
                        initial_value=initial_value,
                        description=f"Переменная {var_name}"
                    ))
                
                # Создаем шаг присваивания
                step = Step(
                    id=self._generate_step_id("assign"),
                    type=StepType.ASSIGN,
                    description=f"Присвоение {var_name} = {value_expr}",
                    parameters=[var_name, value_expr],
                    visualize=self._should_visualize_assignment(var_name)
                )
                self._add_step(step)
            elif isinstance(target, ast.Subscript):
                # Обработка присваивания элементу массива: arr[i] = value
                value_expr = self._expression_to_string(assign.value)
                array_expr = self._expression_to_string(target.value)
                index_expr = self._expression_to_string(target.slice)
                
                step = Step(
                    id=self._generate_step_id("assign_array"),
                    type=StepType.ASSIGN,
                    description=f"Присвоение элементу массива {array_expr}[{index_expr}] = {value_expr}",
                    parameters=[f"{array_expr}[{index_expr}]", value_expr],
                    visualize=True
                )
                self._add_step(step)
    
    def _process_if(self, if_node: ast.If):
        """Обрабатывает условный оператор"""
        condition = self._expression_to_string(if_node.test)
        
        # Генерируем ID для шагов
        condition_id = self._generate_step_id("condition")
        true_start_id = self._generate_step_id("if_true")
        false_start_id = self._generate_step_id("if_false")
        after_if_id = self._generate_step_id("after_if")
        
        # Создаем шаг условия
        condition_step = Step(
            id=condition_id,
            type=StepType.CONDITION,
            description=f"Проверка условия: {condition}",
            parameters=[condition],
            condition_cases=[
                ConditionCase(condition="true", next_step=true_start_id),
                ConditionCase(condition="false", next_step=false_start_id)
            ],
            visualize=True
        )
        
        self._add_step(condition_step)
        
        # Обрабатываем ветку if (true)
        true_label_step = Step(
            id=true_start_id,
            type=StepType.GENERIC,
            description="Ветка if (условие истинно)",
            visualize=False
        )
        self._add_step(true_label_step)
        
        self._process_statements(if_node.body)
        
        # Добавляем переход к after_if после выполнения ветки true
        if not self._last_step_has_jump():
            goto_after_if = Step(
                id=self._generate_step_id("goto"),
                type=StepType.GENERIC,
                description="Переход после if",
                next_step=after_if_id,
                visualize=False
            )
            self._add_step(goto_after_if)
        
        # Обрабатываем ветку else если есть
        if if_node.orelse:
            # Добавляем метку начала ветки false
            false_label_step = Step(
                id=false_start_id,
                type=StepType.GENERIC,
                description="Ветка else",
                visualize=False
            )
            self._add_step(false_label_step)
            
            self._process_statements(if_node.orelse)
            
            # Добавляем переход к after_if после выполнения ветки false
            if not self._last_step_has_jump():
                goto_after_if_else = Step(
                    id=self._generate_step_id("goto_else"),
                    type=StepType.GENERIC,
                    description="Переход после else",
                    next_step=after_if_id,
                    visualize=False
                )
                self._add_step(goto_after_if_else)
        else:
            # Если нет else, ветка false сразу переходит к after_if
            false_label_step = Step(
                id=false_start_id,
                type=StepType.GENERIC,
                description="Ветка else отсутствует",
                next_step=after_if_id,
                visualize=False
            )
            self._add_step(false_label_step)
        
        # Добавляем метку after_if
        after_if_step = Step(
            id=after_if_id,
            type=StepType.GENERIC,
            description="Продолжение после условия",
            visualize=False
        )
        self._add_step(after_if_step)
    
    def _process_for(self, for_node: ast.For):
        """Обрабатывает цикл for"""
        # Получаем имя переменной цикла
        if isinstance(for_node.target, ast.Name):
            var_name = for_node.target.id
        else:
            var_name = "i"
        
        # Для range-based циклов
        if (isinstance(for_node.iter, ast.Call) and
            isinstance(for_node.iter.func, ast.Name) and
            for_node.iter.func.id == 'range'):
            
            range_args = for_node.iter.args
            
            # Генерируем ID для шагов цикла
            loop_start_id = self._generate_step_id("for_start")
            loop_check_id = self._generate_step_id("for_check")
            loop_body_id = self._generate_step_id("for_body")
            loop_incr_id = self._generate_step_id("for_incr")
            loop_end_id = self._generate_step_id("for_end")
            
            # Создаем начальную метку цикла
            loop_start_step = Step(
                id=loop_start_id,
                type=StepType.GENERIC,
                description="Начало цикла for",
                next_step=loop_check_id,
                visualize=False
            )
            self._add_step(loop_start_step)
            
            # Инициализация счетчика
            if len(range_args) == 1:
                # range(n) - от 0 до n-1
                init_step = Step(
                    id=self._generate_step_id("for_init"),
                    type=StepType.ASSIGN,
                    description=f"Инициализация счетчика {var_name}",
                    parameters=[var_name, "0"],
                    next_step=loop_check_id,
                    visualize=False
                )
                condition = f"{var_name} < {self._expression_to_string(range_args[0])}"
            elif len(range_args) == 2:
                # range(start, stop)
                init_step = Step(
                    id=self._generate_step_id("for_init"),
                    type=StepType.ASSIGN,
                    description=f"Инициализация счетчика {var_name}",
                    parameters=[var_name, self._expression_to_string(range_args[0])],
                    next_step=loop_check_id,
                    visualize=False
                )
                condition = f"{var_name} < {self._expression_to_string(range_args[1])}"
            else:
                # range(start, stop, step)
                init_step = Step(
                    id=self._generate_step_id("for_init"),
                    type=StepType.ASSIGN,
                    description=f"Инициализация счетчика {var_name}",
                    parameters=[var_name, self._expression_to_string(range_args[0])],
                    next_step=loop_check_id,
                    visualize=False
                )
                condition = f"{var_name} < {self._expression_to_string(range_args[1])}"
            
            self._add_step(init_step)
            
            # Добавляем метку проверки
            loop_check_step = Step(
                id=loop_check_id,
                type=StepType.GENERIC,
                description="Проверка условия цикла",
                visualize=False
            )
            self._add_step(loop_check_step)
            
            # Шаг проверки условия
            check_step = Step(
                id=self._generate_step_id("for_condition"),
                type=StepType.CONDITION,
                description=f"Проверка границ цикла for: {condition}",
                parameters=[condition],
                condition_cases=[
                    ConditionCase(condition="true", next_step=loop_body_id),
                    ConditionCase(condition="false", next_step=loop_end_id)
                ],
                visualize=True
            )
            self._add_step(check_step)
            
            # Добавляем метку тела цикла
            loop_body_step = Step(
                id=loop_body_id,
                type=StepType.GENERIC,
                description="Начало тела цикла",
                visualize=False
            )
            self._add_step(loop_body_step)
            
            # Добавляем контекст цикла в стек
            self.label_stack.append({
                'type': 'for',
                'continue_label': loop_incr_id,
                'break_label': loop_end_id
            })
            
            # Обрабатываем тело цикла
            self._process_statements(for_node.body)
            
            # Убираем контекст цикла из стека
            self.label_stack.pop()
            
            # Добавляем метку инкремента
            loop_incr_step = Step(
                id=loop_incr_id,
                type=StepType.GENERIC,
                description="Инкремент счетчика",
                visualize=False
            )
            self._add_step(loop_incr_step)
            
            # Шаг инкремента счетчика
            if len(range_args) == 3:
                incr_expr = f"{var_name} + {self._expression_to_string(range_args[2])}"
            else:
                incr_expr = f"{var_name} + 1"
            
            incr_step = Step(
                id=self._generate_step_id("for_incr_step"),
                type=StepType.ASSIGN,
                description=f"Увеличение счетчика {var_name}",
                parameters=[var_name, incr_expr],
                next_step=loop_check_id,
                visualize=False
            )
            self._add_step(incr_step)
            
            # Добавляем метку конца цикла
            loop_end_step = Step(
                id=loop_end_id,
                type=StepType.GENERIC,
                description="Конец цикла for",
                visualize=False
            )
            self._add_step(loop_end_step)
        else:
            # Для других итераторов создаем generic цикл
            self._create_generic_step(f"Цикл for по {self._expression_to_string(for_node.iter)}")
    
    def _process_while(self, while_node: ast.While):
        """Обрабатывает цикл while"""
        condition = self._expression_to_string(while_node.test)
        
        # Генерируем ID для шагов цикла
        loop_start_id = self._generate_step_id("while_start")
        loop_check_id = self._generate_step_id("while_check")
        loop_body_id = self._generate_step_id("while_body")
        loop_end_id = self._generate_step_id("while_end")
        
        # Создаем начальную метку цикла
        loop_start_step = Step(
            id=loop_start_id,
            type=StepType.GENERIC,
            description="Начало цикла while",
            next_step=loop_check_id,
            visualize=False
        )
        self._add_step(loop_start_step)
        
        # Добавляем метку проверки
        loop_check_step = Step(
            id=loop_check_id,
            type=StepType.GENERIC,
            description="Проверка условия цикла",
            visualize=False
        )
        self._add_step(loop_check_step)
        
        # Шаг проверки условия
        check_step = Step(
            id=self._generate_step_id("while_condition"),
            type=StepType.CONDITION,
            description=f"Проверка условия цикла: {condition}",
            parameters=[condition],
            condition_cases=[
                ConditionCase(condition="true", next_step=loop_body_id),
                ConditionCase(condition="false", next_step=loop_end_id)
            ],
            visualize=True
        )
        self._add_step(check_step)
        
        # Добавляем метку тела цикла
        loop_body_step = Step(
            id=loop_body_id,
            type=StepType.GENERIC,
            description="Начало тела цикла",
            visualize=False
        )
        self._add_step(loop_body_step)
        
        # Добавляем контекст цикла в стек
        self.label_stack.append({
            'type': 'while',
            'continue_label': loop_check_id,
            'break_label': loop_end_id
        })
        
        # Обрабатываем тело цикла
        self._process_statements(while_node.body)
        
        # Убираем контекст цикла из стека
        self.label_stack.pop()
        
        # Добавляем переход обратно к проверке условия
        goto_check = Step(
            id=self._generate_step_id("while_back"),
            type=StepType.GENERIC,
            description="Возврат к проверке условия",
            next_step=loop_check_id,
            visualize=False
        )
        self._add_step(goto_check)
        
        # Добавляем метку конца цикла
        loop_end_step = Step(
            id=loop_end_id,
            type=StepType.GENERIC,
            description="Конец цикла while",
            visualize=False
        )
        self._add_step(loop_end_step)
    
    def _process_return(self, return_node: ast.Return):
        """Обрабатывает оператор return"""
        if return_node.value is None:
            # Return без значения
            step = Step(
                id=self._generate_step_id("return"),
                type=StepType.RETURN,
                description="Возврат из функции",
                visualize=False
            )
        else:
            # Return со значением
            return_expr = self._expression_to_string(return_node.value)
            step = Step(
                id=self._generate_step_id("return"),
                type=StepType.RETURN,
                description=f"Возврат значения: {return_expr}",
                parameters=[return_expr],
                visualize=False
            )
        self._add_step(step)
    
    def _process_augmented_assignment(self, aug_assign: ast.AugAssign):
        """Обрабатывает составное присваивание (например, i += 1)"""
        if isinstance(aug_assign.target, ast.Name):
            var_name = aug_assign.target.id
            
            # Преобразуем оператор в строку
            op_map = {
                ast.Add: "+",
                ast.Sub: "-",
                ast.Mult: "*",
                ast.Div: "/",
                ast.Mod: "%",
                ast.Pow: "^",
            }
            
            op_str = op_map.get(type(aug_assign.op), "?")
            value_expr = self._expression_to_string(aug_assign.value)
            
            # Создаем выражение для составного присваивания
            expr = f"{var_name} {op_str} {value_expr}"
            
            step = Step(
                id=self._generate_step_id("assign_aug"),
                type=StepType.ASSIGN,
                description=f"Составное присваивание: {var_name} {op_str}= {value_expr}",
                parameters=[var_name, expr],
                visualize=self._should_visualize_assignment(var_name)
            )
            self._add_step(step)
    
    def _process_expression(self, expr: ast.expr):
        """Обрабатывает выражение (вызов функции и т.д.)"""
        expr_str = self._expression_to_string(expr)
        
        # Если это вызов функции
        if isinstance(expr, ast.Call):
            func_name = self._expression_to_string(expr.func)
            step = Step(
                id=self._generate_step_id("call"),
                type=StepType.GENERIC,
                description=f"Вызов функции: {expr_str}",
                parameters=[expr_str],
                visualize=True
            )
            self._add_step(step)
        else:
            # Просто выражение
            step = Step(
                id=self._generate_step_id("expression"),
                type=StepType.GENERIC,
                description=f"Вычисление: {expr_str}",
                parameters=[expr_str],
                visualize=True
            )
            self._add_step(step)
    
    def _process_break(self, break_node: ast.Break):
        """Обрабатывает оператор break"""
        # Ищем ближайший цикл в стеке
        for context in reversed(self.label_stack):
            if context['type'] in ['for', 'while']:
                break_label = context['break_label']
                break_step = Step(
                    id=self._generate_step_id("break"),
                    type=StepType.GENERIC,
                    description="Выход из цикла (break)",
                    next_step=break_label,
                    visualize=False
                )
                self._add_step(break_step)
                return
        
        # Если не нашли цикл, создаем generic break
        self._create_generic_step("break (вне цикла)")
    
    def _process_continue(self, continue_node: ast.Continue):
        """Обрабатывает оператор continue"""
        # Ищем ближайший цикл в стеке
        for context in reversed(self.label_stack):
            if context['type'] in ['for', 'while']:
                continue_label = context['continue_label']
                continue_step = Step(
                    id=self._generate_step_id("continue"),
                    type=StepType.GENERIC,
                    description="Переход к следующей итерации (continue)",
                    next_step=continue_label,
                    visualize=False
                )
                self._add_step(continue_step)
                return
        
        # Если не нашли цикл, создаем generic continue
        self._create_generic_step("continue (вне цикла)")
    
    def _create_generic_step(self, description: str):
        """Создает универсальный шаг"""
        step = Step(
            id=self._generate_step_id("generic"),
            type=StepType.GENERIC,
            description=description,
            parameters=[],
            visualize=True
        )
        self._add_step(step)
    
    def _expression_to_string(self, expr: ast.expr) -> str:
        """Конвертирует выражение AST в строку"""
        if isinstance(expr, ast.Name):
            return expr.id
        elif isinstance(expr, ast.Constant):
            if isinstance(expr.value, str):
                return f'"{expr.value}"'
            else:
                return str(expr.value)
        elif isinstance(expr, ast.BinOp):
            left = self._expression_to_string(expr.left)
            right = self._expression_to_string(expr.right)
            op = self._operator_to_string(expr.op)
            return f"({left} {op} {right})"
        elif isinstance(expr, ast.Compare):
            left = self._expression_to_string(expr.left)
            ops = [self._comparator_to_string(op) for op in expr.ops]
            comparators = [self._expression_to_string(comp) for comp in expr.comparators]
            return " ".join([left] + [f"{op} {comp}" for op, comp in zip(ops, comparators)])
        elif isinstance(expr, ast.Call):
            func_name = self._expression_to_string(expr.func)
            args = [self._expression_to_string(arg) for arg in expr.args]
            return f"{func_name}({', '.join(args)})"
        elif isinstance(expr, ast.Subscript):
            value = self._expression_to_string(expr.value)
            slice_val = self._expression_to_string(expr.slice)
            return f"{value}[{slice_val}]"
        elif isinstance(expr, ast.Attribute):
            value = self._expression_to_string(expr.value)
            return f"{value}.{expr.attr}"
        elif isinstance(expr, ast.UnaryOp):
            op = self._operator_to_string(expr.op)
            operand = self._expression_to_string(expr.operand)
            return f"{op}{operand}"
        elif isinstance(expr, ast.List):
            elements = [self._expression_to_string(e) for e in expr.elts]
            return f"[{', '.join(elements)}]"
        elif isinstance(expr, ast.Tuple):
            elements = [self._expression_to_string(e) for e in expr.elts]
            return f"({', '.join(elements)})"
        elif isinstance(expr, ast.Dict):
            keys = [self._expression_to_string(k) for k in expr.keys if k is not None]
            values = [self._expression_to_string(v) for v in expr.values]
            items = [f"{k}: {v}" for k, v in zip(keys, values)]
            return f"{{{', '.join(items)}}}"
        else:
            return str(ast.dump(expr, indent=2))
    
    def _operator_to_string(self, op: ast.operator) -> str:
        """Конвертирует оператор в строку"""
        if isinstance(op, ast.Add):
            return "+"
        elif isinstance(op, ast.Sub):
            return "-"
        elif isinstance(op, ast.Mult):
            return "*"
        elif isinstance(op, ast.Div):
            return "/"
        elif isinstance(op, ast.FloorDiv):
            return "//"
        elif isinstance(op, ast.Mod):
            return "%"
        elif isinstance(op, ast.Pow):
            return "^"
        elif isinstance(op, ast.LShift):
            return "<<"
        elif isinstance(op, ast.RShift):
            return ">>"
        elif isinstance(op, ast.BitOr):
            return "|"
        elif isinstance(op, ast.BitXor):
            return "^"
        elif isinstance(op, ast.BitAnd):
            return "&"
        elif isinstance(op, ast.MatMult):
            return "@"
        elif isinstance(op, ast.USub):
            return "-"
        elif isinstance(op, ast.UAdd):
            return "+"
        elif isinstance(op, ast.Not):
            return "not "
        elif isinstance(op, ast.Invert):
            return "~"
        else:
            return str(op)
    
    def _comparator_to_string(self, op: ast.cmpop) -> str:
        """Конвертирует компаратор в строку"""
        if isinstance(op, ast.Eq):
            return "=="
        elif isinstance(op, ast.NotEq):
            return "!="
        elif isinstance(op, ast.Lt):
            return "<"
        elif isinstance(op, ast.LtE):
            return "<="
        elif isinstance(op, ast.Gt):
            return ">"
        elif isinstance(op, ast.GtE):
            return ">="
        elif isinstance(op, ast.Is):
            return "is"
        elif isinstance(op, ast.IsNot):
            return "is not"
        elif isinstance(op, ast.In):
            return "in"
        elif isinstance(op, ast.NotIn):
            return "not in"
        else:
            return str(op)
    
    def _infer_type(self, expr: ast.expr) -> str:
        """Определяет тип выражения"""
        if isinstance(expr, ast.Constant):
            if isinstance(expr.value, int):
                return "int"
            elif isinstance(expr.value, float):
                return "float"
            elif isinstance(expr.value, bool):
                return "bool"
            elif isinstance(expr.value, str):
                return "string"
        elif isinstance(expr, ast.List):
            return "array"
        elif isinstance(expr, ast.Dict):
            return "object"
        elif isinstance(expr, ast.Name):
            # Пытаемся определить по имени
            if expr.id in self.variable_types:
                return self.variable_types[expr.id]
            return "int"  # По умолчанию
        elif isinstance(expr, ast.Call):
            # Для функций предполагаем int по умолчанию
            return "int"
        return "int"  # Дефолтный тип
    
    def _get_initial_value(self, expr: ast.expr) -> Any:
        """Получает начальное значение"""
        if isinstance(expr, ast.Constant):
            return expr.value
        elif isinstance(expr, ast.List):
            return [self._get_initial_value(e) for e in expr.elts]
        elif isinstance(expr, ast.Dict):
            result = {}
            for key, value in zip(expr.keys, expr.values):
                if key is not None:
                    key_value = self._get_initial_value(key)
                    value_value = self._get_initial_value(value)
                    result[key_value] = value_value
            return result
        else:
            # Для выражений возвращаем строковое представление
            return self._expression_to_string(expr)
    
    def _should_visualize_assignment(self, var_name: str) -> bool:
        """Определяет, нужно ли визуализировать присваивание"""
        # Присваивания счетчиков обычно не визуализируются
        non_visual_vars = {'i', 'j', 'k', 'temp', 'swapped', 'found', 'n', 'm'}
        return var_name not in non_visual_vars
    
    def _generate_step_id(self, prefix: str = "step") -> str:
        """Генерирует уникальный ID шага"""
        self.step_counter += 1
        return f"{prefix}_{self.step_counter}"
    
    def _get_current_steps(self) -> List[Step]:
        """Получает текущий список шагов"""
        return self.current_function.steps
    
    def _last_step_has_jump(self) -> bool:
        """Проверяет, имеет ли последний шаг явный переход"""
        steps = self.current_function.steps
        if not steps:
            return False
        
        last_step = steps[-1]
        # Шаг имеет явный переход если:
        # 1. У него есть next_step
        # 2. Это условие (condition)
        # 3. Это возврат (return)
        # 4. Это вызов функции (call_function)
        return (last_step.next_step is not None or
                last_step.type in [StepType.CONDITION, StepType.RETURN, StepType.CALL_FUNCTION])
    
    def _add_step(self, step: Step):
        """Добавляет шаг в текущую функцию"""
        self.current_function.steps.append(step)
    
    def _create_start_end_steps(self):
        """Создает начальный и конечный шаги"""
        # Удаляем существующие start/end если есть
        self.algorithm.steps = [s for s in self.algorithm.steps 
                              if s.id not in ["start", "end"]]
        
        # Добавляем start
        start_step = Step(
            id="start",
            type=StepType.GENERIC,
            description="Начало алгоритма",
            parameters=[],
            visualize=True
        )
        
        # Добавляем end
        end_step = Step(
            id="end",
            type=StepType.GENERIC,
            description="Алгоритм завершен",
            parameters=[],
            visualize=True
        )
        
        self.algorithm.steps.insert(0, start_step)
        self.algorithm.steps.append(end_step)
        
        # Устанавливаем next_step для start
        if self.algorithm.functions:
            # Вызываем первую функцию
            call_step = Step(
                id="call_main",
                type=StepType.CALL_FUNCTION,
                description=f"Вызов функции {self.algorithm.functions[0].name}",
                function_name=self.algorithm.functions[0].name,
                function_parameters={},
                return_to_step="end",
                visualize=True
            )
            self.algorithm.steps.insert(1, call_step)
            start_step.next_step = call_step.id
        else:
            start_step.next_step = "end"
    
    def _process_global_variables(self, tree: ast.AST):
        """Обрабатывает глобальные переменные"""
        for node in ast.walk(tree):
            if isinstance(node, ast.Assign):
                for target in node.targets:
                    if isinstance(target, ast.Name):
                        var_name = target.id
                        # Пропускаем struct и переменные из функций
                        if (var_name != self.struct_name and 
                            not any(var_name in f.parameters 
                                   for f in self.algorithm.functions)):
                            if not any(v.name == var_name 
                                      for v in self.algorithm.variables):
                                var_type = self._infer_type(node.value)
                                initial_value = self._get_initial_value(node.value)
                                
                                # Форматируем значение для JSON
                                if var_type == "string":
                                    if isinstance(initial_value, str):
                                        # Убираем лишние кавычки
                                        if initial_value.startswith('"') and initial_value.endswith('"'):
                                            initial_value = initial_value[1:-1]
                                        initial_value = f'"{initial_value}"'
                                    else:
                                        initial_value = f'"{str(initial_value)}"'
                                elif var_type == "bool":
                                    if isinstance(initial_value, str):
                                        if initial_value.lower() == "true":
                                            initial_value = True
                                        elif initial_value.lower() == "false":
                                            initial_value = False
                                        else:
                                            initial_value = False
                                    elif not isinstance(initial_value, bool):
                                        initial_value = False
                                elif var_type in ["int", "float"]:
                                    # Попробуем преобразовать строку в число
                                    if isinstance(initial_value, str):
                                        try:
                                            if var_type == "int":
                                                initial_value = int(initial_value)
                                            else:
                                                initial_value = float(initial_value)
                                        except (ValueError, TypeError):
                                            # Оставляем как строку
                                            pass
                                
                                self.algorithm.variables.append(Variable(
                                    name=var_name,
                                    type=var_type,
                                    initial_value=initial_value,
                                    description=f"Глобальная переменная {var_name}"
                                ))
    
    def _collect_imports(self, tree: ast.AST):
        """Собирает информацию об импортах"""
        for node in ast.walk(tree):
            if isinstance(node, ast.Import):
                for alias in node.names:
                    self.imported_modules.add(alias.name)
            elif isinstance(node, ast.ImportFrom):
                module = node.module or ""
                for alias in node.names:
                    self.imported_modules.add(f"{module}.{alias.name}")
    
    def _connect_steps(self):
        """Устанавливает связи между шагами"""
        # Связываем шаги в основной программе
        self._connect_step_list(self.algorithm.steps)
        
        # Связываем шаги в функциях
        for func in self.algorithm.functions:
            self._connect_step_list(func.steps)
    
    def _connect_step_list(self, steps: List[Step]):
        """Связывает шаги в списке"""
        # Создаем словарь для быстрого поиска шагов по ID
        step_dict = {step.id: step for step in steps}
        
        # Проходим по шагам и устанавливаем связи
        for i in range(len(steps)):
            current_step = steps[i]
            
            # Если у шага нет next_step и это не последний шаг
            if (current_step.next_step is None and 
                i < len(steps) - 1 and
                current_step.type not in [StepType.CONDITION, StepType.RETURN, StepType.CALL_FUNCTION]):
                # Устанавливаем next_step на следующий шаг
                current_step.next_step = steps[i + 1].id
            
            # Проверяем condition_cases
            if current_step.type == StepType.CONDITION:
                for case in current_step.condition_cases:
                    if case.next_step and case.next_step not in step_dict:
                        # Ищем шаг с таким ID в следующих шагах
                        found = False
                        for j in range(i + 1, len(steps)):
                            if steps[j].id == case.next_step:
                                found = True
                                break
                        if not found:
                            # Если не нашли, создаем новый шаг с этим ID
                            new_step = Step(
                                id=case.next_step,
                                type=StepType.GENERIC,
                                description=f"Метка: {case.next_step}",
                                visualize=False
                            )
                            steps.insert(i + 1, new_step)
                            step_dict[new_step.id] = new_step
    
    def _find_main_function(self, tree: ast.AST) -> Optional[ast.FunctionDef]:
        """Находит основную функцию алгоритма"""
        functions = []
        for node in ast.walk(tree):
            if isinstance(node, ast.FunctionDef):
                functions.append(node)
        
        if not functions:
            return None
            
        # Предполагаем, что первая функция - основная
        return functions[0]
    
    def _extract_docstring(self, func_node: ast.FunctionDef) -> str:
        """Извлекает docstring из функции"""
        if (func_node.body and 
            isinstance(func_node.body[0], ast.Expr) and
            isinstance(func_node.body[0].value, ast.Constant)):
            return str(func_node.body[0].value.value)
        return ""
    
    def _detect_structure_type(self, tree: ast.AST) -> StructureType:
        """Определяет тип структуры данных по использованию"""
        # По умолчанию массив
        return StructureType.ARRAY