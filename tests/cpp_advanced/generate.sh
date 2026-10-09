#!/bin/bash
set -e
cd "$(dirname "$0")"

# ── 01. vector: базовые операции ──
cat > 01_vector_basic.cpp <<'EOF'
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
EOF

# ── 02. vector: sort ──
cat > 02_vector_sort.cpp <<'EOF'
#include <iostream>
#include <vector>
#include <algorithm>
using namespace std;

int main() {
    vector<int> v = {5, 2, 8, 1, 9, 3};
    sort(v.begin(), v.end());
    return 0;
}
EOF

# ── 03. pair ──
cat > 03_pair.cpp <<'EOF'
#include <iostream>
#include <utility>
using namespace std;

pair<int, int> minMax(int A[], int n) {
    int lo = A[0];
    int hi = A[0];
    for (int i = 1; i < n; i++) {
        if (A[i] < lo) lo = A[i];
        if (A[i] > hi) hi = A[i];
    }
    return make_pair(lo, hi);
}

int main() {
    int A[] = {5, 2, 8, 1, 9};
    int n = 5;
    pair<int, int> result = minMax(A, n);
    return 0;
}
EOF

# ── 04. struct ──
cat > 04_struct.cpp <<'EOF'
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
EOF

# ── 05. class с методами ──
cat > 05_class.cpp <<'EOF'
#include <iostream>
using namespace std;

class Counter {
public:
    int value;

    Counter() {
        value = 0;
    }

    void inc() {
        value = value + 1;
    }

    int get() {
        return value;
    }
};

int main() {
    Counter c;
    c.inc();
    c.inc();
    c.inc();
    int v = c.get();
    return 0;
}
EOF

# ── 06. stack через vector ──
cat > 06_stack.cpp <<'EOF'
#include <iostream>
#include <vector>
using namespace std;

bool matchBrackets(string s) {
    vector<char> stack;
    for (int i = 0; i < s.length(); i++) {
        char ch = s[i];
        if (ch == '(') {
            stack.push_back(ch);
        } else if (ch == ')') {
            if (stack.size() == 0) return false;
            stack.pop_back();
        }
    }
    return stack.size() == 0;
}

int main() {
    bool r1 = matchBrackets("(())");
    bool r2 = matchBrackets("(()");
    return 0;
}
EOF

# ── 07. queue через vector ──
cat > 07_queue.cpp <<'EOF'
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
EOF

# ── 08. map ──
cat > 08_map.cpp <<'EOF'
#include <iostream>
#include <map>
#include <string>
using namespace std;

int main() {
    map<string, int> counts;
    counts["apple"] = 3;
    counts["banana"] = 5;
    counts["cherry"] = 2;

    int total = 0;
    for (auto it = counts.begin(); it != counts.end(); it++) {
        total = total + it->second;
    }
    return 0;
}
EOF

# ── 09. set ──
cat > 09_set.cpp <<'EOF'
#include <iostream>
#include <set>
using namespace std;

int main() {
    set<int> s;
    s.insert(5);
    s.insert(2);
    s.insert(8);
    s.insert(2);
    s.insert(5);

    int count = 0;
    for (auto it = s.begin(); it != s.end(); it++) {
        count = count + 1;
    }
    return 0;
}
EOF

# ── 10. string ──
cat > 10_string.cpp <<'EOF'
#include <iostream>
#include <string>
using namespace std;

string reverseStr(string s) {
    string result = "";
    for (int i = s.length() - 1; i >= 0; i--) {
        result = result + s[i];
    }
    return result;
}

int main() {
    string r = reverseStr("hello");
    return 0;
}
EOF

# ── 11. swap через ссылки ──
cat > 11_swap_ref.cpp <<'EOF'
#include <iostream>
using namespace std;

void swapRef(int &a, int &b) {
    int t = a;
    a = b;
    b = t;
}

int main() {
    int x = 1;
    int y = 2;
    swapRef(x, y);
    swapRef(x, y);
    return 0;
}
EOF

