#include <iostream>
#include <vector>
#include <string>
using namespace std;

void hanoi(int n, string fromPeg, string toPeg, string auxPeg, vector<string> &moves) {
    if (n == 0) return;

    hanoi(n - 1, fromPeg, auxPeg, toPeg, moves);
    moves.push_back(fromPeg + " -> " + toPeg);
    hanoi(n - 1, auxPeg, toPeg, fromPeg, moves);
}

int main() {
    vector<string> moves;
    hanoi(3, "A", "C", "B", moves);
    cout << "Total moves: " << moves.size() << endl;
    return 0;
}
