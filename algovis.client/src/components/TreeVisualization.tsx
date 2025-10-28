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
  breadthFirst: (root: TreeNode | null): number[] => {
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

  const validateNode = (node: TreeNode | null, path: string = 'root'): void => {
    if (!node) return;

    // Проверка на отрицательные значения
    if (node.value < 0) {
      errors.push(`Отрицательная вершина: ${node.value} (путь: ${path})`);
    }

    // Проверка на дубликаты
    if (values.has(node.value)) {
      errors.push(`Дубликат вершины: ${node.value} (путь: ${path})`);
    } else {
      values.add(node.value);
    }

    // Рекурсивная проверка детей
    if (node.left) {
      // Проверка свойства BST (если это BST)
      if (node.left.value >= node.value) {
        errors.push(`Нарушение свойства BST: левый ребенок ${node.left.value} >= родителя ${node.value}`);
      }
      validateNode(node.left, `${path} → left`);
    }

    if (node.right) {
      // Проверка свойства BST (если это BST)
      if (node.right.value <= node.value) {
        errors.push(`Нарушение свойства BST: правый ребенок ${node.right.value} <= родителя ${node.value}`);
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

export function TreeVisualization({ 
  tree, 
  highlightedNodes = [], 
  visitedNodes = [],
  currentNode,
  traversalResult = tree ? treeTraversals.preOrder(tree) : [] // По умолчанию прямой обход
}: TreeVisualizationProps) {
  const [validation, setValidation] = useState<{ isValid: boolean; errors: string[] }>({ isValid: true, errors: [] });

  useEffect(() => {
    if (tree) {
      setValidation(validateTree(tree));
    }
  }, [tree]);

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
    
    // Рендер линий к дочерним узлам ПЕРВЫМ
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
          className="transition-all duration-300"
        />
        
        {/* Текст значения */}
        <text
          x={node.x}
          y={node.y}
          textAnchor="middle"
          dominantBaseline="middle"
          fill={textColor}
          className="select-none font-medium text-sm pointer-events-none"
        >
          {node.value}
        </text>
      </g>
    );
    
    // Рекурсивно рендерим дочерние узлы
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
        <p className="text-muted-foreground">Дерево пусто</p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Валидационные ошибки */}
      {!validation.isValid && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4">
          <h3 className="text-red-800 font-semibold mb-2">Обнаружены ошибки в дереве:</h3>
          <ul className="text-red-700 text-sm list-disc list-inside space-y-1">
            {validation.errors.map((error, index) => (
              <li key={index}>{error}</li>
            ))}
          </ul>
        </div>
      )}

      {/* Результат обхода */}
      {traversalResult.length > 0 && (
        <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
          <h3 className="text-blue-800 font-semibold mb-2">Результат обхода:</h3>
          <div className="text-blue-700 text-sm">
            {traversalResult.join(' → ')}
          </div>
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

// Пример использования компонента с разными обходами
export const TreeWithTraversals: React.FC<{ tree: TreeNode | null }> = ({ tree }) => {
  const [currentTraversal, setCurrentTraversal] = useState<'preOrder' | 'inOrder' | 'postOrder' | 'breadthFirst'>('preOrder');
  
  const traversalResult = tree ? treeTraversals[currentTraversal](tree) : [];

  return (
    <div className="space-y-4">
      {/* Выбор типа обхода */}
      <div className="flex gap-2 flex-wrap">
        <button
          onClick={() => setCurrentTraversal('preOrder')}
          className={`px-3 py-2 text-sm rounded border ${
            currentTraversal === 'preOrder' 
              ? 'bg-blue-500 text-white border-blue-500' 
              : 'bg-white text-gray-700 border-gray-300'
          }`}
        >
          Прямой обход
        </button>
        <button
          onClick={() => setCurrentTraversal('inOrder')}
          className={`px-3 py-2 text-sm rounded border ${
            currentTraversal === 'inOrder' 
              ? 'bg-blue-500 text-white border-blue-500' 
              : 'bg-white text-gray-700 border-gray-300'
          }`}
        >
          Центрированный
        </button>
        <button
          onClick={() => setCurrentTraversal('postOrder')}
          className={`px-3 py-2 text-sm rounded border ${
            currentTraversal === 'postOrder' 
              ? 'bg-blue-500 text-white border-blue-500' 
              : 'bg-white text-gray-700 border-gray-300'
          }`}
        >
          Обратный
        </button>
        <button
          onClick={() => setCurrentTraversal('breadthFirst')}
          className={`px-3 py-2 text-sm rounded border ${
            currentTraversal === 'breadthFirst' 
              ? 'bg-blue-500 text-white border-blue-500' 
              : 'bg-white text-gray-700 border-gray-300'
          }`}
        >
          В ширину
        </button>
      </div>

      {/* Визуализация с выбранным обходом */}
      <TreeVisualization 
        tree={tree}
        traversalResult={traversalResult}
      />
    </div>
  );
};