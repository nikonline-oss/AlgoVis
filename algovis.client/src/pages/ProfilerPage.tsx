import React, { useState } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Slider } from '../components/ui/slider';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line, Legend, PieChart, Pie, Cell } from 'recharts';
import { useApp } from '../contexts/AppContext';
import { Send, Play, Square, Info, BookOpen, Zap, Cpu, Clock } from 'lucide-react';

interface AlgorithmResult {
  name: string;
  time: number;
  comparisons?: number;
  swaps?: number;
  operations?: number;
  memory?: number;
  complexity: {
    time: string;
    space: string;
  };
}

interface ProfilerPageProps {
  onNavigate: (page: string) => void;
}

// Mock Backend Service
class MockBackendService {
  private async delay(ms: number) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  async profileAlgorithm(algorithm: string, data: any, config: any) {
    await this.delay(200 + Math.random() * 300);

    const size = data?.length || config?.dataSize || 1000;

    const performanceMap: Record<string, {
      time: number;
      operations: number;
      comparisons: number;
      swaps: number;
      complexity: { time: string; space: string };
    }> = {
      'bubbleSort': {
        time: size * size * 0.001,
        operations: (size * (size - 1)) / 2,
        comparisons: (size * (size - 1)) / 2,
        swaps: Math.floor((size * (size - 1)) / 4),
        complexity: { time: 'O(n²)', space: 'O(1)' }
      },
      'quickSort': {
        time: size * Math.log2(size) * 0.001,
        operations: size * Math.log2(size),
        comparisons: Math.floor(size * Math.log2(size) * 0.7),
        swaps: Math.floor(size * Math.log2(size) * 0.3),
        complexity: { time: 'O(n log n)', space: 'O(log n)' }
      },
      'mergeSort': {
        time: size * Math.log2(size) * 0.0015,
        operations: size * Math.log2(size) * 1.2,
        comparisons: Math.floor(size * Math.log2(size) * 0.8),
        swaps: Math.floor(size * Math.log2(size) * 0.4),
        complexity: { time: 'O(n log n)', space: 'O(n)' }
      },
      'insertionSort': {
        time: size * size * 0.0005,
        operations: (size * (size - 1)) / 2,
        comparisons: (size * (size - 1)) / 2,
        swaps: Math.floor((size * (size - 1)) / 3),
        complexity: { time: 'O(n²)', space: 'O(1)' }
      },
      'bstInsert': {
        time: size * Math.log2(size) * 0.001,
        operations: size * Math.log2(size),
        comparisons: Math.floor(size * Math.log2(size) * 0.8),
        swaps: 0,
        complexity: { time: 'O(log n)', space: 'O(n)' }
      },
      'bstSearch': {
        time: size * Math.log2(size) * 0.0005,
        operations: size * Math.log2(size),
        comparisons: Math.floor(size * Math.log2(size) * 0.8),
        swaps: 0,
        complexity: { time: 'O(log n)', space: 'O(1)' }
      },
      'bstTraversal': {
        time: size * 0.001,
        operations: size,
        comparisons: 0,
        swaps: 0,
        complexity: { time: 'O(n)', space: 'O(n)' }
      },
      'bfs': {
        time: size * 0.002,
        operations: size * 2,
        comparisons: size,
        swaps: 0,
        complexity: { time: 'O(V + E)', space: 'O(V)' }
      },
      'dfs': {
        time: size * 0.002,
        operations: size * 2,
        comparisons: size,
        swaps: 0,
        complexity: { time: 'O(V + E)', space: 'O(V)' }
      },
      'dijkstra': {
        time: size * size * 0.005,
        operations: size * size,
        comparisons: size * size,
        swaps: 0,
        complexity: { time: 'O(V²)', space: 'O(V)' }
      }
    };

    const perf = performanceMap[algorithm] || {
      time: size * 0.1,
      operations: size,
      comparisons: Math.floor(size * 0.6),
      swaps: Math.floor(size * 0.4),
      complexity: { time: 'O(n)', space: 'O(1)' }
    };

    const randomizedTime = perf.time * (0.9 + Math.random() * 0.2);
    const randomizedOperations = Math.floor(perf.operations * (0.8 + Math.random() * 0.4));

    return {
      success: true,
      data: {
        name: algorithm,
        time: randomizedTime,
        operations: randomizedOperations,
        comparisons: Math.floor(randomizedOperations * 0.6),
        swaps: Math.floor(randomizedOperations * 0.4),
        memory: size * 8 * (1 + Math.random() * 0.2),
        complexity: perf.complexity
      },
      timestamp: Date.now()
    };
  }
}

