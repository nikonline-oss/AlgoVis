#include <iostream>
#include <utility>
using namespace std;

pair<int, int> minMax(int A[], int n) {
    int lo = A[0];
    int hi = A[0];
    for (int i = 1; i < n; i++) {
        if (A[i] < lo) lo = A[i];
        if (A[i] > hi) hi = A[i];
    }
    return make_pair(lo, hi);
}

int main() {
    int A[] = {5, 2, 8, 1, 9};
    int n = 5;
    pair<int, int> result = minMax(A, n);
    return 0;
}
