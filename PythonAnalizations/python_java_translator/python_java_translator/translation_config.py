"""
Конфигурация безопасности и ограничений трансляции
"""

from dataclasses import dataclass
from typing import Optional, List

@dataclass
class TranslationConfig:
    """Конфигурация безопасности и ограничений трансляции"""
    
    # Ограничения по размеру
    max_code_size: int = 10000  # ~100 строк
    max_steps: int = 1000       # Максимальное количество шагов
    max_recursion_depth: int = 50  # Максимальная глубина рекурсии
    
    # Безопасность
    allow_dangerous_constructs: bool = False
    safe_mode: bool = True
    
    # Валидация
    validate_output: bool = True
    check_loops: bool = True
    ensure_start_end: bool = True
    
    # Дополнительные настройки
    auto_add_struct: bool = True
    default_structure_type: str = "array"
    
    # Черный список конструкций
    blacklisted_keywords: List[str] = None
    
    def __post_init__(self):
        if self.blacklisted_keywords is None:
            self.blacklisted_keywords = [
            'import', 'from',  # Добавлено
            '__import__', 'eval', 'exec', 'compile', 'open',
            'globals', 'locals', '__builtins__', '__loader__',
            '__file__', '__name__', '__package__',
            'system', 'os.', 'subprocess', 'sys.'  # Расширено
        ]