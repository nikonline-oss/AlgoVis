#include <iostream>
using namespace std;

struct Point {
    int x;
    int y;
};

int distanceSq(Point a, Point b) {
    int dx = a.x - b.x;
    int dy = a.y - b.y;
    return dx * dx + dy * dy;
}

int main() {
    Point p1;
    p1.x = 3;
    p1.y = 4;
    Point p2;
    p2.x = 0;
    p2.y = 0;
    int d = distanceSq(p1, p2);
    return 0;
}
