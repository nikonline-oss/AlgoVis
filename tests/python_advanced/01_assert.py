def check(A):
    assert len(A) > 0, "empty"
    assert all_positive(A), "negative found"
    return True

def all_positive(A):
    for x in A:
        if x < 0:
            return False
    return True

def main():
    A = [1, 2, 3]
    check(A)
