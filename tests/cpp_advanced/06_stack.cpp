#include <iostream>
#include <vector>
using namespace std;

bool matchBrackets(string s) {
    vector<char> stack;
    for (int i = 0; i < s.length(); i++) {
        char ch = s[i];
        if (ch == '(') {
            stack.push_back(ch);
        } else if (ch == ')') {
            if (stack.size() == 0) return false;
            stack.pop_back();
        }
    }
    return stack.size() == 0;
}

int main() {
    bool r1 = matchBrackets("(())");
    bool r2 = matchBrackets("(()");
    return 0;
}
