#include <iostream>
using namespace std;

template<typename T>
T mymax(T a, T b) {
    if (a > b) return a;
    return b;
}

int main() {
    int m = mymax(3, 7);
    cout << "max=" << m << endl;
    return 0;
}
