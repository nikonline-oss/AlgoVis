import React, { useState, useEffect } from 'react';

export interface TreeNode {
  value: number;
  left?: TreeNode;
  right?: TreeNode;
  x?: number;
  y?: number;
}

interface TreeVisualizationProps {
  tree: TreeNode | null;
  highlightedNodes?: number[];
  visitedNodes?: number[];
  currentNode?: number;
  traversalResult?: number[];
}

interface TreeBounds {
  width: number;
  height: number;
}

// Функции обхода дерева
export const treeTraversals = {
  // Прямой обход (Pre-order): Корень → Лево → Право
  preOrder: (node: TreeNode | null, result: number[] = []): number[] => {
    if (!node) return result;
    
    result.push(node.value); // Корень
    treeTraversals.preOrder(node.left || null, result); // Лево
    treeTraversals.preOrder(node.right || null, result); // Право
    
    return result;
  },

  // Центрированный обход (In-order): Лево → Корень → Право
  inOrder: (node: TreeNode | null, result: number[] = []): number[] => {
    if (!node) return result;
    
    treeTraversals.inOrder(node.left || null, result); // Лево
    result.push(node.value); // Корень
    treeTraversals.inOrder(node.right || null, result); // Право
    
    return result;
  },

  // Обратный обход (Post-order): Лево → Право → Корень
  postOrder: (node: TreeNode | null, result: number[] = []): number[] => {
    if (!node) return result;
    
    treeTraversals.postOrder(node.left || null, result); // Лево
    treeTraversals.postOrder(node.right || null, result); // Право
    result.push(node.value); // Корень
    
    return result;
  },

  // Обход в ширину (BFS)
  levelorder: (root: TreeNode | null): number[] => {
    if (!root) return [];
    
    const result: number[] = [];
    const queue: TreeNode[] = [root];
    
    while (queue.length > 0) {
      const node = queue.shift()!;
      result.push(node.value);
      
      if (node.left) queue.push(node.left);
      if (node.right) queue.push(node.right);
    }
    
    return result;
  }
};

// Валидация дерева
export const validateTree = (tree: TreeNode | null): { isValid: boolean; errors: string[] } => {
  const errors: string[] = [];
  const values = new Set<number>();
  const duplicates = new Set<number>();

  const validateNode = (node: TreeNode | null, path: string = 'root'): void => {
    if (!node) return;

    // Проверка на отрицательные значения
    if (node.value < 0) {
      errors.push(`Отрицательная вершина: ${node.value} (путь: ${path})`);
    }

    // Проверка на дубликаты
    if (values.has(node.value)) {
      if (!duplicates.has(node.value)) {
        errors.push(`Дубликат вершины: ${node.value}`);
        duplicates.add(node.value);
      }
    } else {
      values.add(node.value);
    }

    // Проверка корректности значений
    if (isNaN(node.value)) {
      errors.push(`Некорректное значение вершины: ${node.value} (путь: ${path})`);
    }

    // Проверка на допустимый диапазон (опционально)
    if (node.value > 999 || node.value < -999) {
      errors.push(`Слишком большое/маленькое значение: ${node.value} (путь: ${path})`);
    }

    // Рекурсивная проверка детей
    if (node.left) {
      // Проверка свойства BST (если это BST)
      if (node.left.value >= node.value) {
        errors.push(`Нарушение свойства BST: левый ребенок ${node.left.value} >= родителя ${node.value} (путь: ${path})`);
      }
      validateNode(node.left, `${path} → left`);
    }

    if (node.right) {
      // Проверка свойства BST (если это BST)
      if (node.right.value <= node.value) {
        errors.push(`Нарушение свойства BST: правый ребенок ${node.right.value} <= родителя ${node.value} (путь: ${path})`);
      }
      validateNode(node.right, `${path} → right`);
    }
  };

  validateNode(tree);

  return {
    isValid: errors.length === 0,
    errors
  };
};

