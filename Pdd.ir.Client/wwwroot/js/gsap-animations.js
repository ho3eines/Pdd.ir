// GSAP Animation Functions for Pdd.ir - OPTIMIZED

// ═══════════════════════════════════════════════════════════
// SCROLL REVEAL (Lightweight)
// ═══════════════════════════════════════════════════════════
window.initScrollAnimations = function () {
    if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
    gsap.registerPlugin(ScrollTrigger);

    // Use batch for better performance
    gsap.utils.toArray('.anim-fade-up').forEach(function (el) {
        gsap.from(el, {
            y: 40,
            opacity: 0,
            duration: 0.6,
            ease: 'power2.out',
            scrollTrigger: { trigger: el, start: 'top 90%', toggleActions: 'play none none none' }
        });
    });

    gsap.utils.toArray('.anim-fade-in').forEach(function (el) {
        gsap.from(el, {
            opacity: 0,
            duration: 0.5,
            ease: 'power2.out',
            scrollTrigger: { trigger: el, start: 'top 90%', toggleActions: 'play none none none' }
        });
    });

    gsap.utils.toArray('.anim-slide-left').forEach(function (el) {
        gsap.from(el, {
            x: -40,
            opacity: 0,
            duration: 0.6,
            ease: 'power2.out',
            scrollTrigger: { trigger: el, start: 'top 90%', toggleActions: 'play none none none' }
        });
    });

    gsap.utils.toArray('.anim-slide-right').forEach(function (el) {
        gsap.from(el, {
            x: 40,
            opacity: 0,
            duration: 0.6,
            ease: 'power2.out',
            scrollTrigger: { trigger: el, start: 'top 90%', toggleActions: 'play none none none' }
        });
    });

    gsap.utils.toArray('.anim-scale-up').forEach(function (el) {
        gsap.from(el, {
            scale: 0.9,
            opacity: 0,
            duration: 0.5,
            ease: 'back.out(1.4)',
            scrollTrigger: { trigger: el, start: 'top 90%', toggleActions: 'play none none none' }
        });
    });
};

// ═══════════════════════════════════════════════════════════
// MOUSE PARALLAX (Throttled)
// ═══════════════════════════════════════════════════════════
var _mouseThrottle = null;
window.initMouseParallax = function () {
    document.addEventListener('mousemove', function (e) {
        if (_mouseThrottle) return;
        _mouseThrottle = setTimeout(function () { _mouseThrottle = null; }, 16);
        var cards = document.querySelectorAll('.parallax-card');
        cards.forEach(function (card) {
            var rect = card.getBoundingClientRect();
            var x = (e.clientX - rect.left - rect.width / 2) / rect.width;
            var y = (e.clientY - rect.top - rect.height / 2) / rect.height;
            card.style.transform = 'translate(' + (x * 15) + 'px, ' + (y * 15) + 'px)';
        });
    });
};

// ═══════════════════════════════════════════════════════════
// COUNTERS (Optimized)
// ═══════════════════════════════════════════════════════════
window.initCounters = function () {
    if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
    gsap.registerPlugin(ScrollTrigger);

    gsap.utils.toArray('[data-count]').forEach(function (el) {
        var target = parseInt(el.getAttribute('data-count'));
        var obj = { val: 0 };
        gsap.to(obj, {
            val: target,
            duration: 1.5,
            ease: 'power2.out',
            scrollTrigger: { trigger: el, start: 'top 90%' },
            onUpdate: function () {
                el.textContent = Math.floor(obj.val).toLocaleString('fa-IR');
            }
        });
    });
};

// ═══════════════════════════════════════════════════════════
// HERO ANIMATION (Fast)
// ═══════════════════════════════════════════════════════════
window.initHeroAnimation = function () {
    if (typeof gsap === 'undefined') return;

    var tl = gsap.timeline();
    tl.from('.hero-title', { y: 30, opacity: 0, duration: 0.6, ease: 'power2.out' })
      .from('.hero-subtitle', { y: 20, opacity: 0, duration: 0.5, ease: 'power2.out' }, '-=0.3')
      .from('.hero-desc', { y: 15, opacity: 0, duration: 0.5, ease: 'power2.out' }, '-=0.3')
      .from('.hero-actions', { y: 15, opacity: 0, duration: 0.5, ease: 'power2.out' }, '-=0.2')
      .from('.floating-card', { scale: 0.8, opacity: 0, duration: 0.4, ease: 'back.out(1.4)', stagger: 0.1 }, '-=0.2');
};

