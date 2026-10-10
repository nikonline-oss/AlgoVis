def linear_search(A, target):
    for i in range(len(A)):
        if A[i] == target:
            return i
    return -1

def main():
    A = [4, 2, 7, 1, 9, 5]
    linear_search(A, 7)
