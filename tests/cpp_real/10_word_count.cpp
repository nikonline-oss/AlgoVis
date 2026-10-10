#include <iostream>
#include <string>
#include <map>
#include <vector>
using namespace std;

vector<string> split(string s, char delim) {
    vector<string> result;
    string current = "";
    for (int i = 0; i < s.length(); i++) {
        if (s[i] == delim) {
            if (current.length() > 0) {
                result.push_back(current);
                current = "";
            }
        } else {
            current = current + s[i];
        }
    }
    if (current.length() > 0) {
        result.push_back(current);
    }
    return result;
}

map<string, int> countWords(string text) {
    vector<string> words = split(text, ' ');
    map<string, int> counts;
    for (int i = 0; i < words.size(); i++) {
        string w = words[i];
        if (counts.find(w) == counts.end()) {
            counts[w] = 1;
        } else {
            counts[w] = counts[w] + 1;
        }
    }
    return counts;
}

int main() {
    map<string, int> counts = countWords("the cat and the dog and the bird");
    cout << "the: " << counts["the"] << endl;
    cout << "and: " << counts["and"] << endl;
    cout << "unique: " << counts.size() << endl;
    return 0;
}
