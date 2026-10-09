#!/bin/bash
set -e
cd "$(dirname "$0")"

# ── 01: Стек на классах — проверка скобок ──
cat > 01_bracket_checker.py <<'PY'
class Stack:
    def __init__(self):
        self.items = []

    def push(self, x):
        self.items.append(x)

    def pop(self):
        if len(self.items) == 0:
            return None
        top = self.items[-1]
        self.items = self.items[:-1]
        return top

    def is_empty(self):
        return len(self.items) == 0


def check_brackets(s):
    pairs = {")": "(", "]": "[", "}": "{"}
    stack = Stack()

    for ch in s:
        if ch in "([{":
            stack.push(ch)
        elif ch in ")]}":
            if stack.is_empty():
                return False
            opening = stack.pop()
            if opening != pairs[ch]:
                return False

    return stack.is_empty()


def main():
    test1 = "((()))"
    test2 = "([{}])"
    test3 = "((())"
    test4 = "([)]"

    r1 = check_brackets(test1)
    r2 = check_brackets(test2)
    r3 = check_brackets(test3)
    r4 = check_brackets(test4)

    annotate(f"test1 = {r1}, test4 = {r4}")
PY

# ── 02: Очередь + BFS на сетке ──
cat > 02_grid_bfs.py <<'PY'
def shortest_path(grid, start, end):
    rows = len(grid)
    cols = len(grid[0])

    queue = [start]
    visited = set()
    visited.add(start)
    dist = 0

    while len(queue) > 0:
        size = len(queue)
        for _ in range(size):
            node = queue[0]
            queue = queue[1:]

            if node[0] == end[0] and node[1] == end[1]:
                return dist

            for dr, dc in [(0, 1), (0, -1), (1, 0), (-1, 0)]:
                nr = node[0] + dr
                nc = node[1] + dc

                if 0 <= nr < rows and 0 <= nc < cols:
                    if grid[nr][nc] == 0 and (nr, nc) not in visited:
                        visited.add((nr, nc))
                        queue.append((nr, nc))

        dist = dist + 1

    return -1


def main():
    grid = [
        [0, 0, 0, 0, 0],
        [0, 1, 1, 1, 0],
        [0, 0, 0, 1, 0],
        [1, 1, 0, 0, 0],
        [0, 0, 0, 0, 0]
    ]
    d = shortest_path(grid, (0, 0), (4, 4))
    annotate(f"кратчайший путь: {d} шагов")
PY

# ── 03: Матрица — транспонирование, след, сумма ──
cat > 03_matrix_ops.py <<'PY'
def transpose(M):
    if len(M) == 0:
        return []
    rows = len(M)
    cols = len(M[0])

    result = []
    for j in range(cols):
        new_row = []
        for i in range(rows):
            new_row.append(M[i][j])
        result.append(new_row)

    return result


def trace(M):
    n = min(len(M), len(M[0]))
    total = 0
    for i in range(n):
        total = total + M[i][i]
    return total


def row_sums(M):
    result = []
    for row in M:
        total = 0
        for x in row:
            total = total + x
        result.append(total)
    return result


def main():
    M = [
        [1, 2, 3],
        [4, 5, 6],
        [7, 8, 9]
    ]
    T = transpose(M)
    t = trace(M)
    rs = row_sums(M)
    annotate(f"trace = {t}")
PY

# ── 04: Динамика — мемоизация через словарь ──
cat > 04_memoization.py <<'PY'
def make_fib():
    cache = {}

    def fib(n):
        if n in cache:
            return cache[n]
        if n < 2:
            cache[n] = n
            return n
        result = fib(n - 1) + fib(n - 2)
        cache[n] = result
        return result

    return fib


def count_coins(coins, amount):
    dp = [amount + 1] * (amount + 1)
    dp[0] = 0

    for i in range(1, amount + 1):
        for c in coins:
            if c <= i:
                if dp[i - c] + 1 < dp[i]:
                    dp[i] = dp[i - c] + 1

    if dp[amount] == amount + 1:
        return -1
    return dp[amount]


def main():
    fib = make_fib()
    f20 = fib(20)
    f30 = fib(30)
    annotate(f"fib(30) = {f30}")

    coins = [1, 2, 5]
    min_coins = count_coins(coins, 11)
    annotate(f"минимум монет для 11: {min_coins}")
PY

# ── 05: Строки — палиндром, анаграмма, счёт слов ──
cat > 05_string_tasks.py <<'PY'
def is_palindrome(s):
    cleaned = ""
    for c in s:
        if c.isalpha():
            cleaned = cleaned + c.lower()

    i = 0
    j = len(cleaned) - 1
    while i < j:
        if cleaned[i] != cleaned[j]:
            return False
        i = i + 1
        j = j - 1
    return True


def are_anagrams(a, b):
    if len(a) != len(b):
        return False
    count_a = {}
    for c in a:
        if c in count_a:
            count_a[c] = count_a[c] + 1
        else:
            count_a[c] = 1

    for c in b:
        if c not in count_a:
            return False
        count_a[c] = count_a[c] - 1
        if count_a[c] < 0:
            return False

    return True