// ═══════════════════════════════════════════════════════════
// TILT EFFECT (Optimized)
// ═══════════════════════════════════════════════════════════
var _tiltThrottle = null;
window.initTiltEffect = function () {
    document.querySelectorAll('.tilt-card').forEach(function (card) {
        card.addEventListener('mousemove', function (e) {
            if (_tiltThrottle) return;
            _tiltThrottle = setTimeout(function () { _tiltThrottle = null; }, 16);
            var rect = card.getBoundingClientRect();
            var x = e.clientX - rect.left;
            var y = e.clientY - rect.top;
            var centerX = rect.width / 2;
            var centerY = rect.height / 2;
            var rotateX = (y - centerY) / centerY * -8;
            var rotateY = (x - centerX) / centerX * 8;
            card.style.transform = 'perspective(1000px) rotateX(' + rotateX + 'deg) rotateY(' + rotateY + 'deg) scale3d(1.01, 1.01, 1.01)';
        });
        card.addEventListener('mouseleave', function () {
            card.style.transform = 'perspective(1000px) rotateX(0deg) rotateY(0deg) scale3d(1, 1, 1)';
            card.style.transition = 'transform 0.4s ease';
        });
        card.addEventListener('mouseenter', function () {
            card.style.transition = 'none';
        });
    });
};

// ═══════════════════════════════════════════════════════════
// STAGGER REVEAL
// ═══════════════════════════════════════════════════════════
window.staggerReveal = function (selector, delay) {
    if (typeof gsap === 'undefined') return;
    gsap.from(selector, {
        y: 30,
        opacity: 0,
        duration: 0.5,
        stagger: delay || 0.08,
        ease: 'power2.out'
    });
};

// ═══════════════════════════════════════════════════════════
// COUNT UP
// ═══════════════════════════════════════════════════════════
window.countUp = function (elementId, target, duration) {
    if (typeof gsap === 'undefined') return;
    var el = document.getElementById(elementId);
    if (!el) return;
    var obj = { val: 0 };
    gsap.to(obj, {
        val: target,
        duration: (duration || 1500) / 1000,
        ease: 'power2.out',
        onUpdate: function () {
            el.textContent = Math.floor(obj.val).toLocaleString('fa-IR');
        }
    });
};

// ═══════════════════════════════════════════════════════════
// PARALLAX SCROLL (Optimized - No Lenis for performance)
// ═══════════════════════════════════════════════════════════
var _parallaxTrigger = null;

window.initParallaxScroll = function (dotNetRef) {
    if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
    gsap.registerPlugin(ScrollTrigger);

    var triggerElement = document.querySelector('[data-parallax-layers]');
    if (!triggerElement) return;

    // Skip parallax on mobile for performance
    if (window.innerWidth < 768) return;

    var tl = gsap.timeline({
        scrollTrigger: {
            trigger: triggerElement,
            start: 'top top',
            end: 'bottom top',
            scrub: 1
        }
    });

    var layers = [
        { layer: '1', yPercent: 30 },
        { layer: '2', yPercent: 20 },
        { layer: '3', yPercent: 10 },
        { layer: '4', yPercent: 5 }
    ];

    layers.forEach(function (layerObj, idx) {
        tl.to(
            triggerElement.querySelectorAll('[data-parallax-layer="' + layerObj.layer + '"]'),
            { yPercent: layerObj.yPercent, ease: 'none' },
            idx === 0 ? undefined : '<'
        );
    });

    _parallaxTrigger = tl;
};

window.disposeParallaxScroll = function () {
    if (_parallaxTrigger) {
        ScrollTrigger.getAll().forEach(function (st) { st.kill(); });
        _parallaxTrigger = null;
    }
};

// ═══════════════════════════════════════════════════════════
// ARCHITECTURE SCROLL (Optimized)
// ═══════════════════════════════════════════════════════════
var _archTimeline = null;
var _typingDone = false;

