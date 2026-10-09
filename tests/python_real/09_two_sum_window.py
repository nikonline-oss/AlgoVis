def two_sum(A, target):
    seen = {}
    for i in range(len(A)):
        complement = target - A[i]
        if complement in seen:
            return (seen[complement], i)
        seen[A[i]] = i
    return (-1, -1)


def max_sum_window(A, k):
    if len(A) < k:
        return 0

    window_sum = 0
    for i in range(k):
        window_sum = window_sum + A[i]

    best = window_sum
    for i in range(k, len(A)):
        window_sum = window_sum + A[i] - A[i - k]
        if window_sum > best:
            best = window_sum

    return best


def main():
    A = [2, 7, 11, 15, 3, 6]
    i, j = two_sum(A, 9)
    annotate(f"two sum 9: индексы {i}, {j}")

    B = [1, 4, 2, 10, 23, 3, 1, 0, 20]
    max_w = max_sum_window(B, 4)
    annotate(f"макс окно из 4: {max_w}")
