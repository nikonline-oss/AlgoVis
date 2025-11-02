import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';

type Language = 'ru' | 'en';
type Theme = 'light' | 'dark';
type DataStructureType = 'array' | 'tree' | 'graph' | 'list' | 'stack' | 'queue' | 'heap' | 'hashtable';

export interface SharedVisualizationData {
  type: DataStructureType;
  algorithm?: string;
  data?: any;
  stats?: {
    comparisons?: number;
    swaps?: number;
    operations?: number;
    time?: number;
  };
}

interface AppContextType {
  language: Language;
  setLanguage: (lang: Language) => void;
  theme: Theme;
  setTheme: (theme: Theme) => void;
  translations: Record<string, string>;
  sharedData: SharedVisualizationData | null;
  setSharedData: (data: SharedVisualizationData | null) => void;
}

const translations = {
  ru: {
    'site.title': 'Визуализатор Алгоритмов',
    'nav.home': 'Главная',
    'nav.visualizer': 'Визуализатор',
    'nav.profiler': 'Профайлер',
    'nav.analyzer': 'Анализатор',
    'nav.help': 'Справка',
    'hero.title': 'Интерактивный Визуализатор Алгоритмов',
    'hero.subtitle': 'Изучайте алгоритмы и структуры данных через визуализацию',
    'hero.description': 'Наш инструмент поможет вам понять, как работают различные алгоритмы сортировки, поиска и структуры данных через интерактивные анимации.',
    'hero.start': 'Начать изучение',
    'features.title': 'Возможности',
    'features.visual.title': 'Визуальное обучение',
    'features.visual.desc': 'Смотрите как работают алгоритмы в реальном времени',
    'features.interactive.title': 'Интерактивность',
    'features.interactive.desc': 'Управляйте скоростью и параметрами выполнения',
    'features.analysis.title': 'Анализ производительности',
    'features.analysis.desc': 'Сравнивайте временную сложность алгоритмов',
    'controls.play': 'Воспроизвести',
    'controls.pause': 'Пауза',
    'controls.forward': 'Вперед',
    'controls.backward': 'Назад',
    'controls.reset': 'Сброс',
    'controls.speed': 'Скорость',
    'structure.select': 'Структура данных',
    'structure.array': 'Массив',
    'structure.tree': 'Дерево',
    'structure.graph': 'Граф',
    'structure.list': 'Список',
    'structure.stack': 'Стек',
    'structure.queue': 'Очередь',
    'structure.heap': 'Куча',
    'structure.hashtable': 'Хэш-таблица',
    'algorithm.select': 'Выберите алгоритм',
    'algorithm.bubblesort': 'Пузырьковая сортировка',
    'algorithm.quicksort': 'Быстрая сортировка',
    'algorithm.insertionsort': 'Сортировка вставками',
    'algorithm.selectionsort': 'Сортировка выбором',
    'algorithm.bst.inorder': 'Инордерный обход',
    'algorithm.bst.preorder': 'Прямой обход',
    'algorithm.bst.postorder': 'Обратный обход',
    'algorithm.bst.levelorder': 'Обход в ширину',
    'algorithm.bfs': 'Поиск в ширину (BFS)',
    'algorithm.dfs': 'Поиск в глубину (DFS)',
    'data.generate': 'Генерировать данные',
    'data.size': 'Размер',
    'data.value': 'Значение',
    'data.insert': 'Вставить',
    'data.search': 'Найти',
    'data.delete': 'Удалить',
    'list.type': 'Тип списка',
    'list.singly': 'Односвязный',
    'list.doubly': 'Двусвязный',
    'list.position': 'Позиция',
    'list.insertAtPosition': 'Вставить на позицию',
    'list.deleteAtPosition': 'Удалить на позиции',
    'heap.type': 'Тип кучи',
    'heap.max': 'Макс-куча',
    'heap.min': 'Мин-куча',
    'hashtable.size': 'Размер таблицы',
    'hashtable.key': 'Ключ',
    'profiler.title': 'Анализ производительности',
    'profiler.compare': 'Сравнить алгоритмы',
    'profiler.run': 'Запустить тест',
    'profiler.time': 'Время выполнения',
    'profiler.comparisons': 'Сравнения',
    'profiler.swaps': 'Перестановки',
    'profiler.operations': 'Операции',
    'profiler.sendToVisualizer': 'Отправить в визуализатор',
    'analyzer.title': 'Анализатор кода',
    'analyzer.description': 'Анализируйте сложность и производительность вашего Python кода',
    'analyzer.codeInput': 'Ввод кода',
    'analyzer.codeInputDesc': 'Вставьте ваш код на Python для анализа',
    'analyzer.placeholder': 'Вставьте ваш код на Python здесь...',
    'analyzer.lines': 'строк',
    'analyzer.analyze': 'Анализировать',
    'analyzer.analyzing': 'Анализ...',
    'analyzer.clear': 'Очистить',
    'analyzer.loadExample': 'Загрузить пример',
    'analyzer.results': 'Результаты анализа',
    'analyzer.resultsDesc': 'Результаты анализа производительности кода',
    'analyzer.noResults': 'Результаты анализа появятся здесь',
    'analyzer.analyzingMessage': 'Анализируем ваш код...',
    'analyzer.emptyCode': 'Пожалуйста, введите код для анализа',
    'analyzer.success': 'Анализ завершен успешно',
    'analyzer.error': 'Ошибка при анализе кода',
    'analyzer.complexity': 'Временная сложность:',
    'analyzer.warnings': 'Предупреждения:',
    'analyzer.suggestions': 'Рекомендации:',
    'analyzer.note': 'Примечание: Это демо-версия. Для реальной работы требуется настройка бэкенда.',
    'analyzer.infoTitle': 'Что анализирует инструмент?',
    'analyzer.info1': 'Временная и пространственная сложность алгоритмов',
    'analyzer.info2': 'Потенциальные проблемы с производительностью',
    'analyzer.info3': 'Рекомендации по оптимизации кода',
  },
  en: {
    'site.title': 'Algorithm Visualizer',
    'nav.home': 'Home',
    'nav.visualizer': 'Visualizer',
    'nav.profiler': 'Profiler',
    'nav.analyzer': 'Code Analyzer',
    'nav.help': 'Help',
    'hero.title': 'Interactive Algorithm Visualizer',
    'hero.subtitle': 'Learn algorithms and data structures through visualization',
    'hero.description': 'Our tool helps you understand how various sorting, searching algorithms and data structures work through interactive animations.',
    'hero.start': 'Start Learning',
    'features.title': 'Features',
    'features.visual.title': 'Visual Learning',
    'features.visual.desc': 'Watch algorithms work in real-time',
    'features.interactive.title': 'Interactive',
    'features.interactive.desc': 'Control speed and execution parameters',
    'features.analysis.title': 'Performance Analysis',
    'features.analysis.desc': 'Compare time complexity of algorithms',
    'controls.play': 'Play',
    'controls.pause': 'Pause',
    'controls.forward': 'Forward',
    'controls.backward': 'Backward',
    'controls.reset': 'Reset',
    'controls.speed': 'Speed',
    'structure.select': 'Data Structure',
    'structure.array': 'Array',
    'structure.tree': 'Tree',
    'structure.graph': 'Graph',
    'structure.list': 'Linked List',
    'structure.stack': 'Stack',
    'structure.queue': 'Queue',
    'structure.heap': 'Heap',
    'structure.hashtable': 'Hash Table',
    'algorithm.select': 'Select Algorithm',
    'algorithm.bubblesort': 'Bubble Sort',
    'algorithm.quicksort': 'Quick Sort',
    'algorithm.insertionsort': 'Insertion Sort',
    'algorithm.selectionsort': 'Selection Sort',
    'algorithm.bst.inorder': 'Inorder Traversal',
    'algorithm.bst.preorder': 'Preorder Traversal',
    'algorithm.bst.postorder': 'Postorder Traversal',
    'algorithm.bst.levelorder': 'Level-Order Traversal',
    'algorithm.bfs': 'Breadth-First Search (BFS)',
    'algorithm.dfs': 'Depth-First Search (DFS)',
    'data.generate': 'Generate Data',
    'data.size': 'Size',
    'data.value': 'Value',
    'data.insert': 'Insert',
    'data.search': 'Search',
    'data.delete': 'Delete',
    'list.type': 'List Type',
    'list.singly': 'Singly Linked',
    'list.doubly': 'Doubly Linked',
    'list.position': 'Position',
    'list.insertAtPosition': 'Insert at Position',
    'list.deleteAtPosition': 'Delete at Position',
    'heap.type': 'Heap Type',
    'heap.max': 'Max Heap',
    'heap.min': 'Min Heap',
    'hashtable.size': 'Table Size',
    'hashtable.key': 'Key',
    'profiler.title': 'Performance Analysis',
    'profiler.compare': 'Compare Algorithms',
    'profiler.run': 'Run Test',
    'profiler.time': 'Execution Time',
    'profiler.comparisons': 'Comparisons',
    'profiler.swaps': 'Swaps',
    'profiler.operations': 'Operations',
    'profiler.sendToVisualizer': 'Send to Visualizer',
    'analyzer.title': 'Code Analyzer',
    'analyzer.description': 'Analyze complexity and performance of your Python code',
    'analyzer.codeInput': 'Code Input',
    'analyzer.codeInputDesc': 'Paste your Python code for analysis',
    'analyzer.placeholder': 'Paste your Python code here...',
    'analyzer.lines': 'lines',
    'analyzer.analyze': 'Analyze',
    'analyzer.analyzing': 'Analyzing...',
    'analyzer.clear': 'Clear',
    'analyzer.loadExample': 'Load Example',
    'analyzer.results': 'Analysis Results',
    'analyzer.resultsDesc': 'Code performance analysis results',
    'analyzer.noResults': 'Analysis results will appear here',
    'analyzer.analyzingMessage': 'Analyzing your code...',
    'analyzer.emptyCode': 'Please enter code to analyze',
    'analyzer.success': 'Analysis completed successfully',
    'analyzer.error': 'Error analyzing code',
    'analyzer.complexity': 'Time Complexity:',
    'analyzer.warnings': 'Warnings:',
    'analyzer.suggestions': 'Suggestions:',
    'analyzer.note': 'Note: This is a demo version. Backend setup required for real analysis.',
    'analyzer.infoTitle': 'What does the tool analyze?',
    'analyzer.info1': 'Time and space complexity of algorithms',
    'analyzer.info2': 'Potential performance issues',
    'analyzer.info3': 'Code optimization recommendations',
  }
};

const AppContext = createContext<AppContextType | undefined>(undefined);

export function AppProvider({ children }: { children: ReactNode }) {
  const [language, setLanguage] = useState<Language>('ru');
  const [theme, setTheme] = useState<Theme>('light');
  const [sharedData, setSharedData] = useState<SharedVisualizationData | null>(null);

  useEffect(() => {
    const savedLanguage = localStorage.getItem('language') as Language;
    const savedTheme = localStorage.getItem('theme') as Theme;
    
    if (savedLanguage) setLanguage(savedLanguage);
    if (savedTheme) setTheme(savedTheme);
  }, []);

  useEffect(() => {
    localStorage.setItem('language', language);
  }, [language]);

  useEffect(() => {
    localStorage.setItem('theme', theme);
    document.documentElement.classList.toggle('dark', theme === 'dark');
  }, [theme]);

  return (
    <AppContext.Provider
      value={{
        language,
        setLanguage,
        theme,
        setTheme,
        translations: translations[language],
        sharedData,
        setSharedData,
      }}
    >
      {children}
    </AppContext.Provider>
  );
}

export function useApp() {
  const context = useContext(AppContext);
  if (context === undefined) {
    throw new Error('useApp must be used within an AppProvider');
  }
  return context;
}