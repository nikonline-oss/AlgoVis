#include <iostream>
#include <queue>
using namespace std;

int main() {
    priority_queue<int> pq;
    pq.push(3);
    pq.push(1);
    pq.push(4);
    int t = pq.top();
    pq.pop();
    cout << "top=" << t << " size=" << pq.size() << " empty=" << pq.empty() << endl;
    return 0;
}
