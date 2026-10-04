def swap_pair(a, b):
    a, b = b, a
    return a

def min_max(A):
    lo, hi = A[0], A[0]
    for i in range(len(A)):
        if A[i] < lo:
            lo = A[i]
        if A[i] > hi:
            hi = A[i]
    return lo

def main():
    min_max([5, 2, 8, 1, 9])