const mockBackend = new MockBackendService();

// Теоретическая информация об алгоритмах
const algorithmTheory = {
  array: {
    bubbleSort: {
      description: "Простой алгоритм сортировки, который многократно проходит по массиву, сравнивая соседние элементы и меняя их местами, если они находятся в неправильном порядке.",
      steps: [
        "Сравниваем первый и второй элемент",
        "Если первый больше второго - меняем местами",
        "Переходим к следующей паре",
        "Повторяем до полной сортировки"
      ],
      bestCase: "O(n) - когда массив уже отсортирован",
      worstCase: "O(n²) - когда массив отсортирован в обратном порядке",
      usage: "Образовательные цели, маленькие массивы"
    },
    quickSort: {
      description: "Эффективный алгоритм 'разделяй и властвуй'. Выбирает опорный элемент и разделяет массив на две части: элементы меньше опорного и больше опорного.",
      steps: [
        "Выбираем опорный элемент",
        "Разделяем массив на две части",
        "Рекурсивно сортируем каждую часть",
        "Объединяем результаты"
      ],
      bestCase: "O(n log n) - сбалансированное разделение",
      worstCase: "O(n²) - неудачный выбор опорного элемента",
      usage: "Большие массивы, общее применение"
    },
    mergeSort: {
      description: "Стабильный алгоритм сортировки, который разделяет массив на меньшие части, сортирует их, а затем объединяет.",
      steps: [
        "Разделяем массив пополам",
        "Рекурсивно сортируем каждую половину",
        "Объединяем отсортированные половины"
      ],
      bestCase: "O(n log n)",
      worstCase: "O(n log n)",
      usage: "Большие наборы данных, внешняя сортировка"
    },
    insertionSort: {
      description: "Строит отсортированный массив по одному элементу за раз, вставляя каждый новый элемент в правильную позицию.",
      steps: [
        "Начинаем со второго элемента",
        "Сравниваем с элементами в отсортированной части",
        "Вставляем в правильную позицию",
        "Повторяем для всех элементов"
      ],
      bestCase: "O(n) - почти отсортированный массив",
      worstCase: "O(n²) - обратно отсортированный массив",
      usage: "Маленькие массивы, почти отсортированные данные"
    }
  },
  tree: {
    bstInsert: {
      description: "Вставка элемента в бинарное дерево поиска с сохранением свойств BST.",
      steps: [
        "Начинаем с корня",
        "Сравниваем значение с текущим узлом",
        "Если меньше - идем влево, если больше - вправо",
        "Вставляем в найденную пустую позицию"
      ]
    },
    bstSearch: {
      description: "Поиск элемента в бинарном дереве поиска.",
      steps: [
        "Начинаем с корня",
        "Сравниваем искомое значение с текущим узлом",
        "Если равно - нашли",
        "Если меньше - идем влево, если больше - вправо"
      ]
    }
  }
};

// Цвета для диаграмм
const COLORS = ['#0088FE', '#00C49F', '#9a19e5a1', '#FF8042', '#8884D8'];

