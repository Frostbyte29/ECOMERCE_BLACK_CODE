// Menú de cuenta: abre/cierra el dropdown y cierra con clic afuera o Escape.
(function () {
    var btn = document.getElementById('menu-cuenta-btn');
    var dropdown = document.getElementById('menu-cuenta-dropdown');

    if (!btn || !dropdown) return;

    function abrir() {
        dropdown.removeAttribute('hidden');
        dropdown.classList.add('abierto');
        btn.setAttribute('aria-expanded', 'true');
    }

    function cerrar() {
        dropdown.setAttribute('hidden', '');
        dropdown.classList.remove('abierto');
        btn.setAttribute('aria-expanded', 'false');
    }

    btn.addEventListener('click', function (e) {
        e.stopPropagation();
        if (btn.getAttribute('aria-expanded') === 'true') {
            cerrar();
        } else {
            abrir();
        }
    });

    document.addEventListener('click', function (e) {
        if (!document.getElementById('menu-cuenta').contains(e.target)) {
            cerrar();
        }
    });

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') cerrar();
    });
}());
