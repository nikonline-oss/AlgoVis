#include <iostream>
#include <vector>
using namespace std;

void merge(vector<int> &A, int lo, int mid, int hi) {
    vector<int> left;
    vector<int> right;

    for (int i = lo; i <= mid; i++) left.push_back(A[i]);
    for (int j = mid + 1; j <= hi; j++) right.push_back(A[j]);

    int i = 0;
    int j = 0;
    int k = lo;

    while (i < left.size() && j < right.size()) {
        if (left[i] <= right[j]) {
            A[k] = left[i];
            i = i + 1;
        } else {
            A[k] = right[j];
            j = j + 1;
        }
        k = k + 1;
    }

    while (i < left.size()) {
        A[k] = left[i];
        i = i + 1;
        k = k + 1;
    }
    while (j < right.size()) {
        A[k] = right[j];
        j = j + 1;
        k = k + 1;
    }
}

void mergesort(vector<int> &A, int lo, int hi) {
    if (lo >= hi) return;
    int mid = (lo + hi) / 2;
    mergesort(A, lo, mid);
    mergesort(A, mid + 1, hi);
    merge(A, lo, mid, hi);
}

int main() {
    vector<int> A = {5, 2, 8, 1, 9, 3, 7, 4};
    mergesort(A, 0, A.size() - 1);
    cout << "Sorted size: " << A.size() << endl;
    return 0;
}
