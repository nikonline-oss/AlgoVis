def greet(name, count):
    return f"Hello, {name}! You have {count * 2} items"

def main():
    msg = greet("Alice", 5)
    annotate(f"len = {len(msg)}")
