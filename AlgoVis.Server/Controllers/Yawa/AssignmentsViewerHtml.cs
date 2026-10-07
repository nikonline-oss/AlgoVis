namespace AlgoVis.Server.Controllers.Yawa;

/// <summary>
/// HTML-страница среды заданий. Отдаётся из YawaController GET /api/yawa/assignments-viewer.
/// Дев-страница для бэкендера; продакшн-фронт будет отдельно.
/// </summary>
internal static class AssignmentsViewerHtml
{
    public static readonly string Value = """
<!doctype html>
<html lang="ru">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>AlgoVis — задания</title>
<style>
  :root {
    --bg-0:#070a14; --bg-1:#0d1220; --bg-2:#141a2b; --bg-3:#1c2338;
    --border:#232a42; --border-hi:#2f3854;
    --text:#e8ecf4; --text-dim:#8b95ad; --text-muted:#5a6478;
    --accent:#6366f1; --accent-hi:#818cf8; --accent-glow:rgba(99,102,241,0.35);
    --pink:#ec4899; --danger:#ef4444; --ok:#10b981; --warn:#f59e0b;
    --gold:#fbbf24; --silver:#cbd5e1; --bronze:#d97706;
    --mono: ui-monospace,'SF Mono',Menlo,'JetBrains Mono',monospace;
    --sans: -apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;
  }
  *{box-sizing:border-box}
  html,body{margin:0;padding:0;color:var(--text);font-family:var(--sans);min-height:100vh;
    background:radial-gradient(1200px 600px at 20% -10%,#182042 0%,transparent 60%),
               radial-gradient(900px 500px at 100% 100%,#1a1230 0%,transparent 55%),
               var(--bg-0);font-size:14px;line-height:1.5}
  .app{max-width:1500px;margin:0 auto;padding:20px 24px 60px}

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
    cursor:pointer;transition:.15s;font-weight:500;text-decoration:none;display:inline-flex;
    align-items:center;gap:6px}
  .btn:hover{background:var(--bg-3);border-color:var(--border-hi)}
  .btn.primary{background:linear-gradient(135deg,var(--accent),#4f46e5);
    border-color:var(--accent);box-shadow:0 4px 14px -4px var(--accent-glow)}
  .btn.primary:hover{background:linear-gradient(135deg,var(--accent-hi),#6366f1)}
  .btn.ghost{background:transparent}
  .btn.small{padding:5px 10px;font-size:12px}
  .btn:disabled{opacity:.4;cursor:not-allowed}

  .layout{display:grid;grid-template-columns:340px 1fr;gap:18px;align-items:start}
  @media(max-width:1100px){.layout{grid-template-columns:1fr}}

  .card{background:linear-gradient(180deg,var(--bg-2),var(--bg-1));
    border:1px solid var(--border);border-radius:12px;padding:16px;margin-bottom:14px;
    box-shadow:0 4px 16px -8px rgba(0,0,0,.5)}
  .card:last-child{margin-bottom:0}
  .card-title{font-size:11px;font-weight:600;letter-spacing:.08em;text-transform:uppercase;
    color:var(--text-muted);margin:0 0 12px;display:flex;align-items:center;justify-content:space-between}

  .tabs{display:flex;gap:4px;margin-bottom:12px;background:var(--bg-1);padding:3px;border-radius:6px}
  .tab{flex:1;padding:6px 10px;border-radius:4px;background:transparent;border:none;
    color:var(--text-dim);font-family:inherit;font-size:12px;cursor:pointer;transition:.15s}
  .tab:hover{color:var(--text)}
  .tab.active{background:var(--bg-3);color:var(--text)}

  .item-list{display:flex;flex-direction:column;gap:4px}
  .item{display:flex;align-items:flex-start;gap:8px;padding:9px 11px;background:transparent;
    border:1px solid transparent;border-radius:6px;color:var(--text);font-size:13px;
    cursor:pointer;text-align:left;transition:.15s;width:100%;font-family:inherit;line-height:1.35}
  .item:hover{background:var(--bg-3);border-color:var(--border-hi)}
  .item.active{background:linear-gradient(135deg,rgba(99,102,241,.18),rgba(236,72,153,.10));
    border-color:var(--accent)}
  .item .icon{width:22px;height:22px;flex-shrink:0;border-radius:5px;background:var(--bg-3);
    display:flex;align-items:center;justify-content:center;font-size:11px;margin-top:1px}
  .item .body{flex:1;min-width:0}
  .item .name{display:block;font-weight:500;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
  .item .meta{display:block;font-size:11px;color:var(--text-muted);margin-top:2px}
  .item .badge{font-size:9px;padding:1px 5px;border-radius:3px;background:var(--bg-3);
    color:var(--text-muted);text-transform:uppercase;letter-spacing:.05em}
  .item .badge.passed{background:rgba(16,185,129,.2);color:var(--ok)}
  .item .badge.draft{background:rgba(245,158,11,.2);color:var(--warn)}
  .empty{color:var(--text-muted);font-size:12px;font-style:italic;padding:12px;text-align:center}

  .asg-header{margin-bottom:16px}
  .asg-header h2{margin:0 0 6px;font-size:20px;font-weight:600}
  .asg-meta{display:flex;flex-wrap:wrap;gap:10px;align-items:center;
    font-size:12px;color:var(--text-dim);margin-bottom:10px}
  .asg-meta .tag{padding:3px 9px;background:var(--bg-3);border-radius:4px;
    font-family:var(--mono);font-size:11px;color:var(--text-dim)}
  .asg-meta .tag.primary{background:rgba(99,102,241,.2);color:var(--accent-hi)}
  .asg-meta .tag.warn{background:rgba(245,158,11,.2);color:var(--warn)}
  .asg-desc{color:var(--text);font-size:13.5px;line-height:1.6;margin:8px 0 16px;
    padding:12px 14px;background:var(--bg-3);border-radius:6px;border-left:3px solid var(--accent)}

  .section-tabs{display:flex;gap:4px;margin-bottom:14px;background:var(--bg-1);padding:3px;border-radius:6px;width:fit-content}
  .section-tab{padding:6px 14px;border-radius:4px;background:transparent;border:none;
    color:var(--text-dim);font-family:inherit;font-size:12.5px;cursor:pointer;transition:.15s}
  .section-tab:hover{color:var(--text)}
  .section-tab.active{background:var(--bg-3);color:var(--text)}

  .code-area{width:100%;min-height:280px;padding:12px;background:var(--bg-0);
    border:1px solid var(--border);border-radius:8px;color:var(--text);
    font-family:var(--mono);font-size:12.5px;line-height:1.55;resize:vertical;
    tab-size:4;outline:none;transition:.15s}
  .code-area:focus{border-color:var(--accent);box-shadow:0 0 0 3px var(--accent-glow)}

  .actions{display:flex;gap:8px;margin-top:12px;flex-wrap:wrap}

  .result-panel{margin-top:16px;padding:16px;border-radius:10px;
    background:var(--bg-3);border:1px solid var(--border)}
  .result-panel.passed{border-left:4px solid var(--ok);background:linear-gradient(180deg,rgba(16,185,129,.08),var(--bg-3))}
  .result-panel.failed{border-left:4px solid var(--danger);background:linear-gradient(180deg,rgba(239,68,68,.08),var(--bg-3))}
  .result-panel.error{border-left:4px solid var(--warn);background:linear-gradient(180deg,rgba(245,158,11,.08),var(--bg-3))}

  .result-head{display:flex;align-items:center;gap:12px;margin-bottom:12px;flex-wrap:wrap}
  .result-head .grade{font-size:22px;font-weight:700;font-family:var(--mono);letter-spacing:.02em}
  .result-head .grade.perfect{color:var(--gold)}
  .result-head .grade.excellent{color:var(--accent-hi)}
  .result-head .grade.good{color:var(--ok)}
  .result-head .grade.failed,.result-head .grade.error{color:var(--danger)}
  .result-head .xp{margin-left:auto;font-family:var(--mono);
    padding:4px 12px;background:rgba(251,191,36,.15);border-radius:999px;
    color:var(--gold);font-size:13px;font-weight:600}

  .stats-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(110px,1fr));
    gap:10px;margin-bottom:14px}
  .stat{background:var(--bg-1);border:1px solid var(--border);border-radius:8px;padding:10px 12px}
  .stat .label{font-size:10px;color:var(--text-muted);text-transform:uppercase;
    letter-spacing:.06em;margin-bottom:3px}
  .stat .value{font-size:20px;font-weight:700;font-family:var(--mono);color:var(--text)}

  .diff-line{font-family:var(--mono);font-size:12px;padding:8px 12px;
    background:var(--bg-1);border-radius:6px;margin-bottom:6px;line-height:1.5}
  .diff-line .label{color:var(--text-muted);margin-right:8px}
  .diff-line.expected .value{color:var(--ok)}
  .diff-line.actual .value{color:var(--danger)}

  .criteria{margin-top:12px}
  .criteria .row{display:flex;align-items:center;gap:10px;padding:6px 10px;
    border-radius:5px;font-size:12px;font-family:var(--mono)}
  .criteria .row + .row{margin-top:3px}
  .criteria .row.ok{background:rgba(16,185,129,.10);color:#a7f3d0}
  .criteria .row.no{background:rgba(239,68,68,.10);color:#fecaca}
  .criteria .row.skip{background:var(--bg-1);color:var(--text-muted)}
  .criteria .icon{width:16px;text-align:center;font-weight:700}

  .error-banner{display:none;margin-top:12px;padding:10px 14px;
    background:linear-gradient(90deg,rgba(239,68,68,.18),rgba(239,68,68,.05));
    border-left:3px solid var(--danger);border-radius:6px;
    font-family:var(--mono);font-size:12.5px;color:#fecaca;white-space:pre-wrap}
  .error-banner.show{display:block}

  .modal-overlay{position:fixed;inset:0;background:rgba(0,0,0,.78);backdrop-filter:blur(4px);
    display:none;align-items:flex-start;justify-content:center;z-index:1000;padding:20px;
    overflow-y:auto}
  .modal-overlay.show{display:flex}
  .modal{background:linear-gradient(180deg,var(--bg-2),var(--bg-1));
    border:1px solid var(--border);border-radius:14px;padding:26px;
    max-width:520px;width:100%;box-shadow:0 20px 60px -20px rgba(0,0,0,.8);
    max-height:calc(100vh - 40px);overflow-y:auto;margin:auto}
  .modal.wide{max-width:880px}
  .modal h2{margin:0 0 6px;font-size:20px;font-weight:600}
  .modal .sub{color:var(--text-dim);font-size:13px;margin-bottom:20px}
  .modal .field{margin-bottom:14px}
  .modal label{display:block;font-size:12px;color:var(--text-dim);margin-bottom:6px;font-weight:500}
  .modal input,.modal textarea,.modal select{width:100%;padding:10px 12px;
    background:var(--bg-0);border:1px solid var(--border);border-radius:7px;
    color:var(--text);font-family:inherit;font-size:13px;outline:none;transition:.15s}
  .modal textarea{font-family:var(--mono);font-size:12.5px;min-height:100px;resize:vertical}
  .modal input:focus,.modal textarea:focus,.modal select:focus{
    border-color:var(--accent);box-shadow:0 0 0 3px var(--accent-glow)}
  .modal .row2{display:grid;grid-template-columns:1fr 1fr;gap:10px}
  .modal .row3{display:grid;grid-template-columns:1fr 1fr 1fr;gap:10px}
  .modal .actions{display:flex;gap:8px;margin-top:20px}
  .modal .actions .btn{flex:1;justify-content:center;padding:10px}
  .modal .error{display:none;margin-bottom:14px;padding:8px 12px;
    background:rgba(239,68,68,.15);border-left:3px solid var(--danger);
    border-radius:5px;color:#fecaca;font-size:12px}
  .modal .error.show{display:block}
  .modal .hint{font-size:11px;color:var(--text-muted);margin-top:4px}
  .modal .checkbox-row{display:flex;align-items:center;gap:8px;margin:10px 0}
  .modal .checkbox-row input{width:auto;}

  .preview-result{margin-top:14px;padding:14px;background:var(--bg-3);
    border-radius:8px;border-left:3px solid var(--ok);display:none}
  .preview-result.show{display:block}
  .preview-result.error{border-left-color:var(--danger);background:rgba(239,68,68,.08)}

  .toast{position:fixed;bottom:24px;right:24px;padding:12px 20px;background:var(--bg-2);
    border:1px solid var(--border);border-left:3px solid var(--accent);
    border-radius:8px;font-size:13px;box-shadow:0 10px 30px -10px rgba(0,0,0,.7);
    transform:translateY(100px);opacity:0;transition:.3s;z-index:2000;max-width:380px}
  .toast.show{transform:translateY(0);opacity:1}
  .toast.success{border-left-color:var(--ok)}
  .toast.error{border-left-color:var(--danger)}

  .empty-state{text-align:center;padding:60px 20px;color:var(--text-muted)}
  .empty-state .icon{font-size:48px;margin-bottom:12px;opacity:.5}
  .empty-state .title{font-size:15px;color:var(--text-dim);margin-bottom:4px}

  .loading{display:inline-block;width:16px;height:16px;border:2px solid var(--border-hi);
    border-top-color:var(--accent);border-radius:50%;animation:spin .7s linear infinite}
  @keyframes spin{to{transform:rotate(360deg)}}

  /* Comments */
  .comment{padding:10px 12px;background:var(--bg-1);border-radius:6px;margin-bottom:8px;
    border-left:3px solid var(--border-hi)}
  .comment.mine{border-left-color:var(--accent)}
  .comment .head{display:flex;align-items:center;gap:8px;margin-bottom:6px;font-size:12px}
  .comment .head .author{font-weight:600;color:var(--text)}
  .comment .head .role{font-size:9px;padding:1px 5px;border-radius:3px;background:var(--bg-3);
    color:var(--text-muted);text-transform:uppercase;letter-spacing:.05em}
  .comment .head .role.teacher{background:rgba(99,102,241,.2);color:var(--accent-hi)}
  .comment .head .time{color:var(--text-muted);font-size:11px;margin-left:auto}
  .comment .head .del{background:transparent;border:none;color:var(--text-muted);
    cursor:pointer;font-size:11px;padding:2px 6px;border-radius:3px}
  .comment .head .del:hover{color:var(--danger);background:rgba(239,68,68,.1)}
  .comment .body{font-size:13px;line-height:1.5;white-space:pre-wrap;word-break:break-word}
  .comment-form{display:flex;gap:8px;margin-top:10px}
  .comment-form input{flex:1;padding:8px 12px;background:var(--bg-0);
    border:1px solid var(--border);border-radius:6px;color:var(--text);
    font-family:inherit;font-size:13px;outline:none}
  .comment-form input:focus{border-color:var(--accent)}
</style>
</head>
<body>

<div class="app">
  <header>
    <div class="brand">
      <div class="logo">🎯</div>
      <div>
        <h1>AlgoVis — Задания</h1>
        <p class="sub">Отправляйте решения, смотрите результаты и рейтинг</p>
      </div>
    </div>
    <div class="header-right" id="header-right"></div>
  </header>

  <div class="layout">
    <aside>
      <div class="card">
        <div class="tabs">
          <button class="tab active" data-tab="available">Доступные</button>
          <button class="tab" data-tab="mine">Мои</button>
          <button class="tab" data-tab="leaderboard">Рейтинг</button>
        </div>

        <div id="tab-available">
          <div class="item-list" id="list-available">
            <div class="empty"><span class="loading"></span></div>
          </div>
        </div>

        <div id="tab-mine" style="display:none;">
          <button class="btn primary small" style="width:100%;margin-bottom:8px;"
                  id="btn-create">+ Создать задание</button>
          <div class="item-list" id="list-mine"></div>
        </div>

        <div id="tab-leaderboard" style="display:none;">
          <div style="display:flex;gap:4px;margin-bottom:10px;">
            <button class="btn small" id="lb-players" style="flex:1;">Игроки</button>
            <button class="btn small" id="lb-teachers" style="flex:1;">Преподаватели</button>
          </div>
          <div class="item-list" id="list-leaderboard"></div>
        </div>
      </div>
    </aside>

    <main id="main">
      <div class="empty-state" id="empty-state">
        <div class="icon">🎯</div>
        <div class="title">Выберите задание слева</div>
        <div style="font-size:12px;">или создайте своё во вкладке «Мои»</div>
      </div>

      <div id="content" style="display:none;">
        <div class="card">
          <div class="asg-header" id="asg-header"></div>
          <div class="asg-desc" id="asg-desc"></div>

          <div id="author-actions" style="display:none;gap:8px;margin-bottom:12px;flex-wrap:wrap;">
            <button class="btn small" id="btn-toggle-publish">—</button>
            <button class="btn small" id="btn-edit">✎ Редактировать</button>
            <button class="btn small" id="btn-delete" style="color:#fecaca;">✕ Удалить</button>
          </div>

          <div class="section-tabs" id="section-tabs">
            <button class="section-tab active" data-section="solve">Решение</button>
            <button class="section-tab" data-section="submissions">Отправки</button>
            <button class="section-tab" data-section="leaderboard">Рейтинг</button>
            <button class="section-tab" data-section="comments">Обсуждение</button>
          </div>

          <div id="section-solve">
            <label style="display:block;font-size:12px;color:var(--text-dim);
                          margin-bottom:6px;font-weight:500;">
              <span id="solve-label">Ваше решение</span>
            </label>
            <textarea id="code-input" class="code-area" spellcheck="false"></textarea>

            <div class="error-banner" id="error-banner"></div>

            <div class="actions" id="solve-actions">
              <button class="btn primary" id="btn-submit">▶ Отправить решение</button>
              <button class="btn" id="btn-reset">⟲ Сбросить</button>
              <button class="btn ghost" id="btn-my-subs">Мои отправки</button>
            </div>
          </div>

          <div id="section-submissions" style="display:none;">
            <div id="subs-area"></div>
          </div>

          <div id="section-leaderboard" style="display:none;">
            <div id="lb-assignment-area"></div>
          </div>

          <div id="section-comments" style="display:none;">
            <div id="comments-area"></div>
          </div>
        </div>

        <div id="result-area"></div>
      </div>
    </main>
  </div>
</div>

<!-- Модалка логина -->
<div class="modal-overlay" id="auth-modal">
  <div class="modal">
    <h2 id="auth-title">Вход</h2>
    <div class="sub" id="auth-sub">Войдите, чтобы работать с заданиями</div>
    <div class="error" id="auth-error"></div>
    <div class="field" id="field-username" style="display:none;">
      <label>Имя пользователя</label>
      <input type="text" id="input-username" autocomplete="username">
    </div>
    <div class="field">
      <label>Email</label>
      <input type="email" id="input-email" autocomplete="email">
    </div>
    <div class="field">
      <label>Пароль</label>
      <input type="password" id="input-password" autocomplete="current-password">
    </div>
    <div class="actions">
      <button class="btn primary" id="btn-auth-submit">Войти</button>
      <button class="btn ghost" id="btn-auth-close">Отмена</button>
    </div>
  </div>
</div>

<!-- Модалка создания/редактирования задания -->
<div class="modal-overlay" id="asg-modal">
  <div class="modal wide">
    <h2 id="asg-modal-title">Создать задание</h2>
    <div class="sub">Заполните поля, проверьте эталон, опубликуйте</div>
    <div class="error" id="asg-error"></div>

    <div class="field">
      <label>Название *</label>
      <input type="text" id="asg-title" placeholder="Например: Сортировка пузырьком">
    </div>

    <div class="field">
      <label>Описание</label>
      <textarea id="asg-description" style="min-height:80px;"
                placeholder="Что нужно сделать?"></textarea>
    </div>

    <div class="row2">
      <div class="field">
        <label>Режим</label>
        <select id="asg-mode">
          <option value="visual">visual — язык не ограничен</option>
          <option value="code">code — только разрешённые языки</option>
        </select>
      </div>
      <div class="field">
        <label>Целевая переменная *</label>
        <input type="text" id="asg-compare-target" value="A"
               placeholder="A или __return__">
        <div class="hint">Какую переменную сравнивать с эталоном</div>
      </div>
    </div>

    <div class="field">
      <label>Эталонное решение * (python)</label>
      <textarea id="asg-reference" style="min-height:180px;"
                placeholder="def main():\n    A = [5,2,8,1]\n    ..."></textarea>
      <div class="hint">Сервер запустит этот код и вычислит ожидаемый результат</div>
    </div>

    <div class="field">
      <label>Шаблон для ученика (опционально)</label>
      <textarea id="asg-template" style="min-height:100px;"
                placeholder="def sort(A):\n    # ваш код здесь\n    pass"></textarea>
    </div>

    <div class="field" id="field-allowed-langs" style="display:none;">
      <label>Разрешённые языки (через запятую)</label>
      <input type="text" id="asg-langs" placeholder="python, csharp">
    </div>

    <div class="field">
      <label>Критерии грейдов (опционально)</label>
      <div class="row3">
        <input type="number" id="asg-good-cmp" placeholder="good: макс. сравнений">
        <input type="number" id="asg-exc-cmp" placeholder="excellent: сравнений">
        <input type="number" id="asg-perf-cmp" placeholder="perfect: сравнений">
      </div>
      <div class="row3" style="margin-top:8px;">
        <input type="number" id="asg-good-swap" placeholder="good: макс. обменов">
        <input type="number" id="asg-exc-swap" placeholder="excellent: обменов">
        <input type="number" id="asg-perf-swap" placeholder="perfect: обменов">
      </div>
      <div class="hint">Пустое поле — критерий не проверяется для этого грейда</div>
    </div>

    <div class="row2">
      <div class="field">
        <label>Макс. шагов</label>
        <input type="number" id="asg-max-steps" value="100000">
      </div>
      <div class="field">
        <label>Макс. секунд</label>
        <input type="number" id="asg-max-seconds" value="5" step="0.1">
      </div>
    </div>

    <div class="field">
      <label>Дедлайн (опционально)</label>
      <input type="datetime-local" id="asg-deadline">
    </div>

    <div class="checkbox-row">
      <input type="checkbox" id="asg-public" checked>
      <label for="asg-public" style="margin:0;">Публичное (видно всем авторизованным)</label>
    </div>

    <div class="preview-result" id="asg-preview"></div>

    <div class="actions">
      <button class="btn" id="btn-preview">🧪 Проверить эталон</button>
      <button class="btn primary" id="btn-asg-save">💾 Сохранить</button>
      <button class="btn ghost" id="btn-asg-close">Отмена</button>
    </div>
  </div>
</div>

<div class="toast" id="toast"></div>

<script>
// ─────────────── State ───────────────
const API = '';
let auth = {
  access: localStorage.getItem('algovis_access') || null,
  refresh: localStorage.getItem('algovis_refresh') || null,
  user: null
};
let currentAssignment = null;
let isMine = false;
let currentSection = 'solve';
let availableList = [];
let mineList = [];
let editingAssignment = null;   // объект при редактировании

const $ = (id) => document.getElementById(id);

// ─────────────── Utils ───────────────
function esc(s) {
  return String(s ?? '').replace(/[&<>"']/g, c =>
    ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}
function fmt(x) {
  if (x === null || x === undefined) return 'null';
  if (typeof x === 'object') return JSON.stringify(x);
  return String(x);
}
function toast(msg, kind) {
  const el = $('toast');
  el.textContent = msg;
  el.className = 'toast ' + (kind || '');
  el.classList.add('show');
  setTimeout(() => el.classList.remove('show'), 3000);
}

// ─────────────── API ───────────────
async function api(path, opts = {}, retry = true) {
  opts.headers = opts.headers || {};
  if (auth.access) opts.headers['Authorization'] = 'Bearer ' + auth.access;
  if (opts.body && typeof opts.body === 'string' && !opts.headers['Content-Type'])
    opts.headers['Content-Type'] = 'application/json';
  const r = await fetch(API + path, opts);
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
      method: 'POST', headers: {'Content-Type':'application/json'},
      body: JSON.stringify({ refreshToken: auth.refresh })
    });
    if (!r.ok) return false;
    const d = await r.json();
    setAuth(d.accessToken, d.refreshToken, d.user);
    return true;
  } catch { return false; }
}
function setAuth(a, r, u) {
  auth.access = a; auth.refresh = r;
  if (u) auth.user = u;
  if (a) localStorage.setItem('algovis_access', a); else localStorage.removeItem('algovis_access');
  if (r) localStorage.setItem('algovis_refresh', r); else localStorage.removeItem('algovis_refresh');
  renderHeader();
}
function logout() {
  if (auth.refresh) {
    fetch(API + '/api/auth/logout', {
      method: 'POST', headers: {'Content-Type':'application/json'},
      body: JSON.stringify({ refreshToken: auth.refresh })
    }).catch(()=>{});
  }
  auth = { access:null, refresh:null, user:null };
  localStorage.removeItem('algovis_access');
  localStorage.removeItem('algovis_refresh');
  renderHeader();
  showEmpty('Войдите, чтобы видеть задания');
}

// ─────────────── Header ───────────────
function renderHeader() {
  const box = $('header-right');
  if (auth.user) {
    const initials = (auth.user.username || '?').slice(0, 1).toUpperCase();
    box.innerHTML = `
      <a href="/api/yawa/viewer" class="btn ghost" title="Визуализатор">⚡ Визуализатор</a>
      <span style="font-size:11px;color:var(--text-muted);font-family:var(--mono);">${esc(auth.user.email)}</span>
      <button class="user-chip" id="btn-logout">
        <span class="avatar">${esc(initials)}</span>
        <span>${esc(auth.user.username)}</span>
      </button>`;
    $('btn-logout').onclick = () => { if (confirm('Выйти?')) logout(); };
  } else {
    box.innerHTML = `
      <a href="/api/yawa/viewer" class="btn ghost">⚡ Визуализатор</a>
      <button class="btn" id="btn-login">Войти</button>
      <button class="btn primary" id="btn-register">Регистрация</button>`;
    $('btn-login').onclick = () => openAuth('login');
    $('btn-register').onclick = () => openAuth('register');
  }
}

// ─────────────── Auth modal ───────────────
let authMode = 'login';
function openAuth(mode) {
  authMode = mode;
  $('auth-modal').classList.add('show');
  $('auth-title').textContent = mode === 'login' ? 'Вход' : 'Регистрация';
  $('field-username').style.display = mode === 'register' ? 'block' : 'none';
  $('btn-auth-submit').textContent = mode === 'login' ? 'Войти' : 'Зарегистрироваться';
  $('auth-error').classList.remove('show');
}
function closeAuth(){ $('auth-modal').classList.remove('show'); $('input-password').value=''; }
async function submitAuth() {
  const email = $('input-email').value.trim();
  const password = $('input-password').value;
  const username = $('input-username').value.trim();
  const errBox = $('auth-error'); errBox.classList.remove('show');
  try {
    const url = authMode === 'login' ? '/api/auth/login' : '/api/auth/register';
    const body = authMode === 'login' ? {email,password} : {email,username,password};
    const r = await fetch(API + url, {method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify(body)});
    const d = await r.json();
    if (!r.ok) { errBox.textContent = d.error || 'Ошибка'; errBox.classList.add('show'); return; }
    setAuth(d.accessToken, d.refreshToken, d.user);
    closeAuth();
    toast(authMode === 'login' ? 'Вход выполнен' : 'Регистрация успешна', 'success');
    await loadAll();
  } catch (e) { errBox.textContent = 'Сеть: ' + e.message; errBox.classList.add('show'); }
}
$('btn-auth-submit').onclick = submitAuth;
$('btn-auth-close').onclick = closeAuth;
$('auth-modal').addEventListener('click', e => { if (e.target === $('auth-modal')) closeAuth(); });
document.addEventListener('keydown', e => {
  if (e.key === 'Enter' && $('auth-modal').classList.contains('show')) submitAuth();
});

// ─────────────── Tabs ───────────────
document.querySelectorAll('.tab').forEach(t => {
  t.onclick = () => {
    document.querySelectorAll('.tab').forEach(x => x.classList.remove('active'));
    t.classList.add('active');
    const w = t.dataset.tab;
    $('tab-available').style.display = w === 'available' ? 'block' : 'none';
    $('tab-mine').style.display = w === 'mine' ? 'block' : 'none';
    $('tab-leaderboard').style.display = w === 'leaderboard' ? 'block' : 'none';
    if (w === 'leaderboard') loadLeaderboard('players');
  };
});
document.querySelectorAll('.section-tab').forEach(t => {
  t.onclick = () => {
    document.querySelectorAll('.section-tab').forEach(x => x.classList.remove('active'));
    t.classList.add('active');
    const s = t.dataset.section;
    currentSection = s;
    $('section-solve').style.display = s === 'solve' ? 'block' : 'none';
    $('section-submissions').style.display = s === 'submissions' ? 'block' : 'none';
    $('section-leaderboard').style.display = s === 'leaderboard' ? 'block' : 'none';
    $('section-comments').style.display = s === 'comments' ? 'block' : 'none';
    if (s === 'submissions') loadSubmissions();
    if (s === 'leaderboard') loadAssignmentLeaderboard();
    if (s === 'comments') loadComments();
  };
});

// ─────────────── Load ───────────────
async function loadAll() {
  if (!auth.user) return;
  await Promise.all([loadAvailable(), loadMine()]);
}
async function loadAvailable() {
  const r = await api('/api/assignments');
  if (!r.ok) return;
  availableList = await r.json();
  renderAvailableList();
}
async function loadMine() {
  const r = await api('/api/assignments/mine');
  if (!r.ok) return;
  mineList = await r.json();
  renderMineList();
}
function renderAvailableList() {
  const box = $('list-available');
  if (!availableList.length) { box.innerHTML = '<div class="empty">нет доступных заданий</div>'; return; }
  box.innerHTML = availableList.map(a => `
    <button class="item ${currentAssignment?.id === a.id && !isMine ? 'active' : ''}"
            data-id="${a.id}" data-kind="available">
      <span class="icon">📝</span>
      <span class="body">
        <span class="name">${esc(a.title)}</span>
        <span class="meta">${esc(a.authorUsername)} · ${a.submissionsCount} попыток · ${a.passedCount} прошло
          ${a.mySubmissionPassed ? ' · <span class="badge passed">✓</span>' : ''}</span>
      </span></button>`).join('');
  box.querySelectorAll('.item').forEach(el => el.onclick = () => openAssignment(+el.dataset.id, false));
}
function renderMineList() {
  const box = $('list-mine');
  if (!mineList.length) { box.innerHTML = '<div class="empty">нет созданных заданий</div>'; return; }
  box.innerHTML = mineList.map(a => `
    <button class="item ${currentAssignment?.id === a.id && isMine ? 'active' : ''}"
            data-id="${a.id}" data-kind="mine">
      <span class="icon">${a.isPublished ? '✅' : '📄'}</span>
      <span class="body">
        <span class="name">${esc(a.title)}</span>
        <span class="meta">${a.submissionsCount} попыток · ${a.passedCount} прошло
          ${a.isPublished ? '' : ' · <span class="badge draft">черновик</span>'}</span>
      </span></button>`).join('');
  box.querySelectorAll('.item').forEach(el => el.onclick = () => openAssignment(+el.dataset.id, true));
}

// ─────────────── Open ───────────────
async function openAssignment(id, mine) {
  isMine = mine;
  const url = mine ? `/api/assignments/${id}/mine` : `/api/assignments/${id}`;
  const r = await api(url);
  if (!r.ok) { toast('Не удалось загрузить', 'error'); return; }
  currentAssignment = await r.json();
  currentAssignment.__mine = mine;
  currentSection = 'solve';
  document.querySelectorAll('.section-tab').forEach(x =>
    x.classList.toggle('active', x.dataset.section === 'solve'));
  $('section-solve').style.display = 'block';
  $('section-submissions').style.display = 'none';
  $('section-leaderboard').style.display = 'none';
  $('section-comments').style.display = 'none';

  renderAssignment();
  $('empty-state').style.display = 'none';
  $('content').style.display = 'block';
  $('result-area').innerHTML = '';
}

function renderAssignment() {
  const a = currentAssignment;
  const tags = [
    `<span class="tag primary">${esc(a.mode)}</span>`,
    `<span class="tag">автор: ${esc(a.authorUsername)}</span>`,
    `<span class="tag">сравнивается: ${esc(a.compareTarget)}</span>`,
    `<span class="tag">max ${a.maxSteps} шагов / ${a.maxSeconds}с</span>`
  ];
  if (a.deadline) {
    const d = new Date(a.deadline);
    const past = d < new Date();
    tags.push(`<span class="tag ${past ? 'warn' : ''}">дедлайн: ${d.toLocaleString('ru-RU')}${past ? ' (истёк)' : ''}</span>`);
  }
  if (a.mode === 'code' && a.allowedLanguages?.length)
    tags.push(`<span class="tag">языки: ${a.allowedLanguages.map(esc).join(', ')}</span>`);

  $('asg-header').innerHTML = `
    <h2>${esc(a.title)}</h2>
    <div class="asg-meta">${tags.join('')}</div>
    <div style="font-size:12px;color:var(--text-muted);margin-top:6px;">
      Попыток: ${a.submissionsCount ?? 0} · Прошло: ${a.passedCount ?? 0}
      ${a.mySubmissionPassed ? ' · <span style="color:var(--ok);font-weight:600;">✓ вы уже сдали</span>' : ''}
      ${a.myBestGrade ? ' · лучший грейд: <b>' + esc(a.myBestGrade) + '</b>' : ''}
    </div>`;

  $('asg-desc').textContent = a.description || 'Описание не указано';

  const ta = $('code-input');
  if (a.__mine) {
    ta.value = a.referenceSolution || '';
    $('solve-label').textContent = 'Ваше эталонное решение (для справки)';
  } else {
    ta.value = a.templateCode || '';
    $('solve-label').textContent = 'Ваше решение';
  }

  $('btn-submit').style.display = a.__mine ? 'none' : 'inline-flex';
  $('btn-reset').style.display = a.__mine ? 'none' : 'inline-flex';
  $('btn-my-subs').style.display = a.__mine ? 'inline-flex' : 'none';
  $('btn-my-subs').textContent = 'Мои отправки';

  // Author actions
  const authActions = $('author-actions');
  if (a.__mine) {
    authActions.style.display = 'flex';
    $('btn-toggle-publish').textContent = a.isPublished ? '📥 Снять с публикации' : '📤 Опубликовать';
    $('btn-toggle-publish').onclick = togglePublish;
    $('btn-edit').onclick = () => openEditAssignment(a);
    $('btn-delete').onclick = deleteAssignment;
  } else {
    authActions.style.display = 'none';
  }
}

// ─────────────── Solve ───────────────
$('btn-submit').onclick = async () => {
  if (!currentAssignment || currentAssignment.__mine) return;
  const code = $('code-input').value;
  if (!code.trim()) { toast('Пустой код', 'error'); return; }
  $('error-banner').classList.remove('show');
  $('result-area').innerHTML = '<div class="card"><div class="empty"><span class="loading"></span> выполняется…</div></div>';
  const r = await api(`/api/assignments/${currentAssignment.id}/submit`, {
    method: 'POST',
    headers: {'Content-Type':'application/json'},
    body: JSON.stringify({language:'python', code})
  });
  const d = await r.json();
  if (!r.ok) {
    $('result-area').innerHTML = '';
    $('error-banner').textContent = d.error || 'Ошибка';
    $('error-banner').classList.add('show');
    return;
  }
  renderSubmissionResult(d);
  await loadAvailable();
};
$('btn-reset').onclick = () => {
  if (!currentAssignment || currentAssignment.__mine) return;
  $('code-input').value = currentAssignment.templateCode || '';
  $('result-area').innerHTML = '';
  $('error-banner').classList.remove('show');
};
$('btn-my-subs').onclick = () => {
  // Переключаемся на раздел отправок
  document.querySelectorAll('.section-tab').forEach(x =>
    x.classList.toggle('active', x.dataset.section === 'submissions'));
  currentSection = 'submissions';
  $('section-solve').style.display = 'none';
  $('section-submissions').style.display = 'block';
  $('section-leaderboard').style.display = 'none';
  $('section-comments').style.display = 'none';
  loadSubmissions();
};

// ─────────────── Result rendering ───────────────
function renderSubmissionResult(sub) {
  const area = $('result-area');
  const cls = sub.status === 'passed' ? 'passed' :
              sub.status === 'error' ? 'error' : 'failed';
  const result = sub.result;

  let grade = '';
  if (sub.grade) grade = `<span class="grade ${esc(sub.grade)}">${esc(sub.grade.toUpperCase())}</span>`;
  else if (sub.status === 'failed') grade = '<span class="grade failed">НЕ ПРОШЛО</span>';
  else if (sub.status === 'error') grade = '<span class="grade error">ОШИБКА</span>';

  const xp = sub.xpAwarded > 0 ? `<span class="xp">+${sub.xpAwarded} XP</span>` : '';

  let stats = '';
  if (result?.stats) {
    stats = `<div class="stats-grid">
      <div class="stat"><div class="label">Шагов</div><div class="value">${result.stats.totalSteps}</div></div>
      <div class="stat"><div class="label">Сравнений</div><div class="value">${result.stats.comparisons}</div></div>
      <div class="stat"><div class="label">Обменов</div><div class="value">${result.stats.swaps}</div></div>
      <div class="stat"><div class="label">Обращений</div><div class="value">${result.stats.memoryAccesses}</div></div>
    </div>`;
  }

  let diff = '';
  if (result && (result.expectedResult !== undefined || result.actualResult !== undefined)) {
    diff = `
      <div class="diff-line expected"><span class="label">Ожидалось:</span><span class="value">${esc(fmt(result.expectedResult))}</span></div>
      <div class="diff-line actual"><span class="label">Получено:</span><span class="value">${esc(fmt(result.actualResult))}</span></div>`;
  }

  let criteria = '';
  if (result?.criteriaChecked) {
    criteria = '<div class="criteria">' +
      Object.entries(result.criteriaChecked).map(([k, v]) => {
        const label = k === 'result' ? 'результат совпадает' :
                      k === 'good' ? 'грейд good' :
                      k === 'excellent' ? 'грейд excellent' :
                      k === 'perfect' ? 'грейд perfect' : k;
        return `<div class="row ${v ? 'ok' : 'no'}"><span class="icon">${v ? '✓' : '✕'}</span><span>${esc(label)}</span></div>`;
      }).join('') + '</div>';
  }

  const reason = result?.reason
    ? `<div style="margin-top:12px;color:#fecaca;font-size:12.5px;padding:10px 12px;background:rgba(239,68,68,.10);border-radius:6px;">${esc(result.reason)}</div>`
    : '';

  const err = sub.errorMessage
    ? `<div style="margin-top:12px;color:#fecaca;font-family:var(--mono);font-size:12.5px;padding:10px 12px;background:rgba(239,68,68,.10);border-radius:6px;">${esc(sub.errorMessage)}</div>`
    : '';

  area.innerHTML = `<div class="card">
    <div class="result-panel ${cls}">
      <div class="result-head">${grade}${xp}</div>
      ${stats}${diff}${criteria}${reason}${err}
    </div>
    <div style="margin-top:10px;">
      <button class="btn small" id="btn-show-comments">💬 Обсудить (${sub.commentsCount || 0})</button>
    </div>
  </div>`;

  const btn = $('btn-show-comments');
  if (btn) {
    btn.onclick = () => {
      currentSubmissionId = sub.id;
      document.querySelectorAll('.section-tab').forEach(x =>
        x.classList.toggle('active', x.dataset.section === 'comments'));
      currentSection = 'comments';
      $('section-solve').style.display = 'none';
      $('section-submissions').style.display = 'none';
      $('section-leaderboard').style.display = 'none';
      $('section-comments').style.display = 'block';
      loadComments();
    };
  }
}

// ─────────────── Submissions list ───────────────
async function loadSubmissions() {
  const area = $('subs-area');
  area.innerHTML = '<div class="empty"><span class="loading"></span></div>';

  const url = currentAssignment.__mine
    ? `/api/assignments/${currentAssignment.id}/submissions`
    : '/api/submissions/mine';
  const r = await api(url);
  if (!r.ok) { area.innerHTML = '<div class="empty">не удалось загрузить</div>'; return; }
  const subs = await r.json();

  if (!subs.length) { area.innerHTML = '<div class="empty">нет отправок</div>'; return; }

  const rows = subs.map(s => `
    <tr style="border-bottom:1px solid var(--border);cursor:pointer;"
        onclick='openSubmission(${s.id})'>
      <td style="padding:8px 10px;font-family:var(--mono);color:var(--text-muted);">#${s.id}</td>
      <td style="padding:8px 10px;">${esc(s.username || auth.user.username)}</td>
      <td style="padding:8px 10px;font-family:var(--mono);">
        <span style="color:${s.status === 'passed' ? 'var(--ok)' : s.status === 'failed' ? 'var(--danger)' : 'var(--warn)'};">
          ${esc(s.status)}</span></td>
      <td style="padding:8px 10px;font-family:var(--mono);">${s.grade ? esc(s.grade) : '—'}</td>
      <td style="padding:8px 10px;font-family:var(--mono);color:${s.xpAwarded ? 'var(--gold)' : 'var(--text-muted)'};">
        ${s.xpAwarded ? '+' + s.xpAwarded : '0'}</td>
      <td style="padding:8px 10px;color:var(--text-muted);font-size:11px;">
        ${new Date(s.createdAt).toLocaleString('ru-RU')}</td>
    </tr>`).join('');

  area.innerHTML = `<table style="width:100%;border-collapse:collapse;font-size:13px;">
    <thead><tr style="color:var(--text-muted);font-size:11px;text-transform:uppercase;
      letter-spacing:.05em;text-align:left;">
      <th style="padding:6px 10px;">ID</th><th style="padding:6px 10px;">Ученик</th>
      <th style="padding:6px 10px;">Статус</th><th style="padding:6px 10px;">Грейд</th>
      <th style="padding:6px 10px;">XP</th><th style="padding:6px 10px;">Когда</th>
    </tr></thead><tbody>${rows}</tbody></table>`;
}

window.openSubmission = async (id) => {
  currentSubmissionId = id;
  const r = await api('/api/submissions/' + id);
  if (!r.ok) { toast('Не удалось', 'error'); return; }
  const sub = await r.json();
  // Показываем в разделе solve
  document.querySelectorAll('.section-tab').forEach(x =>
    x.classList.toggle('active', x.dataset.section === 'solve'));
  currentSection = 'solve';
  $('section-solve').style.display = 'block';
  $('section-submissions').style.display = 'none';
  $('section-leaderboard').style.display = 'none';
  $('section-comments').style.display = 'none';
  $('code-input').value = sub.code;
  renderSubmissionResult(sub);
};

// ─────────────── Assignment leaderboard ───────────────
async function loadAssignmentLeaderboard() {
  const area = $('lb-assignment-area');
  area.innerHTML = '<div class="empty"><span class="loading"></span></div>';
  const r = await api(`/api/assignments/${currentAssignment.id}/leaderboard`);
  if (!r.ok) { area.innerHTML = '<div class="empty">не удалось загрузить</div>'; return; }
  const list = await r.json();
  if (!list.length) { area.innerHTML = '<div class="empty">пока никто не решил</div>'; return; }
  area.innerHTML = list.map(e => `
    <div class="item" style="cursor:default;">
      <span class="icon" style="background:${
        e.rank === 1 ? 'linear-gradient(135deg,var(--gold),var(--bronze))' :
        e.rank === 2 ? 'linear-gradient(135deg,var(--silver),#94a3b8)' :
        e.rank === 3 ? 'linear-gradient(135deg,var(--bronze),#92400e)' :
        'var(--bg-3)'
      };color:${e.rank <= 3 ? '#0a0a0a' : 'var(--text-dim)'};font-weight:700;">
        ${e.rank}</span>
      <span class="body">
        <span class="name">${esc(e.username)}</span>
        <span class="meta">
          ${e.grade ? '<b style="color:var(--gold);">' + esc(e.grade) + '</b> · ' : ''}
          ${e.comparisons} сравн. · ${e.swaps} обм. · ${e.totalSteps} шагов
        </span>
      </span>
    </div>`).join('');
}

// ─────────────── Comments ───────────────
let currentSubmissionId = null;

async function loadComments() {
  const area = $('comments-area');
  if (!currentSubmissionId) {
    area.innerHTML = `
      <div class="empty" style="padding:20px;">
        <div style="margin-bottom:8px;">Выберите отправку в разделе «Отправки»</div>
        <div style="font-size:11px;color:var(--text-muted);">
          Обсуждение привязано к конкретной попытке решения
        </div>
      </div>`;
    return;
  }
  area.innerHTML = '<div class="empty"><span class="loading"></span></div>';
  const r = await api(`/api/submissions/${currentSubmissionId}/comments`);
  if (!r.ok) { area.innerHTML = '<div class="empty">нет доступа</div>'; return; }
  const list = await r.json();

  const listHtml = list.length ? list.map(c => {
    const isMine = c.authorId === auth.user?.id;
    return `<div class="comment ${isMine ? 'mine' : ''}">
      <div class="head">
        <span class="author">${esc(c.authorUsername)}</span>
        <span class="role ${c.authorRole === 'teacher' ? 'teacher' : ''}">${esc(c.authorRole)}</span>
        <span class="time">${new Date(c.createdAt).toLocaleString('ru-RU')}</span>
        ${isMine ? `<button class="del" onclick="delComment(${c.id})">удалить</button>` : ''}
      </div>
      <div class="body">${esc(c.text)}</div>
    </div>`;
  }).join('') : '<div class="empty">пока нет комментариев</div>';

  area.innerHTML = `
    ${listHtml}
    <div class="comment-form">
      <input type="text" id="new-comment" placeholder="Ваш комментарий…"
             onkeydown="if(event.key==='Enter') sendComment()">
      <button class="btn primary small" onclick="sendComment()">Отправить</button>
    </div>`;
}

window.sendComment = async () => {
  const inp = $('new-comment');
  if (!inp) return;
  const text = inp.value.trim();
  if (!text) return;
  const r = await api(`/api/submissions/${currentSubmissionId}/comments`, {
    method: 'POST',
    headers: {'Content-Type':'application/json'},
    body: JSON.stringify({text})
  });
  if (!r.ok) { toast('Не удалось', 'error'); return; }
  inp.value = '';
  await loadComments();
};

window.delComment = async (id) => {
  if (!confirm('Удалить комментарий?')) return;
  const r = await api('/api/comments/' + id, {method:'DELETE'});
  if (!r.ok) { toast('Не удалось', 'error'); return; }
  await loadComments();
};

// ─────────────── Global leaderboard ───────────────
async function loadLeaderboard(mode) {
  $('list-leaderboard').innerHTML = '<div class="empty"><span class="loading"></span></div>';
  $('lb-players').classList.toggle('primary', mode === 'players');
  $('lb-teachers').classList.toggle('primary', mode === 'teachers');
  const url = mode === 'players'
    ? '/api/leaderboard/players?limit=30'
    : '/api/leaderboard/teachers?limit=30';
  const r = await fetch(API + url);
  const list = await r.json();
  const box = $('list-leaderboard');
  if (!list.length) { box.innerHTML = '<div class="empty">пока пусто</div>'; return; }
  box.innerHTML = list.map(e => `
    <div class="item" style="cursor:default;">
      <span class="icon" style="background:${
        e.rank === 1 ? 'linear-gradient(135deg,var(--gold),var(--bronze))' :
        e.rank === 2 ? 'linear-gradient(135deg,var(--silver),#94a3b8)' :
        e.rank === 3 ? 'linear-gradient(135deg,var(--bronze),#92400e)' :
        'var(--bg-3)'
      };color:${e.rank <= 3 ? '#0a0a0a' : 'var(--text-dim)'};font-weight:700;">
        ${e.rank}</span>
      <span class="body">
        <span class="name">${esc(e.username)}</span>
        <span class="meta">
          <b style="color:var(--accent-hi);">${e.score}</b>
          ${mode === 'players'
            ? ' XP · уровень ' + e.level + ' · ' + e.completedOrCreatedCount + ' заданий'
            : ' очков · уровень ' + e.level + ' · ' + e.completedOrCreatedCount + ' создано'}
        </span>
      </span>
    </div>`).join('');
}
$('lb-players').onclick = () => loadLeaderboard('players');
$('lb-teachers').onclick = () => loadLeaderboard('teachers');

// ─────────────── Author actions ───────────────
async function togglePublish() {
  const a = currentAssignment;
  const url = `/api/assignments/${a.id}/publish`;
  const method = a.isPublished ? 'DELETE' : 'POST';
  const r = await api(url, {method});
  const d = await r.json();
  if (!r.ok) { toast(d.error || 'Ошибка', 'error'); return; }
  toast(a.isPublished ? 'Снято с публикации' : 'Опубликовано', 'success');
  await openAssignment(a.id, true);
  await loadMine();
}

async function deleteAssignment() {
  if (!confirm('Удалить задание? Все отправки будут потеряны.')) return;
  const r = await api('/api/assignments/' + currentAssignment.id, {method:'DELETE'});
  if (!r.ok) { toast('Не удалось', 'error'); return; }
  toast('Удалено', 'success');
  currentAssignment = null;
  $('content').style.display = 'none';
  $('empty-state').style.display = 'block';
  await loadAll();
}

// ─────────────── Create / Edit modal ───────────────
function openCreateAssignment() {
  editingAssignment = null;
  $('asg-modal-title').textContent = 'Создать задание';
  $('asg-title').value = '';
  $('asg-description').value = '';
  $('asg-mode').value = 'visual';
  $('asg-compare-target').value = 'A';
  $('asg-reference').value = '';
  $('asg-template').value = '';
  $('asg-langs').value = '';
  $('asg-good-cmp').value = '';
  $('asg-good-swap').value = '';
  $('asg-exc-cmp').value = '';
  $('asg-exc-swap').value = '';
  $('asg-perf-cmp').value = '';
  $('asg-perf-swap').value = '';
  $('asg-max-steps').value = '100000';
  $('asg-max-seconds').value = '5';
  $('asg-deadline').value = '';
  $('asg-public').checked = true;
  $('field-allowed-langs').style.display = 'none';
  $('asg-preview').classList.remove('show');
  $('asg-error').classList.remove('show');
  $('asg-modal').classList.add('show');
}

function openEditAssignment(a) {
  editingAssignment = a;
  $('asg-modal-title').textContent = 'Редактировать задание';
  $('asg-title').value = a.title || '';
  $('asg-description').value = a.description || '';
  $('asg-mode').value = a.mode || 'visual';
  $('asg-compare-target').value = a.compareTarget || 'A';
  $('asg-reference').value = a.referenceSolution || '';
  $('asg-template').value = a.templateCode || '';
  $('asg-langs').value = (a.allowedLanguages || []).join(', ');
  $('asg-max-steps').value = a.maxSteps || 100000;
  $('asg-max-seconds').value = a.maxSeconds || 5;
  $('asg-public').checked = a.isPublic !== false;

  // Парсим GradingRules
  let rules = {};
  try { rules = JSON.parse(a.gradingRules || '{}'); } catch {}
  $('asg-good-cmp').value = rules.good?.maxComparisons ?? '';
  $('asg-good-swap').value = rules.good?.maxSwaps ?? '';
  $('asg-exc-cmp').value = rules.excellent?.maxComparisons ?? '';
  $('asg-exc-swap').value = rules.excellent?.maxSwaps ?? '';
  $('asg-perf-cmp').value = rules.perfect?.maxComparisons ?? '';
  $('asg-perf-swap').value = rules.perfect?.maxSwaps ?? '';

  $('field-allowed-langs').style.display = a.mode === 'code' ? 'block' : 'none';
  $('asg-preview').classList.remove('show');
  $('asg-error').classList.remove('show');
  $('asg-modal').classList.add('show');
}

function closeAsgModal() {
  $('asg-modal').classList.remove('show');
}

$('asg-mode').onchange = (e) => {
  $('field-allowed-langs').style.display = e.target.value === 'code' ? 'block' : 'none';
};

function buildGradingRules() {
  const rules = {};
  const gc = $('asg-good-cmp').value, gs = $('asg-good-swap').value;
  const ec = $('asg-exc-cmp').value, es = $('asg-exc-swap').value;
  const pc = $('asg-perf-cmp').value, ps = $('asg-perf-swap').value;

  const g = {};
  if (gc) g.maxComparisons = +gc;
  if (gs) g.maxSwaps = +gs;
  if (Object.keys(g).length) rules.good = g;

  const e = {};
  if (ec) e.maxComparisons = +ec;
  if (es) e.maxSwaps = +es;
  if (Object.keys(e).length) rules.excellent = e;

  const p = {};
  if (pc) p.maxComparisons = +pc;
  if (ps) p.maxSwaps = +ps;
  if (Object.keys(p).length) rules.perfect = p;

  return Object.keys(rules).length ? JSON.stringify(rules) : null;
}

$('btn-preview').onclick = async () => {
  const ref = $('asg-reference').value;
  const target = $('asg-compare-target').value || 'A';
  const errBox = $('asg-error'); errBox.classList.remove('show');
  if (!ref.trim()) { errBox.textContent = 'Введите эталонное решение'; errBox.classList.add('show'); return; }

  const box = $('asg-preview');
  box.classList.remove('show', 'error');
  box.textContent = 'Проверяем…';
  box.classList.add('show');

  const r = await api('/api/assignments/preview', {
    method: 'POST',
    headers: {'Content-Type':'application/json'},
    body: JSON.stringify({
      referenceSolution: ref,
      language: 'python',
      compareTarget: target,
      maxSteps: +$('asg-max-steps').value || 100000,
      maxSeconds: +$('asg-max-seconds').value || 5
    })
  });
  const d = await r.json();

  if (!d.success) {
    box.classList.add('error');
    box.innerHTML = `<div style="color:#fecaca;font-family:var(--mono);font-size:12px;">
      <b>Ошибка:</b> ${esc(d.error)}${d.line != null ? ' (строка ' + (d.line+1) + ')' : ''}</div>`;
    return;
  }

  box.innerHTML = `
    <div style="font-size:12px;color:var(--ok);font-weight:600;margin-bottom:8px;">✓ Эталон работает</div>
    <div style="font-family:var(--mono);font-size:12px;line-height:1.6;">
      <div><span style="color:var(--text-muted);">Результат (${esc(target)}):</span>
        <b style="color:var(--ok);">${esc(fmt(d.actualResult))}</b></div>
      <div><span style="color:var(--text-muted);">Шагов:</span> ${d.stats.totalSteps}</div>
      <div><span style="color:var(--text-muted);">Сравнений:</span> ${d.stats.comparisons}</div>
      <div><span style="color:var(--text-muted);">Обменов:</span> ${d.stats.swaps}</div>
    </div>
    <div class="hint" style="margin-top:8px;">
      Совет: заполните поля критериев ниже, опираясь на эти числа
    </div>`;
};

$('btn-asg-save').onclick = async () => {
  const errBox = $('asg-error'); errBox.classList.remove('show');

  const title = $('asg-title').value.trim();
  const reference = $('asg-reference').value;
  const target = $('asg-compare-target').value.trim();

  if (!title) { errBox.textContent = 'Введите название'; errBox.classList.add('show'); return; }
  if (!reference.trim()) { errBox.textContent = 'Введите эталонное решение'; errBox.classList.add('show'); return; }
  if (!target) { errBox.textContent = 'Укажите целевую переменную'; errBox.classList.add('show'); return; }

  const langs = $('asg-langs').value.split(',').map(s => s.trim()).filter(s => s);

  const body = {
    title,
    description: $('asg-description').value || null,
    mode: $('asg-mode').value,
    allowedLanguages: langs,
    referenceSolution: reference,
    referenceLanguage: 'python',
    compareTarget: target,
    gradingRules: buildGradingRules(),
    templateCode: $('asg-template').value || null,
    maxSteps: +$('asg-max-steps').value || 100000,
    maxSeconds: +$('asg-max-seconds').value || 5,
    isPublished: false,
    isPublic: $('asg-public').checked,
    deadline: $('asg-deadline').value ? new Date($('asg-deadline').value).toISOString() : null
  };

  let r, d;
  if (editingAssignment) {
    r = await api('/api/assignments/' + editingAssignment.id, {
      method: 'PATCH',
      headers: {'Content-Type':'application/json'},
      body: JSON.stringify(body)
    });
  } else {
    r = await api('/api/assignments', {
      method: 'POST',
      headers: {'Content-Type':'application/json'},
      body: JSON.stringify(body)
    });
  }
  d = await r.json();
  if (!r.ok) { errBox.textContent = d.error || 'Ошибка'; errBox.classList.add('show'); return; }

  toast(editingAssignment ? 'Задание обновлено' : 'Задание создано', 'success');
  closeAsgModal();
  await loadMine();
  await openAssignment(d.id, true);
};

$('btn-asg-close').onclick = closeAsgModal;
$('asg-modal').addEventListener('click', e => {
  if (e.target === $('asg-modal')) closeAsgModal();
});

$('btn-create').onclick = openCreateAssignment;

// ─────────────── Helpers ───────────────
function showEmpty(msg) {
  $('content').style.display = 'none';
  $('empty-state').style.display = 'block';
  $('empty-state').innerHTML = `<div class="icon">🎯</div><div class="title">${esc(msg)}</div>`;
}

// ─────────────── Init ───────────────
async function init() {
  if (auth.access) {
    try {
      const r = await fetch(API + '/api/auth/me', {
        headers: {'Authorization': 'Bearer ' + auth.access}
      });
      if (r.ok) auth.user = await r.json();
      else if (auth.refresh) {
        const ok = await tryRefresh();
        if (ok) {
          const r2 = await fetch(API + '/api/auth/me', {
            headers: {'Authorization': 'Bearer ' + auth.access}
          });
          if (r2.ok) auth.user = await r2.json();
        }
      }
    } catch {}
  }
  renderHeader();
  if (auth.user) await loadAll();
  else showEmpty('Войдите, чтобы видеть задания');
}
init();
</script>
</body>
</html>
""";
}