import React, { useState } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Slider } from '../components/ui/slider';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line, Legend } from 'recharts';
import { useApp } from '../contexts/AppContext';
import { Send } from 'lucide-react';

interface AlgorithmResult {
  name: string;
  time: number;
  comparisons?: number;
  swaps?: number;
  operations?: number;
}

interface ProfilerPageProps {
  onNavigate: (page: string) => void;
}

export function ProfilerPage({ onNavigate }: ProfilerPageProps) {
  const { translations, setSharedData } = useApp();
  const [dataStructure, setDataStructure] = useState<'array' | 'tree' | 'graph'>('array');
  const [dataSize, setDataSize] = useState(1000);
  const [results, setResults] = useState<AlgorithmResult[]>([]);
  const [isRunning, setIsRunning] = useState(false);

  // Sorting algorithms
  const bubbleSort = (arr: number[]) => {
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;
    const start = performance.now();
    
    for (let i = 0; i < array.length - 1; i++) {
      for (let j = 0; j < array.length - i - 1; j++) {
        comparisons++;
        if (array[j] > array[j + 1]) {
          [array[j], array[j + 1]] = [array[j + 1], array[j]];
          swaps++;
        }
      }
    }
    
    const time = performance.now() - start;
    return { time, comparisons, swaps, operations: comparisons + swaps };
  };

  const quickSort = (arr: number[]) => {
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;
    const start = performance.now();
    
    const partition = (low: number, high: number): number => {
      const pivot = array[high];
      let i = low - 1;
      
      for (let j = low; j < high; j++) {
        comparisons++;
        if (array[j] < pivot) {
          i++;
          if (i !== j) {
            [array[i], array[j]] = [array[j], array[i]];
            swaps++;
          }
        }
      }
      
      [array[i + 1], array[high]] = [array[high], array[i + 1]];
      swaps++;
      return i + 1;
    };
    
    const quickSortHelper = (low: number, high: number) => {
      if (low < high) {
        const pi = partition(low, high);
        quickSortHelper(low, pi - 1);
        quickSortHelper(pi + 1, high);
      }
    };
    
    quickSortHelper(0, array.length - 1);
    const time = performance.now() - start;
    return { time, comparisons, swaps, operations: comparisons + swaps };
  };

  const mergeSort = (arr: number[]) => {
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;
    const start = performance.now();
    
    const merge = (left: number[], right: number[]): number[] => {
      const result = [];
      let i = 0, j = 0;
      
      while (i < left.length && j < right.length) {
        comparisons++;
        if (left[i] <= right[j]) {
          result.push(left[i]);
          i++;
        } else {
          result.push(right[j]);
          j++;
          swaps++;
        }
      }
      
      return result.concat(left.slice(i)).concat(right.slice(j));
    };
    
    const mergeSortHelper = (arr: number[]): number[] => {
      if (arr.length <= 1) return arr;
      
      const mid = Math.floor(arr.length / 2);
      const left = mergeSortHelper(arr.slice(0, mid));
      const right = mergeSortHelper(arr.slice(mid));
      
      return merge(left, right);
    };
    
    mergeSortHelper(array);
    const time = performance.now() - start;
    return { time, comparisons, swaps, operations: comparisons + swaps };
  };

  // Tree algorithms
  const bstOperations = (size: number) => {
    let operations = 0;
    const start = performance.now();
    
    interface TreeNode {
      value: number;
      left?: TreeNode;
      right?: TreeNode;
    }
    
    let root: TreeNode | null = null;
    
    const insert = (node: TreeNode | null, value: number): TreeNode => {
      operations++;
      if (!node) return { value };
      
      if (value < node.value) {
        node.left = insert(node.left || null, value);
      } else {
        node.right = insert(node.right || null, value);
      }
      return node;
    };
    
    const search = (node: TreeNode | null, value: number): boolean => {
      operations++;
      if (!node) return false;
      if (node.value === value) return true;
      
      if (value < node.value) {
        return search(node.left || null, value);
      } else {
        return search(node.right || null, value);
      }
    };
    
    // Insert nodes
    for (let i = 0; i < size; i++) {
      root = insert(root, Math.floor(Math.random() * 1000));
    }
    
    // Search for random values
    for (let i = 0; i < Math.min(100, size); i++) {
      search(root, Math.floor(Math.random() * 1000));
    }
    
    const time = performance.now() - start;
    return { time, operations };
  };

  // Graph algorithms
  const graphBFS = (nodeCount: number, edgeCount: number) => {
    let operations = 0;
    const start = performance.now();
    
    // Create adjacency list
    const graph = new Map<number, number[]>();
    for (let i = 0; i < nodeCount; i++) {
      graph.set(i, []);
    }
    
    // Add random edges
    for (let i = 0; i < edgeCount; i++) {
      const from = Math.floor(Math.random() * nodeCount);
      const to = Math.floor(Math.random() * nodeCount);
      if (from !== to) {
        graph.get(from)?.push(to);
      }
    }
    
    // BFS
    const visited = new Set<number>();
    const queue: number[] = [0];
    
    while (queue.length > 0) {
      const current = queue.shift()!;
      operations++;
      
      if (visited.has(current)) continue;
      visited.add(current);
      
      const neighbors = graph.get(current) || [];
      neighbors.forEach(neighbor => {
        if (!visited.has(neighbor)) {
          queue.push(neighbor);
        }
      });
    }
    
    const time = performance.now() - start;
    return { time, operations };
  };

  const graphDFS = (nodeCount: number, edgeCount: number) => {
    let operations = 0;
    const start = performance.now();
    
    // Create adjacency list
    const graph = new Map<number, number[]>();
    for (let i = 0; i < nodeCount; i++) {
      graph.set(i, []);
    }
    
    // Add random edges
    for (let i = 0; i < edgeCount; i++) {
      const from = Math.floor(Math.random() * nodeCount);
      const to = Math.floor(Math.random() * nodeCount);
      if (from !== to) {
        graph.get(from)?.push(to);
      }
    }
    
    // DFS
    const visited = new Set<number>();
    
    const dfs = (node: number) => {
      operations++;
      if (visited.has(node)) return;
      visited.add(node);
      
      const neighbors = graph.get(node) || [];
      neighbors.forEach(neighbor => {
        if (!visited.has(neighbor)) {
          dfs(neighbor);
        }
      });
    };
    
    dfs(0);
    
    const time = performance.now() - start;
    return { time, operations };
  };

  const runComparison = async () => {
    setIsRunning(true);
    setResults([]);
    
    if (dataStructure === 'array') {
      const testArray = Array.from({ length: dataSize }, () => 
        Math.floor(Math.random() * 1000) + 1
      );
      
      const algorithms = [
        { name: translations['algorithm.bubblesort'], fn: () => bubbleSort(testArray) },
        { name: translations['algorithm.quicksort'], fn: () => quickSort(testArray) },
        { name: translations['algorithm.mergesort'], fn: () => mergeSort(testArray) },
      ];
      
      const newResults: AlgorithmResult[] = [];
      
      for (const algorithm of algorithms) {
        await new Promise(resolve => setTimeout(resolve, 100));
        const result = algorithm.fn();
        newResults.push({ name: algorithm.name, ...result });
        setResults([...newResults]);
      }
    } else if (dataStructure === 'tree') {
      const algorithms = [
        { name: 'BST Operations', fn: () => bstOperations(dataSize) },
      ];
      
      const newResults: AlgorithmResult[] = [];
      
      for (const algorithm of algorithms) {
        await new Promise(resolve => setTimeout(resolve, 100));
        const result = algorithm.fn();
        newResults.push({ name: algorithm.name, ...result });
        setResults([...newResults]);
      }
    } else if (dataStructure === 'graph') {
      const edgeCount = Math.min(dataSize * 2, dataSize * (dataSize - 1) / 2);
      
      const algorithms = [
        { name: 'BFS', fn: () => graphBFS(dataSize, edgeCount) },
        { name: 'DFS', fn: () => graphDFS(dataSize, edgeCount) },
      ];
      
      const newResults: AlgorithmResult[] = [];
      
      for (const algorithm of algorithms) {
        await new Promise(resolve => setTimeout(resolve, 100));
        const result = algorithm.fn();
        newResults.push({ name: algorithm.name, ...result });
        setResults([...newResults]);
      }
    }
    
    setIsRunning(false);
  };

  const sendToVisualizer = () => {
    if (dataStructure === 'array') {
      const testArray = Array.from({ length: Math.min(20, dataSize) }, () => 
        Math.floor(Math.random() * 100) + 1
      );
      setSharedData({
        type: 'array',
        algorithm: 'bubblesort',
        data: testArray,
      });
    } else if (dataStructure === 'tree') {
      setSharedData({
        type: 'tree',
        algorithm: 'bst.inorder',
      });
    } else if (dataStructure === 'graph') {
      setSharedData({
        type: 'graph',
        algorithm: 'bfs',
      });
    }
    onNavigate('visualizer');
  };

  const chartData = results.map(result => ({
    name: result.name,
    time: parseFloat(result.time.toFixed(2)),
    comparisons: result.comparisons || 0,
    swaps: result.swaps || 0,
    operations: result.operations || 0,
  }));

  const complexityData = dataStructure === 'array' ? [
    { size: 100, bubble: 100 * 100 / 10000, quick: 100 * Math.log2(100) / 100, merge: 100 * Math.log2(100) / 100 },
    { size: 500, bubble: 500 * 500 / 10000, quick: 500 * Math.log2(500) / 100, merge: 500 * Math.log2(500) / 100 },
    { size: 1000, bubble: 1000 * 1000 / 10000, quick: 1000 * Math.log2(1000) / 100, merge: 1000 * Math.log2(1000) / 100 },
    { size: 2000, bubble: 2000 * 2000 / 10000, quick: 2000 * Math.log2(2000) / 100, merge: 2000 * Math.log2(2000) / 100 },
    { size: 5000, bubble: 5000 * 5000 / 10000, quick: 5000 * Math.log2(5000) / 100, merge: 5000 * Math.log2(5000) / 100 },
  ] : dataStructure === 'tree' ? [
    { size: 100, bst: 100 * Math.log2(100) / 10 },
    { size: 500, bst: 500 * Math.log2(500) / 10 },
    { size: 1000, bst: 1000 * Math.log2(1000) / 10 },
    { size: 2000, bst: 2000 * Math.log2(2000) / 10 },
    { size: 5000, bst: 5000 * Math.log2(5000) / 10 },
  ] : [
    { size: 10, bfs: 10 + 20, dfs: 10 + 20 },
    { size: 50, bfs: 50 + 100, dfs: 50 + 100 },
    { size: 100, bfs: 100 + 200, dfs: 100 + 200 },
    { size: 200, bfs: 200 + 400, dfs: 200 + 400 },
    { size: 500, bfs: 500 + 1000, dfs: 500 + 1000 },
  ];

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>{translations['profiler.title']}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <Tabs value={dataStructure} onValueChange={(v) => setDataStructure(v as any)}>
            <TabsList className="grid w-full grid-cols-3">
              <TabsTrigger value="array">{translations['structure.array']}</TabsTrigger>
              <TabsTrigger value="tree">{translations['structure.tree']}</TabsTrigger>
              <TabsTrigger value="graph">{translations['structure.graph']}</TabsTrigger>
            </TabsList>
          </Tabs>

          <div className="space-y-2">
            <label className="text-sm">
              {translations['data.size']}: {dataSize}
            </label>
            <Slider
              value={[dataSize]}
              onValueChange={(value) => setDataSize(value[0])}
              max={dataStructure === 'array' ? 5000 : dataStructure === 'tree' ? 1000 : 100}
              min={dataStructure === 'array' ? 100 : dataStructure === 'tree' ? 10 : 5}
              step={dataStructure === 'array' ? 100 : dataStructure === 'tree' ? 10 : 1}
              className="w-full"
            />
          </div>
          
          <div className="flex space-x-2">
            <Button
              onClick={runComparison}
              disabled={isRunning}
              className="bg-primary hover:bg-primary/90 text-primary-foreground flex-1"
            >
              {isRunning ? 'Выполняется...' : translations['profiler.run']}
            </Button>
            <Button
              onClick={sendToVisualizer}
              variant="outline"
              className="flex items-center space-x-2"
            >
              <Send className="h-4 w-4" />
              <span>{translations['profiler.sendToVisualizer']}</span>
            </Button>
          </div>
        </CardContent>
      </Card>

      {results.length > 0 && (
        <div className="grid lg:grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <CardTitle>{translations['profiler.time']} (мс)</CardTitle>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="time" fill="var(--color-primary)" />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>
                {dataStructure === 'array' ? 'Операции' : translations['profiler.operations']}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  {dataStructure === 'array' ? (
                    <>
                      <Bar dataKey="comparisons" fill="var(--color-chart-1)" name="Сравнения" />
                      <Bar dataKey="swaps" fill="var(--color-chart-2)" name="Перестановки" />
                    </>
                  ) : (
                    <Bar dataKey="operations" fill="var(--color-chart-1)" name="Операции" />
                  )}
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </div>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Теоретическая сложность</CardTitle>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={400}>
            <LineChart data={complexityData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="size" />
              <YAxis />
              <Tooltip />
              <Legend />
              {dataStructure === 'array' && (
                <>
                  <Line type="monotone" dataKey="bubble" stroke="var(--color-chart-1)" name="Bubble Sort O(n²)" />
                  <Line type="monotone" dataKey="quick" stroke="var(--color-chart-2)" name="Quick Sort O(n log n)" />
                  <Line type="monotone" dataKey="merge" stroke="var(--color-chart-3)" name="Merge Sort O(n log n)" />
                </>
              )}
              {dataStructure === 'tree' && (
                <Line type="monotone" dataKey="bst" stroke="var(--color-chart-1)" name="BST O(log n)" />
              )}
              {dataStructure === 'graph' && (
                <>
                  <Line type="monotone" dataKey="bfs" stroke="var(--color-chart-1)" name="BFS O(V + E)" />
                  <Line type="monotone" dataKey="dfs" stroke="var(--color-chart-2)" name="DFS O(V + E)" />
                </>
              )}
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {results.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Детальные результаты</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {results.map((result, index) => (
                <div key={index} className="flex items-center justify-between p-4 border rounded-lg">
                  <div>
                    <h4 className="font-medium">{result.name}</h4>
                    <div className="flex space-x-4 text-sm text-muted-foreground">
                      <span>Время: {result.time.toFixed(2)} мс</span>
                      {result.comparisons !== undefined && (
                        <span>Сравнения: {result.comparisons}</span>
                      )}
                      {result.swaps !== undefined && (
                        <span>Перестановки: {result.swaps}</span>
                      )}
                      {result.operations !== undefined && (
                        <span>Операции: {result.operations}</span>
                      )}
                    </div>
                  </div>
                  <Badge variant={index === 0 ? "default" : "secondary"}>
                    {index === 0 ? "Лучший" : `+${((result.time / results[0].time - 1) * 100).toFixed(1)}%`}
                  </Badge>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}
