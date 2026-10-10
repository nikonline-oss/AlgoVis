#include <iostream>
using namespace std;

int countPositive(int A[], int n) {
    int count = 0;
    for (int i = 0; i < n; i++) {
        if (A[i] > 0) {
            count = count + 1;
        }
    }
    return count;
}

int main() {
    int A[] = {-3, 1, -2, 4, 5, -1};
    int n = 6;
    int p = countPositive(A, n);
    return 0;
}
