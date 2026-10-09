def hanoi(n, from_p, to_p, aux_p, moves):
    if n == 0:
        return

    hanoi(n - 1, from_p, aux_p, to_p, moves)
    moves.append((n, from_p, to_p))
    hanoi(n - 1, aux_p, to_p, from_p, moves)


def permutations(A):
    if len(A) <= 1:
        return [A]

    result = []
    for i in range(len(A)):
        first = A[i]
        rest = A[:i] + A[i + 1:]
        for perm in permutations(rest):
            result.append([first] + perm)
    return result


def main():
    moves = []
    hanoi(3, "A", "C", "B", moves)
    annotate(f"перемещений: {len(moves)}")

    perms = permutations([1, 2, 3])
    annotate(f"перестановок: {len(perms)}")
