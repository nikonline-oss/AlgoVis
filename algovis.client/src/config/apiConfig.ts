// API endpoints configuration
export const API_CONFIG = {
    baseUrl: 'http://localhost:5266/api',

    endpoints: {
        // Генерация структур данных
        generate: {
            array: '/structuregenerator/generate/array',
            tree: '/structuregenerator/generate/binarytree',
            graph: '/structuregenerator/generate/graph',
            list: '/structuregenerator/generate/list',
            stack: '/structuregenerator/generate/stack',
            queue: '/structuregenerator/generate/queue'
        },

        // Алгоритмы
        algorithms: {
            // Сортировки
            bubbleSort: '/algorithms/array/BubbleSort',
            quickSort: '/algorithms/array/QuickSort',
            insertionSort: '/algorithms/array/InsertionSort',
            selectionSort: '/algorithms/array/SelectionSort',

            // Обходы деревьев
            inorder: '/algorithms/tree/inorder',
            preorder: '/algorithms/tree/preorder',
            postorder: '/algorithms/tree/postorder',
            levelorder: '/algorithms/tree/levelorder',

            // Алгоритмы на графах
            bfs: '/algorithms/graph/bfs',
            dfs: '/algorithms/graph/dfs',
            dijkstra: '/algorithms/graph/dijkstra'
        }
    }
};