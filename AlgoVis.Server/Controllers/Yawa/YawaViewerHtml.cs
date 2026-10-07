namespace AlgoVis.Server.Controllers.Yawa;

/// <summary>
/// HTML-страница просмотрщика YAWA. Отдаётся из YawaController GET /api/yawa/viewer.
/// Это дев-страница для бэкендера, а не продакшн-фронт.
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
    --bg-0:#070a14; --bg-1:#0d1220; --bg-2:#141a2b; --bg-3:#1c2338;
    --border:#232a42; --border-hi:#2f3854;
    --text:#e8ecf4; --text-dim:#8b95ad; --text-muted:#5a6478;
    --accent:#6366f1; --accent-hi:#818cf8; --accent-glow:rgba(99,102,241,0.35);
    --pink:#ec4899;
    --cmp:#f59e0b; --swap:#10b981; --assign:#3b82f6;
    --mark:#ec4899; --call:#a855f7; --danger:#ef4444; --ok:#10b981;
    --mono: ui-monospace,'SF Mono',Menlo,'JetBrains Mono',monospace;
    --sans: -apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;
  }
  *{box-sizing:border-box}
  html,body{
    margin:0;padding:0;color:var(--text);font-family:var(--sans);min-height:100vh;
    background:radial-gradient(1200px 600px at 20% -10%,#182042 0%,transparent 60%),
               radial-gradient(900px 500px at 100% 100%,#1a1230 0%,transparent 55%),
               var(--bg-0);
    font-size:14px;line-height:1.5}
  .app{max-width:1600px;margin:0 auto;padding:20px 24px 60px}

  /* header */
  header{display:flex;align-items:center;justify-content:space-between;
    margin-bottom:20px;padding-bottom:16px;border-bottom:1px solid var(--border)}
  .brand{display:flex;align-items:center;gap:12px}
  .logo{width:38px;height:38px;border-radius:10px;
    background:linear-gradient(135deg,var(--accent),var(--pink));
    display:flex;align-items:center;justify-content:center;font-size:20px;
    box-shadow:0 6px 20px -6px var(--accent-glow)}
  .brand h1{margin:0;font-size:19px;font-weight:600}
  .brand .sub{margin:2px 0 0;font-size:12px;color:var(--text-dim)}
  .header-right{display:flex;gap:10px;align-items:center}
  .user-chip{display:flex;align-items:center;gap:8px;padding:5px 12px;
    background:var(--bg-2);border:1px solid var(--border);border-radius:999px;
    font-size:13px;cursor:pointer;transition:.15s}
  .user-chip:hover{border-color:var(--accent)}
  .user-chip .avatar{width:24px;height:24px;border-radius:50%;
    background:linear-gradient(135deg,var(--accent),var(--pink));
    display:flex;align-items:center;justify-content:center;font-size:11px;font-weight:700}
  .btn{padding:8px 16px;border-radius:8px;border:1px solid var(--border);
    background:var(--bg-2);color:var(--text);font-family:inherit;font-size:13px;
    cursor:pointer;transition:.15s;font-weight:500}
  .btn:hover{background:var(--bg-3);border-color:var(--border-hi)}
  .btn.primary{background:linear-gradient(135deg,var(--accent),#4f46e5);
    border-color:var(--accent);box-shadow:0 4px 14px -4px var(--accent-glow)}
  .btn.primary:hover{background:linear-gradient(135deg,var(--accent-hi),#6366f1)}
  .btn.ghost{background:transparent}
  .btn.small{padding:5px 10px;font-size:12px}

  /* layout */
  .layout{display:grid;grid-template-columns:280px 1fr;gap:18px;align-items:start}
  @media(max-width:1200px){.layout{grid-template-columns:1fr}}

  .card{background:linear-gradient(180deg,var(--bg-2),var(--bg-1));
    border:1px solid var(--border);border-radius:12px;padding:16px;margin-bottom:14px;
    box-shadow:0 4px 16px -8px rgba(0,0,0,.5)}
  .card:last-child{margin-bottom:0}
  .card-title{font-size:11px;font-weight:600;letter-spacing:.08em;text-transform:uppercase;
    color:var(--text-muted);margin:0 0 12px;display:flex;align-items:center;justify-content:space-between}

  /* tabs */
  .tabs{display:flex;gap:4px;margin-bottom:12px;background:var(--bg-1);padding:3px;border-radius:6px}
  .tab{flex:1;padding:6px 10px;border-radius:4px;background:transparent;border:none;
    color:var(--text-dim);font-family:inherit;font-size:12px;cursor:pointer;transition:.15s}
  .tab:hover{color:var(--text)}
  .tab.active{background:var(--bg-3);color:var(--text)}

  /* sample/project list */
  .item-list{display:flex;flex-direction:column;gap:4px}
  .item{display:flex;align-items:center;gap:8px;padding:8px 10px;background:transparent;
    border:1px solid transparent;border-radius:6px;color:var(--text);font-size:13px;
    cursor:pointer;text-align:left;transition:.15s;width:100%;font-family:inherit}
  .item:hover{background:var(--bg-3);border-color:var(--border-hi)}
  .item.active{background:linear-gradient(135deg,rgba(99,102,241,.18),rgba(236,72,153,.10));
    border-color:var(--accent)}
  .item .icon{width:22px;height:22px;flex-shrink:0;border-radius:5px;background:var(--bg-3);
    display:flex;align-items:center;justify-content:center;font-size:11px}
  .item .name{flex:1;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
  .item .badge{font-size:9px;padding:1px 5px;border-radius:3px;background:var(--bg-3);
    color:var(--text-muted);text-transform:uppercase;letter-spacing:.05em}
  .item .badge.pub{background:rgba(16,185,129,.2);color:var(--ok)}
  .empty{color:var(--text-muted);font-size:12px;font-style:italic;padding:8px;text-align:center}

  /* code editor */
  .code-area{width:100%;min-height:280px;padding:12px;background:var(--bg-0);
    border:1px solid var(--border);border-radius:8px;color:var(--text);
    font-family:var(--mono);font-size:12.5px;line-height:1.55;resize:vertical;
    tab-size:4;outline:none;transition:.15s}
  .code-area:focus{border-color:var(--accent);box-shadow:0 0 0 3px var(--accent-glow)}
  .code-actions{display:flex;gap:6px;margin-top:10px;flex-wrap:wrap}

  /* error */
  .error-banner{display:none;margin-top:10px;padding:10px 14px;
    background:linear-gradient(90deg,rgba(239,68,68,.18),rgba(239,68,68,.05));
    border-left:3px solid var(--danger);border-radius:6px;
    font-family:var(--mono);font-size:12.5px;color:#fecaca;white-space:pre-wrap;word-break:break-word}
  .error-banner.show{display:block}
  .error-banner .head{font-family:var(--sans);font-weight:600;color:var(--danger);
    margin-bottom:4px;font-size:12px;text-transform:uppercase;letter-spacing:.06em}

  /* stats */
  .stats-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(120px,1fr));
    gap:10px;margin-bottom:16px}
  .stat{background:linear-gradient(180deg,var(--bg-3),var(--bg-2));
    border:1px solid var(--border);border-radius:10px;padding:12px 14px;position:relative;overflow:hidden}
  .stat::before{content:'';position:absolute;top:0;left:0;right:0;height:2px;
    background:linear-gradient(90deg,var(--accent),var(--pink));opacity:.7}
  .stat .label{font-size:11px;color:var(--text-muted);text-transform:uppercase;
    letter-spacing:.06em;margin-bottom:4px}
  .stat .value{font-size:24px;font-weight:700;font-family:var(--mono);
    background:linear-gradient(135deg,var(--text),var(--text-dim));
    -webkit-background-clip:text;-webkit-text-fill-color:transparent;background-clip:text}

  /* workspace */
  .workspace{display:grid;grid-template-columns:minmax(0,1fr) 260px;gap:14px;margin-bottom:14px}
  @media(max-width:1100px){.workspace{grid-template-columns:1fr}}
  .viz-wrap{padding:16px;background:var(--bg-1);border:1px solid var(--border);
    border-radius:10px;min-height:180px;display:flex;align-items:center;justify-content:center;
    overflow:auto}
  .viz-selector{display:flex;flex-wrap:wrap;gap:5px;margin-bottom:10px}
  .viz-chip{display:inline-flex;align-items:center;gap:5px;padding:4px 9px;
    background:var(--bg-2);border:1px solid var(--border);border-radius:6px;
    color:var(--text-dim);font-family:var(--mono);font-size:11px;cursor:pointer;transition:.15s}
  .viz-chip:hover{background:var(--bg-3);border-color:var(--border-hi);color:var(--text)}
  .viz-chip.active{background:linear-gradient(135deg,rgba(99,102,241,.2),rgba(99,102,241,.05));
    border-color:var(--accent);color:var(--text)}
  .viz-chip .kind-badge{font-size:8px;padding:1px 4px;border-radius:3px;
    background:var(--bg-3);color:var(--text-muted);text-transform:uppercase}

  /* array */
  .array{display:flex;flex-wrap:wrap;gap:6px;justify-content:center}
  .cell{position:relative;min-width:46px;height:46px;padding:0 10px;
    display:flex;align-items:center;justify-content:center;
    background:linear-gradient(180deg,var(--bg-3),var(--bg-2));
    border:2px solid var(--border-hi);border-radius:6px;
    font-family:var(--mono);font-size:16px;font-weight:600;color:var(--text);
    transition:all .25s cubic-bezier(.4,0,.2,1)}
  .cell .idx{position:absolute;top:-7px;left:50%;transform:translateX(-50%);
    font-size:8px;color:var(--text-muted);background:var(--bg-0);
    padding:1px 4px;border-radius:3px;font-weight:400}
  .cell.hl-cmp{border-color:var(--cmp);background:rgba(245,158,11,.2);
    box-shadow:0 0 0 3px rgba(245,158,11,.18);transform:translateY(-2px) scale(1.05);color:#fde68a}
  .cell.hl-swap{border-color:var(--swap);background:rgba(16,185,129,.2);
    box-shadow:0 0 0 3px rgba(16,185,129,.18);transform:translateY(-2px) scale(1.05);color:#a7f3d0}
  .cell.hl-mark{border-color:var(--mark);background:rgba(236,72,153,.2);
    box-shadow:0 0 0 3px rgba(236,72,153,.18);transform:translateY(-2px) scale(1.05);color:#fbcfe8}

  /* matrix */
  .matrix-wrap{overflow-x:auto}
  .matrix{border-collapse:separate;border-spacing:3px;font-family:var(--mono);margin:0 auto}
  .matrix th{font-size:9px;color:var(--text-muted);font-weight:500;padding:0 4px;text-align:center}
  .mcell{min-width:36px;height:36px;padding:3px 6px;text-align:center;vertical-align:middle;
    background:linear-gradient(180deg,var(--bg-3),var(--bg-2));
    border:2px solid var(--border-hi);border-radius:5px;
    font-size:13px;font-weight:600;color:var(--text);transition:.2s}
  .mcell.hl-cmp{border-color:var(--cmp);background:rgba(245,158,11,.25);color:#fde68a;transform:scale(1.06)}
  .mcell.hl-swap{border-color:var(--swap);background:rgba(16,185,129,.25);color:#a7f3d0;transform:scale(1.06)}
  .mcell.hl-mark{border-color:var(--mark);background:rgba(236,72,153,.25);color:#fbcfe8;transform:scale(1.06)}

  /* tree */
  .tree-svg{display:block;max-width:100%;height:auto}
  .tree-node-circle{fill:var(--bg-3);stroke:var(--border-hi);stroke-width:2}
  .tree-node-text{fill:var(--text);font-family:ui-monospace,monospace;font-size:12px;
    font-weight:600;text-anchor:middle;dominant-baseline:central}
  .tree-edge{stroke:var(--border-hi);stroke-width:2;fill:none}

  /* vars */
  .vars{display:flex;flex-direction:column;gap:3px;max-height:380px;overflow-y:auto}
  .var-row{display:grid;grid-template-columns:1fr auto;align-items:center;gap:8px;
    padding:5px 9px;background:var(--bg-1);border:1px solid transparent;border-radius:5px;
    font-family:var(--mono);font-size:12px;transition:.2s;cursor:pointer}
  .var-row:hover{border-color:var(--border-hi)}
  .var-row.selected{border-color:var(--accent);background:rgba(99,102,241,.08)}
  .var-row .name{color:var(--text-dim);overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
  .var-row .name .type{color:var(--text-muted);font-size:9px;margin-left:5px;
    padding:1px 4px;background:var(--bg-3);border-radius:2px}
  .var-row .value{color:var(--text);font-weight:600;text-align:right;max-width:130px;
    overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
  .var-row.changed{background:linear-gradient(90deg,rgba(99,102,241,.14),transparent);
    border-color:rgba(99,102,241,.4);animation:pulse 0.7s ease}
  @keyframes pulse{0%{box-shadow:0 0 0 0 rgba(99,102,241,.5)}
    70%{box-shadow:0 0 0 8px rgba(99,102,241,0)}
    100%{box-shadow:0 0 0 0 rgba(99,102,241,0)}}

  /* timeline + controls */
  .timeline{position:relative;height:6px;background:var(--bg-3);border-radius:3px;
    margin:12px 0;overflow:hidden;cursor:pointer}
  .timeline-fill{height:100%;background:linear-gradient(90deg,var(--accent),var(--pink));
    border-radius:3px;transition:width .15s;box-shadow:0 0 8px var(--accent-glow)}
  .annotation{min-height:30px;padding:6px 12px;
    background:linear-gradient(90deg,rgba(99,102,241,.12),transparent);
    border-left:3px solid var(--accent);border-radius:6px;margin-bottom:12px;
    font-size:12.5px;display:flex;align-items:center;gap:8px}
  .annotation:empty{display:none}
  .annotation .badge{font-size:10px;font-weight:700;letter-spacing:.08em;text-transform:uppercase;
    padding:2px 7px;background:var(--accent);color:white;border-radius:3px}
  .controls{display:flex;gap:5px;align-items:center;flex-wrap:wrap;margin-bottom:10px}
  .ctrl{display:inline-flex;align-items:center;justify-content:center;gap:4px;
    min-width:36px;height:34px;padding:0 10px;background:var(--bg-2);
    border:1px solid var(--border);border-radius:6px;color:var(--text);
    font-family:inherit;font-size:12px;font-weight:500;cursor:pointer;transition:.15s}
  .ctrl:hover:not(:disabled){background:var(--bg-3);border-color:var(--border-hi);transform:translateY(-1px)}
  .ctrl:disabled{opacity:.4;cursor:not-allowed}
  .ctrl.primary{background:linear-gradient(135deg,var(--accent),#4f46e5);
    border-color:var(--accent);box-shadow:0 4px 14px -4px var(--accent-glow);min-width:120px}
  .ctrl.primary:hover:not(:disabled){background:linear-gradient(135deg,var(--accent-hi),#6366f1)}
  .pos{display:inline-flex;align-items:center;height:34px;padding:0 12px;
    background:var(--bg-1);border:1px solid var(--border);border-radius:6px;
    font-family:var(--mono);font-size:12px;color:var(--text-dim);margin:0 4px}
  .pos b{color:var(--text);font-weight:600}

  /* steps */
  .steps-card{padding:0;overflow:hidden}
  .steps-head{padding:12px 16px;border-bottom:1px solid var(--border);
    display:flex;align-items:center;justify-content:space-between}
  .steps{max-height:360px;overflow-y:auto;padding:4px}
  .step{display:grid;grid-template-columns:42px 80px 1fr;gap:10px;align-items:center;
    padding:6px 10px;border-radius:5px;font-family:var(--mono);font-size:11.5px;
    cursor:pointer;transition:.1s;border-left:2px solid transparent}
  .step:hover{background:var(--bg-3)}
  .step.current{background:linear-gradient(90deg,rgba(99,102,241,.14),transparent);
    border-left-color:var(--accent)}
  .step .n{color:var(--text-muted);text-align:right;font-size:10px}
  .step .k{font-size:9px;font-weight:700;letter-spacing:.05em;text-transform:uppercase;
    padding:2px 5px;border-radius:3px;text-align:center;background:var(--bg-3);color:var(--text-dim)}
  .step[data-kind="compare"] .k{background:rgba(245,158,11,.18);color:var(--cmp)}
  .step[data-kind="swap"] .k{background:rgba(16,185,129,.18);color:var(--swap)}
  .step[data-kind="assign"] .k{background:rgba(59,130,246,.18);color:var(--assign)}
  .step[data-kind="call"] .k{background:rgba(168,85,247,.18);color:var(--call)}
  .step[data-kind="return"] .k{background:rgba(16,185,129,.18);color:var(--swap)}
  .step .d{color:var(--text-dim);overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
  .step .d .ann{color:var(--cmp);font-style:italic}

  /* modal */
  .modal-overlay{position:fixed;inset:0;background:rgba(0,0,0,.75);backdrop-filter:blur(4px);
    display:none;align-items:center;justify-content:center;z-index:1000;padding:20px}
  .modal-overlay.show{display:flex}
  .modal{background:linear-gradient(180deg,var(--bg-2),var(--bg-1));
    border:1px solid var(--border);border-radius:14px;padding:28px;max-width:420px;width:100%;
    box-shadow:0 20px 60px -20px rgba(0,0,0,.8)}
  .modal h2{margin:0 0 6px;font-size:20px;font-weight:600}
  .modal .sub{color:var(--text-dim);font-size:13px;margin-bottom:20px}
  .modal .field{margin-bottom:14px}
  .modal label{display:block;font-size:12px;color:var(--text-dim);margin-bottom:6px;
    font-weight:500;letter-spacing:.02em}
  .modal input{width:100%;padding:10px 12px;background:var(--bg-0);
    border:1px solid var(--border);border-radius:7px;color:var(--text);
    font-family:inherit;font-size:13px;outline:none;transition:.15s}
  .modal input:focus{border-color:var(--accent);box-shadow:0 0 0 3px var(--accent-glow)}
  .modal .actions{display:flex;gap:8px;margin-top:20px}
  .modal .actions .btn{flex:1;justify-content:center;padding:10px}
  .modal .switch{text-align:center;margin-top:14px;font-size:12px;color:var(--text-dim)}
  .modal .switch a{color:var(--accent-hi);cursor:pointer;text-decoration:none;font-weight:500}
  .modal .error{display:none;margin-bottom:14px;padding:8px 12px;
    background:rgba(239,68,68,.15);border-left:3px solid var(--danger);
    border-radius:5px;color:#fecaca;font-size:12px}
  .modal .error.show{display:block}

  /* toast */
  .toast{position:fixed;bottom:24px;right:24px;padding:12px 20px;background:var(--bg-2);
    border:1px solid var(--border);border-left:3px solid var(--accent);
    border-radius:8px;font-size:13px;box-shadow:0 10px 30px -10px rgba(0,0,0,.7);
    transform:translateY(100px);opacity:0;transition:.3s;z-index:2000;max-width:380px}
  .toast.show{transform:translateY(0);opacity:1}
  .toast.success{border-left-color:var(--ok)}
  .toast.error{border-left-color:var(--danger)}
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
    <div class="header-right" id="header-right">
      <!-- заполняется JS -->
    </div>
  </header>

  <div class="layout">
    <aside>
      <!-- Проекты / примеры -->
      <div class="card" id="sidebar-card">
        <div class="tabs">
          <button class="tab active" data-tab="projects" id="tab-projects-btn">Проекты</button>
          <button class="tab" data-tab="samples">Примеры</button>
          <button class="tab" data-tab="new">Код</button>
        </div>

        <div id="tab-projects" style="display:none;">
          <div id="projects-toolbar" style="margin-bottom:8px;">
            <button class="btn primary small" style="width:100%;" id="btn-new-project">+ Новый проект</button>
          </div>
          <div class="item-list" id="projects-list"></div>
        </div>

        <div id="tab-samples" style="display:none;">
          <div class="item-list" id="samples-list">
            <div class="empty">загрузка…</div>
          </div>
        </div>

        <div id="tab-new" style="display:block;">
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
            <div class="head">Ошибка</div>
            <div class="body"></div>
          </div>

          <div class="code-actions">
            <button class="btn primary" id="btn-run">▶ Запустить</button>
            <button class="btn" id="btn-save" style="display:none;">💾 Сохранить</button>
            <button class="btn ghost" id="btn-clear">✕</button>
          </div>
        </div>
      </div>
    </aside>

    <main id="main">
      <div class="empty" id="empty-state" style="text-align:center;padding:60px 20px;color:var(--text-muted);">
        <div style="font-size:42px;margin-bottom:10px;opacity:.6;">▶</div>
        <div style="font-size:15px;color:var(--text-dim);margin-bottom:4px;">Выберите пример или запустите код</div>
      </div>

      <div id="content" style="display:none;">
        <div class="stats-grid" id="stats"></div>

        <div class="workspace">
          <div class="card" style="padding:16px;margin-bottom:0;">
            <h3 class="card-title">
              <span id="viz-title">Структура данных</span>
            </h3>
            <div class="viz-selector" id="viz-selector"></div>
            <div class="viz-wrap">
              <div id="viz-content"></div>
            </div>
          </div>

          <div class="card" style="padding:14px;margin-bottom:0;">
            <h3 class="card-title">Переменные</h3>
            <div class="vars" id="vars"></div>
          </div>
        </div>

        <div class="card" style="padding:16px;">
          <div class="annotation" id="annotation"></div>
          <div class="timeline" id="timeline">
            <div class="timeline-fill" id="timeline-fill" style="width:0%"></div>
          </div>
          <div class="controls">
            <button class="ctrl" id="btn-first">⟪</button>
            <button class="ctrl" id="btn-prev">−10</button>
            <button class="ctrl" id="btn-prev1">‹</button>
            <span class="pos" id="pos"><b>—</b> / —</span>
            <button class="ctrl" id="btn-next1">›</button>
            <button class="ctrl" id="btn-next">+10</button>
            <button class="ctrl" id="btn-last">⟫</button>
            <button class="ctrl primary" id="btn-play" style="margin-left:auto;">
              <span id="play-icon">▶</span> <span id="play-label">Воспроизвести</span>
            </button>
          </div>
        </div>

        <div class="card steps-card" style="margin-top:14px;">
          <div class="steps-head">
            <span style="font-size:13px;font-weight:600;">История шагов</span>
            <span style="font-size:11px;color:var(--text-muted);font-family:var(--mono);" id="steps-count">—</span>
          </div>
          <div class="steps" id="steps"></div>
        </div>
      </div>
    </main>
  </div>
</div>

<!-- Модалка логина -->
<div class="modal-overlay" id="auth-modal">
  <div class="modal">
    <h2 id="auth-title">Вход</h2>
    <div class="sub" id="auth-sub">Войдите, чтобы сохранять проекты</div>

    <div class="error" id="auth-error"></div>

    <div class="field" id="field-username" style="display:none;">
      <label>Имя пользователя</label>
      <input type="text" id="input-username" autocomplete="username" placeholder="минимум 3 символа">
    </div>

    <div class="field">
      <label>Email</label>
      <input type="email" id="input-email" autocomplete="email" placeholder="you@example.com">
    </div>

    <div class="field">
      <label>Пароль</label>
      <input type="password" id="input-password" autocomplete="current-password" placeholder="минимум 6 символов">
    </div>

    <div class="actions">
      <button class="btn primary" id="btn-auth-submit">Войти</button>
      <button class="btn ghost" id="btn-auth-close">Отмена</button>
    </div>

    <div class="switch">
      <span id="auth-switch-text">Нет аккаунта?</span>
      <a id="auth-switch-link">Зарегистрироваться</a>
    </div>
  </div>
</div>

<!-- Toast -->
<div class="toast" id="toast"></div>

<script>
// ─────────────── State ───────────────
const API = '';  // тот же origin
let session = null;
let snapshots = [];
let currentStep = 0;
let isPlaying = false;
let playTimer = null;
let lastVars = {};
let selectedVar = null;

// auth
let auth = {
  access: localStorage.getItem('algovis_access') || null,
  refresh: localStorage.getItem('algovis_refresh') || null,
  user: null
};

// projects
let myProjects = [];
let currentProject = null;   // объект проекта, если редактируем сохранённый
let isDirty = false;         // есть ли несохранённые изменения

const $ = (id) => document.getElementById(id);

// ─────────────── Utils ───────────────
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
function looksLikeTree(v) {
  if (!v || typeof v !== 'object') return false;
  if (v.__type === 'tree' || v.__type === 'tree_node') return true;
  return 'value' in v && ('left' in v || 'right' in v);
}
function isMatrix(v) {
  return Array.isArray(v) && v.length > 0 && Array.isArray(v[0]);
}
function detectShape(v) {
  if (isScalar(v)) return 'scalar';
  if (isMatrix(v)) return 'matrix';
  if (Array.isArray(v)) return 'array';
  if (looksLikeTree(v)) return 'tree';
  if (v && typeof v === 'object' && v.__type === 'set') return 'set';
  if (v && typeof v === 'object' && v.__type && v.__type.startsWith('array')) return 'array';
  if (v && typeof v === 'object') return 'dict';
  return 'scalar';
}

function toast(msg, kind) {
  const el = $('toast');
  el.textContent = msg;
  el.className = 'toast ' + (kind || '');
  el.classList.add('show');
  setTimeout(() => el.classList.remove('show'), 3000);
}

// ─────────────── API helper ───────────────
async function api(path, opts = {}, retry = true) {
  opts.headers = opts.headers || {};
  if (auth.access) opts.headers['Authorization'] = 'Bearer ' + auth.access;
  if (opts.body && typeof opts.body === 'string' && !opts.headers['Content-Type'])
    opts.headers['Content-Type'] = 'application/json';

  const r = await fetch(API + path, opts);

  // 401 → пробуем refresh один раз
  if (r.status === 401 && retry && auth.refresh) {
    const ok = await tryRefresh();
    if (ok) return api(path, opts, false);
    logout();
  }
  return r;
}

async function tryRefresh() {
  if (!auth.refresh) return false;
  try {
    const r = await fetch(API + '/api/auth/refresh', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken: auth.refresh })
    });
    if (!r.ok) return false;
    const data = await r.json();
    setAuth(data.accessToken, data.refreshToken, data.user);
    return true;
  } catch { return false; }
}

function setAuth(access, refresh, user) {
  auth.access = access;
  auth.refresh = refresh;
  auth.user = user || auth.user;
  if (access) localStorage.setItem('algovis_access', access);
  else localStorage.removeItem('algovis_access');
  if (refresh) localStorage.setItem('algovis_refresh', refresh);
  else localStorage.removeItem('algovis_refresh');
  renderHeader();
}

function logout() {
  if (auth.refresh) {
    fetch(API + '/api/auth/logout', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ refreshToken: auth.refresh })
    }).catch(() => {});
  }
  auth = { access: null, refresh: null, user: null };
  localStorage.removeItem('algovis_access');
  localStorage.removeItem('algovis_refresh');
  currentProject = null;
  isDirty = false;
  renderHeader();
  renderSidebar();
}

// ─────────────── Header ───────────────
function renderHeader() {
  const box = $('header-right');
  if (auth.user) {
    const initials = (auth.user.username || '?').slice(0, 1).toUpperCase();
    box.innerHTML = `
      <span style="font-size:11px;color:var(--text-muted);font-family:var(--mono);">${esc(auth.user.email)}</span>
      <button class="user-chip" id="btn-logout">
        <span class="avatar">${esc(initials)}</span>
        <span>${esc(auth.user.username)}</span>
      </button>
    `;
    $('btn-logout').onclick = () => {
      if (confirm('Выйти из аккаунта?')) logout();
    };
  } else {
    box.innerHTML = `
      <button class="btn" id="btn-login">Войти</button>
      <button class="btn primary" id="btn-register">Регистрация</button>
    `;
    $('btn-login').onclick = () => openAuth('login');
    $('btn-register').onclick = () => openAuth('register');
  }
}

// ─────────────── Auth modal ───────────────
let authMode = 'login';  // 'login' | 'register'

function openAuth(mode) {
  authMode = mode;
  $('auth-modal').classList.add('show');
  $('auth-title').textContent = mode === 'login' ? 'Вход' : 'Регистрация';
  $('auth-sub').textContent = mode === 'login'
    ? 'Войдите, чтобы сохранять проекты'
    : 'Создайте аккаунт — это бесплатно';
  $('field-username').style.display = mode === 'register' ? 'block' : 'none';
  $('btn-auth-submit').textContent = mode === 'login' ? 'Войти' : 'Зарегистрироваться';
  $('auth-switch-text').textContent = mode === 'login' ? 'Нет аккаунта?' : 'Уже есть аккаунт?';
  $('auth-switch-link').textContent = mode === 'login' ? 'Зарегистрироваться' : 'Войти';
  $('auth-error').classList.remove('show');
  setTimeout(() => $('input-email').focus(), 100);
}

function closeAuth() {
  $('auth-modal').classList.remove('show');
  $('auth-error').classList.remove('show');
  $('input-password').value = '';
}

async function submitAuth() {
  const email = $('input-email').value.trim();
  const password = $('input-password').value;
  const username = $('input-username').value.trim();
  const errBox = $('auth-error');

  errBox.classList.remove('show');

  try {
    let url, body;
    if (authMode === 'login') {
      url = '/api/auth/login';
      body = { email, password };
    } else {
      url = '/api/auth/register';
      body = { email, username, password };
    }

    const r = await fetch(API + url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(body)
    });

    const data = await r.json();

    if (!r.ok) {
      errBox.textContent = data.error || 'Ошибка';
      errBox.classList.add('show');
      return;
    }

    setAuth(data.accessToken, data.refreshToken, data.user);
    closeAuth();
    toast(authMode === 'login' ? 'Вход выполнен' : 'Регистрация успешна', 'success');
    await loadMyProjects();
    renderSidebar();
  } catch (e) {
    errBox.textContent = 'Сеть: ' + e.message;
    errBox.classList.add('show');
  }
}

$('btn-auth-submit').onclick = submitAuth;
$('btn-auth-close').onclick = closeAuth;
$('auth-switch-link').onclick = () => openAuth(authMode === 'login' ? 'register' : 'login');
$('auth-modal').addEventListener('click', (e) => {
  if (e.target === $('auth-modal')) closeAuth();
});
document.addEventListener('keydown', (e) => {
  if (e.key === 'Enter' && $('auth-modal').classList.contains('show')) {
    submitAuth();
  }
});

// ─────────────── Sidebar / Tabs ───────────────
document.querySelectorAll('.tab').forEach(t => {
  t.onclick = () => {
    document.querySelectorAll('.tab').forEach(x => x.classList.remove('active'));
    t.classList.add('active');
    const which = t.dataset.tab;
    $('tab-projects').style.display = which === 'projects' ? 'block' : 'none';
    $('tab-samples').style.display  = which === 'samples' ? 'block' : 'none';
    $('tab-new').style.display      = which === 'new' ? 'block' : 'none';
    if (which === 'projects') renderSidebar();
  };
});

async function loadMyProjects() {
  if (!auth.user) { myProjects = []; return; }
  try {
    const r = await api('/api/projects');
    if (r.ok) myProjects = await r.json();
    else myProjects = [];
  } catch { myProjects = []; }
}

function renderSidebar() {
  // показываем вкладку «Проекты» только для авторизованных
  if (auth.user) {
    $('tab-projects-btn').style.display = '';
    if (document.querySelector('.tab.active')?.dataset.tab === 'projects') {
      $('tab-projects').style.display = 'block';
      $('tab-new').style.display = 'none';
    }
    renderProjectsList();
  } else {
    $('tab-projects-btn').style.display = 'none';
    // если активна была вкладка проектов — переключаемся на «Код»
    if (document.querySelector('.tab.active')?.dataset.tab === 'projects') {
      document.querySelectorAll('.tab').forEach(x => x.classList.remove('active'));
      document.querySelector('[data-tab="new"]').classList.add('active');
      $('tab-projects').style.display = 'none';
      $('tab-new').style.display = 'block';
    }
  }
  // кнопка «Сохранить» только для авторизованных
  $('btn-save').style.display = auth.user ? 'inline-flex' : 'none';
}

function renderProjectsList() {
  const box = $('projects-list');
  if (!myProjects.length) {
    box.innerHTML = '<div class="empty">пока нет проектов</div>';
    return;
  }
  box.innerHTML = myProjects.map(p => `
    <button class="item ${currentProject?.id === p.id ? 'active' : ''}" data-id="${p.id}">
      <span class="icon">📄</span>
      <span class="name">${esc(p.name)}</span>
      ${p.isPublic ? '<span class="badge pub">pub</span>' : ''}
    </button>
  `).join('');
  box.querySelectorAll('.item').forEach(el => {
    el.onclick = () => loadProject(+el.dataset.id);
  });
}

async function loadProject(id) {
  try {
    const r = await api('/api/projects/' + id);
    if (!r.ok) { toast('Проект не найден', 'error'); return; }
    const p = await r.json();
    currentProject = p;
    isDirty = false;
    $('code-input').value = p.pythonCode || '';
    $('empty-state').innerHTML = `
      <div style="text-align:center;padding:60px 20px;color:var(--text-muted);">
        <div style="font-size:42px;margin-bottom:10px;opacity:.6;">▶</div>
        <div style="font-size:15px;color:var(--text-dim);">
          Проект: <b style="color:var(--text);">${esc(p.name)}</b>
        </div>
        <div style="font-size:12px;margin-top:8px;">
          Нажмите <b>Запустить</b>, чтобы увидеть визуализацию
        </div>
      </div>`;
    $('content').style.display = 'none';
    $('empty-state').style.display = 'block';
    renderProjectsList();
    renderSaveButtons();
  } catch (e) { toast('Ошибка загрузки: ' + e.message, 'error'); }
}

function renderSaveButtons() {
  const btn = $('btn-save');
  if (!auth.user) { btn.style.display = 'none'; return; }
  btn.style.display = 'inline-flex';
  btn.textContent = isDirty
    ? (currentProject ? '💾 Сохранить*' : '💾 Сохранить как новый')
    : (currentProject ? '💾 Сохранить' : '💾 Сохранить как новый');
}

$('btn-new-project').onclick = () => {
  currentProject = null;
  isDirty = false;
  $('code-input').value = '# Новый проект\n\ndef main():\n    A = [5, 2, 8, 1]\n    # ...\n';
  renderProjectsList();
  renderSaveButtons();
  toast('Новый проект. Напишите код и нажмите «Сохранить»', 'success');
};

// пометка «dirty» при редактировании
$('code-input').addEventListener('input', () => {
  if (!isDirty) {
    isDirty = true;
    renderSaveButtons();
  }
});

// ─────────────── Samples ───────────────
async function loadSamples() {
  try {
    const r = await fetch(API + '/api/yawa/samples');
    const list = await r.json();
    const box = $('samples-list');
    if (!list.length) { box.innerHTML = '<div class="empty">пусто</div>'; return; }
    box.innerHTML = list.map(s => `
      <button class="item" data-name="${esc(s.name)}">
        <span class="icon">📄</span>
        <span class="name">${esc(s.name.replace('.yawa.json',''))}</span>
      </button>
    `).join('');
    box.querySelectorAll('.item').forEach(el => {
      el.onclick = () => runSample(el.dataset.name);
    });
  } catch (e) {
    $('samples-list').innerHTML = '<div class="empty">Ошибка: ' + esc(e.message) + '</div>';
  }
}

async function runSample(name) {
  showLoading();
  try {
    const r = await fetch(API + '/api/yawa/run-sample?name=' + encodeURIComponent(name),
      { method: 'POST' });
    const data = await r.json();
    if (data.error) { toast('Ошибка: ' + data.error, 'error'); return; }
    currentProject = null;
    loadSession(data);
  } catch (e) { toast('Ошибка: ' + e.message, 'error'); }
}

// ─────────────── Run ───────────────
$('btn-run').onclick = async () => {
  const code = $('code-input').value;
  if (!code.trim()) return;
  showLoading();
  $('error-banner').classList.remove('show');

  try {
    const r = await fetch(API + '/api/yawa/run-python', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ code })
    });

    if (r.status === 429) {
      $('error-banner').querySelector('.head').textContent = 'Слишком часто';
      $('error-banner').querySelector('.body').textContent =
        'Слишком много запусков. Подождите минуту или войдите в аккаунт (лимит выше).';
      $('error-banner').classList.add('show');
      $('empty-state').style.display = 'block';
      $('content').style.display = 'none';
      return;
    }

    const data = await r.json();
    if (data.error) {
      $('error-banner').querySelector('.head').textContent =
        data.kind === 'transpiler' ? 'Ошибка транспайлера' :
        data.kind === 'runtime' ? 'Ошибка исполнения' : 'Ошибка';
      let body = data.error;
      if (data.line !== undefined && data.line > 0)
        body = `Строка ${data.line + 1}:${data.column} — ${body}`;
      $('error-banner').querySelector('.body').textContent = body;
      $('error-banner').classList.add('show');
      $('empty-state').style.display = 'block';
      $('content').style.display = 'none';
      return;
    }
    loadSession(data);
  } catch (e) {
    toast('Сеть: ' + e.message, 'error');
  }
};

$('btn-clear').onclick = () => {
  $('code-input').value = '';
  $('code-input').focus();
};

$('btn-save').onclick = async () => {
  if (!auth.user) { openAuth('login'); return; }
  const code = $('code-input').value;
  if (!code.trim()) { toast('Пустой код', 'error'); return; }

  try {
    if (currentProject) {
      // обновляем
      const r = await api('/api/projects/' + currentProject.id, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ pythonCode: code })
      });
      if (!r.ok) { toast('Ошибка сохранения', 'error'); return; }
      currentProject = await r.json();
      toast('Сохранено', 'success');
    } else {
      // создаём новый
      const name = prompt('Название проекта:', 'Проект ' + new Date().toLocaleString('ru-RU'));
      if (!name) return;
      const r = await api('/api/projects', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name, pythonCode: code })
      });
      if (!r.ok) { toast('Ошибка создания', 'error'); return; }
      currentProject = await r.json();
      toast('Проект создан', 'success');
    }
    isDirty = false;
    await loadMyProjects();
    renderProjectsList();
    renderSaveButtons();
  } catch (e) { toast('Ошибка: ' + e.message, 'error'); }
};

// ─────────────── Render session ───────────────
function showLoading() {
  $('empty-state').innerHTML = '<div class="empty">⏳ выполняется…</div>';
  $('empty-state').style.display = 'block';
  $('content').style.display = 'none';
}

function loadSession(data) {
  session = data;
  currentStep = 0;
  lastVars = {};
  selectedVar = null;
  snapshots = session.steps.map((s, i) => s.snapshot ? i : -1).filter(i => i >= 0);

  $('empty-state').style.display = 'none';
  $('content').style.display = 'block';

  renderStats();
  renderStepsList();
  $('steps-count').textContent = session.steps.length + ' шт.';

  const firstSnap = session.steps.findIndex(s => s.snapshot);
  renderStep(firstSnap >= 0 ? firstSnap : 0);
}

function renderStats() {
  const s = session.statistics;
  const items = [
    ['Шагов', s.total_steps],
    ['Сравнений', s.comparisons],
    ['Обменов', s.swaps],
    ['Обращений', s.memory_accesses],
  ];
  if (s.user_counters)
    for (const [k, v] of Object.entries(s.user_counters)) items.push([k, v]);
  if (s.structure_sizes)
    for (const [k, v] of Object.entries(s.structure_sizes)) items.push(['размер ' + k, v]);
  $('stats').innerHTML = items.map(([k, v]) =>
    `<div class="stat"><div class="label">${esc(k)}</div><div class="value">${esc(v)}</div></div>`).join('');
}

function renderStepsList() {
  const box = $('steps');
  box.innerHTML = session.steps.map((s, i) => {
    let d = '';
    if (s.kind === 'return' && s.annotation) {
      d = `<span class="ann">${esc(s.annotation)}</span>`;
    } else if (s.diff && s.diff.length) {
      d = s.diff.filter(x => !String(x.target).startsWith('compare.'))
        .map(x => `${esc(x.target)}=<span style="color:var(--text);">${esc(fmt(x.new))}</span>`).join(' ');
    }
    if (!d && s.annotation) d = `<span class="ann">${esc(s.annotation)}</span>`;
    if (!d && s.snapshot) d = '<span style="color:var(--text-muted);">snapshot</span>';
    if (!d) d = '&nbsp;';
    return `<div class="step" data-idx="${i}" data-kind="${s.kind}">
      <span class="n">#${s.n}</span>
      <span class="k">${s.kind}</span>
      <span class="d">${d}</span>
    </div>`;
  }).join('');
  box.querySelectorAll('.step').forEach(el => {
    el.onclick = () => renderStep(+el.dataset.idx);
  });
}

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
      let m = t.match(/^([A-Za-z_]\w*)\[(\d+)\]\[(\d+)\]$/);
      if (m) {
        const arr = state[m[1]];
        if (Array.isArray(arr) && Array.isArray(arr[+m[2]]))
          arr[+m[2]][+m[3]] = d.new;
        continue;
      }
      m = t.match(/^([A-Za-z_]\w*)\[(\d+)\]$/);
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

function hlClass(kind) {
  switch (kind) {
    case 'compare': return 'hl-cmp';
    case 'swap': return 'hl-swap';
    case 'mark': return 'hl-mark';
    case 'assign': return 'hl-mark';
    default: return 'hl-cmp';
  }
}

function buildHighlightSets(rawHighlights) {
  const cells = new Set(), rows = new Map(), vars = new Set();
  for (const h of (rawHighlights || [])) {
    let m = h.match(/^([A-Za-z_]\w*)\[(\d+)\]\[(\d+)\]$/);
    if (m) { cells.add(`${m[1]}[${m[2]}][${m[3]}]`); continue; }
    m = h.match(/^([A-Za-z_]\w*)\[(\d+)\]$/);
    if (m) {
      if (!rows.has(m[1])) rows.set(m[1], new Set());
      rows.get(m[1]).add(+m[2]);
      continue;
    }
    m = h.match(/^([A-Za-z_]\w*)$/);
    if (m) vars.add(m[1]);
  }
  return { cells, rows, vars };
}

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

  let annHtml = '';
  if (st.kind) {
    annHtml = `<span class="badge">${esc(st.kind)}</span>`;
    if (st.annotation) annHtml += `<span>${esc(st.annotation)}</span>`;
    else annHtml += `<span style="color:var(--text-muted);font-style:italic;">…</span>`;
  }
  $('annotation').innerHTML = annHtml;

  const state = stateAt(idx);
  const changed = new Set();
  if (st.diff) for (const d of st.diff) {
    const m = String(d.target).match(/^([A-Za-z_]\w*)(\[|\.|$)/);
    if (m) changed.add(m[1]);
  }
  const hl = buildHighlightSets(st.highlight);

  // список визуализируемых переменных
  const list = [];
  for (const [name, v] of Object.entries(state)) {
    if (name.startsWith('__')) continue;
    const shape = detectShape(v);
    if (shape === 'scalar') continue;
    list.push({ name, shape, value: v });
  }

  // выбор переменной
  const wantDefault = !selectedVar || !list.some(x => x.name === selectedVar);
  if (wantDefault) {
    const pref = ['A','arr','matrix','M','grid'];
    selectedVar = null;
    for (const p of pref) if (list.some(x => x.name === p)) { selectedVar = p; break; }
    if (!selectedVar && list.length) selectedVar = list[0].name;
  }

  renderVarChips(list);
  renderVisualization(list, hl, st);
  renderVars(st.vars || {}, changed);
}

function renderVarChips(list) {
  const box = $('viz-selector');
  if (!list.length) { box.innerHTML = ''; return; }
  box.innerHTML = list.map(item => `
    <div class="viz-chip ${item.name === selectedVar ? 'active' : ''}" data-name="${esc(item.name)}">
      ${esc(item.name)}
      <span class="kind-badge">${esc(item.shape)}</span>
    </div>
  `).join('');
  box.querySelectorAll('.viz-chip').forEach(el => {
    el.onclick = () => { selectedVar = el.dataset.name; renderStep(currentStep); };
  });
}

function renderVisualization(list, hl, st) {
  const vizBox = $('viz-content');
  const titleEl = $('viz-title');

  if (!list.length) {
    titleEl.textContent = 'Структура данных';
    vizBox.innerHTML = '<div style="color:var(--text-muted);font-style:italic;font-size:12px;">нет структур на этом шаге</div>';
    return;
  }
  const item = list.find(x => x.name === selectedVar) || list[0];
  const cls = hlClass(st.kind);

  switch (item.shape) {
    case 'array':
      titleEl.textContent = 'Массив ' + item.name;
      vizBox.innerHTML = renderArray(item.name, item.value, hl, cls);
      break;
    case 'matrix':
      titleEl.textContent = 'Матрица ' + item.name;
      vizBox.innerHTML = renderMatrix(item.name, item.value, hl, cls);
      break;
    case 'tree':
      titleEl.textContent = 'Дерево ' + item.name;
      vizBox.innerHTML = renderTreeSvg(item.value, hl);
      break;
    case 'set':
      titleEl.textContent = 'Множество ' + item.name;
      vizBox.innerHTML = renderSet(item.value);
      break;
    case 'dict':
      titleEl.textContent = 'Словарь ' + item.name;
      vizBox.innerHTML = renderDict(item.value);
      break;
    default:
      titleEl.textContent = item.name;
      vizBox.innerHTML = `<div style="font-family:var(--mono);">${esc(fmt(item.value))}</div>`;
  }
}

function renderArray(name, arr, hl, cls) {
  if (!arr.length) return '<div style="color:var(--text-muted);font-style:italic;">пустой массив</div>';
  const rows = hl.rows.get(name) || new Set();
  return '<div class="array">' + arr.map((v, i) =>
    `<div class="cell ${rows.has(i) ? cls : ''}">
      <span class="idx">${i}</span>${esc(fmt(v))}
    </div>`).join('') + '</div>';
}

function renderMatrix(name, m, hl, cls) {
  if (!m.length) return '<div style="color:var(--text-muted);font-style:italic;">пустая матрица</div>';
  const rows = hl.rows.get(name) || new Set();
  const cells = hl.cells;
  const cols = Math.max(...m.map(r => Array.isArray(r) ? r.length : 0));

  let header = '<tr><th></th>';
  for (let j = 0; j < cols; j++) header += `<th>${j}</th>`;
  header += '</tr>';

  let body = '';
  for (let i = 0; i < m.length; i++) {
    body += `<tr><th>${i}</th>`;
    const row = Array.isArray(m[i]) ? m[i] : [];
    for (let j = 0; j < cols; j++) {
      const val = j < row.length ? row[j] : null;
      const isHl = cells.has(`${name}[${i}][${j}]`) || rows.has(i);
      body += `<td class="mcell ${isHl ? cls : ''}">
        ${j < row.length ? esc(fmt(val)) : '<span style="color:var(--text-muted)">·</span>'}
      </td>`;
    }
    body += '</tr>';
  }

  return `<div class="matrix-wrap"><table class="matrix">
    <thead>${header}</thead><tbody>${body}</tbody></table></div>`;
}

function renderSet(s) {
  const items = s.items || [];
  if (!items.length) return '<div style="color:var(--text-muted);font-style:italic;">пустое множество</div>';
  return '<div style="display:flex;flex-wrap:wrap;gap:5px;justify-content:center;">' +
    items.map(v => `<div style="padding:5px 11px;background:var(--bg-3);border:2px solid var(--border-hi);border-radius:999px;font-family:var(--mono);font-size:13px;">${esc(fmt(v))}</div>`).join('') +
    '</div>';
}

function renderDict(d) {
  const entries = Object.entries(d).filter(([k]) => !k.startsWith('__'));
  if (!entries.length) return '<div style="color:var(--text-muted);font-style:italic;">пустой словарь</div>';
  return '<div style="display:flex;flex-direction:column;gap:4px;width:100%;max-width:480px;">' +
    entries.map(([k, v]) =>
      `<div style="display:grid;grid-template-columns:1fr 1fr;gap:10px;padding:7px 12px;background:var(--bg-3);border:1px solid var(--border-hi);border-radius:6px;font-family:var(--mono);font-size:12.5px;">
        <span style="color:#fbbf24;font-weight:600;">${esc(k)}</span>
        <span style="text-align:right;">${esc(fmt(v))}</span>
      </div>`).join('') +
    '</div>';
}

function renderTreeSvg(tree, hl) {
  let root = tree;
  if (tree && tree.__type === 'tree') root = tree.root;
  if (!root) return '<div style="color:var(--text-muted);font-style:italic;">дерево пустое</div>';

  const NODE_R = 20, H_GAP = 45, V_GAP = 68;
  const nodes = [];
  let inorderIdx = 0;
  function layout(node, depth) {
    if (!node) return;
    layout(node.left, depth + 1);
    nodes.push({ node, x: inorderIdx * (2 * NODE_R + H_GAP), y: depth * V_GAP + NODE_R + 18 });
    inorderIdx++;
    layout(node.right, depth + 1);
  }
  layout(root, 0);
  if (!nodes.length) return '';

  const width = Math.max(...nodes.map(n => n.x)) + 2 * NODE_R + 40;
  const height = Math.max(...nodes.map(n => n.y)) + 2 * NODE_R + 20;
  const posByNode = new Map();
  for (const n of nodes) posByNode.set(n.node, n);

  let edges = '';
  for (const n of nodes) {
    for (const side of ['left', 'right']) {
      if (n.node[side]) {
        const c = posByNode.get(n.node[side]);
        if (c) {
          const mx = (n.x + c.x) / 2, my = (n.y + c.y) / 2;
          edges += `<path class="tree-edge" d="M ${n.x} ${n.y} Q ${mx} ${my} ${c.x} ${c.y}"/>`;
        }
      }
    }
  }

  let circles = '';
  for (const n of nodes) {
    circles += `<g>
      <circle class="tree-node-circle" cx="${n.x}" cy="${n.y}" r="${NODE_R}"/>
      <text class="tree-node-text" x="${n.x}" y="${n.y}">${esc(fmt(n.node.value))}</text>
    </g>`;
  }

  return `<svg class="tree-svg" viewBox="0 0 ${width} ${height}" width="${Math.min(width, 800)}" height="${height}">
    ${edges}${circles}</svg>`;
}

function renderVars(vars, changed) {
  const entries = Object.entries(vars)
    .filter(([k]) => k === '__return__' || !k.startsWith('__'))
    .sort((a, b) => {
      // __return__ всегда в самом низу
      if (a[0] === '__return__') return 1;
      if (b[0] === '__return__') return -1;
      const aS = isScalar(a[1]) ? 0 : 1;
      const bS = isScalar(b[1]) ? 0 : 1;
      if (aS !== bS) return aS - bS;
      return a[0].localeCompare(b[0]);
    });

  const box = $('vars');
  if (!entries.length) {
    box.innerHTML = '<div style="color:var(--text-muted);font-size:12px;font-style:italic;padding:6px;">нет переменных</div>';
    return;
  }
    box.innerHTML = entries.map(([name, value]) => {
    // Переименовываем __return__ для отображения
    const isReturn = name === '__return__';
    const displayName = isReturn ? '↩ результат' : name;
    let t, display;
    if (isScalar(value)) { t = typeof value === 'number' ? (Number.isInteger(value) ? 'int' : 'float') : typeof value; display = fmt(value); }
    else if (looksLikeTree(value)) { t = 'tree'; display = 'tree'; }
    else if (value && typeof value === 'object' && value.__type) {
      t = value.__type;
      if (value.__type.startsWith('array')) {
        display = value.items ? '[' + value.items.map(x => fmt(x)).join(', ') + ']' : 'array[' + (value.length ?? '?') + ']';
      } else if (value.__type === 'set') {
        display = value.items ? '{' + value.items.map(x => fmt(x)).join(', ') + '}' : 'set';
      } else display = value.__type;
    } else { t = 'object'; display = fmt(value); }

    const reallyChanged = changed.has(name) && lastVars[name] !== undefined &&
      JSON.stringify(lastVars[name]) !== JSON.stringify(value);
    const sel = name === selectedVar;

    return `<div class="var-row ${reallyChanged ? 'changed' : ''} ${sel ? 'selected' : ''}" data-name="${esc(name)}">
      <span class="name">${esc(name)}<span class="type">${esc(t)}</span></span>
      <span class="value" title="${esc(display)}">${esc(display)}</span>
    </div>`;
  }).join('');
  box.querySelectorAll('.var-row').forEach(el => {
    el.onclick = () => {
      const n = el.dataset.name;
      if (!n) return;
      const s = stateAt(currentStep);
      const v = s[n];
      if (v === undefined || isScalar(v)) return;
      selectedVar = n;
      renderStep(currentStep);
    };
  });
  lastVars = Object.fromEntries(entries.filter(([, v]) => isScalar(v)));
}

// ─────────────── Controls ───────────────
$('btn-first').onclick = () => renderStep(0);
$('btn-last').onclick = () => renderStep(session.steps.length - 1);
$('btn-prev').onclick = () => renderStep(currentStep - 10);
$('btn-next').onclick = () => renderStep(currentStep + 10);
$('btn-prev1').onclick = () => renderStep(currentStep - 1);
$('btn-next1').onclick = () => renderStep(currentStep + 1);
$('btn-play').onclick = togglePlay;
$('timeline').onclick = (e) => {
  const rect = $('timeline').getBoundingClientRect();
  const p = (e.clientX - rect.left) / rect.width;
  renderStep(Math.round(p * (session.steps.length - 1)));
};

function togglePlay() {
  if (!session) return;
  isPlaying = !isPlaying;
  $('play-label').textContent = isPlaying ? 'Пауза' : 'Воспроизвести';
  $('play-icon').textContent = isPlaying ? '⏸' : '▶';
  if (isPlaying) {
    playTimer = setInterval(() => {
      if (currentStep >= session.steps.length - 1) {
        clearInterval(playTimer); isPlaying = false;
        $('play-label').textContent = 'Воспроизвести';
        $('play-icon').textContent = '▶';
        return;
      }
      renderStep(currentStep + 1);
    }, 150);
  } else clearInterval(playTimer);
}

document.addEventListener('keydown', (e) => {
  if (e.target && (e.target.tagName === 'TEXTAREA' || e.target.tagName === 'INPUT')) return;
  if ($('auth-modal').classList.contains('show')) return;
  if (!session) return;
  const step = e.shiftKey ? 10 : 1;
  if (e.key === 'ArrowLeft')  { renderStep(currentStep - step); e.preventDefault(); }
  if (e.key === 'ArrowRight') { renderStep(currentStep + step); e.preventDefault(); }
  if (e.key === ' ')          { togglePlay(); e.preventDefault(); }
});

// ─────────────── Init ───────────────
async function init() {
  // подтягиваем профиль, если есть токен
  if (auth.access) {
    try {
      const r = await fetch(API + '/api/auth/me', {
        headers: { 'Authorization': 'Bearer ' + auth.access }
      });
      if (r.ok) auth.user = await r.json();
      else if (auth.refresh) {
        const ok = await tryRefresh();
        if (ok) auth.user = await tryRefreshUser();
      }
    } catch {}
    if (auth.user) await loadMyProjects();
  }

  renderHeader();
  renderSidebar();
  renderSaveButtons();
  loadSamples();

  // URL-параметры: ?project=N или ?slug=X
  const url = new URL(window.location.href);
  const projectId = url.searchParams.get('project');
  const slug = url.searchParams.get('slug');

  if (slug) {
    try {
      const r = await fetch(API + '/api/public/projects/' + encodeURIComponent(slug));
      if (r.ok) {
        const p = await r.json();
        $('code-input').value = p.pythonCode || '';
        $('empty-state').innerHTML = `
          <div style="text-align:center;padding:40px 20px;color:var(--text-muted);">
            <div style="font-size:14px;color:var(--text-dim);margin-bottom:4px;">
              Публичный проект: <b style="color:var(--text);">${esc(p.name)}</b>
            </div>
            <div style="font-size:12px;">автор: ${esc(p.ownerUsername)}</div>
            <div style="font-size:12px;margin-top:12px;">Нажмите «Запустить», чтобы увидеть визуализацию</div>
          </div>`;
      }
    } catch {}
  } else if (projectId && auth.user) {
    await loadProject(+projectId);
  }
}

async function tryRefreshUser() {
  try {
    const r = await fetch(API + '/api/auth/me', {
      headers: { 'Authorization': 'Bearer ' + auth.access }
    });
    return r.ok ? await r.json() : null;
  } catch { return null; }
}

init();
</script>
</body>
</html>
""";
}