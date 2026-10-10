def reverse_str(s):
    result = ""
    for i in range(len(s) - 1, -1, -1):
        result = result + s[i]
    return result

def count_char(s, ch):
    count = 0
    for c in s:
        if c == ch:
            count = count + 1
    return count

def main():
    r = reverse_str("hello")
    n = count_char("mississippi", "s")
