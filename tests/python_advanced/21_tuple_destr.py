def swap(a, b):
    return b, a

def main():
    x, y = 1, 2
    x, y = swap(x, y)
    head, *rest = [1, 2, 3, 4]
