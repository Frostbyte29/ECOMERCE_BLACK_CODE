const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { execFileSync } = require('node:child_process');
const { chromium } = require('playwright');

const base = process.env.BLACKCODE_URL || 'http://127.0.0.1:5088';
const password = 'ClavePrueba123';
const captures = path.join(__dirname, '..', 'review', 'account-stage');

async function screenshot(page, group, name) {
    const folder = path.join(captures, group);
    fs.mkdirSync(folder, { recursive: true });
    const overflow = await page.evaluate(() => document.documentElement.scrollWidth - innerWidth);
    assert.ok(overflow <= 1, `${group}/${name}: desbordamiento horizontal ${overflow}px`);
    await page.screenshot({ path: path.join(folder, `${name}.png`), fullPage: true });
}

async function exercise(browser, group, viewport) {
    const email = `codex-account-${Date.now()}-${group}@example.invalid`;
    const context = await browser.newContext({ viewport, locale: 'es-PE' });
    const page = await context.newPage();
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    try {
        await page.goto(base + '/Cuenta/Registro');
        await page.locator('#FullName').fill('Prueba Cuenta');
        await page.locator('#Email').fill(email);
        await page.locator('#PhoneNumber').fill('987654321');
        await page.locator('#Password').fill(password);
        await page.locator('#ConfirmPassword').fill(password);
        await page.locator('.auth-form button[type="submit"]').click();
        await page.waitForURL(base + '/');

        await page.goto(base + '/MiCuenta');
        assert.match(await page.locator('.account-main h1').innerText(), /Hola, Prueba/);
        await screenshot(page, group, 'U02_Mi_cuenta');
        await page.goto(base + '/Pedidos');
        assert.equal(await page.locator('.account-sidebar nav a[aria-current="page"]').innerText(), 'Mis pedidos');
        await screenshot(page, group, 'U07_Mis_pedidos_vacio');
        await page.goto(base + '/MiCuenta/Direcciones');
        assert.equal(await page.locator('a[href*="NuevaDireccion"]').count(), 1);
        assert.equal(await page.locator('.account-empty-icon svg').count(), 1);
        await screenshot(page, group, 'U05_Direcciones_vacias');

        await page.goto(base + '/Productos/Detalle/1');
        await page.locator('.detail-heart[data-favorite-toggle]').click();
        await page.locator('#favorite-saved-overlay:visible').waitFor();
        await screenshot(page, group, 'U08_Favorito_agregado');
        await page.locator('#favorite-saved-overlay .explore-button').click();
        await page.goto(base + '/Favoritos');
        assert.equal(await page.locator('.favorites-grid .product-tile').count(), 1);
        await screenshot(page, group, 'U04_Favoritos');

        await page.goto(base + '/MiCuenta/NuevaDireccion');
        await page.locator('#Label').fill('Casa');
        await page.locator('#RecipientName').fill('Prueba Cuenta');
        await page.locator('#Phone').fill('987654321');
        await page.locator('#City').fill('Lima');
        await page.locator('#Street').fill('Calle de prueba 123');
        await page.locator('#District').fill('Miraflores');
        await page.locator('.account-form button[type="submit"]').click();
        await page.waitForURL(/\/MiCuenta\/Direcciones/);
        assert.match(await page.locator('.address-card').innerText(), /PRINCIPAL/);
        await screenshot(page, group, 'U05_Direcciones');

        await page.goto(base + '/Productos/Detalle/1');
        await page.locator('.buy-form button[type="submit"]').first().click();
        await page.waitForURL(/\/Carrito/);
        await page.locator('a[href*="Checkout"]').first().click();
        await page.waitForURL(/\/Checkout/);
        assert.equal(await page.locator('#ShippingAddress').inputValue(), 'Calle de prueba 123');
        assert.equal(await page.locator('[data-new-address-fields]').isVisible(), false);
        await screenshot(page, group, 'U06_Checkout_direccion');
        await page.locator('.shipping-choice-new input').check();
        assert.equal(await page.locator('[data-new-address-fields]').isVisible(), true);
        assert.equal(await page.locator('#ShippingAddress').inputValue(), '');
        await screenshot(page, group, 'U06_Checkout_otra_direccion');
        await page.locator('[data-shipping-address] input').check();
        assert.equal(await page.locator('#ShippingAddress').inputValue(), 'Calle de prueba 123');

        await page.goto(base + '/MiCuenta/MisDatos');
        await page.locator('#FullName').fill('Prueba Editada');
        await page.locator('.account-form button[type="submit"]').click();
        await page.waitForURL(/\/MiCuenta\/MisDatos/);
        assert.equal(await page.locator('#FullName').inputValue(), 'Prueba Editada');
        await screenshot(page, group, 'U03_Mis_datos');

        await page.goto(base + '/MiCuenta/CambiarContrasena');
        assert.equal(await page.locator('[data-password-toggle]').count(), 3);
        await page.locator('[data-password-toggle="CurrentPassword"]').click();
        assert.equal(await page.locator('#CurrentPassword').getAttribute('type'), 'text');
        assert.equal(await page.locator('#NewPassword').getAttribute('type'), 'password');
        await page.locator('[data-password-toggle="CurrentPassword"]').click();
        await page.locator('#CurrentPassword').fill(password);
        await page.locator('#NewPassword').fill(password + 'Nueva');
        await page.locator('#ConfirmPassword').fill(password + 'Nueva');
        await page.locator('.account-form button[type="submit"]').click();
        await page.waitForURL(/\/MiCuenta$/);

        await page.goto(base + '/Favoritos');
        await page.locator('.favorites-grid [data-favorite-toggle]').click();
        await page.waitForFunction(() => document.querySelectorAll('.favorites-grid .product-tile').length === 0);
        assert.equal(await page.locator('[data-favorite-count]').first().innerText(), '0');
        assert.deepEqual(errors, []);
    } finally {
        await context.close();
        const sql = `DELETE FROM AspNetUsers WHERE Email = '${email}';`;
        execFileSync('docker', ['exec', 'blackcode-mysql', 'sh', '-c', `mysql -uroot -p"$MYSQL_ROOT_PASSWORD" blackcode_db -e "${sql}"`], { stdio: 'pipe' });
    }
}

(async () => {
    const browser = await chromium.launch({ channel: 'msedge', headless: true });
    try {
        await exercise(browser, 'desktop', { width: 1440, height: 900 });
        await exercise(browser, 'mobile', { width: 390, height: 844 });
        console.log('Cuenta registrada: flujos de escritorio y móvil correctos.');
    } finally {
        await browser.close();
    }
})().catch(error => { console.error(error); process.exitCode = 1; });
