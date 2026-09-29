# ir_to_yava.py (ПЕРЕРАБОТАННАЯ СИСТЕМА СВЯЗЫВАНИЯ)

import json
from typing import Dict, List, Any, Optional, Tuple, Set
from dataclasses import dataclass, field
from .ir_nodes import *
from .ir_structures import *
from .ir_control import *


@dataclass
class StepInfo:
    """
    Информация о шаге алгоритма в формате ЯВА.
    
    Attributes:
        id (str): Уникальный идентификатор шага
        type (str): Тип шага (assign, compare, condition, etc.)
        description (str): Описание шага для отображения
        parameters (List[Any]): Параметры шага
        nextStep (Optional[str]): ID следующего шага
        conditionCases (Optional[List[Dict]]): Список условий и переходов для условных шагов
        functionName (Optional[str]): Название функции (если шаг вызывает функцию)
        functionParameters (Optional[Dict]): Параметры функции
        returnToStep (Optional[str]): Шаг для возврата после вызова функции
        visualize (bool): Флаг визуализации шага
        highlightElements (Optional[List[str]]): Элементы для подсветки
        highlightColor (Optional[str]): Цвет подсветки
    """
    id: str
    type: str
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
        """
        Преобразует объект StepInfo в словарь.
        
        Returns:
            Dict: Словарь с данными шага
        """
        result = {
            "id": self.id,
            "type": self.type,
            "description": self.description,
            "parameters": self.parameters,
            "visualize": self.visualize
        }
        
        if self.nextStep:
            result["nextStep"] = self.nextStep
        if self.conditionCases:
            result["conditionCases"] = self.conditionCases
        if self.functionName:
            result["functionName"] = self.functionName
        if self.functionParameters:
            result["functionParameters"] = self.functionParameters
        if self.returnToStep:
            result["returnToStep"] = self.returnToStep
        if self.highlightElements:
            result["highlightElements"] = self.highlightElements
        if self.highlightColor:
            result["highlightColor"] = self.highlightColor
            
        return result


@dataclass
class BlockResult:
    """
    Результат трансляции блока операторов.
    
    Attributes:
        entry_step (str): ID первого шага блока
        exit_steps (List[str]): ID всех выходных шагов блока
        steps (List[StepInfo]): Все шаги в блоке
    """
    entry_step: str  # ID первого шага
    exit_steps: List[str]  # ID всех выходных шагов
    steps: List[StepInfo]  # Все шаги в блоке
    
    def add_step(self, step: StepInfo, is_exit: bool = False):
        """
        Добавляет шаг в блок.
        
        Args:
            step (StepInfo): Добавляемый шаг
            is_exit (bool): Является ли шаг выходным
        """
        self.steps.append(step)
        if is_exit:
            self.exit_steps.append(step.id)
    
    @classmethod
    def empty(cls) -> 'BlockResult':
        """
        Создает пустой BlockResult.
        
        Returns:
            BlockResult: Пустой результат блока
        """
        return cls(entry_step="", exit_steps=[], steps=[])
    
    @classmethod
    def single(cls, step: StepInfo) -> 'BlockResult':
        """
        Создает BlockResult с одним шагом.
        
        Args:
            step (StepInfo): Единственный шаг
            
        Returns:
            BlockResult: Результат с одним шагом
        """
        return cls(
            entry_step=step.id,
            exit_steps=[step.id],
            steps=[step]
        )


