# ast_to_ir.py

import ast
from .ir_nodes import *
from .ir_structures import *
from .ir_control import IRFor, IRBreak, IRContinue, IRIf, IRWhile


class ASTtoIR(ast.NodeVisitor):
    """
    Преобразователь абстрактного синтаксического дерева (AST) в промежуточное представление (IR).
    
    Наследует от ast.NodeVisitor для обхода AST и трансляции Python кода
    в промежуточное представление для дальнейшей конвертации в ЯВА формат.
    
    Attributes:
        program (dict): Структура программы в IR формате
        current_class (IRClass): Текущий обрабатываемый класс
        current_method (IRMethod): Текущий обрабатываемый метод
        _pending_assigns (list): Список отложенных присваиваний
    """
    
    def __init__(self):
        """
        Инициализирует преобразователь с пустой структурой программы.
        """
        self.program = {
            "classes": {},
            "functions": {},
            "statements": [],
            "variables": {
                "last_comparison": {
                    "name": "last_comparison",
                    "type": "int",
                    "initialValue": 0
                }
            }
        }

        self.current_class = None
        self.current_method = None
        self._pending_assigns = []

    def visit_If(self, node: ast.If):
        """
        Обрабатывает условный оператор if.
        
        Преобразует AST узел if в IR представление, включая обработку сравнений
        и формирование ветвей true/false.
        
        Args:
            node (ast.If): Узел AST оператора if
            
        Raises:
            NotImplementedError: Если используется неподдерживаемый оператор сравнения
        """
        # 1. Если условие — Compare, делаем IRCompare
        if isinstance(node.test, ast.Compare):
            left = ast.unparse(node.test.left)
            right = ast.unparse(node.test.comparators[0])

            compare_stmt = IRCompare(left, right)
            self._emit(compare_stmt)

            # оператор сравнения
            op = node.test.ops[0]
            if isinstance(op, ast.Gt):
                cond = "last_comparison > 0"
            elif isinstance(op, ast.Lt):
                cond = "last_comparison < 0"
            elif isinstance(op, ast.Eq):
                cond = "last_comparison == 0"
            else:
                raise NotImplementedError("Оператор сравнения не поддержан")

        else:
            # обычное условие
            cond = ast.unparse(node.test)

        ir_if = IRIf(cond)

        # TRUE
        prev = self.current_method
        self.current_method = ir_if
        for stmt in node.body:
            self.visit(stmt)
        self.current_method = prev

        # FALSE
        if node.orelse:
            prev = self.current_method
            self.current_method = ir_if
            for stmt in node.orelse:
                self.visit(stmt)
            self.current_method = prev

        self._emit(ir_if)

    def visit_ClassDef(self, node: ast.ClassDef):
        """
        Обрабатывает определение класса.
        
        Args:
            node (ast.ClassDef): Узел AST определения класса
        """
        ir_class = IRClass(node.name)
        self.program["classes"][node.name] = ir_class

        self.current_class = ir_class
        for stmt in node.body:
            self.visit(stmt)
        self.current_class = None

    def visit_FunctionDef(self, node: ast.FunctionDef):
        """
        Обрабатывает определение функции/метода.
        
        Args:
            node (ast.FunctionDef): Узел AST определения функции
        """
        params = [arg.arg for arg in node.args.args]

        ir_method = IRMethod(node.name, params)

        if self.current_class:
            self.current_class.methods[node.name] = ir_method
        else:
            self.program["functions"][node.name] = ir_method

        prev = self.current_method
        self.current_method = ir_method

        for stmt in node.body:
            self.visit(stmt)

        self.current_method = prev

    def visit_Assign(self, node: ast.Assign):
        """
        Обрабатывает оператор присваивания.
        
        Args:
            node (ast.Assign): Узел AST оператора присваивания
        """
        target = ast.unparse(node.targets[0])
        value = ast.unparse(node.value)

        if target.startswith("self.") and self.current_class:
            field = target.split(".", 1)[1]
            self.current_class.fields.add(field)

        stmt = IRAssign(target, value)
        self._emit(stmt)

    def visit_Compare(self, node: ast.Compare):
        """
        Обрабатывает оператор сравнения.
        
        Args:
            node (ast.Compare): Узел AST оператора сравнения
        """
        left = ast.unparse(node.left)
        right = ast.unparse(node.comparators[0])

        stmt = IRCompare(left, right)
        self._emit(stmt)

    def visit_Return(self, node: ast.Return):
        """
        Обрабатывает оператор return.
        
        Args:
            node (ast.Return): Узел AST оператора return
        """
        value = ast.unparse(node.value) if node.value else None
        self._emit(IRReturn(value))

    def visit_For(self, node: ast.For):
        """
        Обрабатывает цикл for.
        
        Поддерживает только циклы вида for ... in range(...).
        
        Args:
            node (ast.For): Узел AST цикла for
            
        Raises:
            NotImplementedError: Если используется не range() итерируемый объект
            ValueError: Если range() имеет неподдерживаемое количество аргументов
        """
        if not isinstance(node.iter, ast.Call):
            raise NotImplementedError("Поддерживается только for ... in range()")

        if not isinstance(node.iter.func, ast.Name) or node.iter.func.id != "range":
            raise NotImplementedError("Поддерживается только range()")

        args = node.iter.args
        var = ast.unparse(node.target)

        if len(args) == 1:
            start = "0"
            end = ast.unparse(args[0])
            step = "1"
        elif len(args) == 2:
            start = ast.unparse(args[0])
            end = ast.unparse(args[1])
            step = "1"
        elif len(args) == 3:
            start = ast.unparse(args[0])
            end = ast.unparse(args[1])
            step = ast.unparse(args[2])
        else:
            raise ValueError("range() с неподдерживаемым количеством аргументов")

        ir_for = IRFor(var, start, end, step)

        # Регистрируем переменную цикла
        self.program["variables"].setdefault(var, {
            "name": var,
            "type": "int",
            "initialValue": None
        })

        prev_method = self.current_method
        self.current_method = ir_for 

        for stmt in node.body:
            self.visit(stmt)

        self.current_method = prev_method

        self._emit(ir_for)

    def visit_Break(self, node: ast.Break):
        """
        Обрабатывает оператор break.
        
        Args:
            node (ast.Break): Узел AST оператора break
        """
        self._emit(IRBreak())

    def visit_Continue(self, node: ast.Continue):
        """
        Обрабатывает оператор continue.
        
        Args:
            node (ast.Continue): Узел AST оператора continue
        """
        self._emit(IRContinue())

    def _emit(self, stmt):
        """
        Добавляет оператор в текущий контекст выполнения.
        
        В зависимости от текущего контекста (метод, условие if, цикл),
        добавляет оператор в соответствующее тело.
        
        Args:
            stmt: Оператор IR для добавления
        """
        if self.current_method:
            if hasattr(self.current_method, "body"):
                self.current_method.body.append(stmt)
            elif hasattr(self.current_method, "true_body"):
                self.current_method.true_body.append(stmt)
        else:
            self.program["statements"].append(stmt)