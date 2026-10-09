#include <iostream>
#include <vector>
using namespace std;

int sumVector(vector<int> v) {
    int total = 0;
    for (int i = 0; i < v.size(); i++) {
        total = total + v[i];
    }
    return total;
}

int main() {
    vector<int> v;
    v.push_back(5);
    v.push_back(2);
    v.push_back(8);
    v.push_back(1);
    int s = sumVector(v);
    return 0;
}
