import React from 'react';
import { motion } from 'framer-motion';

interface QueueVisualizationProps {
  items: number[];
  front?: number;
  rear?: number;
  highlightedIndex?: number;
  operation?: 'enqueue' | 'dequeue' | null;
}

export function QueueVisualization({
  items = [],
  front,
  rear,
  highlightedIndex,
  operation
}: QueueVisualizationProps) {
  const itemWidth = 80;
  const itemHeight = 80;
  const spacing = 10;
  const startX = 50;
  const startY = 200;

  const getItemColor = (index: number) => {
    if (highlightedIndex === index) {
      if (operation === 'enqueue') return 'rgb(34, 197, 94)';
      if (operation === 'dequeue') return 'rgb(239, 68, 68)';
    }
    if (front === index) return 'rgb(59, 130, 246)';
    if (rear === index) return 'rgb(147, 51, 234)';
    return 'rgb(156, 163, 175)';
  };

  return (
    <div className="w-full overflow-x-auto bg-card rounded-lg border p-6">
      <svg width={Math.max(items.length * (itemWidth + spacing) + 100, 800)} height="400">
        {/* Queue container */}
        {items.length > 0 && (
          <rect
            x={startX - 10}
            y={startY - 10}
            width={items.length * (itemWidth + spacing) + 10}
            height={itemHeight + 20}
            fill="none"
            stroke="rgb(100, 116, 139)"
            strokeWidth="2"
            strokeDasharray="5,5"
            rx="8"
          />
        )}

        {/* Queue items */}
        {items.map((item, index) => {
          const xPos = startX + index * (itemWidth + spacing);
          
          return (
            <motion.g
              key={`${item}-${index}`}
              initial={{ 
                x: operation === 'enqueue' && highlightedIndex === index ? 100 : 0, 
                opacity: 0 
              }}
              animate={{ x: 0, opacity: 1 }}
              exit={{ 
                x: operation === 'dequeue' && highlightedIndex === index ? -100 : 0, 
                opacity: 0 
              }}
              transition={{ duration: 0.5 }}
            >
              <motion.rect
                x={xPos}
                y={startY}
                width={itemWidth}
                height={itemHeight}
                fill={getItemColor(index)}
                stroke="rgb(100, 116, 139)"
                strokeWidth="2"
                rx="8"
                animate={{
                  scale: (front === index || rear === index) ? [1, 1.05, 1] : 1,
                }}
                transition={{
                  duration: 0.8,
                  repeat: (front === index || rear === index) ? Infinity : 0,
                }}
              />
              <text
                x={xPos + itemWidth / 2}
                y={startY + itemHeight / 2 + 6}
                textAnchor="middle"
                className="fill-white select-none"
              >
                {item}
              </text>
              
              {/* Index label */}
              <text
                x={xPos + itemWidth / 2}
                y={startY - 15}
                textAnchor="middle"
                className="fill-muted-foreground text-sm select-none"
              >
                [{index}]
              </text>
              
              {/* FRONT marker */}
              {front === index && (
                <g>
                  <text
                    x={xPos + itemWidth / 2}
                    y={startY + itemHeight + 25}
                    textAnchor="middle"
                    className="fill-blue-500 select-none"
                  >
                    FRONT
                  </text>
                  <motion.polygon
                    points={`${xPos + itemWidth / 2},${startY + itemHeight + 10} ${xPos + itemWidth / 2 - 6},${startY + itemHeight + 18} ${xPos + itemWidth / 2 + 6},${startY + itemHeight + 18}`}
                    fill="rgb(59, 130, 246)"
                    animate={{ y: [0, 5, 0] }}
                    transition={{ duration: 1, repeat: Infinity }}
                  />
                </g>
              )}
              
              {/* REAR marker */}
              {rear === index && (
                <g>
                  <text
                    x={xPos + itemWidth / 2}
                    y={startY + itemHeight + 45}
                    textAnchor="middle"
                    className="fill-purple-500 select-none"
                  >
                    REAR
                  </text>
                  <motion.polygon
                    points={`${xPos + itemWidth / 2},${startY + itemHeight + 30} ${xPos + itemWidth / 2 - 6},${startY + itemHeight + 38} ${xPos + itemWidth / 2 + 6},${startY + itemHeight + 38}`}
                    fill="rgb(147, 51, 234)"
                    animate={{ y: [0, 5, 0] }}
                    transition={{ duration: 1, repeat: Infinity }}
                  />
                </g>
              )}
            </motion.g>
          );
        })}

        {/* Empty queue message */}
        {items.length === 0 && (
          <text
            x={400}
            y={startY + itemHeight / 2}
            textAnchor="middle"
            className="fill-muted-foreground select-none"
          >
            Empty Queue
          </text>
        )}

        {/* Direction arrow */}
        {items.length > 0 && (
          <g>
            <text
              x={startX}
              y={startY - 35}
              className="fill-muted-foreground text-sm select-none"
            >
              Dequeue ←
            </text>
            <text
              x={startX + items.length * (itemWidth + spacing) - 50}
              y={startY - 35}
              className="fill-muted-foreground text-sm select-none"
            >
              → Enqueue
            </text>
          </g>
        )}
      </svg>
    </div>
  );
}
