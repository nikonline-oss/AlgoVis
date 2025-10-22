# Руководство по добавлению новых алгоритмов

Это руководство поможет вам добавить новые алгоритмы в визуализатор.

## 📋 Общая структура

Для добавления нового алгоритма необходимо:

1. Добавить переводы в контекст
2. Реализовать логику алгоритма
3. Добавить опцию в селектор алгоритмов
4. (Опционально) Добавить в профайлер
5. (Опционально) Обновить справку

## 🔧 Пошаговая инструкция

### 1. Добавление алгоритма сортировки массива

#### Шаг 1: Добавьте переводы
**Файл:** `/contexts/AppContext.tsx`

```typescript
const translations = {
  ru: {
    // ...
    'algorithm.insertionsort': 'Сортировка вставками',
  },
  en: {
    // ...
    'algorithm.insertionsort': 'Insertion Sort',
  }
}
```

#### Шаг 2: Реализуйте алгоритм
**Файл:** `/pages/VisualizerPage.tsx`

```typescript
const insertionSort = (arr: number[]): SortingStep[] => {
  const steps: SortingStep[] = [];
  const array = [...arr];
  let comparisons = 0;
  let swaps = 0;
  
  steps.push({ array: [...array] });
  
  for (let i = 1; i < array.length; i++) {
    let key = array[i];
    let j = i - 1;
    
    while (j >= 0 && array[j] > key) {
      comparisons++;
      steps.push({
        array: [...array],
        comparing: [j, j + 1],
      });
      
      array[j + 1] = array[j];
      swaps++;
      steps.push({
        array: [...array],
        swapping: [j, j + 1],
      });
      j--;
    }
    
    array[j + 1] = key;
  }
  
  steps.push({
    array: [...array],
    sorted: Array.from({ length: array.length }, (_, i) => i),
  });
  
  setStats({ comparisons, swaps, operations: comparisons + swaps });
  return steps;
};
```

#### Шаг 3: Добавьте в селектор
**Файл:** `/pages/VisualizerPage.tsx`

В функции `getAlgorithmOptions()`:
```typescript
if (dataStructure === 'array') {
  return (
    <>
      <SelectItem value="bubblesort">{translations['algorithm.bubblesort']}</SelectItem>
      <SelectItem value="quicksort">{translations['algorithm.quicksort']}</SelectItem>
      <SelectItem value="mergesort">{translations['algorithm.mergesort']}</SelectItem>
      <SelectItem value="insertionsort">{translations['algorithm.insertionsort']}</SelectItem>
    </>
  );
}
```

#### Шаг 4: Добавьте в обработчик
**Файл:** `/pages/VisualizerPage.tsx`

В функции `runAlgorithm()`:
```typescript
if (dataStructure === 'array') {
  switch (algorithm) {
    case 'bubblesort':
      algorithmSteps = bubbleSort(originalArray);
      break;
    case 'quicksort':
      algorithmSteps = quickSort(originalArray);
      break;
    case 'insertionsort':
      algorithmSteps = insertionSort(originalArray);
      break;
    // ...
  }
}
```

#### Шаг 5: Добавьте в профайлер
**Файл:** `/pages/ProfilerPage.tsx`

```typescript
const insertionSort = (arr: number[]) => {
  const array = [...arr];
  let comparisons = 0;
  let swaps = 0;
  const start = performance.now();
  
  // Реализация алгоритма без визуализации
  
  const time = performance.now() - start;
  return { time, comparisons, swaps, operations: comparisons + swaps };
};
```

И добавьте в массив алгоритмов:
```typescript
const algorithms = [
  { name: translations['algorithm.bubblesort'], fn: () => bubbleSort(testArray) },
  { name: translations['algorithm.insertionsort'], fn: () => insertionSort(testArray) },
  // ...
];
```

#### Шаг 6: Обновите StatsPanel
**Файл:** `/components/StatsPanel.tsx`

```typescript
const complexities: Record<string, { best: string; avg: string; worst: string }> = {
  'insertionsort': { best: 'O(n)', avg: 'O(n²)', worst: 'O(n²)' },
  // ...
};

const names: Record<string, string> = {
  'insertionsort': translations['algorithm.insertionsort'],
  // ...
};
```

#### Шаг 7: Обновите справку
**Файл:** `/pages/HelpPage.tsx`

```typescript
const algorithms = [
  {
    name: translations['algorithm.insertionsort'],
    type: 'sorting',
    complexity: 'O(n²)',
    description: language === 'ru'
      ? 'Описание на русском'
      : 'Description in English',
    bestCase: 'O(n)',
    worstCase: 'O(n²)',
    stable: true,
  },
  // ...
];
```

### 2. Добавление алгоритма для деревьев

#### Структура TreeStep
```typescript
interface TreeStep {
  tree: TreeNode | null;
  currentNode?: number;
  highlightedNodes?: number[];
  visitedNodes?: number[];
}
```

#### Пример: Прямой обход (Preorder)

