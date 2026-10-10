import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from './ui/card';
import { useApp } from '../contexts/AppContext';

interface TheoryPanelProps {
  dataStructure: string;
  algorithm: string;
}

export function TheoryPanel({ dataStructure, algorithm }: TheoryPanelProps) {
  const { translations } = useApp();

  const getTheoryContent = () => {
    // Теория для массивов
    if (dataStructure === 'array') {
      switch (algorithm) {
        case 'bubblesort':
          return {
            title: translations['algorithm.bubblesort'],
            complexity: {
              best: 'O(n)',
              average: 'O(n²)',
              worst: 'O(n²)',
              space: 'O(1)'
            },
            description: 'Простой алгоритм сортировки, который многократно проходит по массиву, сравнивая соседние элементы и меняя их местами при необходимости.',
            steps: [
              'Сравнивает первые два элемента массива',
              'Если первый больше второго, меняет их местами',
              'Переходит к следующей паре элементов',
              'Повторяет до тех пор, пока массив не будет отсортирован'
            ],
            useCases: [
              'Обучение основам алгоритмов',
              'Сортировка небольших массивов',
              'Когда массив почти отсортирован'
            ]
          };
        case 'quicksort':
          return {
            title: translations['algorithm.quicksort'],
            complexity: {
              best: 'O(n log n)',
              average: 'O(n log n)',
              worst: 'O(n²)',
              space: 'O(log n)'
            },
            description: 'Эффективный алгоритм "разделяй и властвуй", который выбирает опорный элемент и рекурсивно сортирует элементы относительно него.',
            steps: [
              'Выбирает опорный элемент (pivot)',
              'Перераспределяет элементы: меньшие слева, большие справа',
              'Рекурсивно сортирует левую и правую части',
              'Повторяет до полной сортировки массива'
            ],
            useCases: [
              'Сортировка больших массивов',
              'Когда нужна высокая производительность',
              'Встроенная сортировка во многих языках'
            ]
          };
        case 'insertionsort':
          return {
            title: translations['algorithm.insertionsort'],
            complexity: {
              best: 'O(n)',
              average: 'O(n²)',
              worst: 'O(n²)',
              space: 'O(1)'
            },
            description: 'Алгоритм строит отсортированную последовательность, постепенно вставляя элементы на правильные позиции.',
            steps: [
              'Начинает со второго элемента',
              'Сравнивает с элементами слева',
              'Вставляет на правильную позицию',
              'Повторяет для всех элементов'
            ],
            useCases: [
              'Маленькие массивы',
              'Почти отсортированные массивы',
              'Онлайн-сортировка (поступление данных в реальном времени)'
            ]
          };
        case 'selectionsort':
          return {
            title: translations['algorithm.selectionsort'],
            complexity: {
              best: 'O(n²)',
              average: 'O(n²)',
              worst: 'O(n²)',
              space: 'O(1)'
            },
            description: 'Находит минимальный элемент и помещает его в начало, затем повторяет для оставшейся части.',
            steps: [
              'Находит минимальный элемент в неотсортированной части',
              'Меняет его с первым неотсортированным элементом',
              'Увеличивает границу отсортированной части',
              'Повторяет до полной сортировки'
            ],
            useCases: [
              'Когда нужно минимизировать количество перестановок',
              'Обучение алгоритмам сортировки',
              'Небольшие массивы'
            ]
          };
      }
    }

    // Теория для деревьев
    else if (dataStructure === 'tree') {
      switch (algorithm) {
        case 'bst.inorder':
          return {
            title: translations['algorithm.bst.inorder'],
            complexity: {
              best: 'O(n)',
              average: 'O(n)',
              worst: 'O(n)',
              space: 'O(h)'
            },
            description: 'Обход дерева в порядке: левое поддерево → корень → правое поддерево.',
            steps: [
              'Рекурсивно обойти левое поддерево',
              'Посетить корневой узел',
              'Рекурсивно обойти правое поддерево'
            ],
            useCases: [
              'Получение элементов в возрастающем порядке (для BST)',
              'Копирование дерева',
              'Выражение деревьев (инфиксная нотация)'
            ]
          };
        case 'bst.preorder':
          return {
            title: translations['algorithm.bst.preorder'],
            complexity: {
              best: 'O(n)',
              average: 'O(n)',
              worst: 'O(n)',
              space: 'O(h)'
            },
            description: 'Обход дерева в порядке: корень → левое поддерево → правое поддерево.',
            steps: [
              'Посетить корневой узел',
              'Рекурсивно обойти левое поддерево',
              'Рекурсивно обойти правое поддерево'
            ],
            useCases: [
              'Создание копии дерева',
              'Префиксная нотация выражений',
              'Сериализация дерева'
            ]
          };
        case 'bst.postorder':
          return {
            title: translations['algorithm.bst.postorder'],
            complexity: {
              best: 'O(n)',
              average: 'O(n)',
              worst: 'O(n)',
              space: 'O(h)'
            },
            description: 'Обход дерева в порядке: левое поддерево → правое поддерево → корень.',
            steps: [
              'Рекурсивно обойти левое поддерево',
              'Рекурсивно обойти правое поддерево',
              'Посетить корневой узел'
            ],
            useCases: [
              'Удаление дерева',
              'Постфиксная нотация выражений',
              'Вычисление выражений'
            ]
          };
        case 'bst.levelorder':
          return {
            title: translations['algorithm.bst.levelorder'],
            complexity: {
              best: 'O(n)',
              average: 'O(n)',
              worst: 'O(n)',
              space: 'O(w)'
            },
            description: 'Обход дерева уровень за уровнем, слева направо.',
            steps: [
              'Начать с корневого узла',
              'Посетить все узлы текущего уровня',
              'Перейти на следующий уровень',
              'Повторить для всех уровней'
            ],
            useCases: [
              'Поиск кратчайшего пути',
              'Построчное отображение дерева',
              'Нахождение ширины дерева'
            ]
          };
      }
    }

    // Теория для графов
    else if (dataStructure === 'graph') {
      switch (algorithm) {
        case 'bfs':
          return {
            title: translations['algorithm.bfs'],
            complexity: {
              best: 'O(V + E)',
              average: 'O(V + E)',
              worst: 'O(V + E)',
              space: 'O(V)'
            },
            description: 'Алгоритм обхода графа, который исследует все соседние вершины перед переходом на следующий уровень.',
            steps: [
              'Начать с исходной вершины',
              'Посетить все соседние вершины',
              'Добавить непосещённых соседей в очередь',
              'Повторять, пока очередь не пуста'
            ],
            useCases: [
              'Поиск кратчайшего пути в невзвешенном графе',
              'Проверка связности графа',
              'Поиск компонент связности'
            ]
          };
        case 'dfs':
          return {
            title: translations['algorithm.dfs'],
            complexity: {
              best: 'O(V + E)',
              average: 'O(V + E)',
              worst: 'O(V + E)',
              space: 'O(h)'
            },
            description: 'Алгоритм обхода графа, который идёт как можно глубже по одной ветке перед возвратом.',
            steps: [
              'Начать с исходной вершины',
              'Идти как можно глубже по одному пути',
              'Возвращаться при отсутствии непосещённых соседей',
              'Повторять для всех вершин'
            ],
            useCases: [
              'Поиск цикла в графе',
              'Топологическая сортировка',
              'Поиск компонент сильной связности'
            ]
          };
        case 'dijkstra':
          return {
            title: translations['algorithm.dijkstra'],
            complexity: {
              best: 'O((V + E) log V)',
              average: 'O((V + E) log V)',
              worst: 'O((V + E) log V)',
              space: 'O(V)'
            },
            description: 'Алгоритм нахождения кратчайших путей от одной вершины до всех остальных во взвешенном графе.',
            steps: [
              'Установить расстояние до начальной вершины = 0',
              'Для остальных вершин установить бесконечность',
              'Посетить вершину с минимальным расстоянием',
              'Обновить расстояния до соседей',
              'Повторять, пока все вершины не посещены'
            ],
            useCases: [
              'Поиск кратчайшего пути во взвешенном графе',
              'Маршрутизация в сетях',
              'Навигационные системы'
            ]
          };
      }
    }

    return null;
  };

  const content = getTheoryContent();
  if (!content) return null;

  return (
    <Card className="bg-card/50">
      <CardHeader>
        <CardTitle className="text-lg">Теория: {content.title}</CardTitle>
      </CardHeader>
      <CardContent className="space-y-4">
        <div>
          <p className="text-sm text-muted-foreground mb-2">{content.description}</p>
        </div>

        <div>
          <h4 className="font-semibold text-sm mb-2">Сложность алгоритма:</h4>
          <div className="grid grid-cols-2 gap-2 text-sm">
            <div className="bg-muted p-2 rounded">
              <div className="text-xs text-muted-foreground">Лучший случай</div>
              <div className="font-mono">{content.complexity.best}</div>
            </div>
            <div className="bg-muted p-2 rounded">
              <div className="text-xs text-muted-foreground">Средний случай</div>
              <div className="font-mono">{content.complexity.average}</div>
            </div>
            <div className="bg-muted p-2 rounded">
              <div className="text-xs text-muted-foreground">Худший случай</div>
              <div className="font-mono">{content.complexity.worst}</div>
            </div>
            <div className="bg-muted p-2 rounded">
              <div className="text-xs text-muted-foreground">Память</div>
              <div className="font-mono">{content.complexity.space}</div>
            </div>
          </div>
        </div>

        <div>
          <h4 className="font-semibold text-sm mb-2">Шаги алгоритма:</h4>
          <ol className="text-sm space-y-1 list-decimal list-inside">
            {content.steps.map((step, index) => (
              <li key={index} className="pl-2">
                {step}
              </li>
            ))}
          </ol>
        </div>

        <div>
          <h4 className="font-semibold text-sm mb-2">Когда использовать:</h4>
          <ul className="text-sm space-y-1">
            {content.useCases.map((useCase, index) => (
              <li key={index} className="flex items-start">
                <span className="mr-2">•</span>
                {useCase}
              </li>
            ))}
          </ul>
        </div>
      </CardContent>
    </Card>
  );
}