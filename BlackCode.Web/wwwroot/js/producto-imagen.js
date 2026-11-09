(() => {
    const editor = document.querySelector('[data-image-editor]');
    if (!editor) return;
    const input = editor.querySelector('input[type=file]');
    const preview = editor.querySelector('img');
    const caption = editor.querySelector('[data-image-caption]');
    const originalCaption = caption.textContent;
    const error = editor.querySelector('#imagen-error');
    const reset = editor.querySelector('[data-image-reset]');
    let objectUrl;
    function restore() {
        if (objectUrl) URL.revokeObjectURL(objectUrl);
        objectUrl = null;
        preview.src = preview.dataset.currentImage;
        caption.textContent = originalCaption;
        reset.hidden = true;
    }
    input.addEventListener('change', () => {
        restore();
        input.setCustomValidity('');
        error.textContent = '';
        const file = input.files[0];
        if (!file) return;
        let message = '';
        if (!/\.(jpe?g|png|webp)$/i.test(file.name)) message = 'Selecciona una imagen JPG, PNG o WebP.';
        else if (!file.size || file.size > 5 * 1024 * 1024) message = 'La imagen debe contener datos y pesar como máximo 5 MB.';
        if (message) {
            error.textContent = message;
            input.setCustomValidity(message);
            return;
        }
        objectUrl = URL.createObjectURL(file);
        const candidate = objectUrl;
        const probe = new Image();
        probe.onload = () => {
            if (candidate !== objectUrl) return;
            preview.src = candidate;
            caption.textContent = file.name + ' · Vista previa, aún sin guardar';
            reset.hidden = false;
        };
        probe.onerror = () => {
            if (candidate !== objectUrl) return;
            restore();
            error.textContent = 'No se pudo abrir la imagen. Elige otro archivo.';
            input.setCustomValidity(error.textContent);
        };
        probe.src = candidate;
    });
    reset.addEventListener('click', () => {
        input.value = '';
        input.setCustomValidity('');
        error.textContent = '';
        restore();
    });
    window.addEventListener('pagehide', () => { if (objectUrl) URL.revokeObjectURL(objectUrl); });
})();
