#!/bin/bash
# tests/run_all.sh
# Запускает все наборы тестов транспайлеров и интеграционные тесты API.
# Опции: --only=NAME, --skip-dotnet, --verbose, --help

set -u

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

RED='\033[0;31m'; GREEN='\033[0;32m'; CYAN='\033[0;36m'; NC='\033[0m'

ONLY=""
SKIP_DOTNET=false
VERBOSE=false

for arg in "$@"; do
    case "$arg" in
        --only=*)     ONLY="${arg#*=}" ;;
        --skip-dotnet) SKIP_DOTNET=true ;;
        --verbose|-v) VERBOSE=true ;;
        --help|-h)
            cat <<HELP
Usage: $0 [OPTIONS]

Запускает все тесты проекта.

Options:
  --only=NAME       Только один набор: python_smoke, python_advanced,
                    python_real, cpp_smoke, cpp_advanced, cpp_real
  --skip-dotnet     Пропустить интеграционные тесты API (dotnet test)
  --verbose, -v     Показывать полный вывод каждого набора
  --help, -h        Эта справка
HELP
            exit 0
            ;;
    esac
done

# Сбор всех наборов с tests/*/run_all.sh
declare -a SUITES=()
for dir in "$SCRIPT_DIR"/*/; do
    name=$(basename "$dir")
    [ -f "$dir/run_all.sh" ] && SUITES+=("$name")
done
IFS=$'\n' SUITES=($(sort <<<"${SUITES[*]}")); unset IFS

TOTAL_PASSED=0
TOTAL_FAILED=0
TOTAL_SUITES=0
FAILED_SUITES=()

echo "════════════════════════════════════════════════════════════"
echo "  AlgoVis — полный прогон тестов"
echo "════════════════════════════════════════════════════════════"
echo "  Найдено наборов: ${#SUITES[@]}"
echo

for suite in "${SUITES[@]}"; do
    [ -n "$ONLY" ] && [ "$suite" != "$ONLY" ] && continue

    TOTAL_SUITES=$((TOTAL_SUITES + 1))
    echo -e "${CYAN}▶ ${suite}${NC}"

    output=$("$SCRIPT_DIR/$suite/run_all.sh" 2>&1)
    $VERBOSE && echo "$output"

    passed=$(echo "$output" | grep -E "Прошло полностью:" | head -1 | awk '{print $NF}')
    total=$(echo "$output" | grep -E "Всего:" | head -1 | awk '{print $NF}')
    tfail=$(echo "$output" | grep -E "Упало транспайлер:" | head -1 | awk '{print $NF}')
    rfail=$(echo "$output" | grep -E "Упало исполнение:" | head -1 | awk '{print $NF}')

    passed=${passed:-0}; total=${total:-0}
    tfail=${tfail:-0};   rfail=${rfail:-0}
    failed=$((tfail + rfail))

    TOTAL_PASSED=$((TOTAL_PASSED + passed))
    TOTAL_FAILED=$((TOTAL_FAILED + failed))

    if [ "$failed" -eq 0 ]; then
        echo -e "  ${GREEN}✅ $passed / $total${NC}"
    else
        echo -e "  ${RED}❌ $passed / $total (упало: $failed)${NC}"
        FAILED_SUITES+=("$suite")
        if ! $VERBOSE; then
            echo "$output" | grep -E "^❌|^⚠️" | head -10 | sed 's/^/     /'
        fi
    fi
    echo
done

# Интеграционные тесты API
if ! $SKIP_DOTNET && [ -z "$ONLY" ]; then
    echo -e "${CYAN}▶ dotnet test (интеграционные API)${NC}"

    output=$(cd "$ROOT" && dotnet test AlgoVis.Server.Tests/AlgoVis.Server.Tests.csproj --nologo -v q 2>&1)
    passed=$(echo "$output" | grep -oE "Passed:[[:space:]]+[0-9]+" | tail -1 | awk '{print $2}')
    failed=$(echo "$output" | grep -oE "Failed:[[:space:]]+[0-9]+" | tail -1 | awk '{print $2}')
    passed=${passed:-0}; failed=${failed:-0}
    total=$((passed + failed))

    TOTAL_PASSED=$((TOTAL_PASSED + passed))
    TOTAL_FAILED=$((TOTAL_FAILED + failed))

    if [ "$failed" -eq 0 ]; then
        echo -e "  ${GREEN}✅ $passed / $total${NC}"
    else
        echo -e "  ${RED}❌ $passed / $total (упало: $failed)${NC}"
        FAILED_SUITES+=("dotnet test")
        if ! $VERBOSE; then
            echo "$output" | grep -E "\[FAIL\]" | head -10 | sed 's/^/     /'
        fi
    fi
    echo
fi

# Итог
echo "════════════════════════════════════════════════════════════"
echo "  ИТОГО:"
echo "    Наборов:  $TOTAL_SUITES"
echo "    Прошло:   $TOTAL_PASSED"
echo "    Упало:    $TOTAL_FAILED"
echo

if [ "$TOTAL_FAILED" -eq 0 ]; then
    echo -e "  ${GREEN}✅ ВСЁ ЗЕЛЁНОЕ${NC}"
else
    echo -e "  ${RED}❌ ПРОВАЛИВШИЕСЯ:${NC}"
    for s in "${FAILED_SUITES[@]}"; do
        echo "      - $s"
    done
fi
echo "════════════════════════════════════════════════════════════"

exit $TOTAL_FAILED
