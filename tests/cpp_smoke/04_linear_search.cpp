#include <iostream>
using namespace std;

int linearSearch(int A[], int n, int target) {
    for (int i = 0; i < n; i++) {
        if (A[i] == target) {
            return i;
        }
    }
    return -1;
}

int main() {
    int A[] = {4, 2, 7, 1, 9, 5};
    int n = 6;
    int idx = linearSearch(A, n, 7);
    return 0;
}
