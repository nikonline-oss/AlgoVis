#include <iostream>
using namespace std;

void selectionSort(int A[], int n) {
    for (int i = 0; i < n - 1; i++) {
        int minIdx = i;
        for (int j = i + 1; j < n; j++) {
            if (A[j] < A[minIdx]) {
                minIdx = j;
            }
        }
        if (minIdx != i) {
            int temp = A[i];
            A[i] = A[minIdx];
            A[minIdx] = temp;
        }
    }
}

int main() {
    int A[] = {5, 2, 8, 1, 9, 3};
    int n = 6;
    selectionSort(A, n);
    return 0;
}
