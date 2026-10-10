#include <iostream>
#include <string>
#include <vector>
using namespace std;

bool checkBrackets(string s) {
    vector<char> stack;
    for (int i = 0; i < s.length(); i++) {
        char ch = s[i];
        if (ch == '(' || ch == '[' || ch == '{') {
            stack.push_back(ch);
        } else if (ch == ')' || ch == ']' || ch == '}') {
            if (stack.size() == 0) return false;
            char top = stack[stack.size() - 1];
            stack.pop_back();

            if (ch == ')' && top != '(') return false;
            if (ch == ']' && top != '[') return false;
            if (ch == '}' && top != '{') return false;
        }
    }
    return stack.size() == 0;
}

int main() {
    bool r1 = checkBrackets("((()))");
    bool r2 = checkBrackets("([{}])");
    bool r3 = checkBrackets("((())");
    bool r4 = checkBrackets("([)]");
    cout << r1 << " " << r2 << " " << r3 << " " << r4 << endl;
    return 0;
}
