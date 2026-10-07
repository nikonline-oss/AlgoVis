// common/interfaces.ts

// Базовые интерфейсы для всех структур данных
export interface VisualizationStep {
    step: number;
    description?: string;
    metadata?: Record<string, any>;
}

// Массив
export interface ArrayStep extends VisualizationStep {
    array: number[];
    comparing?: number[];
    swapping?: number[];
    sorted?: number[];
    pivotIndex?: number;
}

// Дерево
export interface TreeNode {
    value: number;
    left?: TreeNode | null;
    right?: TreeNode | null;
}

export interface TreeStep extends VisualizationStep {
    tree: TreeNode | null;
    currentNode?: number;
    highlightedNodes?: number[];
    visitedNodes?: number[];
}

// Граф
export interface GraphNode {
    id: number;
    x: number;
    y: number;
    label?: string;
}

export interface GraphEdge {
    from: number;
    to: number;
    weight?: number;
}

export interface GraphStep extends VisualizationStep {
    nodes: GraphNode[];
    edges: GraphEdge[];
    currentNode?: number;
    highlightedNodes?: number[];
    visitedNodes?: number[];
    highlightedEdges?: Array<[number, number]>;
}

// Ответы API
export interface ApiResponse<T> {
    success: boolean;
    data: T;
    error?: string;
}

export interface AlgorithmStepsResponse {
    steps: VisualizationStep[];
    stats: {
        comparisons: number;
        swaps: number;
        operations: number;
        timeComplexity: string;
        spaceComplexity: string;
    };
}

export interface DataStructureResponse {
    data: any; // Может быть массивом, деревом, графом и т.д.
    type: 'array' | 'tree' | 'graph' | 'list' | 'stack' | 'queue';
}