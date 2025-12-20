# yava_quick.py

"""
Упрощенный интерфейс для быстрой трансляции.
"""

from yava_translator_service import YAVATranslatorService

# Создаем глобальный экземпляр для удобства
translator = YAVATranslatorService()


def quick_translate(python_code: str, **kwargs):
    """
    Быстрая трансляция Python кода в ЯВА.
    
    Args:
        python_code: Исходный код на Python
        **kwargs: Дополнительные параметры (program_name, description, structure_type)
    
    Returns:
        Результат трансляции в формате ЯВА
    """
    return translator.translate_from_python(python_code, **kwargs)


def quick_save(python_code: str, filename: str = "algorithm.yava.json", **kwargs):
    """
    Быстрая трансляция и сохранение в файл.
    
    Args:
        python_code: Исходный код на Python
        filename: Имя файла для сохранения
        **kwargs: Дополнительные параметры
    
    Returns:
        Путь к сохраненному файлу
    """
    result = quick_translate(python_code, **kwargs)
    return translator.save_to_file(result, filename)


# Предопределенные алгоритмы
ALGORITHMS = {
    "bubble_sort": """
def bubble_sort(arr):
    n = len(arr)
    for i in range(n):
        for j in range(0, n - i - 1):
            if arr[j] > arr[j + 1]:
                arr[j], arr[j + 1] = arr[j + 1], arr[j]
    return arr
""",
    
    "binary_search": """
def binary_search(arr, target):
    low = 0
    high = len(arr) - 1
    
    while low <= high:
        mid = (low + high) // 2
        if arr[mid] == target:
            return mid
        elif arr[mid] < target:
            low = mid + 1
        else:
            high = mid - 1
    
    return -1
""",
    
    "quicksort": """
def quicksort(arr, low, high):
    if low < high:
        pi = partition(arr, low, high)
        quicksort(arr, low, pi - 1)
        quicksort(arr, pi + 1, high)

def partition(arr, low, high):
    pivot = arr[high]
    i = low - 1
    
    for j in range(low, high):
        if arr[j] <= pivot:
            i += 1
            arr[i], arr[j] = arr[j], arr[i]
    
    arr[i + 1], arr[high] = arr[high], arr[i + 1]
    return i + 1
"""
}


def get_algorithm(name: str):
    """
    Получение предопределенного алгоритма.
    
    Args:
        name: Имя алгоритма (bubble_sort, binary_search, quicksort)
    
    Returns:
        Python код алгоритма
    """
    if name not in ALGORITHMS:
        raise ValueError(f"Алгоритм '{name}' не найден. Доступные: {list(ALGORITHMS.keys())}")
    
    return ALGORITHMS[name]


# Декоратор для трансляции функций
def yava_algorithm(func):
    """
    Декоратор для автоматической трансляции функции в формат ЯВА.
    
    Использование:
    @yava_algorithm
    def my_algorithm():
        arr = [5, 2, 8, 1, 9, 3]
        n = len(arr)
        for i in range(n):
            for j in range(0, n - i - 1):
                if arr[j] > arr[j + 1]:
                    temp = arr[j]
                    arr[j] = arr[j + 1]
                    arr[j + 1] = temp
        return arr
    
    result = my_algorithm.to_yava()  # Получить ЯВА представление
    """
    import inspect
    
    def wrapper(*args, **kwargs):
        return func(*args, **kwargs)
    
    def to_yava(**kwargs):
        source = inspect.getsource(func)
        # Убираем декоратор и определение функции
        lines = source.strip().split('\n')
        # Находим тело функции
        body_lines = []
        in_body = False
        indent = 0
        
        for line in lines:
            if line.strip().startswith('def '):
                # Находим отступ первой строки тела
                for i, char in enumerate(line):
                    if char != ' ':
                        indent = i
                        break
                in_body = True
            elif in_body and line.strip() and not line.strip().startswith('@'):
                if line.startswith(' ' * (indent + 4)):
                    body_lines.append(line[indent + 4:])
                elif line.strip() == 'pass':
                    continue
                else:
                    break
        
        python_code = '\n'.join(body_lines)
        return quick_translate(python_code, **kwargs)
    
    wrapper.to_yava = to_yava
    wrapper.__name__ = func.__name__
    wrapper.__doc__ = func.__doc__
    
    return wrapper