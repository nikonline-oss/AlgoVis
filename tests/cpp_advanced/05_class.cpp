#include <iostream>
using namespace std;

class Counter {
public:
    int value;

    Counter() {
        value = 0;
    }

    void inc() {
        value = value + 1;
    }

    int get() {
        return value;
    }
};

int main() {
    Counter c;
    c.inc();
    c.inc();
    c.inc();
    int v = c.get();
    return 0;
}
