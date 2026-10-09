#include <iostream>
#include <vector>
using namespace std;

int main() {
    vector<vector<int>> M;
    M.push_back({1, 2, 3});
    M.push_back({4, 5, 6});
    M.push_back({7, 8, 9});

    int trace = 0;
    for (int i = 0; i < M.size(); i++) {
        trace = trace + M[i][i];
    }
    return 0;
}
