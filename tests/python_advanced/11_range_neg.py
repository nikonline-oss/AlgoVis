def countdown(n):
    result = []
    for i in range(n, 0, -1):
        result.append(i)
    return result

def main():
    r = countdown(5)