def word_count(text):
    words = text.lower().split()
    counts = {}
    for w in words:
        if w in counts:
            counts[w] = counts[w] + 1
        else:
            counts[w] = 1
    return counts


def main():
    p1 = is_palindrome("A man a plan a canal Panama")
    p2 = is_palindrome("hello")
    a = are_anagrams("listen", "silent")
    wc = word_count("the cat and the dog and the bird")
    annotate(f"palindrome: {p1}, {p2}")
    annotate(f"anagrams: {a}")
PY

# ── 06: Рекурсия — Ханойские башни, перестановки ──
cat > 06_recursion.py <<'PY'
def hanoi(n, from_p, to_p, aux_p, moves):
    if n == 0:
        return

    hanoi(n - 1, from_p, aux_p, to_p, moves)
    moves.append((n, from_p, to_p))
    hanoi(n - 1, aux_p, to_p, from_p, moves)


def permutations(A):
    if len(A) <= 1:
        return [A]

    result = []
    for i in range(len(A)):
        first = A[i]
        rest = A[:i] + A[i + 1:]
        for perm in permutations(rest):
            result.append([first] + perm)
    return result


def main():
    moves = []
    hanoi(3, "A", "C", "B", moves)
    annotate(f"перемещений: {len(moves)}")

    perms = permutations([1, 2, 3])
    annotate(f"перестановок: {len(perms)}")
PY

# ── 07: Граф — Дейкстра ──
cat > 07_dijkstra.py <<'PY'
def dijkstra(graph, start):
    distances = {}
    for node in graph:
        distances[node] = 10 ** 9
    distances[start] = 0

    visited = set()

    while len(visited) < len(graph):
        current = None
        best = 10 ** 9

        for node in graph:
            if node not in visited:
                if distances[node] < best:
                    best = distances[node]
                    current = node

        if current is None:
            break

        visited.add(current)

        for neighbor, weight in graph[current]:
            new_dist = distances[current] + weight
            if new_dist < distances[neighbor]:
                distances[neighbor] = new_dist

    return distances


def main():
    graph = {
        "A": [("B", 4), ("C", 2)],
        "B": [("C", 5), ("D", 10)],
        "C": [("E", 3)],
        "D": [("F", 11)],
        "E": [("D", 4)],
        "F": []
    }
    dist = dijkstra(graph, "A")
    annotate(f"расстояние до F: {dist['F']}")
PY

# ── 08: Сортировка с comparator ──
cat > 08_sort_with_key.py <<'PY'
def sort_by_second(pairs):
    return sorted(pairs, key=second_element)


def second_element(p):
    return p[1]


def group_by_length(words):
    groups = {}
    for w in words:
        ln = len(w)
        if ln in groups:
            groups[ln].append(w)
        else:
            groups[ln] = [w]
    return groups


def main():
    pairs = [(1, 5), (2, 3), (3, 8), (4, 1)]
    sorted_p = sort_by_second(pairs)
    annotate(f"отсортировано по второму: {sorted_p}")

    words = ["apple", "cat", "dog", "banana", "hi"]
    groups = group_by_length(words)
    annotate(f"группы: {len(groups)}")
PY

# ── 09: Две суммы + sliding window ──
cat > 09_two_sum_window.py <<'PY'
def two_sum(A, target):
    seen = {}
    for i in range(len(A)):
        complement = target - A[i]
        if complement in seen:
            return (seen[complement], i)
        seen[A[i]] = i
    return (-1, -1)


def max_sum_window(A, k):
    if len(A) < k:
        return 0

    window_sum = 0
    for i in range(k):
        window_sum = window_sum + A[i]

    best = window_sum
    for i in range(k, len(A)):
        window_sum = window_sum + A[i] - A[i - k]
        if window_sum > best:
            best = window_sum

    return best


def main():
    A = [2, 7, 11, 15, 3, 6]
    i, j = two_sum(A, 9)
    annotate(f"two sum 9: индексы {i}, {j}")

    B = [1, 4, 2, 10, 23, 3, 1, 0, 20]
    max_w = max_sum_window(B, 4)
    annotate(f"макс окно из 4: {max_w}")
PY

# ── 10: Комбинированная задача — LRU-подобный кэш ──
cat > 10_lru_cache.py <<'PY'
class LRUCache:
    def __init__(self, capacity):
        self.capacity = capacity
        self.cache = {}
        self.order = []

    def get(self, key):
        if key not in self.cache:
            return -1

        self.order.remove(key)
        self.order.append(key)
        return self.cache[key]

    def put(self, key, value):
        if key in self.cache:
            self.order.remove(key)
        elif len(self.cache) >= self.capacity:
            oldest = self.order[0]
            self.order = self.order[1:]
            del self.cache[oldest]

        self.cache[key] = value
        self.order.append(key)


def main():
    cache = LRUCache(3)
    cache.put(1, 10)
    cache.put(2, 20)
    cache.put(3, 30)
    v1 = cache.get(1)
    cache.put(4, 40)
    v2 = cache.get(2)
    v3 = cache.get(1)
    annotate(f"get(1) = {v1}, get(2) после вытеснения = {v2}, get(1) снова = {v3}")
PY

echo "✅ Создано $(ls *.py | wc -l) файлов"
ls -1 *.py
