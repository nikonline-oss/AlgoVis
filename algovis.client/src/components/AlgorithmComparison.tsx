import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { useApp } from '../contexts/AppContext';

interface AlgorithmInfo {
  name: string;
  bestCase: string;
  averageCase: string;
  worstCase: string;
  space: string;
  stable: boolean;
}

interface AlgorithmComparisonProps {
  currentAlgorithm: string;
  dataStructure: string;
}

export function AlgorithmComparison({ currentAlgorithm, dataStructure }: AlgorithmComparisonProps) {
  const { translations } = useApp();

  const getAlgorithmsForStructure = () => {
    if (dataStructure === 'array') {
      return {
        'bubblesort': {
          name: translations['algorithm.bubblesort'] || 'Пузырьковая сортировка',
          bestCase: 'O(n)',
          averageCase: 'O(n²)',
          worstCase: 'O(n²)',
          space: 'O(1)',
          stable: true
        },
        'quicksort': {
          name: translations['algorithm.quicksort'] || 'Быстрая сортировка',
          bestCase: 'O(n log n)',
          averageCase: 'O(n log n)',
          worstCase: 'O(n²)',
          space: 'O(log n)',
          stable: false
        },
        'insertionsort': {
          name: translations['algorithm.insertionsort'] || 'Сортировка вставками',
          bestCase: 'O(n)',
          averageCase: 'O(n²)',
          worstCase: 'O(n²)',
          space: 'O(1)',
          stable: true
        },
        'selectionsort': {
          name: translations['algorithm.selectionsort'] || 'Сортировка выбором',
          bestCase: 'O(n²)',
          averageCase: 'O(n²)',
          worstCase: 'O(n²)',
          space: 'O(1)',
          stable: false
        }
      };
    }
    
    if (dataStructure === 'tree') {
      return {
        'bst.inorder': {
          name: translations['algorithm.bst.inorder'] || 'Центрированный обход',
          bestCase: 'O(n)',
          averageCase: 'O(n)',
          worstCase: 'O(n)',
          space: 'O(h)',
          stable: true
        },
        'bst.preorder': {
          name: translations['algorithm.bst.preorder'] || 'Прямой обход',
          bestCase: 'O(n)',
          averageCase: 'O(n)',
          worstCase: 'O(n)',
          space: 'O(h)',
          stable: true
        },
        'bst.postorder': {
          name: translations['algorithm.bst.postorder'] || 'Обратный обход',
          bestCase: 'O(n)',
          averageCase: 'O(n)',
          worstCase: 'O(n)',
          space: 'O(h)',
          stable: true
        },
        'bst.levelorder': {
          name: translations['algorithm.bst.levelorder'] || 'Обход в ширину',
          bestCase: 'O(n)',
          averageCase: 'O(n)',
          worstCase: 'O(n)',
          space: 'O(w)',
          stable: true
        }
      };
    }

    if (dataStructure === 'graph') {
      return {
        'bfs': {
          name: translations['algorithm.bfs'] || 'Поиск в ширину',
          bestCase: 'O(V + E)',
          averageCase: 'O(V + E)',
          worstCase: 'O(V + E)',
          space: 'O(V)',
          stable: true
        },
        'dfs': {
          name: translations['algorithm.dfs'] || 'Поиск в глубину',
          bestCase: 'O(V + E)',
          averageCase: 'O(V + E)',
          worstCase: 'O(V + E)',
          space: 'O(h)',
          stable: true
        },
        'dijkstra': {
          name: translations['algorithm.dijkstra'] || 'Алгоритм Дейкстры',
          bestCase: 'O((V+E)log V)',
          averageCase: 'O((V+E)log V)',
          worstCase: 'O((V+E)log V)',
          space: 'O(V)',
          stable: true
        }
      };
    }

    return {};
  };

  const algorithms = getAlgorithmsForStructure();
  if (Object.keys(algorithms).length === 0) return null;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Сравнение алгоритмов</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="overflow-x-auto rounded-md border">
          <table className="w-full text-sm">
            <thead className="bg-muted/50">
              <tr>
                <th className="text-left p-3 font-medium border-r">Алгоритм</th>
                <th className="text-left p-3 font-medium border-r">Лучший</th>
                <th className="text-left p-3 font-medium border-r">Средний</th>
                <th className="text-left p-3 font-medium border-r">Худший</th>
                <th className="text-left p-3 font-medium border-r">Память</th>
                <th className="text-left p-3 font-medium">Стаб.</th>
              </tr>
            </thead>
            <tbody>
              {Object.entries(algorithms).map(([key, algo]) => (
                <tr 
                  key={key}
                  className={`border-b hover:bg-muted/30 transition-colors ${
                    key === currentAlgorithm 
                      ? 'bg-primary/10 border-l-4 border-l-primary' 
                      : 'border-l-4 border-l-transparent'
                  }`}
                >
                  <td className="p-3 font-medium border-r">
                    <div className="flex items-center">
                      {key === currentAlgorithm && (
                        <div className="w-2 h-2 rounded-full bg-primary mr-2"></div>
                      )}
                      {algo.name}
                    </div>
                  </td>
                  <td className="p-3 font-mono border-r text-green-600 dark:text-green-400">
                    {algo.bestCase}
                  </td>
                  <td className="p-3 font-mono border-r">
                    {algo.averageCase}
                  </td>
                  <td className="p-3 font-mono border-r text-red-600 dark:text-red-400">
                    {algo.worstCase}
                  </td>
                  <td className="p-3 font-mono border-r">
                    {algo.space}
                  </td>
                  <td className="p-3">
                    <div className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${
                      algo.stable 
                        ? 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400' 
                        : 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400'
                    }`}>
                      {algo.stable ? 'Да' : 'Нет'}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
        
        <div className="mt-4 p-3 bg-muted/30 rounded-md">
          
          
          <div className="mt-2 pt-2 border-t text-xs ">
            <p className="mb-1">Обозначения:</p>
            <div className="grid grid-cols-2 gap-1 text-muted-foreground">
              <span>n - количество элементов</span>
              <span>V - количество вершин</span>
              <span>E - количество рёбер</span>
              <span>h - высота дерева</span>
              <span>w - ширина дерева</span>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}