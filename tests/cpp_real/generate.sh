#!/bin/bash
set -e
cd "$(dirname "$0")"

# ── 01. BFS на сетке ──
cat > 01_grid_bfs.cpp <<'EOF'
#include <iostream>
#include <vector>
#include <queue>
#include <utility>
using namespace std;

int shortestPath(vector<vector<int>> &grid, pair<int, int> start, pair<int, int> end) {
    int rows = grid.size();
    int cols = grid[0].size();

    vector<vector<bool>> visited(rows, vector<bool>(cols, false));
    queue<pair<int, int>> q;
    q.push(start);
    visited[start.first][start.second] = true;

    int dist = 0;
    while (!q.empty()) {
        int size = q.size();
        for (int i = 0; i < size; i++) {
            pair<int, int> node = q.front();
            q.pop();

            if (node.first == end.first && node.second == end.second) {
                return dist;
            }

            int dr[4] = {0, 0, 1, -1};
            int dc[4] = {1, -1, 0, 0};
            for (int k = 0; k < 4; k++) {
                int nr = node.first + dr[k];
                int nc = node.second + dc[k];

                if (nr >= 0 && nr < rows && nc >= 0 && nc < cols) {
                    if (grid[nr][nc] == 0 && !visited[nr][nc]) {
                        visited[nr][nc] = true;
                        q.push(make_pair(nr, nc));
                    }
                }
            }
        }
        dist++;
    }

    return -1;
}

int main() {
    vector<vector<int>> grid = {
        {0, 0, 0, 0, 0},
        {0, 1, 1, 1, 0},
        {0, 0, 0, 1, 0},
        {1, 1, 0, 0, 0},
        {0, 0, 0, 0, 0}
    };
    int d = shortestPath(grid, make_pair(0, 0), make_pair(4, 4));
    cout << "Shortest path: " << d << endl;
    return 0;
}
EOF

# ── 02. Дейкстра ──
cat > 02_dijkstra.cpp <<'EOF'
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
EOF

# ── 03. Ханойские башни ──
cat > 03_hanoi.cpp <<'EOF'
#include <iostream>
#include <vector>
#include <string>
using namespace std;

void hanoi(int n, string fromPeg, string toPeg, string auxPeg, vector<string> &moves) {
    if (n == 0) return;

    hanoi(n - 1, fromPeg, auxPeg, toPeg, moves);
    moves.push_back(fromPeg + " -> " + toPeg);
    hanoi(n - 1, auxPeg, toPeg, fromPeg, moves);
}

int main() {
    vector<string> moves;
    hanoi(3, "A", "C", "B", moves);
    cout << "Total moves: " << moves.size() << endl;
    return 0;
}
EOF

# ── 04. LRU-кэш через класс ──
cat > 04_lru_cache.cpp <<'EOF'
#include <iostream>
#include <vector>
#include <map>
using namespace std;

class LRUCache {
public:
    int capacity;
    map<int, int> data;
    vector<int> order;

    LRUCache(int cap) {
        capacity = cap;
    }

    int get(int key) {
        if (data.find(key) == data.end()) {
            return -1;
        }
        for (int i = 0; i < order.size(); i++) {
            if (order[i] == key) {
                order.erase(order.begin() + i);
                break;
            }
        }
        order.push_back(key);
        return data[key];
    }

    void put(int key, int value) {
        if (data.find(key) != data.end()) {
            for (int i = 0; i < order.size(); i++) {
                if (order[i] == key) {
                    order.erase(order.begin() + i);
                    break;
                }
            }
        } else if (data.size() >= capacity) {
            int oldest = order[0];
            order.erase(order.begin());
            data.erase(oldest);
        }
        data[key] = value;
        order.push_back(key);
    }
};

int main() {
    LRUCache cache(3);
    cache.put(1, 10);
    cache.put(2, 20);
    cache.put(3, 30);
    int v1 = cache.get(1);
    cache.put(4, 40);
    int v2 = cache.get(2);
    int v3 = cache.get(1);
    cout << "v1=" << v1 << " v2=" << v2 << " v3=" << v3 << endl;
    return 0;
}
EOF

# ── 05. QuickSort ──
cat > 05_quicksort.cpp <<'EOF'
#include <iostream>
#include <vector>
using namespace std;

int partition(vector<int> &A, int lo, int hi) {
    int pivot = A[hi];
    int i = lo - 1;
    for (int j = lo; j < hi; j++) {
        if (A[j] <= pivot) {
            i = i + 1;
            int t = A[i];
            A[i] = A[j];
            A[j] = t;
        }
    }
    int t = A[i + 1];
    A[i + 1] = A[hi];
    A[hi] = t;
    return i + 1;
}

void quicksort(vector<int> &A, int lo, int hi) {
    if (lo >= hi) return;
    int p = partition(A, lo, hi);
    quicksort(A, lo, p - 1);
    quicksort(A, p + 1, hi);
}

