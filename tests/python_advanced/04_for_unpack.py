def process(pairs):
    total = 0
    for k, v in pairs:
        total = total + v
    return total

def main():
    pairs = [(1, 10), (2, 20), (3, 30)]
    s = process(pairs)
