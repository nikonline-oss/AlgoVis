def quicksort(A):
    if len(A) <= 1:
        return A
    pivot = A[0]
    left = []
    right = []
    for i in range(1, len(A)):
        if A[i] < pivot:
            left.append(A[i])
        else:
            right.append(A[i])
    return quicksort(left) + [pivot] + quicksort(right)

def main():
    A = [5, 2, 8, 1, 9, 3]
    quicksort(A)
