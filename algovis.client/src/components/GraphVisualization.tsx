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
}

export function GraphVisualization({ 
  nodes, 
  edges, 
  highlightedNodes = [], 
  highlightedEdges = [],
  visitedNodes = [],
  currentNode,
  directed = false
}: GraphVisualizationProps) {
  const svgRef = useRef<SVGSVGElement>(null);

  if (!nodes || !edges || nodes.length === 0) {
    return (
      <div className="flex items-center justify-center min-h-[400px] bg-card border rounded-lg">
        <p className="text-muted-foreground">Граф пуст</p>
      </div>
    );
  }

  const isEdgeHighlighted = (from: number, to: number): boolean => {
    return highlightedEdges.some(([f, t]) => 
      (f === from && t === to) || (!directed && f === to && t === from)
    );
  };

  const renderEdge = (edge: GraphEdge, index: number) => {
    const fromNode = nodes.find(n => n.id === edge.from);
    const toNode = nodes.find(n => n.id === edge.to);
    
    if (!fromNode || !toNode) return null;

    const isHighlighted = isEdgeHighlighted(edge.from, edge.to);
    const strokeColor = isHighlighted ? 'hsl(var(--chart-2))' : 'hsl(var(--muted-foreground))';
    const strokeWidth = isHighlighted ? 3 : 2;

    // Вычисляем середину для отображения веса
    const midX = (fromNode.x + toNode.x) / 2;
    const midY = (fromNode.y + toNode.y) / 2;

    if (directed) {
      // Вычисляем угол для стрелки
      const angle = Math.atan2(toNode.y - fromNode.y, toNode.x - fromNode.x);
      const arrowLength = 10;
      const nodeRadius = 25;
      
      // Конечная точка линии (у границы круга)
      const endX = toNode.x - nodeRadius * Math.cos(angle);
      const endY = toNode.y - nodeRadius * Math.sin(angle);
      
      // Точки стрелки
      const arrowX1 = endX - arrowLength * Math.cos(angle - Math.PI / 6);
      const arrowY1 = endY - arrowLength * Math.sin(angle - Math.PI / 6);
      const arrowX2 = endX - arrowLength * Math.cos(angle + Math.PI / 6);
      const arrowY2 = endY - arrowLength * Math.sin(angle + Math.PI / 6);

      return (
        <g key={`edge-${index}`}>
          <line
            x1={fromNode.x}
            y1={fromNode.y}
            x2={endX}
            y2={endY}
            stroke={strokeColor}
            strokeWidth={strokeWidth}
            className="transition-all duration-300"
          />
          <polygon
            points={`${endX},${endY} ${arrowX1},${arrowY1} ${arrowX2},${arrowY2}`}
            fill={strokeColor}
            className="transition-all duration-300"
          />
          {edge.weight !== undefined && (
            <text
              x={midX}
              y={midY - 10}
              textAnchor="middle"
              className="fill-foreground text-sm"
            >
              {edge.weight}
            </text>
          )}
        </g>
      );
    } else {
      return (
        <g key={`edge-${index}`}>
          <line
            x1={fromNode.x}
            y1={fromNode.y}
            x2={toNode.x}
            y2={toNode.y}
            stroke={strokeColor}
            strokeWidth={strokeWidth}
            className="transition-all duration-300"
          />
          {edge.weight !== undefined && (
            <text
              x={midX}
              y={midY - 10}
              textAnchor="middle"
              className="fill-foreground text-sm"
            >
              {edge.weight}
            </text>
          )}
        </g>
      );
    }
  };

  const renderNode = (node: GraphNode) => {
    const isHighlighted = highlightedNodes.includes(node.id);
    const isVisited = visitedNodes.includes(node.id);
    const isCurrent = currentNode === node.id;
    
    let fillColor = 'hsl(var(--muted))';
    if (isCurrent) fillColor = 'hsl(var(--destructive))';
    else if (isHighlighted) fillColor = 'hsl(var(--chart-2))';
    else if (isVisited) fillColor = 'hsl(var(--primary))';

    return (
      <g key={`node-${node.id}`}>
        <circle
          cx={node.x}
          cy={node.y}
          r="25"
          fill={fillColor}
          stroke="currentColor"
          strokeWidth="2"
          className="transition-all duration-300"
        />
        <text
          x={node.x}
          y={node.y}
          textAnchor="middle"
          dominantBaseline="middle"
          className="fill-primary-foreground select-none"
        >
          {node.label || node.id}
        </text>
      </g>
    );
  };

  return (
    <div className="bg-card border rounded-lg p-4">
      <svg ref={svgRef} width="100%" height="500" className="mx-auto">
        <g>
          {edges.map((edge, index) => renderEdge(edge, index))}
          {nodes.map(node => renderNode(node))}
        </g>
      </svg>
      <div className="flex justify-center space-x-4 mt-4 text-sm">
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full" style={{ backgroundColor: 'hsl(var(--muted))' }} />
          <span>Обычный</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full" style={{ backgroundColor: 'hsl(var(--primary))' }} />
          <span>Посещённый</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full" style={{ backgroundColor: 'hsl(var(--chart-2))' }} />
          <span>Выделенный</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full" style={{ backgroundColor: 'hsl(var(--destructive))' }} />
          <span>Текущий</span>
        </div>
      </div>
    </div>
  );
}
