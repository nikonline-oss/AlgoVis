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
