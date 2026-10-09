def unique_squares(A):
    return {x * x for x in A if x > 0}

def main():
    A = [-2, -1, 0, 1, 2, 3]
    s = unique_squares(A)
