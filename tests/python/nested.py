def outer():
    def inner():
        return 42
    return inner()

class Outer:
    class Inner:
        def method(self):
            pass