window.initArchitectureScroll = function (dotNetRef) {
    if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
    gsap.registerPlugin(ScrollTrigger);

    var typingTarget = 'با به‌روزرسانی‌های فوری و بی‌نقص دفتر کل مالی در ERP سازمانی MIS.';

    _archTimeline = gsap.timeline({
        scrollTrigger: {
            trigger: '#architecture',
            start: 'top top',
            end: '+=200%',
            pin: '#archPin',
            scrub: 1
        }
    });

    function addClass(id, cls) { var el = document.getElementById(id); if (el) el.classList.add(cls); }
    function removeClass(id, cls) { var el = document.getElementById(id); if (el) el.classList.remove(cls); }

    _archTimeline
        .to('#archStep1', { opacity: 1, pointerEvents: 'auto', duration: 0.15 }, 0)
        .call(function() { addClass('nodeHIS', 'active'); addClass('nodeHISm', 'active'); }, null, 0)
        .to('#archStep1', { opacity: 0, pointerEvents: 'none', duration: 0.1 }, 0.25)
        .call(function() { removeClass('nodeHIS', 'active'); removeClass('nodeHISm', 'active'); }, null, 0.35)
        .to('#archStep2', { opacity: 1, pointerEvents: 'auto', duration: 0.15 }, 0.3)
        .call(function() {
            addClass('nodeHIS', 'active'); addClass('nodeHISm', 'active');
            addClass('nodeRIS', 'active'); addClass('nodeRISm', 'active');
            addClass('line1', 'active'); addClass('line1m', 'active');
        }, null, 0.3)
        .to('#archStep2', { opacity: 0, pointerEvents: 'none', duration: 0.1 }, 0.55)
        .call(function() {
            removeClass('nodeHIS', 'active'); removeClass('nodeHISm', 'active');
            removeClass('nodeRIS', 'active'); removeClass('nodeRISm', 'active');
            removeClass('line1', 'active'); removeClass('line1m', 'active');
        }, null, 0.65)
        .to('#archStep3', { opacity: 1, pointerEvents: 'auto', duration: 0.15 }, 0.6)
        .call(function() {
            addClass('nodeRIS', 'active'); addClass('nodeRISm', 'active');
            addClass('nodeMIS', 'active'); addClass('nodeMISm', 'active');
            addClass('line2', 'active'); addClass('line2m', 'active');
        }, null, 0.6);

    ScrollTrigger.create({
        trigger: '#archStep3',
        start: 'top 70%',
        onEnter: function () {
            if (_typingDone) return;
            _typingDone = true;
            if (typeof TextPlugin !== 'undefined') {
                gsap.to('#typingText', { text: typingTarget, duration: typingTarget.length * 0.03, ease: 'none' });
            }
        }
    });
};

window.disposeArchitectureScroll = function () {
    _typingDone = false;
    if (_archTimeline) {
        ScrollTrigger.getAll().forEach(function (st) { st.kill(); });
        _archTimeline = null;
    }
};

// ═══════════════════════════════════════════════════════════
// TYPEWRITER EFFECT
// ═══════════════════════════════════════════════════════════
window.initTypewriterEffect = function (text) {
    if (typeof gsap === 'undefined' || typeof TextPlugin === 'undefined') return;
    gsap.registerPlugin(TextPlugin);

    var el = document.getElementById('typingText');
    if (!el) return;

    gsap.to(el, { text: text, duration: text.length * 0.03, ease: 'none' });
};

// ═══════════════════════════════════════════════════════════
// PRODUCT CARD GLOW (Optimized)
// ═══════════════════════════════════════════════════════════
var _glowThrottle = null;
window.initProductCardGlow = function () {
    document.querySelectorAll('.product-card-item').forEach(function (card) {
        var glow = card.querySelector('[data-glow]');
        if (!glow) return;

        card.addEventListener('mousemove', function (e) {
            if (_glowThrottle) return;
            _glowThrottle = setTimeout(function () { _glowThrottle = null; }, 16);
            var rect = card.getBoundingClientRect();
            var x = e.clientX - rect.left;
            var y = e.clientY - rect.top;
            glow.style.background = 'radial-gradient(circle 300px at ' + x + 'px ' + y + 'px, rgba(0,112,243,0.08), transparent)';
        });
    });
};

// ═══════════════════════════════════════════════════════════
// LENIS SMOOTH SCROLL (Optional - disabled by default)
// ═══════════════════════════════════════════════════════════
var _mainLenis = null;

window.initSmoothScroll = function () {
    // Lenis disabled by default for performance
    // Enable only on high-end devices
    if (typeof Lenis === 'undefined') return;
    if (window.innerWidth < 1024) return;

    try {
        _mainLenis = new Lenis({
            duration: 1.0,
            easing: function (t) { return Math.min(1, 1.001 - Math.pow(2, -10 * t)); },
            smoothWheel: true
        });

        if (typeof ScrollTrigger !== 'undefined') {
            _mainLenis.on('scroll', ScrollTrigger.update);
        }

        gsap.ticker.add(function (time) { _mainLenis.raf(time * 1000); });
        gsap.ticker.lagSmoothing(0);
    } catch (e) {
        console.warn('Lenis init failed:', e);
    }
};

window.disposeSmoothScroll = function () {
    if (_mainLenis) {
        _mainLenis.destroy();
        _mainLenis = null;
    }
};

