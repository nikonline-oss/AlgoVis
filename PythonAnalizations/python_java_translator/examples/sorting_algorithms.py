"""
Примеры алгоритмов сортировки для тестирования транслятора
"""

def bubble_sort_example():
    """
    Пример сортировки пузырьком.
    
    Алгоритм последовательно сравнивает соседние элементы массива
    и меняет их местами, если они находятся в неправильном порядке.
    Процесс повторяется до тех пор, пока массив не будет отсортирован.
    """
    n = struct.len
    
    for i in range(n):
        swapped = False
        
        for j in range(0, n - i - 1):
            # Сравнение соседних элементов
            if struct.values[j] > struct.values[j + 1]:
                # Обмен элементов
                temp = struct.values[j]
                struct.values[j] = struct.values[j + 1]
                struct.values[j + 1] = temp
                swapped = True
        
        # Если обменов не было, массив уже отсортирован
        if not swapped:
            break


def selection_sort_example():
    """
    Пример сортировки выбором.
    
    Алгоритм на каждом шаге находит минимальный элемент
    из неотсортированной части массива и помещает его
    в конец отсортированной части.
    """
    n = struct.len
    
    for i in range(n):
        # Находим индекс минимального элемента
        min_idx = i
        
        for j in range(i + 1, n):
            if struct.values[j] < struct.values[min_idx]:
                min_idx = j
        
        # Меняем местами найденный минимальный элемент
        # с первым элементом неотсортированной части
        if min_idx != i:
            temp = struct.values[i]
            struct.values[i] = struct.values[min_idx]
            struct.values[min_idx] = temp


def insertion_sort_example():
    """
    Пример сортировки вставками.
    
    Алгоритм построеночно строит отсортированную часть массива,
    вставляя каждый новый элемент в правильную позицию
    относительно уже отсортированных элементов.
    """
    n = struct.len
    
    for i in range(1, n):
        key = struct.values[i]
        j = i - 1
        
        # Перемещаем элементы arr[0..i-1], которые больше key,
        # на одну позицию вперед
        while j >= 0 and struct.values[j] > key:
            struct.values[j + 1] = struct.values[j]
            j -= 1
        
        struct.values[j + 1] = key


# Пример использования
if __name__ == "__main__":
    # Этот код не будет выполняться при трансляции,
    # но показывает, как выглядит реальный алгоритм
    test_array = [64, 34, 25, 12, 22, 11, 90]
    struct.values = test_array
    
    print("Исходный массив:", struct.values)
    bubble_sort_example()
    print("После сортировки пузырьком:", struct.values)