def safe_div(a, b):
    result = 0
    try:
        result = a // b
    except:
        result = -1
    finally:
        pass
    return result

def main():
    x = safe_div(10, 2)
    y = safe_div(10, 0)
