#include <iostream>
#include <vector>
#include <map>
using namespace std;

void dfs(int node, map<int, vector<int>> &graph, vector<bool> &visited) {
    visited[node] = true;
    vector<int> neighbors = graph[node];
    for (int i = 0; i < neighbors.size(); i++) {
        int neighbor = neighbors[i];
        if (!visited[neighbor]) {
            dfs(neighbor, graph, visited);
        }
    }
}

int countComponents(int n, map<int, vector<int>> &graph) {
    vector<bool> visited(n, false);
    int count = 0;
    for (int i = 0; i < n; i++) {
        if (!visited[i]) {
            count = count + 1;
            dfs(i, graph, visited);
        }
    }
    return count;
}

int main() {
    map<int, vector<int>> graph;
    graph[0].push_back(1);
    graph[1].push_back(0);
    graph[2].push_back(3);
    graph[3].push_back(2);
    int c = countComponents(5, graph);
    cout << "Components: " << c << endl;
    return 0;
}
