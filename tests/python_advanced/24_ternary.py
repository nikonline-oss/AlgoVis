def absolute(x):
    return -x if x < 0 else x

def classify(x):
    result = "positive" if x > 0 else ("zero" if x == 0 else "negative")
    return result

def main():
    a = absolute(-5)
    c = classify(10)
