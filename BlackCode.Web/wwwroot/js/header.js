'use strict';
(function () {
    var btnBuscar   = document.getElementById('btn-buscar');
    var barraDiv    = document.getElementById('barra-busqueda');
    var inputBuscar = document.getElementById('barra-busqueda-input');
    var btnCerrar   = document.getElementById('btn-cerrar-busqueda');

    if (!btnBuscar || !barraDiv) return;

    function abrir() {
        barraDiv.removeAttribute('hidden');
        btnBuscar.setAttribute('aria-expanded', 'true');
        if (inputBuscar) inputBuscar.focus();
    }

    function cerrar() {
        barraDiv.setAttribute('hidden', '');
        btnBuscar.setAttribute('aria-expanded', 'false');
        btnBuscar.focus();
    }

    btnBuscar.addEventListener('click', function () {
        if (barraDiv.hasAttribute('hidden')) abrir(); else cerrar();
    });

    if (btnCerrar) btnCerrar.addEventListener('click', cerrar);

    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && !barraDiv.hasAttribute('hidden')) cerrar();
    });
}());
