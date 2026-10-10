def min_max_avg(A):
    if len(A) == 0:
        return 0, 0, 0
    lo = A[0]
    hi = A[0]
    total = 0
    for x in A:
        if x < lo:
            lo = x
        if x > hi:
            hi = x
        total = total + x
    avg = total // len(A)
    return lo, hi, avg

def main():
    lo, hi, avg = min_max_avg([5, 2, 8, 1, 9])
