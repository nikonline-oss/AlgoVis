"""Промежуточное представление для ЯВА"""
from dataclasses import dataclass, field
from typing import List, Dict, Any, Optional, Union
from enum import Enum


class StructureType(Enum):
    ARRAY = "array"
    BINARY_TREE = "binarytree"
    GRAPH = "graph"
    LINKED_LIST = "linkedlist"


class StepType(Enum):
    ASSIGN = "assign"
    COMPARE = "compare"
    SWAP = "swap"
    CONDITION = "condition"
    CALL_FUNCTION = "call_function"
    RETURN = "return"
    GENERIC = "generic"


@dataclass
class Variable:
    name: str
    type: str  # "int", "float", "bool", "string", "array", "object"
    initial_value: Any
    description: str = ""


@dataclass
class ConditionCase:
    condition: str
    next_step: str
    description: str = ""


@dataclass
class Step:
    id: str
    type: StepType
    description: str
    parameters: List[str] = field(default_factory=list)
    next_step: Optional[str] = None
    condition_cases: List[ConditionCase] = field(default_factory=list)
    function_name: Optional[str] = None
    function_parameters: Dict[str, str] = field(default_factory=dict)
    return_to_step: Optional[str] = None
    visualize: bool = True
    highlight_elements: List[str] = field(default_factory=list)
    highlight_color: Optional[str] = None
    metadata: Dict[str, Any] = field(default_factory=dict)


@dataclass
class Function:
    name: str
    description: str
    parameters: List[str]
    entry_point: str
    steps: List[Step]
    return_type: str = "void"


@dataclass
class Algorithm:
    name: str
    description: str
    structure_type: StructureType
    variables: List[Variable]
    functions: List[Function]
    steps: List[Step]
    metadata: Dict[str, Any] = field(default_factory=dict)