#include <iostream>
#include <vector>
#include <map>
using namespace std;

class LRUCache {
public:
    int capacity;
    map<int, int> data;
    vector<int> order;

    LRUCache(int cap) {
        capacity = cap;
    }

    int get(int key) {
        if (data.find(key) == data.end()) {
            return -1;
        }
        for (int i = 0; i < order.size(); i++) {
            if (order[i] == key) {
                order.erase(order.begin() + i);
                break;
            }
        }
        order.push_back(key);
        return data[key];
    }

    void put(int key, int value) {
        if (data.find(key) != data.end()) {
            for (int i = 0; i < order.size(); i++) {
                if (order[i] == key) {
                    order.erase(order.begin() + i);
                    break;
                }
            }
        } else if (data.size() >= capacity) {
            int oldest = order[0];
            order.erase(order.begin());
            data.erase(oldest);
        }
        data[key] = value;
        order.push_back(key);
    }
};

int main() {
    LRUCache cache(3);
    cache.put(1, 10);
    cache.put(2, 20);
    cache.put(3, 30);
    int v1 = cache.get(1);
    cache.put(4, 40);
    int v2 = cache.get(2);
    int v3 = cache.get(1);
    cout << "v1=" << v1 << " v2=" << v2 << " v3=" << v3 << endl;
    return 0;
}
