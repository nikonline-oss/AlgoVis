import React from 'react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '../components/ui/card';
import { Accordion, AccordionContent, AccordionItem, AccordionTrigger } from '../components/ui/accordion';
import { Badge } from '../components/ui/badge';
import { Play, Pause, SkipForward, SkipBack, RotateCcw } from 'lucide-react';
import { useApp } from '../contexts/AppContext';

export function HelpPage() {
  const { translations, language } = useApp();

  const algorithms = [
    {
      name: translations['algorithm.bubblesort'],
      complexity: 'O(n²)',
      description: language === 'ru' 
        ? 'Простой алгоритм сортировки, который многократно проходит по списку, сравнивает соседние элементы и меняет их местами, если они находятся в неправильном порядке.'
        : 'A simple sorting algorithm that repeatedly steps through the list, compares adjacent elements and swaps them if they are in the wrong order.',
      bestCase: 'O(n)',
      worstCase: 'O(n²)',
      stable: true,
    },
    {
      name: translations['algorithm.quicksort'],
      complexity: 'O(n log n)',
      description: language === 'ru'
        ? 'Эффективный алгоритм сортировки, использующий принцип "разделяй и властвуй". Выбирает опорный элемент и разделяет массив на две части.'
        : 'An efficient sorting algorithm using divide-and-conquer approach. It picks a pivot element and partitions the array into two parts.',
      bestCase: 'O(n log n)',
      worstCase: 'O(n²)',
      stable: false,
    },
    {
      name: translations['algorithm.mergesort'],
      complexity: 'O(n log n)',
      description: language === 'ru'
        ? 'Стабильный алгоритм сортировки, основанный на принципе "разделяй и властвуй". Разделяет массив пополам, сортирует каждую половину и затем объединяет их.'
        : 'A stable sorting algorithm based on divide-and-conquer. It divides the array in half, sorts each half, and then merges them.',
      bestCase: 'O(n log n)',
      worstCase: 'O(n log n)',
      stable: true,
    },
  ];

  const controls = [
    {
      icon: Play,
      name: translations['controls.play'],
      description: language === 'ru' 
        ? 'Запускает автоматическое воспроизведение алгоритма'
        : 'Starts automatic algorithm playback',
    },
    {
      icon: Pause,
      name: translations['controls.pause'],
      description: language === 'ru'
        ? 'Приостанавливает воспроизведение'
        : 'Pauses the playback',
    },
    {
      icon: SkipForward,
      name: translations['controls.forward'],
      description: language === 'ru'
        ? 'Переходит к следующему шагу алгоритма'
        : 'Steps forward to the next algorithm step',
    },
    {
      icon: SkipBack,
      name: translations['controls.backward'],
      description: language === 'ru'
        ? 'Возвращается к предыдущему шагу'
        : 'Steps back to the previous step',
    },
    {
      icon: RotateCcw,
      name: translations['controls.reset'],
      description: language === 'ru'
        ? 'Сбрасывает визуализацию к начальному состоянию'
        : 'Resets visualization to initial state',
    },
  ];

  return (
    <div className="space-y-6">
      <Card>
        <CardHeader>
          <CardTitle>
            {language === 'ru' ? 'Добро пожаловать в Визуализатор Алгоритмов' : 'Welcome to Algorithm Visualizer'}
          </CardTitle>
          <CardDescription>
            {language === 'ru' 
              ? 'Этот инструмент поможет вам понять, как работают различные алгоритмы сортировки через интерактивную визуализацию.'
              : 'This tool helps you understand how different sorting algorithms work through interactive visualization.'
            }
          </CardDescription>
        </CardHeader>
      </Card>

      <Accordion type="single" collapsible className="space-y-4">
        <AccordionItem value="getting-started">
          <AccordionTrigger>
            {language === 'ru' ? 'Как начать работу' : 'Getting Started'}
          </AccordionTrigger>
          <AccordionContent className="space-y-4">
            <ol className="list-decimal list-inside space-y-2">
              <li>{language === 'ru' ? 'Перейдите на страницу "Визуализатор"' : 'Go to the "Visualizer" page'}</li>
              <li>{language === 'ru' ? 'Выберите алгоритм сортировки' : 'Select a sorting algorithm'}</li>
              <li>{language === 'ru' ? 'Настройте размер массива' : 'Adjust the array size'}</li>
              <li>{language === 'ru' ? 'Нажмите "Генерировать данные" для создания случайного массива' : 'Click "Generate Data" to create a random array'}</li>
              <li>{language === 'ru' ? 'Используйте элементы управления для запуска алгоритма' : 'Use the controls to run the algorithm'}</li>
            </ol>
          </AccordionContent>
        </AccordionItem>

        <AccordionItem value="controls">
          <AccordionTrigger>
            {language === 'ru' ? 'Элементы управления' : 'Controls'}
          </AccordionTrigger>
          <AccordionContent>
            <div className="grid gap-4">
              {controls.map((control, index) => (
                <div key={index} className="flex items-center space-x-3">
                  <div className="w-10 h-10 bg-primary/10 rounded-lg flex items-center justify-center">
                    <control.icon className="h-5 w-5 text-primary" />
                  </div>
                  <div>
                    <div className="font-medium">{control.name}</div>
                    <div className="text-sm text-muted-foreground">{control.description}</div>
                  </div>
                </div>
              ))}
            </div>
          </AccordionContent>
        </AccordionItem>

        <AccordionItem value="algorithms">
          <AccordionTrigger>
            {language === 'ru' ? 'Поддерживаемые алгоритмы' : 'Supported Algorithms'}
          </AccordionTrigger>
          <AccordionContent>
            <div className="space-y-6">
              {algorithms.map((algorithm, index) => (
                <Card key={index}>
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <CardTitle className="text-lg">{algorithm.name}</CardTitle>
                      <Badge variant="secondary">{algorithm.complexity}</Badge>
                    </div>
                  </CardHeader>
                  <CardContent className="space-y-3">
                    <p className="text-sm text-muted-foreground">
                      {algorithm.description}
                    </p>
                    <div className="grid grid-cols-3 gap-4 text-sm">
                      <div>
                        <div className="font-medium">
                          {language === 'ru' ? 'Лучший случай' : 'Best Case'}
                        </div>
                        <div className="text-muted-foreground">{algorithm.bestCase}</div>
                      </div>
                      <div>
                        <div className="font-medium">
                          {language === 'ru' ? 'Худший случай' : 'Worst Case'}
                        </div>
                        <div className="text-muted-foreground">{algorithm.worstCase}</div>
                      </div>
                      <div>
                        <div className="font-medium">
                          {language === 'ru' ? 'Стабильность' : 'Stable'}
                        </div>
                        <div className="text-muted-foreground">
                          {algorithm.stable ? 
                            (language === 'ru' ? 'Да' : 'Yes') : 
                            (language === 'ru' ? 'Нет' : 'No')
                          }
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              ))}
            </div>
          </AccordionContent>
        </AccordionItem>

        <AccordionItem value="visualization">
          <AccordionTrigger>
            {language === 'ru' ? 'Обозначения в визуализации' : 'Visualization Legend'}
          </AccordionTrigger>
          <AccordionContent>
            <div className="grid gap-3">
              <div className="flex items-center space-x-3">
                <div className="w-6 h-6 bg-muted rounded"></div>
                <span>{language === 'ru' ? 'Неотсортированные элементы' : 'Unsorted elements'}</span>
              </div>
              <div className="flex items-center space-x-3">
                <div className="w-6 h-6 bg-yellow-500 rounded"></div>
                <span>{language === 'ru' ? 'Сравниваемые элементы' : 'Elements being compared'}</span>
              </div>
              <div className="flex items-center space-x-3">
                <div className="w-6 h-6 bg-red-500 rounded"></div>
                <span>{language === 'ru' ? 'Элементы в процессе перестановки' : 'Elements being swapped'}</span>
              </div>
              <div className="flex items-center space-x-3">
                <div className="w-6 h-6 bg-primary rounded"></div>
                <span>{language === 'ru' ? 'Отсортированные элементы' : 'Sorted elements'}</span>
              </div>
              <div className="flex items-center space-x-3">
                <div className="w-6 h-6 bg-purple-500 rounded"></div>
                <span>{language === 'ru' ? 'Опорный элемент (для Quick Sort)' : 'Pivot element (for Quick Sort)'}</span>
              </div>
            </div>
          </AccordionContent>
        </AccordionItem>

        <AccordionItem value="profiler">
          <AccordionTrigger>
            {language === 'ru' ? 'Анализ производительности' : 'Performance Analysis'}
          </AccordionTrigger>
          <AccordionContent className="space-y-4">
            <p>
              {language === 'ru' 
                ? 'Страница "Профайлер" позволяет сравнить производительность различных алгоритмов сортировки:'
                : 'The "Profiler" page allows you to compare the performance of different sorting algorithms:'
              }
            </p>
            <ul className="list-disc list-inside space-y-1">
              <li>{language === 'ru' ? 'Время выполнения в миллисекундах' : 'Execution time in milliseconds'}</li>
              <li>{language === 'ru' ? 'Количество сравнений элементов' : 'Number of element comparisons'}</li>
              <li>{language === 'ru' ? 'Количество перестановок' : 'Number of swaps'}</li>
              <li>{language === 'ru' ? 'Теоретическая временная сложность' : 'Theoretical time complexity'}</li>
            </ul>
          </AccordionContent>
        </AccordionItem>
      </Accordion>
    </div>
  );
}