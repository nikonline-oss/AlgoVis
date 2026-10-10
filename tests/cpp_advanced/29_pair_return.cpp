#include <iostream>
#include <utility>
using namespace std;

pair<int, int> divmod(int a, int b) {
    int q = a / b;
    int r = a % b;
    return make_pair(q, r);
}

int main() {
    pair<int, int> result = divmod(17, 5);
    int q = result.first;
    int r = result.second;
    return 0;
}
