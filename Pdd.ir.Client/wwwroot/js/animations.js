// GSAP-like animation helpers (lightweight, no GSAP dependency)

window.animateOnScroll = function () {
    var observer = new IntersectionObserver(function (entries) {
        entries.forEach(function (entry) {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate-visible');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    document.querySelectorAll('[data-animate]').forEach(function (el) {
        observer.observe(el);
    });
};

window.pulseElement = function (elementId) {
    var el = document.getElementById(elementId);
    if (el) {
        el.classList.add('pulse');
        setTimeout(function () {
            el.classList.remove('pulse');
        }, 600);
    }
};
