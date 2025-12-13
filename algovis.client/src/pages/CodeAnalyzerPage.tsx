import React, { useState } from 'react';
import { Button } from '../components/ui/button';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '../components/ui/card';
import { Textarea } from '../components/ui/textarea';
import { useApp } from '../contexts/AppContext';
import { PlayCircle, Code, AlertCircle, CheckCircle, Loader2, Eye } from 'lucide-react';
import { Alert, AlertDescription } from '../components/ui/alert';
import AlgorithmVisualizer, { AlgorithmData } from '../components/AlgorithmVisualizer';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '../components/ui/tabs';

export function CodeAnalyzerPage() {
    const { translations } = useApp();
    const [code, setCode] = useState('');
    const [isAnalyzing, setIsAnalyzing] = useState(false);
    const [analysisResult, setAnalysisResult] = useState<{
        success: boolean;
        message: string;
        details?: any;
    } | null>(null);
    const [visualizationData, setVisualizationData] = useState<AlgorithmData | null>(null);
    const [visualizationSpeed, setVisualizationSpeed] = useState(2);
    const [activeTab, setActiveTab] = useState('code');

    const handleAnalyze = async () => {
        if (!code.trim()) {
            setAnalysisResult({
                success: false,
                message: translations['analyzer.emptyCode'] || 'Пожалуйста, введите код для анализа',
            });
            return;
        }

        setIsAnalyzing(true);
        setAnalysisResult(null);
        setVisualizationData(null);

        try {
            const response = await fetch('http://localhost:5266/api/analyze', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Accept': 'application/json'
                },
                body: JSON.stringify({
                    code: code,
                    language: 'python',
                    generateVisualization: true,
                    ModsDir:'',
                    visualizeTypes: ['compare', 'swap', 'condition', 'assign', 'complete'],
                    validate: true
                }),
            });

            const data = await response.json();

            if (data.success && data.data.result) {
                setVisualizationData(data.data.result);
                setActiveTab('visualization');
            }

            setAnalysisResult(data);
        } catch (error) {
            console.error('Error analyzing code:', error);
            setAnalysisResult({
                success: false,
                message: translations['analyzer.error'] || 'Ошибка при анализе кода',
            });
        } finally {
            setIsAnalyzing(false);
        }
    };

    const handleClear = () => {
        setCode('');
        setAnalysisResult(null);
        setVisualizationData(null);
        setActiveTab('code');
    };

    const exampleCode = `def bubble_sort(arr):
    n = len(arr)
    for i in range(n):
        swapped = False
        for j in range(0, n-i-1):
            if arr[j] > arr[j+1]:
                arr[j], arr[j+1] = arr[j+1], arr[j]
                swapped = True
        if not swapped:
            break
    return arr

def quick_sort(arr, low=0, high=None):
    if high is None:
        high = len(arr) - 1
    
    if low < high:
        pi = partition(arr, low, high)
        quick_sort(arr, low, pi-1)
        quick_sort(arr, pi+1, high)
    
    return arr

def partition(arr, low, high):
    pivot = arr[high]
    i = low - 1
    
    for j in range(low, high):
        if arr[j] <= pivot:
            i += 1
            arr[i], arr[j] = arr[j], arr[i]
    
    arr[i+1], arr[high] = arr[high], arr[i+1]
    return i + 1

# Пример использования
numbers = [64, 34, 25, 12, 22, 11, 90]
sorted_bubble = bubble_sort(numbers.copy())
sorted_quick = quick_sort(numbers.copy())
print("Bubble sort:", sorted_bubble)
print("Quick sort:", sorted_quick)`;

    // Функция для запуска демо-визуализации
    const runDemoVisualization = () => {
        // Создаем демо-данные для визуализации
        const demoData: AlgorithmData = {
            success: true,
            algorithmName: "Bubble Sort Demo",
            sessionId: "demo-" + Date.now(),
            structureType: "array",
            steps: [
                {
                    stepNumber: 1,
                    operation: "init",
                    description: "Инициализация массива",
                    metadata: { array_name: "arr" },
                    variables: {
                        arr: [64, 34, 25, 12, 22, 11, 90],
                        i: 0,
                        j: 0,
                        n: 7
                    }
                },
                {
                    stepNumber: 2,
                    operation: "compare",
                    description: "Сравнение элементов 0 и 1",
                    metadata: {
                        index1: 0,
                        index2: 1,
                        value1: 64,
                        value2: 34,
                        comparison_result: 1
                    },
                    variables: {
                        arr: [64, 34, 25, 12, 22, 11, 90],
                        i: 0,
                        j: 0,
                        n: 7
                    }
                },
                {
                    stepNumber: 3,
                    operation: "swap",
                    description: "Обмен элементов 0 и 1",
                    metadata: {
                        index1: 0,
                        index2: 1,
                        value1: 64,
                        value2: 34
                    },
                    variables: {
                        arr: [34, 64, 25, 12, 22, 11, 90],
                        i: 0,
                        j: 0,
                        n: 7
                    }
                },
                // Добавьте больше шагов по необходимости
            ],
            statistics: {
                comparisons: 21,
                swaps: 12,
                steps: 45,
                recursiveCalls: 0,
                memoryOperations: 15,
                timeComplexity: 49,
                spaceComplexity: 7,
                customMetrics: {}
            },
            executionTime: "00:00:01.234",
            outputData: {
                start_structure: [64, 34, 25, 12, 22, 11, 90],
                final_structure: "[11, 12, 22, 25, 34, 64, 90]",
                variables: {
                    arr: [11, 12, 22, 25, 34, 64, 90]
                },
                call_depth: 1,
                function_calls: 1,
                total_steps: 45
            }
        };

        setVisualizationData(demoData);
        setActiveTab('visualization');
    };

    return (
        <div className="space-y-6 max-w-7xl mx-auto">
            <div className="space-y-2">
                <h1 className="text-3xl font-bold">{translations['analyzer.title'] || 'Анализатор кода'}</h1>
                <p className="text-muted-foreground">
                    {translations['analyzer.description'] || 'Проанализируйте код и визуализируйте выполнение алгоритмов'}
                </p>
            </div>

            <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
                <TabsList className="grid w-full grid-cols-3">
                    <TabsTrigger value="code">
                        <Code className="w-4 h-4 mr-2" />
                        Ввод кода
                    </TabsTrigger>
                    <TabsTrigger value="results">
                        <AlertCircle className="w-4 h-4 mr-2" />
                        Результаты анализа
                    </TabsTrigger>
                    <TabsTrigger
                        value="visualization"
                        disabled={!visualizationData}
                        className={!visualizationData ? "opacity-50 cursor-not-allowed" : ""}
                    >
                        <Eye className="w-4 h-4 mr-2" />
                        Визуализация
                    </TabsTrigger>
                </TabsList>

                <TabsContent value="code" className="space-y-6">
                    <Card className="border-2 border-primary/20">
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                <Code className="w-5 h-5 text-primary" />
                                {translations['analyzer.codeInput'] || 'Ввод кода'}
                            </CardTitle>
                            <CardDescription>
                                {translations['analyzer.codeInputDesc'] || 'Введите код алгоритма для анализа и визуализации'}
                            </CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            <div className="relative">
                                <Textarea
                                    value={code}
                                    onChange={(e) => setCode(e.target.value)}
                                    placeholder={exampleCode}
                                    className="min-h-[300px] font-mono text-sm resize-none"
                                    disabled={isAnalyzing}
                                />
                                <div className="absolute bottom-2 right-2 text-xs text-muted-foreground bg-background px-2 py-1 rounded">
                                    {code.split('\n').length} строк
                                </div>
                            </div>

                            <div className="flex gap-2">
                                <Button
                                    onClick={handleAnalyze}
                                    disabled={isAnalyzing || !code.trim()}
                                    className="flex-1"
                                    size="lg"
                                >
                                    {isAnalyzing ? (
                                        <>
                                            <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                                            {translations['analyzer.analyzing'] || 'Анализ...'}
                                        </>
                                    ) : (
                                        <>
                                            <PlayCircle className="w-4 h-4 mr-2" />
                                            {translations['analyzer.analyze'] || 'Проанализировать'}
                                        </>
                                    )}
                                </Button>
                                <Button
                                    onClick={handleClear}
                                    variant="outline"
                                    disabled={isAnalyzing}
                                    size="lg"
                                >
                                    {translations['analyzer.clear'] || 'Очистить'}
                                </Button>
                                <Button
                                    onClick={runDemoVisualization}
                                    variant="secondary"
                                    size="lg"
                                    disabled={isAnalyzing}
                                >
                                    <Eye className="w-4 h-4 mr-2" />
                                    Демо
                                </Button>
                            </div>

                            <div className="grid grid-cols-2 gap-2">
                                <Button
                                    onClick={() => setCode(exampleCode)}
                                    variant="ghost"
                                    size="sm"
                                    disabled={isAnalyzing}
                                >
                                    Загрузить пример
                                </Button>
                                <div className="text-xs text-muted-foreground flex items-center justify-end">
                                    Поддерживаемые языки: Python, JavaScript, C++
                                </div>
                            </div>
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="results" className="space-y-6">
                    <Card className="border-2 border-primary/20">
                        <CardHeader>
                            <CardTitle className="flex items-center gap-2">
                                {analysisResult?.success ? (
                                    <CheckCircle className="w-5 h-5 text-green-500" />
                                ) : (
                                    <AlertCircle className="w-5 h-5 text-amber-500" />
                                )}
                                {translations['analyzer.results'] || 'Результаты анализа'}
                            </CardTitle>
                            <CardDescription>
                                {translations['analyzer.resultsDesc'] || 'Результаты статического анализа кода'}
                            </CardDescription>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            {!analysisResult && !isAnalyzing && (
                                <div className="min-h-[300px] flex items-center justify-center text-center text-muted-foreground">
                                    <div className="space-y-2">
                                        <Code className="w-12 h-12 mx-auto opacity-50" />
                                        <p>{translations['analyzer.noResults'] || 'Нет результатов анализа'}</p>
                                    </div>
                                </div>
                            )}

                            {isAnalyzing && (
                                <div className="min-h-[300px] flex items-center justify-center">
                                    <div className="text-center space-y-4">
                                        <Loader2 className="w-12 h-12 mx-auto animate-spin text-primary" />
                                        <p className="text-muted-foreground">
                                            {translations['analyzer.analyzingMessage'] || 'Анализ кода...'}
                                        </p>
                                        <p className="text-sm text-muted-foreground">
                                            Генерация визуализации может занять некоторое время
                                        </p>
                                    </div>
                                </div>
                            )}

                            {analysisResult && (
                                <div className="space-y-4">
                                    <Alert variant={analysisResult.success ? "default" : "destructive"}>
                                        <AlertDescription className="flex items-center gap-2">
                                            {analysisResult.success ? (
                                                <CheckCircle className="w-4 h-4" />
                                            ) : (
                                                <AlertCircle className="w-4 h-4" />
                                            )}
                                            {analysisResult.message}
                                        </AlertDescription>
                                    </Alert>

                                    {analysisResult.details && (
                                        <div className="space-y-4 pt-4">
                                            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                                                <Card className="bg-muted/30">
                                                    <CardContent className="pt-6">
                                                        <div className="space-y-3">
                                                            <div className="flex justify-between items-center">
                                                                <span className="text-sm">Сложность</span>
                                                                <span className="font-mono px-3 py-1 bg-primary/10 text-primary rounded">
                                                                    {analysisResult.details.complexity || 'O(n²)'}
                                                                </span>
                                                            </div>
                                                            <div className="flex justify-between items-center">
                                                                <span className="text-sm">Предупреждения</span>
                                                                <span className="font-mono px-3 py-1 bg-amber-500/10 text-amber-600 dark:text-amber-400 rounded">
                                                                    {analysisResult.details.warnings || 0}
                                                                </span>
                                                            </div>
                                                            <div className="flex justify-between items-center">
                                                                <span className="text-sm">Предложения</span>
                                                                <span className="font-mono px-3 py-1 bg-green-500/10 text-green-600 dark:text-green-400 rounded">
                                                                    {analysisResult.details.suggestions || 0}
                                                                </span>
                                                            </div>
                                                        </div>
                                                    </CardContent>
                                                </Card>

                                                <Card className="bg-muted/30">
                                                    <CardContent className="pt-6">
                                                        <div className="space-y-3">
                                                            <div className="flex justify-between items-center">
                                                                <span className="text-sm">Строк кода</span>
                                                                <span className="font-mono px-3 py-1 bg-blue-500/10 text-blue-600 rounded">
                                                                    {analysisResult.details.metrics?.lines || 0}
                                                                </span>
                                                            </div>
                                                            <div className="flex justify-between items-center">
                                                                <span className="text-sm">Функций</span>
                                                                <span className="font-mono px-3 py-1 bg-purple-500/10 text-purple-600 rounded">
                                                                    {analysisResult.details.metrics?.functions || 0}
                                                                </span>
                                                            </div>
                                                            <div className="flex justify-between items-center">
                                                                <span className="text-sm">Классов</span>
                                                                <span className="font-mono px-3 py-1 bg-indigo-500/10 text-indigo-600 rounded">
                                                                    {analysisResult.details.metrics?.classes || 0}
                                                                </span>
                                                            </div>
                                                        </div>
                                                    </CardContent>
                                                </Card>

                                                <Card className="bg-muted/30">
                                                    <CardContent className="pt-6">
                                                        <h4 className="font-semibold mb-3">Алгоритмы обнаружены</h4>
                                                        <div className="space-y-2">
                                                            {analysisResult.details.algorithms?.map((algo: string, idx: number) => (
                                                                <div key={idx} className="flex items-center gap-2">
                                                                    <div className="w-2 h-2 rounded-full bg-green-500"></div>
                                                                    <span className="text-sm">{algo}</span>
                                                                </div>
                                                            )) || (
                                                                    <div className="text-sm text-muted-foreground">
                                                                        Алгоритмы не обнаружены
                                                                    </div>
                                                                )}
                                                        </div>
                                                    </CardContent>
                                                </Card>
                                            </div>

                                            {visualizationData && (
                                                <div className="pt-4 border-t">
                                                    <div className="flex items-center justify-between mb-4">
                                                        <h4 className="font-semibold">Готово к визуализации</h4>
                                                        <Button
                                                            onClick={() => setActiveTab('visualization')}
                                                            size="sm"
                                                        >
                                                            <Eye className="w-4 h-4 mr-2" />
                                                            Перейти к визуализации
                                                        </Button>
                                                    </div>
                                                    <div className="text-sm text-muted-foreground">
                                                        Алгоритм "{visualizationData.algorithmName}" успешно проанализирован.
                                                        Доступно {visualizationData.steps.length} шагов визуализации.
                                                    </div>
                                                </div>
                                            )}
                                        </div>
                                    )}
                                </div>
                            )}
                        </CardContent>
                    </Card>
                </TabsContent>

                <TabsContent value="visualization" className="space-y-6">
                    {visualizationData ? (
                        <>
                            <div className="flex justify-between items-center">
                                <div>
                                    <h2 className="text-2xl font-bold">Визуализация алгоритма</h2>
                                    <p className="text-muted-foreground">
                                        Интерактивная визуализация выполнения {visualizationData.algorithmName}
                                    </p>
                                </div>
                                <div className="flex items-center gap-4">
                                    <div className="flex items-center gap-2">
                                        <span className="text-sm">Скорость:</span>
                                        <select
                                            value={visualizationSpeed}
                                            onChange={(e) => setVisualizationSpeed(Number(e.target.value))}
                                            className="px-3 py-1 border rounded text-sm"
                                        >
                                            <option value="0.5">0.5x</option>
                                            <option value="1">1x</option>
                                            <option value="2">2x</option>
                                            <option value="4">4x</option>
                                            <option value="8">8x</option>
                                        </select>
                                    </div>
                                    <Button
                                        onClick={() => setActiveTab('results')}
                                        variant="outline"
                                        size="sm"
                                    >
                                        Назад к результатам
                                    </Button>
                                </div>
                            </div>

                            <AlgorithmVisualizer
                                data={visualizationData}
                                speed={visualizationSpeed}
                                onSpeedChange={setVisualizationSpeed}
                                autoPlay={true}
                                className="shadow-lg"
                            />

                            <Card className="bg-muted/30">
                                <CardHeader>
                                    <CardTitle className="text-lg">Информация о визуализации</CardTitle>
                                </CardHeader>
                                <CardContent>
                                    <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                                        <div>
                                            <div className="text-sm text-muted-foreground">Алгоритм</div>
                                            <div className="font-semibold">{visualizationData.algorithmName}</div>
                                        </div>
                                        <div>
                                            <div className="text-sm text-muted-foreground">Шагов выполнения</div>
                                            <div className="font-semibold">{visualizationData.steps.length}</div>
                                        </div>
                                        <div>
                                            <div className="text-sm text-muted-foreground">Время выполнения</div>
                                            <div className="font-semibold">{visualizationData.executionTime}</div>
                                        </div>
                                        <div>
                                            <div className="text-sm text-muted-foreground">Сравнений</div>
                                            <div className="font-semibold">{visualizationData.statistics.comparisons}</div>
                                        </div>
                                    </div>
                                </CardContent>
                            </Card>
                        </>
                    ) : (
                        <Card className="border-2 border-primary/20">
                            <CardContent className="min-h-[400px] flex flex-col items-center justify-center text-center space-y-4">
                                <Eye className="w-16 h-16 text-muted-foreground/50" />
                                <div>
                                    <h3 className="text-xl font-semibold">Нет данных для визуализации</h3>
                                    <p className="text-muted-foreground mt-2">
                                        Проанализируйте код алгоритма, чтобы увидеть его визуализацию
                                    </p>
                                </div>
                                <Button
                                    onClick={() => setActiveTab('code')}
                                    variant="outline"
                                >
                                    Перейти к вводу кода
                                </Button>
                            </CardContent>
                        </Card>
                    )}
                </TabsContent>
            </Tabs>

            {/* Информационная секция */}
            <Card className="bg-muted/30">
                <CardHeader>
                    <CardTitle className="text-lg">Как работает визуализация</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                        <div className="space-y-2">
                            <div className="flex items-center gap-2">
                                <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center">
                                    <span className="text-primary font-bold">1</span>
                                </div>
                                <h4 className="font-semibold">Анализ кода</h4>
                            </div>
                            <p className="text-sm text-muted-foreground">
                                Система анализирует ваш код, определяет алгоритмы и создает пошаговую трассировку выполнения
                            </p>
                        </div>
                        <div className="space-y-2">
                            <div className="flex items-center gap-2">
                                <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center">
                                    <span className="text-primary font-bold">2</span>
                                </div>
                                <h4 className="font-semibold">Генерация данных</h4>
                            </div>
                            <p className="text-sm text-muted-foreground">
                                Создаются детальные данные для визуализации: состояния переменных, сравнения, обмены значений
                            </p>
                        </div>
                        <div className="space-y-2">
                            <div className="flex items-center gap-2">
                                <div className="w-8 h-8 rounded-full bg-primary/10 flex items-center justify-center">
                                    <span className="text-primary font-bold">3</span>
                                </div>
                                <h4 className="font-semibold">Интерактивная визуализация</h4>
                            </div>
                            <p className="text-sm text-muted-foreground">
                                Интерактивный просмотр выполнения алгоритма с управлением скоростью и выделением ключевых элементов
                            </p>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
}