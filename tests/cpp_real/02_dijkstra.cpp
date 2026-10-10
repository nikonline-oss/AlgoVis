#include <iostream>
#include <vector>
#include <map>
#include <string>
using namespace std;

map<string, int> dijkstra(map<string, vector<pair<string, int>>> &graph, string start) {
    map<string, int> distances;
    for (auto it = graph.begin(); it != graph.end(); it++) {
        distances[it->first] = 1000000;
    }
    distances[start] = 0;

    vector<string> visited;

    while (visited.size() < graph.size()) {
        string current = "";
        int best = 1000000;

        for (auto it = graph.begin(); it != graph.end(); it++) {
            string node = it->first;
            bool wasVisited = false;
            for (int i = 0; i < visited.size(); i++) {
                if (visited[i] == node) wasVisited = true;
            }
            if (!wasVisited && distances[node] < best) {
                best = distances[node];
                current = node;
            }
        }

        if (current == "") break;

        visited.push_back(current);

        vector<pair<string, int>> neighbors = graph[current];
        for (int i = 0; i < neighbors.size(); i++) {
            string neighbor = neighbors[i].first;
            int weight = neighbors[i].second;
            int newDist = distances[current] + weight;
            if (newDist < distances[neighbor]) {
                distances[neighbor] = newDist;
            }
        }
    }

    return distances;
}

int main() {
    map<string, vector<pair<string, int>>> graph;
    graph["A"].push_back(make_pair("B", 4));
    graph["A"].push_back(make_pair("C", 2));
    graph["B"].push_back(make_pair("C", 5));
    graph["B"].push_back(make_pair("D", 10));
    graph["C"].push_back(make_pair("E", 3));
    graph["D"].push_back(make_pair("F", 11));
    graph["E"].push_back(make_pair("D", 4));
    graph["F"] = vector<pair<string, int>>();

    map<string, int> dist = dijkstra(graph, "A");
    cout << "Distance to F: " << dist["F"] << endl;
    return 0;
}
