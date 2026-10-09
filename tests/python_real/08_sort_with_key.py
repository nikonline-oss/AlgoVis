def sort_by_second(pairs):
    return sorted(pairs, key=second_element)


def second_element(p):
    return p[1]


def group_by_length(words):
    groups = {}
    for w in words:
        ln = len(w)
        if ln in groups:
            groups[ln].append(w)
        else:
            groups[ln] = [w]
    return groups


def main():
    pairs = [(1, 5), (2, 3), (3, 8), (4, 1)]
    sorted_p = sort_by_second(pairs)
    annotate(f"отсортировано по второму: {sorted_p}")

    words = ["apple", "cat", "dog", "banana", "hi"]
    groups = group_by_length(words)
    annotate(f"группы: {len(groups)}")
