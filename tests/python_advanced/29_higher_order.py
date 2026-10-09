def apply_twice(f, x):
    return f(f(x))

def inc(x):
    return x + 1

def double(x):
    return x * 2

def main():
    a = apply_twice(inc, 5)
    b = apply_twice(double, 5)
