/* eslint-disable react-refresh/only-export-components */
import React, { createContext, useContext, useState, useEffect, type ReactNode } from 'react';

type Language = 'ru' | 'en';
type Theme = 'light' | 'dark';

interface AppContextType {
  language: Language;
  setLanguage: (lang: Language) => void;
  theme: Theme;
  setTheme: (theme: Theme) => void;
  translations: Record<string, string>;
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
    'algorithm.select': 'Выберите алгоритм',
    'algorithm.bubblesort': 'Пузырьковая сортировка',
    'algorithm.quicksort': 'Быстрая сортировка',
    'algorithm.mergesort': 'Сортировка слиянием',
    'algorithm.heapsort': 'Пирамидальная сортировка',
    'data.generate': 'Генерировать данные',
    'data.size': 'Размер массива',
    'profiler.title': 'Анализ производительности',
    'profiler.compare': 'Сравнить алгоритмы',
    'profiler.time': 'Время выполнения',
    'profiler.comparisons': 'Сравнения',
    'profiler.swaps': 'Перестановки',
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
    'algorithm.select': 'Select Algorithm',
    'algorithm.bubblesort': 'Bubble Sort',
    'algorithm.quicksort': 'Quick Sort',
    'algorithm.mergesort': 'Merge Sort',
    'algorithm.heapsort': 'Heap Sort',
    'data.generate': 'Generate Data',
    'data.size': 'Array Size',
    'profiler.title': 'Performance Analysis',
    'profiler.compare': 'Compare Algorithms',
    'profiler.time': 'Execution Time',
    'profiler.comparisons': 'Comparisons',
    'profiler.swaps': 'Swaps',
  }
};

const AppContext = createContext<AppContextType | undefined>(undefined);

export function AppProvider({ children }: { children: ReactNode }) {
  const [language, setLanguage] = useState<Language>('ru');
  const [theme, setTheme] = useState<Theme>('light');

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