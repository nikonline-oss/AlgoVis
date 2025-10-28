import React from 'react';
import { motion } from 'framer-motion';

interface StackVisualizationProps {
  items: number[];
  top?: number;
  highlightedIndex?: number;
  operation?: 'push' | 'pop' | null;
}

export function StackVisualization({
  items = [],
  top,
  highlightedIndex,
  operation
}: StackVisualizationProps) {
  const itemWidth = 120;
  const itemHeight = 50;
  const startX = 250;
  const startY = 400;

  const getItemColor = (index: number) => {
    if (highlightedIndex === index) {
      if (operation === 'push') return 'rgb(34, 197, 94)';
      if (operation === 'pop') return 'rgb(239, 68, 68)';
    }
    if (top === index) return 'rgb(59, 130, 246)';
    return 'rgb(156, 163, 175)';
  };

  return (
    <div className="w-full h-[500px] flex items-center justify-center bg-card rounded-lg border p-6">
      <svg width="600" height="500">
        {/* Stack base */}
        <rect
          x={startX - 10}
          y={startY - items.length * itemHeight - 20}
          width={itemWidth + 20}
          height={items.length * itemHeight + 50}
          fill="none"
          stroke="rgb(100, 116, 139)"
          strokeWidth="2"
          strokeDasharray="5,5"
          rx="8"
        />

        {/* Base line */}
        <line
          x1={startX - 20}
          y1={startY + 30}
          x2={startX + itemWidth + 20}
          y2={startY + 30}
          stroke="rgb(100, 116, 139)"
          strokeWidth="3"
        />

        {/* Stack items */}
        {items.map((item, index) => {
          const yPos = startY - (index + 1) * itemHeight;
          
          return (
            <motion.g
              key={`${item}-${index}`}
              initial={{ y: operation === 'push' && highlightedIndex === index ? -50 : 0, opacity: 0 }}
              animate={{ y: 0, opacity: 1 }}
              exit={{ y: operation === 'pop' && highlightedIndex === index ? -50 : 0, opacity: 0 }}
              transition={{ duration: 0.5 }}
            >
              <motion.rect
                x={startX}
                y={yPos}
                width={itemWidth}
                height={itemHeight - 5}
                fill={getItemColor(index)}
                stroke="rgb(100, 116, 139)"
                strokeWidth="2"
                rx="6"
                animate={{
                  scale: top === index ? [1, 1.05, 1] : 1,
                }}
                transition={{
                  duration: 0.8,
                  repeat: top === index ? Infinity : 0,
                }}
              />
              <text
                x={startX + itemWidth / 2}
                y={yPos + itemHeight / 2 + 6}
                textAnchor="middle"
                className="fill-white select-none"
              >
                {item}
              </text>
              
              {/* Index label */}
              <text
                x={startX - 25}
                y={yPos + itemHeight / 2 + 6}
                textAnchor="middle"
                className="fill-muted-foreground text-sm select-none"
              >
                {index}
              </text>
            </motion.g>
          );
        })}

        {/* TOP pointer */}
        {items.length > 0 && typeof top === 'number' && top >= 0 && (
          <g>
            <text
              x={startX + itemWidth + 35}
              y={startY - (top + 1) * itemHeight + itemHeight / 2 + 6}
              className="fill-blue-500 select-none"
            >
              ← TOP
            </text>
          </g>
        )}

        {/* Empty stack message */}
        {items.length === 0 && (
          <text
            x={startX + itemWidth / 2}
            y={startY - 20}
            textAnchor="middle"
            className="fill-muted-foreground select-none"
          >
            Empty Stack
          </text>
        )}
      </svg>
    </div>
  );
}
