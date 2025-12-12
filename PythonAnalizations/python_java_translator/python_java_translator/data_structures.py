"""
Структуры данных для трансляции Python в ЯВА
"""

from dataclasses import dataclass, field
from typing import Dict, List, Any, Optional

@dataclass
class VariableInfo:
    """Информация о переменной ЯВА"""
    name: str
    type: str  # 'int', 'float', 'bool', 'string', 'array', 'object'
    initial_value: Any = None

@dataclass
class StepInfo:
    """Информация о шаге ЯВА"""
    id: str
    type: str  # 'assign', 'condition', 'compare', 'swap', 'call_function', 'return', 'generic'
    description: str
    parameters: List[Any] = field(default_factory=list)
    nextStep: Optional[str] = None
    conditionCases: Optional[List[Dict]] = None
    functionName: Optional[str] = None
    functionParameters: Optional[Dict] = None
    returnToStep: Optional[str] = None
    visualize: bool = True
    highlightElements: Optional[List[str]] = None
    highlightColor: Optional[str] = None
    
    def to_dict(self) -> Dict:
        """Преобразование в словарь"""
        result = {
            "id": self.id,
            "type": self.type,
            "description": self.description,
            "parameters": self.parameters,
            "visualize": self.visualize
        }
        
        if self.nextStep is not None:
            result["nextStep"] = self.nextStep
        if self.conditionCases is not None:
            result["conditionCases"] = self.conditionCases
        if self.functionName is not None:
            result["functionName"] = self.functionName
        if self.functionParameters is not None:
            result["functionParameters"] = self.functionParameters
        if self.returnToStep is not None:
            result["returnToStep"] = self.returnToStep
        if self.highlightElements is not None:
            result["highlightElements"] = self.highlightElements
        if self.highlightColor is not None:
            result["highlightColor"] = self.highlightColor
            
        return result