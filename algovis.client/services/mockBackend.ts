// services/mockBackend.ts
export interface MockResponse {
  success: boolean;
  data: any;
  timestamp: number;
}

class MockBackend {
  private async simulateNetworkDelay(min: number = 100, max: number = 500): Promise<void> {
    const delay = Math.random() * (max - min) + min;
    return new Promise(resolve => setTimeout(resolve, delay));
  }

  // Профилирование алгоритмов
  async profileAlgorithm(algorithm: string, data: any, config: any): Promise<MockResponse> {
    await this.simulateNetworkDelay();
    
    const size = data.length || config.dataSize || 1000;
    
    // Простые расчеты производительности на основе размера данных
    const performanceMap: Record<string, { time: number; operations: number }> = {
      'bubbleSort': { 
        time: size * size * 0.001, 
        operations: (size * (size - 1)) / 2 
      },
      'quickSort': { 
        time: size * Math.log2(size) * 0.001, 
        operations: size * Math.log2(size) 
      },
      'mergeSort': { 
        time: size * Math.log2(size) * 0.0015, 
        operations: size * Math.log2(size) * 1.2 
      },
      'insertionSort': { 
        time: size * size * 0.0005, 
        operations: (size * (size - 1)) / 2 
      }
    };

    const perf = performanceMap[algorithm] || { time: size * 0.1, operations: size };

    return {
      success: true,
      data: {
        name: algorithm,
        metrics: {
          time: perf.time + (Math.random() * perf.time * 0.1), // +10% случайность
          operations: perf.operations,
          comparisons: Math.floor(perf.operations * 0.6),
          swaps: Math.floor(perf.operations * 0.4),
          memory: size * 8 * (1 + Math.random() * 0.2)
        },
        complexity: {
          time: this.getTimeComplexity(algorithm),
          space: this.getSpaceComplexity(algorithm)
        }
      },
      timestamp: Date.now()
    };
  }

  // Анализ кода
  async analyzeCode(code: string): Promise<MockResponse> {
    await this.simulateNetworkDelay(200, 800);
    
    const lines = code.split('\n').length;
    const hasLoops = code.includes('for ') || code.includes('while ');
    const hasNestedLoops = (code.match(/for.*\{[^}]*for/g) || []).length > 0;
    
    let complexity = 'O(1)';
    if (hasNestedLoops) complexity = 'O(n²)';
    else if (hasLoops) complexity = 'O(n)';

    return {
      success: true,
      data: {
        complexity,
        lines,
        warnings: hasNestedLoops ? ['Обнаружены вложенные циклы - возможна квадратичная сложность'] : [],
        suggestions: [
          'Используйте более эффективные структуры данных',
          'Рассмотрите возможность кэширования результатов'
        ],
        analysisTime: Math.random() * 100 + 50
      },
      timestamp: Date.now()
    };
  }

  // Визуализация данных
  async getVisualizationData(structure: string, algorithm: string, data: any): Promise<MockResponse> {
    await this.simulateNetworkDelay(50, 200);
    
    return {
      success: true,
      data: {
        structure,
        algorithm,
        steps: this.generateSteps(structure, algorithm, data),
        initialState: data,
        metadata: {
          stepCount: 10,
          currentStep: 0,
          isComplete: false
        }
      },
      timestamp: Date.now()
    };
  }

  private getTimeComplexity(algorithm: string): string {
    const complexities: Record<string, string> = {
      'bubbleSort': 'O(n²)',
      'quickSort': 'O(n log n)',
      'mergeSort': 'O(n log n)',
      'insertionSort': 'O(n²)'
    };
    return complexities[algorithm] || 'O(n)';
  }

  private getSpaceComplexity(algorithm: string): string {
    const complexities: Record<string, string> = {
      'bubbleSort': 'O(1)',
      'quickSort': 'O(log n)',
      'mergeSort': 'O(n)',
      'insertionSort': 'O(1)'
    };
    return complexities[algorithm] || 'O(1)';
  }

  private generateSteps(structure: string, algorithm: string, data: any): any[] {
    // Генерация шагов для визуализации
    const steps = [];
    const stepCount = 10;
    
    for (let i = 0; i < stepCount; i++) {
      steps.push({
        step: i,
        data: this.getStepData(structure, data, i, stepCount),
        description: `Шаг ${i + 1} выполнения ${algorithm}`,
        metrics: {
          comparisons: i * 5,
          swaps: i * 2,
          operations: i * 7
        }
      });
    }
    
    return steps;
  }

  private getStepData(structure: string, originalData: any, step: number, totalSteps: number): any {
    if (structure === 'array') {
      const data = [...originalData];
      // Симуляция частичной сортировки
      const progress = step / totalSteps;
      const sortedCount = Math.floor(data.length * progress);
      
      for (let i = 0; i < sortedCount - 1; i++) {
        if (data[i] > data[i + 1]) {
          [data[i], data[i + 1]] = [data[i + 1], data[i]];
        }
      }
      return data;
    }
    
    return originalData;
  }
}

export const mockBackend = new MockBackend();