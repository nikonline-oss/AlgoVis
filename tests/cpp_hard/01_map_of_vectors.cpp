#include <iostream>
#include <map>
#include <vector>
using namespace std;

int main() {
    map<int, vector<int>> adj;
    adj[1].push_back(2);
    adj[1].push_back(3);
    adj[2].push_back(1);
    cout << "size adj[1]=" << adj[1].size() << endl;
    return 0;
}
