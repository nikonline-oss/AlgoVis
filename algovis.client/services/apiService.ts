// src/services/apiService.ts

export interface ApiResponse {
  success: boolean;
  data: any;
  timestamp: number;
}

class ApiService {
  private baseUrl: string | null = null;
  private useMock: boolean = true;

  // Переключение на реальный бекенд
  setBackendUrl(url: string) {
    this.baseUrl = url;
    this.useMock = false;
    console.log('✅ Переключено на реальный бекенд:', url);
  }

  // Переключение обратно на mock
  useMockBackend() {
    this.useMock = true;
    this.baseUrl = null;
    console.log('✅ Переключено на mock бекенд');
  }

  private async simulateNetworkDelay(min: number = 100, max: number = 500): Promise<void> {
    const delay = Math.random() * (max - min) + min;
    return new Promise(resolve => setTimeout(resolve, delay));
  }

  private async mockProfileAlgorithm(algorithm: string, data: any, config: any) {
    await this.simulateNetworkDelay();
    
    const size = data?.length || config?.dataSize || 1000;
    
    const performanceMap: Record<string, { time: number; operations: number; comparisons: number; swaps: number }> = {
      'bubbleSort': { 
        time: size * size * 0.001, 
        operations: (size * (size - 1)) / 2,
        comparisons: (size * (size - 1)) / 2,
        swaps: Math.floor((size * (size - 1)) / 4)
      },
      'quickSort': { 
        time: size * Math.log2(size) * 0.001, 
        operations: size * Math.log2(size),
        comparisons: Math.floor(size * Math.log2(size) * 0.7),
        swaps: Math.floor(size * Math.log2(size) * 0.3)
      },
      'mergeSort': { 
        time: size * Math.log2(size) * 0.0015, 
        operations: size * Math.log2(size) * 1.2,
        comparisons: Math.floor(size * Math.log2(size) * 0.8),
        swaps: Math.floor(size * Math.log2(size) * 0.4)
      },
      'insertionSort': { 
        time: size * size * 0.0005, 
        operations: (size * (size - 1)) / 2,
        comparisons: (size * (size - 1)) / 2,
        swaps: Math.floor((size * (size - 1)) / 3)
      },
      'bstInsert': {
        time: size * Math.log2(size) * 0.001,
        operations: size * Math.log2(size),
        comparisons: Math.floor(size * Math.log2(size) * 0.8),
        swaps: 0
      },
      'bstSearch': {
        time: size * Math.log2(size) * 0.0005,
        operations: size * Math.log2(size),
        comparisons: Math.floor(size * Math.log2(size) * 0.8),
        swaps: 0
      },
      'bstTraversal': {
        time: size * 0.001,
        operations: size,
        comparisons: 0,
        swaps: 0
      },
      'bfs': {
        time: size * 0.002,
        operations: size * 2,
        comparisons: size,
        swaps: 0
      },
      'dfs': {
        time: size * 0.002,
        operations: size * 2,
        comparisons: size,
        swaps: 0
      },
      'dijkstra': {
        time: size * size * 0.005,
        operations: size * size,
        comparisons: size * size,
        swaps: 0
      }
    };

    const perf = performanceMap[algorithm] || { 
      time: size * 0.1, 
      operations: size,
      comparisons: Math.floor(size * 0.6),
      swaps: Math.floor(size * 0.4)
    };

    // Добавляем немного случайности для реалистичности
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
        memory: size * 8 * (1 + Math.random() * 0.2)
      },
      timestamp: Date.now()
    };
  }

  // Общий метод для API вызовов
  private async fetchFromBackend(endpoint: string, body: any): Promise<ApiResponse> {
    if (this.useMock) {
      // Используем mock бекенд
      const method = endpoint.split('/').pop() || 'profileAlgorithm';
      if (method === 'profile') {
        return await this.mockProfileAlgorithm(body.algorithm, body.data, body.config);
      }
      return await this.mockProfileAlgorithm(body.algorithm, body.data, body.config);
    } else {
      // Реальный API вызов
      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(body)
      });

      if (!response.ok) {
        throw new Error(`API Error: ${response.status}`);
      }

      return await response.json();
    }
  }

  // Публичные методы - интерфейс одинаковый для mock и реального бекенда
  async profileAlgorithm(algorithm: string, data: any, config: any): Promise<ApiResponse> {
    return this.fetchFromBackend('/api/profile', { algorithm, data, config });
  }

  async analyzeCode(code: string): Promise<ApiResponse> {
    return this.fetchFromBackend('/api/analyze', { code });
  }

  async getVisualization(structure: string, algorithm: string, data: any): Promise<ApiResponse> {
    return this.fetchFromBackend('/api/visualize', { structure, algorithm, data });
  }
}

export const apiService = new ApiService();