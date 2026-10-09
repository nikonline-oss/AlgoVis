#include <iostream>
using namespace std;

void transpose(int M[][3], int M2[][3]) {
    for (int i = 0; i < 3; i++) {
        for (int j = 0; j < 3; j++) {
            M2[j][i] = M[i][j];
        }
    }
}

int main() {
    int M[3][3] = {{1, 2, 3}, {4, 5, 6}, {7, 8, 9}};
    int M2[3][3];
    transpose(M, M2);
    return 0;
}
