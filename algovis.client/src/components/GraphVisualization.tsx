import React, { useEffect, useRef } from 'react';

export interface GraphNode {
  id: number;
  x: number;
  y: number;
  label?: string;
}

export interface GraphEdge {
  from: number;
  to: number;
  weight?: number;
}

interface GraphVisualizationProps {
  nodes: GraphNode[];
  edges: GraphEdge[];
  highlightedNodes?: number[];
  highlightedEdges?: Array<[number, number]>;
  visitedNodes?: number[];
  currentNode?: number;
  directed?: boolean;
  width?: number;
  height?: number;
  graphType?: 'circular' | 'grid' | 'complete' | 'random';
}

export function GraphVisualization({
  nodes,
  edges,
  highlightedNodes = [],
  highlightedEdges = [],
  visitedNodes = [],
  currentNode,
  directed = false,
  width = 800,
  height = 500,
  graphType = 'circular'
}: GraphVisualizationProps) {
  const svgRef = useRef<SVGSVGElement>(null);

  if (!nodes || nodes.length === 0) {
    return (
      <div className="flex items-center justify-center min-h-[400px] bg-card border rounded-lg">
        <p className="text-muted-foreground">Граф пуст</p>
      </div>
    );
  }

  // Функция для проверки выделенности ребра
  const isEdgeHighlighted = (from: number, to: number): boolean => {
    return highlightedEdges.some(([f, t]) =>
      (f === from && t === to) || (!directed && f === to && t === from)
    );
  };

  // Функция для рендеринга ребра
  const renderEdge = (edge: GraphEdge, index: number) => {
    const fromNode = nodes.find(n => n.id === edge.from);
    const toNode = nodes.find(n => n.id === edge.to);

    if (!fromNode || !toNode) return null;

    const isHighlighted = isEdgeHighlighted(edge.from, edge.to);
    const isVisited = visitedNodes.includes(edge.from) && visitedNodes.includes(edge.to);

    let strokeColor = '#6b7280'; // gray-500
    if (isHighlighted) strokeColor = '#ef4444'; // red-500
    else if (isVisited) strokeColor = '#3b82f6'; // blue-500

    const strokeWidth = isHighlighted ? 3 : 2;

    // Вычисляем середину для отображения веса
    const midX = (fromNode.x + toNode.x) / 2;
    const midY = (fromNode.y + toNode.y) / 2;

    // Вычисляем угол для направления
    const angle = Math.atan2(toNode.y - fromNode.y, toNode.x - fromNode.x);
    const nodeRadius = 20;

    // Начальная и конечная точки с учетом радиуса узлов
    const startX = fromNode.x + nodeRadius * Math.cos(angle);
    const startY = fromNode.y + nodeRadius * Math.sin(angle);
    const endX = toNode.x - nodeRadius * Math.cos(angle);
    const endY = toNode.y - nodeRadius * Math.sin(angle);

    if (directed) {
      // Длина стрелки
      const arrowLength = 10;
      const arrowWidth = 6;

      // Точки стрелки
      const arrowX1 = endX - arrowLength * Math.cos(angle - Math.PI / 6);
      const arrowY1 = endY - arrowLength * Math.sin(angle - Math.PI / 6);
      const arrowX2 = endX - arrowLength * Math.cos(angle + Math.PI / 6);
      const arrowY2 = endY - arrowLength * Math.sin(angle + Math.PI / 6);

      return (
        <g key={`edge-${edge.from}-${edge.to}-${index}`}>
          {/* Основная линия */}
          <line
            x1={startX}
            y1={startY}
            x2={endX}
            y2={endY}
            stroke={strokeColor}
            strokeWidth={strokeWidth}
            className="transition-all duration-300"
          />
          {/* Стрелка */}
          <polygon
            points={`${endX},${endY} ${arrowX1},${arrowY1} ${arrowX2},${arrowY2}`}
            fill={strokeColor}
            className="transition-all duration-300"
          />
          {/* Вес ребра */}
          {edge.weight !== undefined && (
            <g>
              <rect
                x={midX - 12}
                y={midY - 10}
                width={24}
                height={16}
                fill="white"
                stroke="#374151"
                strokeWidth={1}
                rx={3}
              />
              <text
                x={midX}
                y={midY}
                textAnchor="middle"
                dominantBaseline="middle"
                className="text-xs font-bold fill-gray-800"
                style={{ userSelect: 'none' }}
              >
                {edge.weight}
              </text>
            </g>
          )}
        </g>
      );
    } else {
      return (
        <g key={`edge-${edge.from}-${edge.to}-${index}`}>
          <line
            x1={startX}
            y1={startY}
            x2={endX}
            y2={endY}
            stroke={strokeColor}
            strokeWidth={strokeWidth}
            className="transition-all duration-300"
          />
          {edge.weight !== undefined && (
            <g>
              <rect
                x={midX - 12}
                y={midY - 10}
                width={24}
                height={16}
                fill="white"
                stroke="#374151"
                strokeWidth={1}
                rx={3}
              />
              <text
                x={midX}
                y={midY}
                textAnchor="middle"
                dominantBaseline="middle"
                className="text-xs font-bold fill-gray-800"
                style={{ userSelect: 'none' }}
              >
                {edge.weight}
              </text>
            </g>
          )}
        </g>
      );
    }
  };

  // Функция для рендеринга узла
  const renderNode = (node: GraphNode) => {
    const isHighlighted = highlightedNodes.includes(node.id);
    const isVisited = visitedNodes.includes(node.id);
    const isCurrent = currentNode === node.id;

    let fillColor = '#ffffff'; // БЕЛЫЙ - НЕ ПОСЕЩЁН
    let textColor = '#1f2937'; // gray-800
    let strokeColor = '#374151'; // gray-700
    let strokeWidth = 2;

    if (isCurrent) {
      fillColor = '#ef4444'; // red-500 - ТЕКУЩИЙ
      textColor = '#ffffff';
      strokeColor = '#dc2626'; // red-600
      strokeWidth = 3;
    } else if (isHighlighted) {
      fillColor = '#f59e0b'; // amber-500 - ВЫДЕЛЕННЫЙ
      textColor = '#ffffff';
      strokeColor = '#d97706'; // amber-600
    } else if (isVisited) {
      fillColor = '#3b82f6'; // blue-500 - ПОСЕЩЁННЫЙ
      textColor = '#ffffff';
      strokeColor = '#2563eb'; // blue-600
    }

    return (
      <g key={`node-${node.id}`} className="cursor-pointer">
        <circle
          cx={node.x}
          cy={node.y}
          r="20"
          fill={fillColor}
          stroke={strokeColor}
          strokeWidth={strokeWidth}
          className="transition-all duration-300 shadow-sm"
        />
        <text
          x={node.x}
          y={node.y}
          textAnchor="middle"
          dominantBaseline="middle"
          className="font-bold select-none"
          style={{
            fill: textColor,
            fontSize: '12px',
            userSelect: 'none',
            pointerEvents: 'none'
          }}
        >
          {node.label || node.id}
        </text>
      </g>
    );
  };

  const getGraphTypeName = (type: string) => {
    switch (type) {
      case 'circular': return 'Круговой (замкнутый)';
      case 'grid': return 'Сетка';
      case 'complete': return 'Полный граф';
      case 'random': return 'Случайный граф';
      default: return type;
    }
  };

  return (
    <div className="bg-card border rounded-lg p-4">
      <div className="mb-2 text-sm text-muted-foreground text-center">
        {getGraphTypeName(graphType)} |
        {directed ? ' Ориентированный' : ' Неориентированный'}
      </div>

      <svg
        ref={svgRef}
        width={width}
        height={height}
        className="mx-auto block"
        viewBox={`0 0 ${width} ${height}`}
      >
        <g>
          {/* Сначала рендерим ребра, чтобы они были под узлами */}
          {edges.map((edge, index) => renderEdge(edge, index))}
          {/* Затем рендерим узлы поверх ребер */}
          {nodes.map(node => renderNode(node))}
        </g>
      </svg>

      <div className="flex justify-center flex-wrap gap-4 mt-4 text-sm">
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: '#ffffff', borderColor: '#374151' }} />
          <span className="text-xs text-muted-foreground">Не посещён</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: '#3b82f6', borderColor: '#2563eb' }} />
          <span className="text-xs text-muted-foreground">Посещённый</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: '#f59e0b', borderColor: '#d97706' }} />
          <span className="text-xs text-muted-foreground">Выделенный</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: '#ef4444', borderColor: '#dc2626' }} />
          <span className="text-xs text-muted-foreground">Текущий</span>
        </div>
      </div>




    </div>
  );
}