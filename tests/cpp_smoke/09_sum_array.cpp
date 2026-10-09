#include <iostream>
using namespace std;

int sumArray(int A[], int n) {
    int total = 0;
    for (int i = 0; i < n; i++) {
        total = total + A[i];
    }
    return total;
}

int main() {
    int A[] = {1, 2, 3, 4, 5};
    int n = 5;
    int s = sumArray(A, n);
    return 0;
}
