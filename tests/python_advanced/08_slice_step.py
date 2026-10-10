def evens(A):
    return A[::2]

def reversed_copy(A):
    return A[::-1]

def main():
    A = [1, 2, 3, 4, 5, 6]
    e = evens(A)
    r = reversed_copy(A)
