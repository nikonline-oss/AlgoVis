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

    const exampleCode = `def bubble_sort():
    n = struct.len
    for i in range(n):
        for j in range(0, n-i-1):
            if struct[j] > struct[j+1]:
                temp = struct[j]
                struct[j] = struct[j+1]
                struct[j+1] = temp
        j = 0`;

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
                    metadata: { array_name: "struct" },
                    variables: {
                        struct: [64, 34, 25, 12, 22, 11, 90],
                        i: 0,
                        j: 0,
                        n: 7
                    }
                },
                {
                    stepNumber: 2,
                    operation: "assign",
                    description: "Присвоение n = struct.len",
                    metadata: {
                        variable: 'n',
                        value: 7
                    },
                    variables: {
                        struct: [64, 34, 25, 12, 22, 11, 90],
                        i: 0,
                        j: 0,
                        n: 7
                    }
                },
                {
                    stepNumber: 3,
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
                        struct: [64, 34, 25, 12, 22, 11, 90],
                        i: 0,
                        j: 0,
                        n: 7
                    }
                },
                {
                    stepNumber: 4,
                    operation: "swap",
                    description: "Обмен элементов 0 и 1",
                    metadata: {
                        index1: 0,
                        index2: 1,
                        value1: 64,
                        value2: 34
                    },
                    variables: {
                        struct: [34, 64, 25, 12, 22, 11, 90],
                        i: 0,
                        j: 0,
                        n: 7,
                        temp: 64
                    }
                },
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
                    struct: [11, 12, 22, 25, 34, 64, 90]
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
                <TabsList className="grid w-full grid-cols-2">
                    <TabsTrigger value="code">
                        <Code className="w-4 h-4 mr-2" />
                        Ввод кода
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
                    <Card className="border-2 border-primary/20 bg-gradient-to-br from-background to-card shadow-lg hover:shadow-xl transition-shadow duration-300">
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
                                    className="min-h-[300px] font-mono text-sm resize-none bg-background border-border focus:border-primary focus:ring-2 focus:ring-primary/20"
                                    disabled={isAnalyzing}
                                />
                                <div className="absolute bottom-2 right-2 text-xs text-muted-foreground bg-background/80 backdrop-blur-sm px-2 py-1 rounded border border-border">
                                    {code.split('\n').length} строк
                                </div>
                            </div>

                            <div className="flex gap-2">
                                <Button
                                    onClick={handleAnalyze}
                                    disabled={isAnalyzing || !code.trim()}
                                    className="flex-1 bg-primary hover:bg-primary/90"
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
                                    className="border-border hover:bg-secondary"
                                >
                                    {translations['analyzer.clear'] || 'Очистить'}
                                </Button>
                                <Button
                                    onClick={runDemoVisualization}
                                    variant="secondary"
                                    size="lg"
                                    disabled={isAnalyzing}
                                    className="bg-secondary hover:bg-secondary/80"
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
                                    className="hover:bg-primary/10 hover:text-primary"
                                >
                                    Загрузить пример
                                </Button>
                                <div className="text-xs text-muted-foreground flex items-center justify-end">
                                    Поддерживаемые языки: Python
                                </div>
                            </div>
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
                                        <span className="text-sm text-muted-foreground">Скорость:</span>
                                        <select
                                            value={visualizationSpeed}
                                            onChange={(e) => setVisualizationSpeed(Number(e.target.value))}
                                            className="px-3 py-1.5 border border-border rounded-lg text-sm bg-background focus:outline-none focus:ring-2 focus:ring-primary focus:border-transparent"
                                        >
                                            <option value="0.5">0.5x</option>
                                            <option value="1">1x</option>
                                            <option value="2">2x</option>
                                            <option value="4">4x</option>
                                            <option value="8">8x</option>
                                        </select>
                                    </div>
                                    <Button
                                        onClick={() => setActiveTab('code')}
                                        variant="outline"
                                        size="sm"
                                        className="border-border hover:bg-secondary"
                                    >
                                        Назад к вводу
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

                            <Card className="bg-gradient-to-br from-muted/30 to-background border border-border/50">
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
                        <Card className="border-2 border-primary/20 bg-gradient-to-br from-background to-card">
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
                                    className="border-border hover:bg-secondary"
                                >
                                    Перейти к вводу кода
                                </Button>
                            </CardContent>
                        </Card>
                    )}
                </TabsContent>
            </Tabs>

            {/* Информационная секция */}
            <Card className="bg-gradient-to-br from-muted/30 to-background border border-border/50">
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