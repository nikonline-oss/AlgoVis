#include <iostream>
#include <set>
using namespace std;

int main() {
    set<int> s;
    s.insert(5);
    s.insert(2);
    s.insert(8);
    s.insert(2);
    s.insert(5);

    int count = 0;
    for (auto it = s.begin(); it != s.end(); it++) {
        count = count + 1;
    }
    return 0;
}
