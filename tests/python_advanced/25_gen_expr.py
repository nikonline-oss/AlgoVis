def sum_of_squares(A):
    return sum(x * x for x in A)

def has_negative(A):
    return any(x < 0 for x in A)

def all_positive(A):
    return all(x > 0 for x in A)

def main():
    A = [1, 2, 3, 4]
    s = sum_of_squares(A)
    n = has_negative(A)
    p = all_positive(A)
