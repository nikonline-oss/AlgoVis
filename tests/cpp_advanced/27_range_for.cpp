#include <iostream>
#include <vector>
using namespace std;

int sumRange(vector<int> &v) {
    int total = 0;
    for (int x : v) {
        total = total + x;
    }
    return total;
}

int main() {
    vector<int> v = {1, 2, 3, 4, 5};
    int s = sumRange(v);
    return 0;
}
