#include <iostream>
using namespace std;

void findMinMax(int A[], int n) {
    int lo = A[0];
    int hi = A[0];
    for (int i = 1; i < n; i++) {
        if (A[i] < lo) {
            lo = A[i];
        }
        if (A[i] > hi) {
            hi = A[i];
        }
    }
}

int main() {
    int A[] = {5, 2, 8, 1, 9, 3};
    int n = 6;
    findMinMax(A, n);
    return 0;
}
