def count_words(text):
    counts = {}
    for word in text:
        if word in counts:
            counts[word] = counts[word] + 1
        else:
            counts[word] = 1
    return counts

def main():
    count_words(["a", "b", "a", "c", "a"])
