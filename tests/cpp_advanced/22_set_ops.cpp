#include <iostream>
#include <set>
using namespace std;

int countCommon(int A[], int n, int B[], int m) {
    set<int> sa;
    for (int i = 0; i < n; i++) sa.insert(A[i]);

    int common = 0;
    for (int j = 0; j < m; j++) {
        if (sa.count(B[j]) > 0) {
            common = common + 1;
        }
    }
    return common;
}

int main() {
    int A[] = {1, 2, 3, 4, 5};
    int B[] = {3, 4, 5, 6, 7};
    int c = countCommon(A, 5, B, 5);
    return 0;
}
