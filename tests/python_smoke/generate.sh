#!/bin/bash
# Генерация Python-файлов для smoke-теста транспайлера.
# Каждый файл — минимальный воспроизводимый алгоритм.
set -e
cd "$(dirname "$0")"

# ─── 01. Bubble sort (уже проверяли) ───────────────────────
cat > 01_bubble_sort.py <<'PY'
def bubble_sort(A):
    n = len(A)
    for i in range(n):
        for j in range(n - i - 1):
            if A[j] > A[j + 1]:
                A[j], A[j + 1] = A[j + 1], A[j]
    return A

def main():
    A = [5, 2, 8, 1, 9, 3, 7, 4]
    bubble_sort(A)
PY

# ─── 02. Selection sort ────────────────────────────────────
cat > 02_selection_sort.py <<'PY'
def selection_sort(A):
    n = len(A)
    for i in range(n):
        min_idx = i
        for j in range(i + 1, n):
            if A[j] < A[min_idx]:
                min_idx = j
        if min_idx != i:
            A[i], A[min_idx] = A[min_idx], A[i]
    return A

def main():
    A = [5, 2, 8, 1, 9, 3]
    selection_sort(A)
PY

# ─── 03. Insertion sort ────────────────────────────────────
cat > 03_insertion_sort.py <<'PY'
def insertion_sort(A):
    n = len(A)
    for i in range(1, n):
        key = A[i]
        j = i - 1
        while j >= 0 and A[j] > key:
            A[j + 1] = A[j]
            j = j - 1
        A[j + 1] = key
    return A

def main():
    A = [5, 2, 8, 1, 9]
    insertion_sort(A)
PY

# ─── 04. Binary search ─────────────────────────────────────
cat > 04_binary_search.py <<'PY'
def binary_search(A, target):
    lo = 0
    hi = len(A) - 1
    while lo <= hi:
        mid = (lo + hi) // 2
        if A[mid] == target:
            return mid
        elif A[mid] < target:
            lo = mid + 1
        else:
            hi = mid - 1
    return -1

def main():
    A = [1, 3, 5, 7, 9, 11, 13]
    binary_search(A, 9)
PY

# ─── 05. Linear search ─────────────────────────────────────
cat > 05_linear_search.py <<'PY'
def linear_search(A, target):
    for i in range(len(A)):
        if A[i] == target:
            return i
    return -1

def main():
    A = [4, 2, 7, 1, 9, 5]
    linear_search(A, 7)
PY

# ─── 06. Factorial (рекурсия) ──────────────────────────────
cat > 06_factorial.py <<'PY'
def factorial(n):
    if n <= 1:
        return 1
    return n * factorial(n - 1)

def main():
    factorial(5)
PY

# ─── 07. Fibonacci (рекурсия, неэффективная) ───────────────
cat > 07_fibonacci.py <<'PY'
def fib(n):
    if n < 2:
        return n
    return fib(n - 1) + fib(n - 2)

def main():
    fib(6)
PY

# ─── 08. GCD (Euclid) ──────────────────────────────────────
cat > 08_gcd.py <<'PY'
def gcd(a, b):
    while b != 0:
        t = b
        b = a % b
        a = t
    return a

def main():
    gcd(48, 18)
PY

# ─── 09. Quicksort (срезы) ─────────────────────────────────
cat > 09_quicksort.py <<'PY'
def quicksort(A):
    if len(A) <= 1:
        return A
    pivot = A[0]
    left = []
    right = []
    for i in range(1, len(A)):
        if A[i] < pivot:
            left.append(A[i])
        else:
            right.append(A[i])
    return quicksort(left) + [pivot] + quicksort(right)

def main():
    A = [5, 2, 8, 1, 9, 3]
    quicksort(A)
PY

# ─── 10. Merge sort (срезы + кортежи) ──────────────────────
cat > 10_mergesort.py <<'PY'
def merge(left, right):
    result = []
    i = 0
    j = 0
    while i < len(left) and j < len(right):
        if left[i] <= right[j]:
            result.append(left[i])
            i = i + 1
        else:
            result.append(right[j])
            j = j + 1
    while i < len(left):
        result.append(left[i])
        i = i + 1
    while j < len(right):
        result.append(right[j])
        j = j + 1
    return result