int main() {
    vector<int> A = {5, 2, 8, 1, 9, 3, 7, 4, 6};
    quicksort(A, 0, A.size() - 1);
    cout << "First: " << A[0] << " Last: " << A[A.size() - 1] << endl;
    return 0;
}
EOF

# ── 06. MergeSort ──
cat > 06_mergesort.cpp <<'EOF'
#include <iostream>
#include <vector>
using namespace std;

void merge(vector<int> &A, int lo, int mid, int hi) {
    vector<int> left;
    vector<int> right;

    for (int i = lo; i <= mid; i++) left.push_back(A[i]);
    for (int j = mid + 1; j <= hi; j++) right.push_back(A[j]);

    int i = 0;
    int j = 0;
    int k = lo;

    while (i < left.size() && j < right.size()) {
        if (left[i] <= right[j]) {
            A[k] = left[i];
            i = i + 1;
        } else {
            A[k] = right[j];
            j = j + 1;
        }
        k = k + 1;
    }

    while (i < left.size()) {
        A[k] = left[i];
        i = i + 1;
        k = k + 1;
    }
    while (j < right.size()) {
        A[k] = right[j];
        j = j + 1;
        k = k + 1;
    }
}

void mergesort(vector<int> &A, int lo, int hi) {
    if (lo >= hi) return;
    int mid = (lo + hi) / 2;
    mergesort(A, lo, mid);
    mergesort(A, mid + 1, hi);
    merge(A, lo, mid, hi);
}

int main() {
    vector<int> A = {5, 2, 8, 1, 9, 3, 7, 4};
    mergesort(A, 0, A.size() - 1);
    cout << "Sorted size: " << A.size() << endl;
    return 0;
}
EOF

# ── 07. Проверка скобок ──
cat > 07_brackets.cpp <<'EOF'
#include <iostream>
#include <string>
#include <vector>
using namespace std;

bool checkBrackets(string s) {
    vector<char> stack;
    for (int i = 0; i < s.length(); i++) {
        char ch = s[i];
        if (ch == '(' || ch == '[' || ch == '{') {
            stack.push_back(ch);
        } else if (ch == ')' || ch == ']' || ch == '}') {
            if (stack.size() == 0) return false;
            char top = stack[stack.size() - 1];
            stack.pop_back();

            if (ch == ')' && top != '(') return false;
            if (ch == ']' && top != '[') return false;
            if (ch == '}' && top != '{') return false;
        }
    }
    return stack.size() == 0;
}

int main() {
    bool r1 = checkBrackets("((()))");
    bool r2 = checkBrackets("([{}])");
    bool r3 = checkBrackets("((())");
    bool r4 = checkBrackets("([)]");
    cout << r1 << " " << r2 << " " << r3 << " " << r4 << endl;
    return 0;
}
EOF

# ── 08. Перестановки ──
cat > 08_permutations.cpp <<'EOF'
#include <iostream>
#include <vector>
using namespace std;

void permute(vector<int> &current, vector<int> &remaining, vector<vector<int>> &result) {
    if (remaining.size() == 0) {
        result.push_back(current);
        return;
    }

    for (int i = 0; i < remaining.size(); i++) {
        int x = remaining[i];
        current.push_back(x);
        remaining.erase(remaining.begin() + i);

        permute(current, remaining, result);

        remaining.insert(remaining.begin() + i, x);
        current.pop_back();
    }
}

int main() {
    vector<int> A = {1, 2, 3};
    vector<int> current;
    vector<vector<int>> result;
    permute(current, A, result);
    cout << "Total permutations: " << result.size() << endl;
    return 0;
}
EOF

# ── 09. Компоненты связности ──
cat > 09_connected.cpp <<'EOF'
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
EOF

# ── 10. Подсчёт слов ──
cat > 10_word_count.cpp <<'EOF'
#include <iostream>
#include <string>
#include <map>
#include <vector>
using namespace std;

vector<string> split(string s, char delim) {
    vector<string> result;
    string current = "";
    for (int i = 0; i < s.length(); i++) {
        if (s[i] == delim) {
            if (current.length() > 0) {
                result.push_back(current);
                current = "";
            }
        } else {
            current = current + s[i];
        }
    }
    if (current.length() > 0) {
        result.push_back(current);
    }
    return result;
}

map<string, int> countWords(string text) {
    vector<string> words = split(text, ' ');
    map<string, int> counts;
    for (int i = 0; i < words.size(); i++) {
        string w = words[i];
        if (counts.find(w) == counts.end()) {
            counts[w] = 1;
        } else {
            counts[w] = counts[w] + 1;
        }
    }
    return counts;
}

int main() {
    map<string, int> counts = countWords("the cat and the dog and the bird");
    cout << "the: " << counts["the"] << endl;
    cout << "and: " << counts["and"] << endl;
    cout << "unique: " << counts.size() << endl;
    return 0;
}
EOF

echo "✅ Создано $(ls *.cpp | wc -l) файлов"
ls -1 *.cpp
