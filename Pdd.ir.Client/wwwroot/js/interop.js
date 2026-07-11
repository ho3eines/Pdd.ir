// Pdd.ir Interop Functions

window.initAnimations = function () {
    var elements = document.querySelectorAll('.product-card, .feature-card, .stat-card, .value-card');
    elements.forEach(function (el, index) {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';
        el.style.transition = 'all 0.5s ease ' + (index * 0.1) + 's';
        setTimeout(function () {
            el.style.opacity = '1';
            el.style.transform = 'translateY(0)';
        }, 100);
    });
};

window.scrollToElement = function (id) {
    var el = document.getElementById(id);
    if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
};

window.scrollToRow = function (id) {
    var el = document.getElementById(id);
    if (el) {
        el.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
};

window.setFocus = function (elementId) {
    var el = document.getElementById(elementId);
    if (el) {
        el.focus();
    }
};

// CryptoUtils - AES-256-CBC encryption/decryption (compatible with CryptoJsService on server)
window.CryptoUtils = {
    encryptData: function (plaintext, key) {
        try {
            if (!plaintext || !key) {
                throw new Error('Plaintext and key are required');
            }
            if (!window.CryptoJS) {
                throw new Error('CryptoJS not loaded');
            }
            var keyHash = CryptoJS.SHA256(CryptoJS.enc.Utf8.parse(key));
            var iv = CryptoJS.lib.WordArray.random(16);
            var encrypted = CryptoJS.AES.encrypt(
                CryptoJS.enc.Utf8.parse(plaintext),
                keyHash,
                {
                    iv: iv,
                    mode: CryptoJS.mode.CBC,
                    padding: CryptoJS.pad.Pkcs7
                }
            );
            var result = iv.concat(encrypted.ciphertext).toString(CryptoJS.enc.Base64);
            return result;
        } catch (error) {
            console.error('Encryption error:', error);
            throw new Error('Encryption failed');
        }
    },

    decryptData: function (ciphertext, key) {
        try {
            if (!ciphertext || !key) {
                throw new Error('Ciphertext and key are required');
            }
            if (!window.CryptoJS) {
                throw new Error('CryptoJS not loaded');
            }
            var keyHash = CryptoJS.SHA256(CryptoJS.enc.Utf8.parse(key));
            var encryptedData = CryptoJS.enc.Base64.parse(ciphertext);
            var iv = CryptoJS.lib.WordArray.create(encryptedData.words.slice(0, 4));
            var ciphertextBytes = CryptoJS.lib.WordArray.create(encryptedData.words.slice(4));
            var cipherParams = CryptoJS.lib.CipherParams.create({
                ciphertext: ciphertextBytes
            });
            var decrypted = CryptoJS.AES.decrypt(
                cipherParams,
                keyHash,
                {
                    iv: iv,
                    mode: CryptoJS.mode.CBC,
                    padding: CryptoJS.pad.Pkcs7
                }
            );
            var result = decrypted.toString(CryptoJS.enc.Utf8);
            if (!result) {
                throw new Error('Decryption failed - possibly wrong key');
            }
            return result;
        } catch (error) {
            console.error('Decryption error:', error);
            throw new Error('Decryption failed');
        }
    },

    generateRandomKey: function (length) {
        length = length || 32;
        if (!window.CryptoJS) {
            throw new Error('CryptoJS not loaded');
        }
        var randomKey = CryptoJS.lib.WordArray.random(length);
        return CryptoJS.enc.Base64.stringify(randomKey);
    }
};

window.addRippleEffect = function () {
    var activator = document.querySelector('.language-activator');
    if (!activator) return;
    activator.style.transform = 'scale(0.95)';
    setTimeout(function () {
        activator.style.transform = 'scale(1)';
    }, 200);
};
