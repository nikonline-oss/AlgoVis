import React, { createContext, useContext, useState, useEffect, type ReactNode } from 'react';
type Language = 'ru' | 'en';
type Theme = 'light' | 'dark';
type DataStructureType = 'array' | 'tree' | 'graph';

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
    'algorithm.select': 'Выберите алгоритм',
    'algorithm.bubblesort': 'Пузырьковая сортировка',
    'algorithm.quicksort': 'Быстрая сортировка',
    'algorithm.mergesort': 'Сортировка слиянием',
    'algorithm.heapsort': 'Пирамидальная сортировка',
    'algorithm.bst.insert': 'Вставка в BST',
    'algorithm.bst.search': 'Поиск в BST',
    'algorithm.bst.delete': 'Удаление из BST',
    'algorithm.bst.inorder': 'Инордерный обход',
    'algorithm.bst.preorder': 'Прямой обход',
    'algorithm.bst.postorder': 'Обратный обход',
    'algorithm.bst.levelorder': 'Обход в ширину',
    'algorithm.bfs': 'Поиск в ширину (BFS)',
    'algorithm.dfs': 'Поиск в глубину (DFS)',
    'algorithm.dijkstra': 'Алгоритм Дейкстры',
    'algorithm.prim': 'Алгоритм Прима',
    'data.generate': 'Генерировать данные',
    'data.size': 'Размер',
    'data.value': 'Значение',
    'data.insert': 'Вставить',
    'data.search': 'Найти',
    'data.delete': 'Удалить',
    'profiler.title': 'Анализ производительности',
    'profiler.compare': 'Сравнить алгоритмы',
    'profiler.run': 'Запустить тест',
    'profiler.time': 'Время выполнения',
    'profiler.comparisons': 'Сравнения',
    'profiler.swaps': 'Перестановки',
    'profiler.operations': 'Операции',
    'profiler.sendToVisualizer': 'Отправить в визуализатор',
  },
  en: {
    'site.title': 'Algorithm Visualizer',
    'nav.home': 'Home',
    'nav.visualizer': 'Visualizer',
    'nav.profiler': 'Profiler',
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
    'algorithm.select': 'Select Algorithm',
    'algorithm.bubblesort': 'Bubble Sort',
    'algorithm.quicksort': 'Quick Sort',
    'algorithm.mergesort': 'Merge Sort',
    'algorithm.heapsort': 'Heap Sort',
    'algorithm.bst.insert': 'BST Insert',
    'algorithm.bst.search': 'BST Search',
    'algorithm.bst.delete': 'BST Delete',
    'algorithm.bst.inorder': 'Inorder Traversal',
    'algorithm.bst.preorder': 'Preorder Traversal',
    'algorithm.bst.postorder': 'Postorder Traversal',
    'algorithm.bst.levelorder': 'Level-Order Traversal',
    'algorithm.bfs': 'Breadth-First Search (BFS)',
    'algorithm.dfs': 'Depth-First Search (DFS)',
    'algorithm.dijkstra': 'Dijkstra\'s Algorithm',
    'algorithm.prim': 'Prim\'s Algorithm',
    'data.generate': 'Generate Data',
    'data.size': 'Size',
    'data.value': 'Value',
    'data.insert': 'Insert',
    'data.search': 'Search',
    'data.delete': 'Delete',
    'profiler.title': 'Performance Analysis',
    'profiler.compare': 'Compare Algorithms',
    'profiler.run': 'Run Test',
    'profiler.time': 'Execution Time',
    'profiler.comparisons': 'Comparisons',
    'profiler.swaps': 'Swaps',
    'profiler.operations': 'Operations',
    'profiler.sendToVisualizer': 'Send to Visualizer',
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