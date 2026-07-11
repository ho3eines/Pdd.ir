/* ============================================================
   SPLASH ANIMATION ENGINE
   Pdd.ir | Cinematic Brand Reveal
   Timeline: ~3.2s (4 phases)
   ============================================================ */

(function () {
    "use strict";

    const STORAGE_KEY = "pdd_splash_completed";
    const TOTAL_DURATION = 3200; // ms

    // ---------- Helpers ----------
    const $ = (sel, ctx = document) => ctx.querySelector(sel);
    const $$ = (sel, ctx = document) => Array.from(ctx.querySelectorAll(sel));

    // ---------- Particle System ----------
    function createParticleCanvas() {
        const canvas = document.createElement("canvas");
        canvas.id = "splash-particles";
        const container = $("#splash-container");
        if (!container) return null;
        container.insertBefore(canvas, container.firstChild);
        return canvas;
    }

    function initParticles(canvas) {
        if (!canvas) return null;
        const ctx = canvas.getContext("2d");
        let particles = [];
        const dpr = window.devicePixelRatio || 1;
        const count = window.innerWidth < 576 ? 40 : 80;

        function resize() {
            canvas.width = window.innerWidth * dpr;
            canvas.height = window.innerHeight * dpr;
            canvas.style.width = window.innerWidth + "px";
            canvas.style.height = window.innerHeight + "px";
        }

        function spawn() {
            particles = [];
            for (let i = 0; i < count; i++) {
                particles.push({
                    x: Math.random() * canvas.width,
                    y: Math.random() * canvas.height,
                    vx: (Math.random() - 0.5) * 0.4 * dpr,
                    vy: (Math.random() - 0.5) * 0.4 * dpr,
                    r: (Math.random() * 1.6 + 0.4) * dpr,
                    a: Math.random() * 0.6 + 0.2,
                    hue: Math.random() < 0.7 ? 215 : 38 // blue or orange
                });
            }
        }

        function draw() {
            if (!canvas.isConnected) return;
            ctx.clearRect(0, 0, canvas.width, canvas.height);
            particles.forEach(p => {
                p.x += p.vx;
                p.y += p.vy;
                if (p.x < 0) p.x = canvas.width;
                if (p.x > canvas.width) p.x = 0;
                if (p.y < 0) p.y = canvas.height;
                if (p.y > canvas.height) p.y = 0;

                const color = p.hue === 215
                    ? `rgba(13, 110, 253, ${p.a})`
                    : `rgba(255, 176, 32, ${p.a})`;

                ctx.beginPath();
                ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2);
                ctx.fillStyle = color;
                ctx.shadowColor = color;
                ctx.shadowBlur = 8;
                ctx.fill();
            });
            requestAnimationFrame(draw);
        }

        resize();
        spawn();
        draw();

        window.addEventListener("resize", () => {
            resize();
            spawn();
        });

        return { ctx, particles };
    }

    // ---------- Main Splash API ----------
    window.splash = {
        _timeline: null,
        _dotNetRef: null,
        _skipped: false,

        /**
         * Check whether splash should be shown
         */
        shouldShow: function () {
            try {
                return !localStorage.getItem(STORAGE_KEY);
            } catch (e) {
                return true;
            }
        },

        /**
         * Start the cinematic animation
         * @param {Object} dotNetRef - DotNetObjectReference from Blazor (optional)
         */
        start: function (dotNetRef) {
            this._dotNetRef = dotNetRef || null;
            this._skipped = false;

            // Wait for DOM ready
            if (document.readyState === "loading") {
                document.addEventListener("DOMContentLoaded", () => this._play());
            } else {
                this._play();
            }
        },

        /**
         * Internal: build the timeline
         */
        _play: function () {
            const container = $("#splash-container");
            if (!container || typeof gsap === "undefined") {
                this._finish();
                return;
            }

            // Init particle system
            const canvas = createParticleCanvas();
            initParticles(canvas);

            // Setup Skip Button
            const skipBtn = $(".splash-skip", container);
            const skipHandler = (e) => {
                if (e) e.preventDefault();
                this.skip();
            };
            if (skipBtn) {
                skipBtn.addEventListener("click", skipHandler);
            }

            // ESC key to skip
            const escHandler = (e) => {
                if (e.key === "Escape" || e.key === "Enter" || e.key === " ") {
                    this.skip();
                }
            };
            window.addEventListener("keydown", escHandler);

            this._cleanupSkip = () => {
                if (skipBtn) skipBtn.removeEventListener("click", skipHandler);
                window.removeEventListener("keydown", escHandler);
            };

            // Reveal container
            container.classList.add("is-ready");

            const tl = gsap.timeline({
                defaults: { ease: "power3.out" },
                onComplete: () => {
                    this._finish();
                }
            });

            this._timeline = tl;

            // ===========================================
            // PHASE 1: ENTRANCE (0s - 0.7s)
            // ===========================================
            tl
                // Center orb appears
                .fromTo(".splash-orb", {
                    scale: 0,
                    opacity: 0
                }, {
                    scale: 1,
                    opacity: 1,
                    duration: 0.7,
                    ease: "power2.out"
                }, 0)

                // Corner marks draw in
                .to(".corner-mark", {
                    opacity: 0.7,
                    duration: 0.5,
                    stagger: 0.05
                }, 0.2)

                // Light ring fades in
                .to(".light-ring", {
                    opacity: 0.6,
                    duration: 0.6
                }, 0.3)

                .to(".light-ring-inner", {
                    opacity: 0.7,
                    duration: 0.6
                }, 0.4);

            // ===========================================
            // PHASE 2: LOGO FORMATION (0.7s - 1.8s)
            // ===========================================
            tl
                .to("#brand-logo", {
                    opacity: 1,
                    scale: 1,
                    rotation: 0,
                    duration: 1.0,
                    ease: "elastic.out(1, 0.6)"
                }, 0.7)

                // Logo reflection fades
                .to(".logo-reflection", {
                    opacity: 0.6,
                    duration: 0.6
                }, 1.2);

            // ===========================================
            // PHASE 3: COMPLETION & PULSE (1.8s - 2.5s)
            // ===========================================
            tl
                // Brand name slides up
                .fromTo(".brand-text", {
                    opacity: 0,
                    y: 20
                }, {
                    opacity: 1,
                    y: 0,
                    duration: 0.6,
                    ease: "power2.out"
                }, 1.8)

                // Loading line appears
                .to(".loading-line", {
                    opacity: 1,
                    duration: 0.4
                }, 2.0)

                .to(".loading-text", {
                    opacity: 1,
                    duration: 0.3
                }, 2.1)

                // Animate loading bar
                .to(".loading-line::before", {
                    width: "100%",
                    duration: 1.0,
                    ease: "power1.inOut",
                    onUpdate: function () {
                        const progress = Math.round(this.progress() * 100);
                        const textEl = $(".loading-text");
                        if (textEl) textEl.textContent = progress + "%";
                    }
                }, 2.1)

                // Pulse waves emit from logo
                .to(".pulse-wave", {
                    opacity: 0.6,
                    scale: 2.5,
                    duration: 0.8,
                    stagger: 0.15,
                    ease: "power2.out"
                }, 2.1)

                .to(".pulse-wave", {
                    opacity: 0,
                    scale: 3.5,
                    duration: 0.6,
                    stagger: 0.15,
                    ease: "power2.out"
                }, 2.4)

                // Skip button appears
                .to(".splash-skip", {
                    opacity: 1,
                    y: 0,
                    duration: 0.4
                }, 2.6)

                // Light sweep passes over logo
                .to(".light-sweep", {
                    left: "120%",
                    opacity: 1,
                    duration: 0.8,
                    ease: "power2.inOut"
                }, 2.5)
                .to(".light-sweep", {
                    opacity: 0,
                    duration: 0.3
                }, 3.2);

            // ===========================================
            // PHASE 4: EXIT (2.5s - 3.5s)
            // ===========================================
            tl
                .to(".logo-stage", {
                    scale: 1.1,
                    duration: 0.3,
                    ease: "power2.out"
                }, 3.2)

                .to([".brand-text", ".loading-line", ".loading-text"], {
                    opacity: 0,
                    y: -10,
                    duration: 0.3
                }, 3.3)

                .to("#splash-container", {
                    opacity: 0,
                    scale: 1.05,
                    duration: 0.6,
                    ease: "power3.inOut",
                    onStart: () => {
                        if (container) container.classList.add("is-leaving");
                    }
                }, 3.5);
        },

        /**
         * Skip the animation gracefully
         */
        skip: function () {
            if (this._skipped || !this._timeline) {
                this._finish();
                return;
            }
            this._skipped = true;
            const container = $("#splash-container");
            if (container) container.classList.add("is-leaving");

            // Quick fade out
            if (this._timeline) this._timeline.kill();

            gsap && gsap.to("#splash-container", {
                opacity: 0,
                scale: 1.02,
                duration: 0.35,
                ease: "power2.inOut",
                onComplete: () => this._finish()
            });
        },

        /**
         * Mark splash as completed (call from Blazor)
         */
        completed: function () {
            try {
                localStorage.setItem(STORAGE_KEY, JSON.stringify(Date.now()));
            } catch (e) { /* private mode */ }
        },

        /**
         * Reset splash (for testing)
         */
        reset: function () {
            try {
                localStorage.removeItem(STORAGE_KEY);
            } catch (e) { /* */ }
        },

        /**
         * Internal: fire callback and persist
         */
        _finish: function () {
            if (this._cleanupSkip) this._cleanupSkip();

            // Notify Blazor
            if (this._dotNetRef && this._dotNetRef.invokeMethodAsync) {
                try {
                    this._dotNetRef.invokeMethodAsync("OnAnimationComplete");
                } catch (e) { /* component may be disposed */ }
            }
            this._dotNetRef = null;

            // Persist completion
            this.completed();
        }
    };

})();
