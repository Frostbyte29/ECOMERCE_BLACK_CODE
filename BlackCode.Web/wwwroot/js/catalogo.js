(() => {
    const selects = [...document.querySelectorAll('select[data-brand-select]')];
    let openMenu = null;

    function closeMenu(restoreFocus = false) {
        if (!openMenu) return;
        openMenu.menu.hidden = true;
        openMenu.trigger.setAttribute('aria-expanded', 'false');
        if (restoreFocus) openMenu.trigger.focus();
        openMenu = null;
    }

    for (const select of selects) {
        const wrapper = document.createElement('div');
        wrapper.className = 'catalog-select';
        select.before(wrapper);
        wrapper.append(select);
        select.hidden = true;

        const trigger = document.createElement('button');
        trigger.type = 'button';
        trigger.className = 'catalog-select-trigger';
        trigger.setAttribute('aria-haspopup', 'listbox');
        trigger.setAttribute('aria-expanded', 'false');
        trigger.setAttribute('aria-controls', `${select.id}-options`);

        const menu = document.createElement('div');
        menu.id = `${select.id}-options`;
        menu.className = 'catalog-select-menu';
        menu.setAttribute('role', 'listbox');
        const label = document.querySelector(`label[for="${select.id}"]`)?.textContent?.trim() || 'Opciones';
        menu.setAttribute('aria-label', label);
        menu.hidden = true;

        const options = [...select.options].map((option, index) => {
            const item = document.createElement('button');
            item.type = 'button';
            item.className = 'catalog-select-option';
            item.id = `${select.id}-option-${index}`;
            item.textContent = option.textContent;
            item.setAttribute('role', 'option');
            item.setAttribute('aria-selected', String(option.selected));
            item.addEventListener('click', () => {
                select.value = option.value;
                sync();
                closeMenu(true);
                select.dispatchEvent(new Event('change', { bubbles: true }));
            });
            menu.append(item);
            return item;
        });

        function sync() {
            trigger.textContent = select.selectedOptions[0]?.textContent || '';
            trigger.setAttribute('aria-label', `${label}: ${trigger.textContent}`);
            options.forEach((item, index) => item.setAttribute('aria-selected', String(select.selectedIndex === index)));
        }

        function open() {
            closeMenu();
            menu.hidden = false;
            trigger.setAttribute('aria-expanded', 'true');
            openMenu = { menu, trigger };
            options[Math.max(0, select.selectedIndex)]?.focus();
        }

        trigger.addEventListener('click', () => menu.hidden ? open() : closeMenu());
        trigger.addEventListener('keydown', event => {
            if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
                event.preventDefault();
                open();
            }
        });
        menu.addEventListener('keydown', event => {
            const current = options.indexOf(document.activeElement);
            let next = current;
            if (event.key === 'ArrowDown') next = Math.min(options.length - 1, current + 1);
            else if (event.key === 'ArrowUp') next = Math.max(0, current - 1);
            else if (event.key === 'Home') next = 0;
            else if (event.key === 'End') next = options.length - 1;
            else if (event.key === 'Escape') {
                event.preventDefault();
                closeMenu(true);
                return;
            } else if (event.key === 'Tab') {
                closeMenu();
                return;
            } else return;
            event.preventDefault();
            options[next]?.focus();
        });
        wrapper.append(trigger, menu);
        sync();
    }

    document.addEventListener('pointerdown', event => {
        if (openMenu && !openMenu.menu.parentElement.contains(event.target)) closeMenu();
    });

    const priceRange = document.querySelector('[data-price-range]');
    if (!priceRange) return;
    const minRange = priceRange.querySelector('[data-price-min-range]');
    const maxRange = priceRange.querySelector('[data-price-max-range]');
    const minField = priceRange.querySelector('#filter-price-min');
    const maxField = priceRange.querySelector('#filter-price-max');
    const ceiling = Number(maxRange.max);
    const clamp = value => Math.min(ceiling, Math.max(0, Number.isFinite(value) ? value : 0));

    function render(min, max, source, updateFields = true) {
        min = clamp(min);
        max = clamp(max);
        if (min > max) {
            if (source === 'min') max = min;
            else min = max;
        }
        minRange.value = String(min);
        maxRange.value = String(max);
        if (updateFields) {
            minField.value = String(min);
            maxField.value = String(max);
        }
        priceRange.style.setProperty('--price-start', `${min / ceiling * 100}%`);
        priceRange.style.setProperty('--price-end', `${max / ceiling * 100}%`);
        minRange.style.zIndex = min >= max ? '4' : '2';
        maxRange.style.zIndex = min >= max ? '3' : '4';
    }

    minRange.addEventListener('input', () => render(Number(minRange.value), Number(maxRange.value), 'min'));
    maxRange.addEventListener('input', () => render(Number(minRange.value), Number(maxRange.value), 'max'));
    minField.addEventListener('change', () => render(Number(minField.value), Number(maxField.value), 'min'));
    maxField.addEventListener('change', () => render(Number(minField.value), Number(maxField.value), 'max'));
    priceRange.closest('form').addEventListener('submit', () => {
        render(Number(minField.value), Number(maxField.value), 'max');
        minField.disabled = Number(minField.value) <= 0;
        maxField.disabled = Number(maxField.value) >= ceiling;
    });
    render(Number(minField.value), Number(maxField.value), 'max');
})();
