// hooks/useAlgorithmApi.ts
import { useState, useCallback } from 'react';
import { apiService } from '../services/apiService';
import type { VisualizationStep, ArrayStep, TreeStep, GraphStep, AlgorithmStepsResponse } from '../types/interfaces';

export const useAlgorithmApi = () => {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const runAlgorithm = useCallback(async (
        dataStructure: string,
        algorithm: string,
        data: any,
        options: any = {}
    ): Promise<AlgorithmStepsResponse> => {
        setLoading(true);
        setError(null);

        try {
            let result;

            switch (dataStructure) {
                case 'array':
                    switch (algorithm) {
                        case 'bubblesort':
                            result = await apiService.bubbleSort(data);
                            break;
                        case 'quicksort':
                            result = await apiService.quickSort(data);
                            break;
                        case 'insertionsort':
                            result = await apiService.insertionSort(data);
                            break;
                        case 'selectionsort':
                            result = await apiService.selectionSort(data);
                            break;
                        default:
                            throw new Error(`Unsupported array algorithm: ${algorithm}`);
                    }
                    break;

                case 'tree':
                    switch (algorithm) {
                        case 'bst.inorder':
                            result = await apiService.inorderTraversal(data);
                            break;
                        case 'bst.preorder':
                            result = await apiService.preorderTraversal(data);
                            break;
                        case 'bst.postorder':
                            result = await apiService.postorderTraversal(data);
                            break;
                        case 'bst.levelorder':
                            result = await apiService.levelorderTraversal(data);
                            break;
                        default:
                            throw new Error(`Unsupported tree algorithm: ${algorithm}`);
                    }
                    break;

                case 'graph':
                    switch (algorithm) {
                        case 'bfs':
                            result = await apiService.bfs(data.nodes, data.edges, options.startNode || 0);
                            break;
                        case 'dfs':
                            result = await apiService.dfs(data.nodes, data.edges, options.startNode || 0);
                            break;
                        case 'dijkstra':
                            result = await apiService.dijkstra(data.nodes, data.edges, options.startNode || 0);
                            break;
                        default:
                            throw new Error(`Unsupported graph algorithm: ${algorithm}`);
                    }
                    break;

                default:
                    throw new Error(`Unsupported data structure: ${dataStructure}`);
            }

            console.log('Algorithm result:', result);

            // Преобразуем ответ бэкенда к формату фронтенда
            if (result && result.steps) {
                return {
                    steps: result.steps,
                    stats: result.statistics || {
                        comparisons: 0,
                        swaps: 0,
                        operations: 0,
                        timeComplexity: '',
                        spaceComplexity: ''
                    }
                };
            } else {
                throw new Error('Invalid response format from server');
            }
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Unknown error occurred';
            setError(errorMessage);
            console.error('API Error:', err);
            return null;
        } finally {
            setLoading(false);
        }
    }, []);

    const generateData = useCallback(async (type: string, options: any = {}) => {
        setLoading(true);
        setError(null);

        try {
            switch (type) {
                case 'array':
                    return await apiService.generateArray(options.size || 20);
                case 'tree':
                    return await apiService.generateTree();
                case 'graph':
                    return await apiService.generateGraph(options.graphType || 'circular', options.nodeCount || 6);
                default:
                    throw new Error(`Unsupported data structure: ${type}`);
            }
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Unknown error occurred';
            setError(errorMessage);
            console.error('API Error:', err);
            return null;
        } finally {
            setLoading(false);
        }
    }, []);

    return {
        loading,
        error,
        runAlgorithm,
        generateData,
        clearError: () => setError(null),
    };
};