#include <iostream>
#include <string>
using namespace std;

string reverseStr(string s) {
    string result = "";
    for (int i = s.length() - 1; i >= 0; i--) {
        result = result + s[i];
    }
    return result;
}

int main() {
    string r = reverseStr("hello");
    return 0;
}
