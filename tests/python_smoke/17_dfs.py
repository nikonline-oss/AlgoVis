def dfs(graph, node, visited):
    if node in visited:
        return
    visited.append(node)
    for neighbor in graph[node]:
        dfs(graph, neighbor, visited)

def main():
    g = {"A": ["B", "C"], "B": ["D"], "C": [], "D": []}
    visited = []
    dfs(g, "A", visited)
