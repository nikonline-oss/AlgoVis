def index_by_square(A):
    return {x: x * x for x in A}

def main():
    A = [1, 2, 3, 4]
    d = index_by_square(A)
