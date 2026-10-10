#!/bin/bash
set -e
cd "$(dirname "$0")"

# ── 01. Bubble sort ──
cat > 01_bubble_sort.cpp <<'EOF'
#include <iostream>
using namespace std;

void bubbleSort(int A[], int n) {
    for (int i = 0; i < n - 1; i++) {
        for (int j = 0; j < n - i - 1; j++) {
            if (A[j] > A[j + 1]) {
                int temp = A[j];
                A[j] = A[j + 1];
                A[j + 1] = temp;
            }
        }
    }
}

int main() {
    int A[] = {5, 2, 8, 1, 9, 3};
    int n = 6;
    bubbleSort(A, n);
    return 0;
}
EOF

# ── 02. Selection sort ──
cat > 02_selection_sort.cpp <<'EOF'
#include <iostream>
using namespace std;

void selectionSort(int A[], int n) {
    for (int i = 0; i < n - 1; i++) {
        int minIdx = i;
        for (int j = i + 1; j < n; j++) {
            if (A[j] < A[minIdx]) {
                minIdx = j;
            }
        }
        if (minIdx != i) {
            int temp = A[i];
            A[i] = A[minIdx];
            A[minIdx] = temp;
        }
    }
}

int main() {
    int A[] = {5, 2, 8, 1, 9, 3};
    int n = 6;
    selectionSort(A, n);
    return 0;
}
EOF

# ── 03. Insertion sort ──
cat > 03_insertion_sort.cpp <<'EOF'
#include <iostream>
using namespace std;

void insertionSort(int A[], int n) {
    for (int i = 1; i < n; i++) {
        int key = A[i];
        int j = i - 1;
        while (j >= 0 && A[j] > key) {
            A[j + 1] = A[j];
            j = j - 1;
        }
        A[j + 1] = key;
    }
}

int main() {
    int A[] = {5, 2, 8, 1, 9};
    int n = 5;
    insertionSort(A, n);
    return 0;
}
EOF

# ── 04. Linear search ──
cat > 04_linear_search.cpp <<'EOF'
#include <iostream>
using namespace std;

int linearSearch(int A[], int n, int target) {
    for (int i = 0; i < n; i++) {
        if (A[i] == target) {
            return i;
        }
    }
    return -1;
}

int main() {
    int A[] = {4, 2, 7, 1, 9, 5};
    int n = 6;
    int idx = linearSearch(A, n, 7);
    return 0;
}
EOF

# ── 05. Binary search ──
cat > 05_binary_search.cpp <<'EOF'
#include <iostream>
using namespace std;

int binarySearch(int A[], int n, int target) {
    int lo = 0;
    int hi = n - 1;
    while (lo <= hi) {
        int mid = (lo + hi) / 2;
        if (A[mid] == target) {
            return mid;
        } else if (A[mid] < target) {
            lo = mid + 1;
        } else {
            hi = mid - 1;
        }
    }
    return -1;
}

int main() {
    int A[] = {1, 3, 5, 7, 9, 11, 13};
    int n = 7;
    int idx = binarySearch(A, n, 9);
    return 0;
}
EOF

# ── 06. Factorial (рекурсия) ──
cat > 06_factorial.cpp <<'EOF'
#include <iostream>
using namespace std;

int factorial(int n) {
    if (n <= 1) {
        return 1;
    }
    return n * factorial(n - 1);
}

int main() {
    int result = factorial(5);
    return 0;
}
EOF

# ── 07. Fibonacci (рекурсия) ──
cat > 07_fibonacci.cpp <<'EOF'
#include <iostream>
using namespace std;

int fib(int n) {
    if (n < 2) {
        return n;
    }
    return fib(n - 1) + fib(n - 2);
}

int main() {
    int result = fib(6);
    return 0;
}
EOF

# ── 08. GCD ──
cat > 08_gcd.cpp <<'EOF'
#include <iostream>
using namespace std;

int gcd(int a, int b) {
    while (b != 0) {
        int t = b;
        b = a % b;
        a = t;
    }
    return a;
}

int main() {
    int result = gcd(48, 18);
    return 0;
}
EOF

# ── 09. Sum of array ──
cat > 09_sum_array.cpp <<'EOF'
#include <iostream>
using namespace std;

int sumArray(int A[], int n) {
    int total = 0;
    for (int i = 0; i < n; i++) {
        total = total + A[i];
    }
    return total;
}

int main() {
    int A[] = {1, 2, 3, 4, 5};
    int n = 5;
    int s = sumArray(A, n);
    return 0;
}
EOF

# ── 10. Reverse array in place ──
cat > 10_reverse_array.cpp <<'EOF'
#include <iostream>
using namespace std;

