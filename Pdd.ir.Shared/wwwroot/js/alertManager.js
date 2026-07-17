// Pdd.ir.Shared — Alert Manager

let alertContainer = null;

export function initialize() {
    if (alertContainer) return;
    alertContainer = document.createElement('div');
    alertContainer.id = 'pdd-alert-container';
    alertContainer.style.cssText = 'position:fixed;top:20px;right:20px;z-index:99999;display:flex;flex-direction:column;gap:10px;max-width:400px;';
    document.body.appendChild(alertContainer);

    const style = document.createElement('style');
    style.textContent = `
        .pdd-alert { padding:14px 20px;border-radius:12px;color:#fff;font-family:inherit;font-size:14px;font-weight:500;display:flex;align-items:center;gap:12px;box-shadow:0 10px 40px rgba(0,0,0,0.3);transform:translateX(120%);transition:all 0.4s cubic-bezier(0.23,1,0.32,1);cursor:pointer;backdrop-filter:blur(12px);-webkit-backdrop-filter:blur(12px); }
        .pdd-alert.show { transform:translateX(0); }
        .pdd-alert.hide { transform:translateX(120%);opacity:0; }
        .pdd-alert-success { background:linear-gradient(135deg,rgba(34,197,94,0.9),rgba(22,163,74,0.9));border:1px solid rgba(34,197,94,0.3); }
        .pdd-alert-error { background:linear-gradient(135deg,rgba(239,68,68,0.9),rgba(220,38,38,0.9));border:1px solid rgba(239,68,68,0.3); }
        .pdd-alert-warning { background:linear-gradient(135deg,rgba(234,179,8,0.9),rgba(202,138,4,0.9));border:1px solid rgba(234,179,8,0.3); }
        .pdd-alert-info { background:linear-gradient(135deg,rgba(59,130,246,0.9),rgba(37,99,235,0.9));border:1px solid rgba(59,130,246,0.3); }
        .pdd-alert-icon { font-size:20px;flex-shrink:0; }
        .pdd-alert-body { flex:1; }
        .pdd-alert-title { font-weight:700;margin-bottom:2px; }
        .pdd-alert-message { font-size:13px;opacity:0.9; }
        .pdd-alert-close { background:none;border:none;color:rgba(255,255,255,0.7);font-size:18px;cursor:pointer;padding:0;flex-shrink:0;transition:color 0.2s; }
        .pdd-alert-close:hover { color:#fff; }
        .pdd-alert-progress { position:absolute;bottom:0;left:0;height:3px;background:rgba(255,255,255,0.5);border-radius:0 0 12px 12px;transition:width linear; }
        @media(max-width:576px) { #pdd-alert-container { right:10px;left:10px;max-width:none; } }
    `;
    document.head.appendChild(style);
}

export function showAlert(type, title, message, duration = 5000) {
    if (!alertContainer) initialize();

    const icons = { success: '✓', error: '✕', warning: '⚠', info: 'ℹ' };

    const alert = document.createElement('div');
    alert.className = `pdd-alert pdd-alert-${type}`;
    alert.style.position = 'relative';
    alert.innerHTML = `
        <span class="pdd-alert-icon">${icons[type] || icons.info}</span>
        <div class="pdd-alert-body">
            <div class="pdd-alert-title">${title}</div>
            <div class="pdd-alert-message">${message}</div>
        </div>
        <button class="pdd-alert-close" onclick="this.parentElement.remove()">✕</button>
        <div class="pdd-alert-progress" style="width:100%"></div>
    `;

    alertContainer.appendChild(alert);
    requestAnimationFrame(() => requestAnimationFrame(() => alert.classList.add('show')));

    const progress = alert.querySelector('.pdd-alert-progress');
    if (progress && duration > 0) {
        progress.style.transition = `width ${duration}ms linear`;
        requestAnimationFrame(() => { progress.style.width = '0%'; });
        setTimeout(() => { alert.classList.add('hide'); setTimeout(() => alert.remove(), 400); }, duration);
    }

    alert.addEventListener('click', () => { alert.classList.add('hide'); setTimeout(() => alert.remove(), 400); });
}

export function hideAllAlerts() {
    if (!alertContainer) return;
    alertContainer.innerHTML = '';
}
