def evens(A):
    return list(filter(lambda x: x % 2 == 0, A))

def doubled(A):
    return list(map(lambda x: x * 2, A))

def main():
    A = [1, 2, 3, 4, 5, 6]
    e = evens(A)
    d = doubled(A)
