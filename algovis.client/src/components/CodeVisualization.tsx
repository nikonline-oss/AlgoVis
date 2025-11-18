import React from 'react';
import { motion } from 'motion/react';

interface CodeVisualizationProps {
  analysisData: {
    complexity: string;
    warnings: number;
    suggestions: number;
    functionGraph?: {
      functions: Array<{ name: string; complexity: string }>;
      calls: Array<{ from: string; to: string }>;
    };
    metrics?: {
      lines: number;
      comments: number;
      functions: number;
      classes: number;
    };
  };
}

export function CodeVisualization({ analysisData }: CodeVisualizationProps) {
  const { functionGraph, metrics } = analysisData;

  // Если есть граф функций, отображаем его
  if (functionGraph && functionGraph.functions.length > 0) {
    const nodeRadius = 40;
    const centerX = 400;
    const centerY = 250;
    const radius = 150;

    return (
      <div className="w-full h-[500px] bg-card rounded-lg border p-6">
        <div className="space-y-4">
          <h3 className="text-sm">Граф вызовов функций</h3>
          <svg width="800" height="450">
            {/* Рисуем связи */}
            {functionGraph.calls.map((call, index) => {
              const fromIndex = functionGraph.functions.findIndex(f => f.name === call.from);
              const toIndex = functionGraph.functions.findIndex(f => f.name === call.to);
              
              if (fromIndex === -1 || toIndex === -1) return null;

              const fromAngle = (fromIndex * 2 * Math.PI) / functionGraph.functions.length;
              const toAngle = (toIndex * 2 * Math.PI) / functionGraph.functions.length;
              
              const x1 = centerX + radius * Math.cos(fromAngle);
              const y1 = centerY + radius * Math.sin(fromAngle);
              const x2 = centerX + radius * Math.cos(toAngle);
              const y2 = centerY + radius * Math.sin(toAngle);

              return (
                <motion.line
                  key={`${call.from}-${call.to}-${index}`}
                  x1={x1}
                  y1={y1}
                  x2={x2}
                  y2={y2}
                  stroke="rgb(34, 197, 94)"
                  strokeWidth="2"
                  markerEnd="url(#arrowhead)"
                  initial={{ pathLength: 0, opacity: 0 }}
                  animate={{ pathLength: 1, opacity: 0.6 }}
                  transition={{ duration: 0.5, delay: index * 0.1 }}
                />
              );
            })}

            {/* Определяем стрелку */}
            <defs>
              <marker
                id="arrowhead"
                markerWidth="10"
                markerHeight="10"
                refX="9"
                refY="3"
                orient="auto"
              >
                <polygon points="0 0, 10 3, 0 6" fill="rgb(34, 197, 94)" />
              </marker>
            </defs>

            {/* Рисуем узлы функций */}
            {functionGraph.functions.map((func, index) => {
              const angle = (index * 2 * Math.PI) / functionGraph.functions.length;
              const x = centerX + radius * Math.cos(angle);
              const y = centerY + radius * Math.sin(angle);

              const getComplexityColor = (complexity: string) => {
                if (complexity.includes('n²') || complexity.includes('n^2')) return 'rgb(239, 68, 68)';
                if (complexity.includes('n log n')) return 'rgb(245, 158, 11)';
                if (complexity.includes('n')) return 'rgb(34, 197, 94)';
                return 'rgb(59, 130, 246)';
              };

              return (
                <motion.g
                  key={func.name}
                  initial={{ scale: 0, opacity: 0 }}
                  animate={{ scale: 1, opacity: 1 }}
                  transition={{ duration: 0.3, delay: index * 0.1 }}
                >
                  <circle
                    cx={x}
                    cy={y}
                    r={nodeRadius}
                    fill={getComplexityColor(func.complexity)}
                    stroke="rgb(100, 116, 139)"
                    strokeWidth="2"
                  />
                  <text
                    x={x}
                    y={y - 5}
                    textAnchor="middle"
                    className="fill-white text-sm select-none"
                  >
                    {func.name}
                  </text>
                  <text
                    x={x}
                    y={y + 10}
                    textAnchor="middle"
                    className="fill-white text-xs select-none"
                  >
                    {func.complexity}
                  </text>
                </motion.g>
              );
            })}
          </svg>
        </div>
      </div>
    );
  }

  // Если нет графа, показываем метрики в виде диаграммы
  if (metrics) {
    const maxValue = Math.max(metrics.lines, metrics.comments, metrics.functions * 10, metrics.classes * 20);
    const barHeight = 40;
    const barSpacing = 20;
    const startY = 50;
    const maxBarWidth = 500;

    const data = [
      { label: 'Строки кода', value: metrics.lines, color: 'rgb(59, 130, 246)' },
      { label: 'Комментарии', value: metrics.comments, color: 'rgb(34, 197, 94)' },
      { label: 'Функции', value: metrics.functions * 10, color: 'rgb(245, 158, 11)', displayValue: metrics.functions },
      { label: 'Классы', value: metrics.classes * 20, color: 'rgb(147, 51, 234)', displayValue: metrics.classes },
    ];

    return (
      <div className="w-full h-[400px] bg-card rounded-lg border p-6">
        <div className="space-y-4">
          <h3 className="text-sm">Метрики кода</h3>
          <svg width="650" height="350">
            {data.map((item, index) => {
              const y = startY + index * (barHeight + barSpacing);
              const width = (item.value / maxValue) * maxBarWidth;

              return (
                <g key={item.label}>
                  <text
                    x="10"
                    y={y + barHeight / 2 + 5}
                    className="fill-foreground text-sm select-none"
                  >
                    {item.label}:
                  </text>
                  <motion.rect
                    x="150"
                    y={y}
                    height={barHeight}
                    fill={item.color}
                    rx="4"
                    initial={{ width: 0 }}
                    animate={{ width }}
                    transition={{ duration: 0.8, delay: index * 0.1 }}
                  />
                  <motion.text
                    x={150 + width + 10}
                    y={y + barHeight / 2 + 5}
                    className="fill-foreground select-none"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ duration: 0.3, delay: index * 0.1 + 0.5 }}
                  >
                    {item.displayValue !== undefined ? item.displayValue : item.value}
                  </motion.text>
                </g>
              );
            })}
          </svg>
        </div>
      </div>
    );
  }

  // Простая визуализация сложности
  return (
    <div className="w-full h-[400px] bg-card rounded-lg border p-6 flex items-center justify-center">
      <div className="text-center space-y-6">
        <motion.div
          initial={{ scale: 0, rotate: -180 }}
          animate={{ scale: 1, rotate: 0 }}
          transition={{ duration: 0.6, type: 'spring' }}
          className="mx-auto w-32 h-32 rounded-full bg-gradient-to-br from-green-500 to-green-600 flex items-center justify-center"
        >
          <span className="text-3xl text-white font-mono">
            {analysisData.complexity}
          </span>
        </motion.div>
        <div className="space-y-2">
          <p className="text-muted-foreground">Временная сложность</p>
          <div className="flex gap-4 justify-center text-sm">
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-amber-500"></div>
              <span>{analysisData.warnings} предупреждений</span>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-3 h-3 rounded-full bg-green-500"></div>
              <span>{analysisData.suggestions} рекомендаций</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
