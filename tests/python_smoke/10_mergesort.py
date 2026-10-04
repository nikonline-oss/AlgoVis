def merge(left, right):
    result = []
    i = 0
    j = 0
    while i < len(left) and j < len(right):
        if left[i] <= right[j]:
            result.append(left[i])
            i = i + 1
        else:
            result.append(right[j])
            j = j + 1
    while i < len(left):
        result.append(left[i])
        i = i + 1
    while j < len(right):
        result.append(right[j])
        j = j + 1
    return result

def mergesort(A):
    if len(A) <= 1:
        return A
    mid = len(A) // 2
    left = A[0:mid]
    right = A[mid:len(A)]
    return merge(mergesort(left), mergesort(right))

def main():
    A = [5, 2, 8, 1, 9, 3, 7, 4]
    mergesort(A)