// Функция для генерации случайного сбалансированного BST
export const generateRandomBinarySearchTree = (nodeCount: number = 10): TreeNode | null => {
  if (nodeCount <= 0) return null;

  // Генерируем отсортированный массив уникальных значений
  const generateUniqueSortedValues = (count: number): number[] => {
    const values = new Set<number>();
    const min = 1;
    const max = 100;
    
    // Гарантируем уникальность значений
    while (values.size < count) {
      const value = Math.floor(Math.random() * (max - min + 1)) + min;
      values.add(value);
    }
    
    return Array.from(values).sort((a, b) => a - b);
  };

  const sortedValues = generateUniqueSortedValues(nodeCount);
  
  // Создаем сбалансированное BST из отсортированного массива
  const createBalancedBST = (arr: number[], start: number, end: number): TreeNode | null => {
    if (start > end) return null;
    
    const mid = Math.floor((start + end) / 2);
    const node: TreeNode = {
      value: arr[mid],
      left: createBalancedBST(arr, start, mid - 1),
      right: createBalancedBST(arr, mid + 1, end)
    };
    
    return node;
  };

  return createBalancedBST(sortedValues, 0, sortedValues.length - 1);
};

// Функция для генерации случайного BST (не обязательно сбалансированного)
export const generateRandomBST = (nodeCount: number = 10): TreeNode | null => {
  if (nodeCount <= 0) return null;

  // Генерируем уникальные значения
  const generateUniqueValues = (count: number): number[] => {
    const values = new Set<number>();
    const min = 1;
    const max = 100;
    
    while (values.size < count) {
      const value = Math.floor(Math.random() * (max - min + 1)) + min;
      values.add(value);
    }
    
    return Array.from(values);
  };

  const values = generateUniqueValues(nodeCount);
  let root: TreeNode | null = null;

  const insertNode = (node: TreeNode | null, value: number): TreeNode => {
    if (!node) {
      return { value };
    }
    
    if (value < node.value) {
      node.left = insertNode(node.left || null, value);
    } else {
      node.right = insertNode(node.right || null, value);
    }
    
    return node;
  };

  // Перемешиваем значения для лучшего баланса
  const shuffledValues = [...values].sort(() => Math.random() - 0.5);
  shuffledValues.forEach(value => {
    root = insertNode(root, value);
  });

  return root;
};

// Функция для генерации полного бинарного дерева
export const generateCompleteBinaryTree = (nodeCount: number = 10): TreeNode | null => {
  if (nodeCount <= 0) return null;

  // Генерируем уникальные значения
  const generateUniqueValues = (count: number): number[] => {
    const values = new Set<number>();
    const min = 1;
    const max = 100;
    
    while (values.size < count) {
      const value = Math.floor(Math.random() * (max - min + 1)) + min;
      values.add(value);
    }
    
    return Array.from(values);
  };

  const values = generateUniqueValues(nodeCount);
  const nodes: (TreeNode | null)[] = [];
  
  // Создаем узлы
  for (let i = 0; i < nodeCount; i++) {
    nodes[i] = { value: values[i] };
  }
  
  // Связываем узлы для создания полного дерева
  for (let i = 0; i < nodeCount; i++) {
    const leftIndex = 2 * i + 1;
    const rightIndex = 2 * i + 2;
    
    if (leftIndex < nodeCount) {
      nodes[i]!.left = nodes[leftIndex];
    }
    if (rightIndex < nodeCount) {
      nodes[i]!.right = nodes[rightIndex];
    }
  }
  
  return nodes[0] || null;
};

// Функция для генерации случайного дерева (не BST)
export const generateRandomBinaryTree = (nodeCount: number = 10): TreeNode | null => {
  if (nodeCount <= 0) return null;

  // Генерируем уникальные значения
  const generateUniqueValues = (count: number): number[] => {
    const values = new Set<number>();
    const min = 1;
    const max = 100;
    
    while (values.size < count) {
      const value = Math.floor(Math.random() * (max - min + 1)) + min;
      values.add(value);
    }
    
    return Array.from(values);
  };

  const values = generateUniqueValues(nodeCount);
  const nodes: TreeNode[] = values.map(value => ({ value }));
  
  // Случайным образом связываем узлы
  for (let i = 1; i < nodes.length; i++) {
    const parentIndex = Math.floor(Math.random() * i);
    const parent = nodes[parentIndex];
    
    if (!parent.left) {
      parent.left = nodes[i];
    } else if (!parent.right) {
      parent.right = nodes[i];
    } else {
      // Ищем другого родителя с пустым местом
      let j = i - 1;
      while (j >= 0) {
        const alternativeParent = nodes[j];
        if (!alternativeParent.left || !alternativeParent.right) {
          if (!alternativeParent.left) {
            alternativeParent.left = nodes[i];
          } else {
            alternativeParent.right = nodes[i];
          }
          break;
        }
        j--;
      }
    }
  }
  
  return nodes[0];
};

