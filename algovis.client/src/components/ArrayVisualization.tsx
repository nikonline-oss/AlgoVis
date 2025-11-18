import React from 'react';

interface ArrayVisualizationProps {
  array: number[];
  comparing?: number[];
  swapping?: number[];
  sorted?: number[];
  pivotIndex?: number;
}

export function ArrayVisualization({ 
  array, 
  comparing = [], 
  swapping = [], 
  sorted = [],
  pivotIndex 
}: ArrayVisualizationProps) {
  const maxValue = Math.max(...array);

  const getBarColor = (index: number) => {
    if (pivotIndex === index) return 'bg-purple-500';
    if (sorted.includes(index)) return 'bg-primary';
    if (swapping.includes(index)) return 'bg-red-500';
    if (comparing.includes(index)) return 'bg-yellow-500';
    return 'bg-muted';
  };

  const getBarHeight = (value: number) => {
    return (value / maxValue) * 300;
  };

  return (
    <div className="space-y-4">
      {/* Визуализация массива */}
      <div className="flex items-end justify-center space-x-1 p-4 bg-card border rounded-lg min-h-[400px]">
        {array.map((value, index) => (
          <div
            key={index}
            className="flex flex-col items-center space-y-2"
          >
            <div
              className={`transition-all duration-300 rounded-t ${getBarColor(index)}`}
              style={{
                height: `${getBarHeight(value)}px`,
                width: Math.max(400 / array.length - 4, 20),
                minWidth: '4px',
              }}
            />
            <span className="text-xs text-muted-foreground">
              {value}
            </span>
          </div>
        ))}
      </div>

      {/* Легенда для массива */}
      <div className="flex justify-center flex-wrap gap-4 text-sm">
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 bg-muted rounded-sm" />
          <span className="text-muted-foreground">Обычный</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 bg-yellow-500 rounded-sm" />
          <span className="text-muted-foreground">Сравнивается</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 bg-red-500 rounded-sm" />
          <span className="text-muted-foreground">Обменивается</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 bg-primary rounded-sm" />
          <span className="text-muted-foreground">Отсортирован</span>
        </div>
        <div className="flex items-center space-x-2">
          <div className="w-4 h-4 bg-purple-500 rounded-sm" />
          <span className="text-muted-foreground">Опорный элемент</span>
        </div>
      </div>
    </div>
  );
}