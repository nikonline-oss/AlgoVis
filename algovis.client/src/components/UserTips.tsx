import React, { useState } from 'react';
import { Card, CardContent } from './ui/card';
import { Button } from './ui/button';
import { HelpCircle, X, ChevronDown, ChevronUp } from 'lucide-react';

interface UserTipsProps {
  dataStructure: string;
}

export function UserTips({ dataStructure }: UserTipsProps) {
  const [isVisible, setIsVisible] = useState(true);
  const [isExpanded, setIsExpanded] = useState(false);

  const getTipsForStructure = () => {
    const generalTips = [
      'Начните с сортировки пузырьком - это самый простой алгоритм',
      'Используйте пошаговую анимацию для понимания каждого шага',
      'Измените размер данных, чтобы увидеть разницу в скорости',
      'Сравните время работы разных алгоритмов на одних данных'
    ];

    const structureTips: Record<string, string[]> = {
      'array': [
        'Уменьшите скорость анимации для изучения деталей',
        'Попробуйте отсортировать уже отсортированный массив',
        'Измените размер массива от 5 до 35 элементов',
        'Сравните Bubble Sort и Quick Sort на большом массиве'
      ],
      'tree': [
        'Используйте разные обходы для одного дерева',
        'Вставляйте значения и наблюдайте изменения',
        'Обратите внимание на балансировку дерева',
        'Изучите порядок обхода для каждого алгоритма'
      ],
      'graph': [
        'Измените тип графа для разных визуализаций',
        'Попробуйте ориентированный и неориентированный графы',
        'Сравните BFS и DFS на одном графе',
        'Добавьте вес ребрам для алгоритма Дейкстры'
      ],
      'list': [
        'Сравните односвязный и двусвязный списки',
        'Попробуйте вставить элемент в начало и конец',
        'Удалите элемент из середины списка',
        'Проследите за изменением связей'
      ],
      'stack': [
        'Посмотрите принцип LIFO (Last In, First Out)',
        'Попробуйте операцию Pop после Push',
        'Обратите внимание на указатель top',
        'Сравните со очередью'
      ],
      'queue': [
        'Посмотрите принцип FIFO (First In, First Out)',
        'Попробуйте операцию Dequeue после Enqueue',
        'Обратите внимание на указатели front и rear',
        'Сравните со стеком'
      ]
    };

    return [...generalTips, ...(structureTips[dataStructure] || [])];
  };

  if (!isVisible) {
    return (
      <Button 
        variant="outline" 
        size="sm" 
        onClick={() => setIsVisible(true)}
        className="w-full"
      >
        <HelpCircle className="w-4 h-4 mr-2" />
        Показать подсказки
      </Button>
    );
  }

  const tips = getTipsForStructure();
  const displayedTips = isExpanded ? tips : tips.slice(0, 3);

  return (
    <Card className="bg-blue-50 dark:bg-blue-900/20 border-blue-200 dark:border-blue-800">
      <CardContent className="pt-4">
        <div className="flex justify-between items-start mb-2">
          <div className="flex items-center">
            <HelpCircle className="w-5 h-5 mr-2 text-blue-600 dark:text-blue-400" />
            <span className="font-semibold">🎓 Советы для обучения:</span>
          </div>
          <Button 
            variant="ghost" 
            size="sm" 
            onClick={() => setIsVisible(false)}
            className="h-6 w-6 p-0"
          >
            <X className="w-4 h-4" />
          </Button>
        </div>
        
        <ul className="text-sm space-y-1 mb-3">
          {displayedTips.map((tip, index) => (
            <li key={index} className="flex items-start">
              <span className="mr-2">•</span>
              {tip}
            </li>
          ))}
        </ul>

        {tips.length > 3 && (
          <Button 
            variant="ghost" 
            size="sm" 
            onClick={() => setIsExpanded(!isExpanded)}
            className="text-blue-600 dark:text-blue-400 p-0 h-auto"
          >
            {isExpanded ? (
              <>
                <ChevronUp className="w-4 h-4 mr-1" />
                Свернуть
              </>
            ) : (
              <>
                <ChevronDown className="w-4 h-4 mr-1" />
                Ещё {tips.length - 3} советов
              </>
            )}
          </Button>
        )}
      </CardContent>
    </Card>
  );
}