def gcd(a, b):
    while b != 0:
        t = b
        b = a % b
        a = t
    return a

def main():
    gcd(48, 18)
