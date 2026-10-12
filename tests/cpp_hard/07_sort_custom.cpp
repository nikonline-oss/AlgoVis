#include <iostream>
#include <vector>
#include <algorithm>
using namespace std;

bool byAbs(int a, int b) {
    if (a < 0) a = -a;
    if (b < 0) b = -b;
    return a < b;
}

int main() {
    vector<int> A = {-5, 2, -8, 1, 9};
    sort(A.begin(), A.end(), byAbs);
    cout << "first=" << A[0] << endl;
    return 0;
}
