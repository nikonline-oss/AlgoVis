#include <iostream>
using namespace std;

void printArray(int A[], int n) {
    for (int i = 0; i < n; i++) {
        cout << A[i] << " ";
    }
    cout << endl;
}

int main() {
    int A[] = {1, 2, 3, 4, 5};
    int n = 5;
    printArray(A, n);
    return 0;
}
