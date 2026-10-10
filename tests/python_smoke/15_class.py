class Counter:
    def __init__(self):
        self.value = 0

    def inc(self):
        self.value = self.value + 1

def main():
    c = Counter()
    c.inc()
    c.inc()
