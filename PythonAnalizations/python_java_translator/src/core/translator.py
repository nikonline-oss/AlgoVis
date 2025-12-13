"""Основной транслятор Python → ЯВА"""
import os
import importlib
import json
from pathlib import Path
from typing import Dict, Any, List

from .ast_parser import ASTParser
from .java_generator import JAVAGenerator
from .validator import JAVAValidator
from ..mods.base_mod import BaseMod
from ..config.visualization import VisualizationConfig


class JAVATranslator:
    def __init__(self, mods_dir: str = None):
        self.parser = ASTParser()
        self.generator = JAVAGenerator()
        self.validator = JAVAValidator()
        self.visualization_config = VisualizationConfig()
        
        # Система модов
        self.mods: List[BaseMod] = []
        self.mods_dir = mods_dir or "mods"
        
        # Загружаем стандартные моды
        self._load_standard_mods()
        
        # Загружаем пользовательские моды
        if os.path.exists(self.mods_dir):
            self._load_custom_mods()
    
    def translate(self, python_code: str, 
                  visualize_types: List[str] = None) -> Dict[str, Any]:
        """
        Транслирует Python код в ЯВА JSON
        
        Args:
            python_code: Исходный код на Python
            visualize_types: Список типов шагов для визуализации
                (например: ["assign", "compare", "swap"])
        
        Returns:
            Словарь с JSON в формате ЯВА
        """
        # Парсим Python код
        algorithm = self.parser.parse(python_code)
        
        # Настраиваем визуализацию
        if visualize_types:
            self._setup_visualization(visualize_types)
        
        # Применяем моды (сортировка по приоритету)
        self.mods.sort(key=lambda m: m.priority)
        for mod in self.mods:
            algorithm = mod.process(algorithm)
        
        # Применяем конфигурацию визуализации
        algorithm = self.visualization_config.apply_to_algorithm(algorithm)
        
        # Генерируем JSON
        java_json_str = self.generator.generate(algorithm)
        java_dict = json.loads(java_json_str)
        
        # Применяем моды после генерации
        for mod in self.mods:
            java_dict = mod.after_generation(java_dict)
        
        return java_dict
    
    def _setup_visualization(self, visualize_types: List[str]):
        """Настраивает визуализацию по типам шагов"""
        from .intermediate import StepType
        
        # Отключаем все типы
        for st in StepType:
            self.visualization_config.disable_type(st)
        
        # Включаем только указанные типы
        type_mapping = {
            "assign": StepType.ASSIGN,
            "compare": StepType.COMPARE,
            "swap": StepType.SWAP,
            "condition": StepType.CONDITION,
            "call_function": StepType.CALL_FUNCTION,
            "return": StepType.RETURN,
            "generic": StepType.GENERIC
        }
        
        for vt in visualize_types:
            if vt in type_mapping:
                self.visualization_config.enable_type(type_mapping[vt])
    
    def _load_standard_mods(self):
        """Загружает стандартные моды"""
        try:
            from ..mods.standard_mods.sorting_mod import SortingMod
            self.mods.append(SortingMod())
        except ImportError:
            pass
        
        try:
            from ..mods.standard_mods.search_mod import SearchMod
            self.mods.append(SearchMod())
        except ImportError:
            pass
    
    def _load_custom_mods(self):
        """Загружает пользовательские моды из директории"""
        mods_path = Path(self.mods_dir)
        
        for file_path in mods_path.glob("*.py"):
            if file_path.name == "__init__.py":
                continue
            
            try:
                # Динамически импортируем модуль
                spec = importlib.util.spec_from_file_location(
                    file_path.stem,
                    file_path
                )
                module = importlib.util.module_from_spec(spec)
                spec.loader.exec_module(module)
                
                # Ищем классы-наследники BaseMod
                for attr_name in dir(module):
                    attr = getattr(module, attr_name)
                    if (isinstance(attr, type) and 
                        issubclass(attr, BaseMod) and 
                        attr != BaseMod):
                        # Создаем экземпляр и добавляем
                        mod_instance = attr()
                        self.mods.append(mod_instance)
                        print(f"Загружен мод: {mod_instance.name}")
                        
            except Exception as e:
                print(f"Ошибка загрузки мода {file_path}: {e}")
    
    def register_mod(self, mod: BaseMod):
        """Регистрирует мод вручную"""
        self.mods.append(mod)
        self.mods.sort(key=lambda m: m.priority)
    
    def set_visualization_filter(self, filter_func):
        """Устанавливает пользовательский фильтр визуализации"""
        self.visualization_config.add_filter(filter_func)