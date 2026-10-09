#include <iostream>
using namespace std;

int countDigits(int n) {
    if (n == 0) {
        return 0;
    }
    int count = 0;
    while (n > 0) {
        count = count + 1;
        n = n / 10;
    }
    return count;
}

int main() {
    int d = countDigits(12345);
    return 0;
}