void reverseArray(int A[], int n) {
    int i = 0;
    int j = n - 1;
    while (i < j) {
        int temp = A[i];
        A[i] = A[j];
        A[j] = temp;
        i = i + 1;
        j = j - 1;
    }
}

int main() {
    int A[] = {1, 2, 3, 4, 5, 6};
    int n = 6;
    reverseArray(A, n);
    return 0;
}
EOF

# ── 11. Min/Max ──
cat > 11_min_max.cpp <<'EOF'
#include <iostream>
using namespace std;

void findMinMax(int A[], int n) {
    int lo = A[0];
    int hi = A[0];
    for (int i = 1; i < n; i++) {
        if (A[i] < lo) {
            lo = A[i];
        }
        if (A[i] > hi) {
            hi = A[i];
        }
    }
}

int main() {
    int A[] = {5, 2, 8, 1, 9, 3};
    int n = 6;
    findMinMax(A, n);
    return 0;
}
EOF

# ── 12. Count elements ──
cat > 12_count_positive.cpp <<'EOF'
#include <iostream>
using namespace std;

int countPositive(int A[], int n) {
    int count = 0;
    for (int i = 0; i < n; i++) {
        if (A[i] > 0) {
            count = count + 1;
        }
    }
    return count;
}

int main() {
    int A[] = {-3, 1, -2, 4, 5, -1};
    int n = 6;
    int p = countPositive(A, n);
    return 0;
}
EOF

# ── 13. Is prime ──
cat > 13_is_prime.cpp <<'EOF'
#include <iostream>
using namespace std;

bool isPrime(int n) {
    if (n < 2) {
        return false;
    }
    int i = 2;
    while (i * i <= n) {
        if (n % i == 0) {
            return false;
        }
        i = i + 1;
    }
    return true;
}

int main() {
    bool result = isPrime(17);
    bool result2 = isPrime(15);
    return 0;
}
EOF

# ── 14. Power (рекурсия) ──
cat > 14_power.cpp <<'EOF'
#include <iostream>
using namespace std;

int power(int base, int exp) {
    if (exp == 0) {
        return 1;
    }
    return base * power(base, exp - 1);
}

int main() {
    int result = power(2, 10);
    return 0;
}
EOF

# ── 15. Count digits ──
cat > 15_count_digits.cpp <<'EOF'
#include <iostream>
using namespace std;

int countDigits(int n) {
    if (n == 0) {
        return 0;
    }
    int count = 0;
    while (n > 0) {
        count = count + 1;
        n = n / 10;
    }
    return count;
}

int main() {
    int d = countDigits(12345);
    return 0;
}
EOF

# ── 16. Sum digits ──
cat > 16_sum_digits.cpp <<'EOF'
#include <iostream>
using namespace std;

int sumDigits(int n) {
    int total = 0;
    while (n > 0) {
        total = total + n % 10;
        n = n / 10;
    }
    return total;
}

int main() {
    int s = sumDigits(9876);
    return 0;
}
EOF

# ── 17. Print array ──
cat > 17_print_array.cpp <<'EOF'
#include <iostream>
using namespace std;

void printArray(int A[], int n) {
    for (int i = 0; i < n; i++) {
        cout << A[i] << " ";
    }
    cout << endl;
}

int main() {
    int A[] = {1, 2, 3, 4, 5};
    int n = 5;
    printArray(A, n);
    return 0;
}
EOF

# ── 18. Matrix transpose (2D array) ──
cat > 18_matrix_transpose.cpp <<'EOF'
#include <iostream>
using namespace std;

void transpose(int M[][3], int M2[][3]) {
    for (int i = 0; i < 3; i++) {
        for (int j = 0; j < 3; j++) {
            M2[j][i] = M[i][j];
        }
    }
}

int main() {
    int M[3][3] = {{1, 2, 3}, {4, 5, 6}, {7, 8, 9}};
    int M2[3][3];
    transpose(M, M2);
    return 0;
}
EOF

# ── 19. Swap via reference ──
cat > 19_swap.cpp <<'EOF'
#include <iostream>
using namespace std;

void swapValues(int &a, int &b) {
    int temp = a;
    a = b;
    b = temp;
}

int main() {
    int x = 5;
    int y = 10;
    swapValues(x, y);
    return 0;
}
EOF

# ── 20. FizzBuzz-style loop ──
cat > 20_divisible_count.cpp <<'EOF'
#include <iostream>
using namespace std;

int countDivisible(int n, int d) {
    int count = 0;
    for (int i = 1; i <= n; i++) {
        if (i % d == 0) {
            count = count + 1;
        }
    }
    return count;
}

int main() {
    int c = countDivisible(20, 3);
    return 0;
}
EOF

echo "✅ Создано $(ls *.cpp | wc -l) файлов"
ls -1 *.cpp
