// Playthrough harness: runs the real playtest page in a headless browser and lets the orchestrator drive it turn by
// turn over HTTP. The page's order reader prompt is handed out at /send and its answer taken at /reader, so a real
// model (a Haiku agent) reads every order exactly as the page would ask it. The director and the voices are left to
// the page's own fallback (the die, and the code's own words), as when Haiku isn't available.
const { chromium } = require('playwright');
const http = require('http'), fs = require('fs'), path = require('path');
const root = process.argv[2], shots = process.argv[3], ctlPort = +(process.argv[4] || 8802), webPort = ctlPort + 100;
const types = { '.html': 'text/html; charset=utf-8', '.js': 'text/javascript', '.json': 'application/json', '.wasm': 'application/wasm' };
http.createServer((q, s) => { const f = path.join(root, decodeURIComponent(q.url.split('?')[0])); fs.readFile(f, (e, d) => { if (e) { s.writeHead(404); s.end(); return; } s.writeHead(200, { 'Content-Type': types[path.extname(f)] || 'application/octet-stream' }); s.end(d); }); }).listen(webPort);

let page, pending = null, netCursor = 0, errs = [], shotN = 0;
const sleep = ms => new Promise(r => setTimeout(r, ms));
async function until(fn, ms = 120000) { const t = Date.now(); while (Date.now() - t < ms) { if (await fn()) return true; await sleep(150); } return false; }

