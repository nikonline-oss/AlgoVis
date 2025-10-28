import React, { useState, useEffect, useCallback } from 'react';
import { Button } from '../components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { Slider } from '../components/ui/slider';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Input } from '../components/ui/input';
import { AnimationControls } from '../components/AnimationControls';
import { ArrayVisualization } from '../components/ArrayVisualization';
import { TreeVisualization,type TreeNode } from '../components/TreeVisualization';
import { GraphVisualization,type GraphNode,type GraphEdge } from '../components/GraphVisualization';
import { ListVisualization,type ListNode } from '../components/ListVisualization';
import { StackVisualization } from '../components/StackVisualization';
import { QueueVisualization } from '../components/QueueVisualization';
import { HeapVisualization, type HeapNode } from '../components/HeapVisualization';
import { HashTableVisualization,type HashBucket } from '../components/HashTableVisualization';
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

interface HeapStep {
  nodes: HeapNode[];
  currentIndex?: number;
  highlightedIndices?: number[];
  comparedIndices?: number[];
  type?: 'max' | 'min';
}

interface HashTableStep {
  buckets: (HashBucket | null)[];
  size: number;
  currentBucket?: number;
  highlightedBuckets?: number[];
  collisionBuckets?: number[];
}

type VisualizationStep = SortingStep | TreeStep | GraphStep | ListStep | StackStep | QueueStep | HeapStep | HashTableStep;

