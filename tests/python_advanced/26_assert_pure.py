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
