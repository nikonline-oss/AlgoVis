"""
Реестр структур данных ЯВА
"""

from typing import Dict, Any, List
from dataclasses import dataclass

@dataclass
class StructureProperties:
    """Свойства структуры данных"""
    name: str
    description: str
    defaults: Dict[str, Any]
    access_patterns: Dict[str, str]  # Паттерны доступа к элементам
    validation_rules: List[str]      # Правила валидации

class StructureRegistry:
    """Реестр поддерживаемых структур данных"""
    
    def __init__(self):
        self._structures: Dict[str, StructureProperties] = {}
        self._init_default_structures()
    
    def _init_default_structures(self):
        """Инициализация стандартных структур"""
        
        # Массив
        self.register_structure(
            "array",
            StructureProperties(
                name="array",
                description="Одномерный массив элементов",
                defaults={
                    "len": 0,
                    "first": 0,
                    "last": 0,
                    "isEmpty": True,
                    "values": []
                },
                access_patterns={
                    "length": "struct.len",
                    "element": "struct[index]",
                    "first": "struct.first",
                    "last": "struct.last"
                },
                validation_rules=[
                    "Индексы должны быть в диапазоне [0, struct.len-1]",
                    "Доступ только для чтения к len, first, last, isEmpty"
                ]
            )
        )
        
        # Бинарное дерево
        self.register_structure(
            "binarytree",
            StructureProperties(
                name="binarytree",
                description="Бинарное дерево с узлами",
                defaults={
                    "value": 0,
                    "hasLeft": False,
                    "hasRight": False,
                    "isLeaf": True,
                    "height": 0,
                    "nodeCount": 0
                },
                access_patterns={
                    "value": "struct.value",
                    "left": "struct.left",
                    "right": "struct.right",
                    "is_leaf": "struct.isLeaf"
                },
                validation_rules=[
                    "Доступ к left/right только если hasLeft/hasRight = true",
                    "isLeaf = !hasLeft && !hasRight"
                ]
            )
        )
        
        # Связный список
        self.register_structure(
            "linkedlist",
            StructureProperties(
                name="linkedlist",
                description="Односвязный список",
                defaults={
                    "headValue": 0,
                    "hasNext": False,
                    "length": 0
                },
                access_patterns={
                    "head": "struct.headValue",
                    "next": "struct.next",
                    "length": "struct.length"
                },
                validation_rules=[
                    "Доступ к next только если hasNext = true",
                    "length >= 0"
                ]
            )
        )
        
        # Граф
        self.register_structure(
            "graph",
            StructureProperties(
                name="graph",
                description="Граф с узлами и ребрами",
                defaults={
                    "nodeCount": 0,
                    "edgeCount": 0,
                    "isDirected": False
                },
                access_patterns={
                    "nodes": "struct.nodes",
                    "edges": "struct.edges",
                    "adjacent": "struct.adjacent[node]"
                },
                validation_rules=[
                    "nodeCount >= 0",
                    "edgeCount >= 0",
                    "Для неориентированного графа ребра симметричны"
                ]
            )
        )
    
    def register_structure(self, name: str, properties: StructureProperties):
        """Регистрация новой структуры"""
        self._structures[name] = properties
    
    def get_structure(self, name: str) -> StructureProperties:
        """Получение свойств структуры"""
        if name not in self._structures:
            raise ValueError(f"Структура '{name}' не поддерживается")
        return self._structures[name]
    
    def get_defaults(self, name: str) -> Dict[str, Any]:
        """Получение значений по умолчанию"""
        props = self.get_structure(name)
        return props.defaults
    
    def get_supported_structures(self) -> List[str]:
        """Получение списка поддерживаемых структур"""
        return list(self._structures.keys())
    
    def validate_access(self, structure_type: str, access_expr: str) -> bool:
        """Валидация выражения доступа к структуре"""
        if structure_type not in self._structures:
            return False
        
        props = self._structures[structure_type]
        
        # Проверяем, соответствует ли выражение паттернам доступа
        for pattern in props.access_patterns.values():
            if pattern.replace("index", ".*") in access_expr:
                return True
        
        return False