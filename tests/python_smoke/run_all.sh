#!/bin/bash
# Прогоняет все .py файлы через транспайлер и собирает отчёт.
set -u

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
PROJECT="$ROOT/AlgoVis.Yawa.Cli/AlgoVis.Yawa.Cli.csproj"

echo "═══════════════════════════════════════════════════════════════"
echo "  Smoke test: Python → YAWA → Trace"
echo "  Root:    $ROOT"
echo "  Project: $PROJECT"
echo "═══════════════════════════════════════════════════════════════"
echo

PASS=0
TRANSPILE_FAIL=0
RUN_FAIL=0
TOTAL=0

OUT_DIR="$SCRIPT_DIR/_out"
mkdir -p "$OUT_DIR"

for py_path in "$SCRIPT_DIR"/[0-9]*.py; do
    py=$(basename "$py_path")
    TOTAL=$((TOTAL + 1))
    name="${py%.py}"
    yawa="$OUT_DIR/${name}.yawa.json"
    trace="$OUT_DIR/${name}.trace.json"

    # Шаг 1: транспиляция
    trans_out=$(dotnet run --project "$PROJECT" -- from-python "$py_path" --out="$yawa" 2>&1)
    trans_rc=$?

    if [ $trans_rc -ne 0 ]; then
        echo "❌ $name — ТРАНСПАЙЛЕР"
        echo "$trans_out" | tail -3 | sed 's/^/   /'
        TRANSPILE_FAIL=$((TRANSPILE_FAIL + 1))
        continue
    fi

    # Шаг 2: исполнение
    run_out=$(dotnet run --project "$PROJECT" -- run "$yawa" 2>&1)
    run_rc=$?

    if [ $run_rc -ne 0 ]; then
        echo "⚠️  $name — ИСПОЛНЕНИЕ"
        echo "$run_out" | tail -3 | sed 's/^/   /'
        RUN_FAIL=$((RUN_FAIL + 1))
        continue
    fi

    steps=$(echo "$run_out" | grep "total_steps" | awk '{print $2}')
    cmp=$(echo "$run_out" | grep "comparisons" | head -1 | awk '{print $2}')
    swp=$(echo "$run_out" | grep "swaps" | head -1 | awk '{print $2}')

    echo "✅ $name  (steps=$steps, cmp=$cmp, swaps=$swp)"
    PASS=$((PASS + 1))
done

echo
echo "═══════════════════════════════════════════════════════════════"
echo "  Итого:"
echo "    Всего:              $TOTAL"
echo "    Прошло полностью:   $PASS"
echo "    Упало транспайлер:  $TRANSPILE_FAIL"
echo "    Упало исполнение:   $RUN_FAIL"
echo "═══════════════════════════════════════════════════════════════"