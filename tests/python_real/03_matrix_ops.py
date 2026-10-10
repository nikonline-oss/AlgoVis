def transpose(M):
    if len(M) == 0:
        return []
    rows = len(M)
    cols = len(M[0])

    result = []
    for j in range(cols):
        new_row = []
        for i in range(rows):
            new_row.append(M[i][j])
        result.append(new_row)

    return result


def trace(M):
    n = min(len(M), len(M[0]))
    total = 0
    for i in range(n):
        total = total + M[i][i]
    return total


def row_sums(M):
    result = []
    for row in M:
        total = 0
        for x in row:
            total = total + x
        result.append(total)
    return result


def main():
    M = [
        [1, 2, 3],
        [4, 5, 6],
        [7, 8, 9]
    ]
    T = transpose(M)
    t = trace(M)
    rs = row_sums(M)
    annotate(f"trace = {t}")
