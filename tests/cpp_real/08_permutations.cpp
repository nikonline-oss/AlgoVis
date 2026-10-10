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
