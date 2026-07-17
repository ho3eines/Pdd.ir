// Pdd.ir.Shared — Interop Functions

window.setDocDirection = function (lang) {
    var dir = lang === 'fa' ? 'rtl' : 'ltr';
    document.documentElement.setAttribute('dir', dir);
    document.documentElement.setAttribute('lang', lang);
};

window.scrollToElement = function (id) {
    var el = document.getElementById(id);
    if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' });
};

window.setFocus = function (elementId) {
    var el = document.getElementById(elementId);
    if (el) el.focus();
};

// Theme Toggle
window.getTheme = function () {
    return localStorage.getItem('pdd_theme') || 'dark';
};

window.setTheme = function (theme) {
    document.documentElement.setAttribute('data-theme', theme);
    localStorage.setItem('pdd_theme', theme);
};

window.toggleTheme = function () {
    var current = document.documentElement.getAttribute('data-theme') || 'dark';
    var next = current === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('pdd_theme', next);
    return next;
};

// Tab activation (Bootstrap)
window.activateTab = function (tabId) {
    var el = document.querySelector('[data-bs-target="#' + tabId + '"]');
    if (el) {
        var tab = new bootstrap.Tab(el);
        tab.show();
    }
};

// Encryption (CryptoJS)
window.encryptData = function (plaintext, key) {
    if (!window.CryptoJS) throw new Error('CryptoJS not loaded');
    var keyHash = CryptoJS.SHA256(CryptoJS.enc.Utf8.parse(key));
    var iv = CryptoJS.lib.WordArray.random(16);
    var encrypted = CryptoJS.AES.encrypt(CryptoJS.enc.Utf8.parse(plaintext), keyHash, { iv: iv, mode: CryptoJS.mode.CBC, padding: CryptoJS.pad.Pkcs7 });
    return iv.concat(encrypted.ciphertext).toString(CryptoJS.enc.Base64);
};

window.decryptData = function (ciphertext, key) {
    if (!window.CryptoJS) throw new Error('CryptoJS not loaded');
    var keyHash = CryptoJS.SHA256(CryptoJS.enc.Utf8.parse(key));
    var encryptedData = CryptoJS.enc.Base64.parse(ciphertext);
    var iv = CryptoJS.lib.WordArray.create(encryptedData.words.slice(0, 4));
    var ciphertextBytes = CryptoJS.lib.WordArray.create(encryptedData.words.slice(4));
    var cipherParams = CryptoJS.CipherParams.create({ ciphertext: ciphertextBytes });
    var decrypted = CryptoJS.AES.decrypt(cipherParams, keyHash, { iv: iv, mode: CryptoJS.mode.CBC, padding: CryptoJS.pad.Pkcs7 });
    var result = decrypted.toString(CryptoJS.enc.Utf8);
    if (!result) throw new Error('Decryption failed');
    return result;
};

window.generateRandomKey = function () {
    if (!window.CryptoJS) throw new Error('CryptoJS not loaded');
    return CryptoJS.enc.Base64.stringify(CryptoJS.lib.WordArray.random(32));
};
