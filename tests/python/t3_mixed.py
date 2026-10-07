import math
from typing import List

PI = 3.14159

def memoize(func):
    cache = {}
    def wrapper(n):
        if n not in cache:
            cache[n] = func(n)
        return cache[n]
    return wrapper

@memoize
def slow_square(n):
    return n * n

class Circle:
    radius: float

    def __init__(self, r: float):
        self.radius = r

    def area(self) -> float:
        return PI * self.radius ** 2

    def circumference(self) -> float:
        return 2 * PI * self.radius

def process_circles(radii: List[float]) -> List[float]:
    result = []
    for r in radii:
        c = Circle(r)
        result.append(c.area())
    return result