// Вспомогательная функция для сокращения названий
const getShortName = (name: string) => {
  const shortNames: Record<string, string> = {
    'Bubble Sort': 'Bubble',
    'Quick Sort': 'Quick',
    'Merge Sort': 'Merge',
    'Insertion Sort': 'Insertion',
    'BST Insertion': 'BST Ins',
    'BST Search': 'BST Srch',
    'BST Traversal': 'BST Trav',
    'BFS': 'BFS',
    'DFS': 'DFS',
    'Dijkstra': 'Dijkstra'
  };
  return shortNames[name] || name.substring(0, 8);
};

export function ProfilerPage({ onNavigate }: ProfilerPageProps) {
  const { translations, setSharedData } = useApp();
  const [dataStructure, setDataStructure] = useState<'array' | 'tree' | 'graph'>('array');
  const [dataSize, setDataSize] = useState(1000);
  const [results, setResults] = useState<AlgorithmResult[]>([]);
  const [isRunning, setIsRunning] = useState(false);
  const [progress, setProgress] = useState(0);
  const [selectedAlgorithm, setSelectedAlgorithm] = useState<string | null>(null);

  const runComparison = async () => {
    setIsRunning(true);
    setResults([]);
    setProgress(0);

    if (dataStructure === 'array') {
      const testArray = Array.from({ length: dataSize }, () =>
        Math.floor(Math.random() * 1000) + 1
      );

      const algorithms = [
        { name: translations['algorithm.bubblesort'], key: 'bubbleSort' },
        { name: translations['algorithm.quicksort'], key: 'quickSort' },
        { name: translations['algorithm.mergesort'], key: 'mergeSort' },
        { name: translations['algorithm.insertionsort'], key: 'insertionSort' },
      ];

      const newResults: AlgorithmResult[] = [];

      for (let i = 0; i < algorithms.length; i++) {
        const algorithm = algorithms[i];
        setProgress(((i) / algorithms.length) * 100);

        try {
          const result = await mockBackend.profileAlgorithm(algorithm.key, testArray, {
            dataSize,
            dataStructure
          });

          if (result.success) {
            newResults.push({
              name: algorithm.name,
              ...result.data
            });
            setResults([...newResults]);
          }
        } catch (error) {
          console.error(`Error profiling ${algorithm.name}:`, error);
        }

        await new Promise(resolve => setTimeout(resolve, 300));
      }
    } else if (dataStructure === 'tree') {
      const algorithms = [
        { name: 'BST Insertion', key: 'bstInsert' },
        { name: 'BST Search', key: 'bstSearch' },
        { name: 'BST Traversal', key: 'bstTraversal' },
      ];

      const newResults: AlgorithmResult[] = [];

      for (let i = 0; i < algorithms.length; i++) {
        const algorithm = algorithms[i];
        setProgress(((i) / algorithms.length) * 100);

        try {
          const result = await mockBackend.profileAlgorithm(algorithm.key, [], {
            dataSize,
            dataStructure
          });

          if (result.success) {
            newResults.push({
              name: algorithm.name,
              ...result.data
            });
            setResults([...newResults]);
          }
        } catch (error) {
          console.error(`Error profiling ${algorithm.name}:`, error);
        }

        await new Promise(resolve => setTimeout(resolve, 300));
      }
    } else if (dataStructure === 'graph') {
      const algorithms = [
        { name: 'BFS', key: 'bfs' },
        { name: 'DFS', key: 'dfs' },
        { name: 'Dijkstra', key: 'dijkstra' },
      ];

      const newResults: AlgorithmResult[] = [];

      for (let i = 0; i < algorithms.length; i++) {
        const algorithm = algorithms[i];
        setProgress(((i) / algorithms.length) * 100);

        try {
          const result = await mockBackend.profileAlgorithm(algorithm.key, [], {
            dataSize,
            dataStructure
          });

          if (result.success) {
            newResults.push({
              name: algorithm.name,
              ...result.data
            });
            setResults([...newResults]);
          }
        } catch (error) {
          console.error(`Error profiling ${algorithm.name}:`, error);
        }

        await new Promise(resolve => setTimeout(resolve, 300));
      }
    }

    setProgress(100);
    setIsRunning(false);
  };

  const stopComparison = () => {
    setIsRunning(false);
    setProgress(0);
  };



  const chartData = results.map(result => ({
    name: result.name,
    time: parseFloat(result.time.toFixed(2)),
    comparisons: result.comparisons || 0,
    swaps: result.swaps || 0,
    operations: result.operations || 0,
    memory: result.memory ? parseFloat((result.memory / 1024).toFixed(2)) : 0,
  }));

  const efficiencyData = results.map(result => ({
    name: result.name,
    efficiency: result.operations ? (dataSize / result.operations * 1000) : 0,
    timePerOp: result.operations ? (result.time / result.operations) : 0
  }));

  const complexityData = dataStructure === 'array' ? [
    { size: 100, bubble: 10000, quick: 665, merge: 665, insertion: 10000 },
    { size: 500, bubble: 250000, quick: 4483, merge: 4483, insertion: 250000 },
    { size: 1000, bubble: 1000000, quick: 9966, merge: 9966, insertion: 1000000 },
    { size: 2000, bubble: 4000000, quick: 21932, merge: 21932, insertion: 4000000 },
    { size: 5000, bubble: 25000000, quick: 61497, merge: 61497, insertion: 25000000 },
  ] : dataStructure === 'tree' ? [
    { size: 100, bstInsert: 665, bstSearch: 665, bstTraversal: 665 },
    { size: 500, bstInsert: 4483, bstSearch: 4483, bstTraversal: 4483 },
    { size: 1000, bstInsert: 9966, bstSearch: 9966, bstTraversal: 9966 },
    { size: 2000, bstInsert: 21932, bstSearch: 21932, bstTraversal: 21932 },
    { size: 5000, bstInsert: 61497, bstSearch: 61497, bstTraversal: 61497 },
  ] : [
    { size: 10, bfs: 100, dfs: 100, dijkstra: 1000 },
    { size: 50, bfs: 2500, dfs: 2500, dijkstra: 125000 },
    { size: 100, bfs: 10000, dfs: 10000, dijkstra: 1000000 },
    { size: 200, bfs: 40000, dfs: 40000, dijkstra: 8000000 },
    { size: 500, bfs: 250000, dfs: 250000, dijkstra: 125000000 },
  ];

  const getMaxDataSize = () => {
    switch (dataStructure) {
      case 'array': return 10000;
      case 'tree': return 5000;
      case 'graph': return 1000;
      default: return 5000;
    }
  };

  const getDataSizeStep = () => {
    switch (dataStructure) {
      case 'array': return 100;
      case 'tree': return 50;
      case 'graph': return 10;
      default: return 100;
    }
  };

  const getSelectedAlgorithmTheory = () => {
    if (!selectedAlgorithm) return null;
    const key = Object.keys(algorithmTheory[dataStructure] || {}).find(k =>
      algorithmTheory[dataStructure][k] && selectedAlgorithm.toLowerCase().includes(k.toLowerCase())
    );
    return key ? algorithmTheory[dataStructure][key] : null;
  };

  const theory = getSelectedAlgorithmTheory();

  return (
    <div className="space-y-6">
      {/* Теоретическая информация */}
      <Card className="bg-blue-50 border-blue-200">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <BookOpen className="h-5 w-5" />
            Теоретическая справка
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid md:grid-cols-3 gap-4 text-sm">
            <div className="space-y-2">
              <div className="flex items-center gap-2 font-semibold">
                <Zap className="h-4 w-4 text-orange-500" />
                Временная сложность
              </div>
              <div className="space-y-1 text-xs">
                <div>O(1) - константное время</div>
                <div>O(log n) - логарифмическое</div>
                <div>O(n) - линейное</div>
                <div>O(n log n) - линейно-логарифмическое</div>
                <div>O(n²) - квадратичное</div>
              </div>
            </div>
            <div className="space-y-2">
              <div className="flex items-center gap-2 font-semibold">
                <Cpu className="h-4 w-4 text-green-500" />
                Пространственная сложность
              </div>
              <div className="space-y-1 text-xs">
                <div>O(1) - константная память</div>
                <div>O(log n) - логарифмическая</div>
                <div>O(n) - линейная</div>
                <div>O(n²) - квадратичная</div>
              </div>
            </div>
            <div className="space-y-2">
              <div className="flex items-center gap-2 font-semibold">
                <Clock className="h-4 w-4 text-purple-500" />
                Практическое применение
              </div>
              <div className="space-y-1 text-xs">
                <div>Маленькие данные: O(n²) алгоритмы</div>
                <div>Большие данные: O(n log n) алгоритмы</div>
                <div>Поиск: O(log n) алгоритмы</div>
                <div>Графы: O(V + E) алгоритмы</div>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Основные настройки */}
      <Card>
        <CardHeader>
          <div className="flex justify-between items-center">
            <CardTitle>{translations['profiler.title']}</CardTitle>
            <Badge variant="outline" className="bg-amber-100 text-amber-800 border-amber-300">
              🔧 Mock Backend
            </Badge>
          </div>
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
            <label className="text-sm font-medium">
              {translations['data.size']}: {dataSize}
            </label>
            <Slider
              value={[dataSize]}
              onValueChange={(value) => setDataSize(value[0])}
              max={getMaxDataSize()}
              min={dataStructure === 'graph' ? 10 : 100}
              step={getDataSizeStep()}
              className="w-full"
            />
            <div className="flex justify-between text-xs text-muted-foreground">
              <span>{dataStructure === 'graph' ? 10 : 100}</span>
              <span>{getMaxDataSize()}</span>
            </div>
          </div>

          {isRunning && (
            <div className="space-y-2">
              <div className="flex justify-between text-sm">
                <span>Выполнение тестов...</span>
                <span>{progress.toFixed(0)}%</span>
              </div>
              <div className="w-full bg-gray-200 rounded-full h-2">
                <div
                  className="bg-blue-600 h-2 rounded-full transition-all duration-300"
                  style={{ width: `${progress}%` }}
                />
              </div>
            </div>
          )}

          <div className="flex space-x-2">
            {!isRunning ? (
              <Button
                onClick={runComparison}
                disabled={isRunning}
                className="bg-primary hover:bg-primary/90 text-primary-foreground flex-1"
              >
                <Play className="h-4 w-4 mr-2" />
                {translations['profiler.run']}
              </Button>
            ) : (
              <Button
                onClick={stopComparison}
                variant="destructive"
                className="flex-1"
              >
                <Square className="h-4 w-4 mr-2" />
                Остановить
              </Button>
            )}

          </div>
        </CardContent>
      </Card>

      {results.length > 0 && (
        <>
          {/* Основные метрики */}
          <div className="grid lg:grid-cols-2 gap-6">
            {/* Время выполнения (Horizontal BarChart) */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Clock className="h-5 w-5" />
                  Время выполнения (мс)
                </CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={chartData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis
                      dataKey="name"
                      tickFormatter={getShortName}
                      interval={0}
                      angle={-45}
                      textAnchor="end"
                      height={60}
                    />
                    <YAxis />
                    <Tooltip
                      formatter={(value) => [`${value} мс`, 'Время']}
                      labelFormatter={(label) => `Алгоритм: ${label}`}
                    />
                    <Bar
                      dataKey="time"
                      fill="#3b82f6"
                      name="Время (мс)"
                      onClick={(data) => setSelectedAlgorithm(data.name)}
                      style={{ cursor: 'pointer' }}
                    />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            {/* Эффективность (BarChart) */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Cpu className="h-5 w-5" />
                  Эффективность (операций/сек)
                </CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart data={efficiencyData}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis
                      dataKey="name"
                      tickFormatter={getShortName}
                      interval={0}
                      angle={-45}
                      textAnchor="end"
                      height={60}
                    />
                    <YAxis />
                    <Tooltip
                      formatter={(value) => [`${Number(value).toFixed(2)}`, 'Операций/сек']}
                    />
                    <Bar
                      dataKey="efficiency"
                      fill="#10b981"
                      name="Эффективность"
                      onClick={(data) => setSelectedAlgorithm(data.name)}
                      style={{ cursor: 'pointer' }}
                    />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>
          </div>

          {/* Детальная статистика */}
          <div className="grid md:grid-cols-3 gap-6">
            {/* Распределение операций (PieChart) */}
            <Card className="min-h-[300px]">
              <CardHeader className="pb-2">
                <CardTitle className="text-sm">Распределение операций</CardTitle>
              </CardHeader>
              <CardContent className="pt-0">
                <ResponsiveContainer width="100%" height={200}>
                  <PieChart>
                    <Pie
                      data={results.map(result => ({
                        name: getShortName(result.name),
                        value: result.operations || 0
                      }))}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={({ percent }) => `${(percent * 100).toFixed(0)}%`}
                      outerRadius={70}
                      innerRadius={40}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {results.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip
                      formatter={(value) => [value.toLocaleString(), 'Операции']}
                    />
                  </PieChart>
                </ResponsiveContainer>
                <div className="mt-2 space-y-1 max-h-20 overflow-y-auto">
                  {results.map((result, index) => (
                    <div key={index} className="flex items-center justify-between text-xs">
                      <div className="flex items-center gap-1">
                        <div
                          className="w-2 h-2 rounded-full"
                          style={{ backgroundColor: COLORS[index % COLORS.length] }}
                        />
                        <span className="truncate">{getShortName(result.name)}</span>
                      </div>
                      <span className="text-muted-foreground">
                        {((result.operations || 0) / results.reduce((sum, r) => sum + (r.operations || 0), 0) * 100).toFixed(1)}%
                      </span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            {/* Сравнения vs Перестановки (Vertical BarChart) */}
            <Card className="min-h-[300px] overflow-visible">
              <CardHeader className="pb-2">
                <CardTitle className="text-sm">Сравнения vs Перестановки</CardTitle>
              </CardHeader>
              <CardContent className="p-0 pr-1 pl-0">
                <div className="relative w-full" style={{ height: '200px' }}>
                  <ResponsiveContainer width="98%" height="100%">
                    <BarChart
                      data={chartData}
                      layout="vertical"
                      margin={{ left: 35, right: 15, top: 10, bottom: 10 }}
                    >
                      <CartesianGrid strokeDasharray="3 3" />
                      <XAxis type="number" tick={{ fontSize: 11 }} />
                      <YAxis
                        dataKey="name"
                        type="category"
                        tickFormatter={getShortName}
                        width={40}
                        tick={{ fontSize: 11 }}
                      />
                      <Tooltip
                        formatter={(value) => [value.toLocaleString(), 'Количество']}
                        contentStyle={{ fontSize: '12px', padding: '8px' }}
                      />
                      <Bar
                        dataKey="comparisons"
                        fill="#4F46E5"
                        name="Сравнения"
                        radius={[0, 2, 2, 0]}
                      />
                      <Bar
                        dataKey="swaps"
                        fill="#10B981"
                        name="Перестановки"
                        radius={[0, 2, 2, 0]}
                      />
                    </BarChart>
                  </ResponsiveContainer>
                </div>
                <div className="mt-3 flex justify-center gap-6 text-xs px-2">
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full bg-[#4F46E5]" />
                    <span>Сравнения</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 rounded-full bg-[#10B981]" />
                    <span>Перестановки</span>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Использование памяти (Custom Bars) */}
            <Card className="min-h-[300px]">
              <CardHeader className="pb-2">
                <CardTitle className="text-sm">Использование памяти</CardTitle>
              </CardHeader>
              <CardContent className="pt-0">
                <div className="space-y-3 h-[200px] flex flex-col justify-center">
                  {results.map((result, index) => (
                    <div key={index} className="space-y-1">
                      <div className="flex justify-between items-center text-xs">
                        <span className="truncate font-medium">{getShortName(result.name)}</span>
                        <span className="text-muted-foreground whitespace-nowrap">
                          {((result.memory || 0) / 1024).toFixed(0)} KB
                        </span>
                      </div>
                      <div className="w-full bg-gray-200 rounded-full h-2">
                        <div
                          className="bg-purple-600 h-2 rounded-full transition-all duration-300"
                          style={{
                            width: `${Math.min(100, ((result.memory || 0) / 1024) / 50)}%`
                          }}
                        />
                      </div>
                    </div>
                  ))}
                </div>
                <div className="mt-2 text-xs text-center text-muted-foreground">
                  Относительное использование памяти
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Теория выбранного алгоритма */}
          {selectedAlgorithm && theory && (
            <Card className="bg-gradient-to-r from-blue-50 to-purple-50 border-blue-200">
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Info className="h-5 w-5 text-blue-600" />
                  Теория: {selectedAlgorithm}
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <p className="text-sm">{theory.description}</p>

                  {theory.steps && (
                    <div>
                      <h4 className="font-semibold mb-2">Шаги выполнения:</h4>
                      <ol className="list-decimal list-inside space-y-1 text-sm">
                        {theory.steps.map((step, index) => (
                          <li key={index}>{step}</li>
                        ))}
                      </ol>
                    </div>
                  )}

                  <div className="grid md:grid-cols-2 gap-4 text-sm">
                    {theory.bestCase && (
                      <div>
                        <span className="font-semibold">Лучший случай: </span>
                        <Badge variant="outline" className="bg-green-100">
                          {theory.bestCase}
                        </Badge>
                      </div>
                    )}
                    {theory.worstCase && (
                      <div>
                        <span className="font-semibold">Худший случай: </span>
                        <Badge variant="outline" className="bg-red-100">
                          {theory.worstCase}
                        </Badge>
                      </div>
                    )}
                  </div>

                  {theory.usage && (
                    <div>
                      <span className="font-semibold">Применение: </span>
                      <span className="text-sm">{theory.usage}</span>
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
          )}

          <Card className="w-full overflow-visible">
            <CardHeader className="pb-2">
              <CardTitle className="text-lg">Сравнение теоретической сложности</CardTitle>
            </CardHeader>
            <CardContent className="p-0 pr-2 pl-1">
              <div className="relative w-full" style={{ height: '550px' }}>
                <ResponsiveContainer width="98%" height="100%">
                  <LineChart
                    data={complexityData}
                    margin={{ top: 20, right: 25, left: 15, bottom: 25 }}
                  >
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis
                      dataKey="size"
                      label={{
                        value: 'Размер данных',
                        position: 'insideBottom',
                        offset: -2,
                        style: { fontSize: '12px' }
                      }}
                      tick={{ fontSize: 11 }}
                    />
                    <YAxis
                      label={{
                        value: 'Операции',
                        angle: -90,
                        position: 'insideLeft',
                        offset: 10,
                        style: { fontSize: '12px' }
                      }}
                      scale="log"
                      domain={[1, 100000000]}
                      tick={{ fontSize: 11 }}
                    />
                    <Tooltip
                      formatter={(value) => [value.toLocaleString(), 'Операции']}
                      labelFormatter={(label) => `Размер: ${label}`}
                    />
                    <Legend />
                    {dataStructure === 'array' && (
                      <>
                        <Line type="monotone" dataKey="bubble" stroke="#FF6B6B" name="Bubble Sort O(n²)" strokeWidth={2} dot={false} />
                        <Line type="monotone" dataKey="quick" stroke="#4ECDC4" name="Quick Sort O(n log n)" strokeWidth={2} dot={false} />
                        <Line type="monotone" dataKey="merge" stroke="#45B7D1" name="Merge Sort O(n log n)" strokeWidth={2} dot={false} />
                        <Line type="monotone" dataKey="insertion" stroke="#FFA07A" name="Insertion Sort O(n²)" strokeWidth={2} dot={false} />
                      </>
                    )}
                    {dataStructure === 'tree' && (
                      <>
                        <Line type="monotone" dataKey="bstInsert" stroke="#4ECDC4" name="BST Insert O(log n)" strokeWidth={2} dot={false} />
                        <Line type="monotone" dataKey="bstSearch" stroke="#45B7D1" name="BST Search O(log n)" strokeWidth={2} dot={false} />
                        <Line type="monotone" dataKey="bstTraversal" stroke="#FFA07A" name="BST Traversal O(n)" strokeWidth={2} dot={false} />
                      </>
                    )}
                    {dataStructure === 'graph' && (
                      <>
                        <Line type="monotone" dataKey="bfs" stroke="#4ECDC4" name="BFS O(V + E)" strokeWidth={2} dot={false} />
                        <Line type="monotone" dataKey="dfs" stroke="#45B7D1" name="DFS O(V + E)" strokeWidth={2} dot={false} />
                        <Line type="monotone" dataKey="dijkstra" stroke="#FF6B6B" name="Dijkstra O(V²)" strokeWidth={2} dot={false} />
                      </>
                    )}
                  </LineChart>
                </ResponsiveContainer>
              </div>
            </CardContent>
          </Card>


          {/* Детальные результаты */}
          <Card>
            <CardHeader>
              <CardTitle>Детальные результаты тестирования</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {results.map((result, index) => (
                  <div
                    key={index}
                    className={`flex items-center justify-between p-4 border rounded-lg bg-card cursor-pointer transition-all hover:shadow-md ${selectedAlgorithm === result.name ? 'ring-2 ring-blue-500' : ''
                      }`}
                    onClick={() => setSelectedAlgorithm(result.name)}
                  >
                    <div className="flex-1">
                      <div className="flex items-center gap-3 mb-2">
                        <h4 className="font-medium text-lg">{result.name}</h4>
                        <div className="flex gap-2">
                          <Badge variant="outline" className="bg-blue-50">
                            Время: {result.complexity.time}
                          </Badge>
                          <Badge variant="outline" className="bg-green-50">
                            Память: {result.complexity.space}
                          </Badge>
                        </div>
                      </div>
                      <div className="grid grid-cols-2 md:grid-cols-5 gap-4 text-sm text-muted-foreground">
                        <div>
                          <span className="font-medium">Время: </span>
                          {result.time.toFixed(2)} мс
                        </div>
                        {result.comparisons !== undefined && (
                          <div>
                            <span className="font-medium">Сравнения: </span>
                            {result.comparisons.toLocaleString()}
                          </div>
                        )}
                        {result.swaps !== undefined && (
                          <div>
                            <span className="font-medium">Перестановки: </span>
                            {result.swaps.toLocaleString()}
                          </div>
                        )}
                        {result.operations !== undefined && (
                          <div>
                            <span className="font-medium">Операции: </span>
                            {result.operations.toLocaleString()}
                          </div>
                        )}
                        {result.memory !== undefined && (
                          <div>
                            <span className="font-medium">Память: </span>
                            {(result.memory / 1024).toFixed(2)} KB
                          </div>
                        )}
                      </div>
                    </div>
                    <Badge
                      variant={index === 0 ? "default" : "secondary"}
                      className="ml-4 whitespace-nowrap"
                    >
                      {index === 0 ? "🏆 Лучший" : `+${((result.time / results[0].time - 1) * 100).toFixed(1)}%`}
                    </Badge>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </>
      )}

      {results.length === 0 && !isRunning && (
        <Card>
          <CardContent className="p-12 text-center">
            <div className="text-muted-foreground space-y-4">
              <BookOpen className="h-12 w-12 mx-auto text-gray-400" />
              <p className="text-lg font-medium">Запустите сравнение алгоритмов</p>
              <p className="text-sm max-w-md mx-auto">
                Выберите структуру данных, установите размер данных и нажмите "Запустить тест"
                для анализа производительности различных алгоритмов.
              </p>
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}