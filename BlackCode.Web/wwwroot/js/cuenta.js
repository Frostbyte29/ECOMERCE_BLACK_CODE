// Pantallas de cuenta: botón "ojo" para mostrar u ocultar la contraseña
// y nota de ayuda de "¿Olvidaste tu contraseña?".
document.addEventListener("click", function (evento) {
    var ojo = evento.target.closest("[data-mostrar-contrasena]");
    if (ojo) {
        var campo = document.getElementById(ojo.getAttribute("data-mostrar-contrasena"));
        if (!campo) return;
        var mostrar = campo.type === "password";
        campo.type = mostrar ? "text" : "password";
        ojo.setAttribute("aria-pressed", mostrar ? "true" : "false");
        ojo.setAttribute("aria-label", mostrar ? "Ocultar contraseña" : "Mostrar contraseña");
        return;
    }

    var ayuda = evento.target.closest("[data-alternar-ayuda]");
    if (ayuda) {
        evento.preventDefault();
        var nota = document.getElementById(ayuda.getAttribute("data-alternar-ayuda"));
        if (nota) nota.hidden = !nota.hidden;
    }
});
