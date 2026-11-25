(function () {
    var selectMetodo = document.querySelector('[data-metodo-pago]');
    var tarjetaDiv = document.getElementById('tarjeta-form');
    var operacionDiv = document.getElementById('operacion-form');
    var cardNumero = document.getElementById('card-numero');
    var cardExpiry = document.getElementById('card-expiry');
    var cardCvv = document.getElementById('card-cvv');
    var marcaInput = document.querySelector('input[name="TarjetaMarca"]');
    var ultimosInput = document.querySelector('input[name="TarjetaUltimos"]');
    var form = document.querySelector('.checkout-layout');

    function luhn(numero) {
        var num = numero.replace(/\D/g, '');
        var suma = 0, alt = false;
        for (var i = num.length - 1; i >= 0; i--) {
            var d = parseInt(num[i], 10);
            if (alt) { d *= 2; if (d > 9) d -= 9; }
            suma += d;
            alt = !alt;
        }
        return num.length >= 13 && suma % 10 === 0;
    }

    function detectarMarca(num) {
        if (/^4/.test(num)) return 'Visa';
        if (/^5[1-5]/.test(num) || /^2[2-7]/.test(num)) return 'Mastercard';
        if (/^3[47]/.test(num)) return 'Amex';
        return 'Tarjeta';
    }

    function actualizarVisibilidad() {
        var metodo = selectMetodo ? selectMetodo.value : 'tarjeta';
        if (tarjetaDiv) tarjetaDiv.hidden = metodo !== 'tarjeta';
        if (operacionDiv) operacionDiv.hidden = metodo === 'tarjeta';
    }

    if (selectMetodo) selectMetodo.addEventListener('change', actualizarVisibilidad);
    actualizarVisibilidad();

    if (cardNumero) {
        cardNumero.addEventListener('input', function () {
            var val = cardNumero.value.replace(/\D/g, '').slice(0, 16);
            cardNumero.value = val.replace(/(.{4})/g, '$1 ').trim();
        });
    }
    if (cardExpiry) {
        cardExpiry.addEventListener('input', function () {
            var val = cardExpiry.value.replace(/\D/g, '').slice(0, 4);
            cardExpiry.value = val.length > 2 ? val.slice(0, 2) + '/' + val.slice(2) : val;
        });
    }

    if (form) {
        form.addEventListener('submit', function (e) {
            if (!tarjetaDiv || tarjetaDiv.hidden) return;
            var num = cardNumero ? cardNumero.value.replace(/\D/g, '') : '';
            if (!luhn(num)) {
                e.preventDefault();
                if (cardNumero) { cardNumero.focus(); cardNumero.setCustomValidity('El número de tarjeta no es válido.'); cardNumero.reportValidity(); }
                return;
            }
            if (marcaInput) marcaInput.value = detectarMarca(num);
            if (ultimosInput) ultimosInput.value = num.slice(-4);
        });
        if (cardNumero) cardNumero.addEventListener('input', function () { cardNumero.setCustomValidity(''); });
    }
}());
