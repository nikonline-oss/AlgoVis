#include <iostream>
#include <vector>
#include <algorithm>
using namespace std;

int main() {
    vector<int> v = {5, 2, 8, 1, 9, 3};
    int mn = *min_element(v.begin(), v.end());
    int mx = *max_element(v.begin(), v.end());
    return 0;
}
