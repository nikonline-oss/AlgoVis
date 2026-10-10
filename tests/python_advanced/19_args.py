def total(*nums):
    s = 0
    for n in nums:
        s = s + n
    return s

def main():
    a = total(1, 2, 3)
    b = total(10, 20, 30, 40)
