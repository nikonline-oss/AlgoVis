#include <iostream>
using namespace std;

void swapRef(int &a, int &b) {
    int t = a;
    a = b;
    b = t;
}

int main() {
    int x = 1;
    int y = 2;
    swapRef(x, y);
    swapRef(x, y);
    return 0;
}
