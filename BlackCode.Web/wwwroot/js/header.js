'use strict';

// Hamburguesa: alterna la visibilidad del menú en móvil
(function () {
    var btnNav  = document.getElementById('btn-nav');
    var mainNav = document.getElementById('main-nav');
    if (!btnNav || !mainNav) return;

    btnNav.addEventListener('click', function () {
        var abierta = mainNav.classList.toggle('nav-abierta');
        btnNav.setAttribute('aria-expanded', String(abierta));
    });

    document.addEventListener('click', function (e) {
        if (!btnNav.contains(e.target) && !mainNav.contains(e.target)) {
            mainNav.classList.remove('nav-abierta');
            btnNav.setAttribute('aria-expanded', 'false');
        }
    });
}());

// Barra de búsqueda
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
