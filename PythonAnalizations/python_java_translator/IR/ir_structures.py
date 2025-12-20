# ir_structures.py

class IRClass:
    def __init__(self, name):
        self.name = name
        self.fields = set()
        self.methods = {}


class IRMethod:
    def __init__(self, name, params):
        self.name = name
        self.params = params
        self.body = []
