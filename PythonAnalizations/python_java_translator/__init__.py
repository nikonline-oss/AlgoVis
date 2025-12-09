"""
Модуль трансляции Python-кода в ЯВА с настройками визуализации операций
"""

from .visualization_config import VisualizationConfig, create_preset_configs
from .data_structures import VariableInfo, StepInfo
from .variable_collector import VariableCollector
from .java_translator import JavaTranslator

__version__ = "1.0.0"
__all__ = [
    'VisualizationConfig',
    'VariableInfo',
    'StepInfo',
    'VariableCollector',
    'JavaTranslator',
    'create_preset_configs'
]