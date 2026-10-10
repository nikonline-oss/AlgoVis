def bfs(graph, start):
    visited = []
    queue = [start]
    while len(queue) > 0:
        node = queue[0]
        queue = queue[1:len(queue)]
        if node not in visited:
            visited.append(node)
            for neighbor in graph[node]:
                queue.append(neighbor)
    return visited

def main():
    g = {"A": ["B", "C"], "B": ["D"], "C": [], "D": []}
    bfs(g, "A")