export function VisualizerPage() {
  const { translations, sharedData, setSharedData } = useApp();
  const [dataStructure, setDataStructure] = useState<'array' | 'tree' | 'graph' | 'list' | 'stack' | 'queue' | 'heap' | 'hashtable'>('array');
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
  
  // List state
  const [listNodes, setListNodes] = useState<ListNode[]>([]);
  const [originalListNodes, setOriginalListNodes] = useState<ListNode[]>([]);
  const [listType, setListType] = useState<'singly' | 'doubly'>('singly');
  
  // Stack state
  const [stackItems, setStackItems] = useState<number[]>([]);
  const [originalStackItems, setOriginalStackItems] = useState<number[]>([]);
  
  // Queue state
  const [queueItems, setQueueItems] = useState<number[]>([]);
  const [originalQueueItems, setOriginalQueueItems] = useState<number[]>([]);
  
  // Heap state
  const [heapNodes, setHeapNodes] = useState<HeapNode[]>([]);
  const [originalHeapNodes, setOriginalHeapNodes] = useState<HeapNode[]>([]);
  const [heapType, setHeapType] = useState<'max' | 'min'>('max');
  
  // Hash Table state
  const [hashBuckets, setHashBuckets] = useState<(HashBucket | null)[]>([]);
  const [originalHashBuckets, setOriginalHashBuckets] = useState<(HashBucket | null)[]>([]);
  const [hashTableSize, setHashTableSize] = useState(10);
  
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

  const generateRandomGraph = useCallback(() => {
    const nodes: GraphNode[] = [];
    const edges: GraphEdge[] = [];
    
    // Create nodes in a circle
    const centerX = 400;
    const centerY = 250;
    const radius = 150;
    
    for (let i = 0; i < nodeCount; i++) {
      const angle = (i * 2 * Math.PI) / nodeCount;
      nodes.push({
        id: i,
        x: centerX + radius * Math.cos(angle),
        y: centerY + radius * Math.sin(angle),
      });
    }
    
    // Create random edges
    const edgeCount = Math.min(nodeCount * 2, nodeCount * (nodeCount - 1) / 2);
    const addedEdges = new Set<string>();
    
    while (edges.length < edgeCount) {
      const from = Math.floor(Math.random() * nodeCount);
      const to = Math.floor(Math.random() * nodeCount);
      
      if (from !== to) {
        const edgeKey = `${Math.min(from, to)}-${Math.max(from, to)}`;
        if (!addedEdges.has(edgeKey)) {
          addedEdges.add(edgeKey);
          edges.push({
            from,
            to,
            weight: Math.floor(Math.random() * 10) + 1,
          });
        }
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
  }, [nodeCount]);

  const generateRandomList = useCallback(() => {
    const size = 5;
    const nodes: ListNode[] = [];
    
    for (let i = 0; i < size; i++) {
      nodes.push({
        value: Math.floor(Math.random() * 100) + 1,
        next: i < size - 1 ? i + 1 : null,
        prev: listType === 'doubly' ? (i > 0 ? i - 1 : null) : undefined,
      });
    }
    
    setListNodes(nodes);
    setOriginalListNodes(JSON.parse(JSON.stringify(nodes)));
    setCurrentStep(0);
    setSteps([]);
    setStats({ comparisons: 0, swaps: 0, operations: 0 });
    setIsPlaying(false);
  }, [listType]);

  const generateRandomStack = useCallback(() => {
    const items = Array.from({ length: 5 }, () => Math.floor(Math.random() * 100) + 1);
    setStackItems(items);
    setOriginalStackItems([...items]);
    setCurrentStep(0);
    setSteps([]);
    setStats({ comparisons: 0, swaps: 0, operations: 0 });
    setIsPlaying(false);
  }, []);

  const generateRandomQueue = useCallback(() => {
    const items = Array.from({ length: 5 }, () => Math.floor(Math.random() * 100) + 1);
    setQueueItems(items);
    setOriginalQueueItems([...items]);
    setCurrentStep(0);
    setSteps([]);
    setStats({ comparisons: 0, swaps: 0, operations: 0 });
    setIsPlaying(false);
  }, []);

  const generateRandomHeap = useCallback(() => {
    const values = Array.from({ length: 10 }, () => Math.floor(Math.random() * 100) + 1);
    const nodes = values.map((value, index) => ({ value, index }));
    setHeapNodes(nodes);
    setOriginalHeapNodes(JSON.parse(JSON.stringify(nodes)));
    setCurrentStep(0);
    setSteps([]);
    setStats({ comparisons: 0, swaps: 0, operations: 0 });
    setIsPlaying(false);
  }, []);

  const generateRandomHashTable = useCallback(() => {
    const buckets: (HashBucket | null)[] = Array(hashTableSize).fill(null);
    const keys = ['key1', 'key2', 'key3', 'key4', 'key5'];
    
    keys.forEach((key, idx) => {
      const hash = key.split('').reduce((acc, char) => acc + char.charCodeAt(0), 0) % hashTableSize;
      if (!buckets[hash]) {
        buckets[hash] = {
          key,
          value: Math.floor(Math.random() * 100) + 1,
          hash,
        };
      }
    });
    
    setHashBuckets(buckets);
    setOriginalHashBuckets(JSON.parse(JSON.stringify(buckets)));
    setCurrentStep(0);
    setSteps([]);
    setStats({ comparisons: 0, swaps: 0, operations: 0 });
    setIsPlaying(false);
  }, [hashTableSize]);

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
      setAlgorithm('list.insert');
    } else if (dataStructure === 'stack') {
      generateRandomStack();
      setAlgorithm('stack.push');
    } else if (dataStructure === 'queue') {
      generateRandomQueue();
      setAlgorithm('queue.enqueue');
    } else if (dataStructure === 'heap') {
      generateRandomHeap();
      setAlgorithm('heap.insert');
    } else if (dataStructure === 'hashtable') {
      generateRandomHashTable();
      setAlgorithm('hashtable.insert');
    }
  }, [dataStructure, generateRandomArray, generateRandomTree, generateRandomGraph, 
      generateRandomList, generateRandomStack, generateRandomQueue, generateRandomHeap, generateRandomHashTable]);

  // Regenerate data when type or size changes
  useEffect(() => {
    if (dataStructure === 'list') {
      generateRandomList();
    }
  }, [listType, dataStructure, generateRandomList]);

  useEffect(() => {
    if (dataStructure === 'heap') {
      generateRandomHeap();
    }
  }, [heapType, dataStructure, generateRandomHeap]);

  useEffect(() => {
    if (dataStructure === 'hashtable') {
      generateRandomHashTable();
    }
  }, [hashTableSize, dataStructure, generateRandomHashTable]);

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
        default:
          algorithmSteps = graphBFS(originalGraphNodes, originalGraphEdges);
      }
    } else if (dataStructure === 'list') {
      const randomValue = Math.floor(Math.random() * 100) + 1;
      const randomPosition = Math.floor(Math.random() * (originalListNodes.length + 1));
      algorithmSteps = listInsert(originalListNodes, randomValue, randomPosition);
    } else if (dataStructure === 'stack') {
      if (algorithm === 'stack.push') {
        const randomValue = Math.floor(Math.random() * 100) + 1;
        algorithmSteps = stackPush(originalStackItems, randomValue);
      } else if (algorithm === 'stack.pop') {
        algorithmSteps = stackPop(originalStackItems);
      }
    } else if (dataStructure === 'queue') {
      if (algorithm === 'queue.enqueue') {
        const randomValue = Math.floor(Math.random() * 100) + 1;
        algorithmSteps = queueEnqueue(originalQueueItems, randomValue);
      } else if (algorithm === 'queue.dequeue') {
        algorithmSteps = queueDequeue(originalQueueItems);
      }
    } else if (dataStructure === 'heap') {
      const randomValue = Math.floor(Math.random() * 100) + 1;
      algorithmSteps = heapInsert(originalHeapNodes, randomValue);
    } else if (dataStructure === 'hashtable') {
      const randomKey = `key${Math.floor(Math.random() * 100)}`;
      const randomValue = Math.floor(Math.random() * 100) + 1;
      algorithmSteps = hashTableInsert(originalHashBuckets, randomKey, randomValue);
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
      const stepData = (currentStepData as GraphStep) || { 
        nodes: graphNodes, 
        edges: graphEdges,
        currentNode: undefined,
        highlightedNodes: [],
        visitedNodes: [],
        highlightedEdges: [],
      };
      return <GraphVisualization {...stepData} />;
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
    } else if (dataStructure === 'heap') {
      const stepData = (currentStepData as HeapStep) || {
        nodes: heapNodes,
        type: heapType,
      };
      return <HeapVisualization {...stepData} />;
    } else if (dataStructure === 'hashtable') {
      const stepData = (currentStepData as HashTableStep) || {
        buckets: hashBuckets,
        size: hashTableSize,
      };
      return <HashTableVisualization {...stepData} />;
    }
  };

  const getAlgorithmOptions = () => {
    if (dataStructure === 'array') {
      return (
        <>
          <SelectItem value="bubblesort">{translations['algorithm.bubblesort']}</SelectItem>
          <SelectItem value="quicksort">{translations['algorithm.quicksort']}</SelectItem>
          <SelectItem value="mergesort">{translations['algorithm.mergesort']}</SelectItem>
          <SelectItem value="heapsort">{translations['algorithm.heapsort']}</SelectItem>
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
          <SelectItem value="dijkstra">{translations['algorithm.dijkstra']}</SelectItem>
        </>
      );
    } else if (dataStructure === 'list') {
      return (
        <>
          <SelectItem value="list.insert">{translations['algorithm.list.insert']}</SelectItem>
          <SelectItem value="list.delete">{translations['algorithm.list.delete']}</SelectItem>
          <SelectItem value="list.search">{translations['algorithm.list.search']}</SelectItem>
        </>
      );
    } else if (dataStructure === 'stack') {
      return (
        <>
          <SelectItem value="stack.push">{translations['algorithm.stack.push']}</SelectItem>
          <SelectItem value="stack.pop">{translations['algorithm.stack.pop']}</SelectItem>
        </>
      );
    } else if (dataStructure === 'queue') {
      return (
        <>
          <SelectItem value="queue.enqueue">{translations['algorithm.queue.enqueue']}</SelectItem>
          <SelectItem value="queue.dequeue">{translations['algorithm.queue.dequeue']}</SelectItem>
        </>
      );
    } else if (dataStructure === 'heap') {
      return (
        <>
          <SelectItem value="heap.insert">{translations['algorithm.heap.insert']}</SelectItem>
          <SelectItem value="heap.extractMax">{translations['algorithm.heap.extractMax']}</SelectItem>
          <SelectItem value="heap.heapify">{translations['algorithm.heap.heapify']}</SelectItem>
        </>
      );
    } else if (dataStructure === 'hashtable') {
      return (
        <>
          <SelectItem value="hashtable.insert">{translations['algorithm.hashtable.insert']}</SelectItem>
          <SelectItem value="hashtable.search">{translations['algorithm.hashtable.search']}</SelectItem>
          <SelectItem value="hashtable.delete">{translations['algorithm.hashtable.delete']}</SelectItem>
        </>
      );
    }
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
                  <SelectItem value="heap">{translations['structure.heap']}</SelectItem>
                  <SelectItem value="hashtable">{translations['structure.hashtable']}</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <Select value={algorithm} onValueChange={setAlgorithm}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {getAlgorithmOptions()}
              </SelectContent>
            </Select>

            {dataStructure === 'array' && (
              <>
                <div className="space-y-2">
                  <label className="text-sm">{translations['data.size']}: {arraySize}</label>
                  <Slider
                    value={[arraySize]}
                    onValueChange={(value) => setArraySize(value[0])}
                    max={50}
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
                  <label className="text-sm">{translations['data.size']}: {nodeCount}</label>
                  <Slider
                    value={[nodeCount]}
                    onValueChange={(value) => setNodeCount(value[0])}
                    max={12}
                    min={4}
                    step={1}
                  />
                </div>
                <Button onClick={generateRandomGraph} variant="outline" className="w-full">
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
                <Button onClick={generateRandomList} variant="outline" className="w-full">
                  {translations['data.generate']}
                </Button>
              </>
            )}

            {dataStructure === 'stack' && (
              <Button onClick={generateRandomStack} variant="outline" className="w-full">
                {translations['data.generate']}
              </Button>
            )}

            {dataStructure === 'queue' && (
              <Button onClick={generateRandomQueue} variant="outline" className="w-full">
                {translations['data.generate']}
              </Button>
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
                <Button onClick={generateRandomHeap} variant="outline" className="w-full">
                  {translations['data.generate']}
                </Button>
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
