#!/bin/bash
set -u

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PROJECT="$ROOT/AlgoVis.Yawa.Cli/AlgoVis.Yawa.Cli.csproj"
OUT_DIR="$SCRIPT_DIR/_out"
mkdir -p "$OUT_DIR"

PASS=0; TRANSPILE_FAIL=0; RUN_FAIL=0; TOTAL=0

echo "══════════════════════════════════════════════════════════"
echo "  C++ Advanced → YAWA tests"
echo "══════════════════════════════════════════════════════════"
echo

dotnet build "$PROJECT" -c Debug --nologo -v q > /dev/null 2>&1

for cpp_path in "$SCRIPT_DIR"/[0-9]*.cpp; do
    cpp=$(basename "$cpp_path")
    TOTAL=$((TOTAL + 1))
    name="${cpp%.cpp}"
    yawa="$OUT_DIR/${name}.yawa.json"

    trans_out=$(dotnet run --project "$PROJECT" --no-build -- from-cpp "$cpp_path" --out="$yawa" 2>&1)
    trans_rc=$?

    if [ $trans_rc -ne 0 ]; then
        echo "❌ $name — ТРАНСПАЙЛЕР"
        echo "$trans_out" | grep -E "Строка|❌" | head -3 | sed 's/^/   /'
        TRANSPILE_FAIL=$((TRANSPILE_FAIL + 1))
        continue
    fi

    run_out=$(dotnet run --project "$PROJECT" --no-build -- run "$yawa" 2>&1)
    run_rc=$?

    if [ $run_rc -ne 0 ]; then
        echo "⚠️  $name — ИСПОЛНЕНИЕ"
        echo "$run_out" | grep -E "RUNTIME|Error" | head -1 | sed 's/^/   /'
        RUN_FAIL=$((RUN_FAIL + 1))
        continue
    fi

    steps=$(echo "$run_out" | grep "total_steps" | awk '{print $2}')
    echo "✅ $name  (steps=$steps)"
    PASS=$((PASS + 1))
done

echo
echo "══════════════════════════════════════════════════════════"
echo "  Итого:"
echo "    Всего:              $TOTAL"
echo "    Прошло полностью:   $PASS"
echo "    Упало транспайлер:  $TRANSPILE_FAIL"
echo "    Упало исполнение:   $RUN_FAIL"
echo "══════════════════════════════════════════════════════════"
