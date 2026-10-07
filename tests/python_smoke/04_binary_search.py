def binary_search(A, target):
    lo = 0
    hi = len(A) - 1
    while lo <= hi:
        mid = (lo + hi) // 2
        if A[mid] == target:
            return mid
        elif A[mid] < target:
            lo = mid + 1
        else:
            hi = mid - 1
    return -1

def main():
    A = [1, 3, 5, 7, 9, 11, 13]
    binary_search(A, 9)
