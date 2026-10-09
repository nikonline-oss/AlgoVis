class MathHelper:
    @staticmethod
    def double(x):
        return x * 2

    @staticmethod
    def triple(x):
        return x * 3

def main():
    a = MathHelper.double(5)
    b = MathHelper.triple(5)
