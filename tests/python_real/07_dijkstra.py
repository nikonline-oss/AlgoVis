def dijkstra(graph, start):
    distances = {}
    for node in graph:
        distances[node] = 10 ** 9
    distances[start] = 0

    visited = set()

    while len(visited) < len(graph):
        current = None
        best = 10 ** 9

        for node in graph:
            if node not in visited:
                if distances[node] < best:
                    best = distances[node]
                    current = node

        if current is None:
            break

        visited.add(current)

        for neighbor, weight in graph[current]:
            new_dist = distances[current] + weight
            if new_dist < distances[neighbor]:
                distances[neighbor] = new_dist

    return distances


def main():
    graph = {
        "A": [("B", 4), ("C", 2)],
        "B": [("C", 5), ("D", 10)],
        "C": [("E", 3)],
        "D": [("F", 11)],
        "E": [("D", 4)],
        "F": []
    }
    dist = dijkstra(graph, "A")
    annotate(f"расстояние до F: {dist['F']}")
