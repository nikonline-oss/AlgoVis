import React, { useState, useEffect, useCallback, useMemo } from 'react';

// Типы данных
interface Element {
    value: number;
    index: number;
    label: string;
}

interface Metadata {
    variable?: string;
    value?: string;
    expression?: string;
    condition?: string;
    result?: boolean;
    value_type?: string;
    visualization_type?: string;
    array_name?: string;
    index1?: number | { type: number; rawValue: number };
    index2?: number | { type: number; rawValue: number };
    value1?: number;
    value2?: number;
    comparison_result?: number;
    [key: string]: any;
}

interface Variables {
    struct?: {
        values?: Array<{ Type: number; RawValue: any }>;
        len?: number;
        isEmpty?: boolean;
        type?: { Type: number; RawValue: string };
        id?: { Type: number; RawValue: string };
    } | Array<{ Type: number; RawValue: any }>;
    low?: number;
    high?: number;
    i?: number;
    j?: number;
    pivot?: number;
    pivot_index?: number;
    stack?: Array<{ Type: number; RawValue: any }>;
    stack_top?: number;
    temp?: number;
    swapped?: boolean;
    last_comparison?: number;
    result?: number;
    [key: string]: any;
}

interface VisualizationStep {
    stepNumber: number;
    operation: string;
    description: string;
    metadata: Metadata;
    variables: Variables;
}

interface Statistics {
    comparisons: number;
    swaps: number;
    steps: number;
    recursiveCalls: number;
    memoryOperations: number;
    timeComplexity: number;
    spaceComplexity: number;
    customMetrics: Record<string, any>;
}

interface AlgorithmData {
    success: boolean;
    algorithmName: string;
    sessionId: string;
    structureType: string;
    steps: VisualizationStep[];
    statistics: Statistics;
    executionTime: string;
    outputData: {
        start_structure: number[];
        final_structure: string;
        variables: Variables;
        call_depth: number;
        function_calls: number;
        total_steps: number;
    };
}

interface VariableConfig {
    variable: string;
    color: string;
    label: string;
    isActive: boolean;
    type: 'index' | 'value' | 'metadata';
}

interface AlgorithmVisualizerProps {
    data: AlgorithmData;
    autoPlay?: boolean;
    speed?: number;
    onSpeedChange?: (speed: number) => void;
    className?: string;
}

