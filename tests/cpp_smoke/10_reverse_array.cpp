#include <iostream>
using namespace std;

void reverseArray(int A[], int n) {
    int i = 0;
    int j = n - 1;
    while (i < j) {
        int temp = A[i];
        A[i] = A[j];
        A[j] = temp;
        i = i + 1;
        j = j - 1;
    }
}

int main() {
    int A[] = {1, 2, 3, 4, 5, 6};
    int n = 6;
    reverseArray(A, n);
    return 0;
}
