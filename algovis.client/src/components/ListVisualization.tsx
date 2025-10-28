import React from 'react';
import { motion } from 'framer-motion';

export interface ListNode {
  value: number;
  next?: number | null;
  prev?: number | null;
}

interface ListVisualizationProps {
  nodes: ListNode[];
  head?: number | null;
  tail?: number | null;
  currentIndex?: number;
  highlightedIndices?: number[];
  comparedIndices?: number[];
  type?: 'singly' | 'doubly';
}

export function ListVisualization({
  nodes = [],
  head,
  tail,
  currentIndex,
  highlightedIndices = [],
  comparedIndices = [],
  type = 'singly'
}: ListVisualizationProps) {
  const nodeWidth = 80;
  const nodeHeight = 60;
  const horizontalSpacing = 140;
  const startX = 50;
  const startY = 150;

  const getNodeColor = (index: number) => {
    if (currentIndex === index) return 'rgb(239, 68, 68)';
    if (highlightedIndices.includes(index)) return 'rgb(34, 197, 94)';
    if (comparedIndices.includes(index)) return 'rgb(59, 130, 246)';
    return 'rgb(156, 163, 175)';
  };

  const renderArrow = (fromIndex: number, toIndex: number, isReverse: boolean = false) => {
    const fromX = startX + fromIndex * horizontalSpacing + nodeWidth;
    const fromY = startY + (isReverse ? nodeHeight - 15 : 15);
    const toX = startX + toIndex * horizontalSpacing;
    const toY = startY + (isReverse ? nodeHeight - 15 : 15);

    const arrowLength = toX - fromX - 10;
    const arrowHeadSize = 8;

    return (
      <g key={`arrow-${fromIndex}-${toIndex}-${isReverse ? 'back' : 'forward'}`}>
        <motion.line
          x1={fromX}
          y1={fromY}
          x2={toX - 10}
          y2={toY}
          stroke={isReverse ? 'rgb(147, 51, 234)' : 'rgb(34, 197, 94)'}
          strokeWidth="2"
          initial={{ pathLength: 0, opacity: 0 }}
          animate={{ pathLength: 1, opacity: 1 }}
          transition={{ duration: 0.5 }}
        />
        <motion.polygon
          points={`${toX - 10},${toY} ${toX - 10 - arrowHeadSize},${toY - arrowHeadSize / 2} ${toX - 10 - arrowHeadSize},${toY + arrowHeadSize / 2}`}
          fill={isReverse ? 'rgb(147, 51, 234)' : 'rgb(34, 197, 94)'}
          initial={{ opacity: 0 }}
          animate={{ opacity: 1 }}
          transition={{ duration: 0.3, delay: 0.3 }}
        />
      </g>
    );
  };

  return (
    <div className="w-full overflow-x-auto bg-card rounded-lg border p-6">
      <svg width={Math.max(nodes.length * horizontalSpacing + 100, 800)} height="350">
        {/* Draw arrows */}
        {nodes.map((node, index) => {
          if (typeof node.next === 'number' && node.next >= 0 && node.next < nodes.length) {
            return renderArrow(index, node.next, false);
          }
          return null;
        })}
        
        {type === 'doubly' && nodes.map((node, index) => {
          if (typeof node.prev === 'number' && node.prev >= 0 && node.prev < nodes.length) {
            return renderArrow(index, node.prev, true);
          }
          return null;
        })}

        {/* Draw nodes */}
        {nodes.map((node, index) => (
          <motion.g
            key={index}
            initial={{ scale: 0, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ duration: 0.3, delay: index * 0.05 }}
          >
            <motion.rect
              x={startX + index * horizontalSpacing}
              y={startY}
              width={nodeWidth}
              height={nodeHeight}
              fill={getNodeColor(index)}
              stroke={currentIndex === index ? 'rgb(239, 68, 68)' : 'rgb(100, 116, 139)'}
              strokeWidth={currentIndex === index ? 3 : 2}
              rx="8"
              animate={{
                scale: currentIndex === index ? [1, 1.1, 1] : 1,
              }}
              transition={{
                duration: 0.6,
                repeat: currentIndex === index ? Infinity : 0,
              }}
            />
            <text
              x={startX + index * horizontalSpacing + nodeWidth / 2}
              y={startY + nodeHeight / 2 + 6}
              textAnchor="middle"
              className="fill-white select-none"
            >
              {node.value}
            </text>
            
            {/* Index label */}
            <text
              x={startX + index * horizontalSpacing + nodeWidth / 2}
              y={startY - 10}
              textAnchor="middle"
              className="fill-muted-foreground text-xs select-none"
            >
              [{index}]
            </text>

            {/* Head/Tail markers */}
            {head === index && (
              <text
                x={startX + index * horizontalSpacing + nodeWidth / 2}
                y={startY + nodeHeight + 20}
                textAnchor="middle"
                className="fill-green-500 text-sm select-none"
              >
                HEAD
              </text>
            )}
            {tail === index && (
              <text
                x={startX + index * horizontalSpacing + nodeWidth / 2}
                y={startY + nodeHeight + 35}
                textAnchor="middle"
                className="fill-purple-500 text-sm select-none"
              >
                TAIL
              </text>
            )}
          </motion.g>
        ))}

        {/* NULL pointer at the end */}
        {nodes.length > 0 && nodes[nodes.length - 1].next === null && (
          <g>
            <text
              x={startX + nodes.length * horizontalSpacing}
              y={startY + 35}
              className="fill-muted-foreground select-none"
            >
              NULL
            </text>
          </g>
        )}
      </svg>
    </div>
  );
}
