def products(A, B):
    return [a * b for a in A for b in B]

def main():
    A = [1, 2]
    B = [10, 20]
    p = products(A, B)