class IRToYAVA:
    """
    Транслятор промежуточного представления (IR) в формат ЯВА.
    
    Осуществляет преобразование IR представления в структурированный JSON формат,
    пригодный для визуализации алгоритмов в системе ЯВА.
    
    Attributes:
        program_name (str): Название программы
        description (str): Описание программы
        structure_type (str): Тип структуры данных
        steps (Dict[str, StepInfo]): Словарь шагов по ID
        variables (List[Dict]): Список переменных
        functions (List[Dict]): Список функций
        step_counter (int): Счетчик для генерации уникальных ID шагов
        variable_set (Set[str]): Множество зарегистрированных переменных
    """
    
    def __init__(self, 
                 program_name: str = "Algorithm", 
                 description: str = "Сгенерированный алгоритм",
                 structure_type: str = "array"):
        """
        Инициализирует транслятор.
        
        Args:
            program_name (str): Название программы
            description (str): Описание программы
            structure_type (str): Тип структуры данных
        """
        self.program_name = program_name
        self.description = description
        self.structure_type = structure_type
        
        self.steps: Dict[str, StepInfo] = {}
        self.variables: List[Dict] = []
        self.functions: List[Dict] = []
        
        self.step_counter = 0
        self.variable_set: Set[str] = set()
        
        self._register_system_variables()
    
    def _register_system_variables(self):
        """Регистрация системных переменных."""
        system_vars = [
        ]
        
        for var in system_vars:
            if var["name"] not in self.variable_set:
                self.variables.append(var)
                self.variable_set.add(var["name"])
    
    def _new_step_id(self, prefix: str = "step") -> str:
        """
        Генерирует новый уникальный ID шага.
        
        Args:
            prefix (str): Префикс для ID
            
        Returns:
            str: Уникальный ID шага
        """
        self.step_counter += 1
        return f"{prefix}_{self.step_counter}"
    
    def _create_step(self, 
                    step_type: str, 
                    description: str, 
                    parameters: List = None,
                    **kwargs) -> StepInfo:
        """
        Создает новый шаг.
        
        Args:
            step_type (str): Тип шага
            description (str): Описание шага
            parameters (List): Параметры шага
            **kwargs: Дополнительные параметры шага
            
        Returns:
            StepInfo: Созданный шаг
        """
        step_id = kwargs.pop('id', self._new_step_id(prefix=step_type))
        
        step = StepInfo(
            id=step_id,
            type=step_type,
            description=description,
            parameters=parameters or [],
            **kwargs
        )
        
        self.steps[step_id] = step
        return step
    
    def _register_variable(self, name: str, var_type: str = "int", initial_value: Any = None):
        """
        Регистрирует переменную в списке переменных ЯВА.
        
        Args:
            name (str): Имя переменной
            var_type (str): Тип переменной
            initial_value (Any): Начальное значение
        """
        # Не регистрируем выражения с квадратными скобками как переменные
        if '[' in name or ']' in name:
            return
            
        if name not in self.variable_set:
            var_info = {
                "name": name,
                "type": var_type,
                "initialValue": initial_value
            }
            
            # Определяем тип по значению, если не указан
            if var_type == "int" and initial_value is not None:
                try:
                    if isinstance(initial_value, list):
                        var_info["type"] = "array"
                    elif isinstance(initial_value, str) and initial_value.startswith('['):
                        var_info["type"] = "array"
                except:
                    pass
            
            self.variables.append(var_info)
            self.variable_set.add(name)
    
    def _link_blocks(self, block1: BlockResult, block2: BlockResult) -> BlockResult:
        """
        Связывает два блока последовательно.
        
        Args:
            block1 (BlockResult): Первый блок
            block2 (BlockResult): Второй блок
            
        Returns:
            BlockResult: Объединенный блок
        """
        if not block1.steps:
            return block2
        if not block2.steps:
            return block1
        
        # Связываем все выходы первого блока с входом второго блока
        for exit_step_id in block1.exit_steps:
            if exit_step_id in self.steps:
                exit_step = self.steps[exit_step_id]
                if not exit_step.nextStep and exit_step.type not in ["condition", "return"]:
                    exit_step.nextStep = block2.entry_step
        
        # Объединяем шаги
        all_steps = block1.steps + block2.steps
        
        return BlockResult(
            entry_step=block1.entry_step,
            exit_steps=block2.exit_steps,
            steps=all_steps
        )
    
    def _link_blocks_with_merge(self, blocks: List[BlockResult], merge_step: StepInfo) -> BlockResult:
        """
        Связывает несколько блоков с точкой слияния.
        
        Args:
            blocks (List[BlockResult]): Список блоков
            merge_step (StepInfo): Точка слияния
            
        Returns:
            BlockResult: Объединенный блок с точкой слияния
        """
        if not blocks:
            return BlockResult.single(merge_step)
        
        all_steps = []
        entry_step = blocks[0].entry_step if blocks else merge_step.id
        
        for block in blocks:
            all_steps.extend(block.steps)
            # Связываем выходы каждого блока с точкой слияния
            for exit_step_id in block.exit_steps:
                if exit_step_id in self.steps:
                    exit_step = self.steps[exit_step_id]
                    if not exit_step.nextStep and exit_step.type not in ["condition", "return"]:
                        exit_step.nextStep = merge_step.id
        
        all_steps.append(merge_step)
        
        return BlockResult(
            entry_step=entry_step,
            exit_steps=[merge_step.id],
            steps=all_steps
        )
    
    def _translate_assign(self, node: IRAssign) -> BlockResult:
        """
        Трансляция присваивания.
        
        Args:
            node (IRAssign): Узел присваивания IR
            
        Returns:
            BlockResult: Блок с шагом присваивания
        """
        # Регистрируем переменную, если это не элемент массива
        if '[' not in node.target and ']' not in node.target:
            self._register_variable(node.target, initial_value=None)
        
        # Определяем описание и параметры визуализации
        if '[' in node.target and ']' in node.target:
            description = f"Присваивание элемента {node.target} = {node.value}"
            # Извлекаем индекс из target
            import re
            match = re.search(r'\[(.*?)\]', node.target)
            if match:
                index = match.group(1)
                highlight_elements = [index]
                return BlockResult.single(self._create_step(
                    step_type="assign",
                    description=description,
                    parameters=[node.target, node.value],
                    visualize=True,
                    highlightElements=highlight_elements,
                    highlightColor="blue"
                ))
        else:
            description = f"Присваивание {node.target} = {node.value}"
        
        return BlockResult.single(self._create_step(
            step_type="assign",
            description=description,
            parameters=[node.target, node.value],
            visualize=True
        ))
    
    def _translate_compare(self, node: IRCompare) -> BlockResult:
        """
        Трансляция сравнения.
        
        Args:
            node (IRCompare): Узел сравнения IR
            
        Returns:
            BlockResult: Блок с шагом сравнения
        """
        # Извлекаем индексы для подсветки
        highlight_elements = []
        import re
        
        # Ищем индексы в левой и правой частях
        for expr in [node.left, node.right]:
            match = re.search(r'\[(.*?)\]', expr)
            if match:
                index = match.group(1).strip()
                highlight_elements.append(index)
        
        return BlockResult.single(self._create_step(
            step_type="compare",
            description=f"Сравнение {node.left} и {node.right}",
            parameters=[node.left, node.right],
            visualize=True,
            highlightElements=highlight_elements if highlight_elements else None,
            highlightColor="yellow"
        ))
    
    def _translate_swap(self, node: IRSwap) -> BlockResult:
        """
        Трансляция обмена элементов.
        
        Args:
            node (IRSwap): Узел обмена IR
            
        Returns:
            BlockResult: Блок с шагом обмена
        """
        return BlockResult.single(self._create_step(
            step_type="swap",
            description=f"Обмен элементов {node.idx1} и {node.idx2}",
            parameters=[node.idx1, node.idx2],
            visualize=True,
            highlightElements=[str(node.idx1), str(node.idx2)],
            highlightColor="red"
        ))
    
    def _translate_if(self, node: IRIf) -> BlockResult:
        """
        Трансляция условного оператора if.
        
        Args:
            node (IRIf): Узел условного оператора IR
            
        Returns:
            BlockResult: Блок с условным оператором
        """
        # Шаг условия
        cond_step = self._create_step(
            step_type="condition",
            description=f"Проверка: {node.condition}",
            parameters=[node.condition],
            conditionCases=[],
            visualize=True
        )
        
        # Транслируем true ветку
        true_result = self._translate_block(node.true_body)
        
        # Транслируем false ветку (если есть)
        false_result = None
        if node.false_body:
            false_result = self._translate_block(node.false_body)
        
        # Создаем точку слияния
        merge_step = self._create_step(
            step_type="generic",
            description="Продолжение после условия",
            parameters=[],
            visualize=False
        )
        
        # Настраиваем переходы условия
        if true_result.steps:
            cond_step.conditionCases.append({
                "condition": "true",
                "nextStep": true_result.entry_step
            })
        else:
            # Если true ветка пустая, переходим сразу на слияние
            cond_step.conditionCases.append({
                "condition": "true",
                "nextStep": merge_step.id
            })
        
        if false_result and false_result.steps:
            cond_step.conditionCases.append({
                "condition": "false",
                "nextStep": false_result.entry_step
            })
        else:
            # Если false ветки нет или она пустая, переходим на слияние
            cond_step.conditionCases.append({
                "condition": "false",
                "nextStep": merge_step.id
            })
        
        # Собираем все шаги
        all_steps = [cond_step]
        
        # Добавляем true ветку
        if true_result.steps:
            all_steps.extend(true_result.steps)
            # Связываем выходы true ветки с точкой слияния
            for exit_id in true_result.exit_steps:
                if exit_id in self.steps:
                    exit_step = self.steps[exit_id]
                    if not exit_step.nextStep and exit_step.type not in ["condition", "return"]:
                        exit_step.nextStep = merge_step.id
        
        # Добавляем false ветку
        if false_result and false_result.steps:
            all_steps.extend(false_result.steps)
            # Связываем выходы false ветки с точкой слияния
            for exit_id in false_result.exit_steps:
                if exit_id in self.steps:
                    exit_step = self.steps[exit_id]
                    if not exit_step.nextStep and exit_step.type not in ["condition", "return"]:
                        exit_step.nextStep = merge_step.id
        
        all_steps.append(merge_step)
        
        return BlockResult(
            entry_step=cond_step.id,
            exit_steps=[merge_step.id],
            steps=all_steps
        )
    
    def _translate_for(self, node: IRFor) -> BlockResult:
        """
        Трансляция цикла for.
        
        Args:
            node (IRFor): Узел цикла for IR
            
        Returns:
            BlockResult: Блок с циклом for
        """
        # Регистрируем переменную цикла
        self._register_variable(node.var, "int", node.start)
        
        all_steps = []
        
        # 1. Инициализация переменной цикла
        init_step = self._create_step(
            step_type="assign",
            description=f"Инициализация {node.var} = {node.start}",
            parameters=[node.var, node.start],
            visualize=False
        )
        all_steps.append(init_step)
        
        # 2. Условие цикла
        cond_step = self._create_step(
            step_type="condition",
            description=f"Проверка {node.var} < {node.end}",
            parameters=[f"{node.var} < {node.end}"],
            conditionCases=[],
            visualize=True
        )
        all_steps.append(cond_step)
        
        # Связываем инициализацию с условием
        init_step.nextStep = cond_step.id
        
        # 3. Тело цикла
        body_result = self._translate_block(node.body)
        if body_result.steps:
            all_steps.extend(body_result.steps)
            
            # Связываем условие с телом (если true)
            cond_step.conditionCases.append({
                "condition": "true",
                "nextStep": body_result.entry_step
            })
        else:
            # Если тело пустое, условие true ведет на инкремент
            pass
        
        # 4. Инкремент переменной цикла
        incr_step = self._create_step(
            step_type="assign",
            description=f"Инкремент {node.var} = {node.var} + {node.step}",
            parameters=[node.var, f"{node.var} + {node.step}"],
            visualize=False
        )
        all_steps.append(incr_step)
        
        # Инкремент ведет обратно к условию
        incr_step.nextStep = cond_step.id
        
        # 5. Выход из цикла
        exit_step = self._create_step(
            step_type="generic",
            description="Выход из цикла",
            parameters=[],
            visualize=False
        )
        all_steps.append(exit_step)
        
        # Настраиваем выход из условия (если false)
        cond_step.conditionCases.append({
            "condition": "false",
            "nextStep": exit_step.id
        })
        
        # Связываем выходы тела с инкрементом
        if body_result.steps:
            for exit_id in body_result.exit_steps:
                if exit_id in self.steps:
                    body_exit = self.steps[exit_id]
                    if not body_exit.nextStep and body_exit.type not in ["condition", "return"]:
                        body_exit.nextStep = incr_step.id
        else:
            # Если тело пустое, условие true ведет сразу на инкремент
            cond_step.conditionCases[0]["nextStep"] = incr_step.id
        
        return BlockResult(
            entry_step=init_step.id,
            exit_steps=[exit_step.id],
            steps=all_steps
        )
    
    def _translate_statement(self, node) -> BlockResult:
        """
        Трансляция одного оператора.
        
        Args:
            node: Узел IR оператора
            
        Returns:
            BlockResult: Блок с транслированным оператором
        """
        if isinstance(node, IRAssign):
            return self._translate_assign(node)
        elif isinstance(node, IRCompare):
            return self._translate_compare(node)
        elif isinstance(node, IRSwap):
            return self._translate_swap(node)
        elif isinstance(node, IRIf):
            return self._translate_if(node)
        elif isinstance(node, IRFor):
            return self._translate_for(node)
        elif isinstance(node, IRWhile):
            # Заглушка для while
            return BlockResult.single(self._create_step(
                step_type="generic",
                description="Цикл while",
                parameters=[],
                visualize=False
            ))
        elif isinstance(node, IRReturn):
            return BlockResult.single(self._create_step(
                step_type="return",
                description="Возврат из функции",
                parameters=[node.value] if node.value else [],
                visualize=False
            ))
        else:
            return BlockResult.single(self._create_step(
                step_type="generic",
                description=f"Операция: {type(node).__name__}",
                parameters=[],
                visualize=False
            ))
    
    def _translate_block(self, statements: List) -> BlockResult:
        """
        Трансляция блока операторов.
        
        Args:
            statements (List): Список операторов IR
            
        Returns:
            BlockResult: Блок с транслированными операторами
        """
        if not statements:
            return BlockResult.empty()
        
        result = BlockResult.empty()
        
        for stmt in statements:
            stmt_result = self._translate_statement(stmt)
            
            if not result.steps:
                result = stmt_result
            else:
                result = self._link_blocks(result, stmt_result)
        
        return result
    
    def _collect_variables_from_ir(self, ir_program: Dict):
        """
        Сбор переменных из IR программы.
        
        Args:
            ir_program (Dict): IR программа
        """
        for var_name, var_info in ir_program.get("variables", {}).items():
            # Не добавляем выражения с квадратными скобками
            if '[' in var_name or ']' in var_name:
                continue
                
            if var_name not in self.variable_set:
                self.variables.append(var_info)
                self.variable_set.add(var_name)
    
    def translate(self, ir_program: Dict) -> Dict[str, Any]:
        """
        Основной метод трансляции IR → ЯВА.
        
        Args:
            ir_program (Dict): IR программа
            
        Returns:
            Dict[str, Any]: Программа в формате ЯВА
        """
        # Очищаем состояние
        self.steps.clear()
        self.step_counter = 0
        
        # Собираем переменные
        self._collect_variables_from_ir(ir_program)
        
        # Транслируем основной блок операторов
        main_result = self._translate_block(ir_program.get("statements", []))
        
        # Создаем start шаг
        start_step = self._create_step(
            step_type="generic",
            description="Начало алгоритма",
            parameters=[],
            id="start",
            visualize=True
        )
        
        # Создаем end шаг
        end_step = self._create_step(
            step_type="generic",
            description="Алгоритм завершен",
            parameters=[],
            id="end",
            visualize=True
        )
        
        # Связываем start с первым шагом main_result
        if main_result.steps:
            start_step.nextStep = main_result.entry_step
        else:
            start_step.nextStep = end_step.id
        
        # Связываем выходы main_result с end
        for exit_id in main_result.exit_steps:
            if exit_id in self.steps:
                exit_step = self.steps[exit_id]
                if not exit_step.nextStep and exit_step.type not in ["condition", "return"]:
                    exit_step.nextStep = end_step.id
        
        # Если main_result не имеет выходов, связываем start с end
        if not main_result.exit_steps and main_result.steps:
            # Находим последний шаг
            last_step = main_result.steps[-1]
            if last_step.type not in ["condition", "return"]:
                last_step.nextStep = end_step.id
        
        # Собираем все шаги
        all_steps = [start_step] + main_result.steps + [end_step]
        
        # Обновляем словарь steps
        for step in all_steps:
            self.steps[step.id] = step
        
        # Формируем финальный JSON
        return {
            "name": self.program_name,
            "description": self.description,
            "structureType": self.structure_type,
            "variables": self.variables,
            "functions": self.functions,
            "steps": [step.to_dict() for step in all_steps]
        }


