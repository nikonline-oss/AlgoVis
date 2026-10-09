def pairs(A, B):
    result = []
    for i, x in enumerate(A):
        result.append(x)
    return result

def zip_lists(A, B):
    result = []
    for a, b in zip(A, B):
        result.append(a + b)
    return result

def main():
    p = pairs(["a", "b"], [1, 2])
    z = zip_lists([1, 2, 3], [10, 20, 30])
