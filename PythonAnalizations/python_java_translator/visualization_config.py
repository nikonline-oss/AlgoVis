"""
Конфигурация визуализации операций ЯВА
"""

from dataclasses import dataclass
from typing import Optional

@dataclass
class VisualizationConfig:
    """Конфигурация визуализации операций ЯВА"""
    
    # Флаги для включения/отключения визуализации разных типов операций
    visualize_assign: bool = True          # Присваивания (x = y)
    visualize_condition: bool = True       # Условия (if, while)
    visualize_compare: bool = True         # Сравнения элементов
    visualize_swap: bool = True            # Обмен элементов
    visualize_call: bool = True            # Вызовы функций
    visualize_generic: bool = True         # Универсальные операции
    visualize_loop_init: bool = False      # Инициализация циклов (обычно скрываем)
    visualize_loop_increment: bool = False # Инкременты в циклах (обычно скрываем)
    visualize_start_end: bool = True       # Старт и конец алгоритма
    
    # Настройки подсветки
    highlight_enabled: bool = True
    highlight_color_assign: str = "blue"
    highlight_color_condition: str = "orange"
    highlight_color_compare: str = "yellow"
    highlight_color_swap: str = "red"
    highlight_color_call: str = "green"
    
    def should_visualize(self, step_type: str, description: str = "") -> bool:
        """Определяет, нужно ли визуализировать шаг данного типа"""
        step_type_lower = step_type.lower()
        description_lower = description.lower()
        
        # Проверяем специальные случаи для циклов
        if "инициализация счетчика" in description_lower or "init" in description_lower:
            return self.visualize_loop_init
        
        if "инкремент" in description_lower or "увеличение счетчика" in description_lower:
            return self.visualize_loop_increment
        
        # Базовые проверки по типу
        if step_type_lower == "assign":
            return self.visualize_assign
        elif step_type_lower == "condition":
            return self.visualize_condition
        elif step_type_lower == "compare":
            return self.visualize_compare
        elif step_type_lower == "swap":
            return self.visualize_swap
        elif step_type_lower in ["call_function", "generic"]:
            # Проверяем описание для различения call и generic
            if "вызов функции" in description_lower or "call" in description_lower:
                return self.visualize_call
            else:
                return self.visualize_generic
        elif step_type_lower == "generic":
            return self.visualize_generic
        elif "start" in description_lower or "end" in description_lower:
            return self.visualize_start_end
        
        return True  # По умолчанию визуализируем
    
    def get_highlight_color(self, step_type: str) -> Optional[str]:
        """Возвращает цвет подсветки для типа шага"""
        if not self.highlight_enabled:
            return None
            
        step_type_lower = step_type.lower()
        
        if step_type_lower == "assign":
            return self.highlight_color_assign
        elif step_type_lower == "condition":
            return self.highlight_color_condition
        elif step_type_lower == "compare":
            return self.highlight_color_compare
        elif step_type_lower == "swap":
            return self.highlight_color_swap
        elif step_type_lower in ["call_function", "generic"]:
            return self.highlight_color_call
        
        return None


def create_preset_configs():
    """Создание предустановленных конфигураций визуализации"""
    
    presets = {}
    
    # 1. Полная визуализация (все шаги)
    presets["full"] = VisualizationConfig(
        visualize_assign=True,
        visualize_condition=True,
        visualize_compare=True,
        visualize_swap=True,
        visualize_call=True,
        visualize_generic=True,
        visualize_loop_init=True,
        visualize_loop_increment=True,
        visualize_start_end=True,
        highlight_enabled=True
    )
    
    # 2. Минимальная визуализация (только ключевые шаги)
    presets["minimal"] = VisualizationConfig(
        visualize_assign=True,
        visualize_condition=True,
        visualize_compare=True,
        visualize_swap=True,
        visualize_call=False,
        visualize_generic=False,
        visualize_loop_init=False,
        visualize_loop_increment=False,
        visualize_start_end=True,
        highlight_enabled=False
    )
    
    # 3. Отладка (только присваивания и условия)
    presets["debug"] = VisualizationConfig(
        visualize_assign=True,
        visualize_condition=True,
        visualize_compare=False,
        visualize_swap=False,
        visualize_call=False,
        visualize_generic=False,
        visualize_loop_init=True,
        visualize_loop_increment=False,
        visualize_start_end=True,
        highlight_enabled=True,
        highlight_color_assign="blue",
        highlight_color_condition="orange"
    )
    
    # 4. Производительность (минимум визуализации)
    presets["performance"] = VisualizationConfig(
        visualize_assign=False,
        visualize_condition=False,
        visualize_compare=False,
        visualize_swap=False,
        visualize_call=False,
        visualize_generic=False,
        visualize_loop_init=False,
        visualize_loop_increment=False,
        visualize_start_end=True,
        highlight_enabled=False
    )
    
    # 5. Обучение (подробная визуализация с подсветкой)
    presets["educational"] = VisualizationConfig(
        visualize_assign=True,
        visualize_condition=True,
        visualize_compare=True,
        visualize_swap=True,
        visualize_call=True,
        visualize_generic=True,
        visualize_loop_init=True,
        visualize_loop_increment=True,
        visualize_start_end=True,
        highlight_enabled=True,
        highlight_color_assign="blue",
        highlight_color_condition="orange",
        highlight_color_compare="yellow",
        highlight_color_swap="red",
        highlight_color_call="green"
    )
    
    return presets