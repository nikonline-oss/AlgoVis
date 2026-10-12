#include <iostream>
#include <vector>
#include <algorithm>
#include <functional>
using namespace std;

int main() {
    vector<int> A = {5, 2, 8, 1, 9};
    sort(A.begin(), A.end(), greater<int>());
    cout << "first=" << A[0] << " last=" << A[4] << endl;
    return 0;
}
