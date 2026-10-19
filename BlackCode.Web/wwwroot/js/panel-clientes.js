// Interruptor de estado: al cambiar el checkbox envía el formulario padre.
document.querySelectorAll('.estado-form input[type="checkbox"]').forEach(function (chk) {
    chk.addEventListener('change', function () {
        this.closest('form').submit();
    });
});