// Функция для подсчета высоты дерева
export const getTreeHeight = (node: TreeNode | null): number => {
  if (!node) return 0;
  return 1 + Math.max(getTreeHeight(node.left), getTreeHeight(node.right));
};

// Функция для подсчета количества узлов
export const countNodes = (node: TreeNode | null): number => {
  if (!node) return 0;
  return 1 + countNodes(node.left) + countNodes(node.right);
};

// Функция для сбора всех значений дерева
export const collectTreeValues = (node: TreeNode | null): number[] => {
  if (!node) return [];
  return [
    node.value,
    ...collectTreeValues(node.left),
    ...collectTreeValues(node.right)
  ];
};

// Функция для проверки наличия значения в дереве
export const containsValue = (node: TreeNode | null, value: number): boolean => {
  if (!node) return false;
  if (node.value === value) return true;
  return containsValue(node.left, value) || containsValue(node.right, value);
};

export function TreeVisualization({ 
  tree, 
  highlightedNodes = [], 
  visitedNodes = [],
  currentNode,
  traversalResult = tree ? treeTraversals.preOrder(tree) : [] // По умолчанию прямой обход
}: TreeVisualizationProps) {
  const [validation, setValidation] = useState<{ isValid: boolean; errors: string[] }>({ isValid: true, errors: [] });
  const [treeStats, setTreeStats] = useState({
    height: 0,
    nodeCount: 0,
    isBST: true
  });

  useEffect(() => {
    if (tree) {
      setValidation(validateTree(tree));
      
      // Собираем статистику дерева
      const height = getTreeHeight(tree);
      const nodeCount = countNodes(tree);
      const values = collectTreeValues(tree);
      const uniqueValues = new Set(values);
      const isBST = validation.isValid && uniqueValues.size === values.length;
      
      setTreeStats({
        height,
        nodeCount,
        isBST
      });
    } else {
      setTreeStats({
        height: 0,
        nodeCount: 0,
        isBST: true
      });
    }
  }, [tree, validation.isValid]);

  // Расчет позиций с использованием подхода "level-based positioning"
  const { positionedTree, bounds } = React.useMemo(() => {
    if (!tree) return { positionedTree: null, bounds: { width: 0, height: 0 } };

    const treeCopy = JSON.parse(JSON.stringify(tree)) as TreeNode;
    const levelNodes: TreeNode[][] = [];

    // Сначала собираем информацию об уровнях
    const collectLevelInfo = (node: TreeNode | null, level: number = 0): void => {
      if (!node) return;
      
      if (!levelNodes[level]) levelNodes[level] = [];
      levelNodes[level].push(node);
      
      collectLevelInfo(node.left || null, level + 1);
      collectLevelInfo(node.right || null, level + 1);
    };

    collectLevelInfo(treeCopy, 0);

    // Рассчитываем максимальную ширину для каждого уровня
    const levelWidths: number[] = [];
    levelNodes.forEach((nodes, level) => {
      levelWidths[level] = nodes.length * 120;
    });

    const maxLevelWidth = Math.max(...levelWidths);
    const svgWidth = Math.max(800, maxLevelWidth + 200);
    const svgHeight = Math.max(500, levelNodes.length * 120);

    // Позиционируем узлы
    levelNodes.forEach((nodes, level) => {
      const levelWidth = svgWidth - 200;
      const nodeSpacing = levelWidth / (nodes.length + 1);
      
      nodes.forEach((node, index) => {
        const x = 100 + nodeSpacing * (index + 1);
        const y = 60 + level * 120;
        
        node.x = x;
        node.y = y;
      });
    });

    return { 
      positionedTree: treeCopy, 
      bounds: { width: svgWidth, height: svgHeight } 
    };
  }, [tree]);

  const renderNode = (node: TreeNode | null, path: string = 'root'): React.ReactNode[] => {
    if (!node || node.x === undefined || node.y === undefined) return [];
    
    const elements: React.ReactNode[] = [];
    
    // Рендер линий к дочерним узлам ПЕРВЫМ (они должны быть под узлами)
    if (node.left && node.left.x !== undefined && node.left.y !== undefined) {
      elements.push(
        <line
          key={`line-left-${path}`}
          x1={node.x}
          y1={node.y + 25}
          x2={node.left.x}
          y2={node.left.y - 25}
          stroke="#64748b"
          strokeWidth="2"
          className="transition-all duration-300"
        />
      );
    }
    
    if (node.right && node.right.x !== undefined && node.right.y !== undefined) {
      elements.push(
        <line
          key={`line-right-${path}`}
          x1={node.x}
          y1={node.y + 25}
          x2={node.right.x}
          y2={node.right.y - 25}
          stroke="#64748b"
          strokeWidth="2"
          className="transition-all duration-300"
        />
      );
    }
    
    // Рендер текущего узла
    const isHighlighted = highlightedNodes.includes(node.value);
    const isVisited = visitedNodes.includes(node.value);
    const isCurrent = currentNode === node.value;
    
    let fillColor = '#94a3b8';
    let strokeColor = '#64748b';
    let strokeWidth = 2;
    let textColor = '#ffffff';
    
    if (isCurrent) {
      fillColor = '#ef4444';
      strokeColor = '#dc2626';
      strokeWidth = 3;
    } else if (isHighlighted) {
      fillColor = '#22c55e';
      strokeColor = '#16a34a';
      strokeWidth = 3;
    } else if (isVisited) {
      fillColor = '#86efac';
      strokeColor = '#4ade80';
      textColor = '#166534';
    }
    
    elements.push(
      <g key={`node-${path}`}>
        {/* Анимированное кольцо для текущего узла */}
        {isCurrent && (
          <circle
            cx={node.x}
            cy={node.y}
            r="30"
            fill="none"
            stroke={strokeColor}
            strokeWidth="2"
            opacity="0.6"
            className="animate-pulse"
          />
        )}
        
        {/* Основной круг узла */}
        <circle
          cx={node.x}
          cy={node.y}
          r="25"
          fill={fillColor}
          stroke={strokeColor}
          strokeWidth={strokeWidth}
          className="transition-all duration-300 cursor-pointer hover:opacity-90"
        />
        
        {/* Текст значения */}
        <text
          x={node.x}
          y={node.y}
          textAnchor="middle"
          dominantBaseline="middle"
          fill={textColor}
          className="select-none font-medium text-sm pointer-events-none"
          fontWeight="600"
        >
          {node.value}
        </text>
      </g>
    );
    
    // Рекурсивно рендерим дочерние узлы (они будут поверх линий)
    if (node.left) {
      elements.push(...renderNode(node.left, `${path}-L`));
    }
    if (node.right) {
      elements.push(...renderNode(node.right, `${path}-R`));
    }
    
    return elements;
  };

  if (!positionedTree) {
    return (
      <div className="flex items-center justify-center min-h-[400px] bg-card border rounded-lg">
        <div className="text-center space-y-2">
          <p className="text-muted-foreground">Дерево пусто</p>
          <p className="text-sm text-muted-foreground">Сгенерируйте дерево или вставьте значения</p>
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-4">
     

      {/* Валидационные ошибки */}
      {!validation.isValid && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <h3 className="text-red-800 font-semibold mb-2 flex items-center gap-2">
            <span>⚠️</span>
            Обнаружены ошибки в дереве:
          </h3>
          <ul className="text-red-700 text-sm list-disc list-inside space-y-1 max-h-32 overflow-y-auto">
            {validation.errors.map((error, index) => (
              <li key={index} className="py-1">{error}</li>
            ))}
          </ul>
        </div>
      )}

      {/* Визуализация дерева */}
      <div className="bg-card border rounded-lg p-4 overflow-auto">
        <svg 
          width={bounds.width} 
          height={bounds.height} 
          className="mx-auto"
        >
          {renderNode(positionedTree)}
        </svg>
        
        <div className="flex justify-center flex-wrap gap-4 mt-4 text-sm">
          <div className="flex items-center space-x-2">
            <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: '#94a3b8', borderColor: '#64748b' }} />
            <span className="text-xs">Необработанный</span>
          </div>
          <div className="flex items-center space-x-2">
            <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: '#ef4444', borderColor: '#dc2626' }} />
            <span className="text-xs">Текущий</span>
          </div>
          <div className="flex items-center space-x-2">
            <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: '#22c55e', borderColor: '#16a34a' }} />
            <span className="text-xs">Обрабатывается</span>
          </div>
          <div className="flex items-center space-x-2">
            <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: '#86efac', borderColor: '#4ade80' }} />
            <span className="text-xs">Посещённый</span>
          </div>
        </div>
      </div>
    </div>
  );
}

