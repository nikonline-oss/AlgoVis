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
  height = 500
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
    const strokeColor = isHighlighted ? '#ef4444' : '#6b7280'; // red-500 vs gray-500
    const strokeWidth = isHighlighted ? 3 : 2;

    // Вычисляем середину для отображения веса
    const midX = (fromNode.x + toNode.x) / 2;
    const midY = (fromNode.y + toNode.y) / 2;

    // Вычисляем угол для направления
    const angle = Math.atan2(toNode.y - fromNode.y, toNode.x - fromNode.x);
    const nodeRadius = 25;
    
    // Начальная и конечная точки с учетом радиуса узлов
    const startX = fromNode.x + nodeRadius * Math.cos(angle);
    const startY = fromNode.y + nodeRadius * Math.sin(angle);
    const endX = toNode.x - nodeRadius * Math.cos(angle);
    const endY = toNode.y - nodeRadius * Math.sin(angle);

    if (directed) {
      // Длина стрелки
      const arrowLength = 12;
      
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
            <text
              x={midX}
              y={midY - 8}
              textAnchor="middle"
              dominantBaseline="middle"
              className="fill-foreground text-sm font-medium"
              style={{ fontSize: '12px', userSelect: 'none' }}
            >
              {edge.weight}
            </text>
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
            <text
              x={midX}
              y={midY - 8}
              textAnchor="middle"
              dominantBaseline="middle"
              className="fill-foreground text-sm font-medium"
              style={{ fontSize: '12px', userSelect: 'none' }}
            >
              {edge.weight}
            </text>
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
    
    let fillColor = '#d1d5db'; // gray-300
    let textColor = '#1f2937'; // gray-800
    
    if (isCurrent) {
      fillColor = '#ef4444'; // red-500
      textColor = '#ffffff';
    } else if (isHighlighted) {
      fillColor = '#f59e0b'; // amber-500
      textColor = '#ffffff';
    } else if (isVisited) {
      fillColor = '#3b82f6'; // blue-500
      textColor = '#ffffff';
    }

    return (
      <g key={`node-${node.id}`} className="cursor-pointer">
        <circle
          cx={node.x}
          cy={node.y}
          r="25"
          fill={fillColor}
          stroke="#374151" // gray-700
          strokeWidth="2"
          className="transition-all duration-300 shadow-md"
        />
        <text
          x={node.x}
          y={node.y}
          textAnchor="middle"
          dominantBaseline="middle"
          className="font-medium select-none"
          style={{ fill: textColor, fontSize: '14px', userSelect: 'none' }}
        >
          {node.label || node.id}
        </text>
      </g>
    );
  };

  return (
    <div className="bg-white border border-gray-200 rounded-lg p-6 shadow-sm">
      <svg 
        ref={svgRef} 
        width={width} 
        height={height} 
        className="mx-auto block"
        viewBox={`0 0 ${width} ${height}`}
      >
        {/* Фон */}
        <rect width="100%" height="100%" fill="#f9fafb" />
        
        <g>
          {/* Сначала рендерим ребра, чтобы они были под узлами */}
          {edges.map((edge, index) => renderEdge(edge, index))}
          {/* Затем рендерим узлы поверх ребер */}
          {nodes.map(node => renderNode(node))}
        </g>
      </svg>
      
      {/* Легенда */}
      <div className="flex justify-center flex-wrap gap-6 mt-4 text-sm">
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full bg-gray-300 border border-gray-400" />
          <span className="text-gray-700">Обычный</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full bg-blue-500 border border-gray-400" />
          <span className="text-gray-700">Посещённый</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full bg-amber-500 border border-gray-400" />
          <span className="text-gray-700">Выделенный</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full bg-red-500 border border-gray-400" />
          <span className="text-gray-700">Текущий</span>
        </div>
      </div>
    </div>
  );
}