# ── 12. max через шаблон (базовый) ──
cat > 12_max_function.cpp <<'EOF'
#include <iostream>
using namespace std;

int maxOf(int a, int b) {
    if (a > b) return a;
    return b;
}

int maxArray(int A[], int n) {
    int best = A[0];
    for (int i = 1; i < n; i++) {
        best = maxOf(best, A[i]);
    }
    return best;
}

int main() {
    int A[] = {5, 2, 8, 1, 9, 3};
    int n = 6;
    int m = maxArray(A, n);
    return 0;
}
EOF

# ── 13. gcd рекурсивный ──
cat > 13_gcd_rec.cpp <<'EOF'
#include <iostream>
using namespace std;

int gcd(int a, int b) {
    if (b == 0) return a;
    return gcd(b, a % b);
}

int main() {
    int r = gcd(48, 18);
    return 0;
}
EOF

# ── 14. count chars ──
cat > 14_count_chars.cpp <<'EOF'
#include <iostream>
#include <string>
#include <map>
using namespace std;

int main() {
    string s = "mississippi";
    map<char, int> counts;
    for (int i = 0; i < s.length(); i++) {
        char c = s[i];
        if (counts.find(c) == counts.end()) {
            counts[c] = 1;
        } else {
            counts[c] = counts[c] + 1;
        }
    }
    return 0;
}
EOF

# ── 15. iterate vector ──
cat > 15_vector_iterate.cpp <<'EOF'
#include <iostream>
#include <vector>
using namespace std;

void printAll(vector<int> v) {
    for (int i = 0; i < v.size(); i++) {
        cout << v[i] << " ";
    }
    cout << endl;
}

int main() {
    vector<int> v = {1, 2, 3, 4, 5};
    printAll(v);
    return 0;
}
EOF

# ── 16. matrix через vector<vector> ──
cat > 16_matrix_vec.cpp <<'EOF'
#include <iostream>
#include <vector>
using namespace std;

int main() {
    vector<vector<int>> M;
    M.push_back({1, 2, 3});
    M.push_back({4, 5, 6});
    M.push_back({7, 8, 9});

    int trace = 0;
    for (int i = 0; i < M.size(); i++) {
        trace = trace + M[i][i];
    }
    return 0;
}
EOF

# ── 17. binary search в vector ──
cat > 17_vec_binary_search.cpp <<'EOF'
#include <iostream>
#include <vector>
using namespace std;

int binarySearch(vector<int> v, int target) {
    int lo = 0;
    int hi = v.size() - 1;
    while (lo <= hi) {
        int mid = (lo + hi) / 2;
        if (v[mid] == target) return mid;
        if (v[mid] < target) lo = mid + 1;
        else hi = mid - 1;
    }
    return -1;
}

int main() {
    vector<int> v = {1, 3, 5, 7, 9, 11, 13};
    int idx = binarySearch(v, 9);
    return 0;
}
EOF

# ── 18. swap сортировка с vector ──
cat > 18_vec_sort.cpp <<'EOF'
#include <iostream>
#include <vector>
using namespace std;

void bubbleSortVec(vector<int> &v) {
    int n = v.size();
    for (int i = 0; i < n - 1; i++) {
        for (int j = 0; j < n - i - 1; j++) {
            if (v[j] > v[j + 1]) {
                int t = v[j];
                v[j] = v[j + 1];
                v[j + 1] = t;
            }
        }
    }
}

int main() {
    vector<int> v = {5, 2, 8, 1, 9, 3};
    bubbleSortVec(v);
    return 0;
}
EOF

# ── 19. swap via std::swap ──
cat > 19_std_swap.cpp <<'EOF'
#include <iostream>
#include <algorithm>
using namespace std;

int main() {
    int a = 5;
    int b = 10;
    swap(a, b);
    int x = 1;
    int y = 2;
    swap(x, y);
    return 0;
}
EOF

# ── 20. ternary + bit operators ──
cat > 20_ternary.cpp <<'EOF'
#include <iostream>
using namespace std;

int absolute(int x) {
    return x < 0 ? -x : x;
}

