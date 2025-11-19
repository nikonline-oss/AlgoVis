import React, { useState, useEffect, useCallback } from 'react';
import { Button } from '../components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { Slider } from '../components/ui/slider';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Input } from '../components/ui/input';
import { AnimationControls } from '../components/AnimationControls';
import { ArrayVisualization } from '../components/ArrayVisualization';
import { TreeVisualization, TreeNode } from '../components/TreeVisualization';
import { GraphVisualization, GraphNode, GraphEdge } from '../components/GraphVisualization';
import { ListVisualization, ListNode } from '../components/ListVisualization';
import { StackVisualization } from '../components/StackVisualization';
import { QueueVisualization } from '../components/QueueVisualization';
import { StatsPanel } from '../components/StatsPanel';
import { useApp } from '../contexts/AppContext';


interface SortingStep {
  array: number[];
  comparing?: number[];
  swapping?: number[];
  sorted?: number[];
  pivotIndex?: number;
}

interface TreeStep {
  tree: TreeNode | null;
  currentNode?: number;
  highlightedNodes?: number[];
  visitedNodes?: number[];
}

interface GraphStep {
  nodes: GraphNode[];
  edges: GraphEdge[];
  currentNode?: number;
  highlightedNodes?: number[];
  visitedNodes?: number[];
  highlightedEdges?: Array<[number, number]>;
}

interface ListStep {
  nodes: ListNode[];
  head?: number | null;
  tail?: number | null;
  currentIndex?: number;
  highlightedIndices?: number[];
  comparedIndices?: number[];
  type?: 'singly' | 'doubly';
}

interface StackStep {
  items: number[];
  top?: number;
  highlightedIndex?: number;
  operation?: 'push' | 'pop' | null;
}

interface QueueStep {
  items: number[];
  front?: number;
  rear?: number;
  highlightedIndex?: number;
  operation?: 'enqueue' | 'dequeue' | null;
}

type VisualizationStep = SortingStep | TreeStep | GraphStep | ListStep | StackStep | QueueStep;

