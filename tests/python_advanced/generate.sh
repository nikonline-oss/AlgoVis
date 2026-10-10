#!/bin/bash
set -e
cd "$(dirname "$0")"

# ── 01: assert + raise ──
cat > 01_assert.py <<'PY'
def check(A):
    assert len(A) > 0, "empty"
    assert all_positive(A), "negative found"
    return True

def all_positive(A):
    for x in A:
        if x < 0:
            return False
    return True

def main():
    A = [1, 2, 3]
    check(A)
PY

# ── 02: try/except/finally ──
cat > 02_try_except.py <<'PY'
def safe_div(a, b):
    result = 0
    try:
        result = a // b
    except:
        result = -1
    finally:
        pass
    return result

def main():
    x = safe_div(10, 2)
    y = safe_div(10, 0)
PY

# ── 03: множественное присваивание ──
cat > 03_multi_assign.py <<'PY'
def main():
    a = b = c = 0
    a = b = 5
    x, y, z = 1, 2, 3
    x = y = z
PY

# ── 04: распаковка в for ──
cat > 04_for_unpack.py <<'PY'
def process(pairs):
    total = 0
    for k, v in pairs:
        total = total + v
    return total

def main():
    pairs = [(1, 10), (2, 20), (3, 30)]
    s = process(pairs)
PY

# ── 05: dict comprehension ──
cat > 05_dict_comp.py <<'PY'
def index_by_square(A):
    return {x: x * x for x in A}

def main():
    A = [1, 2, 3, 4]
    d = index_by_square(A)
PY

# ── 06: set comprehension ──
cat > 06_set_comp.py <<'PY'
def unique_squares(A):
    return {x * x for x in A if x > 0}

def main():
    A = [-2, -1, 0, 1, 2, 3]
    s = unique_squares(A)
PY

# ── 07: вложенные comprehension ──
cat > 07_nested_comp.py <<'PY'
def products(A, B):
    return [a * b for a in A for b in B]

def main():
    A = [1, 2]
    B = [10, 20]
    p = products(A, B)
PY

# ── 08: срезы со step ──
cat > 08_slice_step.py <<'PY'
def evens(A):
    return A[::2]

def reversed_copy(A):
    return A[::-1]

def main():
    A = [1, 2, 3, 4, 5, 6]
    e = evens(A)
    r = reversed_copy(A)
PY

# ── 09: строковые методы ──
cat > 09_str_methods.py <<'PY'
def normalize(s):
    return s.strip().lower()

def split_words(s):
    return s.split(",")

def join_words(A):
    return "-".join(A)

def main():
    s = "  Hello, World  "
    n = normalize(s)
    parts = split_words("a,b,c")
    joined = join_words(["x", "y", "z"])
PY

# ── 10: sorted с key ──
cat > 10_sorted_key.py <<'PY'
def by_abs(A):
    return sorted(A, key=abs)

def by_length(words):
    return sorted(words, key=len)

def main():
    A = [3, -1, -4, 2]
    sorted_A = by_abs(A)
    words = ["ccc", "a", "bb"]
    sorted_w = by_length(words)
PY

# ── 11: отрицательный range ──
cat > 11_range_neg.py <<'PY'
def countdown(n):
    result = []
    for i in range(n, 0, -1):
        result.append(i)
    return result

def main():
    r = countdown(5)
PY

# ── 12: наследование классов ──
cat > 12_inheritance.py <<'PY'
class Animal:
    def __init__(self, name):
        self.name = name

    def speak(self):
        return "..."

class Dog(Animal):
    def speak(self):
        return "Woof"

def main():
    d = Dog("Rex")
    s = d.speak()
PY

# ── 13: статические методы ──
cat > 13_static_method.py <<'PY'
class MathHelper:
    @staticmethod
    def double(x):
        return x * 2

    @staticmethod
    def triple(x):
        return x * 3

def main():
    a = MathHelper.double(5)
    b = MathHelper.triple(5)
PY

# ── 14: chr / ord / str / int ──
cat > 14_conversions.py <<'PY'
def to_char(code):
    return chr(code)

def to_code(ch):
    return ord(ch)

def to_int(s):
    return int(s)

def to_str(n):
    return str(n)

def main():
    c = to_char(65)
    o = to_code("A")
    i = to_int("42")
    s = to_str(123)
PY

# ── 15: global / nonlocal ──
cat > 15_global.py <<'PY'
counter = 0

def increment():
    global counter
    counter = counter + 1

def main():
    increment()
    increment()
    increment()
PY

# ── 16: while/break/continue ──
cat > 16_while_control.py <<'PY'
def find_first_negative(A):
    i = 0
    while i < len(A):
        if A[i] < 0:
            return i
        i = i + 1
    return -1

def skip_zeros(A):
    result = []
    for x in A:
        if x == 0:
            continue
        result.append(x)
    return result

def main():
    idx = find_first_negative([1, 2, -3, 4])
    nz = skip_zeros([0, 1, 0, 2, 0])
PY