async function boot() {
  const b = await chromium.launch();
  const ctx = await b.newContext({ viewport: { width: 1280, height: 900 }, deviceScaleFactor: 1.5 });
  page = await ctx.newPage();
  page.on('pageerror', e => errs.push('pageerror: ' + e.message));
  page.on('console', m => { if (m.type() === 'error') errs.push(m.text()); });
  await page.exposeFunction('__readOrders', prompt => new Promise(res => { pending = { prompt, res }; }));
  await page.addInitScript(() => {
    const sample = async () => ({ text: '' });
    sample.json = async (prompt) => {
      // The director and the voices fall back to the page's own die and words.
      if (/You're the director/.test(prompt) || /their own voices/.test(prompt)) { const e = new Error('off'); e.code = 'unavailable'; throw e; }
      const text = await window.__readOrders(prompt);
      const m = /\{[\s\S]*\}/.exec(text || '');
      if (!m) { const e = new Error('no json'); e.code = 'bad_json'; throw e; }
      return JSON.parse(m[0]);
    };
    window.claude = { use: async n => n === 'sample' ? sample : n === 'db' ? { collection: c => ({ add: async d => ({}) }) } : null };
  });
  await page.goto('http://localhost:' + webPort + '/playtest.html');
  await page.waitForSelector('#bootPick button[data-kind]', { timeout: 120000 });
}

function stateFn() {
  const S = window.__S; if (!S || !S.m) return null;
  const st = S.state, sq = S.sq;
  const grid = (x, y) => String.fromCharCode(65 + Math.max(0, Math.min(25, Math.floor(x / sq)))) + (Math.max(0, Math.floor(y / sq)) + 1);
  const ours = [], contacts = [];
  S.roster.forEach((r, i) => {
    const u = st.units[i]; if (!u) return;
    if (r.side === 'us') {
      if (r.figure === 'drone') { if (!u.gone) ours.push({ name: 'drone', grid: u.grid, note: 'in the air' }); return; }
      ours.push({ name: r.name, role: r.kind, team: r.team || 'HQ', grid: u.grid, condition: u.condition + (u.legHit ? ', leg hit' : '') + (u.armHit ? ', arm hit' : '') + (u.stable ? ', stable' : ''), gone: u.gone ? (u.evacuated ? 'evacuated' : 'gone') : '', doing: u.order ? [u.order.verb, u.order.where, u.order.target, u.order.facing ? 'facing ' + u.order.facing : ''].filter(Boolean).join(' ') : 'nothing', ammo: u.ammo });
    } else if (u.seen || u.lastSeen) {
      const age = Math.max(0, Math.round((st.step - (u.lastSeen || 0)) / 600));
      contacts.push({ name: r.name, side: r.side, grid: u.grid, seen: u.seen ? 'seen now' : 'last seen ' + (age < 1 ? 'just now' : age + ' min ago'), condition: u.condition });
    }
  });
  const places = S.map.places.filter(p => p.kind !== 'control').map(p => p.label + ' (' + p.grid + ')');
  const controls = (S.m.controls || []).map(c => c.label);
  const buildings = S.map.buildings.map(b => b.label + ' ' + grid(b.x + b.w / 2, b.y + b.h / 2));
  const o = st.objectives;
  return {
    title: S.m.title, side: S.m.side && S.m.side.name, mission: S.m.kindName, clock: document.getElementById('clock').textContent,
    turn: st.turn, turnLimit: st.turnLimit, outcome: st.outcome || null, outcomeText: st.outcomeText || null, rating: st.rating || null,
    enemyGoalText: st.enemyGoalText || null, lies: st.lies || [], addIns: st.addIns || [],
    brief: S.m.brief, objectives: { main: o.main, mainStatus: o.mainStatus, win: o.win, lose: o.lose, roe: o.roe, secondary: o.secondary.map(x => x.text + ': ' + x.status) },
    map: { columns: 'A to ' + String.fromCharCode(64 + S.map.cols), rows: '1 to ' + S.map.rows, squareMeters: S.sq, places, controlLines: controls, buildings },
    ours, contacts, casevac: st.casevac || null,
  };
}

async function state() { return await page.evaluate(stateFn); }
async function newNet() {
  const all = await page.evaluate(() => (window.__S.log || []).map(e => ({ kind: e.kind, t: e.step != null ? Math.floor(e.step / 600) + ':' + String(Math.floor(e.step / 10) % 60).padStart(2, '0') : '', who: e.who || '', text: e.text })));
  const fresh = all.slice(netCursor); netCursor = all.length; return fresh;
}
async function shot() {
  await page.click('#zfit').catch(() => {});
  await sleep(250);
  const f = path.join(shots, 'map-' + String(++shotN).padStart(3, '0') + '.png');
  await page.locator('#tableBox').screenshot({ path: f });
  return f;
}
async function previewText() { return await page.evaluate(() => { const p = document.querySelector('#pane .preview'); return p ? p.innerText : ''; }); }

const routes = {
  async start(body) {
    netCursor = 0;
    await page.click('#bootPick button[data-side="' + body.side + '"]');
    await sleep(100);
    await page.click('#bootPick button[data-kind="' + (body.kind || 'random') + '"]');
    await page.waitForSelector('#boot', { state: 'hidden', timeout: 120000 });
    await page.click('.tabs button[data-tab="net"]');
    return { state: await state(), map: await shot() };
  },
  async state() { return { state: await state(), net: await newNet(), errors: errs.splice(0) }; },
  async shot() { return { map: await shot() }; },
  async send(body) {
    pending = null;
    await page.click('.tabs button[data-tab="net"]');
    await page.fill('#orderBox', body.text);
    await page.click('#sendBtn');
    await until(async () => pending || !(await page.evaluate(() => window.__S.reading)), 60000);
    if (pending) return { prompt: pending.prompt };
    return { prompt: null, preview: await previewText() };
  },
  async reader(body) {
    if (!pending) return { error: 'nothing waiting' };
    const p = pending; pending = null; p.res(body.text);
    await until(async () => !(await page.evaluate(() => window.__S.reading || window.__S.busy)), 60000);
    await sleep(200);
    return { preview: await previewText(), errors: errs.splice(0) };
  },
  async play() {
    await page.click('#playBtn');
    const shown = await until(async () => await page.evaluate(() => !document.getElementById('playbar').hidden || !document.getElementById('banner').hidden), 180000);
    if (await page.evaluate(() => !document.getElementById('playbar').hidden)) {
      await page.click('#pskip');
      await until(async () => await page.evaluate(() => document.getElementById('playbar').hidden), 60000);
    }
    await sleep(300);
    const banner = await page.evaluate(() => { const b = document.getElementById('banner'); return b.hidden ? '' : b.innerText; });
    return { net: await newNet(), state: await state(), banner, map: await shot(), errors: errs.splice(0) };
  },
};

boot().then(() => {
  http.createServer(async (q, s) => {
    let body = '';
    q.on('data', c => body += c);
    q.on('end', async () => {
      try {
        const name = q.url.replace(/^\//, '').split('?')[0];
        const out = await routes[name](body ? JSON.parse(body) : {});
        s.writeHead(200, { 'Content-Type': 'application/json' }); s.end(JSON.stringify(out));
      } catch (e) { s.writeHead(500); s.end(JSON.stringify({ error: String(e && e.stack || e) })); }
    });
  }).listen(ctlPort, () => console.log('harness ready on ' + ctlPort));
}).catch(e => { console.error('boot failed', e); process.exit(1); });
