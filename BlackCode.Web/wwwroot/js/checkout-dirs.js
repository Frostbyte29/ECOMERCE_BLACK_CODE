(function () {
    var sel = document.getElementById('dir-guardada');
    if (!sel) return;
    sel.addEventListener('change', function () {
        var opt = sel.options[sel.selectedIndex];
        if (!opt.value) return;
        document.getElementById('Nombre').value = opt.dataset.nombre || '';
        document.getElementById('Telefono').value = opt.dataset.tel || '';
        document.getElementById('Direccion').value = opt.dataset.dir || '';
        document.getElementById('Distrito').value = opt.dataset.dist || '';
        var refEl = document.getElementById('Referencia');
        if (refEl) refEl.value = opt.dataset.ref || '';
    });
}());