export function VisualizerPage() {
  const { translations, sharedData, setSharedData } = useApp();
  const [dataStructure, setDataStructure] = useState<'array' | 'tree' | 'graph' | 'list' | 'stack' | 'queue'>('array');
  const [algorithm, setAlgorithm] = useState('bubblesort');

  // Array state
  const [arraySize, setArraySize] = useState(20);
  const [array, setArray] = useState<number[]>([]);
  const [originalArray, setOriginalArray] = useState<number[]>([]);

  // Tree state
  const [tree, setTree] = useState<TreeNode | null>(null);
  const [originalTree, setOriginalTree] = useState<TreeNode | null>(null);
  const [insertValue, setInsertValue] = useState('');

  // Graph state
  const [graphNodes, setGraphNodes] = useState<GraphNode[]>([]);
  const [graphEdges, setGraphEdges] = useState<GraphEdge[]>([]);
  const [originalGraphNodes, setOriginalGraphNodes] = useState<GraphNode[]>([]);
  const [originalGraphEdges, setOriginalGraphEdges] = useState<GraphEdge[]>([]);
  const [nodeCount, setNodeCount] = useState(6);
  const [graphType, setGraphType] = useState<'circular' | 'grid' | 'complete' | 'random'>('circular');
  const [directedGraph, setDirectedGraph] = useState(false);

  // List state
  const [listNodes, setListNodes] = useState<ListNode[]>([]);
  const [originalListNodes, setOriginalListNodes] = useState<ListNode[]>([]);
  const [listType, setListType] = useState<'singly' | 'doubly'>('singly');
  const [listSize, setListSize] = useState(5);
  const [listValue, setListValue] = useState('');
  const [listPosition, setListPosition] = useState('');

  // Stack state
  const [stackItems, setStackItems] = useState<number[]>([]);
  const [originalStackItems, setOriginalStackItems] = useState<number[]>([]);
  const [stackSize, setStackSize] = useState(5);
  const [stackValue, setStackValue] = useState('');

  // Queue state
  const [queueItems, setQueueItems] = useState<number[]>([]);
  const [originalQueueItems, setOriginalQueueItems] = useState<number[]>([]);
  const [queueSize, setQueueSize] = useState(5);
  const [queueValue, setQueueValue] = useState('');

  // Animation state
  const [isPlaying, setIsPlaying] = useState(false);
  const [speed, setSpeed] = useState(1);
  const [currentStep, setCurrentStep] = useState(0);
  const [steps, setSteps] = useState<VisualizationStep[]>([]);
  const [stats, setStats] = useState({ comparisons: 0, swaps: 0, operations: 0 });

  // Load shared data from profiler
  useEffect(() => {
    if (sharedData) {
      setDataStructure(sharedData.type);
      if (sharedData.algorithm) {
        setAlgorithm(sharedData.algorithm);
      }
      if (sharedData.data) {
        if (sharedData.type === 'array' && Array.isArray(sharedData.data)) {
          setArray(sharedData.data);
          setOriginalArray([...sharedData.data]);
        }
      }
      setSharedData(null);
    }
  }, [sharedData, setSharedData]);

  const generateRandomArray = useCallback(() => {
    const newArray = Array.from({ length: arraySize }, () =>
      Math.floor(Math.random() * 100) + 1
    );
    setArray(newArray);
    setOriginalArray([...newArray]);
    setCurrentStep(0);
    setSteps([]);
    setStats({ comparisons: 0, swaps: 0, operations: 0 });
    setIsPlaying(false);
  }, [arraySize]);

  const generateRandomTree = useCallback(() => {
    const values = Array.from({ length: 10 }, () => Math.floor(Math.random() * 100) + 1);
    let root: TreeNode | null = null;

    const insertNode = (node: TreeNode | null, value: number): TreeNode => {
      if (!node) {
        return { value };
      }
      if (value < node.value) {
        node.left = insertNode(node.left || null, value);
      } else {
        node.right = insertNode(node.right || null, value);
      }
      return node;
    };

    values.forEach(value => {
      root = insertNode(root, value);
    });

    setTree(root);
    setOriginalTree(JSON.parse(JSON.stringify(root)));
    setCurrentStep(0);
    setSteps([]);
    setStats({ comparisons: 0, swaps: 0, operations: 0 });
    setIsPlaying(false);
  }, []);

const generateRandomGraph = useCallback((type: 'circular' | 'grid' | 'complete' | 'random' = graphType) => {
  const nodes: GraphNode[] = [];
  const edges: GraphEdge[] = [];
  
  const centerX = 400;
  const centerY = 250;
  const radius = 150;

  if (type === 'circular') {
    // Круговое расположение
    for (let i = 0; i < nodeCount; i++) {
      const angle = (i * 2 * Math.PI) / nodeCount;
      nodes.push({
        id: i,
        x: centerX + radius * Math.cos(angle),
        y: centerY + radius * Math.sin(angle),
        label: String(i)
      });
    }
    
    // Создаем кольцевые связи
    for (let i = 0; i < nodeCount; i++) {
      const next = (i + 1) % nodeCount;
      edges.push({
        from: i,
        to: next,
        weight: Math.floor(Math.random() * 5) + 1,
      });
    }
  } else if (type === 'grid') {
    // Сеточное расположение
    const cols = Math.ceil(Math.sqrt(nodeCount));
    const rows = Math.ceil(nodeCount / cols);
    const cellWidth = 250 / Math.max(cols - 1, 1);
    const cellHeight = 250 / Math.max(rows - 1, 1);
    
    for (let i = 0; i < nodeCount; i++) {
      const row = Math.floor(i / cols);
      const col = i % cols;
      nodes.push({
        id: i,
        x: 200 + col * cellWidth,
        y: 150 + row * cellHeight,
        label: String(i)
      });
    }
    
    // Горизонтальные связи
    for (let i = 0; i < nodeCount; i++) {
      const row = Math.floor(i / cols);
      const col = i % cols;
      
      if (col < cols - 1 && i + 1 < nodeCount) {
        edges.push({
          from: i,
          to: i + 1,
          weight: Math.floor(Math.random() * 5) + 1,
        });
      }
    }
    
    // Вертикальные связи
    for (let i = 0; i < nodeCount; i++) {
      const row = Math.floor(i / cols);
      const col = i % cols;
      
      if (row < rows - 1 && i + cols < nodeCount) {
        edges.push({
          from: i,
          to: i + cols,
          weight: Math.floor(Math.random() * 5) + 1,
        });
      }
    }
  } else if (type === 'complete') {
    // Полный граф K_n
    for (let i = 0; i < nodeCount; i++) {
      const angle = (i * 2 * Math.PI) / nodeCount;
      nodes.push({
        id: i,
        x: centerX + radius * Math.cos(angle),
        y: centerY + radius * Math.sin(angle),
        label: String(i)
      });
    }
    
    // Все узлы соединены со всеми
    for (let i = 0; i < nodeCount; i++) {
      for (let j = i + 1; j < nodeCount; j++) {
        edges.push({
          from: i,
          to: j,
          weight: Math.floor(Math.random() * 5) + 1,
        });
      }
    }
  } else if (type === 'random') {
    // Свободное расположение узлов с улучшенной проверкой коллизий
    const padding = 60;
    const minDistance = 70; // Минимальное расстояние между узлами
    
    // Функция для проверки коллизий
    const hasCollision = (x: number, y: number, existingNodes: GraphNode[]) => {
      for (const node of existingNodes) {
        const distance = Math.sqrt(Math.pow(x - node.x, 2) + Math.pow(y - node.y, 2));
        if (distance < minDistance) {
          return true;
        }
      }
      return false;
    };
    
    // Размещаем узлы с улучшенным алгоритмом
    for (let i = 0; i < nodeCount; i++) {
      let attempts = 0;
      let x, y;
      
      // Пытаемся найти свободную позицию
      do {
        x = padding + Math.random() * (800 - 2 * padding);
        y = padding + Math.random() * (500 - 2 * padding);
        attempts++;
        
        // После 50 попыток увеличиваем поисковое пространство
        if (attempts > 50) {
          // Пробуем позиции ближе к центру
          x = 200 + Math.random() * 400;
          y = 150 + Math.random() * 200;
        }
        
        // После 100 попыток принимаем любую позицию
        if (attempts > 100) {
          break;
        }
      } while (hasCollision(x, y, nodes));
      
      nodes.push({
        id: i,
        x: x,
        y: y,
        label: String(i)
      });
    }

    // Применяем простую силовую раскладку для финального выравнивания
    const iterations = 30;
    const repulsionForce = 80;
    
    for (let iter = 0; iter < iterations; iter++) {
      for (let i = 0; i < nodeCount; i++) {
        let forceX = 0;
        let forceY = 0;
        
        // Отталкивание от других узлов
        for (let j = 0; j < nodeCount; j++) {
          if (i !== j) {
            const dx = nodes[i].x - nodes[j].x;
            const dy = nodes[i].y - nodes[j].y;
            const distance = Math.sqrt(dx * dx + dy * dy);
            
            if (distance > 0 && distance < 150) {
              const force = repulsionForce / distance;
              forceX += (dx / distance) * force;
              forceY += (dy / distance) * force;
            }
          }
        }
        
        // Притяжение к центру (чтобы узлы не улетали за границы)
        const centerX = 400;
        const centerY = 250;
        const toCenterX = centerX - nodes[i].x;
        const toCenterY = centerY - nodes[i].y;
        const toCenterDist = Math.sqrt(toCenterX * toCenterX + toCenterY * toCenterY);
        
        if (toCenterDist > 200) {
          forceX += toCenterX * 0.1;
          forceY += toCenterY * 0.1;
        }
        
        // Применяем силы с ограничением
        const forceMagnitude = Math.sqrt(forceX * forceX + forceY * forceY);
        if (forceMagnitude > 15) {
          forceX = (forceX / forceMagnitude) * 15;
          forceY = (forceY / forceMagnitude) * 15;
        }
        
        nodes[i].x += forceX;
        nodes[i].y += forceY;
        
        // Ограничиваем границы
        nodes[i].x = Math.max(padding, Math.min(800 - padding, nodes[i].x));
        nodes[i].y = Math.max(padding, Math.min(500 - padding, nodes[i].y));
      }
    }

    // Создаем минимальное остовное дерево для базовой связности
    const addedEdges = new Set<string>();
    const connectedNodes = new Set<number>([0]);
    
    while (connectedNodes.size < nodeCount) {
      let bestEdge: {from: number, to: number, distance: number} | null = null;
      
      // Ищем ближайший неподключенный узел
      for (const connectedNode of connectedNodes) {
        for (let i = 0; i < nodeCount; i++) {
          if (!connectedNodes.has(i)) {
            const dx = nodes[connectedNode].x - nodes[i].x;
            const dy = nodes[connectedNode].y - nodes[i].y;
            const distance = Math.sqrt(dx * dx + dy * dy);
            
            if (!bestEdge || distance < bestEdge.distance) {
              bestEdge = { from: connectedNode, to: i, distance };
            }
          }
        }
      }
      
      if (bestEdge) {
        const edgeKey = `${Math.min(bestEdge.from, bestEdge.to)}-${Math.max(bestEdge.from, bestEdge.to)}`;
        addedEdges.add(edgeKey);
        edges.push({
          from: bestEdge.from,
          to: bestEdge.to,
          weight: Math.floor(bestEdge.distance / 25) + 1,
        });
        connectedNodes.add(bestEdge.to);
      } else {
        break;
      }
    }

    // Добавляем только очень короткие дополнительные ребра для читаемости
    const shortEdges: {from: number, to: number, distance: number}[] = [];
    
    for (let i = 0; i < nodeCount; i++) {
      for (let j = i + 1; j < nodeCount; j++) {
        const edgeKey = `${i}-${j}`;
        if (!addedEdges.has(edgeKey)) {
          const dx = nodes[i].x - nodes[j].x;
          const dy = nodes[i].y - nodes[j].y;
          const distance = Math.sqrt(dx * dx + dy * dy);
          
          // Добавляем только очень короткие ребра (меньше 120px)
          if (distance < 120) {
            shortEdges.push({ from: i, to: j, distance });
          }
        }
      }
    }
    
    // Сортируем по расстоянию и добавляем самые короткие
    shortEdges.sort((a, b) => a.distance - b.distance);
    const maxExtraEdges = Math.min(shortEdges.length, Math.max(2, Math.floor(nodeCount * 0.4)));
    
    for (let i = 0; i < maxExtraEdges; i++) {
      const edge = shortEdges[i];
      const edgeKey = `${edge.from}-${edge.to}`;
      addedEdges.add(edgeKey);
      edges.push({
        from: edge.from,
        to: edge.to,
        weight: Math.floor(edge.distance / 25) + 1,
      });
    }
  }
  
  setGraphNodes(nodes);
  setGraphEdges(edges);
  setOriginalGraphNodes([...nodes]);
  setOriginalGraphEdges([...edges]);
  setCurrentStep(0);
  setSteps([]);
  setStats({ comparisons: 0, swaps: 0, operations: 0 });
  setIsPlaying(false);
}, [nodeCount, graphType]);

  const generateRandomList = useCallback(() => {
    const nodes: ListNode[] = [];

    for (let i = 0; i < listSize; i++) {
      nodes.push({
        value: Math.floor(Math.random() * 100) + 1,
        next: i < listSize - 1 ? i + 1 : null,
        prev: listType === 'doubly' ? (i > 0 ? i - 1 : null) : undefined,
      });
    }

    setListNodes(nodes);
    setOriginalListNodes(JSON.parse(JSON.stringify(nodes)));
    setCurrentStep(0);
    setSteps([]);
    setStats({ comparisons: 0, swaps: 0, operations: 0 });
    setIsPlaying(false);
  }, [listType, listSize]);

  const generateRandomStack = useCallback(() => {
    const items = Array.from({ length: stackSize }, () => Math.floor(Math.random() * 100) + 1);
    setStackItems(items);
    setOriginalStackItems([...items]);
    setCurrentStep(0);
    setSteps([]);
    setStats({ comparisons: 0, swaps: 0, operations: 0 });
    setIsPlaying(false);
  }, [stackSize]);

  const generateRandomQueue = useCallback(() => {
    const items = Array.from({ length: queueSize }, () => Math.floor(Math.random() * 100) + 1);
    setQueueItems(items);
    setOriginalQueueItems([...items]);
    setCurrentStep(0);
    setSteps([]);
    setStats({ comparisons: 0, swaps: 0, operations: 0 });
    setIsPlaying(false);
  }, [queueSize]);

  useEffect(() => {
    if (dataStructure === 'array') {
      generateRandomArray();
      setAlgorithm('bubblesort');
    } else if (dataStructure === 'tree') {
      generateRandomTree();
      setAlgorithm('bst.inorder');
    } else if (dataStructure === 'graph') {
      generateRandomGraph();
      setAlgorithm('bfs');
    } else if (dataStructure === 'list') {
      generateRandomList();
      setAlgorithm('');
    } else if (dataStructure === 'stack') {
      generateRandomStack();
      setAlgorithm('');
    } else if (dataStructure === 'queue') {
      generateRandomQueue();
      setAlgorithm('');
    }
  }, [dataStructure, generateRandomArray, generateRandomTree, generateRandomGraph,
    generateRandomList, generateRandomStack, generateRandomQueue]);

  // Regenerate data when type or size changes
  useEffect(() => {
    if (dataStructure === 'list') {
      generateRandomList();
    }
  }, [listType, dataStructure, generateRandomList]);

  // Sorting algorithms
  const bubbleSort = (arr: number[]): SortingStep[] => {
    const steps: SortingStep[] = [];
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;

    steps.push({ array: [...array] });

    for (let i = 0; i < array.length - 1; i++) {
      for (let j = 0; j < array.length - i - 1; j++) {
        comparisons++;
        steps.push({
          array: [...array],
          comparing: [j, j + 1],
        });

        if (array[j] > array[j + 1]) {
          [array[j], array[j + 1]] = [array[j + 1], array[j]];
          swaps++;
          steps.push({
            array: [...array],
            swapping: [j, j + 1],
          });
        }
      }
      steps.push({
        array: [...array],
        sorted: Array.from({ length: i + 1 }, (_, k) => array.length - 1 - k),
      });
    }

    steps.push({
      array: [...array],
      sorted: Array.from({ length: array.length }, (_, i) => i),
    });

    setStats({ comparisons, swaps, operations: comparisons + swaps });
    return steps;
  };

  const quickSort = (arr: number[]): SortingStep[] => {
    const steps: SortingStep[] = [];
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;

    const partition = (low: number, high: number): number => {
      const pivot = array[high];
      let i = low - 1;

      steps.push({
        array: [...array],
        pivotIndex: high,
      });

      for (let j = low; j < high; j++) {
        comparisons++;
        steps.push({
          array: [...array],
          comparing: [j, high],
          pivotIndex: high,
        });

        if (array[j] < pivot) {
          i++;
          if (i !== j) {
            [array[i], array[j]] = [array[j], array[i]];
            swaps++;
            steps.push({
              array: [...array],
              swapping: [i, j],
              pivotIndex: high,
            });
          }
        }
      }

      [array[i + 1], array[high]] = [array[high], array[i + 1]];
      swaps++;
      steps.push({
        array: [...array],
        swapping: [i + 1, high],
      });

      return i + 1;
    };

    const quickSortHelper = (low: number, high: number) => {
      if (low < high) {
        const pi = partition(low, high);
        quickSortHelper(low, pi - 1);
        quickSortHelper(pi + 1, high);
      }
    };

    steps.push({ array: [...array] });
    quickSortHelper(0, array.length - 1);
    steps.push({
      array: [...array],
      sorted: Array.from({ length: array.length }, (_, i) => i),
    });

    setStats({ comparisons, swaps, operations: comparisons + swaps });
    return steps;
  };

  const insertionSort = (arr: number[]): SortingStep[] => {
    const steps: SortingStep[] = [];
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;

    steps.push({ array: [...array] });

    for (let i = 1; i < array.length; i++) {
      const key = array[i];
      let j = i - 1;

      steps.push({
        array: [...array],
        comparing: [i],
      });

      while (j >= 0 && array[j] > key) {
        comparisons++;
        steps.push({
          array: [...array],
          comparing: [j, j + 1],
        });

        array[j + 1] = array[j];
        swaps++;
        steps.push({
          array: [...array],
          swapping: [j, j + 1],
        });
        j--;
      }

      if (j >= 0) {
        comparisons++;
      }

      array[j + 1] = key;
      steps.push({
        array: [...array],
        sorted: Array.from({ length: i + 1 }, (_, k) => k),
      });
    }

    steps.push({
      array: [...array],
      sorted: Array.from({ length: array.length }, (_, i) => i),
    });

    setStats({ comparisons, swaps, operations: comparisons + swaps });
    return steps;
  };

  const selectionSort = (arr: number[]): SortingStep[] => {
    const steps: SortingStep[] = [];
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;

    steps.push({ array: [...array] });

    for (let i = 0; i < array.length - 1; i++) {
      let minIndex = i;

      steps.push({
        array: [...array],
        comparing: [i],
      });

      for (let j = i + 1; j < array.length; j++) {
        comparisons++;
        steps.push({
          array: [...array],
          comparing: [minIndex, j],
        });

        if (array[j] < array[minIndex]) {
          minIndex = j;
        }
      }

      if (minIndex !== i) {
        [array[i], array[minIndex]] = [array[minIndex], array[i]];
        swaps++;
        steps.push({
          array: [...array],
          swapping: [i, minIndex],
        });
      }

      steps.push({
        array: [...array],
        sorted: Array.from({ length: i + 1 }, (_, k) => k),
      });
    }

    steps.push({
      array: [...array],
      sorted: Array.from({ length: array.length }, (_, i) => i),
    });

    setStats({ comparisons, swaps, operations: comparisons + swaps });
    return steps;
  };

  // Tree algorithms
  const bstInsert = (root: TreeNode | null, value: number): TreeStep[] => {
    const steps: TreeStep[] = [];
    let operations = 0;

    const copyTree = (node: TreeNode | null): TreeNode | null => {
      if (!node) return null;
      return {
        value: node.value,
        left: copyTree(node.left || null),
        right: copyTree(node.right || null),
      };
    };

    let newRoot = copyTree(root);
    steps.push({ tree: copyTree(newRoot) });

    const insert = (node: TreeNode | null, val: number, path: number[] = []): TreeNode => {
      operations++;

      if (!node) {
        const newNode = { value: val };
        steps.push({
          tree: copyTree(newRoot),
          highlightedNodes: [val],
          visitedNodes: path,
        });
        return newNode;
      }

      steps.push({
        tree: copyTree(newRoot),
        currentNode: node.value,
        visitedNodes: path,
      });

      if (val < node.value) {
        node.left = insert(node.left || null, val, [...path, node.value]);
      } else {
        node.right = insert(node.right || null, val, [...path, node.value]);
      }

      return node;
    };

    if (newRoot) {
      insert(newRoot, value);
    } else {
      newRoot = { value };
    }

    steps.push({ tree: copyTree(newRoot), visitedNodes: [], highlightedNodes: [value] });

    setTree(newRoot);
    setOriginalTree(copyTree(newRoot));
    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  const bstInorder = (root: TreeNode | null): TreeStep[] => {
    const steps: TreeStep[] = [];
    const visited: number[] = [];
    let operations = 0;

    const copyTree = (node: TreeNode | null): TreeNode | null => {
      if (!node) return null;
      return {
        value: node.value,
        left: copyTree(node.left || null),
        right: copyTree(node.right || null),
      };
    };

    steps.push({ tree: copyTree(root), visitedNodes: [] });

    const traverse = (node: TreeNode | null) => {
      if (!node) return;

      operations++;
      // Показываем, что мы посещаем узел (красный)
      steps.push({
        tree: copyTree(root),
        currentNode: node.value,
        visitedNodes: [...visited],
      });

      // Обходим левое поддерево
      traverse(node.left || null);

      // Обрабатываем текущий узел (зеленый)
      steps.push({
        tree: copyTree(root),
        highlightedNodes: [node.value],
        visitedNodes: [...visited],
      });

      // Добавляем в посещенные (светло-зеленый)
      visited.push(node.value);
      steps.push({
        tree: copyTree(root),
        visitedNodes: [...visited],
      });

      // Обходим правое поддерево
      traverse(node.right || null);
    };

    traverse(root);
    steps.push({ tree: copyTree(root), visitedNodes: [...visited] });

    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  const bstPreorder = (root: TreeNode | null): TreeStep[] => {
    const steps: TreeStep[] = [];
    const visited: number[] = [];
    let operations = 0;

    const copyTree = (node: TreeNode | null): TreeNode | null => {
      if (!node) return null;
      return {
        value: node.value,
        left: copyTree(node.left || null),
        right: copyTree(node.right || null),
      };
    };

    steps.push({ tree: copyTree(root), visitedNodes: [] });

    const traverse = (node: TreeNode | null) => {
      if (!node) return;

      operations++;
      // Показываем, что мы посещаем узел (красный)
      steps.push({
        tree: copyTree(root),
        currentNode: node.value,
        visitedNodes: [...visited],
      });

      // Обрабатываем узел сразу (зеленый)
      steps.push({
        tree: copyTree(root),
        highlightedNodes: [node.value],
        visitedNodes: [...visited],
      });

      // Добавляем в посещенные (светло-зеленый)
      visited.push(node.value);
      steps.push({
        tree: copyTree(root),
        visitedNodes: [...visited],
      });

      // Обходим поддеревья
      traverse(node.left || null);
      traverse(node.right || null);
    };

    traverse(root);
    steps.push({ tree: copyTree(root), visitedNodes: [...visited] });

    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  const bstPostorder = (root: TreeNode | null): TreeStep[] => {
    const steps: TreeStep[] = [];
    const visited: number[] = [];
    let operations = 0;

    const copyTree = (node: TreeNode | null): TreeNode | null => {
      if (!node) return null;
      return {
        value: node.value,
        left: copyTree(node.left || null),
        right: copyTree(node.right || null),
      };
    };

    steps.push({ tree: copyTree(root), visitedNodes: [] });

    const traverse = (node: TreeNode | null) => {
      if (!node) return;

      operations++;
      // Показываем, что мы посещаем узел (красный)
      steps.push({
        tree: copyTree(root),
        currentNode: node.value,
        visitedNodes: [...visited],
      });

      // Обходим поддеревья
      traverse(node.left || null);
      traverse(node.right || null);

      // Обрабатываем узел после поддеревьев (зеленый)
      steps.push({
        tree: copyTree(root),
        highlightedNodes: [node.value],
        visitedNodes: [...visited],
      });

      // Добавляем в посещенные (светло-зеленый)
      visited.push(node.value);
      steps.push({
        tree: copyTree(root),
        visitedNodes: [...visited],
      });
    };

    traverse(root);
    steps.push({ tree: copyTree(root), visitedNodes: [...visited] });

    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  const bstLevelorder = (root: TreeNode | null): TreeStep[] => {
    const steps: TreeStep[] = [];
    const visited: number[] = [];
    let operations = 0;

    const copyTree = (node: TreeNode | null): TreeNode | null => {
      if (!node) return null;
      return {
        value: node.value,
        left: copyTree(node.left || null),
        right: copyTree(node.right || null),
      };
    };

    if (!root) {
      return steps;
    }

    steps.push({ tree: copyTree(root), visitedNodes: [] });

    const queue: TreeNode[] = [root];

    while (queue.length > 0) {
      const node = queue.shift()!;
      operations++;

      // Показываем, что мы посещаем узел (красный)
      steps.push({
        tree: copyTree(root),
        currentNode: node.value,
        visitedNodes: [...visited],
      });

      // Обрабатываем узел (зеленый)
      steps.push({
        tree: copyTree(root),
        highlightedNodes: [node.value],
        visitedNodes: [...visited],
      });

      // Добавляем в посещенные (светло-зеленый)
      visited.push(node.value);
      steps.push({
        tree: copyTree(root),
        visitedNodes: [...visited],
      });

      if (node.left) {
        queue.push(node.left);
      }
      if (node.right) {
        queue.push(node.right);
      }
    }

    steps.push({ tree: copyTree(root), visitedNodes: [...visited] });

    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  // Graph algorithms
  const graphBFS = (nodes: GraphNode[], edges: GraphEdge[], startNode: number = 0): GraphStep[] => {
    const steps: GraphStep[] = [];
    const visited = new Set<number>();
    const queue: number[] = [startNode];
    let operations = 0;

    steps.push({ nodes, edges });

    while (queue.length > 0) {
      const current = queue.shift()!;

      if (visited.has(current)) continue;

      operations++;
      visited.add(current);

      steps.push({
        nodes,
        edges,
        currentNode: current,
        visitedNodes: Array.from(visited),
      });

      // Find neighbors
      const neighbors = edges
        .filter(e => e.from === current || e.to === current)
        .map(e => e.from === current ? e.to : e.from)
        .filter(n => !visited.has(n));

      neighbors.forEach(neighbor => {
        if (!queue.includes(neighbor)) {
          queue.push(neighbor);
          steps.push({
            nodes,
            edges,
            currentNode: current,
            highlightedNodes: [neighbor],
            visitedNodes: Array.from(visited),
            highlightedEdges: [[current, neighbor]],
          });
        }
      });
    }

    steps.push({ nodes, edges, visitedNodes: Array.from(visited) });
    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  const graphDFS = (nodes: GraphNode[], edges: GraphEdge[], startNode: number = 0): GraphStep[] => {
    const steps: GraphStep[] = [];
    const visited = new Set<number>();
    let operations = 0;

    steps.push({ nodes, edges });

    const dfs = (current: number) => {
      operations++;
      visited.add(current);

      steps.push({
        nodes,
        edges,
        currentNode: current,
        visitedNodes: Array.from(visited),
      });

      const neighbors = edges
        .filter(e => e.from === current || e.to === current)
        .map(e => e.from === current ? e.to : e.from)
        .filter(n => !visited.has(n));

      neighbors.forEach(neighbor => {
        if (!visited.has(neighbor)) {
          steps.push({
            nodes,
            edges,
            currentNode: current,
            highlightedNodes: [neighbor],
            visitedNodes: Array.from(visited),
            highlightedEdges: [[current, neighbor]],
          });
          dfs(neighbor);
        }
      });
    };

    dfs(startNode);
    steps.push({ nodes, edges, visitedNodes: Array.from(visited) });
    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };
  const graphDijkstra = (nodes: GraphNode[], edges: GraphEdge[], startNode: number = 0): GraphStep[] => {
    const steps: GraphStep[] = [];
    const distances: number[] = new Array(nodes.length).fill(Infinity);
    const visited = new Set<number>();
    let operations = 0;

    distances[startNode] = 0;
    steps.push({ nodes, edges, currentNode: startNode });

    const priorityQueue: [number, number][] = [[0, startNode]]; // [distance, node]

    while (priorityQueue.length > 0) {
      priorityQueue.sort((a, b) => a[0] - b[0]);
      const [currentDistance, current] = priorityQueue.shift()!;

      if (visited.has(current)) continue;

      operations++;
      visited.add(current);

      steps.push({
        nodes,
        edges,
        currentNode: current,
        visitedNodes: Array.from(visited),
      });

      // Находим соседей
      const neighbors = edges
        .filter(e => e.from === current || (!directedGraph && e.to === current))
        .map(e => {
          const neighbor = e.from === current ? e.to : e.from;
          const weight = e.weight || 1;
          return { neighbor, weight };
        });

      for (const { neighbor, weight } of neighbors) {
        if (!visited.has(neighbor)) {
          const newDistance = currentDistance + weight;

          steps.push({
            nodes,
            edges,
            currentNode: current,
            highlightedNodes: [neighbor],
            visitedNodes: Array.from(visited),
            highlightedEdges: [[current, neighbor]],
          });

          if (newDistance < distances[neighbor]) {
            distances[neighbor] = newDistance;
            priorityQueue.push([newDistance, neighbor]);
          }
        }
      }
    }

    steps.push({
      nodes,
      edges,
      visitedNodes: Array.from(visited),
    });

    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  // List algorithms
  const listInsert = (nodes: ListNode[], value: number, position: number): ListStep[] => {
    const steps: ListStep[] = [];
    const newNodes = JSON.parse(JSON.stringify(nodes));
    let operations = 0;

    steps.push({ nodes: newNodes, type: listType });

    const newNode: ListNode = {
      value,
      next: null,
      prev: listType === 'doubly' ? null : undefined
    };

    if (position === 0) {
      operations++;
      newNode.next = 0;
      if (listType === 'doubly' && newNodes.length > 0) {
        newNodes[0].prev = newNodes.length;
      }
      newNodes.push(newNode);
      steps.push({
        nodes: newNodes,
        highlightedIndices: [newNodes.length - 1],
        head: newNodes.length - 1,
        type: listType
      });
    } else if (position >= newNodes.length) {
      operations++;
      if (newNodes.length > 0) {
        newNodes[newNodes.length - 1].next = newNodes.length;
        if (listType === 'doubly') {
          newNode.prev = newNodes.length - 1;
        }
      }
      newNodes.push(newNode);
      steps.push({
        nodes: newNodes,
        highlightedIndices: [newNodes.length - 1],
        tail: newNodes.length - 1,
        type: listType
      });
    } else {
      operations++;
      newNode.next = position;
      if (listType === 'doubly') {
        newNode.prev = position - 1;
        newNodes[position].prev = newNodes.length;
      }
      if (position > 0) {
        newNodes[position - 1].next = newNodes.length;
      }
      newNodes.push(newNode);
      steps.push({
        nodes: newNodes,
        highlightedIndices: [newNodes.length - 1],
        comparedIndices: [position - 1, position],
        type: listType
      });
    }

    setListNodes(newNodes);
    setOriginalListNodes(JSON.parse(JSON.stringify(newNodes)));
    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  const listDelete = (nodes: ListNode[], position: number): ListStep[] => {
    const steps: ListStep[] = [];
    let operations = 0;

    if (nodes.length === 0 || position >= nodes.length) {
      return steps;
    }

    steps.push({ nodes: JSON.parse(JSON.stringify(nodes)), type: listType });

    // Highlight the node to be deleted
    steps.push({
      nodes: JSON.parse(JSON.stringify(nodes)),
      highlightedIndices: [position],
      type: listType
    });

    operations++;
    const newNodes: ListNode[] = [];

    // Build new list without the deleted node
    for (let i = 0; i < nodes.length; i++) {
      if (i !== position) {
        const node: ListNode = {
          value: nodes[i].value,
          next: null,
          prev: listType === 'doubly' ? null : undefined
        };
        newNodes.push(node);
      }
    }

    // Reconnect the links
    for (let i = 0; i < newNodes.length; i++) {
      if (i < newNodes.length - 1) {
        newNodes[i].next = i + 1;
      }
      if (listType === 'doubly' && i > 0) {
        newNodes[i].prev = i - 1;
      }
    }

    steps.push({
      nodes: JSON.parse(JSON.stringify(newNodes)),
      type: listType
    });

    setListNodes(newNodes);
    setOriginalListNodes(JSON.parse(JSON.stringify(newNodes)));
    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  // Stack algorithms
  const stackPush = (items: number[], value: number): StackStep[] => {
    const steps: StackStep[] = [];
    const newItems = [...items, value];
    let operations = 1;

    steps.push({ items: [...items], top: items.length - 1 });
    steps.push({
      items: newItems,
      top: newItems.length - 1,
      highlightedIndex: newItems.length - 1,
      operation: 'push'
    });

    setStackItems(newItems);
    setOriginalStackItems([...newItems]);
    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  const stackPop = (items: number[]): StackStep[] => {
    const steps: StackStep[] = [];
    let operations = 1;

    if (items.length === 0) return steps;

    steps.push({ items: [...items], top: items.length - 1 });
    steps.push({
      items: [...items],
      top: items.length - 1,
      highlightedIndex: items.length - 1,
      operation: 'pop'
    });

    const newItems = items.slice(0, -1);
    steps.push({ items: newItems, top: newItems.length - 1 });

    setStackItems(newItems);
    setOriginalStackItems([...newItems]);
    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  // Queue algorithms
  const queueEnqueue = (items: number[], value: number): QueueStep[] => {
    const steps: QueueStep[] = [];
    const newItems = [...items, value];
    let operations = 1;

    steps.push({ items: [...items], front: 0, rear: items.length - 1 });
    steps.push({
      items: newItems,
      front: 0,
      rear: newItems.length - 1,
      highlightedIndex: newItems.length - 1,
      operation: 'enqueue'
    });

    setQueueItems(newItems);
    setOriginalQueueItems([...newItems]);
    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  const queueDequeue = (items: number[]): QueueStep[] => {
    const steps: QueueStep[] = [];
    let operations = 1;

    if (items.length === 0) return steps;

    steps.push({ items: [...items], front: 0, rear: items.length - 1 });
    steps.push({
      items: [...items],
      front: 0,
      rear: items.length - 1,
      highlightedIndex: 0,
      operation: 'dequeue'
    });

    const newItems = items.slice(1);
    steps.push({ items: newItems, front: 0, rear: newItems.length - 1 });

    setQueueItems(newItems);
    setOriginalQueueItems([...newItems]);
    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  // Heap algorithms
  const heapInsert = (nodes: HeapNode[], value: number): HeapStep[] => {
    const steps: HeapStep[] = [];
    const newNodes = [...nodes, { value, index: nodes.length }];
    let operations = 0;

    steps.push({ nodes: [...nodes], type: heapType });
    steps.push({
      nodes: newNodes,
      highlightedIndices: [newNodes.length - 1],
      type: heapType
    });

    // Bubble up
    let currentIndex = newNodes.length - 1;
    while (currentIndex > 0) {
      const parentIndex = Math.floor((currentIndex - 1) / 2);
      operations++;

      steps.push({
        nodes: newNodes,
        currentIndex,
        comparedIndices: [parentIndex],
        type: heapType
      });

      const shouldSwap = heapType === 'max'
        ? newNodes[currentIndex].value > newNodes[parentIndex].value
        : newNodes[currentIndex].value < newNodes[parentIndex].value;

      if (shouldSwap) {
        [newNodes[currentIndex], newNodes[parentIndex]] = [newNodes[parentIndex], newNodes[currentIndex]];
        steps.push({
          nodes: newNodes,
          highlightedIndices: [parentIndex, currentIndex],
          type: heapType
        });
        currentIndex = parentIndex;
      } else {
        break;
      }
    }

    steps.push({ nodes: newNodes, type: heapType });

    setHeapNodes(newNodes);
    setOriginalHeapNodes(JSON.parse(JSON.stringify(newNodes)));
    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  // Hash Table algorithms
  const hashTableInsert = (buckets: (HashBucket | null)[], key: string, value: number): HashTableStep[] => {
    const steps: HashTableStep[] = [];
    const newBuckets = [...buckets];
    let operations = 1;

    const hash = key.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0) % hashTableSize;

    steps.push({ buckets: newBuckets, size: hashTableSize });
    steps.push({
      buckets: newBuckets,
      size: hashTableSize,
      currentBucket: hash
    });

    if (newBuckets[hash]) {
      steps.push({
        buckets: newBuckets,
        size: hashTableSize,
        collisionBuckets: [hash]
      });
    }

    newBuckets[hash] = { key, value, hash };
    steps.push({
      buckets: newBuckets,
      size: hashTableSize,
      highlightedBuckets: [hash]
    });

    setHashBuckets(newBuckets);
    setOriginalHashBuckets(JSON.parse(JSON.stringify(newBuckets)));
    setStats({ comparisons: 0, swaps: 0, operations });
    return steps;
  };

  const runAlgorithm = () => {
    let algorithmSteps: VisualizationStep[] = [];

    if (dataStructure === 'array') {
      switch (algorithm) {
        case 'bubblesort':
          algorithmSteps = bubbleSort(originalArray);
          break;
        case 'quicksort':
          algorithmSteps = quickSort(originalArray);
          break;
        case 'insertionsort':
          algorithmSteps = insertionSort(originalArray);
          break;
        case 'selectionsort':
          algorithmSteps = selectionSort(originalArray);
          break;
        default:
          algorithmSteps = bubbleSort(originalArray);
      }
    } else if (dataStructure === 'tree') {
      switch (algorithm) {
        case 'bst.inorder':
          algorithmSteps = bstInorder(originalTree);
          break;
        case 'bst.preorder':
          algorithmSteps = bstPreorder(originalTree);
          break;
        case 'bst.postorder':
          algorithmSteps = bstPostorder(originalTree);
          break;
        case 'bst.levelorder':
          algorithmSteps = bstLevelorder(originalTree);
          break;
        default:
          algorithmSteps = bstInorder(originalTree);
      }
    } else if (dataStructure === 'graph') {
      switch (algorithm) {
        case 'bfs':
          algorithmSteps = graphBFS(originalGraphNodes, originalGraphEdges);
          break;
        case 'dfs':
          algorithmSteps = graphDFS(originalGraphNodes, originalGraphEdges);
          break;
        case 'dijkstra':
          algorithmSteps = graphDijkstra(originalGraphNodes, originalGraphEdges);
          break;
        default:
          algorithmSteps = graphBFS(originalGraphNodes, originalGraphEdges);
      }
    }

    setSteps(algorithmSteps);
    setCurrentStep(0);
  };

  const handleInsertValue = () => {
    const value = parseInt(insertValue);
    if (!isNaN(value)) {
      const insertSteps = bstInsert(tree, value);
      setSteps(insertSteps);
      setCurrentStep(0);
      setInsertValue('');
      setIsPlaying(true);
    }
  };

  const handleListInsert = () => {
    const value = parseInt(listValue);
    const position = listPosition === '' ? listNodes.length : parseInt(listPosition);

    if (!isNaN(value) && !isNaN(position) && position >= 0) {
      const insertSteps = listInsert(listNodes, value, position);
      setSteps(insertSteps);
      setCurrentStep(0);
      setListValue('');
      setListPosition('');
      setIsPlaying(true);
    }
  };

  const handleListDelete = () => {
    const position = listPosition === '' ? (listNodes.length > 0 ? listNodes.length - 1 : 0) : parseInt(listPosition);

    if (!isNaN(position) && position >= 0 && position < listNodes.length) {
      const deleteSteps = listDelete(listNodes, position);
      setSteps(deleteSteps);
      setCurrentStep(0);
      setListPosition('');
      setIsPlaying(true);
    }
  };

  const handleStackPush = () => {
    const value = parseInt(stackValue);
    if (!isNaN(value)) {
      const pushSteps = stackPush(stackItems, value);
      setSteps(pushSteps);
      setCurrentStep(0);
      setStackValue('');
      setIsPlaying(true);
    }
  };

  const handleStackPop = () => {
    if (stackItems.length > 0) {
      const popSteps = stackPop(stackItems);
      setSteps(popSteps);
      setCurrentStep(0);
      setIsPlaying(true);
    }
  };

  const handleQueueEnqueue = () => {
    const value = parseInt(queueValue);
    if (!isNaN(value)) {
      const enqueueSteps = queueEnqueue(queueItems, value);
      setSteps(enqueueSteps);
      setCurrentStep(0);
      setQueueValue('');
      setIsPlaying(true);
    }
  };

  const handleQueueDequeue = () => {
    if (queueItems.length > 0) {
      const dequeueSteps = queueDequeue(queueItems);
      setSteps(dequeueSteps);
      setCurrentStep(0);
      setIsPlaying(true);
    }
  };

  const handleHeapInsert = () => {
    const value = parseInt(heapValue);
    if (!isNaN(value)) {
      const insertSteps = heapInsert(heapNodes, value);
      setSteps(insertSteps);
      setCurrentStep(0);
      setHeapValue('');
      setIsPlaying(true);
    }
  };

  const handleHashInsert = () => {
    const key = hashKey.trim();
    const value = parseInt(hashValue);
    if (key && !isNaN(value)) {
      const insertSteps = hashTableInsert(hashBuckets, key, value);
      setSteps(insertSteps);
      setCurrentStep(0);
      setHashKey('');
      setHashValue('');
      setIsPlaying(true);
    }
  };

  useEffect(() => {
    if (isPlaying && steps.length > 0) {
      const timer = setTimeout(() => {
        if (currentStep < steps.length - 1) {
          setCurrentStep(currentStep + 1);
        } else {
          setIsPlaying(false);
        }
      }, 1000 / speed);

      return () => clearTimeout(timer);
    }
  }, [isPlaying, currentStep, steps.length, speed]);

  const handlePlay = () => {
    if (steps.length === 0) {
      runAlgorithm();
    }
    setIsPlaying(true);
  };

  const handlePause = () => {
    setIsPlaying(false);
  };

  const handleStepForward = () => {
    if (steps.length === 0) {
      runAlgorithm();
      return;
    }
    if (currentStep < steps.length - 1) {
      setCurrentStep(currentStep + 1);
    }
  };

  const handleStepBackward = () => {
    if (currentStep > 0) {
      setCurrentStep(currentStep - 1);
    }
  };

  const handleReset = () => {
    setCurrentStep(0);
    setIsPlaying(false);

    if (dataStructure === 'array') {
      setArray([...originalArray]);
    } else if (dataStructure === 'tree') {
      setTree(JSON.parse(JSON.stringify(originalTree)));
    } else if (dataStructure === 'graph') {
      setGraphNodes([...originalGraphNodes]);
      setGraphEdges([...originalGraphEdges]);
    } else if (dataStructure === 'list') {
      setListNodes(JSON.parse(JSON.stringify(originalListNodes)));
    } else if (dataStructure === 'stack') {
      setStackItems([...originalStackItems]);
    } else if (dataStructure === 'queue') {
      setQueueItems([...originalQueueItems]);
    } else if (dataStructure === 'heap') {
      setHeapNodes(JSON.parse(JSON.stringify(originalHeapNodes)));
    } else if (dataStructure === 'hashtable') {
      setHashBuckets(JSON.parse(JSON.stringify(originalHashBuckets)));
    }

    setSteps([]);
  };

  const renderVisualization = () => {
    const currentStepData = steps[currentStep];

    if (dataStructure === 'array') {
      const stepData = (currentStepData as SortingStep) || {
        array,
        comparing: [],
        swapping: [],
        sorted: []
      };
      return <ArrayVisualization {...stepData} />;
    } else if (dataStructure === 'tree') {
      const stepData = (currentStepData as TreeStep) || {
        tree,
        currentNode: undefined,
        highlightedNodes: [],
        visitedNodes: [],
      };
      return (
        <div className="space-y-4">
          <TreeVisualization {...stepData} />
          {stepData.visitedNodes && stepData.visitedNodes.length > 0 && (
            <Card className="bg-card/50">
              <CardContent className="pt-4">
                <div className="text-sm">
                  <span className="text-muted-foreground">Порядок обхода: </span>
                  <span className="font-mono">
                    {stepData.visitedNodes.map((value, index) => (
                      <span
                        key={index}
                        className={
                          stepData.currentNode === value ? 'text-red-500 font-bold' :
                            stepData.highlightedNodes?.includes(value) ? 'text-green-500 font-bold' :
                              'text-green-400'
                        }
                      >
                        {value}{index < stepData.visitedNodes!.length - 1 ? ' → ' : ''}
                      </span>
                    ))}
                  </span>
                </div>
              </CardContent>
            </Card>
          )}
        </div>
      );
    } else if (dataStructure === 'graph') {
      let stepData: GraphStep;

      if (currentStepData) {
        // Есть шаги анимации - используем данные из шага
        stepData = currentStepData as GraphStep;
      } else {
        // Нет шагов - создаем базовые данные с серыми узлами
        stepData = {
          nodes: graphNodes,
          edges: graphEdges,
          currentNode: undefined,
          highlightedNodes: [],
          visitedNodes: [], // Пустой массив = все узлы не посещены (серые)
          highlightedEdges: [],
        };
      }

      return (
        <GraphVisualization
          {...stepData}
          directed={directedGraph}
          graphType={graphType}
        />
      );
    } else if (dataStructure === 'list') {
      const stepData = (currentStepData as ListStep) || {
        nodes: listNodes,
        head: 0,
        tail: listNodes.length > 0 ? listNodes.length - 1 : null,
        type: listType,
      };
      return <ListVisualization {...stepData} />;
    } else if (dataStructure === 'stack') {
      const stepData = (currentStepData as StackStep) || {
        items: stackItems,
        top: stackItems.length > 0 ? stackItems.length - 1 : undefined,
      };
      return <StackVisualization {...stepData} />;
    } else if (dataStructure === 'queue') {
      const stepData = (currentStepData as QueueStep) || {
        items: queueItems,
        front: 0,
        rear: queueItems.length > 0 ? queueItems.length - 1 : undefined,
      };
      return <QueueVisualization {...stepData} />;
    }
  };

  const getAlgorithmOptions = () => {
    if (dataStructure === 'array') {
      return (
        <>
          <SelectItem value="bubblesort">{translations['algorithm.bubblesort']}</SelectItem>
          <SelectItem value="quicksort">{translations['algorithm.quicksort']}</SelectItem>
          <SelectItem value="insertionsort">{translations['algorithm.insertionsort']}</SelectItem>
          <SelectItem value="selectionsort">{translations['algorithm.selectionsort']}</SelectItem>
        </>
      );
    } else if (dataStructure === 'tree') {
      return (
        <>
          <SelectItem value="bst.inorder">{translations['algorithm.bst.inorder']}</SelectItem>
          <SelectItem value="bst.preorder">{translations['algorithm.bst.preorder']}</SelectItem>
          <SelectItem value="bst.postorder">{translations['algorithm.bst.postorder']}</SelectItem>
          <SelectItem value="bst.levelorder">{translations['algorithm.bst.levelorder']}</SelectItem>
        </>
      );
    } else if (dataStructure === 'graph') {
      return (
        <>
          <SelectItem value="bfs">{translations['algorithm.bfs']}</SelectItem>
          <SelectItem value="dfs">{translations['algorithm.dfs']}</SelectItem>
        </>
      );
    }
    return null;
  };

  const getDataSize = () => {
    if (dataStructure === 'array') return arraySize;
    if (dataStructure === 'graph') return nodeCount;
    return undefined;
  };

  return (
    <div className="space-y-6">
      <div className="grid lg:grid-cols-5 gap-6">
        <Card className="lg:col-span-1">
          <CardHeader>
            <CardTitle>{translations['structure.select']}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <label className="text-sm">{translations['structure.select']}</label>
              <Select value={dataStructure} onValueChange={(v) => setDataStructure(v as any)}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="array">{translations['structure.array']}</SelectItem>
                  <SelectItem value="tree">{translations['structure.tree']}</SelectItem>
                  <SelectItem value="graph">{translations['structure.graph']}</SelectItem>
                  <SelectItem value="list">{translations['structure.list']}</SelectItem>
                  <SelectItem value="stack">{translations['structure.stack']}</SelectItem>
                  <SelectItem value="queue">{translations['structure.queue']}</SelectItem>
                </SelectContent>
              </Select>
            </div>

            {(dataStructure === 'array' || dataStructure === 'tree' || dataStructure === 'graph') && (
              <div className="space-y-2">
                <label className="text-sm">{translations['algorithm.select']}</label>
                <Select value={algorithm} onValueChange={setAlgorithm}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {getAlgorithmOptions()}
                  </SelectContent>
                </Select>
              </div>
            )}

            {dataStructure === 'array' && (
              <>
                <div className="space-y-2">
                  <label className="text-sm">{translations['data.size']}: {arraySize}</label>
                  <Slider
                    value={[arraySize]}
                    onValueChange={(value) => setArraySize(value[0])}
                    max={35}
                    min={5}
                    step={1}
                  />
                </div>
                <Button onClick={generateRandomArray} variant="outline" className="w-full">
                  {translations['data.generate']}
                </Button>
              </>
            )}

            {dataStructure === 'tree' && (
              <>
                <Button onClick={generateRandomTree} variant="outline" className="w-full">
                  {translations['data.generate']}
                </Button>
                <div className="flex space-x-2">
                  <Input
                    type="number"
                    placeholder={translations['data.value']}
                    value={insertValue}
                    onChange={(e) => setInsertValue(e.target.value)}
                  />
                  <Button onClick={handleInsertValue}>
                    {translations['data.insert']}
                  </Button>
                </div>
              </>
            )}

      {dataStructure === 'graph' && (
  <>
    <div className="space-y-2">
      <label className="text-sm">Тип графа</label>
      <Select value={graphType} onValueChange={(v: 'circular' | 'grid' | 'complete' | 'random') => {
        setGraphType(v);
        generateRandomGraph(v);
      }}>
        <SelectTrigger>
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="circular">Круговой (замкнутый)</SelectItem>
          <SelectItem value="grid">Сетка</SelectItem>
          <SelectItem value="complete">Полный граф</SelectItem>
          <SelectItem value="random">Случайный граф</SelectItem>
        </SelectContent>
      </Select>
    </div>
    
    <div className="space-y-2">
      <label className="text-sm">Направление</label>
      <Select value={directedGraph ? 'directed' : 'undirected'} 
              onValueChange={(v) => setDirectedGraph(v === 'directed')}>
        <SelectTrigger>
          <SelectValue />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="undirected">Неориентированный</SelectItem>
          <SelectItem value="directed">Ориентированный</SelectItem>
        </SelectContent>
      </Select>
    </div>
    
    <div className="space-y-2">
      <label className="text-sm">{translations['data.size']}: {nodeCount}</label>
      <Slider
        value={[nodeCount]}
        onValueChange={(value) => setNodeCount(value[0])}
        max={8}
        min={4}
        step={1}
      />
    </div>
    
    <Button onClick={() => generateRandomGraph()} variant="outline" className="w-full">
      {translations['data.generate']}
    </Button>
  </>
)}

            {dataStructure === 'list' && (
              <>
                <div className="space-y-2">
                  <label className="text-sm">{translations['list.type'] || 'Type'}</label>
                  <Select value={listType} onValueChange={(v) => setListType(v as 'singly' | 'doubly')}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="singly">{translations['list.singly'] || 'Singly Linked'}</SelectItem>
                      <SelectItem value="doubly">{translations['list.doubly'] || 'Doubly Linked'}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <label className="text-sm">{translations['data.size']}: {listSize}</label>
                  <Slider
                    value={[listSize]}
                    onValueChange={(value) => setListSize(value[0])}
                    max={8}
                    min={2}
                    step={1}
                  />
                </div>
                <Button onClick={generateRandomList} variant="outline" className="w-full">
                  {translations['data.generate']}
                </Button>
                <div className="space-y-2">
                  <Input
                    type="number"
                    placeholder={translations['data.value']}
                    value={listValue}
                    onChange={(e) => setListValue(e.target.value)}
                  />
                  <Input
                    type="number"
                    placeholder={translations['list.position'] || 'Position (optional)'}
                    value={listPosition}
                    onChange={(e) => setListPosition(e.target.value)}
                  />
                  <div className="grid grid-cols-2 gap-2">
                    <Button onClick={handleListInsert} variant="outline" className="w-full">
                      {translations['data.insert']}
                    </Button>
                    <Button onClick={handleListDelete} variant="outline" className="w-full">
                      {translations['data.delete']}
                    </Button>
                  </div>
                </div>
              </>
            )}

            {dataStructure === 'stack' && (
              <>
                <div className="space-y-2">
                  <label className="text-sm">{translations['data.size']}: {stackSize}</label>
                  <Slider
                    value={[stackSize]}
                    onValueChange={(value) => setStackSize(value[0])}
                    max={5}
                    min={2}
                    step={1}
                  />
                </div>
                <Button onClick={generateRandomStack} variant="outline" className="w-full">
                  {translations['data.generate']}
                </Button>
                <div className="space-y-2">
                  <Input
                    type="number"
                    placeholder={translations['data.value']}
                    value={stackValue}
                    onChange={(e) => setStackValue(e.target.value)}
                  />
                  <div className="grid grid-cols-2 gap-2">
                    <Button onClick={handleStackPush} variant="outline" className="w-full">
                      Push
                    </Button>
                    <Button onClick={handleStackPop} variant="outline" className="w-full">
                      Pop
                    </Button>
                  </div>
                </div>
              </>
            )}

            {dataStructure === 'queue' && (
              <>
                <div className="space-y-2">
                  <label className="text-sm">{translations['data.size']}: {queueSize}</label>
                  <Slider
                    value={[queueSize]}
                    onValueChange={(value) => setQueueSize(value[0])}
                    max={10}
                    min={1}
                    step={1}
                  />
                </div>
                <Button onClick={generateRandomQueue} variant="outline" className="w-full">
                  {translations['data.generate']}
                </Button>
                <div className="space-y-2">
                  <Input
                    type="number"
                    placeholder={translations['data.value']}
                    value={queueValue}
                    onChange={(e) => setQueueValue(e.target.value)}
                  />
                  <div className="grid grid-cols-2 gap-2">
                    <Button onClick={handleQueueEnqueue} variant="outline" className="w-full">
                      Enqueue
                    </Button>
                    <Button onClick={handleQueueDequeue} variant="outline" className="w-full">
                      Dequeue
                    </Button>
                  </div>
                </div>
              </>
            )}

            {dataStructure === 'heap' && (
              <>
                <div className="space-y-2">
                  <label className="text-sm">{translations['heap.type'] || 'Heap Type'}</label>
                  <Select value={heapType} onValueChange={(v) => setHeapType(v as 'max' | 'min')}>
                    <SelectTrigger>
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="max">{translations['heap.max'] || 'Max Heap'}</SelectItem>
                      <SelectItem value="min">{translations['heap.min'] || 'Min Heap'}</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
                <div className="space-y-2">
                  <label className="text-sm">{translations['data.size']}: {heapSize}</label>
                  <Slider
                    value={[heapSize]}
                    onValueChange={(value) => setHeapSize(value[0])}
                    max={15}
                    min={3}
                    step={1}
                  />
                </div>
                <Button onClick={generateRandomHeap} variant="outline" className="w-full">
                  {translations['data.generate']}
                </Button>
                <div className="space-y-2">
                  <Input
                    type="number"
                    placeholder={translations['data.value']}
                    value={heapValue}
                    onChange={(e) => setHeapValue(e.target.value)}
                  />
                  <Button onClick={handleHeapInsert} variant="outline" className="w-full">
                    {translations['data.insert']}
                  </Button>
                </div>
              </>
            )}

            {dataStructure === 'hashtable' && (
              <>
                <div className="space-y-2">
                  <label className="text-sm">{translations['hashtable.size'] || 'Table Size'}: {hashTableSize}</label>
                  <Slider
                    value={[hashTableSize]}
                    onValueChange={(value) => setHashTableSize(value[0])}
                    max={20}
                    min={5}
                    step={1}
                  />
                </div>
                <Button onClick={generateRandomHashTable} variant="outline" className="w-full">
                  {translations['data.generate']}
                </Button>
                <div className="space-y-2">
                  <Input
                    type="text"
                    placeholder={translations['hashtable.key'] || 'Key'}
                    value={hashKey}
                    onChange={(e) => setHashKey(e.target.value)}
                  />
                  <Input
                    type="number"
                    placeholder={translations['data.value']}
                    value={hashValue}
                    onChange={(e) => setHashValue(e.target.value)}
                  />
                  <Button onClick={handleHashInsert} variant="outline" className="w-full">
                    {translations['data.insert']}
                  </Button>
                </div>
              </>
            )}

          </CardContent>
        </Card>

        <div className="lg:col-span-3 space-y-6">
          {renderVisualization()}

          <AnimationControls
            isPlaying={isPlaying}
            onPlay={handlePlay}
            onPause={handlePause}
            onStepForward={handleStepForward}
            onStepBackward={handleStepBackward}
            onReset={handleReset}
            speed={speed}
            onSpeedChange={setSpeed}
            hidePlayButton={!(dataStructure === 'array' || dataStructure === 'tree' || dataStructure === 'graph')}
          />

          {steps.length > 0 && (
            <div className="text-center text-sm text-muted-foreground">
              Шаг {currentStep + 1} из {steps.length}
            </div>
          )}
        </div>

        <div className="lg:col-span-1">
          <StatsPanel
            dataStructure={dataStructure}
            algorithm={algorithm}
            stats={stats}
            dataSize={getDataSize()}
          />
        </div>
      </div>
    </div>
  );
}
