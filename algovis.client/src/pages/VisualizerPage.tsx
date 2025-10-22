import React, { useState, useEffect, useCallback } from 'react';
import { Button } from '../components/ui/button';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '../components/ui/select';
import { Slider } from '../components/ui/slider';
import { Card, CardContent, CardHeader, CardTitle } from '../components/ui/card';
import { AnimationControls } from '../components/AnimationControls';
import { ArrayVisualization } from '../components/ArrayVisualization';
import { useApp } from '../contexts/AppContext';

interface ApiStep {
    stepNumber: number;
    operation: string;
    description: string;
    metadata: {
        current_array: number[];
    };
    visualizationData: {
        structureType: string;
        elements: Record<string, { value: number; index: number; label: string }>;
        highlights: Array<{
            elementId: string;
            highlightType: string;
            color: string;
            label: string;
        }>;
        connections: any[];
    };
}

interface ApiStatistics {
    comparisons: number;
    swaps: number;
    steps: number;
    recursiveCalls: number;
    memoryOperations: number;
    timeComplexity: number;
    spaceComplexity: number;
    customMetrics: Record<string, number>;
}

interface ApiResult {
    algorithmName: string;
    sessionId: string;
    structureType: string;
    steps: ApiStep[];
    statistics: ApiStatistics;
    executionTime: string;
    outputData: {
        origin_array: number[];
        sorted_array: number[];
        is_sorted: boolean;
    };
}

interface ApiResponse {
    success: boolean;
    result: ApiResult;
    message: string;
}

export function VisualizerPage() {
    const { translations } = useApp();
    const [algorithm, setAlgorithm] = useState('BubbleSort');
    const [arraySize, setArraySize] = useState(20);
    const [originalArray, setOriginalArray] = useState<number[]>([]);
    const [isPlaying, setIsPlaying] = useState(false);
    const [speed, setSpeed] = useState(1);
    const [currentStep, setCurrentStep] = useState(0);
    const [apiSteps, setApiSteps] = useState<ApiStep[]>([]);
    const [stats, setStats] = useState({ comparisons: 0, swaps: 0, steps: 0 });
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const generateRandomArray = useCallback(() => {
        const newArray = Array.from({ length: arraySize }, () =>
            Math.floor(Math.random() * 100) + 1
        );
        setOriginalArray([...newArray]);
        setCurrentStep(0);
        setApiSteps([]);
        setStats({ comparisons: 0, swaps: 0, steps: 0 });
        setIsPlaying(false);
        setError(null);
    }, [arraySize]);

    useEffect(() => {
        generateRandomArray();
    }, [generateRandomArray]);

    const transformApiStepToVisualization = (apiStep: ApiStep) => {
        const array = apiStep.metadata.current_array;
        const highlights = apiStep.visualizationData.highlights;

        const comparing: number[] = [];
        const swapping: number[] = [];
        const sorted: number[] = [];

        highlights.forEach(highlight => {
            const index = parseInt(highlight.elementId);
            if (highlight.highlightType === 'comparing') {
                comparing.push(index);
            } else if (highlight.highlightType === 'swapping') {
                swapping.push(index);
            } else if (highlight.highlightType === 'sorted') {
                sorted.push(index);
            }
        });

        return {
            array,
            comparing,
            swapping,
            sorted
        };
    };

    const executeAlgorithm = async () => {
        if (originalArray.length === 0) return;

        setLoading(true);
        setError(null);

        try {
            const response = await fetch('http://localhost:5266/api/Algorithms/execute', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    algorithmName: algorithm,
                    data: originalArray,
                    parameters: {
                        Detailed: false
                    }
                }),
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const result: ApiResponse = await response.json();
            console.log(result);

            if (result.success) {
                setApiSteps(result.result.steps);
                setStats({
                    comparisons: result.result.statistics.comparisons,
                    swaps: result.result.statistics.swaps,
                    steps: result.result.statistics.steps
                });
                setCurrentStep(0);
            } else {
                throw new Error(result.message);
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'An error occurred');
            console.error('Error executing algorithm:', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (isPlaying && apiSteps.length > 0) {
            const timer = setTimeout(() => {
                if (currentStep < apiSteps.length - 1) {
                    setCurrentStep(currentStep + 1);
                } else {
                    setIsPlaying(false);
                }
            }, 1000 / speed);

            return () => clearTimeout(timer);
        }
    }, [isPlaying, currentStep, apiSteps.length, speed]);

    const handlePlay = async () => {
        if (apiSteps.length === 0) {
            await executeAlgorithm();
        }
        setIsPlaying(true);
    };

    const handlePause = () => {
        setIsPlaying(false);
    };

    const handleStepForward = () => {
        if (apiSteps.length === 0) {
            executeAlgorithm();
            return;
        }
        if (currentStep < apiSteps.length - 1) {
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
    };

    // Получаем текущие данные для визуализации
    const currentStepData = apiSteps.length > 0
        ? transformApiStepToVisualization(apiSteps[currentStep])
        : { array: originalArray, comparing: [], swapping: [], sorted: [] };

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
                                <SelectItem value="BubbleSort">
                                    {translations['algorithm.bubblesort']}
                                </SelectItem>
                                <SelectItem value="QuickSort">
                                    {translations['algorithm.quicksort']}
                                </SelectItem>
                                {/* Добавьте другие алгоритмы по мере их реализации на бэкенде */}
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

                        <Button
                            onClick={executeAlgorithm}
                            disabled={loading || originalArray.length === 0}
                            className="w-full"
                        >
                            {loading ? 'Executing...' : 'Run Algorithm'}
                        </Button>

                        {error && (
                            <div className="text-sm text-red-500 p-2 bg-red-50 rounded">
                                Error: {error}
                            </div>
                        )}

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
                                <div className="flex justify-between">
                                    <span>Total Steps:</span>
                                    <span className="text-primary">{stats.steps}</span>
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
                        disabled={apiSteps.length === 0}
                    />

                    {apiSteps.length > 0 && (
                        <div className="text-center text-sm text-muted-foreground">
                            Step {currentStep + 1} of {apiSteps.length}
                            {apiSteps[currentStep] && (
                                <div className="mt-1">
                                    {apiSteps[currentStep].description}
                                </div>
                            )}
                        </div>
                    )}
                </div>
            </div>
        </div>
    );
}