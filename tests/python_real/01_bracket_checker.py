class Stack:
    def __init__(self):
        self.items = []

    def push(self, x):
        self.items.append(x)

    def pop(self):
        if len(self.items) == 0:
            return None
        top = self.items[-1]
        self.items = self.items[:-1]
        return top

    def is_empty(self):
        return len(self.items) == 0


def check_brackets(s):
    pairs = {")": "(", "]": "[", "}": "{"}
    stack = Stack()

    for ch in s:
        if ch in "([{":
            stack.push(ch)
        elif ch in ")]}":
            if stack.is_empty():
                return False
            opening = stack.pop()
            if opening != pairs[ch]:
                return False

    return stack.is_empty()


def main():
    test1 = "((()))"
    test2 = "([{}])"
    test3 = "((())"
    test4 = "([)]"

    r1 = check_brackets(test1)
    r2 = check_brackets(test2)
    r3 = check_brackets(test3)
    r4 = check_brackets(test4)

    annotate(f"test1 = {r1}, test4 = {r4}")