const AlgorithmVisualizer: React.FC<AlgorithmVisualizerProps> = ({
    data,
    autoPlay = false,
    speed = 2,
    onSpeedChange,
    className = ''
}) => {
    const [currentStepIndex, setCurrentStepIndex] = useState<number>(0);
    const [isPlaying, setIsPlaying] = useState<boolean>(autoPlay);
    const [showAllMetadata, setShowAllMetadata] = useState<boolean>(false);
    const [history, setHistory] = useState<number[]>([0]);
    const [animationState, setAnimationState] = useState<'idle' | 'comparing' | 'swapping' | 'updating'>('idle');
    const [variableConfigs, setVariableConfigs] = useState<VariableConfig[]>([
        { variable: 'i', color: '#3b82f6', label: 'i', isActive: true, type: 'index' },
        { variable: 'j', color: '#ef4444', label: 'j', isActive: true, type: 'index' },
        { variable: 'pivot_index', color: '#f59e0b', label: 'pivot', isActive: true, type: 'index' },
        { variable: 'low', color: '#10b981', label: 'low', isActive: true, type: 'index' },
        { variable: 'high', color: '#8b5cf6', label: 'high', isActive: true, type: 'index' },
        { variable: 'index1', color: '#ec4899', label: 'index1', isActive: true, type: 'metadata' },
        { variable: 'index2', color: '#14b8a6', label: 'index2', isActive: true, type: 'metadata' },
    ]);
    const [showVariableConfig, setShowVariableConfig] = useState<boolean>(false);
    const [newVariable, setNewVariable] = useState<string>('');
    const [stepInfoHeight, setStepInfoHeight] = useState<'auto' | number>('auto');
    const [availableArrays, setAvailableArrays] = useState<string[]>([]);
    const [selectedArray, setSelectedArray] = useState<string>('struct');
    const [currentSpeed, setCurrentSpeed] = useState<number>(speed);

    // Извлекаем данные
    const algorithmName = data.algorithmName;
    const steps = data.steps;
    const statistics = data.statistics;
    const executionTime = data.executionTime;
    const currentStep = steps[currentStepIndex];

    // Обновляем скорость при изменении пропса
    useEffect(() => {
        setCurrentSpeed(speed);
    }, [speed]);

    // Функция для извлечения массива значений из переменных
    const extractArrayFromVariables = useCallback((variables: Variables, arrayName: string = 'struct'): number[] => {
        const target = variables[arrayName];

        if (!target) return [];

        // Если target - это массив напрямую
        if (Array.isArray(target)) {
            return target.map(item => {
                if (typeof item === 'object' && 'RawValue' in item) {
                    return Number(item.RawValue);
                }
                return Number(item);
            });
        }

        // Если struct - объект с полем values
        if (target.values && Array.isArray(target.values.RawValue)) {
            return target.values.RawValue.map(item => {
                if (typeof item === 'object' && 'RawValue' in item) {
                    return Number(item.RawValue);
                }
                return Number(item);
            });
        }

        // Если target - это примитив или другой объект
        if (typeof target === 'number' || typeof target === 'string') {
            return [Number(target)];
        }

        return [];
    }, []);

    // Обновляем список доступных массивов при изменении шага
    useEffect(() => {
        if (currentStep?.variables) {
            const arrays: string[] = [];

            // Ищем все переменные, которые могут быть массивами
            Object.entries(currentStep.variables).forEach(([key, value]) => {
                if (Array.isArray(value)) {
                    arrays.push(key);
                } else if (value && typeof value === 'object') {
                    // Проверяем, есть ли поле values (структура массива)
                    if ('values' in value && Array.isArray(value.values)) {
                        arrays.push(key);
                    } else if ('RawValue' in value && Array.isArray(value.RawValue)) {
                        arrays.push(key);
                    }
                }
            });

            // Добавляем стандартные массивы
            if (!arrays.includes('struct')) arrays.unshift('struct');
            if (!arrays.includes('stack') && currentStep.variables.stack) arrays.push('stack');

            setAvailableArrays(arrays);

            // Если выбранного массива нет в доступных, выбираем первый
            if (!arrays.includes(selectedArray) && arrays.length > 0) {
                setSelectedArray(arrays[0]);
            }
        }
    }, [currentStep, selectedArray]);

    // Получаем элементы массива для текущего шага
    const elementsArray = useMemo<Array<{ value: number; index: number; label: string }>>(() => {
        const arrayValues = extractArrayFromVariables(currentStep.variables, selectedArray);

        return arrayValues.map((value, index) => ({
            value,
            index,
            label: `Элемент ${index}`
        }));
    }, [currentStep, selectedArray, extractArrayFromVariables]);

    // Определяем выделенные элементы на основе конфигураций переменных
    const highlightedElements = useMemo(() => {
        const highlights: Array<{ index: number; color: string; label: string }> = [];
        const arrayValues = extractArrayFromVariables(currentStep.variables, selectedArray);
        const arrayLength = arrayValues.length;

        // Функция для безопасного добавления индекса
        const addHighlight = (index: number, config: VariableConfig) => {
            if (typeof index === 'number' && index >= 0 && index < arrayLength) {
                highlights.push({
                    index,
                    color: config.color,
                    label: config.label
                });
            }
        };

        // Проверяем все активные конфигурации переменных
        variableConfigs.forEach(config => {
            if (!config.isActive) return;

            if (config.type === 'index') {
                // Для переменных-индексов (i, j, pivot_index, low, high)
                const value = currentStep.variables[config.variable];
                if (typeof value === 'number') {
                    addHighlight(value, config);
                }
            } else if (config.type === 'metadata') {
                // Для метаданных (index1, index2 из операции swap/compare)
                const metadata = currentStep.metadata;
                if (config.variable === 'index1' && metadata.index1 !== undefined) {
                    const index1 = typeof metadata.index1 === 'object' && metadata.index1 !== null && 'rawValue' in metadata.index1
                        ? metadata.index1.rawValue
                        : metadata.index1;
                    if (typeof index1 === 'number') {
                        addHighlight(index1, config);
                    }
                }
                if (config.variable === 'index2' && metadata.index2 !== undefined) {
                    const index2 = typeof metadata.index2 === 'object' && metadata.index2 !== null && 'rawValue' in metadata.index2
                        ? metadata.index2.rawValue
                        : metadata.index2;
                    if (typeof index2 === 'number') {
                        addHighlight(index2, config);
                    }
                }
            }
        });

        // Убираем дубликаты (берем первую конфигурацию для каждого индекса)
        const uniqueHighlights: Array<{ index: number; color: string; label: string }> = [];
        const seen = new Set<number>();

        highlights.reverse().forEach(h => {
            if (!seen.has(h.index)) {
                seen.add(h.index);
                uniqueHighlights.unshift(h);
            }
        });

        return uniqueHighlights.sort((a, b) => a.index - b.index);
    }, [currentStep, variableConfigs, selectedArray, extractArrayFromVariables]);

    // Получаем переменные, которые можно настроить
    const availableVariables = useMemo(() => {
        const vars = new Set<string>();

        // Добавляем все переменные из текущего шага
        Object.keys(currentStep.variables).forEach(key => {
            const value = currentStep.variables[key];
            if (typeof value === 'number' || Array.isArray(value) ||
                (value && typeof value === 'object' && ('values' in value || 'RawValue' in value))) {
                vars.add(key);
            }
        });

        // Добавляем переменные из метаданных
        const metadata = currentStep.metadata;
        if (metadata.index1 !== undefined) vars.add('index1');
        if (metadata.index2 !== undefined) vars.add('index2');
        if (metadata.value1 !== undefined) vars.add('value1');
        if (metadata.value2 !== undefined) vars.add('value2');

        // Добавляем стандартные переменные
        ['i', 'j', 'pivot_index', 'low', 'high', 'stack_top', 'temp', 'result'].forEach(v => vars.add(v));

        return Array.from(vars).sort();
    }, [currentStep]);

    // Создаем анимированное состояние для элементов
    const [animatedElements, setAnimatedElements] = useState<Array<{ value: number; index: number; label: string }>>(elementsArray);

    // Обновляем анимированные элементы при изменении шага
    useEffect(() => {
        if (elementsArray.length > 0) {
            // Сначала скрываем старые значения
            setAnimatedElements(prev => prev.map(el => ({ ...el, value: -1 })));

            // Затем плавно показываем новые значения
            setTimeout(() => {
                setAnimatedElements(elementsArray);
            }, 300);
        }
    }, [elementsArray]);

    // Определяем тип анимации на основе операции
    useEffect(() => {
        if (currentStep?.operation === 'assign' && currentStep?.metadata?.variable === 'i') {
            setAnimationState('updating');
        } else if (currentStep?.operation === 'condition' || currentStep?.operation === 'compare') {
            setAnimationState('comparing');
        } else if (currentStep?.operation === 'swap' || currentStep?.description?.toLowerCase().includes('swap') || currentStep?.description?.toLowerCase().includes('обмен')) {
            setAnimationState('swapping');
        } else {
            setAnimationState('idle');
        }
    }, [currentStep]);

    // Обработчики навигации
    const goToStep = useCallback((index: number) => {
        if (index >= 0 && index < steps.length) {
            setCurrentStepIndex(index);
            setHistory(prev => [...prev, index]);
        }
    }, [steps.length]);

    const nextStep = useCallback(() => {
        goToStep(currentStepIndex + 1);
    }, [currentStepIndex, goToStep]);

    const prevStep = useCallback(() => {
        goToStep(currentStepIndex - 1);
    }, [currentStepIndex, goToStep]);

    const firstStep = () => goToStep(0);
    const lastStep = () => goToStep(steps.length - 1);

    // Обработчики для конфигурации переменных
    const toggleVariableConfig = (variable: string) => {
        setVariableConfigs(configs =>
            configs.map(config =>
                config.variable === variable
                    ? { ...config, isActive: !config.isActive }
                    : config
            )
        );
    };

    const updateVariableColor = (variable: string, color: string) => {
        setVariableConfigs(configs =>
            configs.map(config =>
                config.variable === variable
                    ? { ...config, color }
                    : config
            )
        );
    };

    const updateVariableLabel = (variable: string, label: string) => {
        setVariableConfigs(configs =>
            configs.map(config =>
                config.variable === variable
                    ? { ...config, label }
                    : config
            )
        );
    };

    const addVariableConfig = () => {
        if (newVariable && !variableConfigs.some(config => config.variable === newVariable)) {
            // Определяем тип переменной
            let type: 'index' | 'value' | 'metadata' = 'index';
            if (['index1', 'index2', 'value1', 'value2'].includes(newVariable)) {
                type = 'metadata';
            }

            setVariableConfigs([
                ...variableConfigs,
                {
                    variable: newVariable,
                    color: '#3b82f6', // синий по умолчанию
                    label: newVariable,
                    isActive: true,
                    type
                }
            ]);
            setNewVariable('');
        }
    };

    const removeVariableConfig = (variable: string) => {
        setVariableConfigs(configs => configs.filter(config => config.variable !== variable));
    };

    // Обработчик изменения скорости
    const handleSpeedChange = (newSpeed: number) => {
        setCurrentSpeed(newSpeed);
        if (onSpeedChange) {
            onSpeedChange(newSpeed);
        }
    };

    // Автовоспроизведение
    useEffect(() => {
        let intervalId: NodeJS.Timeout;

        if (isPlaying && currentStepIndex < steps.length - 1) {
            intervalId = setInterval(() => {
                nextStep();
            }, 1000 / currentSpeed);
        } else if (isPlaying && currentStepIndex >= steps.length - 1) {
            setIsPlaying(false);
        }

        return () => {
            if (intervalId) clearInterval(intervalId);
        };
    }, [isPlaying, currentStepIndex, steps.length, nextStep, currentSpeed]);

    // Обработчики клавиш
    useEffect(() => {
        const handleKeyPress = (e: KeyboardEvent) => {
            switch (e.key) {
                case 'ArrowRight':
                    nextStep();
                    break;
                case 'ArrowLeft':
                    prevStep();
                    break;
                case ' ':
                    e.preventDefault();
                    setIsPlaying(!isPlaying);
                    break;
                case 'Home':
                    firstStep();
                    break;
                case 'End':
                    lastStep();
                    break;
            }
        };

        window.addEventListener('keydown', handleKeyPress);
        return () => window.removeEventListener('keydown', handleKeyPress);
    }, [nextStep, prevStep, isPlaying]);

    // Определяем цвет для операции
    const getOperationColor = (operation: string) => {
        switch (operation) {
            case 'assign':
                return '#60a5fa'; // blue
            case 'condition':
            case 'compare':
                return '#34d399'; // green
            case 'swap':
                return '#f87171'; // red
            case 'complete':
                return '#10b981'; // green
            default:
                return '#9ca3af'; // gray
        }
    };

    // Определяем иконку для операции
    const getOperationIcon = (operation: string) => {
        switch (operation) {
            case 'assign':
                return '→';
            case 'condition':
            case 'compare':
                return '?';
            case 'swap':
                return '↔';
            case 'complete':
                return '✓';
            default:
                return '↷';
        }
    };

    // Форматирование времени
    const formatTime = (timeStr: string) => {
        try {
            // Убираем лишние цифры после секунд, если есть
            const parts = timeStr.split('.');
            if (parts[0]) {
                return parts[0];
            }
            return timeStr;
        } catch {
            return timeStr;
        }
    };

    // Функция для отображения значения с учетом типа
    const formatValue = (value: any, valueType?: string) => {
        if (value === undefined || value === null) return 'null';

        // Обработка объектов с rawValue
        if (typeof value === 'object' && value !== null) {
            // Если это объект с rawValue (как index1: {type: 1, rawValue: 4})
            if ('rawValue' in value) {
                return formatValue(value.rawValue, valueType);
            }
            // Если это простой объект, показываем JSON
            return JSON.stringify(value);
        }

        if (valueType?.includes('String')) {
            return `"${value}"`;
        } else if (valueType?.includes('Int') || valueType?.includes('Double')) {
            return value;
        } else if (valueType?.includes('Bool')) {
            return value ? 'true' : 'false';
        }

        return String(value);
    };

    // Компонент для отображения метаданных
    const MetadataDisplay = ({ metadata }: { metadata: Metadata }) => {
        const entries = Object.entries(metadata);

        if (entries.length === 0 || (entries.length === 1 && entries[0][0] === 'visualization_type')) {
            return <div style={{ color: '#6b7280', fontStyle: 'italic' }}>Нет метаданных</div>;
        }

        return (
            <div style={{
                display: 'grid',
                gridTemplateColumns: 'repeat(auto-fill, minmax(250px, 1fr))',
                gap: '10px',
                marginTop: '10px'
            }}>
                {entries.map(([key, value]) => {
                    // Пропускаем visualization_type, так как он не несет полезной информации
                    if (key === 'visualization_type') return null;

                    return (
                        <div key={key} style={{
                            backgroundColor: '#f9fafb',
                            borderRadius: '6px',
                            padding: '8px 12px',
                            border: '1px solid #e5e7eb'
                        }}>
                            <div style={{
                                display: 'flex',
                                alignItems: 'center',
                                marginBottom: '4px'
                            }}>
                                <div style={{
                                    backgroundColor: '#e5e7eb',
                                    color: '#374151',
                                    fontSize: '11px',
                                    fontWeight: '600',
                                    padding: '2px 6px',
                                    borderRadius: '4px',
                                    textTransform: 'uppercase'
                                }}>
                                    {key}
                                </div>
                            </div>

                            <div style={{
                                fontFamily: 'monospace',
                                fontSize: '14px',
                                wordBreak: 'break-all',
                                color: key === 'result' ? (value === true ? '#059669' : '#dc2626') : '#111827'
                            }}>
                                {key === 'result' ? (value ? '✅ true' : '❌ false') : formatValue(value, metadata.value_type)}
                            </div>

                            {key === 'expression' && value && (
                                <div style={{
                                    marginTop: '4px',
                                    fontSize: '12px',
                                    color: '#6b7280',
                                    fontStyle: 'italic'
                                }}>
                                    Выражение: {formatValue(value)}
                                </div>
                            )}

                            {key === 'condition' && value && (
                                <div style={{
                                    marginTop: '4px',
                                    fontSize: '12px',
                                    color: '#6b7280',
                                    fontStyle: 'italic'
                                }}>
                                    Условие: {formatValue(value)}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>
        );
    };

    // Получаем цвет для анимации
    const getAnimationColor = () => {
        switch (animationState) {
            case 'comparing': return '#fbbf24'; // желтый
            case 'swapping': return '#ef4444'; // красный
            case 'updating': return '#3b82f6'; // синий
            default: return '#6b7280'; // серый
        }
    };

    // Получаем текст для анимации
    const getAnimationText = () => {
        switch (animationState) {
            case 'comparing': return 'Сравнение...';
            case 'swapping': return 'Обмен...';
            case 'updating': return 'Обновление...';
            default: return 'Ожидание...';
        }
    };

    // Функция для форматирования переменных для отображения
    const formatVariableValue = (key: string, value: any): string => {
        if (value === undefined || value === null) return 'undefined';

        // Если это массив или объект
        if (typeof value === 'object') {
            if (Array.isArray(value)) {
                // Если массив содержит объекты с RawValue
                if (value.length > 0 && typeof value[0] === 'object' && 'RawValue' in value[0]) {
                    return `[${value.map((item: any) => item.RawValue).join(', ')}]`;
                }
                return `[${value.join(', ')}]`;
            }

            // Если это объект struct
            if (key === 'struct') {
                if ('values' in value && Array.isArray(value.values)) {
                    return `Массив[${value.values.length}]`;
                }
                return 'Объект struct';
            }

            return JSON.stringify(value);
        }

        return String(value);
    };

    // Стили для компонента
    const styles = {
        container: {
            backgroundColor: '#ffffff',
            borderRadius: '12px',
            boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
            padding: '24px',
            fontFamily: '"Segoe UI", system-ui, -apple-system, sans-serif'
        },
        header: {
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            marginBottom: '24px',
            borderBottom: '2px solid #e5e7eb',
            paddingBottom: '16px'
        },
        title: {
            fontSize: '28px',
            fontWeight: '700',
            color: '#1f2937',
            margin: 0
        },
        subtitle: {
            fontSize: '14px',
            color: '#6b7280',
            marginTop: '4px'
        },
        controls: {
            display: 'flex',
            flexWrap: 'wrap' as const,
            alignItems: 'center',
            gap: '8px',
            backgroundColor: '#f9fafb',
            borderRadius: '10px',
            padding: '16px',
            marginBottom: '20px'
        },
        button: {
            padding: '8px 16px',
            borderRadius: '8px',
            border: 'none',
            cursor: 'pointer',
            fontWeight: '600',
            fontSize: '14px',
            transition: 'all 0.2s',
            display: 'flex',
            alignItems: 'center',
            gap: '6px'
        },
        buttonPrimary: {
            backgroundColor: '#3b82f6',
            color: 'white'
        },
        buttonSecondary: {
            backgroundColor: '#e5e7eb',
            color: '#374151'
        },
        buttonSuccess: {
            backgroundColor: '#10b981',
            color: 'white'
        },
        buttonDanger: {
            backgroundColor: '#ef4444',
            color: 'white'
        },
        buttonDisabled: {
            opacity: 0.5,
            cursor: 'not-allowed'
        },
        stepInfo: {
            backgroundColor: '#f8fafc',
            borderRadius: '10px',
            padding: '20px',
            marginBottom: '24px',
            border: '1px solid #e2e8f0',
            overflow: 'hidden',
            transition: 'all 0.3s ease-in-out'
        },
        stepHeader: {
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            marginBottom: '16px'
        },
        operationBadge: {
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            width: '36px',
            height: '36px',
            borderRadius: '50%',
            fontWeight: 'bold',
            fontSize: '16px',
            color: 'white'
        },
        stepDescription: {
            fontSize: '18px',
            fontWeight: '600',
            color: '#1f2937',
            margin: 0,
            flex: 1
        },
        arrayContainer: {
            display: 'flex',
            flexWrap: 'wrap' as const,
            gap: '12px',
            justifyContent: 'center',
            marginBottom: '32px',
            padding: '20px',
            backgroundColor: '#f8fafc',
            borderRadius: '10px',
            minHeight: '150px'
        },
        arrayElement: {
            position: 'relative' as const,
            width: '80px',
            height: '80px',
            display: 'flex',
            flexDirection: 'column' as const,
            alignItems: 'center',
            justifyContent: 'center',
            border: '2px solid #cbd5e1',
            borderRadius: '8px',
            backgroundColor: 'white',
            boxShadow: '0 2px 4px rgba(0, 0, 0, 0.05)',
            transition: 'all 0.3s ease-in-out'
        },
        elementIndex: {
            position: 'absolute' as const,
            top: '4px',
            left: '4px',
            fontSize: '12px',
            color: '#64748b',
            fontWeight: '600'
        },
        elementValue: {
            fontSize: '22px',
            fontWeight: '700',
            color: '#1e293b',
            transition: 'all 0.3s ease-in-out'
        },
        elementLabel: {
            marginTop: '4px',
            fontSize: '12px',
            color: '#64748b'
        },
        statsContainer: {
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(180px, 1fr))',
            gap: '16px',
            marginBottom: '32px'
        },
        statCard: {
            backgroundColor: '#f1f5f9',
            borderRadius: '10px',
            padding: '20px',
            textAlign: 'center' as const
        },
        statValue: {
            fontSize: '28px',
            fontWeight: '800',
            color: '#0f172a',
            margin: '8px 0'
        },
        statLabel: {
            fontSize: '14px',
            color: '#475569',
            fontWeight: '600',
            textTransform: 'uppercase' as const,
            letterSpacing: '0.5px'
        },
        variablesContainer: {
            backgroundColor: '#f8fafc',
            borderRadius: '10px',
            padding: '20px',
            marginBottom: '24px'
        },
        variablesGrid: {
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))',
            gap: '12px',
            marginTop: '16px'
        },
        variableItem: {
            backgroundColor: 'white',
            border: '1px solid #e2e8f0',
            borderRadius: '8px',
            padding: '12px',
            fontFamily: 'monospace'
        },
        stepNavigation: {
            display: 'flex',
            flexWrap: 'wrap' as const,
            gap: '8px',
            maxHeight: '120px',
            overflowY: 'auto' as const,
            backgroundColor: '#f8fafc',
            borderRadius: '10px',
            padding: '16px',
            marginBottom: '24px'
        },
        stepButton: {
            padding: '8px 12px',
            borderRadius: '6px',
            border: '1px solid #cbd5e1',
            backgroundColor: 'white',
            cursor: 'pointer',
            minWidth: '50px',
            fontSize: '14px',
            fontWeight: '600',
            transition: 'all 0.2s'
        },
        animationIndicator: {
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            gap: '10px',
            marginBottom: '20px',
            padding: '12px',
            backgroundColor: '#fef3c7',
            borderRadius: '8px',
            border: `2px solid ${getAnimationColor()}`
        },
        variableConfigPanel: {
            backgroundColor: '#f8fafc',
            borderRadius: '10px',
            padding: '20px',
            marginBottom: '24px',
            border: '1px solid #e2e8f0'
        },
        configRow: {
            display: 'flex',
            alignItems: 'center',
            gap: '12px',
            marginBottom: '12px',
            padding: '12px',
            backgroundColor: 'white',
            borderRadius: '8px',
            border: '1px solid #e5e7eb'
        },
        stepInfoContent: {
            maxHeight: stepInfoHeight === 'auto' ? 'none' : `${stepInfoHeight}px`,
            overflowY: stepInfoHeight === 'auto' ? 'visible' : 'auto',
            transition: 'max-height 0.3s ease-in-out'
        },
        heightControls: {
            display: 'flex',
            alignItems: 'center',
            gap: '8px',
            marginTop: '16px',
            padding: '8px',
            backgroundColor: '#f1f5f9',
            borderRadius: '6px'
        }
    };

    // Получаем текущие переменные для отображения
    const currentVariables = useMemo(() => {
        if (!currentStep?.variables) return {};

        const vars: Record<string, any> = {};
        Object.entries(currentStep.variables).forEach(([key, value]) => {
            // Пропускаем выбранный массив, так как он отображается отдельно
            if (key !== selectedArray) {
                vars[key] = value;
            }
        });
        return vars;
    }, [currentStep, selectedArray]);

    return (
        <div style={styles.container} className={className}>
            {/* Заголовок и управление */}
            <div style={styles.header}>
                <div>
                    <h1 style={styles.title}>{algorithmName} - Визуализация алгоритма</h1>
                    <div style={styles.subtitle}>
                        Session ID: {data.sessionId} •
                        Тип структуры: {data.structureType} •
                        Всего шагов: {steps.length}
                    </div>
                </div>
                <div style={{ textAlign: 'right' }}>
                    <div style={{ fontSize: '16px', fontWeight: '600', color: '#1f2937' }}>
                        Шаг {currentStepIndex + 1} из {steps.length}
                    </div>
                    <div style={{ fontSize: '14px', color: '#6b7280' }}>
                        {formatTime(executionTime)}
                    </div>
                </div>
            </div>

            {/* Панель управления */}
            <div style={styles.controls}>
                <button
                    onClick={firstStep}
                    disabled={currentStepIndex === 0}
                    style={{
                        ...styles.button,
                        ...styles.buttonSecondary,
                        ...(currentStepIndex === 0 ? styles.buttonDisabled : {})
                    }}
                >
                    ↞ Первый
                </button>
                <button
                    onClick={prevStep}
                    disabled={currentStepIndex === 0}
                    style={{
                        ...styles.button,
                        ...styles.buttonSecondary,
                        ...(currentStepIndex === 0 ? styles.buttonDisabled : {})
                    }}
                >
                    ← Назад
                </button>

                <button
                    onClick={() => setIsPlaying(!isPlaying)}
                    style={{
                        ...styles.button,
                        ...styles.buttonSuccess,
                        minWidth: '140px'
                    }}
                >
                    {isPlaying ? '⏸ Пауза' : '▶ Воспроизвести'}
                </button>

                <button
                    onClick={nextStep}
                    disabled={currentStepIndex === steps.length - 1}
                    style={{
                        ...styles.button,
                        ...styles.buttonSecondary,
                        ...(currentStepIndex === steps.length - 1 ? styles.buttonDisabled : {})
                    }}
                >
                    Вперёд →
                </button>
                <button
                    onClick={lastStep}
                    disabled={currentStepIndex === steps.length - 1}
                    style={{
                        ...styles.button,
                        ...styles.buttonSecondary,
                        ...(currentStepIndex === steps.length - 1 ? styles.buttonDisabled : {})
                    }}
                >
                    Последний ↠
                </button>

                {/* Выбор массива */}
                {availableArrays.length > 0 && (
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                        <span style={{ fontSize: '14px', color: '#374151', fontWeight: '500' }}>Массив:</span>
                        <select
                            value={selectedArray}
                            onChange={(e) => setSelectedArray(e.target.value)}
                            style={{
                                padding: '8px 12px',
                                borderRadius: '6px',
                                border: '1px solid #d1d5db',
                                backgroundColor: 'white',
                                fontSize: '14px',
                                minWidth: '120px'
                            }}
                        >
                            {availableArrays.map(array => (
                                <option key={array} value={array}>
                                    {array} ({extractArrayFromVariables(currentStep.variables, array).length} элементов)
                                </option>
                            ))}
                        </select>
                    </div>
                )}

                {/* Скорость воспроизведения */}
                <div style={{ marginLeft: 'auto', display: 'flex', alignItems: 'center', gap: '8px' }}>
                    <span style={{ fontSize: '14px', color: '#374151', fontWeight: '500' }}>Скорость:</span>
                    <select
                        value={currentSpeed}
                        onChange={(e) => handleSpeedChange(Number(e.target.value))}
                        style={{
                            padding: '8px 12px',
                            borderRadius: '6px',
                            border: '1px solid #d1d5db',
                            backgroundColor: 'white',
                            fontSize: '14px'
                        }}
                    >
                        <option value="0.5">0.5x</option>
                        <option value="1">1x</option>
                        <option value="2">2x</option>
                        <option value="4">4x</option>
                        <option value="8">8x</option>
                    </select>
                </div>

                <button
                    onClick={() => setShowAllMetadata(!showAllMetadata)}
                    style={{
                        ...styles.button,
                        backgroundColor: showAllMetadata ? '#7c3aed' : '#8b5cf6',
                        color: 'white'
                    }}
                >
                    {showAllMetadata ? '▲ Скрыть метаданные' : '▼ Показать все метаданные'}
                </button>

                <button
                    onClick={() => setShowVariableConfig(!showVariableConfig)}
                    style={{
                        ...styles.button,
                        backgroundColor: showVariableConfig ? '#f59e0b' : '#fbbf24',
                        color: 'white'
                    }}
                >
                    {showVariableConfig ? '▲ Скрыть настройки' : '▼ Настройки переменных'}
                </button>
            </div>

            {/* Панель настройки переменных */}
            {showVariableConfig && (
                <div style={styles.variableConfigPanel}>
                    <h3 style={{ fontSize: '20px', fontWeight: '600', color: '#1f2937', marginBottom: '16px' }}>
                        Настройка отображения переменных
                    </h3>

                    <div style={{ marginBottom: '20px' }}>
                        <h4 style={{ fontSize: '16px', fontWeight: '500', color: '#4b5563', marginBottom: '12px' }}>
                            Доступные переменные:
                        </h4>
                        <div style={{ display: 'flex', flexWrap: 'wrap', gap: '8px' }}>
                            {availableVariables.map(variable => (
                                <button
                                    key={variable}
                                    onClick={() => {
                                        if (!variableConfigs.some(config => config.variable === variable)) {
                                            setNewVariable(variable);
                                            addVariableConfig();
                                        }
                                    }}
                                    style={{
                                        padding: '6px 12px',
                                        borderRadius: '6px',
                                        border: '1px solid #d1d5db',
                                        backgroundColor: variableConfigs.some(config => config.variable === variable)
                                            ? '#3b82f6'
                                            : 'white',
                                        color: variableConfigs.some(config => config.variable === variable)
                                            ? 'white'
                                            : '#374151',
                                        fontSize: '14px',
                                        cursor: 'pointer'
                                    }}
                                >
                                    {variable}
                                </button>
                            ))}
                        </div>
                    </div>

                    <div style={{ marginBottom: '20px' }}>
                        <div style={{ display: 'flex', gap: '12px', alignItems: 'center', marginBottom: '16px' }}>
                            <input
                                type="text"
                                placeholder="Имя переменной (например: temp)"
                                value={newVariable}
                                onChange={(e) => setNewVariable(e.target.value)}
                                style={{
                                    padding: '8px 12px',
                                    borderRadius: '6px',
                                    border: '1px solid #d1d5db',
                                    flex: 1
                                }}
                            />
                            <button
                                onClick={addVariableConfig}
                                style={{
                                    padding: '8px 16px',
                                    backgroundColor: '#10b981',
                                    color: 'white',
                                    border: 'none',
                                    borderRadius: '6px',
                                    cursor: 'pointer',
                                    fontWeight: '600'
                                }}
                            >
                                Добавить
                            </button>
                        </div>
                    </div>

                    <div>
                        <h4 style={{ fontSize: '16px', fontWeight: '500', color: '#4b5563', marginBottom: '12px' }}>
                            Настроенные переменные:
                        </h4>
                        {variableConfigs.map(config => (
                            <div key={config.variable} style={styles.configRow}>
                                <input
                                    type="checkbox"
                                    checked={config.isActive}
                                    onChange={() => toggleVariableConfig(config.variable)}
                                    style={{ cursor: 'pointer' }}
                                />
                                <div style={{ minWidth: '100px', fontWeight: '600', color: '#374151' }}>
                                    {config.variable}
                                </div>
                                <input
                                    type="text"
                                    value={config.label}
                                    onChange={(e) => updateVariableLabel(config.variable, e.target.value)}
                                    style={{
                                        padding: '6px 12px',
                                        borderRadius: '4px',
                                        border: '1px solid #d1d5db',
                                        width: '100px'
                                    }}
                                />
                                <input
                                    type="color"
                                    value={config.color}
                                    onChange={(e) => updateVariableColor(config.variable, e.target.value)}
                                    style={{
                                        width: '50px',
                                        height: '40px',
                                        border: '1px solid #d1d5db',
                                        borderRadius: '4px',
                                        cursor: 'pointer'
                                    }}
                                />
                                <div style={{
                                    width: '40px',
                                    height: '40px',
                                    backgroundColor: config.color,
                                    borderRadius: '4px',
                                    border: '1px solid #d1d5db'
                                }} />
                                <div style={{ marginLeft: 'auto', fontSize: '14px', color: '#6b7280' }}>
                                    {config.type}
                                </div>
                                <button
                                    onClick={() => removeVariableConfig(config.variable)}
                                    style={{
                                        padding: '6px 12px',
                                        backgroundColor: '#ef4444',
                                        color: 'white',
                                        border: 'none',
                                        borderRadius: '6px',
                                        cursor: 'pointer',
                                        fontWeight: '600'
                                    }}
                                >
                                    Удалить
                                </button>
                            </div>
                        ))}
                    </div>

                    <div style={{ marginTop: '16px', padding: '12px', backgroundColor: '#fef3c7', borderRadius: '8px' }}>
                        <p style={{ fontSize: '14px', color: '#92400e', margin: 0 }}>
                            <strong>Как это работает:</strong> Переменные, помеченные как активные, будут отображаться на массиве.
                            Переменные типа "index" берутся из значений переменных (i, j, pivot_index и т.д.).
                            Переменные типа "metadata" берутся из метаданных операции (index1, index2 для операций swap/compare).
                        </p>
                    </div>
                </div>
            )}

            {/* Информация о текущем шаге */}
            <div style={styles.stepInfo}>
                <div style={styles.stepHeader}>
                    <div style={{
                        ...styles.operationBadge,
                        backgroundColor: getOperationColor(currentStep.operation),
                        animation: animationState !== 'idle' ? 'pulse 1.5s infinite' : 'none'
                    }}>
                        {getOperationIcon(currentStep.operation)}
                    </div>
                    <h2 style={styles.stepDescription}>
                        Шаг {currentStep.stepNumber}: {currentStep.description}
                    </h2>
                    <div style={{
                        fontSize: '14px',
                        color: '#6b7280',
                        backgroundColor: '#f3f4f6',
                        padding: '4px 12px',
                        borderRadius: '20px',
                        fontWeight: '500'
                    }}>
                        {currentStep.operation || 'navigate'}
                    </div>
                </div>

                {/* Управление высотой блока с информацией о шаге */}
                <div style={styles.heightControls}>
                    <span style={{ fontSize: '14px', color: '#374151', fontWeight: '500' }}>Высота информации:</span>
                    <button
                        onClick={() => setStepInfoHeight(150)}
                        style={{
                            padding: '4px 12px',
                            backgroundColor: stepInfoHeight === 150 ? '#3b82f6' : '#e5e7eb',
                            color: stepInfoHeight === 150 ? 'white' : '#374151',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer',
                            fontSize: '12px'
                        }}
                    >
                        Маленькая
                    </button>
                    <button
                        onClick={() => setStepInfoHeight(300)}
                        style={{
                            padding: '4px 12px',
                            backgroundColor: stepInfoHeight === 300 ? '#3b82f6' : '#e5e7eb',
                            color: stepInfoHeight === 300 ? 'white' : '#374151',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer',
                            fontSize: '12px'
                        }}
                    >
                        Средняя
                    </button>
                    <button
                        onClick={() => setStepInfoHeight(450)}
                        style={{
                            padding: '4px 12px',
                            backgroundColor: stepInfoHeight === 450 ? '#3b82f6' : '#e5e7eb',
                            color: stepInfoHeight === 450 ? 'white' : '#374151',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer',
                            fontSize: '12px'
                        }}
                    >
                        Большая
                    </button>
                    <button
                        onClick={() => setStepInfoHeight('auto')}
                        style={{
                            padding: '4px 12px',
                            backgroundColor: stepInfoHeight === 'auto' ? '#3b82f6' : '#e5e7eb',
                            color: stepInfoHeight === 'auto' ? 'white' : '#374151',
                            border: 'none',
                            borderRadius: '4px',
                            cursor: 'pointer',
                            fontSize: '12px'
                        }}
                    >
                        Авто
                    </button>
                </div>

                {/* Основные метаданные */}
                <div style={styles.stepInfoContent}>
                    <MetadataDisplay metadata={currentStep.metadata} />
                </div>

                {/* Расширенные метаданные (показываются по клику) */}
                {showAllMetadata && (
                    <div style={{ marginTop: '20px', paddingTop: '20px', borderTop: '2px dashed #e5e7eb' }}>
                        <h3 style={{ fontSize: '16px', fontWeight: '600', color: '#374151', marginBottom: '12px' }}>
                            Детальные метаданные шага:
                        </h3>
                        <div style={{
                            backgroundColor: '#f9fafb',
                            borderRadius: '8px',
                            padding: '16px',
                            fontFamily: 'monospace',
                            fontSize: '13px',
                            color: '#374151',
                            whiteSpace: 'pre-wrap',
                            overflowX: 'auto',
                            maxHeight: '300px',
                            overflowY: 'auto'
                        }}>
                            {JSON.stringify(currentStep.metadata, null, 2)}
                        </div>
                    </div>
                )}
            </div>

            {/* Визуализация массива */}
            <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
                    <h3 style={{ fontSize: '20px', fontWeight: '600', color: '#1f2937' }}>
                        Визуализация массива ({selectedArray})
                    </h3>
                    <div style={{ fontSize: '14px', color: '#6b7280' }}>
                        Элементов: {animatedElements.length}
                    </div>
                </div>

                <div style={styles.arrayContainer}>
                    {animatedElements.map((element) => {
                        const highlight = highlightedElements.find(h => h.index === element.index);
                        const isHighlighted = !!highlight;
                        const isUpdating = element.value === -1;

                        return (
                            <div
                                key={`${element.index}-${currentStepIndex}`}
                                style={{
                                    ...styles.arrayElement,
                                    borderColor: isHighlighted ? highlight.color : '#cbd5e1',
                                    boxShadow: isHighlighted
                                        ? `0 0 0 3px ${highlight.color}40`
                                        : '0 2px 4px rgba(0, 0, 0, 0.05)',
                                    transform: isHighlighted ? 'translateY(-5px)' : 'translateY(0)',
                                    animation: isHighlighted ? 'bounce 0.5s ease-in-out infinite' : 'none'
                                }}
                            >
                                <div style={styles.elementIndex}>
                                    [{element.index}]
                                </div>
                                <div style={{
                                    ...styles.elementValue,
                                    opacity: isUpdating ? 0 : 1,
                                    transform: isUpdating ? 'scale(0.8)' : 'scale(1)'
                                }}>
                                    {isUpdating ? '?' : element.value}
                                </div>
                                <div style={styles.elementLabel}>
                                    {element.label}
                                </div>
                                {isHighlighted && (
                                    <div style={{
                                        position: 'absolute',
                                        bottom: '-10px',
                                        backgroundColor: highlight.color,
                                        color: 'white',
                                        fontSize: '10px',
                                        padding: '2px 8px',
                                        borderRadius: '12px',
                                        fontWeight: '600',
                                        zIndex: 10
                                    }}>
                                        {highlight.label}
                                    </div>
                                )}
                            </div>
                        );
                    })}
                </div>

                {/* Информация о выделенных элементах */}
                {highlightedElements.length > 0 && (
                    <div style={{
                        marginTop: '16px',
                        padding: '12px',
                        backgroundColor: '#fef3c7',
                        borderRadius: '8px',
                        fontSize: '14px',
                        color: '#92400e'
                    }}>
                        <strong>Выделенные элементы:</strong>{' '}
                        {highlightedElements.map((h, i) => (
                            <span key={h.index} style={{ marginLeft: '8px' }}>
                                <span style={{
                                    display: 'inline-block',
                                    width: '12px',
                                    height: '12px',
                                    backgroundColor: h.color,
                                    borderRadius: '2px',
                                    marginRight: '4px',
                                    verticalAlign: 'middle'
                                }}></span>
                                [{h.index}] = {h.label}
                                {i < highlightedElements.length - 1 && ', '}
                            </span>
                        ))}
                    </div>
                )}
            </div>

            {/* Статистика */}
            <div>
                <h3 style={{ fontSize: '20px', fontWeight: '600', color: '#1f2937', marginBottom: '16px' }}>
                    Статистика выполнения
                </h3>
                <div style={styles.statsContainer}>
                    <div style={styles.statCard}>
                        <div style={styles.statLabel}>Всего шагов</div>
                        <div style={styles.statValue}>{statistics.steps}</div>
                    </div>
                    <div style={styles.statCard}>
                        <div style={styles.statLabel}>Сравнения</div>
                        <div style={styles.statValue}>{statistics.comparisons}</div>
                    </div>
                    <div style={styles.statCard}>
                        <div style={styles.statLabel}>Обмены</div>
                        <div style={styles.statValue}>{statistics.swaps}</div>
                    </div>
                    <div style={styles.statCard}>
                        <div style={styles.statLabel}>Время выполнения</div>
                        <div style={styles.statValue}>{formatTime(executionTime)}</div>
                    </div>
                </div>
            </div>

            {/* Состояние выполнения */}
            <div style={styles.variablesContainer}>
                <h3 style={{ fontSize: '20px', fontWeight: '600', color: '#1f2937', marginBottom: '16px' }}>
                    Текущие значения переменных
                </h3>
                <div style={styles.variablesGrid}>
                    {Object.entries(currentVariables).map(([key, value]) => {
                        const config = variableConfigs.find(c => c.variable === key);
                        const isChanging = currentStep.metadata?.variable === key;

                        return (
                            <div key={key} style={{
                                ...styles.variableItem,
                                borderColor: config?.isActive ? config.color : '#e2e8f0',
                                backgroundColor: isChanging ? '#eff6ff' : 'white',
                                transform: isChanging ? 'scale(1.02)' : 'scale(1)',
                                transition: 'all 0.3s'
                            }}>
                                <div style={{
                                    fontSize: '12px',
                                    color: config?.isActive ? config.color : '#6b7280',
                                    marginBottom: '4px',
                                    fontWeight: '600',
                                    display: 'flex',
                                    alignItems: 'center',
                                    gap: '4px'
                                }}>
                                    {key}:
                                    {config?.isActive && (
                                        <span style={{
                                            fontSize: '10px',
                                            color: config.color,
                                            display: 'inline-block',
                                            width: '8px',
                                            height: '8px',
                                            backgroundColor: config.color,
                                            borderRadius: '50%'
                                        }}></span>
                                    )}
                                </div>
                                <div style={{
                                    fontSize: '14px',
                                    fontWeight: '700',
                                    color: isChanging ? '#1d4ed8' : '#111827',
                                    wordBreak: 'break-all'
                                }}>
                                    {formatVariableValue(key, value)}
                                </div>
                            </div>
                        );
                    })}
                </div>
            </div>

            {/* Навигация по шагам */}
            <div>
                <h3 style={{ fontSize: '20px', fontWeight: '600', color: '#1f2937', marginBottom: '16px' }}>
                    Навигация по шагам
                </h3>
                <div style={styles.stepNavigation}>
                    {steps.map((step, index) => (
                        <button
                            key={step.stepNumber}
                            onClick={() => goToStep(index)}
                            style={{
                                ...styles.stepButton,
                                backgroundColor: index === currentStepIndex ? '#3b82f6' : 'white',
                                color: index === currentStepIndex ? 'white' : '#374151',
                                borderColor: index === currentStepIndex ? '#2563eb' : '#cbd5e1',
                                transform: index === currentStepIndex ? 'scale(1.05)' : 'scale(1)'
                            }}
                            title={`Шаг ${step.stepNumber}: ${step.operation} - ${step.description}`}
                        >
                            {step.stepNumber}
                        </button>
                    ))}
                </div>
            </div>

            {/* CSS анимации */}
            <style>{`
        @keyframes pulse {
          0%, 100% { opacity: 1; }
          50% { opacity: 0.5; }
        }
        
        @keyframes bounce {
          0%, 100% { transform: translateY(-5px); }
          50% { transform: translateY(0); }
        }
        
        @keyframes fadeIn {
          from { opacity: 0; transform: scale(0.9); }
          to { opacity: 1; transform: scale(1); }
        }
        
        .array-element {
          animation: fadeIn 0.3s ease-out;
        }
        
        .updating {
          animation: pulse 1s infinite;
        }
      `}</style>

            {/* Горячие клавиши */}
            <div style={{
                marginTop: '24px',
                paddingTop: '16px',
                borderTop: '2px solid #e5e7eb',
                fontSize: '14px',
                color: '#6b7280'
            }}>
                <h4 style={{ fontWeight: '600', marginBottom: '8px', color: '#374151' }}>Управление:</h4>
                <div style={{ display: 'flex', flexWrap: 'wrap', gap: '16px' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                        <kbd style={{
                            backgroundColor: '#e5e7eb',
                            padding: '4px 8px',
                            borderRadius: '4px',
                            fontFamily: 'monospace',
                            fontSize: '12px',
                            fontWeight: '600'
                        }}>←</kbd>
                        <span>Назад</span>
                    </div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                        <kbd style={{
                            backgroundColor: '#e5e7eb',
                            padding: '4px 8px',
                            borderRadius: '4px',
                            fontFamily: 'monospace',
                            fontSize: '12px',
                            fontWeight: '600'
                        }}>→</kbd>
                        <span>Вперёд</span>
                    </div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
                        <kbd style={{
                            backgroundColor: '#e5e7eb',
                            padding: '4px 8px',
                            borderRadius: '4px',
                            fontFamily: 'monospace',
                            fontSize: '12px',
                            fontWeight: '600'
                        }}>Пробел</kbd>
                        <span>Воспроизвести/Пауза</span>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AlgorithmVisualizer;