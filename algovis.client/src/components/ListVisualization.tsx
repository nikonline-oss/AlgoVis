import React from 'react';
import { motion, AnimatePresence } from 'framer-motion';

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
  operation?: 'insert' | 'delete' | null;
  operationIndex?: number;
}

export function ListVisualization({
  nodes = [],
  head,
  tail,
  currentIndex,
  highlightedIndices = [],
  comparedIndices = [],
  type = 'singly',
  operation = null,
  operationIndex = -1
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

  const renderForwardArrow = (fromIndex: number, toIndex: number) => {
    const fromX = startX + fromIndex * horizontalSpacing + nodeWidth;
    const fromY = startY + nodeHeight / 4;
    const toX = startX + toIndex * horizontalSpacing;
    const toY = startY + nodeHeight / 4;

    const arrowHeadSize = 8;

    return (
      <motion.g
        key={`arrow-forward-${fromIndex}-${toIndex}`}
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ duration: 0.3 }}
      >
        <line
          x1={fromX}
          y1={fromY}
          x2={toX - 10}
          y2={toY}
          stroke="rgb(34, 197, 94)"
          strokeWidth="2"
        />
        <polygon
          points={`${toX - 10},${toY} ${toX - 10 - arrowHeadSize},${toY - arrowHeadSize / 2} ${toX - 10 - arrowHeadSize},${toY + arrowHeadSize / 2}`}
          fill="rgb(34, 197, 94)"
        />
      </motion.g>
    );
  };

  const renderBackwardArrow = (fromIndex: number, toIndex: number) => {
    const fromX = startX + fromIndex * horizontalSpacing;
    const fromY = startY + nodeHeight * 3/4;
    const toX = startX + toIndex * horizontalSpacing + nodeWidth;
    const toY = startY + nodeHeight * 3/4;

    const arrowHeadSize = 8;

    return (
      <motion.g
        key={`arrow-backward-${fromIndex}-${toIndex}`}
        initial={{ opacity: 0 }}
        animate={{ opacity: 1 }}
        transition={{ duration: 0.3 }}
      >
        <line
          x1={fromX}
          y1={fromY}
          x2={toX + 10}
          y2={toY}
          stroke="rgb(147, 51, 234)"
          strokeWidth="2"
        />
        <polygon
          points={`${toX + 10},${toY} ${toX + 10 + arrowHeadSize},${toY - arrowHeadSize / 2} ${toX + 10 + arrowHeadSize},${toY + arrowHeadSize / 2}`}
          fill="rgb(147, 51, 234)"
        />
      </motion.g>
    );
  };

  const getNodeAnimation = (index: number) => {
    // Анимация для нового узла при вставке
    if (operation === 'insert' && highlightedIndices.includes(index)) {
      return {
        initial: { 
          scale: 0, 
          opacity: 0,
          y: -50 
        },
        animate: { 
          scale: 1, 
          opacity: 1,
          y: 0 
        },
        transition: {
          type: "spring",
          stiffness: 300,
          damping: 20
        }
      };
    }

    // Анимация для удаляемого узла
    if (operation === 'delete' && highlightedIndices.includes(index)) {
      return {
        animate: {
          scale: [1, 1.1, 0],
          opacity: [1, 0.7, 0],
          transition: {
            duration: 0.5,
            ease: "easeOut"
          }
        }
      };
    }

    // Стандартная анимация для существующих узлов
    return {
      initial: { scale: 0, opacity: 0 },
      animate: { scale: 1, opacity: 1 },
      transition: { duration: 0.3, delay: index * 0.05 }
    };
  };

  const getArrowAnimations = () => {
    if (operation === 'insert' || operation === 'delete') {
      return {
        initial: { opacity: 0 },
        animate: { opacity: 1 },
        transition: { duration: 0.5, delay: 0.2 }
      };
    }

    return {
      initial: { opacity: 0 },
      animate: { opacity: 1 },
      transition: { duration: 0.3 }
    };
  };

  return (
    <div className="w-full overflow-x-auto bg-card rounded-lg border p-6">
      <svg width={Math.max(nodes.length * horizontalSpacing + 100, 800)} height="350">
        {/* Draw forward arrows (next pointers) */}
        {nodes.map((node, index) => {
          if (typeof node.next === 'number' && node.next >= 0 && node.next < nodes.length) {
            return renderForwardArrow(index, node.next);
          }
          return null;
        })}
        
        {/* Draw backward arrows (prev pointers) */}
        {type === 'doubly' && nodes.map((node, index) => {
          if (typeof node.prev === 'number' && node.prev >= 0 && node.prev < nodes.length) {
            return renderBackwardArrow(index, node.prev);
          }
          return null;
        })}

        {/* Draw nodes with animations */}
        <AnimatePresence>
          {nodes.map((node, index) => {
            const animationProps = getNodeAnimation(index);
            
            return (
              <motion.g
                key={index}
                layout
                {...animationProps}
                exit={operation === 'delete' && highlightedIndices.includes(index) ? 
                  { scale: 0, opacity: 0, transition: { duration: 0.3 } } : 
                  undefined
                }
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
                  className="fill-white select-none font-medium"
                >
                  {node.value}
                </text>
                
                {/* Index label */}
                <text
                  x={startX + index * horizontalSpacing + nodeWidth / 2}
                  y={startY - 10}
                  textAnchor="middle"
                  className="text-foreground text-sm font-medium select-none"
                  style={{ fill: 'currentColor' }}
                >
                  [{index}]
                </text>

                {/* Head/Tail markers */}
                {head === index && (
                  <motion.text
                    x={startX + index * horizontalSpacing + nodeWidth / 2}
                    y={startY + nodeHeight + 25}
                    textAnchor="middle"
                    className="fill-green-500 text-sm font-medium select-none"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ delay: 0.3 }}
                  >
                    HEAD
                  </motion.text>
                )}
                {tail === index && head !== index && (
                  <motion.text
                    x={startX + index * horizontalSpacing + nodeWidth / 2}
                    y={startY + nodeHeight + 25}
                    textAnchor="middle"
                    className="fill-purple-500 text-sm font-medium select-none"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ delay: 0.3 }}
                  >
                    TAIL
                  </motion.text>
                )}
              </motion.g>
            );
          })}
        </AnimatePresence>

        {/* Operation indicator */}
        {operation && operationIndex >= 0 && (
          <motion.text
            x={startX + (operation === 'insert' ? operationIndex * horizontalSpacing : 
                         operationIndex * horizontalSpacing)}
            y={startY - 40}
            textAnchor="middle"
            className="fill-yellow-500 text-sm font-bold select-none"
            initial={{ opacity: 0, y: -20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.3 }}
          >
            {operation === 'insert' ? 'INSERTING' : 'DELETING'}
          </motion.text>
        )}
      </svg>

      {/* Легенда */}
      <div className="flex justify-center flex-wrap gap-4 mt-4 text-sm">
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: 'rgb(156, 163, 175)', borderColor: 'rgb(100, 116, 139)' }} />
          <span className="text-xs text-muted-foreground">Обычный узел</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: 'rgb(59, 130, 246)', borderColor: 'rgb(37, 99, 235)' }} />
          <span className="text-xs text-muted-foreground">Сравниваемый</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: 'rgb(34, 197, 94)', borderColor: 'rgb(21, 128, 61)' }} />
          <span className="text-xs text-muted-foreground">Выделенный</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 rounded-full border-2" style={{ backgroundColor: 'rgb(239, 68, 68)', borderColor: 'rgb(220, 38, 38)' }} />
          <span className="text-xs text-muted-foreground">Текущий</span>
        </div>
       
      </div>
    </div>
  );
}