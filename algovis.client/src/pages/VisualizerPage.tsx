import React, { useState, useEffect, useCallback } from 'react';
import { Button } from '../components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { Slider } from '../components/ui/slider';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Input } from '../components/ui/input';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';
import { AnimationControls } from '../components/AnimationControls';
import { ArrayVisualization } from '../components/ArrayVisualization';
import { TreeVisualization, TreeNode } from '../components/TreeVisualization';
import { GraphVisualization, GraphNode, GraphEdge } from '../components/GraphVisualization';
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

type VisualizationStep = SortingStep | TreeStep | GraphStep;

export function VisualizerPage() {
  const { translations, sharedData, setSharedData } = useApp();
  const [dataStructure, setDataStructure] = useState<'array' | 'tree' | 'graph'>('array');
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

  useEffect(() => {
    if (dataStructure === 'array') {
      generateRandomArray();
    } else if (dataStructure === 'tree') {
      generateRandomTree();
    } else if (dataStructure === 'graph') {
      generateRandomGraph();
    }
  }, [dataStructure, generateRandomArray, generateRandomTree, generateRandomGraph]);

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
            <Tabs value={dataStructure} onValueChange={(v) => setDataStructure(v as any)}>
              <TabsList className="grid w-full grid-cols-3">
                <TabsTrigger value="array">{translations['structure.array']}</TabsTrigger>
                <TabsTrigger value="tree">{translations['structure.tree']}</TabsTrigger>
                <TabsTrigger value="graph">{translations['structure.graph']}</TabsTrigger>
              </TabsList>
            </Tabs>

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
