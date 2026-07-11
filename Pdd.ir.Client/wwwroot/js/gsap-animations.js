// GSAP Animation Functions for Pdd.ir

window.initScrollAnimations = function () {
    if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
    gsap.registerPlugin(ScrollTrigger);

    gsap.utils.toArray('.anim-fade-up').forEach(function (el) {
        gsap.from(el, {
            y: 60,
            opacity: 0,
            duration: 0.8,
            ease: 'power3.out',
            scrollTrigger: { trigger: el, start: 'top 85%', toggleActions: 'play none none none' }
        });
    });

    gsap.utils.toArray('.anim-fade-in').forEach(function (el) {
        gsap.from(el, {
            opacity: 0,
            duration: 0.8,
            ease: 'power2.out',
            scrollTrigger: { trigger: el, start: 'top 85%', toggleActions: 'play none none none' }
        });
    });

    gsap.utils.toArray('.anim-slide-left').forEach(function (el) {
        gsap.from(el, {
            x: -80,
            opacity: 0,
            duration: 0.8,
            ease: 'power3.out',
            scrollTrigger: { trigger: el, start: 'top 85%', toggleActions: 'play none none none' }
        });
    });

    gsap.utils.toArray('.anim-slide-right').forEach(function (el) {
        gsap.from(el, {
            x: 80,
            opacity: 0,
            duration: 0.8,
            ease: 'power3.out',
            scrollTrigger: { trigger: el, start: 'top 85%', toggleActions: 'play none none none' }
        });
    });

    gsap.utils.toArray('.anim-scale-up').forEach(function (el) {
        gsap.from(el, {
            scale: 0.8,
            opacity: 0,
            duration: 0.8,
            ease: 'back.out(1.7)',
            scrollTrigger: { trigger: el, start: 'top 85%', toggleActions: 'play none none none' }
        });
    });
};

window.initMouseParallax = function () {
    document.addEventListener('mousemove', function (e) {
        var cards = document.querySelectorAll('.parallax-card');
        cards.forEach(function (card) {
            var rect = card.getBoundingClientRect();
            var x = (e.clientX - rect.left - rect.width / 2) / rect.width;
            var y = (e.clientY - rect.top - rect.height / 2) / rect.height;
            card.style.transform = 'translate(' + (x * 20) + 'px, ' + (y * 20) + 'px)';
        });
    });
};

window.initCounters = function () {
    if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;
    gsap.registerPlugin(ScrollTrigger);

    gsap.utils.toArray('[data-count]').forEach(function (el) {
        var target = parseInt(el.getAttribute('data-count'));
        var obj = { val: 0 };
        gsap.to(obj, {
            val: target,
            duration: 2,
            ease: 'power2.out',
            scrollTrigger: { trigger: el, start: 'top 85%' },
            onUpdate: function () {
                el.textContent = Math.floor(obj.val).toLocaleString('fa-IR');
            }
        });
    });
};

window.initHeroAnimation = function () {
    if (typeof gsap === 'undefined') return;

    var tl = gsap.timeline();
    tl.from('.hero-title', { y: 50, opacity: 0, duration: 0.8, ease: 'power3.out' })
      .from('.hero-subtitle', { y: 30, opacity: 0, duration: 0.6, ease: 'power3.out' }, '-=0.4')
      .from('.hero-desc', { y: 20, opacity: 0, duration: 0.6, ease: 'power3.out' }, '-=0.3')
      .from('.hero-actions', { y: 20, opacity: 0, duration: 0.6, ease: 'power3.out' }, '-=0.2')
      .from('.floating-card', { scale: 0, opacity: 0, duration: 0.5, ease: 'back.out(1.7)', stagger: 0.2 }, '-=0.3');
};

window.initTiltEffect = function () {
    document.querySelectorAll('.tilt-card').forEach(function (card) {
        card.addEventListener('mousemove', function (e) {
            var rect = card.getBoundingClientRect();
            var x = e.clientX - rect.left;
            var y = e.clientY - rect.top;
            var centerX = rect.width / 2;
            var centerY = rect.height / 2;
            var rotateX = (y - centerY) / centerY * -10;
            var rotateY = (x - centerX) / centerX * 10;
            card.style.transform = 'perspective(1000px) rotateX(' + rotateX + 'deg) rotateY(' + rotateY + 'deg) scale3d(1.02, 1.02, 1.02)';
        });
        card.addEventListener('mouseleave', function () {
            card.style.transform = 'perspective(1000px) rotateX(0deg) rotateY(0deg) scale3d(1, 1, 1)';
            card.style.transition = 'transform 0.5s ease';
        });
        card.addEventListener('mouseenter', function () {
            card.style.transition = 'none';
        });
    });
};

window.staggerReveal = function (selector, delay) {
    if (typeof gsap === 'undefined') return;
    gsap.from(selector, {
        y: 40,
        opacity: 0,
        duration: 0.6,
        stagger: delay || 0.1,
        ease: 'power3.out'
    });
};

window.countUp = function (elementId, target, duration) {
    if (typeof gsap === 'undefined') return;
    var el = document.getElementById(elementId);
    if (!el) return;
    var obj = { val: 0 };
    gsap.to(obj, {
        val: target,
        duration: (duration || 2000) / 1000,
        ease: 'power2.out',
        onUpdate: function () {
            el.textContent = Math.floor(obj.val).toLocaleString('fa-IR');
        }
    });
};