def test_new_system():
    """
    Тест новой системы связывания.
    
    Returns:
        Dict: Результат тестовой трансляции
    """
    # Создаем простой IR для тестирования
    from ir_nodes import IRAssign, IRCompare
    from ir_control import IRIf, IRFor
    
    # Пример: if с присваиваниями
    ir_program = {
        "variables": {
            "x": {"name": "x", "type": "int", "initialValue": 0},
            "y": {"name": "y", "type": "int", "initialValue": 0}
        },
        "statements": [
            IRAssign("x", "5"),
            IRCompare("x", "3"),
            IRIf("last_comparison > 0", [
                IRAssign("y", "10"),
                IRAssign("y", "y + 1")
            ], [])
        ]
    }
    
    translator = IRToYAVA(
        program_name="TestSystem",
        description="Тест новой системы связывания",
        structure_type="array"
    )
    
    result = translator.translate(ir_program)
    
    # Проверяем связи
    print("Проверка связей:")
    for step in result["steps"]:
        if step["type"] == "condition":
            print(f"  {step['id']}: {step['type']} -> {step.get('conditionCases')}")
        else:
            print(f"  {step['id']}: {step['type']} -> {step.get('nextStep', 'END')}")
    
    return result


if __name__ == "__main__":
    test_result = test_new_system()
    with open("test_system.json", "w", encoding="utf-8") as f:
        json.dump(test_result, f, ensure_ascii=False, indent=2)
    print("Тест сохранен в test_system.json")