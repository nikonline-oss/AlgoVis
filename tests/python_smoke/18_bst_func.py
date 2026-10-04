def insert(node, value):
    if node is None:
        return {"value": value, "left": None, "right": None}
    if value < node["value"]:
        node["left"] = insert(node["left"], value)
    else:
        node["right"] = insert(node["right"], value)
    return node

def main():
    root = None
    root = insert(root, 50)
    root = insert(root, 30)
    root = insert(root, 70)
