#include <iostream>
using namespace std;

int sumDigits(int n) {
    int total = 0;
    while (n > 0) {
        total = total + n % 10;
        n = n / 10;
    }
    return total;
}

int main() {
    int s = sumDigits(9876);
    return 0;
}