def mergesort(A):
    if len(A) <= 1:
        return A
    mid = len(A) // 2
    left = A[0:mid]
    right = A[mid:len(A)]
    return merge(mergesort(left), mergesort(right))

def main():
    A = [5, 2, 8, 1, 9, 3, 7, 4]
    mergesort(A)
PY

# ─── 11. Tuple assignment ──────────────────────────────────
cat > 11_tuple_assign.py <<'PY'
def swap_pair(a, b):
    a, b = b, a
    return a

def min_max(A):
    lo, hi = A[0], A[0]
    for i in range(len(A)):
        if A[i] < lo:
            lo = A[i]
        if A[i] > hi:
            hi = A[i]
    return lo

def main():
    min_max([5, 2, 8, 1, 9])
PY

# ─── 12. Chained comparison ────────────────────────────────
cat > 12_chained_cmp.py <<'PY'
def in_range(x, lo, hi):
    if lo <= x <= hi:
        return True
    return False

def main():
    in_range(5, 1, 10)
PY

# ─── 13. f-strings ─────────────────────────────────────────
cat > 13_fstrings.py <<'PY'
def describe(n):
    annotate(f"число: {n}")

def main():
    describe(42)
PY

# ─── 14. Nested functions ──────────────────────────────────
cat > 14_nested_func.py <<'PY'
def outer(n):
    def inner(x):
        return x * 2
    return inner(n)

def main():
    outer(5)
PY

# ─── 15. Simple class ──────────────────────────────────────
cat > 15_class.py <<'PY'
class Counter:
    def __init__(self):
        self.value = 0

    def inc(self):
        self.value = self.value + 1

def main():
    c = Counter()
    c.inc()
    c.inc()
PY

# ─── 16. BFS (очередь) ─────────────────────────────────────
cat > 16_bfs.py <<'PY'
def bfs(graph, start):
    visited = []
    queue = [start]
    while len(queue) > 0:
        node = queue[0]
        queue = queue[1:len(queue)]
        if node not in visited:
            visited.append(node)
            for neighbor in graph[node]:
                queue.append(neighbor)
    return visited

def main():
    g = {"A": ["B", "C"], "B": ["D"], "C": [], "D": []}
    bfs(g, "A")
PY

# ─── 17. DFS (рекурсия + visited) ──────────────────────────
cat > 17_dfs.py <<'PY'
def dfs(graph, node, visited):
    if node in visited:
        return
    visited.append(node)
    for neighbor in graph[node]:
        dfs(graph, neighbor, visited)

def main():
    g = {"A": ["B", "C"], "B": ["D"], "C": [], "D": []}
    visited = []
    dfs(g, "A", visited)
PY

# ─── 18. Recursive BST insert (функциями, без классов) ────
cat > 18_bst_func.py <<'PY'
def insert(node, value):
    if node is None:
        return {"value": value, "left": None, "right": None}
    if value < node["value"]:
        node["left"] = insert(node["left"], value)
    else:
        node["right"] = insert(node["right"], value)
    return node

def main():
    root = None
    root = insert(root, 50)
    root = insert(root, 30)
    root = insert(root, 70)
PY

# ─── 19. Dict (hashmap) ────────────────────────────────────
cat > 19_dict.py <<'PY'
def count_words(text):
    counts = {}
    for word in text:
        if word in counts:
            counts[word] = counts[word] + 1
        else:
            counts[word] = 1
    return counts

def main():
    count_words(["a", "b", "a", "c", "a"])
PY

# ─── 20. Stack (list as stack) ─────────────────────────────
cat > 20_stack.py <<'PY'
def match_brackets(s):
    stack = []
    for ch in s:
        if ch == "(":
            stack.append(ch)
        elif ch == ")":
            if len(stack) == 0:
                return False
            stack.pop()
    return len(stack) == 0

def main():
    match_brackets("(())")
PY

echo "✅ Создано $(ls *.py | wc -l) файлов"
ls -1 *.py