# ── 17: множественное возвращение из функций ──
cat > 17_multi_return.py <<'PY'
def min_max_avg(A):
    if len(A) == 0:
        return 0, 0, 0
    lo = A[0]
    hi = A[0]
    total = 0
    for x in A:
        if x < lo:
            lo = x
        if x > hi:
            hi = x
        total = total + x
    avg = total // len(A)
    return lo, hi, avg

def main():
    lo, hi, avg = min_max_avg([5, 2, 8, 1, 9])
PY

# ── 18: lambda (для filter/map/sorted) ──
cat > 18_lambda.py <<'PY'
def evens(A):
    return list(filter(lambda x: x % 2 == 0, A))

def doubled(A):
    return list(map(lambda x: x * 2, A))

def main():
    A = [1, 2, 3, 4, 5, 6]
    e = evens(A)
    d = doubled(A)
PY

# ── 19: *args ──
cat > 19_args.py <<'PY'
def total(*nums):
    s = 0
    for n in nums:
        s = s + n
    return s

def main():
    a = total(1, 2, 3)
    b = total(10, 20, 30, 40)
PY

# ── 20: **kwargs ──
cat > 20_kwargs.py <<'PY'
def describe(**info):
    keys = []
    for k in info:
        keys.append(k)
    return keys

def main():
    result = describe(name="Alice", age=30, city="NYC")
PY

# ── 21: деструктуризация кортежа ──
cat > 21_tuple_destr.py <<'PY'
def swap(a, b):
    return b, a

def main():
    x, y = 1, 2
    x, y = swap(x, y)
    head, *rest = [1, 2, 3, 4]
PY

# ── 22: f-strings с выражениями ──
cat > 22_fstring_expr.py <<'PY'
def greet(name, count):
    return f"Hello, {name}! You have {count * 2} items"

def main():
    msg = greet("Alice", 5)
    annotate(f"len = {len(msg)}")
PY

# ── 23: enumerate / zip ──
cat > 23_enumerate_zip.py <<'PY'
def pairs(A, B):
    result = []
    for i, x in enumerate(A):
        result.append(x)
    return result

def zip_lists(A, B):
    result = []
    for a, b in zip(A, B):
        result.append(a + b)
    return result

def main():
    p = pairs(["a", "b"], [1, 2])
    z = zip_lists([1, 2, 3], [10, 20, 30])
PY

# ── 24: тернарный оператор в выражениях ──
cat > 24_ternary.py <<'PY'
def absolute(x):
    return -x if x < 0 else x

def classify(x):
    result = "positive" if x > 0 else ("zero" if x == 0 else "negative")
    return result

def main():
    a = absolute(-5)
    c = classify(10)
PY

# ── 25: генераторные выражения ──
cat > 25_gen_expr.py <<'PY'
def sum_of_squares(A):
    return sum(x * x for x in A)

def has_negative(A):
    return any(x < 0 for x in A)

def all_positive(A):
    return all(x > 0 for x in A)

def main():
    A = [1, 2, 3, 4]
    s = sum_of_squares(A)
    n = has_negative(A)
    p = all_positive(A)
PY

# ── 26: assert с несколькими проверками ──
cat > 26_assert_pure.py <<'PY'
def is_sorted(A):
    for i in range(len(A) - 1):
        if A[i] > A[i + 1]:
            return False
    return True

def verify(A):
    assert is_sorted(A), "not sorted"
    assert len(A) > 0
    return A

def main():
    A = [1, 2, 3, 4, 5]
    verify(A)
PY

# ── 27: вложенные классы + методы с параметрами ──
cat > 27_class_complex.py <<'PY'
class Stack:
    def __init__(self):
        self.items = []

    def push(self, x):
        self.items.append(x)

    def pop(self):
        if len(self.items) == 0:
            return None
        last = self.items[len(self.items) - 1]
        self.items = self.items[0:len(self.items) - 1]
        return last

    def size(self):
        return len(self.items)

def main():
    s = Stack()
    s.push(1)
    s.push(2)
    s.push(3)
    a = s.pop()
    n = s.size()
PY

# ── 28: import из стандартной библиотеки ──
cat > 28_imports.py <<'PY'
import math
from typing import List

def circle_area(r):
    return math.pi * r * r

def main():
    a = circle_area(5)
PY

# ── 29: передача функций как аргументов ──
cat > 29_higher_order.py <<'PY'
def apply_twice(f, x):
    return f(f(x))

def inc(x):
    return x + 1

def double(x):
    return x * 2

def main():
    a = apply_twice(inc, 5)
    b = apply_twice(double, 5)
PY

# ── 30: работа с символами в строках ──
cat > 30_string_chars.py <<'PY'
def reverse_str(s):
    result = ""
    for i in range(len(s) - 1, -1, -1):
        result = result + s[i]
    return result

def count_char(s, ch):
    count = 0
    for c in s:
        if c == ch:
            count = count + 1
    return count

def main():
    r = reverse_str("hello")
    n = count_char("mississippi", "s")
PY

echo "✅ Создано $(ls *.py | wc -l) файлов"
ls -1 *.py
