window.splash = {

    shouldShow: function () {
        return !localStorage.getItem("splash_completed");
    },

    start: function () {
        let tl = gsap.timeline();

        tl.to("#brand-logo", {
            opacity: 1,
            scale: 1,
            duration: 1.2,
            ease: "elastic.out(1,.5)"
        })

        .to(".brand-name", {
            opacity: 1,
            y: -10,
            duration: .7
        })

        .to(".loading-line span", {
            width: "100%",
            duration: 1.5,
            ease: "power2.out"
        })

        .to("#splash-container", {
            opacity: 0,
            duration: .8,
            delay: .3
        });
    },

    completed: function () {
        localStorage.setItem("splash_completed", "true");
    }
}
