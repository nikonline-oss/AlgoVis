class LRUCache:
    def __init__(self, capacity):
        self.capacity = capacity
        self.cache = {}
        self.order = []

    def get(self, key):
        if key not in self.cache:
            return -1

        self.order.remove(key)
        self.order.append(key)
        return self.cache[key]

    def put(self, key, value):
        if key in self.cache:
            self.order.remove(key)
        elif len(self.cache) >= self.capacity:
            oldest = self.order[0]
            self.order = self.order[1:]
            del self.cache[oldest]

        self.cache[key] = value
        self.order.append(key)


def main():
    cache = LRUCache(3)
    cache.put(1, 10)
    cache.put(2, 20)
    cache.put(3, 30)
    v1 = cache.get(1)
    cache.put(4, 40)
    v2 = cache.get(2)
    v3 = cache.get(1)
    annotate(f"get(1) = {v1}, get(2) после вытеснения = {v2}, get(1) снова = {v3}")
