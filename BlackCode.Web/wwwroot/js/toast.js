(() => {
    const region = document.getElementById('toast-region');
    if (!region) return;

    function attach(toast) {
        const timeout = toast.classList.contains('toast-error') ? 8000 : 5000;
        let timer;

        const close = () => {
            if (toast.classList.contains('is-leaving')) return;
            clearTimeout(timer);
            toast.classList.add('is-leaving');
            if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) toast.remove();
            else window.setTimeout(() => toast.remove(), 220);
        };
        const pause = () => clearTimeout(timer);
        const resume = () => { clearTimeout(timer); timer = window.setTimeout(close, timeout); };

        toast.querySelector('[data-toast-close]')?.addEventListener('click', close);
        toast.addEventListener('mouseenter', pause);
        toast.addEventListener('mouseleave', resume);
        toast.addEventListener('focusin', pause);
        toast.addEventListener('focusout', event => {
            if (!toast.contains(event.relatedTarget)) resume();
        });
        resume();
    }

    region.querySelectorAll('[data-toast]').forEach(attach);

    window.BlackCodeToast = {
        show(type, message) {
            const toast = document.createElement('div');
            const error = type === 'error';
            toast.className = `toast ${error ? 'toast-error' : 'toast-success'}`;
            toast.dataset.toast = '';
            toast.setAttribute('role', error ? 'alert' : 'status');

            const symbol = document.createElement('span');
            symbol.className = 'toast-symbol';
            symbol.setAttribute('aria-hidden', 'true');
            symbol.textContent = error ? '!' : '✓';
            const copy = document.createElement('span');
            copy.className = 'toast-message';
            copy.textContent = message;
            const close = document.createElement('button');
            close.className = 'toast-close';
            close.type = 'button';
            close.dataset.toastClose = '';
            close.setAttribute('aria-label', 'Cerrar notificación');
            close.textContent = '×';

            toast.append(symbol, copy, close);
            region.append(toast);
            attach(toast);
        }
    };
})();
