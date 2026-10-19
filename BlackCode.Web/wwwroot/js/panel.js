// Panel: pide confirmación antes de enviar los formularios marcados con data-confirmar.
document.addEventListener("submit", function (evento) {
    var mensaje = evento.target.getAttribute("data-confirmar");
    if (mensaje && !window.confirm(mensaje)) evento.preventDefault();
});
