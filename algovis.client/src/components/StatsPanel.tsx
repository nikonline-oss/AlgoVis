import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { Badge } from './ui/badge';
import { useApp } from '../contexts/AppContext';

interface StatsPanelProps {
  dataStructure: 'array' | 'tree' | 'graph' | 'list' | 'stack' | 'queue' | 'heap' | 'hashtable';
  algorithm: string;
  stats: {
    comparisons?: number;
    swaps?: number;
    operations?: number;
    time?: number;
  };
  dataSize?: number;
}

export function StatsPanel({ dataStructure, algorithm, stats, dataSize }: StatsPanelProps) {
  const { translations, language } = useApp();

  const getComplexity = () => {
    const complexities: Record<string, { best: string; avg: string; worst: string }> = {
      // Sorting algorithms
      'bubblesort': { best: 'O(n)', avg: 'O(n²)', worst: 'O(n²)' },
      'quicksort': { best: 'O(n log n)', avg: 'O(n log n)', worst: 'O(n²)' },
      'insertionsort': { best: 'O(n)', avg: 'O(n²)', worst: 'O(n²)' },
      'selectionsort': { best: 'O(n²)', avg: 'O(n²)', worst: 'O(n²)' },
      
      // Tree algorithms - ВНИМАНИЕ: используйте ТОЧНО ТЕ ЖЕ ключи, что и в вашем коде
      'bst.inorder': { best: 'O(n)', avg: 'O(n)', worst: 'O(n)' },
      'bst.preorder': { best: 'O(n)', avg: 'O(n)', worst: 'O(n)' },
      'bst.postorder': { best: 'O(n)', avg: 'O(n)', worst: 'O(n)' },
      'bst.levelorder': { best: 'O(n)', avg: 'O(n)', worst: 'O(n)' },
      'bst.insert': { best: 'O(log n)', avg: 'O(log n)', worst: 'O(n)' },
      'bst.search': { best: 'O(log n)', avg: 'O(log n)', worst: 'O(n)' },
      'bst.delete': { best: 'O(log n)', avg: 'O(log n)', worst: 'O(n)' },
      
      // Graph algorithms
      'bfs': { best: 'O(V + E)', avg: 'O(V + E)', worst: 'O(V + E)' },
      'dfs': { best: 'O(V + E)', avg: 'O(V + E)', worst: 'O(V + E)' },
      'dijkstra': { best: 'O((V + E) log V)', avg: 'O((V + E) log V)', worst: 'O((V + E) log V)' },
      'prim': { best: 'O(E log V)', avg: 'O(E log V)', worst: 'O(E log V)' },
    };

    return complexities[algorithm] || { best: '-', avg: '-', worst: '-' };
  };

  const complexity = getComplexity();

  const getAlgorithmName = () => {
    // Пробуем получить перевод из AppContext
    const translationKey = `algorithm.${algorithm}`;
    if (translations[translationKey]) {
      return translations[translationKey];
    }
    
    // Fallback для случаев, если перевода нет
    const fallbackNames: Record<string, string> = {
      // Алгоритмы сортировки
      'bubblesort': language === 'ru' ? 'Пузырьковая сортировка' : 'Bubble Sort',
      'quicksort': language === 'ru' ? 'Быстрая сортировка' : 'Quick Sort',
      'insertionsort': language === 'ru' ? 'Сортировка вставками' : 'Insertion Sort',
      'selectionsort': language === 'ru' ? 'Сортировка выбором' : 'Selection Sort',
      
      // Алгоритмы обхода деревьев
      'bst.inorder': language === 'ru' ? 'Центрированный обход' : 'In-order Traversal',
      'bst.preorder': language === 'ru' ? 'Прямой обход' : 'Pre-order Traversal',
      'bst.postorder': language === 'ru' ? 'Обратный обход' : 'Post-order Traversal',
      'bst.levelorder': language === 'ru' ? 'Обход в ширину' : 'Level-order Traversal',
      
      // Графовые алгоритмы
      'bfs': language === 'ru' ? 'Поиск в ширину (BFS)' : 'Breadth-First Search (BFS)',
      'dfs': language === 'ru' ? 'Поиск в глубину (DFS)' : 'Depth-First Search (DFS)',
      'dijkstra': language === 'ru' ? 'Алгоритм Дейкстры' : "Dijkstra's Algorithm",
    };

    return fallbackNames[algorithm] || algorithm;
  };

  // Проверяем, нужно ли показывать статистику
  const shouldShowStats = () => {
    // Для списков, стеков, очередей и т.д. статистика не рассчитывается
    if (['list', 'stack', 'queue'].includes(dataStructure)) {
      return false;
    }
    
    // Для массивов, деревьев и графов статистика есть
    if (['array', 'tree', 'graph'].includes(dataStructure)) {
      return true;
    }
    
    return false;
  };

  if (!shouldShowStats()) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>
            {language === 'ru' ? 'Информация' : 'Information'}
          </CardTitle>
        </CardHeader>
        <CardContent>
          <p className="text-sm text-muted-foreground">
            {language === 'ru' 
              ? `Для ${dataStructure === 'list' ? 'связного списка' : 
                         dataStructure === 'stack' ? 'стека' : 
                         'очереди'} статистика операций не рассчитывается.`
              : `For ${dataStructure === 'list' ? 'linked list' : 
                         dataStructure === 'stack' ? 'stack' : 
                         'queue'} operations statistics are not calculated.`
            }
          </p>
        </CardContent>
      </Card>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle className="flex items-center justify-between">
          <span>{language === 'ru' ? 'Статистика' : 'Statistics'}</span>
          <Badge variant="secondary">{complexity.avg}</Badge>
        </CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <div className="text-sm text-muted-foreground mb-1">
            {language === 'ru' ? 'Алгоритм' : 'Algorithm'}
          </div>
          <div className="font-medium">{getAlgorithmName()}</div>
        </div>

        {dataSize !== undefined && (
          <div>
            <div className="text-sm text-muted-foreground mb-1">
              {language === 'ru' ? 'Размер данных' : 'Data Size'}
            </div>
            <div className="font-medium">{dataSize}</div>
          </div>
        )}

        <div className="space-y-2">
          <div className="text-sm text-muted-foreground">
            {language === 'ru' ? 'Временная сложность:' : 'Time Complexity:'}
          </div>
          <div className="grid grid-cols-3 gap-2 text-sm">
            <div>
              <div className="text-muted-foreground">
                {language === 'ru' ? 'Лучший' : 'Best'}
              </div>
              <Badge variant="outline" className="mt-1">{complexity.best}</Badge>
            </div>
            <div>
              <div className="text-muted-foreground">
                {language === 'ru' ? 'Средний' : 'Average'}
              </div>
              <Badge variant="outline" className="mt-1">{complexity.avg}</Badge>
            </div>
            <div>
              <div className="text-muted-foreground">
                {language === 'ru' ? 'Худший' : 'Worst'}
              </div>
              <Badge variant="outline" className="mt-1">{complexity.worst}</Badge>
            </div>
          </div>
        </div>

        <div className="space-y-2 pt-2 border-t">
          <div className="text-sm text-muted-foreground">
            {language === 'ru' ? 'Выполнение:' : 'Execution:'}
          </div>
          
          {dataStructure === 'array' && (
            <>
              {stats.comparisons !== undefined && (
                <div className="flex justify-between text-sm">
                  <span>{translations['profiler.comparisons'] || 'Сравнения'}:</span>
                  <span className="font-medium text-primary">{stats.comparisons}</span>
                </div>
              )}
              {stats.swaps !== undefined && (
                <div className="flex justify-between text-sm">
                  <span>{translations['profiler.swaps'] || 'Перестановки'}:</span>
                  <span className="font-medium text-primary">{stats.swaps}</span>
                </div>
              )}
            </>
          )}
          
          {/* Показываем операции для всех алгоритмов, если они есть */}
          {stats.operations !== undefined && (
            <div className="flex justify-between text-sm">
              <span>{translations['profiler.operations'] || 'Операции'}:</span>
              <span className="font-medium text-primary">{stats.operations}</span>
            </div>
          )}
          
          {stats.time !== undefined && (
            <div className="flex justify-between text-sm">
              <span>{translations['profiler.time'] || 'Время'}:</span>
              <span className="font-medium text-primary">{stats.time.toFixed(2)} мс</span>
            </div>
          )}
          
          {/* Если нет статистики, показываем сообщение */}
          {stats.comparisons === undefined && 
           stats.swaps === undefined && 
           stats.operations === undefined && 
           stats.time === undefined && (
            <div className="text-sm text-muted-foreground">
              {language === 'ru' 
                ? 'Запустите алгоритм для получения статистики' 
                : 'Run the algorithm to get statistics'
              }
            </div>
          )}
        </div>

        <div className="pt-2 border-t">
          <div className="text-xs text-muted-foreground">
            {language === 'ru' 
              ? 'V - количество вершин, E - количество рёбер, n - размер данных'
              : 'V - number of vertices, E - number of edges, n - data size'
            }
          </div>
        </div>
      </CardContent>
    </Card>
  );
}