int sign(int x) {
    return x > 0 ? 1 : (x < 0 ? -1 : 0);
}

int main() {
    int a = absolute(-5);
    int s = sign(10);
    int s2 = sign(-3);
    return 0;
}
EOF

# ── 21. lookup в map ──
cat > 21_map_lookup.cpp <<'EOF'
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
EOF

# ── 22. set объединение ──
cat > 22_set_ops.cpp <<'EOF'
#include <iostream>
#include <set>
using namespace std;

int countCommon(int A[], int n, int B[], int m) {
    set<int> sa;
    for (int i = 0; i < n; i++) sa.insert(A[i]);

    int common = 0;
    for (int j = 0; j < m; j++) {
        if (sa.count(B[j]) > 0) {
            common = common + 1;
        }
    }
    return common;
}

int main() {
    int A[] = {1, 2, 3, 4, 5};
    int B[] = {3, 4, 5, 6, 7};
    int c = countCommon(A, 5, B, 5);
    return 0;
}
EOF

# ── 23. reverse vector ──
cat > 23_reverse_vec.cpp <<'EOF'
#include <iostream>
#include <vector>
#include <algorithm>
using namespace std;

int main() {
    vector<int> v = {1, 2, 3, 4, 5};
    reverse(v.begin(), v.end());

    vector<int> w = {5, 2, 8, 1, 9};
    reverse(w.begin(), w.end());
    return 0;
}
EOF

# ── 24. min/max element ──
cat > 24_min_max_element.cpp <<'EOF'
#include <iostream>
#include <vector>
#include <algorithm>
using namespace std;

int main() {
    vector<int> v = {5, 2, 8, 1, 9, 3};
    int mn = *min_element(v.begin(), v.end());
    int mx = *max_element(v.begin(), v.end());
    return 0;
}
EOF

# ── 25. accumulate ──
cat > 25_accumulate.cpp <<'EOF'
#include <iostream>
#include <vector>
#include <numeric>
using namespace std;

int main() {
    vector<int> v = {1, 2, 3, 4, 5};
    int s = accumulate(v.begin(), v.end(), 0);
    return 0;
}
EOF

# ── 26. const и static ──
cat > 26_const_static.cpp <<'EOF'
#include <iostream>
using namespace std;

const int MAX_N = 100;
static int counter = 0;

void bump() {
    counter = counter + 1;
}

int main() {
    bump();
    bump();
    bump();
    return 0;
}
EOF

# ── 27. range-based for ──
cat > 27_range_for.cpp <<'EOF'
#include <iostream>
#include <vector>
using namespace std;

int sumRange(vector<int> &v) {
    int total = 0;
    for (int x : v) {
        total = total + x;
    }
    return total;
}

int main() {
    vector<int> v = {1, 2, 3, 4, 5};
    int s = sumRange(v);
    return 0;
}
EOF

# ── 28. template function ──
cat > 28_template.cpp <<'EOF'
#include <iostream>
using namespace std;

int triple(int x) {
    return x * 3;
}

int main() {
    int a = triple(5);
    int b = triple(10);
    return 0;
}
EOF

# ── 29. multiple return via pair ──
cat > 29_pair_return.cpp <<'EOF'
#include <iostream>
#include <utility>
using namespace std;

pair<int, int> divmod(int a, int b) {
    int q = a / b;
    int r = a % b;
    return make_pair(q, r);
}

int main() {
    pair<int, int> result = divmod(17, 5);
    int q = result.first;
    int r = result.second;
    return 0;
}
EOF

# ── 30. cout форматирование ──
cat > 30_cout.cpp <<'EOF'
#include <iostream>
#include <string>
using namespace std;

int main() {
    string name = "Alice";
    int age = 30;
    cout << "Name: " << name << endl;
    cout << "Age: " << age << endl;
    cout << "Next year: " << age + 1 << endl;
    return 0;
}
EOF

echo "✅ Создано $(ls *.cpp | wc -l) файлов"
ls -1 *.cpp | head -5
echo "..."
ls -1 *.cpp | tail -5
