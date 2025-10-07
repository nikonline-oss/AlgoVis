import React, { useState } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { Badge } from '../components/ui/badge';
import { Slider } from '../components/ui/slider';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, LineChart, Line } from 'recharts';
import { useApp } from '../contexts/AppContext';

interface AlgorithmResult {
  name: string;
  time: number;
  comparisons: number;
  swaps: number;
}

export function ProfilerPage() {
  const { translations } = useApp();
  const [arraySize, setArraySize] = useState(1000);
  const [results, setResults] = useState<AlgorithmResult[]>([]);
  const [isRunning, setIsRunning] = useState(false);

  // Mock algorithm implementations for performance testing
  const bubbleSort = (arr: number[]) => {
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;
    const start = performance.now();
    
    for (let i = 0; i < array.length - 1; i++) {
      for (let j = 0; j < array.length - i - 1; j++) {
        comparisons++;
        if (array[j] > array[j + 1]) {
          [array[j], array[j + 1]] = [array[j + 1], array[j]];
          swaps++;
        }
      }
    }
    
    const time = performance.now() - start;
    return { time, comparisons, swaps };
  };

  const quickSort = (arr: number[]) => {
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;
    const start = performance.now();
    
    const partition = (low: number, high: number): number => {
      const pivot = array[high];
      let i = low - 1;
      
      for (let j = low; j < high; j++) {
        comparisons++;
        if (array[j] < pivot) {
          i++;
          if (i !== j) {
            [array[i], array[j]] = [array[j], array[i]];
            swaps++;
          }
        }
      }
      
      [array[i + 1], array[high]] = [array[high], array[i + 1]];
      swaps++;
      return i + 1;
    };
    
    const quickSortHelper = (low: number, high: number) => {
      if (low < high) {
        const pi = partition(low, high);
        quickSortHelper(low, pi - 1);
        quickSortHelper(pi + 1, high);
      }
    };
    
    quickSortHelper(0, array.length - 1);
    const time = performance.now() - start;
    return { time, comparisons, swaps };
  };

  const mergeSort = (arr: number[]) => {
    const array = [...arr];
    let comparisons = 0;
    let swaps = 0;
    const start = performance.now();
    
    const merge = (left: number[], right: number[]): number[] => {
      const result = [];
      let i = 0, j = 0;
      
      while (i < left.length && j < right.length) {
        comparisons++;
        if (left[i] <= right[j]) {
          result.push(left[i]);
          i++;
        } else {
          result.push(right[j]);
          j++;
          swaps++;
        }
      }
      
      return result.concat(left.slice(i)).concat(right.slice(j));
    };
    
    const mergeSortHelper = (arr: number[]): number[] => {
      if (arr.length <= 1) return arr;
      
      const mid = Math.floor(arr.length / 2);
      const left = mergeSortHelper(arr.slice(0, mid));
      const right = mergeSortHelper(arr.slice(mid));
      
      return merge(left, right);
    };
    
    mergeSortHelper(array);
    const time = performance.now() - start;
    return { time, comparisons, swaps };
  };

  const runComparison = async () => {
    setIsRunning(true);
    setResults([]);
    
    // Generate random array
    const testArray = Array.from({ length: arraySize }, () => 
      Math.floor(Math.random() * 1000) + 1
    );
    
    const algorithms = [
      { name: translations['algorithm.bubblesort'], fn: bubbleSort },
      { name: translations['algorithm.quicksort'], fn: quickSort },
      { name: translations['algorithm.mergesort'], fn: mergeSort },
    ];
    
    const newResults: AlgorithmResult[] = [];
    
    for (const algorithm of algorithms) {
      // Add small delay for UI feedback
      await new Promise(resolve => setTimeout(resolve, 100));
      
      const result = algorithm.fn(testArray);
      newResults.push({
        name: algorithm.name,
        ...result,
      });
      
      setResults([...newResults]);
    }
    
    setIsRunning(false);
  };

  const chartData = results.map(result => ({
    name: result.name,
    time: parseFloat(result.time.toFixed(2)),
    comparisons: result.comparisons,
    swaps: result.swaps,
  }));

  const complexityData = [
    { size: 100, bubble: 100 * 100, quick: 100 * Math.log2(100), merge: 100 * Math.log2(100) },
    { size: 500, bubble: 500 * 500, quick: 500 * Math.log2(500), merge: 500 * Math.log2(500) },
    { size: 1000, bubble: 1000 * 1000, quick: 1000 * Math.log2(1000), merge: 1000 * Math.log2(1000) },
    { size: 2000, bubble: 2000 * 2000, quick: 2000 * Math.log2(2000), merge: 2000 * Math.log2(2000) },
    { size: 5000, bubble: 5000 * 5000, quick: 5000 * Math.log2(5000), merge: 5000 * Math.log2(5000) },
  ];

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>{translations['profiler.title']}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="space-y-2">
            <label className="text-sm">{translations['data.size']}: {arraySize}</label>
            <Slider
              value={[arraySize]}
              onValueChange={(value) => setArraySize(value[0])}
              max={5000}
              min={100}
              step={100}
              className="w-full"
            />
          </div>
          
          <Button
            onClick={runComparison}
            disabled={isRunning}
            className="bg-primary hover:bg-primary/90 text-primary-foreground"
          >
            {isRunning ? 'Выполняется...' : translations['profiler.compare']}
          </Button>
        </CardContent>
      </Card>

      {results.length > 0 && (
        <div className="grid lg:grid-cols-2 gap-6">
          <Card>
            <CardHeader>
              <CardTitle>{translations['profiler.time']} (мс)</CardTitle>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="time" fill="var(--color-primary)" />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Операции</CardTitle>
            </CardHeader>
            <CardContent>
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis dataKey="name" />
                  <YAxis />
                  <Tooltip />
                  <Bar dataKey="comparisons" fill="var(--color-chart-1)" name="Сравнения" />
                  <Bar dataKey="swaps" fill="var(--color-chart-2)" name="Перестановки" />
                </BarChart>
              </ResponsiveContainer>
            </CardContent>
          </Card>
        </div>
      )}

      <Card>
        <CardHeader>
          <CardTitle>Теоретическая сложность</CardTitle>
        </CardHeader>
        <CardContent>
          <ResponsiveContainer width="100%" height={400}>
            <LineChart data={complexityData}>
              <CartesianGrid strokeDasharray="3 3" />
              <XAxis dataKey="size" />
              <YAxis />
              <Tooltip />
              <Line type="monotone" dataKey="bubble" stroke="var(--color-chart-1)" name="Bubble Sort O(n²)" />
              <Line type="monotone" dataKey="quick" stroke="var(--color-chart-2)" name="Quick Sort O(n log n)" />
              <Line type="monotone" dataKey="merge" stroke="var(--color-chart-3)" name="Merge Sort O(n log n)" />
            </LineChart>
          </ResponsiveContainer>
        </CardContent>
      </Card>

      {results.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Детальные результаты</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              {results.map((result, index) => (
                <div key={index} className="flex items-center justify-between p-4 border rounded-lg">
                  <div>
                    <h4 className="font-medium">{result.name}</h4>
                    <div className="flex space-x-4 text-sm text-muted-foreground">
                      <span>Время: {result.time.toFixed(2)} мс</span>
                      <span>Сравнения: {result.comparisons}</span>
                      <span>Перестановки: {result.swaps}</span>
                    </div>
                  </div>
                  <Badge variant={index === 0 ? "default" : "secondary"}>
                    {index === 0 ? "Лучший" : `+${((result.time / results[0].time - 1) * 100).toFixed(1)}%`}
                  </Badge>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      )}
    </div>
  );
}