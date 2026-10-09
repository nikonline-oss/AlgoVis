#include <iostream>
#include <string>
#include <map>
using namespace std;

int main() {
    string s = "mississippi";
    map<char, int> counts;
    for (int i = 0; i < s.length(); i++) {
        char c = s[i];
        if (counts.find(c) == counts.end()) {
            counts[c] = 1;
        } else {
            counts[c] = counts[c] + 1;
        }
    }
    return 0;
}
