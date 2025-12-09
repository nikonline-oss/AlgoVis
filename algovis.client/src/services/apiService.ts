// services/apiService.ts
import { API_CONFIG } from '../config/apiConfig';
import type {
    ApiResponse,
    AlgorithmStepsResponse,
    DataStructureResponse,
    ArrayStep,
    TreeStep,
    GraphStep
} from '../types/interfaces';

class ApiService {
    private baseUrl: string;

    constructor() {
        this.baseUrl = API_CONFIG.baseUrl;
    }

    private async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
        try {
            console.log('Making request to:', `${this.baseUrl}${endpoint}`);
            console.log('Request options:', options);

            const response = await fetch(`${this.baseUrl}${endpoint}`, {
                headers: {
                    'Content-Type': 'application/json',
                    ...options.headers,
                },
                ...options,
            });

            console.log('Response status:', response.status);

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result = await response.json();
            console.log('API Response:', result);

            if (!result.success) {
                throw new Error(result.error || 'Unknown API error');
            }

            return result.data;
        } catch (error) {
            console.error('API request failed:', error);
            throw error;
        }
    }

    // Генерация структур данных
    async generateArray(size: number = 20): Promise<number[]> {
        const data = await this.request<number[]>(`${API_CONFIG.endpoints.generate.array}?size=${size}`);
        return Array.isArray(data) ? data : [];
    }

    async generateTree(): Promise<any> {
        return this.request(API_CONFIG.endpoints.generate.tree);
    }

    async generateGraph(type: string, nodeCount: number): Promise<{ nodes: any[]; edges: any[] }> {
        return this.request(
            `${API_CONFIG.endpoints.generate.graph}?type=${type}&nodeCount=${nodeCount}`
        );
    }

    // Алгоритмы сортировки
    async bubbleSort(array: number[]): Promise<AlgorithmStepsResponse> {
        return this.request<AlgorithmStepsResponse>(API_CONFIG.endpoints.algorithms.bubbleSort, {
            method: 'POST',
            body: JSON.stringify({ data: array }),
        });
    }

    async quickSort(array: number[]): Promise<AlgorithmStepsResponse> {
        return this.request<AlgorithmStepsResponse>(API_CONFIG.endpoints.algorithms.quickSort, {
            method: 'POST',
            body: JSON.stringify({ data: array }),
        });
    }

    async insertionSort(array: number[]): Promise<AlgorithmStepsResponse> {
        return this.request<AlgorithmStepsResponse>(API_CONFIG.endpoints.algorithms.insertionSort, {
            method: 'POST',
            body: JSON.stringify({ data: array }),
        });
    }

    async selectionSort(array: number[]): Promise<AlgorithmStepsResponse> {
        return this.request<AlgorithmStepsResponse>(API_CONFIG.endpoints.algorithms.selectionSort, {
            method: 'POST',
            body: JSON.stringify({ data: array }),
        });
    }

    // Алгоритмы на деревьях
    async inorderTraversal(tree: any): Promise<AlgorithmStepsResponse> {
        return this.request<AlgorithmStepsResponse>(API_CONFIG.endpoints.algorithms.inorder, {
            method: 'POST',
            body: JSON.stringify({ tree }),
        });
    }

    async preorderTraversal(tree: any): Promise<AlgorithmStepsResponse> {
        return this.request<AlgorithmStepsResponse>(API_CONFIG.endpoints.algorithms.preorder, {
            method: 'POST',
            body: JSON.stringify({ tree }),
        });
    }

    async postorderTraversal(tree: any): Promise<AlgorithmStepsResponse> {
        return this.request<AlgorithmStepsResponse>(API_CONFIG.endpoints.algorithms.postorder, {
            method: 'POST',
            body: JSON.stringify({ tree }),
        });
    }

    async levelorderTraversal(tree: any): Promise<AlgorithmStepsResponse> {
        return this.request<AlgorithmStepsResponse>(API_CONFIG.endpoints.algorithms.levelorder, {
            method: 'POST',
            body: JSON.stringify({ tree }),
        });
    }

    // Алгоритмы на графах
    async bfs(nodes: any[], edges: any[], startNode: number = 0): Promise<AlgorithmStepsResponse> {
        return this.request<AlgorithmStepsResponse>(API_CONFIG.endpoints.algorithms.bfs, {
            method: 'POST',
            body: JSON.stringify({ nodes, edges, startNode }),
        });
    }

    async dfs(nodes: any[], edges: any[], startNode: number = 0): Promise<AlgorithmStepsResponse> {
        return this.request<AlgorithmStepsResponse>(API_CONFIG.endpoints.algorithms.dfs, {
            method: 'POST',
            body: JSON.stringify({ nodes, edges, startNode }),
        });
    }

    async dijkstra(nodes: any[], edges: any[], startNode: number = 0): Promise<AlgorithmStepsResponse> {
        return this.request<AlgorithmStepsResponse>(API_CONFIG.endpoints.algorithms.dijkstra, {
            method: 'POST',
            body: JSON.stringify({ nodes, edges, startNode }),
        });
    }
}

export const apiService = new ApiService();