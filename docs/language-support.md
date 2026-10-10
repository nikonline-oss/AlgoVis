# Поддержка языков

Индекс. Детали — на подстраницах.

| Язык | Транспайлер | Наборы тестов | Покрытие |
|---|---|---|---|
| Python | `AlgoVis.Transpiler` (`PythonToYawa`) | `python_smoke` (20), `python_advanced` (30), `python_real` (10) | 60/60 |
| C++ | `AlgoVis.Transpiler.Cpp` (`CppToYawa`) | `cpp_smoke` (20), `cpp_advanced` (30), `cpp_real` (10) | 60/60 |

**Итого:** 120 транспайлерных + 93 интеграционных API = **213 зелёных**.

## Подстраницы

- [`python-support.md`](python-support.md) — Python: что поддерживается, что нет
- [`cpp-support.md`](cpp-support.md) — C++: что поддерживается, что нет

## Общие принципы

1. **Оба транспайлера** эмитят `YawaProgram` (JSON), который интерпретируется `AlgoVis.Yawa`.
2. **Runtime общий** — `Builtins`, `Evaluator`, `Interpreter`, `TraceRecorder` не различают источник.
3. **Визуализация ≠ семантика.** Где C++-контейнер эмулируется проще, чем в реальности (например, `priority_queue` как массив) — это зафиксировано на подстранице.
4. **API:** `/api/yawa/run-python`, `/api/yawa/run-cpp`, `/api/yawa/run-code?language=...` — см. [`api.md`](api.md).
