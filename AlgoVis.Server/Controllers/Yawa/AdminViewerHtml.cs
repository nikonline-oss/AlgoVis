namespace AlgoVis.Server.Controllers.Yawa;

/// <summary>
/// HTML-страница админ-панели. Отдаётся из YawaController GET /api/yawa/admin-viewer.
/// </summary>
internal static class AdminViewerHtml
{
    public static readonly string Value = """
<!doctype html>
<html lang="ru">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>AlgoVis — Админ-панель</title>
<style>
  :root {
    --bg-0:#070a14; --bg-1:#0d1220; --bg-2:#141a2b; --bg-3:#1c2338;
    --border:#232a42; --border-hi:#2f3854;
    --text:#e8ecf4; --text-dim:#8b95ad; --text-muted:#5a6478;
    --accent:#6366f1; --accent-hi:#818cf8; --accent-glow:rgba(99,102,241,0.35);
    --pink:#ec4899; --danger:#ef4444; --ok:#10b981; --warn:#f59e0b;
    --gold:#fbbf24;
    --mono: ui-monospace,'SF Mono',Menlo,'JetBrains Mono',monospace;
    --sans: -apple-system,BlinkMacSystemFont,'Segoe UI',Roboto,sans-serif;
  }
  *{box-sizing:border-box}
  html,body{margin:0;padding:0;color:var(--text);font-family:var(--sans);min-height:100vh;
    background:radial-gradient(1200px 600px at 20% -10%,#182042 0%,transparent 60%),
               radial-gradient(900px 500px at 100% 100%,#1a1230 0%,transparent 55%),
               var(--bg-0);font-size:14px;line-height:1.5}
  .app{max-width:1600px;margin:0 auto;padding:20px 24px 60px}

  header{display:flex;align-items:center;justify-content:space-between;
    margin-bottom:20px;padding-bottom:16px;border-bottom:1px solid var(--border)}
  .brand{display:flex;align-items:center;gap:12px}
  .logo{width:38px;height:38px;border-radius:10px;
    background:linear-gradient(135deg,var(--danger),var(--pink));
    display:flex;align-items:center;justify-content:center;font-size:20px;
    box-shadow:0 6px 20px -6px rgba(239,68,68,0.4)}
  .brand h1{margin:0;font-size:19px;font-weight:600}
  .brand .sub{margin:2px 0 0;font-size:12px;color:var(--text-dim)}
  .header-right{display:flex;gap:10px;align-items:center}
  .btn{padding:8px 16px;border-radius:8px;border:1px solid var(--border);
    background:var(--bg-2);color:var(--text);font-family:inherit;font-size:13px;
    cursor:pointer;transition:.15s;font-weight:500;text-decoration:none;
    display:inline-flex;align-items:center;gap:6px}
  .btn:hover{background:var(--bg-3);border-color:var(--border-hi)}
  .btn.primary{background:linear-gradient(135deg,var(--accent),#4f46e5);
    border-color:var(--accent);box-shadow:0 4px 14px -4px var(--accent-glow)}
  .btn.danger{background:rgba(239,68,68,.15);border-color:rgba(239,68,68,.4);color:#fecaca}
  .btn.danger:hover{background:rgba(239,68,68,.25)}
  .btn.small{padding:4px 10px;font-size:11px;border-radius:5px}
  .btn:disabled{opacity:.4;cursor:not-allowed}

  .stats-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(150px,1fr));
    gap:12px;margin-bottom:24px}
  .stat{background:linear-gradient(180deg,var(--bg-2),var(--bg-1));
    border:1px solid var(--border);border-radius:10px;padding:14px 16px;
    position:relative;overflow:hidden}
  .stat::before{content:'';position:absolute;top:0;left:0;right:0;height:2px;
    background:linear-gradient(90deg,var(--accent),var(--pink));opacity:.7}
  .stat.warn::before{background:linear-gradient(90deg,var(--warn),var(--danger))}
  .stat .label{font-size:11px;color:var(--text-muted);text-transform:uppercase;
    letter-spacing:.06em;margin-bottom:4px}
  .stat .value{font-size:24px;font-weight:700;font-family:var(--mono);
    background:linear-gradient(135deg,var(--text),var(--text-dim));
    -webkit-background-clip:text;-webkit-text-fill-color:transparent;background-clip:text}

  .tabs{display:flex;gap:4px;margin-bottom:16px;background:var(--bg-1);padding:4px;border-radius:8px;
    width:fit-content}
  .tab{padding:8px 18px;border-radius:6px;background:transparent;border:none;
    color:var(--text-dim);font-family:inherit;font-size:13px;cursor:pointer;transition:.15s;font-weight:500}
  .tab:hover{color:var(--text)}
  .tab.active{background:var(--bg-3);color:var(--text)}

  .card{background:linear-gradient(180deg,var(--bg-2),var(--bg-1));
    border:1px solid var(--border);border-radius:12px;padding:16px;
    box-shadow:0 4px 16px -8px rgba(0,0,0,.5)}

  .toolbar{display:flex;gap:8px;margin-bottom:14px;flex-wrap:wrap;align-items:center}
  .search-input{flex:1;min-width:200px;padding:9px 14px;background:var(--bg-0);
    border:1px solid var(--border);border-radius:8px;color:var(--text);
    font-family:inherit;font-size:13px;outline:none;transition:.15s}
  .search-input:focus{border-color:var(--accent);box-shadow:0 0 0 3px var(--accent-glow)}
  .filter-select{padding:9px 12px;background:var(--bg-0);border:1px solid var(--border);
    border-radius:8px;color:var(--text);font-family:inherit;font-size:13px;cursor:pointer}

  table{width:100%;border-collapse:collapse;font-size:13px}
  th{text-align:left;padding:10px 12px;font-size:11px;font-weight:600;
    text-transform:uppercase;letter-spacing:.06em;color:var(--text-muted);
    border-bottom:1px solid var(--border)}
  td{padding:10px 12px;border-bottom:1px solid var(--border);vertical-align:middle}
  tr:hover td{background:rgba(99,102,241,.04)}
  tr:last-child td{border-bottom:none}

  .badge{display:inline-block;padding:2px 8px;border-radius:999px;font-size:10px;
    font-weight:600;text-transform:uppercase;letter-spacing:.05em}
  .badge.admin{background:rgba(239,68,68,.2);color:#fecaca}
  .badge.teacher{background:rgba(99,102,241,.2);color:var(--accent-hi)}
  .badge.student{background:rgba(139,149,173,.15);color:var(--text-dim)}
  .badge.published{background:rgba(16,185,129,.2);color:#a7f3d0}
  .badge.draft{background:rgba(245,158,11,.2);color:#fde68a}
  .badge.passed{background:rgba(16,185,129,.2);color:#a7f3d0}
  .badge.failed{background:rgba(239,68,68,.15);color:#fecaca}
  .badge.error{background:rgba(245,158,11,.2);color:#fde68a}
  .badge.blocked{background:rgba(239,68,68,.15);color:#fecaca}

  .mono{font-family:var(--mono);font-size:12px;color:var(--text-dim)}
  .num{font-family:var(--mono);font-weight:600}
  .num.xp{color:var(--gold)}
  .num.rating{color:var(--accent-hi)}

  .actions-cell{display:flex;gap:4px;flex-wrap:wrap}

  .pagination{display:flex;gap:8px;align-items:center;justify-content:center;
    padding:14px;font-size:13px;color:var(--text-dim)}
  .pagination button{padding:6px 14px;background:var(--bg-2);border:1px solid var(--border);
    border-radius:6px;color:var(--text);cursor:pointer;font-family:inherit;font-size:12px}
  .pagination button:hover:not(:disabled){background:var(--bg-3);border-color:var(--border-hi)}
  .pagination button:disabled{opacity:.4;cursor:not-allowed}

  .empty{text-align:center;padding:40px 20px;color:var(--text-muted);font-style:italic}

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

  .user-cell{display:flex;align-items:center;gap:10px}
  .avatar{width:32px;height:32px;border-radius:50%;flex-shrink:0;
    background:linear-gradient(135deg,var(--accent),var(--pink));
    display:flex;align-items:center;justify-content:center;
    font-size:13px;font-weight:700;color:white}
  .user-cell .meta{display:flex;flex-direction:column;min-width:0}
  .user-cell .name{font-weight:500;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
  .user-cell .email{font-size:11px;color:var(--text-muted);font-family:var(--mono);
    overflow:hidden;text-overflow:ellipsis;white-space:nowrap}
</style>
</head>
<body>

<div class="app">
  <header>
    <div class="brand">
      <div class="logo">🛡</div>
      <div>
        <h1>AlgoVis — Админ-панель</h1>
        <p class="sub">Управление пользователями, проектами и заданиями</p>
      </div>
    </div>
    <div class="header-right" id="header-right"></div>
  </header>

  <div id="content" style="display:none;">
    <div class="stats-grid" id="stats"></div>

    <div class="tabs">
      <button class="tab active" data-tab="users">Пользователи</button>
      <button class="tab" data-tab="projects">Проекты</button>
      <button class="tab" data-tab="assignments">Задания</button>
      <button class="tab" data-tab="submissions">Отправки</button>
    </div>

    <div class="card">
      <div id="tab-content"></div>
    </div>
  </div>

  <div class="empty-state" id="empty-state">
    <div class="icon">🛡</div>
    <div class="title">Проверка доступа…</div>
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
let currentTab = 'users';
let state = {
  users: { page: 1, pageSize: 20, search: '', role: '' },
  projects: { page: 1, pageSize: 20, search: '' },
  assignments: { page: 1, pageSize: 20, search: '' },
  submissions: { page: 1, pageSize: 20, status: '' }
};

const $ = (id) => document.getElementById(id);

// ─────────────── Utils ───────────────
function esc(s) {
  return String(s ?? '').replace(/[&<>"']/g, c =>
    ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
}
function toast(msg, kind) {
  const el = $('toast');
  el.textContent = msg;
  el.className = 'toast ' + (kind || '');
  el.classList.add('show');
  setTimeout(() => el.classList.remove('show'), 3000);
}
function fmtDate(d) {
  if (!d) return '—';
  const dt = new Date(d);
  return dt.toLocaleString('ru-RU', {
    year: 'numeric', month: '2-digit', day: '2-digit',
    hour: '2-digit', minute: '2-digit'
  });
}
function initials(s) {
  return (s || '?').slice(0, 1).toUpperCase();
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
    auth.access = d.accessToken;
    auth.refresh = d.refreshToken;
    localStorage.setItem('algovis_access', d.accessToken);
    localStorage.setItem('algovis_refresh', d.refreshToken);
    return true;
  } catch { return false; }
}

// ─────────────── Header ───────────────
function renderHeader() {
  const box = $('header-right');
  if (auth.user) {
    const inits = initials(auth.user.username);
    box.innerHTML = `
      <a href="/api/yawa/viewer" class="btn">⚡ Визуализатор</a>
      <a href="/api/yawa/assignments-viewer" class="btn">🎯 Задания</a>
      <span style="display:flex;align-items:center;gap:8px;">
        <span class="avatar">${esc(inits)}</span>
        <span style="font-size:13px;">${esc(auth.user.username)}</span>
        <span class="badge admin">admin</span>
      </span>
      <button class="btn" id="btn-logout">Выйти</button>`;
    $('btn-logout').onclick = () => {
      localStorage.removeItem('algovis_access');
      localStorage.removeItem('algovis_refresh');
      location.href = '/api/yawa/assignments-viewer';
    };
  }
}

// ─────────────── Tabs ───────────────
document.querySelectorAll('.tab').forEach(t => {
  t.onclick = () => {
    document.querySelectorAll('.tab').forEach(x => x.classList.remove('active'));
    t.classList.add('active');
    currentTab = t.dataset.tab;
    renderTab();
  };
});

async function renderTab() {
  if (currentTab === 'users') return loadUsers();
  if (currentTab === 'projects') return loadProjects();
  if (currentTab === 'assignments') return loadAssignments();
  if (currentTab === 'submissions') return loadSubmissions();
}

// ─────────────── Stats ───────────────
async function loadStats() {
  const r = await api('/api/admin/stats');
  if (!r.ok) { toast('Не удалось загрузить статистику', 'error'); return; }
  const s = await r.json();

  const items = [
    ['Пользователей', s.totalUsers],
    ['Активных', s.activeUsers, s.activeUsers < s.totalUsers ? 'warn' : ''],
    ['Студентов', s.studentCount],
    ['Преподавателей', s.teacherCount],
    ['Админов', s.adminCount],
    ['Проектов', s.totalProjects],
    ['Публичных проектов', s.publicProjects],
    ['Заданий', s.totalAssignments],
    ['Опубликовано', s.publishedAssignments],
    ['Отправок', s.totalSubmissions],
    ['Прошло', s.passedSubmissions, 'warn'],
    ['Комментариев', s.totalComments],
    ['Активных сессий', s.activeRefreshTokens]
  ];

  $('stats').innerHTML = items.map(([label, value, cls]) => `
    <div class="stat ${cls || ''}">
      <div class="label">${esc(label)}</div>
      <div class="value">${esc(value)}</div>
    </div>`).join('');
}

// ─────────────── Users ───────────────
async function loadUsers() {
  const s = state.users;
  const url = `/api/admin/users?page=${s.page}&pageSize=${s.pageSize}` +
    (s.search ? `&search=${encodeURIComponent(s.search)}` : '') +
    (s.role ? `&role=${s.role}` : '');

  const r = await api(url);
  if (!r.ok) { toast('Ошибка загрузки', 'error'); return; }
  const data = await r.json();

  const rows = data.users.map(u => {
    const roleBadge = `<span class="badge ${u.role}">${esc(u.role)}</span>`;
    const activeBadge = u.isActive ? '' : '<span class="badge blocked">заблокирован</span>';

    return `<tr>
      <td>
        <div class="user-cell">
          <span class="avatar">${esc(initials(u.username))}</span>
          <span class="meta">
            <span class="name">${esc(u.username)} ${activeBadge}</span>
            <span class="email">${esc(u.email)}</span>
          </span>
        </div>
      </td>
      <td>${roleBadge}</td>
      <td class="num">${u.projectsCount}</td>
      <td class="num">${u.assignmentsCount}</td>
      <td class="num">${u.submissionsCount}</td>
      <td class="num xp">${u.playerXp}</td>
      <td class="num rating">${u.teacherRating}</td>
      <td class="mono">${fmtDate(u.lastLoginAt)}</td>
      <td>
        <div class="actions-cell">
          <select class="filter-select" style="padding:4px 8px;font-size:11px;"
                  onchange="changeRole(${u.id}, this.value)">
            <option value="student" ${u.role === 'student' ? 'selected' : ''}>student</option>
            <option value="teacher" ${u.role === 'teacher' ? 'selected' : ''}>teacher</option>
            <option value="admin" ${u.role === 'admin' ? 'selected' : ''}>admin</option>
          </select>
          <button class="btn small ${u.isActive ? '' : 'primary'}"
                  onclick="toggleActive(${u.id}, ${u.isActive})">
            ${u.isActive ? '🚫' : '✓'}
          </button>
          <button class="btn small danger" onclick="deleteUser(${u.id}, '${esc(u.username)}')">✕</button>
        </div>
      </td>
    </tr>`;
  }).join('');

  const toolbar = `
    <div class="toolbar">
      <input class="search-input" type="text" placeholder="Поиск по email или имени..."
             value="${esc(s.search)}" oninput="onUserSearch(this.value)">
      <select class="filter-select" onchange="onUserRoleFilter(this.value)">
        <option value="">Все роли</option>
        <option value="student" ${s.role === 'student' ? 'selected' : ''}>Студенты</option>
        <option value="teacher" ${s.role === 'teacher' ? 'selected' : ''}>Преподаватели</option>
        <option value="admin" ${s.role === 'admin' ? 'selected' : ''}>Админы</option>
      </select>
    </div>`;

  const pagination = renderPagination(
    'users', data.page, data.pageSize, data.totalCount);

  $('tab-content').innerHTML = toolbar +
    (data.users.length ? `
      <div style="overflow-x:auto;">
        <table>
          <thead><tr>
            <th>Пользователь</th><th>Роль</th><th>Проектов</th><th>Заданий</th>
            <th>Отправок</th><th>XP</th><th>Рейтинг</th><th>Последний вход</th><th>Действия</th>
          </tr></thead>
          <tbody>${rows}</tbody>
        </table>
      </div>${pagination}` :
      '<div class="empty">Ничего не найдено</div>');
}

let searchDebounce;
function onUserSearch(v) {
  clearTimeout(searchDebounce);
  searchDebounce = setTimeout(() => {
    state.users.search = v;
    state.users.page = 1;
    loadUsers();
  }, 300);
}
function onUserRoleFilter(v) {
  state.users.role = v;
  state.users.page = 1;
  loadUsers();
}
async function changeRole(id, role) {
  if (!confirm(`Сменить роль пользователя #${id} на "${role}"?`)) return;
  const r = await api(`/api/admin/users/${id}/role`, {
    method: 'PATCH',
    headers: {'Content-Type':'application/json'},
    body: JSON.stringify({ role })
  });
  const d = await r.json();
  if (!r.ok) { toast(d.error || 'Ошибка', 'error'); await loadUsers(); return; }
  toast('Роль изменена', 'success');
  await loadUsers();
}
async function toggleActive(id, isActive) {
  const action = isActive ? 'Заблокировать' : 'Разблокировать';
  if (!confirm(`${action} пользователя #${id}?`)) return;
  const r = await api(`/api/admin/users/${id}/active`, {
    method: 'PATCH',
    headers: {'Content-Type':'application/json'},
    body: JSON.stringify({ isActive: !isActive })
  });
  const d = await r.json();
  if (!r.ok) { toast(d.error || 'Ошибка', 'error'); return; }
  toast(isActive ? 'Заблокирован' : 'Разблокирован', 'success');
  await loadUsers();
}
async function deleteUser(id, username) {
  if (!confirm(`Удалить пользователя "${username}"?\n\nЭто действие необратимо.`)) return;
  const r = await api(`/api/admin/users/${id}`, {method:'DELETE'});
  const d = await r.json();
  if (!r.ok) { toast(d.error || 'Ошибка', 'error'); return; }
  toast('Пользователь удалён', 'success');
  await loadUsers();
}

// ─────────────── Projects ───────────────
async function loadProjects() {
  const s = state.projects;
  const url = `/api/admin/projects?page=${s.page}&pageSize=${s.pageSize}` +
    (s.search ? `&search=${encodeURIComponent(s.search)}` : '');
  const r = await api(url);
  if (!r.ok) return;
  const data = await r.json();

  const rows = data.projects.map(p => `<tr>
    <td class="mono">#${p.id}</td>
    <td>${esc(p.name)}</td>
    <td>${esc(p.ownerUsername)}</td>
    <td>${p.isPublic ? '<span class="badge published">public</span>' : '<span class="badge">private</span>'}</td>
    <td class="mono">${p.publicSlug ? esc(p.publicSlug) : '—'}</td>
    <td class="mono">${fmtDate(p.updatedAt)}</td>
    <td>
      <button class="btn small danger" onclick="deleteProject(${p.id})">✕</button>
    </td>
  </tr>`).join('');

  const toolbar = `
    <div class="toolbar">
      <input class="search-input" type="text" placeholder="Поиск по названию..."
             value="${esc(s.search)}" oninput="onProjectSearch(this.value)">
    </div>`;

  $('tab-content').innerHTML = toolbar +
    (data.projects.length ? `
      <div style="overflow-x:auto;"><table>
        <thead><tr><th>ID</th><th>Название</th><th>Владелец</th>
          <th>Доступ</th><th>Slug</th><th>Обновлён</th><th></th></tr></thead>
        <tbody>${rows}</tbody>
      </table></div>
      ${renderPagination('projects', data.page, data.pageSize, data.totalCount)}` :
      '<div class="empty">Ничего не найдено</div>');
}
let projectDebounce;
function onProjectSearch(v) {
  clearTimeout(projectDebounce);
  projectDebounce = setTimeout(() => {
    state.projects.search = v; state.projects.page = 1; loadProjects();
  }, 300);
}
async function deleteProject(id) {
  if (!confirm(`Удалить проект #${id}?`)) return;
  const r = await api(`/api/admin/projects/${id}`, {method:'DELETE'});
  if (!r.ok) { toast('Ошибка', 'error'); return; }
  toast('Проект удалён', 'success');
  await loadProjects();
}

// ─────────────── Assignments ───────────────
async function loadAssignments() {
  const s = state.assignments;
  const url = `/api/admin/assignments?page=${s.page}&pageSize=${s.pageSize}` +
    (s.search ? `&search=${encodeURIComponent(s.search)}` : '');
  const r = await api(url);
  if (!r.ok) return;
  const data = await r.json();

  const rows = data.assignments.map(a => `<tr>
    <td class="mono">#${a.id}</td>
    <td>${esc(a.title)}</td>
    <td>${esc(a.authorUsername)}</td>
    <td><span class="badge">${esc(a.mode)}</span></td>
    <td>${a.isPublished ? '<span class="badge published">да</span>' : '<span class="badge draft">черновик</span>'}</td>
    <td class="num">${a.submissionsCount}</td>
    <td class="num">${a.passedCount}</td>
    <td class="mono">${fmtDate(a.createdAt)}</td>
    <td><button class="btn small danger" onclick="deleteAssignment(${a.id})">✕</button></td>
  </tr>`).join('');

  $('tab-content').innerHTML = `
    <div class="toolbar">
      <input class="search-input" type="text" placeholder="Поиск..."
             value="${esc(s.search)}" oninput="onAssignmentSearch(this.value)">
    </div>` +
    (data.assignments.length ? `
      <div style="overflow-x:auto;"><table>
        <thead><tr><th>ID</th><th>Название</th><th>Автор</th><th>Режим</th>
          <th>Опубликовано</th><th>Отправок</th><th>Прошло</th><th>Создано</th><th></th>
        </tr></thead><tbody>${rows}</tbody>
      </table></div>
      ${renderPagination('assignments', data.page, data.pageSize, data.totalCount)}` :
      '<div class="empty">Ничего не найдено</div>');
}
let asgDebounce;
function onAssignmentSearch(v) {
  clearTimeout(asgDebounce);
  asgDebounce = setTimeout(() => {
    state.assignments.search = v; state.assignments.page = 1; loadAssignments();
  }, 300);
}
async function deleteAssignment(id) {
  if (!confirm(`Удалить задание #${id}? Все отправки будут потеряны.`)) return;
  const r = await api(`/api/admin/assignments/${id}`, {method:'DELETE'});
  if (!r.ok) { toast('Ошибка', 'error'); return; }
  toast('Задание удалено', 'success');
  await loadAssignments();
}

// ─────────────── Submissions ───────────────
async function loadSubmissions() {
  const s = state.submissions;
  const url = `/api/admin/submissions?page=${s.page}&pageSize=${s.pageSize}` +
    (s.status ? `&status=${s.status}` : '');
  const r = await api(url);
  if (!r.ok) return;
  const data = await r.json();

  const rows = data.submissions.map(x => `<tr>
    <td class="mono">#${x.id}</td>
    <td class="mono">#${x.assignmentId}</td>
    <td>${esc(x.assignmentTitle)}</td>
    <td>${esc(x.username)}</td>
    <td><span class="badge ${x.status}">${esc(x.status)}</span></td>
    <td>${x.grade ? `<b style="color:var(--gold);">${esc(x.grade)}</b>` : '—'}</td>
    <td class="num xp">${x.xpAwarded > 0 ? '+' + x.xpAwarded : 0}</td>
    <td class="mono">${fmtDate(x.createdAt)}</td>
  </tr>`).join('');

  $('tab-content').innerHTML = `
    <div class="toolbar">
      <select class="filter-select" onchange="onStatusFilter(this.value)">
        <option value="">Все статусы</option>
        <option value="passed" ${s.status === 'passed' ? 'selected' : ''}>Прошло</option>
        <option value="failed" ${s.status === 'failed' ? 'selected' : ''}>Не прошло</option>
        <option value="error" ${s.status === 'error' ? 'selected' : ''}>Ошибки</option>
        <option value="pending" ${s.status === 'pending' ? 'selected' : ''}>В процессе</option>
      </select>
    </div>` +
    (data.submissions.length ? `
      <div style="overflow-x:auto;"><table>
        <thead><tr><th>ID</th><th>Задание</th><th>Название</th><th>Ученик</th>
          <th>Статус</th><th>Грейд</th><th>XP</th><th>Когда</th></tr></thead>
        <tbody>${rows}</tbody>
      </table></div>
      ${renderPagination('submissions', data.page, data.pageSize, data.totalCount)}` :
      '<div class="empty">Нет отправок</div>');
}
function onStatusFilter(v) {
  state.submissions.status = v;
  state.submissions.page = 1;
  loadSubmissions();
}

// ─────────────── Pagination ───────────────
function renderPagination(kind, page, pageSize, total) {
  const totalPages = Math.max(1, Math.ceil(total / pageSize));
  const from = (page - 1) * pageSize + 1;
  const to = Math.min(page * pageSize, total);
  return `
    <div class="pagination">
      <button onclick="gotoPage('${kind}', 1)" ${page <= 1 ? 'disabled' : ''}>«</button>
      <button onclick="gotoPage('${kind}', ${page - 1})" ${page <= 1 ? 'disabled' : ''}>‹ Назад</button>
      <span>${from}–${to} из ${total}</span>
      <button onclick="gotoPage('${kind}', ${page + 1})" ${page >= totalPages ? 'disabled' : ''}>Вперёд ›</button>
      <button onclick="gotoPage('${kind}', ${totalPages})" ${page >= totalPages ? 'disabled' : ''}>»</button>
    </div>`;
}
window.gotoPage = (kind, page) => {
  state[kind].page = Math.max(1, page);
  renderTab();
};

// ─────────────── Init ───────────────
async function init() {
  if (!auth.access) {
    location.href = '/api/yawa/assignments-viewer';
    return;
  }
  try {
    const r = await fetch(API + '/api/auth/me', {
      headers: {'Authorization': 'Bearer ' + auth.access}
    });
    if (!r.ok) {
      if (auth.refresh) {
        const ok = await tryRefresh();
        if (ok) return init();
      }
      location.href = '/api/yawa/assignments-viewer';
      return;
    }
    auth.user = await r.json();
  } catch {
    location.href = '/api/yawa/assignments-viewer';
    return;
  }

  if (auth.user.role !== 'admin') {
    $('empty-state').innerHTML = `
      <div class="icon">🚫</div>
      <div class="title">Доступ запрещён</div>
      <div style="font-size:12px;margin-top:12px;">
        <a href="/api/yawa/assignments-viewer" class="btn primary">Вернуться</a>
      </div>`;
    return;
  }

  renderHeader();
  $('empty-state').style.display = 'none';
  $('content').style.display = 'block';

  await loadStats();
  await renderTab();
}
init();
</script>
</body>
</html>
""";
}
