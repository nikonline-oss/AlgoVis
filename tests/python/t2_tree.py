class Tree:
    def __init__(self, value):
        self.value = value
        self.left = None
        self.right = None

    def insert(self, v):
        if v < self.value:
            if self.left is None:
                self.left = Tree(v)
            else:
                self.left.insert(v)
        else:
            if self.right is None:
                self.right = Tree(v)
            else:
                self.right.insert(v)

    def height(self):
        lh = self.left.height() if self.left else 0
        rh = self.right.height() if self.right else 0
        return 1 + max(lh, rh)
