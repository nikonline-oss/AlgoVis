"""
Модуль трансляции Python-кода в ЯВА с настройками визуализации операций
"""

from .visualization_config import VisualizationConfig, create_preset_configs
from .translation_config import TranslationConfig
from .structure_registry import StructureRegistry, StructureProperties
from .error_handler import ErrorHandler, ErrorType, TranslationError
from .data_structures import VariableInfo, StepInfo
from .variable_collector import VariableCollector
from .java_translator import JavaTranslator

__version__ = "2.0.0"
__all__ = [
    'VisualizationConfig',
    'TranslationConfig',
    'StructureRegistry',
    'StructureProperties',
    'ErrorHandler',
    'ErrorType',
    'TranslationError',
    'VariableInfo',
    'StepInfo',
    'VariableCollector',
    'JavaTranslator',
    'create_preset_configs'
]