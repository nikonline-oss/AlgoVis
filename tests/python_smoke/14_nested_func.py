def outer(n):
    def inner(x):
        return x * 2
    return inner(n)

def main():
    outer(5)
