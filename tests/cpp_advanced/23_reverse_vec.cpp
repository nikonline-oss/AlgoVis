#include <iostream>
#include <vector>
#include <algorithm>
using namespace std;

int main() {
    vector<int> v = {1, 2, 3, 4, 5};
    reverse(v.begin(), v.end());

    vector<int> w = {5, 2, 8, 1, 9};
    reverse(w.begin(), w.end());
    return 0;
}
