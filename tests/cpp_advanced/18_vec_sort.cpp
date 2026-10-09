#include <iostream>
#include <vector>
using namespace std;

void bubbleSortVec(vector<int> &v) {
    int n = v.size();
    for (int i = 0; i < n - 1; i++) {
        for (int j = 0; j < n - i - 1; j++) {
            if (v[j] > v[j + 1]) {
                int t = v[j];
                v[j] = v[j + 1];
                v[j + 1] = t;
            }
        }
    }
}

int main() {
    vector<int> v = {5, 2, 8, 1, 9, 3};
    bubbleSortVec(v);
    return 0;
}
