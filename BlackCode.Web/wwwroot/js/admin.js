document.addEventListener('DOMContentLoaded', () => {
    const toggle = document.querySelector('[data-admin-menu-toggle]');
    const navigation = document.getElementById('admin-navigation');
    if (!toggle || !navigation) return;
    toggle.addEventListener('click', () => {
        const open = toggle.getAttribute('aria-expanded') !== 'true';
        toggle.setAttribute('aria-expanded', String(open));
        toggle.setAttribute('aria-label', open ? 'Cerrar menú de administración' : 'Abrir menú de administración');
        navigation.classList.toggle('is-open', open);
    });
});
