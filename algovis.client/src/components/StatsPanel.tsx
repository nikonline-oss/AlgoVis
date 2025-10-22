import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { Badge } from './ui/badge';
import { useApp } from '../contexts/AppContext';

interface StatsPanelProps {
  dataStructure: 'array' | 'tree' | 'graph';
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
      'mergesort': { best: 'O(n log n)', avg: 'O(n log n)', worst: 'O(n log n)' },
      'heapsort': { best: 'O(n log n)', avg: 'O(n log n)', worst: 'O(n log n)' },
      
      // Tree algorithms
      'bst.insert': { best: 'O(log n)', avg: 'O(log n)', worst: 'O(n)' },
      'bst.search': { best: 'O(log n)', avg: 'O(log n)', worst: 'O(n)' },
      'bst.delete': { best: 'O(log n)', avg: 'O(log n)', worst: 'O(n)' },
      'bst.inorder': { best: 'O(n)', avg: 'O(n)', worst: 'O(n)' },
      'bst.preorder': { best: 'O(n)', avg: 'O(n)', worst: 'O(n)' },
      'bst.postorder': { best: 'O(n)', avg: 'O(n)', worst: 'O(n)' },
      
      // Graph algorithms
      'bfs': { best: 'O(V + E)', avg: 'O(V + E)', worst: 'O(V + E)' },
      'dfs': { best: 'O(V + E)', avg: 'O(V + E)', worst: 'O(V + E)' },
      'dijkstra': { best: 'O(V²)', avg: 'O(V²)', worst: 'O(V²)' },
      'prim': { best: 'O(E log V)', avg: 'O(E log V)', worst: 'O(E log V)' },
    };

    return complexities[algorithm] || { best: '-', avg: '-', worst: '-' };
  };

  const complexity = getComplexity();

  const getAlgorithmName = () => {
    const names: Record<string, string> = {
      'bubblesort': translations['algorithm.bubblesort'],
      'quicksort': translations['algorithm.quicksort'],
      'mergesort': translations['algorithm.mergesort'],
      'heapsort': translations['algorithm.heapsort'],
      'bst.insert': translations['algorithm.bst.insert'],
      'bst.search': translations['algorithm.bst.search'],
      'bst.delete': translations['algorithm.bst.delete'],
      'bst.inorder': translations['algorithm.bst.inorder'],
      'bst.preorder': translations['algorithm.bst.preorder'],
      'bst.postorder': translations['algorithm.bst.postorder'],
      'bfs': translations['algorithm.bfs'],
      'dfs': translations['algorithm.dfs'],
      'dijkstra': translations['algorithm.dijkstra'],
      'prim': translations['algorithm.prim'],
    };
    return names[algorithm] || algorithm;
  };

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
                  <span>{translations['profiler.comparisons']}:</span>
                  <span className="font-medium text-primary">{stats.comparisons}</span>
                </div>
              )}
              {stats.swaps !== undefined && (
                <div className="flex justify-between text-sm">
                  <span>{translations['profiler.swaps']}:</span>
                  <span className="font-medium text-primary">{stats.swaps}</span>
                </div>
              )}
            </>
          )}
          
          {stats.operations !== undefined && (
            <div className="flex justify-between text-sm">
              <span>{translations['profiler.operations']}:</span>
              <span className="font-medium text-primary">{stats.operations}</span>
            </div>
          )}
          
          {stats.time !== undefined && (
            <div className="flex justify-between text-sm">
              <span>{translations['profiler.time']}:</span>
              <span className="font-medium text-primary">{stats.time.toFixed(2)} мс</span>
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
