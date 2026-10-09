def make_fib():
    cache = {}

    def fib(n):
        if n in cache:
            return cache[n]
        if n < 2:
            cache[n] = n
            return n
        result = fib(n - 1) + fib(n - 2)
        cache[n] = result
        return result

    return fib


def count_coins(coins, amount):
    dp = [amount + 1] * (amount + 1)
    dp[0] = 0

    for i in range(1, amount + 1):
        for c in coins:
            if c <= i:
                if dp[i - c] + 1 < dp[i]:
                    dp[i] = dp[i - c] + 1

    if dp[amount] == amount + 1:
        return -1
    return dp[amount]


def main():
    fib = make_fib()
    f20 = fib(20)
    f30 = fib(30)
    annotate(f"fib(30) = {f30}")

    coins = [1, 2, 5]
    min_coins = count_coins(coins, 11)
    annotate(f"минимум монет для 11: {min_coins}")