```typescript
const bstPreorder = (root: TreeNode | null): TreeStep[] => {
  const steps: TreeStep[] = [];
  const visited: number[] = [];
  let operations = 0;
  
  const copyTree = (node: TreeNode | null): TreeNode | null => {
    if (!node) return null;
    return {
      value: node.value,
      left: copyTree(node.left || null),
      right: copyTree(node.right || null),
    };
  };
  
  steps.push({ tree: copyTree(root) });
  
  const traverse = (node: TreeNode | null) => {
    if (!node) return;
    
    operations++;
    visited.push(node.value);
    
    steps.push({ 
      tree: copyTree(root), 
      highlightedNodes: [node.value],
      visitedNodes: [...visited],
    });
    
    traverse(node.left || null);
    traverse(node.right || null);
  };
  
  traverse(root);
  steps.push({ tree: copyTree(root), visitedNodes: [...visited] });
  
  setStats({ comparisons: 0, swaps: 0, operations });
  return steps;
};
```

### 3. Добавление алгоритма для графов

#### Структура GraphStep
```typescript
interface GraphStep {
  nodes: GraphNode[];
  edges: GraphEdge[];
  currentNode?: number;
  highlightedNodes?: number[];
  visitedNodes?: number[];
  highlightedEdges?: Array<[number, number]>;
}
```

#### Пример: Алгоритм Дейкстры

```typescript
const graphDijkstra = (
  nodes: GraphNode[], 
  edges: GraphEdge[], 
  startNode: number = 0
): GraphStep[] => {
  const steps: GraphStep[] = [];
  const distances = new Map<number, number>();
  const visited = new Set<number>();
  let operations = 0;
  
  // Инициализация
  nodes.forEach(node => {
    distances.set(node.id, node.id === startNode ? 0 : Infinity);
  });
  
  steps.push({ nodes, edges });
  
  while (visited.size < nodes.length) {
    // Найти узел с минимальным расстоянием
    let minDistance = Infinity;
    let minNode = -1;
    
    nodes.forEach(node => {
      if (!visited.has(node.id)) {
        const dist = distances.get(node.id) || Infinity;
        if (dist < minDistance) {
          minDistance = dist;
          minNode = node.id;
        }
      }
    });
    
    if (minNode === -1) break;
    
    operations++;
    visited.add(minNode);
    
    steps.push({
      nodes,
      edges,
      currentNode: minNode,
      visitedNodes: Array.from(visited),
    });
    
    // Обновить расстояния до соседей
    const currentDistance = distances.get(minNode) || 0;
    edges.forEach(edge => {
      let neighbor = -1;
      let weight = edge.weight || 1;
      
      if (edge.from === minNode) neighbor = edge.to;
      else if (edge.to === minNode) neighbor = edge.from;
      
      if (neighbor !== -1 && !visited.has(neighbor)) {
        const newDistance = currentDistance + weight;
        const oldDistance = distances.get(neighbor) || Infinity;
        
        if (newDistance < oldDistance) {
          distances.set(neighbor, newDistance);
          steps.push({
            nodes,
            edges,
            currentNode: minNode,
            highlightedNodes: [neighbor],
            visitedNodes: Array.from(visited),
            highlightedEdges: [[minNode, neighbor]],
          });
        }
      }
    });
  }
  
  steps.push({ nodes, edges, visitedNodes: Array.from(visited) });
  setStats({ comparisons: 0, swaps: 0, operations });
  return steps;
};
```

## 📝 Важные замечания

### Для всех алгоритмов:
1. **Всегда создавайте копии данных** - используйте `[...array]` или `copyTree()`
2. **Добавляйте шаги постепенно** - каждое важное изменение = новый шаг
3. **Обновляйте статистику** - вызывайте `setStats()` в конце
4. **Сохраняйте историю** - каждый шаг должен содержать полное состояние

### Оптимизация производительности:
- Ограничивайте количество шагов для больших структур
- Используйте shallow copy где возможно
- Избегайте глубокого копирования на каждом шаге для графов

### Визуализация:
- Используйте `comparing` для сравниваемых элементов
- Используйте `swapping` для перемещаемых элементов
- Используйте `sorted` для отсортированных элементов
- Используйте `highlightedNodes` для выделения узлов
- Используйте `currentNode` для текущего обрабатываемого узла

## 🧪 Тестирование

После добавления алгоритма проверьте:

1. ✅ Алгоритм корректно работает на малых данных (5-10 элементов)
2. ✅ Алгоритм корректно работает на больших данных (до максимума)
3. ✅ Визуализация плавная и понятная
4. ✅ Статистика отображается корректно
5. ✅ Элементы управления работают (пауза, шаги, сброс)
6. ✅ Переводы доступны на обоих языках
7. ✅ Профайлер корректно измеряет производительность

## 📚 Дополнительные ресурсы

- [Visualgo](https://visualgo.net/) - примеры визуализаций
- [Algorithm Visualizer](https://algorithm-visualizer.org/) - больше идей
- [Big-O Cheat Sheet](https://www.bigocheatsheet.com/) - временная сложность

## 🤝 Помощь

Если у вас возникли вопросы или проблемы при добавлении нового алгоритма:
1. Проверьте существующие реализации как примеры
2. Убедитесь, что все типы данных корректны
3. Используйте console.log для отладки шагов
4. Проверьте, что все переводы добавлены

---

Спасибо за вклад в развитие проекта! 🎉