// Компонент для генерации деревьев
interface TreeGeneratorProps {
  onTreeGenerated: (tree: TreeNode | null) => void;
  nodeCount?: number;
}

export const TreeGenerator: React.FC<TreeGeneratorProps> = ({ onTreeGenerated, nodeCount = 10 }) => {
  const [treeType, setTreeType] = useState<'bst' | 'balanced' | 'complete' | 'random'>('bst');
  const [customNodeCount, setCustomNodeCount] = useState(nodeCount);
  const [minValue, setMinValue] = useState(1);
  const [maxValue, setMaxValue] = useState(100);
  
  const generateTree = useCallback((type: 'bst' | 'balanced' | 'complete' | 'random' = treeType, count: number = customNodeCount) => {
    let tree: TreeNode | null = null;
    
    // Функция для генерации уникальных значений
    const generateUniqueValues = (count: number): number[] => {
      const values = new Set<number>();
      const range = maxValue - minValue + 1;
      
      if (count > range) {
        throw new Error(`Невозможно сгенерировать ${count} уникальных значений в диапазоне ${minValue}-${maxValue}`);
      }
      
      while (values.size < count) {
        const value = Math.floor(Math.random() * range) + minValue;
        values.add(value);
      }
      
      return Array.from(values);
    };
    
    try {
      switch (type) {
        case 'bst':
          tree = generateRandomBST(count);
          break;
          
        case 'balanced':
          tree = generateRandomBinarySearchTree(count);
          break;
          
        case 'complete':
          const completeValues = generateUniqueValues(count);
          const nodes: (TreeNode | null)[] = [];
          
          for (let i = 0; i < count; i++) {
            nodes[i] = { value: completeValues[i] };
          }
          
          for (let i = 0; i < count; i++) {
            const leftIndex = 2 * i + 1;
            const rightIndex = 2 * i + 2;
            
            if (leftIndex < count) {
              nodes[i]!.left = nodes[leftIndex];
            }
            if (rightIndex < count) {
              nodes[i]!.right = nodes[rightIndex];
            }
          }
          
          tree = nodes[0] || null;
          break;
          
        case 'random':
          tree = generateRandomBinaryTree(count);
          break;
      }
      
      onTreeGenerated(tree);
    } catch (error) {
      alert(error instanceof Error ? error.message : 'Ошибка генерации дерева');
    }
  }, [treeType, customNodeCount, minValue, maxValue, onTreeGenerated]);
  
  useEffect(() => {
    generateTree();
  }, [generateTree]);
  
  return (
    <div className="space-y-4 p-4 bg-gray-50 rounded-lg border">
      <h3 className="font-semibold text-gray-800">Генератор деревьев</h3>
      
      <div className="space-y-3">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">Тип дерева</label>
          <div className="grid grid-cols-2 gap-2">
            <button
              onClick={() => setTreeType('bst')}
              className={`px-3 py-2 text-sm rounded border ${
                treeType === 'bst' 
                  ? 'bg-blue-500 text-white border-blue-500' 
                  : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
              }`}
            >
              BST
            </button>
            <button
              onClick={() => setTreeType('balanced')}
              className={`px-3 py-2 text-sm rounded border ${
                treeType === 'balanced' 
                  ? 'bg-blue-500 text-white border-blue-500' 
                  : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
              }`}
            >
              Сбалансированное
            </button>
            <button
              onClick={() => setTreeType('complete')}
              className={`px-3 py-2 text-sm rounded border ${
                treeType === 'complete' 
                  ? 'bg-blue-500 text-white border-blue-500' 
                  : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
              }`}
            >
              Полное
            </button>
            <button
              onClick={() => setTreeType('random')}
              className={`px-3 py-2 text-sm rounded border ${
                treeType === 'random' 
                  ? 'bg-blue-500 text-white border-blue-500' 
                  : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
              }`}
            >
              Случайное
            </button>
          </div>
        </div>
        
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1">
            Количество вершин: {customNodeCount}
          </label>
          <input
            type="range"
            min="3"
            max="20"
            value={customNodeCount}
            onChange={(e) => setCustomNodeCount(parseInt(e.target.value))}
            className="w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer"
          />
          <div className="flex justify-between text-xs text-gray-500 mt-1">
            <span>3</span>
            <span>20</span>
          </div>
        </div>
        
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Минимальное значение</label>
            <input
              type="number"
              min="1"
              max="100"
              value={minValue}
              onChange={(e) => setMinValue(Math.min(parseInt(e.target.value) || 1, maxValue - 1))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Максимальное значение</label>
            <input
              type="number"
              min="2"
              max="200"
              value={maxValue}
              onChange={(e) => setMaxValue(Math.max(parseInt(e.target.value) || 100, minValue + 1))}
              className="w-full px-3 py-2 border border-gray-300 rounded-md"
            />
          </div>
        </div>
        
        <div className="pt-2">
          <button
            onClick={() => generateTree()}
            className="w-full px-4 py-2 bg-gradient-to-r from-blue-500 to-blue-600 text-white rounded-md hover:from-blue-600 hover:to-blue-700 font-medium"
          >
            Сгенерировать дерево
          </button>
          <p className="text-xs text-gray-500 mt-2 text-center">
            Все вершины будут иметь уникальные значения в диапазоне {minValue}-{maxValue}
          </p>
        </div>
      </div>
    </div>
  );
};

// Пример использования компонента с разными обходами
export const TreeWithTraversals: React.FC<{ tree: TreeNode | null }> = ({ tree }) => {
  const [currentTraversal, setCurrentTraversal] = useState<'preOrder' | 'inOrder' | 'postOrder' | 'levelorder'>('preOrder');
  
  const traversalResult = tree ? treeTraversals[currentTraversal](tree) : [];

  const traversalNames = {
    preOrder: 'Прямой обход (Pre-order)',
    inOrder: 'Центрированный обход (In-order)',
    postOrder: 'Обратный обход (Post-order)',
    levelorder: 'Обход в ширину (BFS)'
  };

  return (
    <div className="space-y-4">
      {/* Выбор типа обхода */}
      <div className="bg-white border rounded-lg p-4">
        
        <h3 className="font-semibold text-gray-800 mb-3">Тип обхода дерева</h3>
        <div className="flex gap-2 flex-wrap">
          {(['preOrder', 'inOrder', 'postOrder', 'levelorder'] as const).map((traversal) => (
            <button
              key={traversal}
              onClick={() => setCurrentTraversal(traversal)}
              className={`px-3 py-2 text-sm rounded-md border transition-colors ${
                currentTraversal === traversal 
                  ? 'bg-blue-500 text-white border-blue-500' 
                  : 'bg-white text-gray-700 border-gray-300 hover:bg-gray-50'
              }`}
            >
              {traversalNames[traversal]}
            </button>
          ))}
        </div>
      </div>

      {/* Визуализация с выбранным обходом */}
      <TreeVisualization 
        tree={tree}
        traversalResult={traversalResult}
      />
    </div>
  );
};