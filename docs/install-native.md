# Установка tree-sitter

AlgoVis использует tree-sitter через P/Invoke. Нужны две `.so`:

- `libtree-sitter.so` — ядро
- `libtree-sitter-python.so` — грамматика Python

**Версии важны.** Мы проверяли на паре:

- `tree-sitter` **v0.22.6** (ABI 14)
- `tree-sitter-python` **v0.21.0** (ABI 14)

Более новые версии грамматики используют ABI 15 и вызовут segfault.

## Сборка

```bash
sudo apt update
sudo apt install -y build-essential git

# Ядро
cd /tmp
git clone --depth 1 --branch v0.22.6 https://github.com/tree-sitter/tree-sitter.git
cd tree-sitter
make
sudo make install

# Грамматика Python
cd /tmp
git clone --depth 1 --branch v0.21.0 https://github.com/tree-sitter/tree-sitter-python.git
cd tree-sitter-python
gcc -shared -fPIC -o libtree-sitter-python.so src/parser.c src/scanner.c -Isrc
sudo cp libtree-sitter-python.so /usr/local/lib/

# Регистрация
echo "/usr/local/lib" | sudo tee /etc/ld.so.conf.d/local.conf
sudo ldconfig
```

## Проверка установки

```bash
ldconfig -p | grep tree-sitter
```

Ожидаемый вывод:

```
libtree-sitter-python.so (libc6,x86-64) => /usr/local/lib/libtree-sitter-python.so
libtree-sitter.so.0 (libc6,x86-64) => /usr/local/lib/libtree-sitter.so.0
libtree-sitter.so (libc6,x86-64) => /usr/local/lib/libtree-sitter.so
```

## Проверка ABI

```bash
cat > /tmp/ts_abi.c <<'EOF'
#include <stdio.h>
#include <tree_sitter/api.h>
extern const TSLanguage *tree_sitter_python(void);
int main() {
    const TSLanguage *lang = tree_sitter_python();
    printf("grammar ABI: %u\n", ts_language_version(lang));
    printf("core   ABI: %u\n", TREE_SITTER_LANGUAGE_VERSION);
    return 0;
}
EOF

gcc /tmp/ts_abi.c -o /tmp/ts_abi -L/usr/local/lib -ltree-sitter -ltree-sitter-python
/tmp/ts_abi
```

Должно вывести **оба ABI = 14**. Если grammar > core — пересоберите грамматику
из v0.21.0.

## Smoke-тест

```bash
cat > /tmp/ts_test.c <<'EOF'
#include <stdio.h>
#include <string.h>
#include <tree_sitter/api.h>
extern const TSLanguage *tree_sitter_python(void);

int main() {
    TSParser *p = ts_parser_new();
    ts_parser_set_language(p, tree_sitter_python());
    const char *code = "def f(n):\n    return f(n-1)\n";
    TSTree *t = ts_parser_parse_string(p, NULL, code, strlen(code));
    TSNode root = ts_tree_root_node(t);
    printf("root type: %s\n", ts_node_type(root));
    printf("child count: %u\n", ts_node_child_count(root));
    ts_tree_delete(t);
    ts_parser_delete(p);
    return 0;
}
EOF

gcc /tmp/ts_test.c -o /tmp/ts_test -L/usr/local/lib -ltree-sitter -ltree-sitter-python
/tmp/ts_test
```

Ожидаемый вывод:
```
root type: module
child count: 1
```

## Где ищутся библиотеки

`AlgoVis.Parser/Native/LibLoader.cs` ищет `.so` в порядке:

1. `/usr/local/lib/`
2. `<директория приложения>/native/linux-x64/`
3. `<рабочая директория>/native/linux-x64/`

Если библиотеки в другом месте — поправьте `LibLoader.cs` или создайте симлинк
в `/usr/local/lib/`.

## Docker

Пример `Dockerfile` для контейнера с tree-sitter и .NET 8:

```dockerfile
FROM gitpod/openvscode-server:latest

USER root

RUN apt-get update && apt-get install -y --no-install-recommends \
        curl wget unzip git ca-certificates \
        libicu70 libssl3 build-essential \
    && rm -rf /var/lib/apt/lists/*

# .NET 8
RUN curl -sSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
    && chmod +x /tmp/dotnet-install.sh \
    && /tmp/dotnet-install.sh --channel 8.0 --install-dir /usr/share/dotnet \
    && ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
    && rm /tmp/dotnet-install.sh

ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="$PATH:/usr/share/dotnet"

# tree-sitter core
RUN cd /tmp \
    && git clone --depth 1 --branch v0.22.6 https://github.com/tree-sitter/tree-sitter.git \
    && cd tree-sitter && make && make install \
    && cd /tmp && rm -rf tree-sitter

# tree-sitter-python
RUN cd /tmp \
    && git clone --depth 1 --branch v0.21.0 https://github.com/tree-sitter/tree-sitter-python.git \
    && cd tree-sitter-python \
    && gcc -shared -fPIC -o libtree-sitter-python.so src/parser.c src/scanner.c -Isrc \
    && cp libtree-sitter-python.so /usr/local/lib/ \
    && cd /tmp && rm -rf tree-sitter-python \
    && echo "/usr/local/lib" > /etc/ld.so.conf.d/local.conf \
    && ldconfig

USER 0
```
