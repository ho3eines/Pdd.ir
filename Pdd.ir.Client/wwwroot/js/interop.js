// Pdd.ir Interop Functions

window.setDocDirection = function (lang) {
    var dir = lang === 'fa' ? 'rtl' : 'ltr';
    document.documentElement.setAttribute('dir', dir);
    document.documentElement.setAttribute('lang', lang);
};

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

// ═══════════════════════════════════════════════════════════
// HOME FULLSCREEN — Mouse Glow (GPU-accelerated, zero lag)
// ═══════════════════════════════════════════════════════════
var _homeGlowRaf = null;
var _homeGlowX = -500;
var _homeGlowY = -500;
var _homeGlowTargetX = -500;
var _homeGlowTargetY = -500;
var _homeGlowActive = false;

function _homeGlowLoop() {
    if (!_homeGlowActive) return;
    _homeGlowX += (_homeGlowTargetX - _homeGlowX) * 0.12;
    _homeGlowY += (_homeGlowTargetY - _homeGlowY) * 0.12;
    var glow = document.getElementById('mouseGlow');
    if (glow) {
        glow.style.transform = 'translate(-50%, -50%) translate3d(' + _homeGlowX + 'px, ' + _homeGlowY + 'px, 0)';
    }
    _homeGlowRaf = requestAnimationFrame(_homeGlowLoop);
}

window.initHomeMouseGlow = function () {
    _homeGlowActive = true;
    document.addEventListener('mousemove', function (e) {
        _homeGlowTargetX = e.clientX;
        _homeGlowTargetY = e.clientY;
    });
    _homeGlowLoop();
};

window.disposeHomeMouseGlow = function () {
    _homeGlowActive = false;
    if (_homeGlowRaf) { cancelAnimationFrame(_homeGlowRaf); _homeGlowRaf = null; }
};

// ═══════════════════════════════════════════════════════════
// HOME FULLSCREEN — Floating Particles
// ═══════════════════════════════════════════════════════════
var _homeParticlesRAF = null;
var _homeParticlesArr = [];

window.initHomeParticles = function () {
    var canvas = document.getElementById('homeParticles');
    if (!canvas) return;
    var ctx = canvas.getContext('2d');
    var dpr = window.devicePixelRatio || 1;
    var count = window.innerWidth < 768 ? 25 : 50;

    function resize() {
        canvas.width = window.innerWidth * dpr;
        canvas.height = window.innerHeight * dpr;
        canvas.style.width = window.innerWidth + 'px';
        canvas.style.height = window.innerHeight + 'px';
    }

    function spawn() {
        _homeParticlesArr = [];
        for (var i = 0; i < count; i++) {
            _homeParticlesArr.push({
                x: Math.random() * canvas.width,
                y: Math.random() * canvas.height,
                vx: (Math.random() - 0.5) * 0.3 * dpr,
                vy: (Math.random() - 0.5) * 0.3 * dpr,
                r: (Math.random() * 1.2 + 0.3) * dpr,
                a: Math.random() * 0.4 + 0.1,
                hue: Math.random() < 0.6 ? 215 : (Math.random() < 0.5 ? 300 : 170)
            });
        }
    }

    function draw() {
        if (!canvas.isConnected) return;
        ctx.clearRect(0, 0, canvas.width, canvas.height);
        _homeParticlesArr.forEach(function (p) {
            p.x += p.vx;
            p.y += p.vy;
            if (p.x < 0) p.x = canvas.width;
            if (p.x > canvas.width) p.x = 0;
            if (p.y < 0) p.y = canvas.height;
            if (p.y > canvas.height) p.y = 0;

            var color;
            if (p.hue === 215) color = 'rgba(13, 110, 253, ' + p.a + ')';
            else if (p.hue === 300) color = 'rgba(248, 28, 229, ' + p.a + ')';
            else color = 'rgba(80, 227, 194, ' + p.a + ')';

            ctx.beginPath();
            ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
            ctx.fillStyle = color;
            ctx.shadowColor = color;
            ctx.shadowBlur = 6;
            ctx.fill();
        });
        _homeParticlesRAF = requestAnimationFrame(draw);
    }

    resize();
    spawn();
    draw();
    window.addEventListener('resize', function () { resize(); spawn(); });
};

window.disposeHomeParticles = function () {
    if (_homeParticlesRAF) {
        cancelAnimationFrame(_homeParticlesRAF);
        _homeParticlesRAF = null;
    }
    _homeParticlesArr = [];
};

// ═══════════════════════════════════════════════════════════
// HOME — Product Row Reveal on Hover Glow
// ═══════════════════════════════════════════════════════════
var _productGlowThrottle = null;
window.initHomeProductReveal = function () {
    document.querySelectorAll('.product-row').forEach(function (row) {
        row.addEventListener('mousemove', function (e) {
            if (_productGlowThrottle) return;
            _productGlowThrottle = setTimeout(function () { _productGlowThrottle = null; }, 16);
            var rect = row.getBoundingClientRect();
            var x = e.clientX - rect.left;
            var y = e.clientY - rect.top;
            row.style.background = 'radial-gradient(circle 180px at ' + x + 'px ' + y + 'px, rgba(13,110,253,0.08), transparent)';
        });
        row.addEventListener('mouseleave', function () {
            row.style.background = '';
        });
    });
};

window.addRippleEffect = function () {
    var activator = document.querySelector('.language-activator');
    if (!activator) return;
    activator.style.transform = 'scale(0.95)';
    setTimeout(function () {
        activator.style.transform = 'scale(1)';
    }, 200);
};
