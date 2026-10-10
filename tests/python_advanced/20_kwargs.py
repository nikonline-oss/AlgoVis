def describe(**info):
    keys = []
    for k in info:
        keys.append(k)
    return keys

def main():
    result = describe(name="Alice", age=30, city="NYC")
