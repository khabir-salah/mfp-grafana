/* ── Footer clock ─────────────────────────────────── */
const timeEl = document.getElementById('footer-time');
if (timeEl) {
  const tick = () => {
    timeEl.textContent = new Date().toLocaleTimeString('en-US', { hour12: false });
  };
  tick();
  setInterval(tick, 1000);
}

/* ── Upload Logic ──────────────────────────────────── */
const zone      = document.getElementById('upload-zone');
const input     = document.getElementById('csv-input');
const progress  = document.getElementById('upload-progress');
const fill      = document.getElementById('progress-fill');
const label     = document.getElementById('progress-label');
const result    = document.getElementById('upload-result');

if (zone && input) {
  // Click to browse
  zone.addEventListener('click', () => input.click());
  input.addEventListener('change', () => {
    if (input.files?.[0]) uploadFile(input.files[0]);
  });

  // Drag & Drop
  zone.addEventListener('dragover', e => {
    e.preventDefault();
    zone.classList.add('dragover');
  });

  zone.addEventListener('dragleave', () => zone.classList.remove('dragover'));

  zone.addEventListener('drop', e => {
    e.preventDefault();
    zone.classList.remove('dragover');
    const file = e.dataTransfer?.files?.[0];
    if (file) uploadFile(file);
  });
}

async function uploadFile(file) {
  if (!file.name.toLowerCase().endsWith('.csv')) {
    showResult(false, 'Only CSV files are accepted.');
    return;
  }

  result.classList.add('hidden');
  progress.classList.remove('hidden');
  fill.style.width = '0%';
  label.textContent = `Uploading ${file.name}…`;

  // Animate progress bar (fake progress while fetching)
  let pct = 0;
  const ticker = setInterval(() => {
    pct = Math.min(pct + (pct < 60 ? 5 : 1), 90);
    fill.style.width = pct + '%';
  }, 150);

  const form = new FormData();
  form.append('file', file);

  try {
    const res = await fetch('/api/upload', { method: 'POST', body: form });
    const data = await res.json();

    clearInterval(ticker);
    fill.style.width = '100%';
    label.textContent = 'Processing complete';

    setTimeout(() => {
      progress.classList.add('hidden');
      showResult(data.success, data.message, data.warnings, data);
      if (data.success) {
        setTimeout(() => window.location.reload(), 3000);
      }
    }, 500);

  } catch (err) {
    clearInterval(ticker);
    progress.classList.add('hidden');
    showResult(false, 'Network error — please try again. ' + err.message);
  }

  // Reset input
  input.value = '';
}

function showResult(success, message, warnings = [], data = null) {
  result.className = 'upload-result ' + (success ? 'success' : 'error');
  result.classList.remove('hidden');

  let html = `<div class="result-title">${success ? '✓' : '✗'} ${escHtml(message)}</div>`;

  if (success && data) {
    const parts = [];
    if (data.foodRowsImported > 0)     parts.push(`${data.foodRowsImported} food entries`);
    if (data.exerciseRowsImported > 0)  parts.push(`${data.exerciseRowsImported} exercise entries`);
    if (data.weightRowsImported > 0)    parts.push(`${data.weightRowsImported} weight entries`);
    if (parts.length) html += `<div>Imported: ${parts.join(', ')}</div>`;
    html += `<div style="font-size:0.8rem;opacity:0.7;margin-top:0.4rem">Refreshing dashboard…</div>`;
  }

  if (warnings?.length) {
    html += `<div class="result-warnings">⚠ ${warnings.slice(0, 5).map(escHtml).join('<br>')}</div>`;
  }

  result.innerHTML = html;
}

function escHtml(str) {
  if (!str) return '';
  return str
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}
