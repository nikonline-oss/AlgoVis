"""
Пользовательский алгоритм для демонстрации возможностей транслятора
"""

def find_maximum():
    """
    Нахождение максимального элемента в массиве.
    
    Алгоритм последовательно проходит по всем элементам массива,
    сохраняя текущий максимальный элемент.
    """
    if struct.len == 0:
        return None
    
    max_value = struct.values[0]
    max_index = 0
    
    for i in range(1, struct.len):
        if struct.values[i] > max_value:
            max_value = struct.values[i]
            max_index = i
    
    return max_value, max_index


def reverse_array():
    """
    Разворот массива.
    
    Алгоритм меняет местами элементы с начала и конца массива,
    постепенно двигаясь к середине.
    """
    left = 0
    right = struct.len - 1
    
    while left < right:
        # Обмен элементов
        temp = struct.values[left]
        struct.values[left] = struct.values[right]
        struct.values[right] = temp
        
        # Двигаем указатели к середине
        left += 1
        right -= 1


def count_occurrences(target_value):
    """
    Подсчет количества вхождений значения в массиве.
    
    Args:
        target_value: Значение для подсчета
    """
    count = 0
    
    for i in range(struct.len):
        if struct.values[i] == target_value:
            count += 1
    
    return count


# Составной алгоритм для демонстрации
def complex_algorithm():
    """
    Комплексный алгоритм, демонстрирующий различные операции.
    
    1. Находит максимальный элемент
    2. Подсчитывает его вхождения
    3. Разворачивает массив
    4. Повторяет поиск в развернутом массиве
    """
    # Шаг 1: Находим максимальный элемент
    max_val, max_idx = find_maximum()
    print(f"Максимальный элемент: {max_val} на позиции {max_idx}")
    
    # Шаг 2: Подсчитываем его вхождения
    count = count_occurrences(max_val)
    print(f"Количество вхождений максимального элемента: {count}")
    
    # Шаг 3: Разворачиваем массив
    reverse_array()
    print(f"Массив после разворота: {struct.values}")
    
    # Шаг 4: Снова находим максимальный элемент
    max_val_new, max_idx_new = find_maximum()
    print(f"Максимальный элемент после разворота: {max_val_new} на позиции {max_idx_new}")
    
    return {
        "original_max": max_val,
        "original_max_index": max_idx,
        "occurrences": count,
        "reversed_max": max_val_new,
        "reversed_max_index": max_idx_new
    }


# Дополнительная функция с условиями
def categorize_numbers():
    """
    Категоризация чисел в массиве.
    
    Разделяет числа на три категории:
    - Отрицательные
    - Нули
    - Положительные
    """
    negative_count = 0
    zero_count = 0
    positive_count = 0
    
    for i in range(struct.len):
        value = struct.values[i]
        
        if value < 0:
            negative_count += 1
        elif value == 0:
            zero_count += 1
        else:
            positive_count += 1
    
    return {
        "negative": negative_count,
        "zero": zero_count,
        "positive": positive_count
    }