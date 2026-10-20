(() => {
    let activeOverlay = null;
    let restoreFocus = null;
    let suppressCatalogFocus = false;

    function openOverlay(overlay, trigger) {
        if (!overlay) return;
        if (activeOverlay) closeOverlay();
        restoreFocus = trigger || document.activeElement;
        overlay.hidden = false;
        activeOverlay = overlay;
        document.body.classList.add('overlay-open');
        const focusTarget = overlay.querySelector('input, [data-overlay-close], button, a');
        focusTarget?.focus();
    }

    function closeOverlay() {
        if (!activeOverlay) return;
        activeOverlay.hidden = true;
        activeOverlay = null;
        document.body.classList.remove('overlay-open');
        if (restoreFocus?.id === 'catalog-q') suppressCatalogFocus = true;
        restoreFocus?.focus?.();
        setTimeout(() => { suppressCatalogFocus = false; }, 0);
        restoreFocus = null;
    }

    document.addEventListener('click', event => {
        const close = event.target.closest('[data-overlay-close]');
        if (close) { closeOverlay(); return; }

        const favorite = event.target.closest('[data-favorite-open]');
        if (favorite) {
            const overlay = document.getElementById('favorite-overlay');
            const image = overlay?.querySelector('[data-favorite-image]');
            if (image && favorite.dataset.productImage) image.src = favorite.dataset.productImage;
            const returnUrl = location.pathname + location.search + location.hash;
            overlay?.querySelectorAll('[data-auth-link]').forEach(link => {
                const base = link.dataset.authLink === 'register' ? '/Cuenta/Registro' : '/Cuenta/Login';
                link.href = base + '?returnUrl=' + encodeURIComponent(returnUrl);
            });
            openOverlay(overlay, favorite);
            return;
        }

        const favoriteToggle = event.target.closest('[data-favorite-toggle]');
        if (favoriteToggle) { toggleFavorite(favoriteToggle); return; }

        const search = event.target.closest('[data-search-open]');
        if (search) { openSearch(search); return; }

        const gallery = event.target.closest('[data-gallery-open]');
        if (gallery) {
            const overlay = document.getElementById('gallery-overlay');
            const image = overlay?.querySelector('[data-gallery-image]');
            if (image) { image.src = gallery.dataset.gallerySrc; image.alt = gallery.dataset.galleryAlt || ''; }
            openOverlay(overlay, gallery);
            return;
        }

        const quantityButton = event.target.closest('[data-quantity-change]');
        if (quantityButton) {
            const input = quantityButton.parentElement.querySelector('input[type="number"]');
            if (!input) return;
            const min = Number(input.min) || 1;
            const max = Number(input.max) || 10;
            input.value = String(Math.min(max, Math.max(min, (Number(input.value) || min) + Number(quantityButton.dataset.quantityChange))));
            input.dispatchEvent(new Event('change', { bubbles: true }));
        }

        const cartQuantityButton = event.target.closest('[data-cart-quantity-change]');
        if (cartQuantityButton) {
            const input = cartQuantityButton.parentElement.querySelector('input[type="number"]');
            if (!input) return;
            const min = Number(input.min) || 1;
            const max = Number(input.max) || 10;
            const next = Math.min(max, Math.max(min, (Number(input.value) || min) + Number(cartQuantityButton.dataset.cartQuantityChange)));
            if (next !== Number(input.value)) {
                input.value = String(next);
                input.dispatchEvent(new Event('change', { bubbles: true }));
            }
        }

        const passwordButton = event.target.closest('[data-password-toggle]');
        if (passwordButton) {
            const input = document.getElementById(passwordButton.dataset.passwordToggle);
            if (!input) return;
            const visible = input.type === 'password';
            input.type = visible ? 'text' : 'password';
            passwordButton.setAttribute('aria-pressed', String(visible));
            passwordButton.setAttribute('aria-label', visible ? 'Ocultar contraseña' : 'Mostrar contraseña');
            input.focus();
        }
    });

    async function toggleFavorite(button) {
        if (button.disabled) return;
        const token = document.querySelector('#favorite-token input[name="__RequestVerificationToken"]')?.value;
        if (!token) return;
        button.disabled = true;
        try {
            const body = new URLSearchParams({ productId: button.dataset.productId, __RequestVerificationToken: token });
            const response = await fetch('/Favoritos/Cambiar', {
                method: 'POST',
                body,
                headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'Accept': 'application/json' }
            });
            if (!response.ok) throw new Error('Favorite unavailable');
            const data = await response.json();
            document.querySelectorAll(`[data-favorite-toggle][data-product-id="${button.dataset.productId}"]`).forEach(item => {
                item.classList.toggle('is-favorite', data.added);
                item.setAttribute('aria-pressed', String(data.added));
                item.setAttribute('aria-label', (data.added ? 'Quitar de favoritos' : 'Guardar en favoritos') + ': ' + data.product.name);
                item.title = data.added ? 'Quitar de favoritos' : 'Guardar en favoritos';
            });
            document.querySelectorAll('[data-favorite-count]').forEach(count => { count.textContent = data.count; });
            if (data.added) {
                const overlay = document.getElementById('favorite-saved-overlay');
                const image = overlay?.querySelector('[data-saved-image]');
                const name = overlay?.querySelector('[data-saved-name]');
                if (image) { image.src = data.product.imageUrl; image.alt = data.product.name; }
                if (name) name.textContent = data.product.name;
                openOverlay(overlay, button);
            } else if (location.pathname.toLowerCase().startsWith('/favoritos')) {
                location.reload();
            }
        } catch {
            const notice = document.createElement('div');
            notice.className = 'flash flash-error';
            notice.textContent = 'No pudimos actualizar tus favoritos. Inténtalo de nuevo.';
            document.querySelector('main')?.prepend(notice);
            setTimeout(() => notice.remove(), 5000);
        } finally {
            button.disabled = false;
        }
    }

    document.addEventListener('keydown', event => {
        if (event.key === 'Escape') { closeOverlay(); return; }
        if (event.key === '/' && !activeOverlay && !['INPUT', 'TEXTAREA'].includes(document.activeElement.tagName)) {
            event.preventDefault();
            openSearch(document.activeElement);
        }
        if (event.key === 'Tab' && activeOverlay) {
            const items = [...activeOverlay.querySelectorAll('a, button, input, select, textarea, [tabindex]:not([tabindex="-1"])')]
                .filter(element => !element.hidden && element.getClientRects().length);
            if (!items.length) return;
            const first = items[0];
            const last = items[items.length - 1];
            if (event.shiftKey && document.activeElement === first) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && document.activeElement === last) { event.preventDefault(); first.focus(); }
        }
    });

    const searchOverlay = document.getElementById('search-overlay');
    const searchInput = document.getElementById('global-search');
    const results = searchOverlay?.querySelector('.search-results');
    let searchTimer;
    let searchRequest;

    function openSearch(trigger) {
        openOverlay(searchOverlay, trigger);
        if (trigger?.id === 'catalog-q') searchInput.value = trigger.value;
        searchInput?.focus();
        updateSearch();
    }

    document.getElementById('catalog-q')?.addEventListener('focus', event => {
        if (!suppressCatalogFocus) openSearch(event.target);
    });
    searchInput?.addEventListener('input', () => {
        clearTimeout(searchTimer);
        searchRequest?.abort();
        searchTimer = setTimeout(updateSearch, 180);
    });

    async function updateSearch() {
        if (!results) return;
        searchRequest?.abort();
        searchRequest = new AbortController();
        const query = searchInput.value.trim();
        results.replaceChildren();
        const loading = document.createElement('p');
        loading.className = 'search-hint';
        loading.textContent = 'Buscando piezas...';
        results.append(loading);
        try {
            const response = await fetch('/Productos/Sugerencias?q=' + encodeURIComponent(query), { signal: searchRequest.signal });
            if (!response.ok) throw new Error('Search unavailable');
            const data = await response.json();
            results.replaceChildren();
            if (data.categories.length) {
                const heading = document.createElement('h3');
                heading.textContent = 'COLECCIONES';
                results.append(heading);
                const categories = document.createElement('div');
                categories.className = 'search-categories';
                data.categories.forEach(category => {
                    const link = document.createElement('a');
                    link.href = '/Productos?categoria=' + encodeURIComponent(category);
                    link.textContent = category + ' →';
                    categories.append(link);
                });
                results.append(categories);
            }
            const heading = document.createElement('h3');
            heading.textContent = query ? 'PRODUCTOS QUE COINCIDEN' : 'PIEZAS PARA EXPLORAR';
            results.append(heading);
            if (!data.products.length) {
                const empty = document.createElement('p');
                empty.className = 'search-hint';
                empty.textContent = 'No encontramos piezas con esa búsqueda. Prueba otro nombre o colección.';
                results.append(empty);
            }
            data.products.forEach(product => {
                const link = document.createElement('a');
                link.className = 'search-result';
                link.href = '/Productos/Detalle/' + product.id;
                const img = document.createElement('img');
                img.src = product.imageUrl;
                img.alt = '';
                const info = document.createElement('span');
                const name = document.createElement('strong');
                name.textContent = product.name;
                const category = document.createElement('small');
                category.textContent = product.category;
                info.append(name, category);
                const arrow = document.createElement('b');
                arrow.textContent = '→';
                link.append(img, info, arrow);
                results.append(link);
            });
        } catch (error) {
            if (error.name === 'AbortError') return;
            results.replaceChildren();
            const fallback = document.createElement('p');
            fallback.className = 'search-hint';
            fallback.textContent = 'No pudimos cargar las sugerencias. Puedes buscar con el botón superior.';
            results.append(fallback);
        }
    }

    const newAddressFields = document.querySelector('[data-new-address-fields]');
    const shippingChoices = [...document.querySelectorAll('[data-shipping-choice]')];
    const shippingInputs = {
        name: document.getElementById('ShippingName'),
        phone: document.getElementById('ShippingPhone'),
        street: document.getElementById('ShippingAddress'),
        district: document.getElementById('ShippingDistrict'),
        city: document.getElementById('ShippingCity'),
        reference: document.getElementById('ShippingReference')
    };
    const readShipping = () => Object.fromEntries(Object.entries(shippingInputs).map(([key, input]) => [key, input?.value || '']));
    const writeShipping = values => Object.entries(shippingInputs).forEach(([key, input]) => { if (input) input.value = values[key] || ''; });
    let newAddressDraft = shippingChoices.some(choice => choice.checked && choice.value) ? null : readShipping();

    function updateShippingChoice() {
        const choice = shippingChoices.find(input => input.checked);
        if (!choice || !newAddressFields) return;
        const isNew = !choice.value;
        if (!isNew && !newAddressFields.hidden) newAddressDraft = readShipping();
        newAddressFields.hidden = !isNew;
        if (isNew) writeShipping(newAddressDraft || {});
        else {
            const data = choice.closest('[data-shipping-address]')?.dataset || {};
            writeShipping({ name: data.shippingName, phone: data.shippingPhone, street: data.shippingStreet, district: data.shippingDistrict, city: data.shippingCity, reference: data.shippingReference });
        }
    }
    shippingChoices.forEach(choice => choice.addEventListener('change', updateShippingChoice));

    const saveAddressCheckbox = document.querySelector('[data-save-address]');
    const addressLabel = document.querySelector('[data-save-address-label]');
    saveAddressCheckbox?.addEventListener('change', () => { if (addressLabel) addressLabel.hidden = !saveAddressCheckbox.checked; });
})();
