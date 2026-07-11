// Pdd.ir Interop Functions

window.initAnimations = function () {
    // Simple reveal animation
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

window.setFocus = function (elementId) {
    var el = document.getElementById(elementId);
    if (el) {
        el.focus();
    }
};
