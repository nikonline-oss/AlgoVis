def find_first_negative(A):
    i = 0
    while i < len(A):
        if A[i] < 0:
            return i
        i = i + 1
    return -1

def skip_zeros(A):
    result = []
    for x in A:
        if x == 0:
            continue
        result.append(x)
    return result

def main():
    idx = find_first_negative([1, 2, -3, 4])
    nz = skip_zeros([0, 1, 0, 2, 0])
