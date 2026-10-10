#include <iostream>
#include <vector>
using namespace std;

int partition(vector<int> &A, int lo, int hi) {
    int pivot = A[hi];
    int i = lo - 1;
    for (int j = lo; j < hi; j++) {
        if (A[j] <= pivot) {
            i = i + 1;
            int t = A[i];
            A[i] = A[j];
            A[j] = t;
        }
    }
    int t = A[i + 1];
    A[i + 1] = A[hi];
    A[hi] = t;
    return i + 1;
}

void quicksort(vector<int> &A, int lo, int hi) {
    if (lo >= hi) return;
    int p = partition(A, lo, hi);
    quicksort(A, lo, p - 1);
    quicksort(A, p + 1, hi);
}

int main() {
    vector<int> A = {5, 2, 8, 1, 9, 3, 7, 4, 6};
    quicksort(A, 0, A.size() - 1);
    cout << "First: " << A[0] << " Last: " << A[A.size() - 1] << endl;
    return 0;
}