// ═══════════════════════════════════════════════════════════
// HOME PAGE ANIMATIONS
// ═══════════════════════════════════════════════════════════
window.home = {
    init: function () {
        if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
        gsap.registerPlugin(ScrollTrigger);

        this.heroAnimation();
        this.revealAnimation();
        this.parallaxHero();
        this.architectureStory();
        this.counters();
        this.mouseGlow();
        this.toolbarScroll();
    },

    heroAnimation: function () {
        var tl = gsap.timeline({ delay: 0.3 });
        tl.from('.hero-badge', { y: 20, opacity: 0, duration: 0.6, ease: 'power2.out' })
          .from('.title .line', { y: 60, opacity: 0, duration: 0.8, stagger: 0.15, ease: 'power3.out' }, '-=0.3')
          .from('.subtitle', { y: 30, opacity: 0, duration: 0.6, ease: 'power2.out' }, '-=0.4')
          .from('.hero-actions', { y: 20, opacity: 0, duration: 0.5, ease: 'power2.out' }, '-=0.2')
          .from('.hero-stats', { y: 20, opacity: 0, duration: 0.5, ease: 'power2.out' }, '-=0.2')
          .from('.float-card', { scale: 0.8, opacity: 0, duration: 0.5, stagger: 0.15, ease: 'back.out(1.4)' }, '-=0.3')
          .from('.scroll-indicator', { opacity: 0, duration: 0.5 }, '-=0.1');
        this.initHeroGlow();
    },

    initHeroGlow: function () {
        var glow = document.getElementById('heroGlow');
        var hero = document.getElementById('hero');
        if (!glow || !hero) return;
        var throttle = null;
        hero.addEventListener('mousemove', function (e) {
            if (throttle) return;
            throttle = setTimeout(function () { throttle = null; }, 16);
            var rect = hero.getBoundingClientRect();
            glow.style.left = (e.clientX - rect.left) + 'px';
            glow.style.top = (e.clientY - rect.top) + 'px';
        });
    },

    revealAnimation: function () {
        gsap.utils.toArray('.reveal').forEach(function (el) {
            gsap.from(el, {
                y: 50, opacity: 0, duration: 0.8, ease: 'power2.out',
                scrollTrigger: { trigger: el, start: 'top 85%', toggleActions: 'play none none none' }
            });
        });
    },

    parallaxHero: function () {
        gsap.to('.hero-bg', {
            backgroundPosition: '50% 100%', ease: 'none',
            scrollTrigger: { trigger: '.hero', start: 'top top', end: 'bottom top', scrub: true }
        });
    },

    architectureStory: function () {
        var steps = gsap.utils.toArray('.arch-step');
        var nodes = document.querySelectorAll('.arch-node');
        var connectors = document.querySelectorAll('.arch-connector');

        steps.forEach(function (el, i) {
            gsap.to(el, {
                opacity: 1, y: 0, duration: 0.5,
                scrollTrigger: {
                    trigger: '.architecture',
                    start: function () { return 'top -' + (i * 100); },
                    end: function () { return 'top -' + ((i + 1) * 100); },
                    scrub: true,
                    onEnter: function () {
                        if (nodes[i]) nodes[i].classList.add('active');
                        if (connectors[i]) connectors[i].classList.add('active');
                    },
                    onLeaveBack: function () {
                        if (nodes[i]) nodes[i].classList.remove('active');
                        if (connectors[i]) connectors[i].classList.remove('active');
                    }
                }
            });
            if (i > 0) {
                gsap.to(steps[i - 1], {
                    opacity: 0, y: -30, duration: 0.3,
                    scrollTrigger: { trigger: '.architecture', start: function () { return 'top -' + (i * 100); }, scrub: true }
                });
            }
        });
    },

    counters: function () {
        gsap.utils.toArray('[data-count]').forEach(function (el) {
            var target = parseInt(el.getAttribute('data-count'));
            var obj = { val: 0 };
            gsap.to(obj, {
                val: target, duration: 2, ease: 'power2.out',
                scrollTrigger: { trigger: el, start: 'top 85%' },
                onUpdate: function () { el.textContent = Math.floor(obj.val).toLocaleString('fa-IR') + '+'; }
            });
        });
    },

    mouseGlow: function () {
        var throttle = null;
        document.addEventListener('mousemove', function (e) {
            if (throttle) return;
            throttle = setTimeout(function () { throttle = null; }, 16);
            document.querySelectorAll('.card, .project').forEach(function (el) {
                var rect = el.getBoundingClientRect();
                var x = e.clientX - rect.left;
                var y = e.clientY - rect.top;
                if (x >= 0 && x <= rect.width && y >= 0 && y <= rect.height) {
                    el.style.background = 'radial-gradient(circle 300px at ' + x + 'px ' + y + 'px, rgba(0,112,243,0.06), rgba(255,255,255,0.03))';
                } else {
                    el.style.background = '';
                }
            });
        });
    },

    toolbarScroll: function () {
        var toolbar = document.getElementById('toolbar');
        if (!toolbar) return;
        window.addEventListener('scroll', function () {
            if (window.scrollY > 50) { toolbar.classList.add('scrolled'); }
            else { toolbar.classList.remove('scrolled'); }
        });
    }
};
