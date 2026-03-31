(() => {
    const registrations = new Map();

    function scheduleUpdate(id) {
        const entry = registrations.get(id);
        if (!entry || entry.pending) {
            return;
        }

        entry.pending = true;
        window.requestAnimationFrame(() => {
            const current = registrations.get(id);
            if (!current) {
                return;
            }

            current.pending = false;
            current.element.classList.toggle("is-condensed", window.scrollY > 24);
        });
    }

    window.cgtPageTopBar = {
        init(id, element) {
            if (!element) {
                return;
            }

            const onScroll = () => scheduleUpdate(id);
            const registration = {
                element,
                onScroll,
                pending: false
            };

            registrations.set(id, registration);
            window.addEventListener("scroll", onScroll, { passive: true });
            window.addEventListener("resize", onScroll, { passive: true });
            scheduleUpdate(id);
        },
        dispose(id) {
            const entry = registrations.get(id);
            if (!entry) {
                return;
            }

            window.removeEventListener("scroll", entry.onScroll);
            window.removeEventListener("resize", entry.onScroll);
            registrations.delete(id);
        }
    };
})();
