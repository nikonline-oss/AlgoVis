#include <iostream>
#include <vector>
using namespace std;

int binarySearch(vector<int> v, int target) {
    int lo = 0;
    int hi = v.size() - 1;
    while (lo <= hi) {
        int mid = (lo + hi) / 2;
        if (v[mid] == target) return mid;
        if (v[mid] < target) lo = mid + 1;
        else hi = mid - 1;
    }
    return -1;
}

int main() {
    vector<int> v = {1, 3, 5, 7, 9, 11, 13};
    int idx = binarySearch(v, 9);
    return 0;
}
