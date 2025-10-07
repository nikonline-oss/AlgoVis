import React, { useState, useEffect, useCallback } from 'react';
import { Button } from '../components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { Slider } from '../components/ui/slider';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { AnimationControls } from '../components/AnimationControls';
import { ArrayVisualization } from '../components/ArrayVisualization';
import { useApp } from '../contexts/AppContext';

interface SortingStep {
  array: number[];
  comparing?: number[];
  swapping?: number[];
  sorted?: number[];
  pivotIndex?: number;
}

export function VisualizerPage() {
  const { translations } = useApp();
  const [algorithm, setAlgorithm] = useState('bubblesort');
  const [arraySize, setArraySize] = useState(20);
  const [array, setArray] = useState<number[]>([]);
  const [originalArray, setOriginalArray] = useState<number[]>([]);
  const [isPlaying, setIsPlaying] = useState(false);
  const [speed, setSpeed] = useState(1);
  const [currentStep, setCurrentStep] = useState(0);
  const [steps, setSteps] = useState<SortingStep[]>([]);
  const [stats, setStats] = useState({ comparisons: 0, swaps: 0 });

  const generateRandomArray = useCallback(() => {
    const newArray = Array.from({ length: arraySize }, () => 
      Math.floor(Math.random() * 100) + 1
    );
    setArray(newArray);
    setOriginalArray([...newArray]);
    setCurrentStep(0);
    setSteps([]);
    setStats({ comparisons: 0, swaps: 0 });
    setIsPlaying(false);
  }, [arraySize]);

  useEffect(() => {
    generateRandomArray();
  }, [generateRandomArray]);

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
    
    setStats({ comparisons, swaps });
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
    
    setStats({ comparisons, swaps });
    return steps;
  };

  const runAlgorithm = () => {
    let algorithmSteps: SortingStep[] = [];
    
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
    
    setSteps(algorithmSteps);
    setCurrentStep(0);
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
    setArray([...originalArray]);
    setSteps([]);
  };

  const currentStepData = steps[currentStep] || { array, comparing: [], swapping: [], sorted: [] };

  return (
    <div className="space-y-6">
      <div className="grid lg:grid-cols-4 gap-6">
        <Card className="lg:col-span-1">
          <CardHeader>
            <CardTitle>{translations['algorithm.select']}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <Select value={algorithm} onValueChange={setAlgorithm}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="bubblesort">
                  {translations['algorithm.bubblesort']}
                </SelectItem>
                <SelectItem value="quicksort">
                  {translations['algorithm.quicksort']}
                </SelectItem>
                <SelectItem value="mergesort">
                  {translations['algorithm.mergesort']}
                </SelectItem>
                <SelectItem value="heapsort">
                  {translations['algorithm.heapsort']}
                </SelectItem>
              </SelectContent>
            </Select>

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

            <Button
              onClick={generateRandomArray}
              variant="outline"
              className="w-full"
            >
              {translations['data.generate']}
            </Button>

            <div className="space-y-2 pt-4 border-t">
              <div className="text-sm">
                <div className="flex justify-between">
                  <span>{translations['profiler.comparisons']}:</span>
                  <span className="text-primary">{stats.comparisons}</span>
                </div>
                <div className="flex justify-between">
                  <span>{translations['profiler.swaps']}:</span>
                  <span className="text-primary">{stats.swaps}</span>
                </div>
              </div>
            </div>
          </CardContent>
        </Card>

        <div className="lg:col-span-3 space-y-6">
          <ArrayVisualization {...currentStepData} />
          
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
      </div>
    </div>
  );
}