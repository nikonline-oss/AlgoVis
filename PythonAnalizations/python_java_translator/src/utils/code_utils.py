"""Утилиты для работы с Python кодом"""
import ast
import re
from typing import Dict, List, Set, Tuple, Optional, Any


def extract_function_info(python_code: str) -> Dict[str, Dict]:
    """Извлекает информацию о функциях из кода"""
    try:
        tree = ast.parse(python_code)
    except SyntaxError as e:
        raise ValueError(f"Синтаксическая ошибка в Python коде: {e}")
    
    functions = {}
    
    for node in ast.walk(tree):
        if isinstance(node, ast.FunctionDef):
            # Получаем docstring
            docstring = ast.get_docstring(node) or ""
            
            # Получаем параметры
            params = []
            for arg in node.args.args:
                params.append(arg.arg)
            
            # Получаем тело функции как строку
            body_lines = []
            for stmt in node.body:
                if isinstance(stmt, ast.Expr) and isinstance(stmt.value, ast.Constant):
                    # Пропускаем docstring
                    continue
                body_lines.append(ast.unparse(stmt))
            
            body = "\n".join(body_lines)
            
            functions[node.name] = {
                "name": node.name,
                "docstring": docstring,
                "params": params,
                "body": body,
                "node": node  # Сохраняем AST узел для дальнейшей обработки
            }
    
    return functions


def find_main_function(functions: Dict[str, Dict]) -> Optional[str]:
    """Находит основную функцию алгоритма"""
    # Сначала ищем функцию с названием "main"
    if "main" in functions:
        return "main"
    
    # Ищем функцию с "algorithm" в названии
    for name in functions:
        if "algorithm" in name.lower():
            return name
    
    # Берем первую функцию
    if functions:
        return list(functions.keys())[0]
    
    return None


def extract_imports(python_code: str) -> List[str]:
    """Извлекает импорты из кода"""
    try:
        tree = ast.parse(python_code)
    except SyntaxError:
        return []
    
    imports = []
    
    for node in ast.walk(tree):
        if isinstance(node, ast.Import):
            for alias in node.names:
                imports.append(alias.name)
        elif isinstance(node, ast.ImportFrom):
            module = node.module or ""
            for alias in node.names:
                imports.append(f"{module}.{alias.name}")
    
    return imports


def check_unsafe_constructs(python_code: str) -> List[str]:
    """Проверяет наличие опасных конструкций в коде"""
    unsafe_patterns = [
        # Файловые операции
        (r'open\s*\(', "Использование open() для работы с файлами"),
        (r'__import__', "Использование __import__"),
        (r'exec\s*\(', "Использование exec()"),
        (r'eval\s*\(', "Использование eval()"),
        (r'compile\s*\(', "Использование compile()"),
        
        # Системные вызовы
        (r'os\.', "Использование модуля os"),
        (r'subprocess\.', "Использование subprocess"),
        (r'sys\.', "Использование sys"),
        
        # Сетевые операции
        (r'http\.', "HTTP запросы"),
        (r'socket\.', "Сокеты"),
        (r'urllib\.', "URL библиотеки"),
        
        # Базы данных
        (r'sqlite3\.', "SQLite"),
        (r'mysql\.', "MySQL"),
        (r'psycopg2\.', "PostgreSQL"),
    ]
    
    warnings = []
    for pattern, message in unsafe_patterns:
        if re.search(pattern, python_code, re.IGNORECASE):
            warnings.append(message)
    
    return warnings


def format_java_variable_value(value: Any, var_type: str) -> Any:
    """Форматирует значение переменной для ЯВА JSON"""
    if var_type == "string":
        if isinstance(value, str):
            # Экранируем кавычки
            value = value.replace('"', '\\"').replace("'", "\\'")
            return f'"{value}"'
        else:
            return f'"{str(value)}"'
    
    elif var_type == "bool":
        if isinstance(value, bool):
            return str(value).lower()
        elif isinstance(value, str):
            if value.lower() in ["true", "false"]:
                return value.lower()
            else:
                return "false"
        else:
            return "false"
    
    elif var_type == "array":
        if isinstance(value, (list, tuple)):
            return value
        elif isinstance(value, str):
            # Пытаемся разобрать строку как массив
            if value.startswith("[") and value.endswith("]"):
                try:
                    return eval(value)
                except:
                    return []
            else:
                return [value]
        else:
            return []
    
    elif var_type == "object":
        if isinstance(value, dict):
            return value
        else:
            return {}
    
    else:  # int, float
        return value


def generate_step_id(base_name: str, existing_ids: Set[str]) -> str:
    """Генерирует уникальный ID для шага"""
    counter = 1
    while True:
        new_id = f"{base_name}_{counter}"
        if new_id not in existing_ids:
            return new_id
        counter += 1


def validate_identifier(name: str) -> bool:
    """Проверяет, является ли строка допустимым идентификатором"""
    if not name or not isinstance(name, str):
        return False
    
    # Проверяем на соответствие правилам именования переменных
    pattern = r'^[a-zA-Z_][a-zA-Z0-9_]*$'
    return bool(re.match(pattern, name))


def split_into_safe_lines(code: str, max_length: int = 100) -> List[str]:
    """Разбивает код на безопасные строки для отображения"""
    lines = []
    current_line = ""
    
    for char in code:
        if len(current_line) >= max_length and char in " ,.;":
            lines.append(current_line)
            current_line = char
        else:
            current_line += char
    
    if current_line:
        lines.append(current_line)
    
    return lines


def extract_comments(python_code: str) -> Dict[int, str]:
    """Извлекает комментарии из Python кода"""
    comments = {}
    
    lines = python_code.split('\n')
    for i, line in enumerate(lines):
        line = line.strip()
        if line.startswith('#'):
            comment = line[1:].strip()
            comments[i] = comment
    
    return comments


def is_recursive_function(func_node: ast.FunctionDef) -> bool:
    """Проверяет, является ли функция рекурсивной"""
    for node in ast.walk(func_node):
        if isinstance(node, ast.Call):
            if isinstance(node.func, ast.Name):
                if node.func.id == func_node.name:
                    return True
    return False