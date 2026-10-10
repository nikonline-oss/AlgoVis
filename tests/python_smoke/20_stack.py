def match_brackets(s):
    stack = []
    for ch in s:
        if ch == "(":
            stack.append(ch)
        elif ch == ")":
            if len(stack) == 0:
                return False
            stack.pop()
    return len(stack) == 0

def main():
    match_brackets("(())")
