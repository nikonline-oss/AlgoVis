#include <iostream>
using namespace std;

int maxOf(int a, int b) {
    if (a > b) return a;
    return b;
}

int maxArray(int A[], int n) {
    int best = A[0];
    for (int i = 1; i < n; i++) {
        best = maxOf(best, A[i]);
    }
    return best;
}

int main() {
    int A[] = {5, 2, 8, 1, 9, 3};
    int n = 6;
    int m = maxArray(A, n);
    return 0;
}
