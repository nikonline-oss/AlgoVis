#include <iostream>
#include <vector>
using namespace std;

int main() {
    vector<int> queue;
    queue.push_back(1);
    queue.push_back(2);
    queue.push_back(3);

    int count = 0;
    while (queue.size() > 0) {
        int front = queue[0];
        queue.erase(queue.begin());
        count = count + 1;
    }
    return 0;
}
