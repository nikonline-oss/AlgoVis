"""Транслятор Python в ЯВА (Язык Визуализации Алгоритмов)"""

__version__ = "0.1.0"
__author__ = "ЯВА Транслятор"
__description__ = "Транслятор Python кода в формат ЯВА для визуализации алгоритмов"

from .core.translator import JAVATranslator

__all__ = ['JAVATranslator']