#include <iostream>
using namespace std;

int absolute(int x) {
    return x < 0 ? -x : x;
}

int sign(int x) {
    return x > 0 ? 1 : (x < 0 ? -1 : 0);
}

int main() {
    int a = absolute(-5);
    int s = sign(10);
    int s2 = sign(-3);
    return 0;
}
