def is_palindrome(s):
    cleaned = ""
    for c in s:
        if c.isalpha():
            cleaned = cleaned + c.lower()

    i = 0
    j = len(cleaned) - 1
    while i < j:
        if cleaned[i] != cleaned[j]:
            return False
        i = i + 1
        j = j - 1
    return True


def are_anagrams(a, b):
    if len(a) != len(b):
        return False
    count_a = {}
    for c in a:
        if c in count_a:
            count_a[c] = count_a[c] + 1
        else:
            count_a[c] = 1

    for c in b:
        if c not in count_a:
            return False
        count_a[c] = count_a[c] - 1
        if count_a[c] < 0:
            return False

    return True


def word_count(text):
    words = text.lower().split()
    counts = {}
    for w in words:
        if w in counts:
            counts[w] = counts[w] + 1
        else:
            counts[w] = 1
    return counts


def main():
    p1 = is_palindrome("A man a plan a canal Panama")
    p2 = is_palindrome("hello")
    a = are_anagrams("listen", "silent")
    wc = word_count("the cat and the dog and the bird")
    annotate(f"palindrome: {p1}, {p2}")
    annotate(f"anagrams: {a}")
