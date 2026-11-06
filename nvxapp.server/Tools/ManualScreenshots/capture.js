// Schermate per i manuali utente (vedi README.md nella stessa cartella).
// Uso: node capture.js <url base> <cartella di uscita> <utente:password> [<utente:password> ...]
// Serve Chrome avviato con: chrome --headless=new --remote-debugging-port=9222 --user-data-dir=<cartella temporanea>
// Per ogni utente: login, voci del menu laterale, una schermata JPEG dell'area della pagina (tema chiaro) per ogni voce.
// Nome del file: indirizzo senza "/" iniziale, "/" interni -> "_" (come UserManualMarkdown.NomeImmagine sul server).
const [base, outDir, ...utenti] = process.argv.slice(2);
const fs = require('fs');
const path = require('path');

if (!base || !outDir || utenti.length === 0) {
  console.log('uso: node capture.js <url base> <cartella di uscita> <utente:password> [...]');
  process.exit(1);
}
fs.mkdirSync(outDir, { recursive: true });
const ESCLUSI = ['/login', '/logout', '/userimpersonate', '/usermanual'];
const sleep = ms => new Promise(r => setTimeout(r, ms));

(async () => {
  const versione = await (await fetch('http://localhost:9222/json/version')).json();
  const ws = new WebSocket(versione.webSocketDebuggerUrl);
  let id = 0; const pending = new Map(); let sid;
  const send = (m, p = {}, s) => new Promise(r => { const i = ++id; pending.set(i, r); ws.send(JSON.stringify({ id: i, method: m, params: p, sessionId: s })); });
  const evalJs = async e => (await send('Runtime.evaluate', { expression: e, awaitPromise: true, returnByValue: true }, sid))?.result?.value;
  ws.onmessage = ev => { const m = JSON.parse(ev.data); if (m.id && pending.has(m.id)) { pending.get(m.id)(m.result ?? m.error); pending.delete(m.id); } };
  await new Promise(r => ws.onopen = r);
  const fatte = new Set();

  for (const credenziali of utenti) {
    const [utente, password] = credenziali.split(':');
    const { targetId } = await send('Target.createTarget', { url: 'about:blank' });
    ({ sessionId: sid } = await send('Target.attachToTarget', { targetId, flatten: true }));
    await send('Runtime.enable', {}, sid); await send('Page.enable', {}, sid);
    await send('Emulation.setDeviceMetricsOverride', { width: 1400, height: 900, deviceScaleFactor: 1, mobile: false }, sid);
    await send('Emulation.setEmulatedMedia', { features: [{ name: 'prefers-color-scheme', value: 'light' }] }, sid);
    await send('Page.navigate', { url: base + '/login' }, sid); await sleep(4000);
    await evalJs(`(async () => {
      const i = [...document.querySelectorAll('ion-input input, input')].filter(x => x.offsetParent !== null);
      const set = (el, v) => { el.value = v; el.dispatchEvent(new Event('input', { bubbles: true })); };
      set(i[0], ${JSON.stringify(utente)}); set(i[1], ${JSON.stringify(password)});
      await new Promise(r => setTimeout(r, 300));
      [...document.querySelectorAll('ion-button, button')].find(b => /login|accedi/i.test(b.textContent))?.click();
    })()`);
    await sleep(6000);
    const urls = await evalJs(`[...new Set([...document.querySelectorAll('ion-menu [href]')].map(e => e.getAttribute('href')).filter(h => h && h.startsWith('/')))]`) ?? [];
    console.log(`${utente}: ${urls.length} voci`);
    for (const url of urls) {
      if (ESCLUSI.some(e => url.startsWith(e)) || fatte.has(url))
        continue;
      await evalJs(`(() => { history.pushState({}, '', '${url}'); window.dispatchEvent(new PopStateEvent('popstate')); })()`);
      await sleep(3500);
      // tema chiaro (le schermate vanno su carta) e chiusura di eventuali finestre aperte
      await evalJs(`document.documentElement.classList.remove('ion-palette-dark'); document.querySelectorAll('ion-modal, ion-alert, ion-toast').forEach(x => x.dismiss?.()); true`);
      await sleep(600);
      const r = await evalJs(`(() => { const p = document.querySelector('ion-router-outlet .ion-page:not(.ion-page-hidden)') ?? document.querySelector('ion-router-outlet');
                                     const b = p.getBoundingClientRect(); return { x: b.left, y: b.top, width: b.width, height: Math.min(b.height, 820) }; })()`);
      const shot = await send('Page.captureScreenshot', { format: 'jpeg', quality: 70, clip: { ...r, scale: 0.8 } }, sid);
      const nome = url.replace(/^\//, '').replace(/\//g, '_').toLowerCase() + '.jpg';
      fs.writeFileSync(path.join(outDir, nome), Buffer.from(shot.data, 'base64'));
      fatte.add(url);
      console.log('  ' + nome);
    }
    await send('Target.closeTarget', { targetId });
  }
  ws.close();
  process.exit(0);
})();
