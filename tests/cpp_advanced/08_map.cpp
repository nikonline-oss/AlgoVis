#include <iostream>
#include <map>
#include <string>
using namespace std;

int main() {
    map<string, int> counts;
    counts["apple"] = 3;
    counts["banana"] = 5;
    counts["cherry"] = 2;

    int total = 0;
    for (auto it = counts.begin(); it != counts.end(); it++) {
        total = total + it->second;
    }
    return 0;
}
