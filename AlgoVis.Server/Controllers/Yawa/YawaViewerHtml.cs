namespace AlgoVis.Server.Controllers.Yawa;

/// <summary>
/// HTML-страница просмотрщика YAWA. Отдаётся из YawaController GET /api/yawa/viewer.
/// </summary>
internal static class YawaViewerHtml
{
    public static readonly string Value = """
<!doctype html>
<html lang="ru">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>AlgoVis — визуализатор алгоритмов</title>
<style>
  :root {
    --bg-0: #070a14;
    --bg-1: #0d1220;
    --bg-2: #141a2b;
    --bg-3: #1c2338;
    --border: #232a42;
    --border-hi: #2f3854;
    --text: #e8ecf4;
    --text-dim: #8b95ad;
    --text-muted: #5a6478;
    --accent: #6366f1;
    --accent-hi: #818cf8;
    --accent-glow: rgba(99, 102, 241, 0.35);
    --pink: #ec4899;
    --cmp: #f59e0b;
    --swap: #10b981;
    --assign: #3b82f6;
    --mark: #ec4899;
    --call: #a855f7;
    --danger: #ef4444;
    --mono: ui-monospace, 'SF Mono', Menlo, 'JetBrains Mono', monospace;
    --sans: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  }
  * { box-sizing: border-box; }
  html, body {
    margin: 0; padding: 0;
    background: radial-gradient(1200px 600px at 20% -10%, #182042 0%, transparent 60%),
                radial-gradient(900px 500px at 100% 100%, #1a1230 0%, transparent 55%),
                var(--bg-0);
    color: var(--text);
    font-family: var(--sans);
    min-height: 100vh;
    font-size: 14px; line-height: 1.5;
  }
  .app { max-width: 1500px; margin: 0 auto; padding: 24px 28px 60px; }

  header {
    display: flex; align-items: center; justify-content: space-between;
    margin-bottom: 24px; padding-bottom: 18px;
    border-bottom: 1px solid var(--border);
  }
  .brand { display: flex; align-items: center; gap: 14px; }
  .logo {
    width: 40px; height: 40px; border-radius: 10px;
    background: linear-gradient(135deg, var(--accent) 0%, var(--pink) 100%);
    display: flex; align-items: center; justify-content: center;
    font-size: 22px;
    box-shadow: 0 6px 20px -6px var(--accent-glow);
  }
  .brand h1 { margin: 0; font-size: 20px; font-weight: 600; letter-spacing: -0.01em; }
  .brand .sub { margin: 2px 0 0; font-size: 12px; color: var(--text-dim); }
  .header-meta { display: flex; gap: 8px; align-items: center; }
  .pill {
    padding: 4px 10px; background: var(--bg-2);
    border: 1px solid var(--border); border-radius: 999px;
    font-family: var(--mono); font-size: 12px; color: var(--text-dim);
  }

  .layout { display: grid; grid-template-columns: 320px 1fr; gap: 20px; align-items: start; }
  @media (max-width: 1100px) { .layout { grid-template-columns: 1fr; } }

  .card {
    background: linear-gradient(180deg, var(--bg-2) 0%, var(--bg-1) 100%);
    border: 1px solid var(--border);
    border-radius: 10px;
    padding: 16px;
    box-shadow: 0 4px 16px -8px rgba(0,0,0,0.5);
    margin-bottom: 16px;
  }
  .card:last-child { margin-bottom: 0; }
  .card-title {
    font-size: 11px; font-weight: 600; letter-spacing: 0.08em;
    text-transform: uppercase; color: var(--text-muted);
    margin: 0 0 12px;
    display: flex; align-items: center; justify-content: space-between;
  }
  .card-title .badge {
    font-size: 10px; padding: 1px 6px; border-radius: 3px;
    background: var(--bg-3); color: var(--text-dim); font-weight: 500;
    text-transform: none; letter-spacing: 0;
  }

  /* ─── Tabs ─── */
  .tabs {
    display: flex; gap: 4px; margin-bottom: 12px;
    background: var(--bg-1); padding: 3px; border-radius: 6px;
  }
  .tab {
    flex: 1; padding: 6px 10px; border-radius: 4px;
    background: transparent; border: none;
    color: var(--text-dim); font-family: inherit; font-size: 12px;
    cursor: pointer; transition: all 0.15s ease;
  }
  .tab:hover { color: var(--text); }
  .tab.active {
    background: var(--bg-3); color: var(--text);
    box-shadow: 0 2px 6px -2px rgba(0,0,0,0.4);
  }

  /* ─── Samples ─── */
  .samples { display: flex; flex-direction: column; gap: 6px; }
  .sample-btn {
    display: flex; align-items: center; gap: 10px;
    padding: 10px 12px; background: transparent;
    border: 1px solid transparent; border-radius: 6px;
    color: var(--text); font-family: inherit; font-size: 13px;
    text-align: left; cursor: pointer; width: 100%;
    transition: all 0.15s ease;
  }
  .sample-btn .icon {
    width: 26px; height: 26px; flex-shrink: 0;
    border-radius: 6px; background: var(--bg-3);
    display: flex; align-items: center; justify-content: center;
    font-size: 14px;
  }
  .sample-btn:hover { background: var(--bg-3); border-color: var(--border-hi); }
  .sample-btn.active {
    background: linear-gradient(135deg, rgba(99,102,241,0.18), rgba(236,72,153,0.10));
    border-color: var(--accent);
    box-shadow: 0 0 0 1px rgba(99,102,241,0.3), 0 4px 20px -8px var(--accent-glow);
  }
  .sample-btn.active .icon { background: linear-gradient(135deg, var(--accent), var(--pink)); }

  /* ─── Code editor ─── */
  .code-area {
    width: 100%; min-height: 320px;
    padding: 12px;
    background: var(--bg-0);
    border: 1px solid var(--border);
    border-radius: 8px;
    color: var(--text);
    font-family: var(--mono); font-size: 12.5px; line-height: 1.55;
    resize: vertical;
    tab-size: 4;
    outline: none;
    transition: border 0.15s ease;
  }
  .code-area:focus { border-color: var(--accent); box-shadow: 0 0 0 3px var(--accent-glow); }

  .code-actions {
    display: flex; gap: 8px; margin-top: 10px; align-items: center;
  }

  .btn {
    display: inline-flex; align-items: center; justify-content: center; gap: 6px;
    height: 36px; padding: 0 16px;
    background: var(--bg-2); border: 1px solid var(--border);
    border-radius: 6px; color: var(--text);
    font-family: inherit; font-size: 13px; font-weight: 500;
    cursor: pointer; transition: all 0.15s ease;
    user-select: none;
  }
  .btn:hover:not(:disabled) {
    background: var(--bg-3); border-color: var(--border-hi);
    transform: translateY(-1px);
  }
  .btn:disabled { opacity: 0.4; cursor: not-allowed; }
  .btn.primary {
    background: linear-gradient(135deg, var(--accent), #4f46e5);
    border-color: var(--accent);
    box-shadow: 0 4px 14px -4px var(--accent-glow);
  }
  .btn.primary:hover:not(:disabled) {
    background: linear-gradient(135deg, var(--accent-hi), #6366f1);
  }
  .btn .icon { font-size: 15px; line-height: 1; }

  /* ─── Error banner ─── */
  .error-banner {
    display: none;
    margin-top: 10px;
    padding: 10px 14px;
    background: linear-gradient(90deg, rgba(239,68,68,0.18), rgba(239,68,68,0.05));
    border-left: 3px solid var(--danger);
    border-radius: 6px;
    font-family: var(--mono); font-size: 12.5px;
    color: #fecaca;
    white-space: pre-wrap; word-break: break-word;
  }
  .error-banner.show { display: block; }
  .error-banner .head {
    font-family: var(--sans); font-weight: 600;
    color: var(--danger); margin-bottom: 4px;
    font-size: 12px; text-transform: uppercase; letter-spacing: 0.06em;
  }

  /* ─── Stats ─── */
  .stats-grid {
    display: grid; grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
    gap: 10px; margin-bottom: 20px;
  }
  .stat {
    background: linear-gradient(180deg, var(--bg-3) 0%, var(--bg-2) 100%);
    border: 1px solid var(--border); border-radius: 10px;
    padding: 12px 14px; position: relative; overflow: hidden;
  }
  .stat::before {
    content: ''; position: absolute; top: 0; left: 0; right: 0; height: 2px;
    background: linear-gradient(90deg, var(--accent), var(--pink)); opacity: 0.7;
  }
  .stat .label {
    font-size: 11px; color: var(--text-muted);
    text-transform: uppercase; letter-spacing: 0.06em; margin-bottom: 4px;
  }
  .stat .value {
    font-size: 24px; font-weight: 700; font-family: var(--mono);
    letter-spacing: -0.02em;
    background: linear-gradient(135deg, var(--text) 0%, var(--text-dim) 100%);
    -webkit-background-clip: text; -webkit-text-fill-color: transparent;
    background-clip: text;
  }

  /* ─── Workspace ─── */
  .workspace {
    display: grid; grid-template-columns: minmax(0, 1fr) 280px;
    gap: 16px; margin-bottom: 16px;
  }
  @media (max-width: 1100px) { .workspace { grid-template-columns: 1fr; } }

  .array-wrap {
    padding: 24px 16px;
    background:
      radial-gradient(circle at 50% 0%, rgba(99,102,241,0.06), transparent 70%),
      var(--bg-1);
    border: 1px solid var(--border); border-radius: 10px;
    min-height: 140px;
    display: flex; align-items: center; justify-content: center;
  }
  .array { display: flex; flex-wrap: wrap; gap: 8px; justify-content: center; }
  .cell {
    position: relative;
    min-width: 52px; height: 52px; padding: 0 12px;
    display: flex; align-items: center; justify-content: center;
    background: linear-gradient(180deg, var(--bg-3) 0%, var(--bg-2) 100%);
    border: 2px solid var(--border-hi); border-radius: 6px;
    font-family: var(--mono); font-size: 18px; font-weight: 600;
    color: var(--text);
    transition: all 0.25s cubic-bezier(0.4, 0, 0.2, 1);
    box-shadow: 0 2px 8px -2px rgba(0,0,0,0.4);
  }
  .cell .idx {
    position: absolute; top: -8px; left: 50%; transform: translateX(-50%);
    font-size: 9px; color: var(--text-muted);
    background: var(--bg-0); padding: 1px 5px; border-radius: 3px;
    font-weight: 400;
  }
  .cell.hl-cmp {
    border-color: var(--cmp);
    background: linear-gradient(180deg, rgba(245,158,11,0.20), rgba(245,158,11,0.08));
    box-shadow: 0 0 0 3px rgba(245,158,11,0.18), 0 4px 16px -4px rgba(245,158,11,0.5);
    transform: translateY(-3px) scale(1.06);
    color: #fde68a;
  }
  .cell.hl-swap {
    border-color: var(--swap);
    background: linear-gradient(180deg, rgba(16,185,129,0.20), rgba(16,185,129,0.08));
    box-shadow: 0 0 0 3px rgba(16,185,129,0.18), 0 4px 16px -4px rgba(16,185,129,0.5);
    transform: translateY(-3px) scale(1.06);
    color: #a7f3d0;
  }
  .cell.hl-mark {
    border-color: var(--mark);
    background: linear-gradient(180deg, rgba(236,72,153,0.20), rgba(236,72,153,0.08));
    box-shadow: 0 0 0 3px rgba(236,72,153,0.18), 0 4px 16px -4px rgba(236,72,153,0.5);
    transform: translateY(-3px) scale(1.06);
    color: #fbcfe8;
  }
  .empty-array { color: var(--text-muted); font-style: italic; font-size: 13px; }

  /* ─── Variables ─── */
  .vars-card { padding: 14px; min-height: 140px; }
  .vars {
    display: flex; flex-direction: column; gap: 4px;
    max-height: 400px; overflow-y: auto;
  }
  .vars::-webkit-scrollbar { width: 8px; }
  .vars::-webkit-scrollbar-thumb { background: var(--bg-3); border-radius: 4px; }
  .var-row {
    display: grid;
    grid-template-columns: 1fr auto;
    align-items: center; gap: 10px;
    padding: 6px 10px;
    background: var(--bg-1);
    border: 1px solid transparent;
    border-radius: 6px;
    font-family: var(--mono); font-size: 13px;
    transition: all 0.2s ease;
  }
  .var-row .name {
    color: var(--text-dim);
    overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
  }
  .var-row .name .type {
    color: var(--text-muted);
    font-size: 10px; margin-left: 6px; padding: 1px 5px;
    background: var(--bg-3); border-radius: 3px;
  }
  .var-row .value {
    color: var(--text); font-weight: 600;
    text-align: right; overflow: hidden; text-overflow: ellipsis;
    white-space: nowrap; max-width: 140px;
  }
  .var-row.changed {
    background: linear-gradient(90deg, rgba(99,102,241,0.14), transparent);
    border-color: rgba(99,102,241,0.4);
    animation: pulse-var 0.7s ease;
  }
  .var-row.array-var .value { color: #60a5fa; }
  .var-row.tree-var .value { color: #a855f7; }
  .var-row.object-var .value { color: #fbbf24; }
  @keyframes pulse-var {
    0%   { box-shadow: 0 0 0 0 rgba(99,102,241,0.5); }
    70%  { box-shadow: 0 0 0 8px rgba(99,102,241,0); }
    100% { box-shadow: 0 0 0 0 rgba(99,102,241,0); }
  }
  .vars-empty {
    color: var(--text-muted); font-size: 12px;
    font-style: italic; padding: 8px;
  }

  /* ─── Timeline ─── */
  .timeline {
    position: relative; height: 6px; background: var(--bg-3);
    border-radius: 3px; margin: 14px 0; overflow: hidden; cursor: pointer;
  }
  .timeline-fill {
    height: 100%;
    background: linear-gradient(90deg, var(--accent), var(--pink));
    border-radius: 3px;
    transition: width 0.15s ease;
    box-shadow: 0 0 8px var(--accent-glow);
  }

  /* ─── Annotation ─── */
  .annotation {
    min-height: 32px; padding: 6px 14px;
    background: linear-gradient(90deg, rgba(99,102,241,0.12), transparent);
    border-left: 3px solid var(--accent);
    border-radius: 6px; margin-bottom: 14px;
    font-size: 13px;
    display: flex; align-items: center; gap: 8px;
  }
  .annotation:empty { display: none; }
  .annotation .badge {
    font-size: 10px; font-weight: 700; letter-spacing: 0.08em;
    text-transform: uppercase;
    padding: 2px 8px; background: var(--accent); color: white; border-radius: 3px;
  }

  /* ─── Controls ─── */
  .controls {
    display: flex; gap: 6px; align-items: center;
    flex-wrap: wrap; margin-bottom: 16px;
  }
  .ctrl {
    display: inline-flex; align-items: center; justify-content: center; gap: 6px;
    min-width: 40px; height: 38px; padding: 0 12px;
    background: var(--bg-2); border: 1px solid var(--border);
    border-radius: 6px; color: var(--text);
    font-family: inherit; font-size: 13px; font-weight: 500;
    cursor: pointer; transition: all 0.15s ease;
    user-select: none;
  }
  .ctrl:hover:not(:disabled) {
    background: var(--bg-3); border-color: var(--border-hi);
    transform: translateY(-1px);
  }
  .ctrl:disabled { opacity: 0.4; cursor: not-allowed; }
  .ctrl.primary {
    background: linear-gradient(135deg, var(--accent), #4f46e5);
    border-color: var(--accent);
    box-shadow: 0 4px 14px -4px var(--accent-glow);
    min-width: 130px;
  }
  .ctrl.primary:hover:not(:disabled) {
    background: linear-gradient(135deg, var(--accent-hi), #6366f1);
  }
  .ctrl .icon { font-size: 15px; line-height: 1; }
  .pos {
    display: inline-flex; align-items: center; height: 38px; padding: 0 14px;
    background: var(--bg-1); border: 1px solid var(--border);
    border-radius: 6px; font-family: var(--mono);
    font-size: 13px; color: var(--text-dim); margin: 0 4px;
  }
  .pos b { color: var(--text); font-weight: 600; }

  /* ─── Steps ─── */
  .steps-card { padding: 0; overflow: hidden; }
  .steps-head {
    padding: 14px 16px; border-bottom: 1px solid var(--border);
    display: flex; align-items: center; justify-content: space-between;
  }
  .steps-head .title { font-size: 13px; font-weight: 600; }
  .steps-head .count { font-size: 11px; color: var(--text-muted); font-family: var(--mono); }
  .steps { max-height: 420px; overflow-y: auto; padding: 6px; }
  .steps::-webkit-scrollbar { width: 10px; }
  .steps::-webkit-scrollbar-thumb { background: var(--bg-3); border-radius: 5px; }
  .steps::-webkit-scrollbar-thumb:hover { background: var(--border-hi); }
  .step {
    display: grid; grid-template-columns: 46px 90px 1fr;
    gap: 12px; align-items: center;
    padding: 7px 10px; border-radius: 6px;
    font-family: var(--mono); font-size: 12px;
    cursor: pointer; transition: background 0.1s ease;
    border-left: 2px solid transparent;
  }
  .step:hover { background: var(--bg-3); }
  .step.current {
    background: linear-gradient(90deg, rgba(99,102,241,0.14), transparent);
    border-left-color: var(--accent);
  }
  .step .n { color: var(--text-muted); text-align: right; font-size: 11px; }
  .step .k {
    font-size: 10px; font-weight: 700; letter-spacing: 0.05em;
    text-transform: uppercase;
    padding: 2px 6px; border-radius: 3px; text-align: center;
    background: var(--bg-3); color: var(--text-dim);
  }
  .step[data-kind="compare"] .k { background: rgba(245,158,11,0.18); color: var(--cmp); }
  .step[data-kind="swap"]    .k { background: rgba(16,185,129,0.18); color: var(--swap); }
  .step[data-kind="assign"]  .k { background: rgba(59,130,246,0.18); color: var(--assign); }
  .step[data-kind="declare"] .k { background: rgba(59,130,246,0.18); color: var(--assign); }
  .step[data-kind="call"]    .k { background: rgba(168,85,247,0.18); color: var(--call); }
  .step[data-kind="mark"]    .k { background: rgba(236,72,153,0.18); color: var(--mark); }
  .step[data-kind="snapshot"] .k { background: rgba(139,149,173,0.15); color: var(--text-dim); }
  .step .d {
    color: var(--text-dim);
    overflow: hidden; text-overflow: ellipsis; white-space: nowrap;
  }
  .step .d .val { color: var(--text); font-weight: 500; }
  .step .d .ann { color: var(--cmp); font-family: var(--sans); font-style: italic; }
  .step .d .arrow { color: var(--text-muted); margin: 0 2px; }

  /* ─── Empty state ─── */
  .empty { text-align: center; padding: 60px 20px; color: var(--text-muted); }
  .empty .icon { font-size: 48px; margin-bottom: 12px; opacity: 0.6; }
  .empty .title { font-size: 15px; color: var(--text-dim); margin-bottom: 4px; }

  .loading {
    display: inline-block; width: 16px; height: 16px;
    border: 2px solid var(--border-hi); border-top-color: var(--accent);
    border-radius: 50%; animation: spin 0.7s linear infinite;
  }
  @keyframes spin { to { transform: rotate(360deg); } }

  .hint {
    display: flex; gap: 12px; align-items: center;
    font-size: 11px; color: var(--text-muted);
    margin-left: auto; flex-wrap: wrap;
  }
  .hint kbd {
    display: inline-block; padding: 1px 6px;
    background: var(--bg-3); border: 1px solid var(--border-hi);
    border-bottom-width: 2px; border-radius: 3px;
    font-family: var(--mono); font-size: 10px;
  }
</style>
</head>
<body>
<div class="app">

  <header>
    <div class="brand">
      <div class="logo">⚡</div>
      <div>
        <h1>AlgoVis</h1>
        <p class="sub">Пошаговый визуализатор алгоритмов</p>
      </div>
    </div>
    <div class="header-meta" id="header-meta">
      <span class="pill">yawa 1.0</span>
    </div>
  </header>

  <div class="layout">
    <aside>
      <div class="card">
        <div class="tabs">
          <button class="tab active" data-tab="samples">Примеры</button>
          <button class="tab" data-tab="python">Python</button>
        </div>

        <div id="tab-samples">
          <div class="samples" id="samples">
            <div style="color: var(--text-muted); font-size: 12px; padding: 4px;">
              <span class="loading"></span> загрузка...
            </div>
          </div>
        </div>

        <div id="tab-python" style="display: none;">
          <textarea id="code-input" class="code-area" spellcheck="false">def bubble_sort(A):
    n = len(A)
    for i in range(n):
        for j in range(n - i - 1):
            if A[j] > A[j + 1]:
                A[j], A[j + 1] = A[j + 1], A[j]
                annotate("поменяли местами")
    return A

def main():
    A = [5, 2, 8, 1, 9, 3, 7, 4]
    bubble_sort(A)</textarea>

          <div class="error-banner" id="error-banner">
            <div class="head">Ошибка транспайлера</div>
            <div class="body"></div>
          </div>

          <div class="code-actions">
            <button class="btn primary" id="btn-run-python">
              <span class="icon">▶</span> Запустить
            </button>
            <button class="btn" id="btn-clear-code" title="Очистить">
              <span class="icon">✕</span>
            </button>
          </div>
        </div>
      </div>
    </aside>

    <main id="main">
      <div class="empty" id="empty-state">
        <div class="icon">▶</div>
        <div class="title">Выберите пример или запустите Python</div>
        <div style="font-size: 12px;">загрузите готовый sample или напишите код слева</div>
      </div>

      <div id="content" style="display: none;">
        <div class="stats-grid" id="stats"></div>

        <div class="workspace">
          <div class="card" style="padding: 16px; margin-bottom: 0;">
            <h3 class="card-title" id="array-title">Структура данных</h3>
            <div class="array-wrap">
              <div class="array" id="arr"></div>
            </div>
          </div>

          <div class="card vars-card" style="margin-bottom: 0;">
            <h3 class="card-title">Переменные</h3>
            <div class="vars" id="vars"></div>
          </div>
        </div>

        <div class="card" style="padding: 16px;">
          <div class="annotation" id="annotation"></div>

          <div class="timeline" id="timeline">
            <div class="timeline-fill" id="timeline-fill" style="width: 0%"></div>
          </div>

          <div class="controls">
            <button class="ctrl" id="btn-first" title="В начало">⟪</button>
            <button class="ctrl" id="btn-prev"  title="-10 шагов">−10</button>
            <button class="ctrl" id="btn-prev1" title="Назад">‹</button>
            <span class="pos" id="pos"><b>—</b> / —</span>
            <button class="ctrl" id="btn-next1" title="Вперёд">›</button>
            <button class="ctrl" id="btn-next"  title="+10 шагов">+10</button>
            <button class="ctrl" id="btn-last"  title="В конец">⟫</button>
            <button class="ctrl primary" id="btn-play" style="margin-left: auto;">
              <span class="icon">▶</span><span id="play-label">Воспроизвести</span>
            </button>
          </div>

          <div class="hint">
            <span><kbd>←</kbd> <kbd>→</kbd> шаг</span>
            <span><kbd>Shift</kbd>+<kbd>←</kbd><kbd>→</kbd> по 10</span>
            <span><kbd>Space</kbd> play/pause</span>
          </div>
        </div>

        <div class="card steps-card" style="margin-top: 16px;">
          <div class="steps-head">
            <span class="title">История шагов</span>
            <span class="count" id="steps-count">—</span>
          </div>
          <div class="steps" id="steps"></div>
        </div>
      </div>
    </main>
  </div>

</div>

<script>
let session = null;
let snapshots = [];
let currentStep = 0;
let isPlaying = false;
let playTimer = null;
let lastVars = {};
let lastArray = null;

const $ = (id) => document.getElementById(id);

// ─── Helpers ────────────────────────────────────────────
function fmt(x) {
  if (x === null || x === undefined) return 'null';
  if (typeof x === 'object') return JSON.stringify(x);
  return String(x);
}
function esc(s) {
  return String(s).replace(/[&<>"']/g, c =>
    ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}
function isScalar(v) {
  return v === null || v === undefined ||
         typeof v === 'number' || typeof v === 'string' || typeof v === 'boolean';
}
function typeOf(v) {
  if (v === null || v === undefined) return 'null';
  if (typeof v === 'number') return Number.isInteger(v) ? 'int' : 'float';
  if (typeof v === 'string') return 'string';
  if (typeof v === 'boolean') return 'bool';
  if (Array.isArray(v)) return 'array';
  if (typeof v === 'object') {
    if ('value' in v && ('left' in v || 'right' in v)) return 'tree_node';
    return 'object';
  }
  return '?';
}

// ─── Tabs ───────────────────────────────────────────────
document.querySelectorAll('.tab').forEach(t => {
  t.onclick = () => {
    document.querySelectorAll('.tab').forEach(x => x.classList.remove('active'));
    t.classList.add('active');
    const which = t.dataset.tab;
    $('tab-samples').style.display = which === 'samples' ? 'block' : 'none';
    $('tab-python').style.display  = which === 'python'  ? 'block' : 'none';
    $('error-banner').classList.remove('show');
  };
});

// ─── Load samples ───────────────────────────────────────
async function loadSamples() {
  try {
    const r = await fetch('/api/yawa/samples');
    const list = await r.json();
    const box = $('samples');
    if (!list.length) {
      box.innerHTML = '<div style="color:var(--text-muted);font-size:12px;">пусто</div>';
      return;
    }
    box.innerHTML = '';
    for (const s of list) {
      const b = document.createElement('button');
      b.className = 'sample-btn';
      b.dataset.name = s.name;
      b.innerHTML = '<span class="icon">📄</span><span class="name">' +
                    esc(s.name.replace('.yawa.json','')) + '</span>';
      b.onclick = () => runSample(s.name, b);
      box.appendChild(b);
    }
  } catch (e) {
    $('samples').innerHTML = '<div style="color:#ef4444;font-size:12px;">' + esc(e.message) + '</div>';
  }
}

// ─── Run sample ─────────────────────────────────────────
async function runSample(name, btn) {
  document.querySelectorAll('.sample-btn').forEach(b => b.classList.remove('active'));
  if (btn) btn.classList.add('active');

  showLoading();

  try {
    const r = await fetch('/api/yawa/run-sample?name=' + encodeURIComponent(name), { method: 'POST' });
    const data = await r.json();
    if (data.error) { alert('Ошибка: ' + data.error); return; }
    loadSession(data);
  } catch (e) {
    showError('Ошибка: ' + e.message);
  }
}

// ─── Run Python ─────────────────────────────────────────
async function runPython() {
  const code = $('code-input').value;
  if (!code.trim()) return;

  showLoading();
  $('error-banner').classList.remove('show');
  document.querySelectorAll('.sample-btn').forEach(b => b.classList.remove('active'));

  try {
    const r = await fetch('/api/yawa/run-python', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ code })
    });
    const data = await r.json();

    if (data.error) {
      const parts = [];
      if (data.line !== undefined && data.line > 0) parts.push('Строка ' + (data.line + 1) + ':' + data.column);
      if (data.kind === 'transpiler') parts.push('транспайлер');
      else if (data.kind === 'runtime') parts.push('исполнение');
      const head = parts.length ? parts.join(' · ') : 'Ошибка';
      $('error-banner').querySelector('.head').textContent = head;
      $('error-banner').querySelector('.body').textContent = data.error;
      $('error-banner').classList.add('show');
      $('empty-state').style.display = 'block';
      $('empty-state').innerHTML = '<div class="icon">⚠</div><div class="title">Не удалось выполнить</div>' +
                                   '<div style="font-size:12px;">исправьте код и попробуйте снова</div>';
      $('content').style.display = 'none';
      return;
    }

    loadSession(data);
  } catch (e) {
    showError('Сеть: ' + e.message);
  }
}

// ─── Common ─────────────────────────────────────────────
function showLoading() {
  $('empty-state').innerHTML = '<div class="loading"></div>';
  $('empty-state').style.display = 'block';
  $('content').style.display = 'none';
}
function showError(msg) {
  $('empty-state').innerHTML = '<div class="icon">⚠</div><div class="title">Ошибка</div>' +
                               '<div style="font-size:12px;">' + esc(msg) + '</div>';
  $('empty-state').style.display = 'block';
  $('content').style.display = 'none';
}
function loadSession(data) {
  session = data;
  currentStep = 0;
  lastVars = {};
  lastArray = null;
  snapshots = session.steps
    .map((s, i) => s.snapshot ? i : -1)
    .filter(i => i >= 0);

  $('header-meta').innerHTML =
    '<span class="pill">yawa 1.0</span>' +
    '<span class="pill">' + esc(session.metadata?.name || '—') + '</span>';

  $('empty-state').style.display = 'none';
  $('content').style.display = 'block';

  renderStats();
  renderStepsList();
  $('steps-count').textContent = session.steps.length + ' шт.';

  const firstSnap = session.steps.findIndex(s => s.snapshot);
  renderStep(firstSnap >= 0 ? firstSnap : 0);
}

// ─── Stats ──────────────────────────────────────────────
function renderStats() {
  const s = session.statistics;
  const items = [
    ['Шагов',     s.total_steps],
    ['Сравнений', s.comparisons],
    ['Обменов',   s.swaps],
    ['Обращений', s.memory_accesses],
  ];
  if (s.user_counters) for (const [k, v] of Object.entries(s.user_counters)) items.push([k, v]);
  if (s.structure_sizes) for (const [k, v] of Object.entries(s.structure_sizes)) items.push(['размер ' + k, v]);

  $('stats').innerHTML = items.map(([k, v]) =>
    '<div class="stat"><div class="label">' + esc(k) + '</div><div class="value">' +
    esc(v) + '</div></div>').join('');
}

// ─── Steps list ─────────────────────────────────────────
function renderStepsList() {
  const box = $('steps');
  box.innerHTML = session.steps.map((s, i) => {
    let d = '';
    if (s.diff && s.diff.length) {
      d = s.diff.filter(x => !String(x.target).startsWith('compare.'))
                  .map(x =>
        esc(x.target) + '<span class="arrow">=</span><span class="val">' +
        esc(fmt(x.new)) + '</span>').join(' ');
    }
    if (!d && s.annotation) d = '<span class="ann">' + esc(s.annotation) + '</span>';
    if (!d && s.snapshot)   d = '<span style="color:var(--text-muted);">snapshot</span>';
    if (!d) d = '&nbsp;';

    return '<div class="step" data-idx="' + i + '" data-kind="' + s.kind + '">' +
             '<span class="n">#' + s.n + '</span>' +
             '<span class="k">' + s.kind + '</span>' +
             '<span class="d">' + d + '</span>' +
           '</div>';
  }).join('');

  box.querySelectorAll('.step').forEach(el => {
    el.onclick = () => renderStep(+el.dataset.idx);
  });
}

// ─── State reconstruction ───────────────────────────────
function stateAt(stepIdx) {
  let snapIdx = -1;
  for (const i of snapshots) if (i <= stepIdx) snapIdx = i;

  let state = {};
  if (snapIdx >= 0) state = JSON.parse(JSON.stringify(session.steps[snapIdx].snapshot));

  for (let i = snapIdx + 1; i <= stepIdx; i++) {
    const st = session.steps[i];
    if (!st.diff) continue;
    for (const d of st.diff) {
      const t = String(d.target);
      if (t.startsWith('compare.') || t.startsWith('__')) continue;

      let m = t.match(/^([A-Za-z_]\w*)\[(\d+)\]$/);
      if (m) {
        const arr = state[m[1]];
        if (Array.isArray(arr)) arr[+m[2]] = d.new;
        continue;
      }

      m = t.match(/^([A-Za-z_]\w*)\.(\w+)$/);
      if (m && m[1] !== 'object') {
        const obj = state[m[1]];
        if (obj && typeof obj === 'object') obj[m[2]] = d.new;
        continue;
      }

      if (/^[A-Za-z_]\w*$/.test(t)) state[t] = d.new;
    }
  }
  return state;
}

// ─── Render step ────────────────────────────────────────
function renderStep(idx) {
  if (!session) return;
  idx = Math.max(0, Math.min(session.steps.length - 1, idx));
  currentStep = idx;

  const st = session.steps[idx];

  document.querySelectorAll('.step').forEach(el =>
    el.classList.toggle('current', +el.dataset.idx === idx));
  const activeEl = document.querySelector('.step[data-idx="' + idx + '"]');
  if (activeEl) activeEl.scrollIntoView({ block: 'nearest', behavior: 'smooth' });

  $('pos').innerHTML = '<b>' + idx + '</b> / ' + (session.steps.length - 1);
  $('timeline-fill').style.width = (100 * idx / Math.max(1, session.steps.length - 1)) + '%';

  const ann = st.annotation || '';
  $('annotation').innerHTML = ann
    ? '<span class="badge">' + esc(st.kind) + '</span><span>' + esc(ann) + '</span>'
    : (st.kind ? '<span class="badge">' + esc(st.kind) + '</span><span style="color:var(--text-muted);font-style:italic;">…</span>' : '');

  const state = stateAt(idx);
  const changed = new Set();
  if (st.diff) for (const d of st.diff) {
    const t = String(d.target);
    const m = t.match(/^([A-Za-z_]\w*)(\[|\.|$)/);
    if (m) changed.add(m[1]);
  }

  // Массив
  let arrName = null;
  if (Array.isArray(state.A)) arrName = 'A';
  else {
    for (const [k, v] of Object.entries(state)) {
      if (Array.isArray(v)) { arrName = k; break; }
    }
  }
  $('array-title').textContent = arrName ? ('Массив ' + arrName) : 'Структура данных';

  const arrBox = $('arr');
  if (arrName) {
    const A = state[arrName];
    const hl = new Set(st.highlight || []);
    const cls = hlClass(st.kind);

    arrBox.innerHTML = A.map((v, i) => {
      const key = arrName + '[' + i + ']';
      const isHl = hl.has(key);
      return '<div class="cell ' + (isHl ? cls : '') + '">' +
                '<span class="idx">' + i + '</span>' +
                esc(fmt(v)) +
             '</div>';
    }).join('');
    lastArray = A.slice();
  } else {
    arrBox.innerHTML = '<div class="empty-array">нет массива на этом шаге</div>';
    lastArray = null;
  }

  // Переменные — берём из step.vars
  const varsFromStep = st.vars || {};
  renderVars(varsFromStep, changed);
  lastVars = Object.fromEntries(
    Object.entries(varsFromStep).filter(([, v]) => isScalar(v))
  );
}

function hlClass(kind) {
  switch (kind) {
    case 'compare': return 'hl-cmp';
    case 'swap':    return 'hl-swap';
    case 'mark':    return 'hl-mark';
    case 'assign':  return 'hl-mark';
    default:        return 'hl-cmp';
  }
}

function renderVars(vars, changed) {
  const entries = Object.entries(vars)
    .filter(([k]) => !k.startsWith('__'))
    .sort((a, b) => {
      const aScalar = isScalar(a[1]) ? 0 : 1;
      const bScalar = isScalar(b[1]) ? 0 : 1;
      if (aScalar !== bScalar) return aScalar - bScalar;
      return a[0].localeCompare(b[0]);
    });

  const box = $('vars');
  if (!entries.length) {
    box.innerHTML = '<div class="vars-empty">нет переменных на этом шаге</div>';
    return;
  }

  box.innerHTML = entries.map(([name, value]) => {
    let t, displayValue;
    if (isScalar(value)) {
      t = typeOf(value);
      displayValue = fmt(value);
    } else if (value && typeof value === 'object' && value.__type) {
      t = value.__type;
      if (value.__type.startsWith('array')) {
        if (value.items) displayValue = '[' + value.items.map(x => fmt(x)).join(', ') + ']';
        else displayValue = 'array[' + (value.length ?? '?') + ']';
      } else if (value.__type === 'tree') {
        displayValue = 'tree(root=' + fmt(value.root) + ', size=' + value.size + ')';
      } else if (value.__type === 'tree_node') {
        displayValue = 'node(' + fmt(value.value) + ')';
      } else if (value.__type === 'graph') {
        displayValue = 'graph(' + value.nodes + 'N, ' + value.edges + 'E)';
      } else if (value.fields !== undefined) {
        displayValue = value.__type + '{' + value.fields + '}';
      } else {
        displayValue = value.__type;
      }
    } else {
      t = typeOf(value);
      displayValue = fmt(value);
    }

    const cls = isScalar(value) ? '' :
                t.startsWith('array') ? 'array-var' :
                t === 'tree' || t === 'tree_node' ? 'tree-var' :
                'object-var';

    const reallyChanged = changed.has(name) && lastVars[name] !== undefined &&
                          JSON.stringify(lastVars[name]) !== JSON.stringify(value);

    return '<div class="var-row ' + cls + (reallyChanged ? ' changed' : '') + '">' +
             '<span class="name">' + esc(name) +
               '<span class="type">' + esc(t) + '</span>' +
             '</span>' +
             '<span class="value" title="' + esc(displayValue) + '">' +
               esc(displayValue) +
             '</span>' +
           '</div>';
  }).join('');
}

// ─── Controls ───────────────────────────────────────────
function setupControls() {
  $('btn-first').onclick = () => renderStep(0);
  $('btn-last').onclick  = () => renderStep(session.steps.length - 1);
  $('btn-prev').onclick  = () => renderStep(currentStep - 10);
  $('btn-next').onclick  = () => renderStep(currentStep + 10);
  $('btn-prev1').onclick = () => renderStep(currentStep - 1);
  $('btn-next1').onclick = () => renderStep(currentStep + 1);

  $('btn-play').onclick = togglePlay;

  $('timeline').onclick = (e) => {
    const rect = $('timeline').getBoundingClientRect();
    const p = (e.clientX - rect.left) / rect.width;
    renderStep(Math.round(p * (session.steps.length - 1)));
  };

  $('btn-run-python').onclick = runPython;
  $('btn-clear-code').onclick = () => { $('code-input').value = ''; $('code-input').focus(); };

  document.addEventListener('keydown', (e) => {
    if (e.target && e.target.tagName === 'TEXTAREA') return;
    if (!session) return;
    const step = e.shiftKey ? 10 : 1;
    if (e.key === 'ArrowLeft')  { renderStep(currentStep - step); e.preventDefault(); }
    if (e.key === 'ArrowRight') { renderStep(currentStep + step); e.preventDefault(); }
    if (e.key === ' ')          { togglePlay(); e.preventDefault(); }
    if (e.key === 'Home')       { renderStep(0); e.preventDefault(); }
    if (e.key === 'End')        { renderStep(session.steps.length - 1); e.preventDefault(); }
  });
}

function togglePlay() {
  if (!session) return;
  isPlaying = !isPlaying;
  $('play-label').textContent = isPlaying ? 'Пауза' : 'Воспроизвести';
  $('btn-play').querySelector('.icon').textContent = isPlaying ? '⏸' : '▶';

  if (isPlaying) {
    playTimer = setInterval(() => {
      if (currentStep >= session.steps.length - 1) {
        clearInterval(playTimer);
        isPlaying = false;
        $('play-label').textContent = 'Воспроизвести';
        $('btn-play').querySelector('.icon').textContent = '▶';
        return;
      }
      renderStep(currentStep + 1);
    }, 150);
  } else {
    clearInterval(playTimer);
  }
}

setupControls();
loadSamples();
</script>
</body>
</html>
""";
}