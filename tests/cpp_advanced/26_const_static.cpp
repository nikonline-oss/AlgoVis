#include <iostream>
using namespace std;

const int MAX_N = 100;
static int counter = 0;

void bump() {
    counter = counter + 1;
}

int main() {
    bump();
    bump();
    bump();
    return 0;
}
