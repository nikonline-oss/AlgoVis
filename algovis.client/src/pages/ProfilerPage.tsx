import React, { useState, useEffect } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Slider } from '../components/ui/slider';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, ResponsiveContainer, PieChart, Pie, Cell } from 'recharts';
import { useApp } from '../contexts/AppContext';
import { Play, Square, Info, BookOpen, Zap, Cpu, Clock, Settings, AlertCircle, Sparkles, Calculator, Scale, Award } from 'lucide-react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';

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
  efficiency?: number;
  timePerOp?: number;
  score?: number; // Итоговый балл для сравнения
}

interface ProfilerPageProps {
  onNavigate: (page: string) => void;
}

// Реализация реальных алгоритмов для профилирования
// Реализация реальных алгоритмов для профилирования
// Реализация реальных алгоритмов для профилирования
class AlgorithmProfiler {
  private generateArray(size: number, maxValue = 1000): number[] {
    return Array.from({ length: size }, () => Math.floor(Math.random() * maxValue) + 1);
  }

  private async delay(ms: number) {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  // Реализация Bubble Sort с подсчетом операций
  bubbleSort(arr: number[]): { sorted: number[]; comparisons: number; swaps: number; time: number } {
    const start = performance.now();
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;

    for (let i = 0; i < array.length; i++) {
      for (let j = 0; j < array.length - i - 1; j++) {
        comparisons++;
        if (array[j] > array[j + 1]) {
          swaps++;
          [array[j], array[j + 1]] = [array[j + 1], array[j]];
        }
      }
    }

    const time = performance.now() - start;
    return { sorted: array, comparisons, swaps, time };
  }

  // Реализация Quick Sort с подсчетом операций
  quickSort(arr: number[]): { sorted: number[]; comparisons: number; swaps: number; time: number } {
    const start = performance.now();
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;

    const sort = (arr: number[], low: number, high: number) => {
      if (low < high) {
        const pi = partition(arr, low, high);
        sort(arr, low, pi - 1);
        sort(arr, pi + 1, high);
      }
    };

    const partition = (arr: number[], low: number, high: number): number => {
      const pivot = arr[high];
      let i = low - 1;

      for (let j = low; j < high; j++) {
        comparisons++;
        if (arr[j] < pivot) {
          i++;
          swaps++;
          [arr[i], arr[j]] = [arr[j], arr[i]];
        }
      }

      swaps++;
      [arr[i + 1], arr[high]] = [arr[high], arr[i + 1]];
      return i + 1;
    };

    sort(array, 0, array.length - 1);
    const time = performance.now() - start;
    return { sorted: array, comparisons, swaps, time };
  }

  // Реализация Merge Sort с подсчетом операций
  mergeSort(arr: number[]): { sorted: number[]; comparisons: number; swaps: number; time: number } {
    const start = performance.now();
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;

    const merge = (left: number[], right: number[]): number[] => {
      const result: number[] = [];
      let leftIndex = 0;
      let rightIndex = 0;

      while (leftIndex < left.length && rightIndex < right.length) {
        comparisons++;
        if (left[leftIndex] < right[rightIndex]) {
          result.push(left[leftIndex]);
          leftIndex++;
        } else {
          result.push(right[rightIndex]);
          rightIndex++;
        }
      }

      return result.concat(left.slice(leftIndex)).concat(right.slice(rightIndex));
    };

    const sort = (arr: number[]): number[] => {
      if (arr.length <= 1) return arr;

      const middle = Math.floor(arr.length / 2);
      const left = arr.slice(0, middle);
      const right = arr.slice(middle);

      return merge(sort(left), sort(right));
    };

    const sorted = sort(array);
    const time = performance.now() - start;
    return { sorted, comparisons, swaps, time };
  }

  // Реализация Insertion Sort с подсчетом операций
  insertionSort(arr: number[]): { sorted: number[]; comparisons: number; swaps: number; time: number } {
    const start = performance.now();
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;

    for (let i = 1; i < array.length; i++) {
      let key = array[i];
      let j = i - 1;

      while (j >= 0 && array[j] > key) {
        comparisons++;
        swaps++;
        array[j + 1] = array[j];
        j--;
      }
      comparisons++;
      array[j + 1] = key;
    }

    const time = performance.now() - start;
    return { sorted: array, comparisons, swaps, time };
  }

  // Симуляция вставки в BST
  bstInsertOperations(size: number): { time: number; operations: number } {
    const start = performance.now();
    let operations = 0;

    // Симуляция вставки элементов в BST
    for (let i = 0; i < size; i++) {
      operations++; // Операция вставки
      // Симуляция поиска места для вставки (в среднем O(log n) сравнений)
      operations += Math.floor(Math.log2(i + 1)) + 1;
    }

    // Реалистичное время для операций
    const simulatedTime = operations * 0.01; // 0.01 мс на операцию
    const time = performance.now() - start + simulatedTime;

    return { time, operations };
  }

  // Симуляция поиска в BST
  bstSearchOperations(size: number): { time: number; operations: number } {
    const start = performance.now();
    let operations = 0;

    // Симуляция поиска элементов в BST
    for (let i = 0; i < size; i++) {
      operations++; // Операция поиска
      // Симуляция обхода дерева (в среднем O(log n) сравнений)
      operations += Math.floor(Math.log2(size)) + 1;
    }

    // Реалистичное время для операций
    const simulatedTime = operations * 0.008; // 0.008 мс на операцию (поиск быстрее вставки)
    const time = performance.now() - start + simulatedTime;

    return { time, operations };
  }

  // Симуляция инордерного обхода BST
  bstInorderTraversal(size: number): { time: number; operations: number } {
    const start = performance.now();
    let operations = 0;

    // Симуляция рекурсивного обхода
    const simulateInorder = (n: number): void => {
      if (n <= 0) return;

      // Обход левого поддерева
      simulateInorder(Math.floor(n / 2));

      // Посещение узла
      operations += 2; // операции сравнения и обработки

      // Обход правого поддерева
      simulateInorder(n - Math.floor(n / 2) - 1);
    };

    simulateInorder(size);

    // Реалистичное время для операций
    const simulatedTime = operations * 0.005; // 0.005 мс на операцию (обход быстрый)
    const time = performance.now() - start + simulatedTime;

    return { time, operations };
  }

  // Симуляция прямого обхода BST
  bstPreorderTraversal(size: number): { time: number; operations: number } {
    const start = performance.now();
    let operations = 0;

    // Симуляция рекурсивного обхода
    const simulatePreorder = (n: number): void => {
      if (n <= 0) return;

      // Посещение узла
      operations += 2; // операции сравнения и обработки

      // Обход левого поддерева
      simulatePreorder(Math.floor(n / 2));

      // Обход правого поддерева
      simulatePreorder(n - Math.floor(n / 2) - 1);
    };

    simulatePreorder(size);

    // Реалистичное время для операций
    const simulatedTime = operations * 0.004; // 0.004 мс на операцию
    const time = performance.now() - start + simulatedTime;

    return { time, operations };
  }

  // Симуляция обратного обхода BST
  bstPostorderTraversal(size: number): { time: number; operations: number } {
    const start = performance.now();
    let operations = 0;

    // Симуляция рекурсивного обхода
    const simulatePostorder = (n: number): void => {
      if (n <= 0) return;

      // Обход левого поддерева
      simulatePostorder(Math.floor(n / 2));

      // Обход правого поддерева
      simulatePostorder(n - Math.floor(n / 2) - 1);

      // Посещение узла
      operations += 2; // операции сравнения и обработки
    };

    simulatePostorder(size);

    // Реалистичное время для операций
    const simulatedTime = operations * 0.005; // 0.005 мс на операцию
    const time = performance.now() - start + simulatedTime;

    return { time, operations };
  }

  // Симуляция обхода в ширину BST
  bstLevelOrderTraversal(size: number): { time: number; operations: number } {
    const start = performance.now();
    let operations = 0;

    // Симуляция обхода в ширину
    // Каждый узел: добавление в очередь, извлечение, обработка
    for (let i = 0; i < size; i++) {
      operations += 3; // операции с очередью и обработка узла
    }

    // Дополнительные операции для поддержки очереди
    operations += Math.floor(size / 2);

    // Реалистичное время для операций
    const simulatedTime = operations * 0.006; // 0.006 мс на операцию (очередь добавляет накладные расходы)
    const time = performance.now() - start + simulatedTime;

    return { time, operations };
  }

  // Симуляция BFS для графов
  bfsOperations(size: number): { time: number; operations: number } {
    const start = performance.now();
    let operations = 0;

    // Симуляция BFS: O(V + E)
    const vertices = size;
    const edges = Math.floor(vertices * 1.5); // Плотность графа 1.5 ребра на вершину

    // Посещение вершин
    operations += vertices * 2; // пометка + обработка

    // Обработка ребер
    operations += edges * 2; // проверка + добавление в очередь

    // Операции с очередью
    operations += vertices * 3; // добавление, извлечение, проверка

    // Реалистичное время для операций
    const simulatedTime = operations * 0.003; // 0.003 мс на операцию (графовые алгоритмы быстрые)
    const time = performance.now() - start + simulatedTime;

    return { time, operations };
  }

  // Симуляция DFS для графов
  dfsOperations(size: number): { time: number; operations: number } {
    const start = performance.now();
    let operations = 0;

    // Симуляция DFS: O(V + E)
    const vertices = size;
    const edges = Math.floor(vertices * 1.5); // Плотность графа 1.5 ребра на вершину

    // Посещение вершин (рекурсивный обход)
    operations += vertices * 3; // пометка + рекурсивный вызов + обработка

    // Обработка ребер
    operations += edges * 2; // проверка соседей + рекурсивный вызов

    // Рекурсивный стек
    operations += vertices * 2;

    // Реалистичное время для операций
    const simulatedTime = operations * 0.002; // 0.002 мс на операцию (DFS обычно быстрее BFS)
    const time = performance.now() - start + simulatedTime;

    return { time, operations };
  }

  // Симуляция алгоритма Дейкстры
  dijkstraOperations(size: number): { time: number; operations: number } {
    const start = performance.now();
    let operations = 0;

    // Симуляция Dijkstra: O(V²) без кучи
    const vertices = size;

    // Инициализация расстояний
    operations += vertices * 2;

    // Основной цикл: для каждой вершины
    for (let i = 0; i < vertices; i++) {
      // Поиск вершины с минимальным расстоянием
      operations += vertices * 2;

      // Обновление расстояний до соседей
      const edgesPerVertex = Math.floor(vertices * 0.3); // средняя степень вершины
      operations += edgesPerVertex * 3;
    }

    // Реалистичное время для операций
    const simulatedTime = operations * 0.001; // 0.001 мс на операцию (Dijkstra сложный, но операции простые)
    const time = performance.now() - start + simulatedTime;

    return { time, operations };
  }

  async profile(algorithm: string, dataSize: number, dataStructure: string): Promise<AlgorithmResult> {
    await this.delay(50); // Минимальная задержка для UI feedback

    if (dataStructure === 'array') {
      const testArray = this.generateArray(dataSize);
      let result;

      switch (algorithm) {
        case 'bubbleSort':
          result = this.bubbleSort(testArray);
          break;
        case 'quickSort':
          result = this.quickSort(testArray);
          break;
        case 'mergeSort':
          result = this.mergeSort(testArray);
          break;
        case 'insertionSort':
          result = this.insertionSort(testArray);
          break;
        default:
          result = { sorted: [], comparisons: 0, swaps: 0, time: 0 };
      }

      const operations = result.comparisons + result.swaps;

      // ПРАВИЛЬНЫЙ РАСЧЕТ ЭФФЕКТИВНОСТИ: операции в секунду
      const efficiency = result.time > 0 ? (operations / result.time) * 1000 : 0;
      const timePerOp = operations > 0 ? result.time / operations : 0;

      return {
        name: algorithm,
        time: result.time,
        comparisons: result.comparisons,
        swaps: result.swaps,
        operations,
        memory: dataSize * 8,
        complexity: this.getComplexity(algorithm, dataStructure),
        efficiency,
        timePerOp
      };
    } else if (dataStructure === 'tree') {
      let result;

      switch (algorithm) {
        case 'bstInsert':
          result = this.bstInsertOperations(dataSize);
          break;
        case 'bstSearch':
          result = this.bstSearchOperations(dataSize);
          break;
        case 'bstInorder':
          result = this.bstInorderTraversal(dataSize);
          break;
        case 'bstPreorder':
          result = this.bstPreorderTraversal(dataSize);
          break;
        case 'bstPostorder':
          result = this.bstPostorderTraversal(dataSize);
          break;
        case 'bstLevelOrder':
          result = this.bstLevelOrderTraversal(dataSize);
          break;
        default:
          result = { time: 0, operations: 0 };
      }

      const complexity = this.getComplexity(algorithm, dataStructure);

      // ПРАВИЛЬНЫЙ РАСЧЕТ ЭФФЕКТИВНОСТИ: операции в секунду
      const efficiency = result.time > 0 ? (result.operations / result.time) * 1000 : 0;
      const timePerOp = result.operations > 0 ? result.time / result.operations : 0;

      return {
        name: algorithm,
        time: result.time,
        operations: result.operations,
        memory: dataSize * 16,
        complexity,
        efficiency,
        timePerOp
      };
    } else if (dataStructure === 'graph') {
      let result;

      switch (algorithm) {
        case 'bfs':
          result = this.bfsOperations(dataSize);
          break;
        case 'dfs':
          result = this.dfsOperations(dataSize);
          break;
        case 'dijkstra':
          result = this.dijkstraOperations(dataSize);
          break;
        default:
          result = { time: 0, operations: 0 };
      }

      const complexity = this.getComplexity(algorithm, dataStructure);

      // ПРАВИЛЬНЫЙ РАСЧЕТ ЭФФЕКТИВНОСТИ: операции в секунду
      const efficiency = result.time > 0 ? (result.operations / result.time) * 1000 : 0;
      const timePerOp = result.operations > 0 ? result.time / result.operations : 0;

      return {
        name: algorithm,
        time: result.time,
        operations: result.operations,
        memory: dataSize * 24,
        complexity,
        efficiency,
        timePerOp
      };
    }

    // Fallback
    return {
      name: algorithm,
      time: dataSize * 0.1,
      operations: dataSize,
      memory: dataSize * 8,
      complexity: { time: 'O(n)', space: 'O(1)' },
      efficiency: 1000,
      timePerOp: 0.1
    };
  }

  private getComplexity(algorithm: string, dataStructure: string): { time: string; space: string } {
    const complexities: Record<string, { time: string; space: string }> = {
      // Сортировка массивов
      'bubbleSort': { time: 'O(n²)', space: 'O(1)' },
      'quickSort': { time: 'O(n log n)', space: 'O(log n)' },
      'mergeSort': { time: 'O(n log n)', space: 'O(n)' },
      'insertionSort': { time: 'O(n²)', space: 'O(1)' },

      // Операции с BST
      'bstInsert': { time: 'O(log n)', space: 'O(n)' },
      'bstSearch': { time: 'O(log n)', space: 'O(1)' },
      'bstInorder': { time: 'O(n)', space: 'O(n)' },
      'bstPreorder': { time: 'O(n)', space: 'O(n)' },
      'bstPostorder': { time: 'O(n)', space: 'O(n)' },
      'bstLevelOrder': { time: 'O(n)', space: 'O(n)' },

      // Алгоритмы на графах
      'bfs': { time: 'O(V + E)', space: 'O(V)' },
      'dfs': { time: 'O(V + E)', space: 'O(V)' },
      'dijkstra': { time: 'O(V²)', space: 'O(V)' }
    };

    return complexities[algorithm] || { time: 'O(n)', space: 'O(1)' };
  }
}

const profiler = new AlgorithmProfiler();



// Теоретическая информация об алгоритмах
const algorithmTheory = {
  array: {
    bubbleSort: {
      name: "Пузырьковая сортировка",
      description: "Простой алгоритм сортировки, который многократно проходит по массиву, сравнивая соседние элементы и меняя их местами, если они находятся в неправильном порядке.",
      steps: [
        "Сравниваем первый и второй элемент",
        "Если первый больше второго - меняем местами",
        "Переходим к следующей паре",
        "Повторяем до полной сортировки"
      ],
      bestCase: "O(n) - когда массив уже отсортирован",
      averageCase: "O(n²)",
      worstCase: "O(n²) - когда массив отсортирован в обратном порядке",
      usage: "Образовательные цели, маленькие массивы",
      stability: "Стабильный",
      whenToUse: "Только для образовательных целей или очень маленьких массивов (до 100 элементов)",
      whenNotToUse: "Большие массивы, реальные приложения где важна производительность"
    },
    quickSort: {
      name: "Быстрая сортировка",
      description: "Эффективный алгоритм 'разделяй и властвуй'. Выбирает опорный элемент и разделяет массив на две части: элементы меньше опорного и больше опорного.",
      steps: [
        "Выбираем опорный элемент",
        "Разделяем массив на две части",
        "Рекурсивно сортируем каждую часть",
        "Объединяем результаты"
      ],
      bestCase: "O(n log n) - сбалансированное разделение",
      averageCase: "O(n log n)",
      worstCase: "O(n²) - неудачный выбор опорного элемента",
      usage: "Большие массивы, общее применение",
      stability: "Нестабильный",
      whenToUse: "Большие массивы общего назначения, когда стабильность не важна",
      whenNotToUse: "Когда требуется стабильная сортировка, при плохом выборе опорного элемента"
    },
    mergeSort: {
      name: "Сортировка слиянием",
      description: "Стабильный алгоритм сортировки, который разделяет массив на меньшие части, сортирует их, а затем объединяет.",
      steps: [
        "Разделяем массив пополам",
        "Рекурсивно сортируем каждую половину",
        "Объединяем отсортированные половины"
      ],
      bestCase: "O(n log n)",
      averageCase: "O(n log n)",
      worstCase: "O(n log n)",
      usage: "Большие наборы данных, внешняя сортировка, когда важна стабильность",
      stability: "Стабильный",
      whenToUse: "Большие массивы, когда важна стабильность, для связанных списков",
      whenNotToUse: "Когда ограничена память, для очень маленьких массивов"
    },
    insertionSort: {
      name: "Сортировка вставками",
      description: "Строит отсортированный массив по одному элементу за раз, вставляя каждый новый элемент в правильную позицию.",
      steps: [
        "Начинаем со второго элемента",
        "Сравниваем с элементами в отсортированной части",
        "Вставляем в правильную позицию",
        "Повторяем для всех элементов"
      ],
      bestCase: "O(n) - почти отсортированный массив",
      averageCase: "O(n²)",
      worstCase: "O(n²) - обратно отсортированный массив",
      usage: "Маленькие массивы, почти отсортированные данные, онлайн-сортировка",
      stability: "Стабильный",
      whenToUse: "Маленькие массивы (до 50 элементов), почти отсортированные данные",
      whenNotToUse: "Большие случайные массивы"
    }
  },
  tree: {
    bstInsert: {
      name: "Вставка в BST",
      description: "Вставка элемента в бинарное дерево поиска с сохранением свойств BST.",
      steps: [
        "Начинаем с корня",
        "Сравниваем значение с текущим узлом",
        "Если меньше - идем влево, если больше - вправо",
        "Вставляем в найденную пустую позицию"
      ],
      bestCase: "O(log n)",
      averageCase: "O(log n)",
      worstCase: "O(n) - вырожденное дерево",
      usage: "Базы данных, файловые системы, реализация множеств",
      whenToUse: "Когда нужен упорядоченный доступ к данным, частые поиски и вставки",
      whenNotToUse: "Когда данные уже отсортированы (приводит к вырожденному дереву)"
    },
    bstSearch: {
      name: "Поиск в BST",
      description: "Поиск элемента в бинарном дереве поиска.",
      steps: [
        "Начинаем с корня",
        "Сравниваем искомое значение с текущим узлом",
        "Если равно - нашли",
        "Если меньше - идем влево, если больше - вправо"
      ],
      bestCase: "O(1) - корень содержит значение",
      averageCase: "O(log n)",
      worstCase: "O(n) - вырожденное дерево",
      usage: "Поиск в базах данных, кэширование",
      whenToUse: "Быстрый поиск в упорядоченных данных",
      whenNotToUse: "Когда дерево может стать вырожденным"
    },
    bstInorder: {
      name: "Инордерный обход",
      description: "Обход бинарного дерева поиска в порядке возрастания: левое поддерево → корень → правое поддерево.",
      steps: [
        "Рекурсивно обходим левое поддерево",
        "Посещаем текущий узел (корень)",
        "Рекурсивно обходим правое поддерево"
      ],
      bestCase: "O(n)",
      averageCase: "O(n)",
      worstCase: "O(n)",
      usage: "Получение отсортированных данных из BST",
      whenToUse: "Когда нужны элементы в отсортированном порядке",
      whenNotToUse: "Когда нужен доступ к корню в начале"
    },
    bstPreorder: {
      name: "Прямой обход",
      description: "Обход бинарного дерева поиска в прямом порядке: корень → левое поддерево → правое поддерево.",
      steps: [
        "Посещаем текущий узел (корень)",
        "Рекурсивно обходим левое поддерево",
        "Рекурсивно обходим правое поддерево"
      ],
      bestCase: "O(n)",
      averageCase: "O(n)",
      worstCase: "O(n)",
      usage: "Копирование структуры дерева, сериализация",
      whenToUse: "Когда нужна копия структуры дерева",
      whenNotToUse: "Когда нужны отсортированные данные"
    },
    bstPostorder: {
      name: "Обратный обход",
      description: "Обход бинарного дерева поиска в обратном порядке: левое поддерево → правое поддерево → корень.",
      steps: [
        "Рекурсивно обходим левое поддерево",
        "Рекурсивно обходим правое поддерево",
        "Посещаем текущий узел (корень)"
      ],
      bestCase: "O(n)",
      averageCase: "O(n)",
      worstCase: "O(n)",
      usage: "Удаление дерева, освобождение памяти",
      whenToUse: "При удалении дерева или постобработке",
      whenNotToUse: "Когда нужен доступ к корню в начале"
    },
    bstLevelOrder: {
      name: "Обход в ширину",
      description: "Обход бинарного дерева поиска по уровням, начиная с корня.",
      steps: [
        "Добавляем корень в очередь",
        "Пока очередь не пуста: извлекаем узел, обрабатываем его",
        "Добавляем левого и правого потомка в очередь"
      ],
      bestCase: "O(n)",
      averageCase: "O(n)",
      worstCase: "O(n)",
      usage: "Поиск кратчайшего пути в дереве",
      whenToUse: "Когда нужен доступ к уровням дерева",
      whenNotToUse: "Когда нужны отсортированные данные"
    }
  },
  graph: {
    bfs: {
      name: "Поиск в ширину (BFS)",
      description: "Поиск в ширину - алгоритм обхода графа, который посещает все вершины на текущей глубине перед переходом на следующий уровень.",
      steps: [
        "Начинаем с исходной вершины",
        "Добавляем ее в очередь",
        "Пока очередь не пуста: извлекаем вершину, обрабатываем ее, добавляем всех непосещенных соседей",
        "Помечаем вершины как посещенные"
      ],
      complexity: "O(V + E)",
      usage: "Поиск кратчайшего пути в невзвешенных графах, проверка связности",
      whenToUse: "Поиск кратчайшего пути в невзвешенных графах, обход уровнями",
      whenNotToUse: "Для взвешенных графов, когда нужна минимальная память"
    },
    dfs: {
      name: "Поиск в глубину (DFS)",
      description: "Поиск в глубину - алгоритм обхода графа, который идет вглубь насколько возможно перед возвратом.",
      steps: [
        "Начинаем с исходной вершины",
        "Помечаем ее как посещенную",
        "Рекурсивно посещаем всех непосещенных соседей",
        "Возвращаемся при отсутствии непосещенных соседей"
      ],
      complexity: "O(V + E)",
      usage: "Топологическая сортировка, поиск компонент связности, решение лабиринтов",
      whenToUse: "Топологическая сортировка, поиск компонент связности, решение лабиринтов",
      whenNotToUse: "Поиск кратчайшего пути в невзвешенных графах"
    },
    dijkstra: {
      name: "Алгоритм Дейкстры",
      description: "Алгоритм Дейкстры находит кратчайшие пути от начальной вершины до всех остальных во взвешенном графе с неотрицательными весами.",
      steps: [
        "Инициализируем расстояния до всех вершин как бесконечность",
        "Устанавливаем расстояние до стартовой вершины как 0",
        "Пока есть непосещенные вершины: выбираем вершину с минимальным расстоянием, обновляем расстояния до ее соседей"
      ],
      complexity: "O(V²) или O(E log V) с кучей",
      usage: "Маршрутизация, навигационные системы, сетевые протоколы",
      whenToUse: "Поиск кратчайшего пути во взвешенных графах с неотрицательными весами",
      whenNotToUse: "Графы с отрицательными весами (использовать алгоритм Беллмана-Форда)"
    }
  }
};

// Теория системы оценки
const scoringSystemTheory = {
  title: "Система оценки алгоритмов",
  description: "Комплексная оценка алгоритмов рассчитывается на основе нескольких метрик производительности с учетом весовых коэффициентов.",
  metrics: [
    {
      name: "Время выполнения",
      weight: "40%",
      description: "Основной показатель производительности. Чем меньше время - тем выше балл."
    },
    {
      name: "Использование памяти",
      weight: "30%",
      description: "Эффективность использования памяти. Важно для больших наборов данных."
    },
    {
      name: "Количество операций",
      weight: "20%",
      description: "Общее количество сравнений, перестановок и других операций."
    },
    {
      name: "Эффективность",
      weight: "10%",
      description: "Операций в секунду - показывает скорость выполнения операций."
    }
  ],
  calculation: "Итоговый балл = (нормализованное_время × 0.4) + (нормализованная_память × 0.3) + (нормализованные_операции × 0.2) + (нормализованная_эффективность × 0.1) × 100",
  details: [
    "Время: Нормализуется как 1 / (время + 1) - чем меньше время, тем выше значение",
    "Память: Нормализуется как 1 / (память + 1) - чем меньше памяти, тем выше значение",
    "Операции: Нормализуется как 1 / (операции + 1) - чем меньше операций, тем выше значение",
    "Эффективность: Нормализуется как эффективность / макс_эффективность - чем больше операций в секунду, тем выше значение"
  ],
  interpretation: [
    { range: "90-100 баллов", meaning: "Отличный алгоритм для данной задачи и размера данных" },
    { range: "75-89 баллов", meaning: "Хороший алгоритм, подходит для большинства случаев" },
    { range: "60-74 балла", meaning: "Средний алгоритм, может потребоваться оптимизация" },
    { range: "0-59 баллов", meaning: "Неэффективный алгоритм для данной задачи" }
  ]
};

// Цвета для диаграмм (адаптированные для темной темы)
const COLORS = ['#3b82f6', '#10b981', '#8b5cf6', '#f59e0b', '#ef4444'];

// Вспомогательная функция для сокращения названий
const getShortName = (name: string) => {
  const shortNames: Record<string, string> = {
    'Пузырьковая сортировка': 'Bubble',
    'Быстрая сортировка': 'Quick',
    'Сортировка слиянием': 'Merge',
    'Сортировка вставками': 'Insertion',
    'Вставка в BST': 'BST Ins',
    'Поиск в BST': 'BST Srch',
    'Инордерный обход': 'Inorder',
    'Прямой обход': 'Preorder',
    'Обратный обход': 'Postorder',
    'Обход в ширину': 'LevelOrder',
    'Поиск в ширину (BFS)': 'BFS',
    'Поиск в глубину (DFS)': 'DFS',
    'Алгоритм Дейкстры': 'Dijkstra'
  };
  return shortNames[name] || name.substring(0, 8);
};

// Компонент для отображения прогресса
const ProgressIndicator: React.FC<{ progress: number; currentAlgorithm: string }> = ({ progress, currentAlgorithm }) => (
  <Card className="bg-card border-border">
    <CardContent className="p-4">
      <div className="space-y-3">
        <div className="flex justify-between text-sm font-medium">
          <span className="text-foreground">Профилирование...</span>
          <span className="text-primary">{progress.toFixed(0)}%</span>
        </div>
        <div className="w-full bg-muted rounded-full h-2.5">
          <div
            className="bg-primary h-2.5 rounded-full transition-all duration-300"
            style={{ width: `${progress}%` }}
          />
        </div>
        {currentAlgorithm && (
          <div className="text-sm text-muted-foreground">
            Текущий алгоритм: <span className="font-medium text-foreground">{currentAlgorithm}</span>
          </div>
        )}
      </div>
    </CardContent>
  </Card>
);

// Функция для расчета итогового балла алгоритма
const calculateAlgorithmScore = (result: AlgorithmResult, results: AlgorithmResult[]): number => {
  // Находим максимальные значения для нормализации
  const maxTime = Math.max(...results.map(r => r.time));
  const maxMemory = Math.max(...results.map(r => r.memory || 0));
  const maxOperations = Math.max(...results.map(r => r.operations || 0));
  const maxEfficiency = Math.max(...results.map(r => r.efficiency || 0));

  // Весовые коэффициенты
  const weights = {
    time: 0.4,
    memory: 0.3,
    operations: 0.2,
    efficiency: 0.1
  };

  // Нормализуем значения
  const normalizedTime = 1 - (result.time / maxTime); // чем меньше время, тем лучше
  const normalizedMemory = maxMemory > 0 ? 1 - ((result.memory || 0) / maxMemory) : 1;
  const normalizedOperations = maxOperations > 0 ? 1 - ((result.operations || 0) / maxOperations) : 1;
  const normalizedEfficiency = maxEfficiency > 0 ? (result.efficiency || 0) / maxEfficiency : 0;

  // Расчет балла с учетом весов (максимум 100 баллов)
  const score = (
    normalizedTime * weights.time +
    normalizedMemory * weights.memory +
    normalizedOperations * weights.operations +
    normalizedEfficiency * weights.efficiency
  ) * 100;

  return Math.min(100, Math.max(0, Math.round(score * 100) / 100));
};

// Компонент теории системы оценки
const ScoringSystemInfo: React.FC = () => (
  <Card className="bg-gradient-to-r from-primary/10 to-purple-500/10 border-border shadow-sm">
    <CardHeader>
      <CardTitle className="flex items-center gap-2 text-foreground">
        <Calculator className="h-5 w-5 text-primary" />
        {scoringSystemTheory.title}
      </CardTitle>
    </CardHeader>
    <CardContent>
      <div className="space-y-4">
        <p className="text-sm text-foreground">{scoringSystemTheory.description}</p>

        <div>
          <h4 className="font-semibold mb-2 text-foreground">Метрики и их вес:</h4>
          <div className="grid md:grid-cols-2 gap-4">
            {scoringSystemTheory.metrics.map((metric, index) => (
              <div key={index} className="bg-card border border-border rounded-lg p-3">
                <div className="flex justify-between items-center mb-2">
                  <span className="font-medium text-foreground">{metric.name}</span>
                  <Badge variant="outline" className="bg-primary/10 text-primary border-primary/50">
                    {metric.weight}
                  </Badge>
                </div>
                <p className="text-xs text-muted-foreground">{metric.description}</p>
              </div>
            ))}
          </div>
        </div>

        <div>
          <h4 className="font-semibold mb-2 text-foreground">Формула расчета:</h4>
          <div className="bg-card border border-border rounded-lg p-3">
            <code className="text-sm text-primary font-mono">
              {scoringSystemTheory.calculation}
            </code>
          </div>
        </div>

        <div>
          <h4 className="font-semibold mb-2 text-foreground">Интерпретация баллов:</h4>
          <div className="space-y-2">
            {scoringSystemTheory.interpretation.map((item, index) => (
              <div key={index} className="flex items-center gap-3">
                <Badge className={
                  index === 0 ? "bg-green-500/20 text-green-600 dark:text-green-400 border-green-500/30" :
                    index === 1 ? "bg-blue-500/20 text-blue-600 dark:text-blue-400 border-blue-500/30" :
                      index === 2 ? "bg-yellow-500/20 text-yellow-600 dark:text-yellow-400 border-yellow-500/30" :
                        "bg-red-500/20 text-red-600 dark:text-red-400 border-red-500/30"
                }>
                  {item.range}
                </Badge>
                <span className="text-sm text-foreground">{item.meaning}</span>
              </div>
            ))}
          </div>
        </div>

        <div className="text-xs text-muted-foreground pt-2 border-t border-border">
          <Info className="h-3 w-3 inline mr-1" />
          Оценка помогает выбрать наиболее подходящий алгоритм для конкретной задачи и размера данных.
        </div>
      </div>
    </CardContent>
  </Card>
);

export function ProfilerPage({ onNavigate }: ProfilerPageProps) {
  const { translations, theme } = useApp();
  const [dataStructure, setDataStructure] = useState<'array' | 'tree' | 'graph'>('array');
  const [dataSize, setDataSize] = useState(1000);
  const [results, setResults] = useState<AlgorithmResult[]>([]);
  const [isRunning, setIsRunning] = useState(false);
  const [progress, setProgress] = useState(0);
  const [selectedTheory, setSelectedTheory] = useState<string>('scoringSystem'); // По умолчанию показываем систему оценки
  const [selectedAlgorithm, setSelectedAlgorithm] = useState<string | null>(null);
  const [currentAlgorithm, setCurrentAlgorithm] = useState<string>('');
  const [config, setConfig] = useState({
    showTheory: true,
    autoSelectBest: true,
    showMemoryUsage: true
  });

  const runProfiling = async () => {
    setIsRunning(true);
    setResults([]);
    setProgress(0);
    setCurrentAlgorithm('');
    setSelectedAlgorithm(null);

    const algorithms = getAlgorithmsForStructure(dataStructure);
    const newResults: AlgorithmResult[] = [];

    for (let i = 0; i < algorithms.length; i++) {
      const algorithm = algorithms[i];
      setCurrentAlgorithm(algorithm.name);
      setProgress(((i) / algorithms.length) * 100);

      try {
        const result = await profiler.profile(algorithm.key, dataSize, dataStructure);

        const tempResults = [...newResults, result];
        const score = calculateAlgorithmScore(result, tempResults);
        const resultWithScore = { ...result, name: algorithm.name, score };
        newResults.push(resultWithScore);
        setResults([...newResults]);

        await new Promise(resolve => setTimeout(resolve, 300));
      } catch (error) {
        console.error(`Error profiling ${algorithm.name}:`, error);
      }
    }

    // Сортируем по баллу (чем выше балл - тем лучше)
    const sortedResults = newResults.sort((a, b) => (b.score || 0) - (a.score || 0));
    setResults(sortedResults);

    setProgress(100);
    setIsRunning(false);
    setCurrentAlgorithm('');

    // Автоматически выбираем лучший алгоритм
    if (config.autoSelectBest && sortedResults.length > 0) {
      setSelectedAlgorithm(sortedResults[0].name);
      // Также выбираем теорию для лучшего алгоритма
      setSelectedTheory(sortedResults[0].name);
    }
  };

  const stopProfiling = () => {
    setIsRunning(false);
    setProgress(0);
    setCurrentAlgorithm('');
  };

  const getAlgorithmsForStructure = (structure: string) => {
    switch (structure) {
      case 'array':
        return [
          { name: 'Пузырьковая сортировка', key: 'bubbleSort' },
          { name: 'Быстрая сортировка', key: 'quickSort' },
          { name: 'Сортировка слиянием', key: 'mergeSort' },
          { name: 'Сортировка вставками', key: 'insertionSort' },
        ];
      case 'tree':
        return [
          { name: 'Инордерный обход', key: 'bstInorder' },
          { name: 'Прямой обход', key: 'bstPreorder' },
          { name: 'Обратный обход', key: 'bstPostorder' },
          { name: 'Обход в ширину', key: 'bstLevelOrder' },
        ];
      case 'graph':
        return [
          { name: 'Поиск в ширину (BFS)', key: 'bfs' },
          { name: 'Поиск в глубину (DFS)', key: 'dfs' },
          { name: 'Алгоритм Дейкстры', key: 'dijkstra' },
        ];
      default:
        return [];
    }
  };

  const getAlgorithmDisplayName = (key: string): string => {
    const algorithm = getAlgorithmsForStructure(dataStructure).find(a => a.key === key);
    return algorithm?.name || key;
  };



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
      case 'array': return 200;
      case 'tree': return 50;
      case 'graph': return 10;
      default: return 100;
    }
  };

  const getTheoryOptions = () => {
    const options = [
      { value: 'scoringSystem', label: 'Система оценки' }
    ];

    // Добавляем все алгоритмы для текущей структуры данных
    const algorithms = getAlgorithmsForStructure(dataStructure);
    algorithms.forEach(alg => {
      options.push({
        value: alg.name,
        label: alg.name
      });
    });

    return options;
  };

  const getSelectedTheoryContent = () => {
    if (selectedTheory === 'scoringSystem') {
      return null; // Будет отображен отдельным компонентом
    }

    const theorySection = algorithmTheory[dataStructure];
    if (!theorySection) return null;

    const key = Object.keys(theorySection).find(k =>
      theorySection[k].name === selectedTheory
    );

    return key ? theorySection[key] : null;
  };

  const chartData = results.map(result => ({
    name: result.name,
    time: parseFloat(result.time.toFixed(2)),
    score: result.score || 0,
    operations: result.operations || 0,
    memory: result.memory ? parseFloat((result.memory / 1024).toFixed(2)) : 0,
    efficiency: result.efficiency || 0,
    comparisons: result.comparisons || 0,
    swaps: result.swaps || 0
  }));

  const selectedAlgorithmTheory = getSelectedTheoryContent();

  // Определяем лучший алгоритм на основе комплексного балла
  const getBestAlgorithm = () => {
    if (results.length === 0) return null;
    return results.reduce((best, current) =>
      (current.score || 0) > (best.score || 0) ? current : best
    );
  };

  const bestAlgorithm = getBestAlgorithm();

  return (
    <div className="space-y-6">
      {/* Теоретическая информация с выбором */}
      {config.showTheory && (
        <Card className="bg-card border-border shadow-sm">
          <CardHeader className="pb-3">
            <div className="flex justify-between items-center">
              <CardTitle className="flex items-center gap-2 text-lg text-foreground">
                <BookOpen className="h-5 w-5 text-primary" />
                Теоретическая справка
              </CardTitle>
              <div className="flex gap-2">
                <Select value={selectedTheory} onValueChange={setSelectedTheory}>
                  <SelectTrigger className="w-[250px] border-border bg-card text-foreground">
                    <SelectValue placeholder="Выберите теорию" />
                  </SelectTrigger>
                  <SelectContent className="bg-card border-border">
                    {getTheoryOptions().map((option) => (
                      <SelectItem key={option.value} value={option.value} className="text-foreground">
                        {option.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setConfig(prev => ({ ...prev, showTheory: !prev.showTheory }))}
                  className="text-muted-foreground hover:text-foreground"
                >
                  {config.showTheory ? 'Скрыть' : 'Показать'}
                </Button>
              </div>
            </div>
          </CardHeader>
          <CardContent>
            {selectedTheory === 'scoringSystem' ? (
              <ScoringSystemInfo />
            ) : selectedAlgorithmTheory ? (
              <Card className="bg-gradient-to-r from-blue-500/10 to-purple-500/10 border-border shadow-sm">
                <CardHeader>
                  <CardTitle className="flex items-center gap-2 text-foreground">
                    <Info className="h-5 w-5 text-primary" />
                    {selectedAlgorithmTheory.name}
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <p className="text-sm text-foreground">{selectedAlgorithmTheory.description}</p>

                    {selectedAlgorithmTheory.steps && (
                      <div>
                        <h4 className="font-semibold mb-2 text-foreground">Шаги выполнения:</h4>
                        <ol className="list-decimal list-inside space-y-1 text-sm ml-4 text-muted-foreground">
                          {selectedAlgorithmTheory.steps.map((step, index) => (
                            <li key={index} className="pl-2">{step}</li>
                          ))}
                        </ol>
                      </div>
                    )}

                    <div className="grid md:grid-cols-2 gap-4 text-sm">
                      {selectedAlgorithmTheory.bestCase && (
                        <div className="space-y-1">
                          <span className="font-semibold text-foreground">Лучший случай: </span>
                          <Badge variant="outline" className="bg-green-500/10 text-green-600 dark:text-green-400 border-green-500/30">
                            {selectedAlgorithmTheory.bestCase}
                          </Badge>
                        </div>
                      )}
                      {selectedAlgorithmTheory.averageCase && (
                        <div className="space-y-1">
                          <span className="font-semibold text-foreground">Средний случай: </span>
                          <Badge variant="outline" className="bg-yellow-500/10 text-yellow-600 dark:text-yellow-400 border-yellow-500/30">
                            {selectedAlgorithmTheory.averageCase}
                          </Badge>
                        </div>
                      )}
                      {selectedAlgorithmTheory.worstCase && (
                        <div className="space-y-1">
                          <span className="font-semibold text-foreground">Худший случай: </span>
                          <Badge variant="outline" className="bg-red-500/10 text-red-600 dark:text-red-400 border-red-500/30">
                            {selectedAlgorithmTheory.worstCase}
                          </Badge>
                        </div>
                      )}
                      {selectedAlgorithmTheory.stability && (
                        <div className="space-y-1">
                          <span className="font-semibold text-foreground">Стабильность: </span>
                          <Badge variant="outline" className={selectedAlgorithmTheory.stability === 'Стабильный' ? 'bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/30' : 'bg-orange-500/10 text-orange-600 dark:text-orange-400 border-orange-500/30'}>
                            {selectedAlgorithmTheory.stability}
                          </Badge>
                        </div>
                      )}
                    </div>

                    <div className="grid md:grid-cols-2 gap-4 text-sm">
                      {selectedAlgorithmTheory.whenToUse && (
                        <div className="space-y-1">
                          <h4 className="font-semibold text-foreground flex items-center gap-2">
                            <CheckCircle className="h-4 w-4 text-green-500" />
                            Когда использовать:
                          </h4>
                          <p className="text-muted-foreground">{selectedAlgorithmTheory.whenToUse}</p>
                        </div>
                      )}
                      {selectedAlgorithmTheory.whenNotToUse && (
                        <div className="space-y-1">
                          <h4 className="font-semibold text-foreground flex items-center gap-2">
                            <XCircle className="h-4 w-4 text-red-500" />
                            Когда не использовать:
                          </h4>
                          <p className="text-muted-foreground">{selectedAlgorithmTheory.whenNotToUse}</p>
                        </div>
                      )}
                    </div>

                    {selectedAlgorithmTheory.usage && (
                      <div className="text-sm">
                        <span className="font-semibold text-foreground">Применение: </span>
                        <span className="text-muted-foreground">{selectedAlgorithmTheory.usage}</span>
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            ) : (
              <div className="text-center p-8 text-muted-foreground">
                <BookOpen className="h-12 w-12 mx-auto mb-4 opacity-50" />
                <p>Выберите алгоритм для просмотра теории</p>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Панель управления */}
      <Card className="bg-card border-border shadow-sm">
        <CardHeader>
          <div className="flex justify-between items-center">
            <CardTitle className="flex items-center gap-2 text-foreground">
              <Settings className="h-5 w-5 text-primary" />
              {translations['profiler.title']}
            </CardTitle>
            <div className="flex gap-2">
              <Button
                variant="outline"
                size="sm"
                onClick={() => setConfig(prev => ({ ...prev, showTheory: !prev.showTheory }))}
                className="text-muted-foreground hover:text-foreground border-border"
              >
                {config.showTheory ? 'Скрыть теорию' : 'Показать теорию'}
              </Button>
            </div>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <Tabs value={dataStructure} onValueChange={(v) => {
            setDataStructure(v as any);
            setResults([]);
            setSelectedAlgorithm(null);
            setSelectedTheory('scoringSystem');
          }}>
            <TabsList className="grid w-full grid-cols-3 bg-muted relative">
              {/* Активный фон */}
              <div
                className={`absolute top-1 bottom-1 rounded-md transition-all duration-300 ease-in-out bg-red-500 ${dataStructure === 'array'
                    ? 'left-1 w-1/3'
                    : dataStructure === 'tree'
                      ? 'left-1/3 w-1/3'
                      : 'left-2/3 w-1/3'
                  }`}
              />

              <TabsTrigger
                value="array"
                className="relative z-10 data-[state=active]:text-white transition-colors"
              >
                {translations['structure.array']}
              </TabsTrigger>
              <TabsTrigger
                value="tree"
                className="relative z-10 data-[state=active]:text-white transition-colors"
              >
                {translations['structure.tree']}
              </TabsTrigger>
              <TabsTrigger
                value="graph"
                className="relative z-10 data-[state=active]:text-white transition-colors"
              >
                {translations['structure.graph']}
              </TabsTrigger>
            </TabsList>
          </Tabs>

          <div className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm font-medium flex items-center justify-between text-foreground">
                <span>{translations['data.size']}: <span className="text-primary">{dataSize.toLocaleString()}</span></span>
                <span className="text-xs text-muted-foreground">
                  {dataSize <= 1000 ? 'Маленький' : dataSize <= 5000 ? 'Средний' : 'Большой'}
                </span>
              </label>
              <Slider
                value={[dataSize]}
                onValueChange={(value) => setDataSize(value[0])}
                max={getMaxDataSize()}
                min={dataStructure === 'graph' ? 10 : (dataStructure === 'array' ? 200 : 50)}
                step={getDataSizeStep()}
                className="w-full"
              />
              <div className="flex justify-between text-xs text-muted-foreground">
                <span>{dataStructure === 'graph' ? '10 узлов' : '200 элементов'}</span>
                <span>{getMaxDataSize().toLocaleString()} элементов</span>
              </div>
            </div>

            {isRunning && (
              <ProgressIndicator progress={progress} currentAlgorithm={currentAlgorithm} />
            )}

            <div className="flex space-x-2">
              {!isRunning ? (
                <Button
                  onClick={runProfiling}
                  disabled={isRunning}
                  className="bg-primary hover:bg-primary/90 text-primary-foreground flex-1 shadow-sm"
                  size="lg"
                >
                  <Play className="h-4 w-4 mr-2" />
                  {translations['profiler.run']}
                </Button>
              ) : (
                <Button
                  onClick={stopProfiling}
                  variant="destructive"
                  className="flex-1 shadow-sm"
                  size="lg"
                >
                  <Square className="h-4 w-4 mr-2" />
                  Остановить
                </Button>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      {isRunning && (
        <ProgressIndicator progress={progress} currentAlgorithm={currentAlgorithm} />
      )}

      {results.length > 0 && (
        <>
          {/* Основные метрики */}
          <div className="grid lg:grid-cols-2 gap-6">
            <Card className="bg-card border-border shadow-sm">
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-foreground">
                  <Clock className="h-5 w-5 text-primary" />
                  Время выполнения (мс)
                </CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart
                    data={chartData}
                    margin={{ top: 20, right: 30, left: 20, bottom: 60 }}
                  >
                    <CartesianGrid
                      strokeDasharray="3 3"
                      stroke={theme === 'dark' ? '#374151' : '#e5e7eb'}
                    />
                    <XAxis
                      dataKey="name"
                      tickFormatter={getShortName}
                      interval={0}
                      angle={-45}
                      textAnchor="end"
                      height={60}
                      stroke={theme === 'dark' ? '#9ca3af' : '#6b7280'}
                    />
                    <YAxis
                      stroke={theme === 'dark' ? '#9ca3af' : '#6b7280'}
                    />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: theme === 'dark' ? '#1f2937' : '#ffffff',
                        borderColor: theme === 'dark' ? '#374151' : '#e5e7eb',
                        color: theme === 'dark' ? '#f3f4f6' : '#111827'
                      }}
                      formatter={(value) => [`${value} мс`, 'Время']}
                      labelFormatter={(label) => `Алгоритм: ${label}`}
                    />
                    <Bar
                      dataKey="time"
                      fill="#3b82f6"
                      name="Время (мс)"
                      onClick={(data) => {
                        setSelectedAlgorithm(data.name);
                        setSelectedTheory(data.name);
                      }}
                      style={{ cursor: 'pointer' }}
                      radius={[4, 4, 0, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
              </CardContent>
            </Card>

            <Card className="bg-card border-border shadow-sm">
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-foreground">
                  <Award className="h-5 w-5 text-accent-green" />
                  Комплексная оценка
                </CardTitle>
              </CardHeader>
              <CardContent>
                <ResponsiveContainer width="100%" height={300}>
                  <BarChart
                    data={chartData}
                    margin={{ top: 20, right: 30, left: 20, bottom: 60 }}
                  >
                    <CartesianGrid
                      strokeDasharray="3 3"
                      stroke={theme === 'dark' ? '#374151' : '#e5e7eb'}
                    />
                    <XAxis
                      dataKey="name"
                      tickFormatter={getShortName}
                      interval={0}
                      angle={-45}
                      textAnchor="end"
                      height={60}
                      stroke={theme === 'dark' ? '#9ca3af' : '#6b7280'}
                    />
                    <YAxis
                      stroke={theme === 'dark' ? '#9ca3af' : '#6b7280'}
                    />
                    <Tooltip
                      contentStyle={{
                        backgroundColor: theme === 'dark' ? '#1f2937' : '#ffffff',
                        borderColor: theme === 'dark' ? '#374151' : '#e5e7eb',
                        color: theme === 'dark' ? '#f3f4f6' : '#111827'
                      }}
                      formatter={(value) => [`${value} баллов`, 'Оценка']}
                      labelFormatter={(label) => `Алгоритм: ${label}`}
                    />
                    <Bar
                      dataKey="score"
                      fill="#10b981"
                      name="Комплексная оценка"
                      onClick={(data) => {
                        setSelectedAlgorithm(data.name);
                        setSelectedTheory(data.name);
                      }}
                      style={{ cursor: 'pointer' }}
                      radius={[4, 4, 0, 0]}
                    />
                  </BarChart>
                </ResponsiveContainer>
                <div className="mt-4 text-xs text-center text-muted-foreground">
                  <Scale className="h-3 w-3 inline mr-1" />
                  Оценка рассчитывается на основе времени, памяти, операций и эффективности
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Детальная статистика */}
          <div className="grid md:grid-cols-3 gap-6">
            {/* Распределение операций */}
            <Card className="bg-card border-border shadow-sm min-h-[300px]">
              <CardHeader className="pb-2">
                <CardTitle className="text-sm text-foreground">
                  Распределение операций
                </CardTitle>
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
                      dataKey="value"
                    >
                      {results.map((entry, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip
                      contentStyle={{
                        backgroundColor: theme === 'dark' ? '#1f2937' : '#ffffff',
                        borderColor: theme === 'dark' ? '#374151' : '#e5e7eb',
                        color: theme === 'dark' ? '#f3f4f6' : '#111827'
                      }}
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
                        <span className="truncate text-foreground">{getShortName(result.name)}</span>
                      </div>
                      <span className="text-muted-foreground">
                        {((result.operations || 0) / results.reduce((sum, r) => sum + (r.operations || 0), 0) * 100).toFixed(1)}%
                      </span>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            {/* Сравнения и перестановки - показываем только для массивов */}
            {dataStructure === 'array' && (
              <Card className="bg-card border-border shadow-sm min-h-[300px] overflow-visible">
                <CardHeader className="pb-2">
                  <CardTitle className="text-sm text-foreground">Сравнения vs Перестановки</CardTitle>
                </CardHeader>
                <CardContent className="p-0 pr-1 pl-0">
                  <div className="relative w-full" style={{ height: '200px' }}>
                    <ResponsiveContainer width="98%" height="100%">
                      <BarChart
                        data={chartData}
                        layout="vertical"
                        margin={{ left: 35, right: 15 }}
                      >
                        <CartesianGrid
                          strokeDasharray="3 3"
                          stroke={theme === 'dark' ? '#374151' : '#e5e7eb'}
                        />
                        <XAxis
                          type="number"
                          tick={{ fontSize: 11 }}
                          stroke={theme === 'dark' ? '#9ca3af' : '#6b7280'}
                        />
                        <YAxis
                          dataKey="name"
                          type="category"
                          tickFormatter={getShortName}
                          width={40}
                          tick={{ fontSize: 11 }}
                          stroke={theme === 'dark' ? '#9ca3af' : '#6b7280'}
                        />
                        <Tooltip
                          contentStyle={{
                            backgroundColor: theme === 'dark' ? '#1f2937' : '#ffffff',
                            borderColor: theme === 'dark' ? '#374151' : '#e5e7eb',
                            color: theme === 'dark' ? '#f3f4f6' : '#111827'
                          }}
                          formatter={(value) => [value.toLocaleString(), 'Количество']}
                        />
                        <Legend
                          verticalAlign="bottom"
                          height={30}
                          iconType="circle"
                          iconSize={8}
                          wrapperStyle={{
                            fontSize: '11px',
                            paddingTop: '10px'
                          }}
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
                </CardContent>
              </Card>
            )}

            {/* Для деревьев и графов показываем эффективность */}
            {dataStructure !== 'array' && (
              <Card className="bg-card border-border shadow-sm min-h-[300px] overflow-visible">
                <CardHeader className="pb-2">
                  <CardTitle className="text-sm text-foreground">
                    {dataStructure === 'tree' ? 'Эффективность обходов' : 'Эффективность графовых алгоритмов'}
                  </CardTitle>
                </CardHeader>
                <CardContent className="p-0 pr-1 pl-0">
                  <div className="relative w-full" style={{ height: '250px' }}>
                    <ResponsiveContainer width="98%" height="100%">
                      <BarChart
                        data={chartData}
                        margin={{ left: 40, right: 15, top: 10, bottom: 10 }}
                      >
                        <CartesianGrid
                          strokeDasharray="3 3"
                          stroke={theme === 'dark' ? '#374151' : '#e5e7eb'}
                        />
                        <XAxis
                          dataKey="name"
                          tickFormatter={getShortName}
                          angle={-45}
                          textAnchor="end"
                          height={70}
                          tick={{ fontSize: 11 }}
                          stroke={theme === 'dark' ? '#9ca3af' : '#6b7280'}
                        />
                        <YAxis
                          tick={{ fontSize: 11 }}
                          stroke={theme === 'dark' ? '#9ca3af' : '#6b7280'}
                          label={{
                            value: 'Операций/сек',
                            angle: -90,
                            position: 'insideLeft',
                            offset: 10,
                            style: { fontSize: '12px' }
                          }}
                        />
                        <Tooltip
                          contentStyle={{
                            backgroundColor: theme === 'dark' ? '#1f2937' : '#ffffff',
                            borderColor: theme === 'dark' ? '#374151' : '#e5e7eb',
                            color: theme === 'dark' ? '#f3f4f6' : '#111827'
                          }}
                          formatter={(value) => [`${Number(value).toFixed(0)}`, 'Операций в секунду']}
                        />
                        <Bar
                          dataKey="efficiency"
                          fill="#8b5cf6"
                          name="Эффективность"
                          radius={[4, 4, 0, 0]}
                        />
                      </BarChart>
                    </ResponsiveContainer>
                  </div>
                </CardContent>
              </Card>
            )}

            {/* Использование памяти */}
            {config.showMemoryUsage && (
              <Card className="bg-card border-border shadow-sm min-h-[300px]">
                <CardHeader className="pb-2">
                  <CardTitle className="text-sm text-foreground">Использование памяти</CardTitle>
                </CardHeader>
                <CardContent className="pt-0">
                  <div className="space-y-3 h-[200px] flex flex-col justify-center">
                    {results.map((result, index) => (
                      <div key={index} className="space-y-1">
                        <div className="flex justify-between items-center text-xs">
                          <span className="truncate font-medium text-foreground">{getShortName(result.name)}</span>
                          <span className="text-muted-foreground whitespace-nowrap">
                            {((result.memory || 0) / 1024).toFixed(0)} KB
                          </span>
                        </div>
                        <div className="w-full bg-muted rounded-full h-2">
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
            )}
          </div>

          {/* Теория выбранного алгоритма (если выбран не система оценки) */}
          {selectedTheory !== 'scoringSystem' && selectedAlgorithmTheory && (
            <Card className="bg-gradient-to-r from-blue-500/10 to-purple-500/10 border-border shadow-sm">
              <CardHeader>
                <CardTitle className="flex items-center gap-2 text-foreground">
                  <Info className="h-5 w-5 text-primary" />
                  {selectedAlgorithmTheory.name}
                  {bestAlgorithm?.name === selectedAlgorithmTheory.name && (
                    <Badge className="ml-2 bg-green-500/20 text-green-600 dark:text-green-400 border-green-500/30">
                      <Sparkles className="h-3 w-3 mr-1" />
                      Лучший выбор для текущих данных
                    </Badge>
                  )}
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <p className="text-sm text-foreground">{selectedAlgorithmTheory.description}</p>

                  {selectedAlgorithmTheory.steps && (
                    <div>
                      <h4 className="font-semibold mb-2 text-foreground">Шаги выполнения:</h4>
                      <ol className="list-decimal list-inside space-y-1 text-sm ml-4 text-muted-foreground">
                        {selectedAlgorithmTheory.steps.map((step, index) => (
                          <li key={index} className="pl-2">{step}</li>
                        ))}
                      </ol>
                    </div>
                  )}

                  <div className="grid md:grid-cols-2 gap-4 text-sm">
                    {selectedAlgorithmTheory.bestCase && (
                      <div className="space-y-1">
                        <span className="font-semibold text-foreground">Лучший случай: </span>
                        <Badge variant="outline" className="bg-green-500/10 text-green-600 dark:text-green-400 border-green-500/30">
                          {selectedAlgorithmTheory.bestCase}
                        </Badge>
                      </div>
                    )}
                    {selectedAlgorithmTheory.averageCase && (
                      <div className="space-y-1">
                        <span className="font-semibold text-foreground">Средний случай: </span>
                        <Badge variant="outline" className="bg-yellow-500/10 text-yellow-600 dark:text-yellow-400 border-yellow-500/30">
                          {selectedAlgorithmTheory.averageCase}
                        </Badge>
                      </div>
                    )}
                    {selectedAlgorithmTheory.worstCase && (
                      <div className="space-y-1">
                        <span className="font-semibold text-foreground">Худший случай: </span>
                        <Badge variant="outline" className="bg-red-500/10 text-red-600 dark:text-red-400 border-red-500/30">
                          {selectedAlgorithmTheory.worstCase}
                        </Badge>
                      </div>
                    )}
                    {selectedAlgorithmTheory.stability && (
                      <div className="space-y-1">
                        <span className="font-semibold text-foreground">Стабильность: </span>
                        <Badge variant="outline" className={selectedAlgorithmTheory.stability === 'Стабильный' ? 'bg-blue-500/10 text-blue-600 dark:text-blue-400 border-blue-500/30' : 'bg-orange-500/10 text-orange-600 dark:text-orange-400 border-orange-500/30'}>
                          {selectedAlgorithmTheory.stability}
                        </Badge>
                      </div>
                    )}
                  </div>

                  {selectedAlgorithmTheory.whenToUse && (
                    <div className="text-sm">
                      <span className="font-semibold text-foreground">Рекомендуется использовать: </span>
                      <span className="text-muted-foreground">{selectedAlgorithmTheory.whenToUse}</span>
                    </div>
                  )}

                  {selectedAlgorithmTheory.usage && (
                    <div className="text-sm">
                      <span className="font-semibold text-foreground">Применение: </span>
                      <span className="text-muted-foreground">{selectedAlgorithmTheory.usage}</span>
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
          )}

          {/* Детальные результаты */}


          <Card className="bg-card border-border shadow-sm">
            <CardHeader>
              <CardTitle className="text-foreground">Детальные результаты тестирования</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {results.map((result, index) => {
                  const isBest = bestAlgorithm?.name === result.name;
                  return (
                    <div
                      key={index}
                      className={`flex items-center justify-between p-4 border rounded-lg transition-all hover:shadow-md ${selectedAlgorithm === result.name
                        ? 'ring-2 ring-primary bg-primary/5 border-primary/30'
                        : 'bg-card border-border'
                        } ${isBest ? 'border-green-500/30 bg-green-500/5' : ''}`}
                      onClick={() => {
                        setSelectedAlgorithm(result.name);
                        setSelectedTheory(result.name);
                      }}
                      style={{ cursor: 'pointer' }}
                    >
                      <div className="flex-1">
                        <div className="flex items-center gap-3 mb-2">
                          <h4 className="font-medium text-lg text-foreground">{result.name}</h4>
                          <div className="flex gap-2">
                            <Badge variant="outline" className="bg-primary/10 text-primary border-primary/30">
                              Время: {result.complexity.time}
                            </Badge>
                            <Badge variant="outline" className="bg-green-500/10 text-green-600 dark:text-green-400 border-green-500/30">
                              Память: {result.complexity.space}
                            </Badge>
                            {isBest && (
                              <Badge className="bg-green-500/20 text-green-600 dark:text-green-400 border-green-500/30">
                                <Sparkles className="h-3 w-3 mr-1" />
                                Лучший выбор
                              </Badge>
                            )}
                          </div>
                        </div>

                        {/* УНИВЕРСАЛЬНЫЕ МЕТРИКИ ДЛЯ ВСЕХ СТРУКТУР */}
                        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                          {/* Время выполнения */}
                          <div className="text-foreground">
                            <span className="font-medium">Время: </span>
                            {result.time.toFixed(2)} мс
                          </div>

                          {/* Оценка */}
                          <div className="text-foreground">
                            <span className="font-medium">Оценка: </span>
                            <span className={`font-bold ${isBest ? 'text-red-500' : 'text-primary'}`}>
                              {result.score?.toFixed(1)} баллов
                            </span>
                          </div>

                          {/* Операции (для всех структур) */}
                          <div className="text-muted-foreground">
                            <span className="font-medium">Операции: </span>
                            {result.operations?.toLocaleString() || '0'}
                          </div>

                          {/* Память (для всех структур) */}
                          <div className="text-muted-foreground">
                            <span className="font-medium">Память: </span>
                            {result.memory ? `${(result.memory / 1024).toFixed(2)} KB` : 'N/A'}
                          </div>

                          {/* Только для массивов - сравнения и перестановки */}
                          {dataStructure === 'array' && result.comparisons !== undefined && (
                            <div className="text-muted-foreground">
                              <span className="font-medium">Сравнения: </span>
                              {result.comparisons.toLocaleString()}
                            </div>
                          )}

                          {dataStructure === 'array' && result.swaps !== undefined && (
                            <div className="text-muted-foreground">
                              <span className="font-medium">Перестановки: </span>
                              {result.swaps.toLocaleString()}
                            </div>
                          )}
                        </div>
                      </div>
                      <div className="flex flex-col items-end gap-2 ml-4">
                        <span className={`text-lg font-bold ${isBest ? 'text-red-500' : 'text-muted-foreground'}`}>
                          #{index + 1}
                        </span>
                        {index > 0 && result.score && results[0].score && (
                          <span className="text-xs text-muted-foreground">
                            -{((results[0].score - result.score) / result.score * 100).toFixed(1)}%
                          </span>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            </CardContent>
          </Card>
        </>
      )}

      {results.length === 0 && !isRunning && (
        <Card className="bg-card border-border shadow-sm">
          <CardContent className="p-12 text-center">
            <div className="text-muted-foreground space-y-4">
              <br />
              <Calculator className="h-12 w-12 mx-auto text-muted-foreground" />
              <p className="text-lg font-medium text-foreground">Начните профилирование алгоритмов</p>
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

// Вспомогательные компоненты для иконок
const CheckCircle = ({ className }: { className?: string }) => (
  <svg xmlns="http://www.w3.org/2000/svg" className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
  </svg>
);

const XCircle = ({ className }: { className?: string }) => (
  <svg xmlns="http://www.w3.org/2000/svg" className={className} fill="none" viewBox="0 0 24 24" stroke="currentColor">
    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M10 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2m7-2a9 9 0 11-18 0 9 9 0 0118 0z" />
  </svg>
);