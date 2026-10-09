#include <iostream>
#include <map>
#include <string>
using namespace std;

int main() {
    map<string, int> ages;
    ages["Alice"] = 30;
    ages["Bob"] = 25;

    int a = ages["Alice"];
    int b = ages["Bob"];
    int notFound = ages.count("Charlie");
    return 0;
}
