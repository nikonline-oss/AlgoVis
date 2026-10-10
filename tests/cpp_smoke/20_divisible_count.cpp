#include <iostream>
using namespace std;

int countDivisible(int n, int d) {
    int count = 0;
    for (int i = 1; i <= n; i++) {
        if (i % d == 0) {
            count = count + 1;
        }
    }
    return count;
}

int main() {
    int c = countDivisible(20, 3);
    return 0;
}
