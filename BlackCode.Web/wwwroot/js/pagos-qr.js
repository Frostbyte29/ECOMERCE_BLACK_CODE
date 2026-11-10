(() => {
    const selector = document.querySelector('[data-metodo-pago]');
    if (!selector) return;

    const codigos = document.querySelectorAll('[data-qr-metodo]');
    const mostrarSeleccionado = () => {
        codigos.forEach(codigo => {
            codigo.hidden = codigo.dataset.qrMetodo !== selector.value;
        });
    };

    selector.addEventListener('change', mostrarSeleccionado);
    window.addEventListener('pageshow', mostrarSeleccionado);
    mostrarSeleccionado();
})();
