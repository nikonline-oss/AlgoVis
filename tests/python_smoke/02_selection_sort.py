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
