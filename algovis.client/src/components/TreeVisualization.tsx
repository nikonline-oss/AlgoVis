import React from 'react';

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
}

export function TreeVisualization({ 
  tree, 
  highlightedNodes = [], 
  visitedNodes = [],
  currentNode 
}: TreeVisualizationProps) {
  if (!tree) {
    return (
      <div className="flex items-center justify-center min-h-[400px] bg-card border rounded-lg">
        <p className="text-muted-foreground">Дерево пусто</p>
      </div>
    );
  }

  const calculatePositions = (node: TreeNode | null, x: number, y: number, offset: number): void => {
    if (!node) return;
    
    node.x = x;
    node.y = y;
    
    if (node.left) {
      calculatePositions(node.left, x - offset, y + 80, offset / 2);
    }
    if (node.right) {
      calculatePositions(node.right, x + offset, y + 80, offset / 2);
    }
  };

  const renderNode = (node: TreeNode | null, path: string = 'root'): React.ReactNode[] => {
    if (!node || node.x === undefined || node.y === undefined) return [];
    
    const elements: React.ReactNode[] = [];
    
    // Рендер линий к дочерним узлам
    if (node.left && node.left.x !== undefined && node.left.y !== undefined) {
      elements.push(
        <line
          key={`line-left-${path}`}
          x1={node.x}
          y1={node.y}
          x2={node.left.x}
          y2={node.left.y}
          stroke="currentColor"
          strokeWidth="2"
          className="text-muted-foreground"
        />
      );
    }
    
    if (node.right && node.right.x !== undefined && node.right.y !== undefined) {
      elements.push(
        <line
          key={`line-right-${path}`}
          x1={node.x}
          y1={node.y}
          x2={node.right.x}
          y2={node.right.y}
          stroke="currentColor"
          strokeWidth="2"
          className="text-muted-foreground"
        />
      );
    }
    
    // Рендер текущего узла
    const isHighlighted = highlightedNodes.includes(node.value);
    const isVisited = visitedNodes.includes(node.value);
    const isCurrent = currentNode === node.value;
    
    let fillColor = '#94a3b8'; // Серый для необработанных
    let strokeColor = '#64748b';
    let strokeWidth = 2;
    let textColor = '#ffffff';
    
    if (isCurrent) {
      fillColor = '#ef4444'; // Ярко-красный для текущего
      strokeColor = '#dc2626';
      strokeWidth = 3;
      textColor = '#ffffff';
    } else if (isHighlighted) {
      fillColor = '#22c55e'; // Ярко-зеленый для только что обработанного
      strokeColor = '#16a34a';
      strokeWidth = 3;
      textColor = '#ffffff';
    } else if (isVisited) {
      fillColor = '#86efac'; // Светло-зеленый для посещенных
      strokeColor = '#4ade80';
      strokeWidth = 2;
      textColor = '#166534';
    }
    
    elements.push(
      <g key={`node-${path}`}>
        {isCurrent && (
          <circle
            cx={node.x}
            cy={node.y}
            r="30"
            fill="none"
            stroke={strokeColor}
            strokeWidth="2"
            opacity="0.5"
            className="animate-ping"
            style={{ animationDuration: '1s' }}
          />
        )}
        <circle
          cx={node.x}
          cy={node.y}
          r="25"
          fill={fillColor}
          stroke={strokeColor}
          strokeWidth={strokeWidth}
          className="transition-all duration-500 ease-in-out"
          style={{
            filter: isCurrent ? 'drop-shadow(0 0 8px rgba(239, 68, 68, 0.6))' : 
                    isHighlighted ? 'drop-shadow(0 0 8px rgba(34, 197, 94, 0.6))' : 'none'
          }}
        />
        <text
          x={node.x}
          y={node.y}
          textAnchor="middle"
          dominantBaseline="middle"
          fill={textColor}
          className="select-none transition-all duration-500"
          style={{ 
            fontWeight: isCurrent || isHighlighted ? 'bold' : 'normal',
            fontSize: isCurrent || isHighlighted ? '16px' : '14px'
          }}
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

  const treeWithPositions = JSON.parse(JSON.stringify(tree));
  calculatePositions(treeWithPositions, 400, 40, 150);

  return (
    <div className="bg-card border rounded-lg p-4 overflow-auto">
      <svg width="800" height="500" className="mx-auto">
        {renderNode(treeWithPositions)}
      </svg>
      <div className="flex justify-center flex-wrap gap-4 mt-4 text-sm">
        <div className="flex items-center space-x-2">
          <div className="w-5 h-5 rounded-full border-2" style={{ backgroundColor: '#94a3b8', borderColor: '#64748b' }} />
          <span>Необработанный</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-5 h-5 rounded-full border-2" style={{ backgroundColor: '#ef4444', borderColor: '#dc2626' }} />
          <span>Текущий</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-5 h-5 rounded-full border-2" style={{ backgroundColor: '#22c55e', borderColor: '#16a34a' }} />
          <span>Обрабатывается</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-5 h-5 rounded-full border-2" style={{ backgroundColor: '#86efac', borderColor: '#4ade80' }} />
          <span>Посещённый</span>
        </div>
      </div>
    </div>
  );
}
