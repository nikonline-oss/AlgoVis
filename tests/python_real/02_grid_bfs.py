def shortest_path(grid, start, end):
    rows = len(grid)
    cols = len(grid[0])

    queue = [start]
    visited = set()
    visited.add(start)
    dist = 0

    while len(queue) > 0:
        size = len(queue)
        for _ in range(size):
            node = queue[0]
            queue = queue[1:]

            if node[0] == end[0] and node[1] == end[1]:
                return dist

            for dr, dc in [(0, 1), (0, -1), (1, 0), (-1, 0)]:
                nr = node[0] + dr
                nc = node[1] + dc

                if 0 <= nr < rows and 0 <= nc < cols:
                    if grid[nr][nc] == 0 and (nr, nc) not in visited:
                        visited.add((nr, nc))
                        queue.append((nr, nc))

        dist = dist + 1

    return -1


def main():
    grid = [
        [0, 0, 0, 0, 0],
        [0, 1, 1, 1, 0],
        [0, 0, 0, 1, 0],
        [1, 1, 0, 0, 0],
        [0, 0, 0, 0, 0]
    ]
    d = shortest_path(grid, (0, 0), (4, 4))
    annotate(f"кратчайший путь: {d} шагов")
