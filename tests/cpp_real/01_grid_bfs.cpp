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
