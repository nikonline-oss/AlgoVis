#include <iostream>
using namespace std;

int binarySearch(int A[], int n, int target) {
    int lo = 0;
    int hi = n - 1;
    while (lo <= hi) {
        int mid = (lo + hi) / 2;
        if (A[mid] == target) {
            return mid;
        } else if (A[mid] < target) {
            lo = mid + 1;
        } else {
            hi = mid - 1;
        }
    }
    return -1;
}

int main() {
    int A[] = {1, 3, 5, 7, 9, 11, 13};
    int n = 7;
    int idx = binarySearch(A, n, 9);
    return 0;
}
