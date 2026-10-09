class Stack:
    def __init__(self):
        self.items = []

    def push(self, x):
        self.items.append(x)

    def pop(self):
        if len(self.items) == 0:
            return None
        last = self.items[len(self.items) - 1]
        self.items = self.items[0:len(self.items) - 1]
        return last

    def size(self):
        return len(self.items)

def main():
    s = Stack()
    s.push(1)
    s.push(2)
    s.push(3)
    a = s.pop()
    n = s.size()
