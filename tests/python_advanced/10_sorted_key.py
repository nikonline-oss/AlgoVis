def by_abs(A):
    return sorted(A, key=abs)

def by_length(words):
    return sorted(words, key=len)

def main():
    A = [3, -1, -4, 2]
    sorted_A = by_abs(A)
    words = ["ccc", "a", "bb"]
    sorted_w = by_length(words)
