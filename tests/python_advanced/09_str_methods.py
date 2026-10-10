def normalize(s):
    return s.strip().lower()

def split_words(s):
    return s.split(",")

def join_words(A):
    return "-".join(A)

def main():
    s = "  Hello, World  "
    n = normalize(s)
    parts = split_words("a,b,c")
    joined = join_words(["x", "y", "z"])
