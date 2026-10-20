const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');

const base = process.env.BLACKCODE_URL || 'http://127.0.0.1:5088';
const captureRoot = path.join(__dirname, '..', 'review', 'guest-stage');

async function capture(page, group, name) {
    const directory = path.join(captureRoot, group);
    fs.mkdirSync(directory, { recursive: true });
    const overflow = await page.evaluate(() => document.documentElement.scrollWidth - window.innerWidth);
    assert.ok(overflow <= 1, `${group}/${name}: desbordamiento horizontal ${overflow}px`);
    await page.screenshot({ path: path.join(directory, name + '.png'), fullPage: !/^G03|^G09/.test(name) });
}

async function checkViewport(browser, group, width, height) {
    const context = await browser.newContext({ viewport: { width, height }, locale: 'es-PE' });
    const page = await context.newPage();
    const failures = [];
    page.on('pageerror', error => failures.push(error.message));

    await page.goto(base + '/');
    await page.locator('.product-tile').first().waitFor();
    await capture(page, group, 'G01_Inicio');
    await page.locator('.main-nav a').filter({ hasText: 'Nosotros' }).click();
    await page.waitForURL(base + '/Nosotros');
    assert.match(await page.locator('.about-hero h1').innerText(), /Más que joyería/);
    assert.equal(await page.locator('.about-values > div').count(), 3);
    assert.equal(await page.locator('.about-hero-image img').getAttribute('src'), '/images/about/black-code-retratos-principal.png');
    await capture(page, group, 'G01b_Nosotros');
    await page.goto(base + '/');
    await page.locator('.products-section [data-favorite-open]').first().click();
    await page.locator('#favorite-overlay:visible').waitFor();
    assert.match(await page.locator('#favorite-overlay [data-auth-link="login"]').getAttribute('href'), /returnUrl=%2F/);
    await capture(page, group, 'G09A_Favoritos_inicio');
    await page.locator('#favorite-overlay .explore-button').click();
    assert.equal(await page.locator('#favorite-overlay').isVisible(), false);

    await page.goto(base + '/Colecciones');
    assert.ok(await page.locator('.collection-card').count() > 0);
    await capture(page, group, 'G02_Colecciones');

    await page.goto(base + '/');
    await page.locator('[data-search-open]').click();
    await page.locator('#global-search').fill('Atlas');
    await page.waitForFunction(() => {
        const results = document.querySelectorAll('.search-result');
        return results.length === 1 && results[0].textContent.includes('Cadena Atlas');
    });
    assert.match(await page.locator('.search-results').innerText(), /Cadena Atlas/);
    await capture(page, group, 'G03_Busqueda_sugerencias');
    await page.keyboard.press('Escape');
    assert.equal(await page.locator('#search-overlay').isVisible(), false);

    await page.goto(base + '/Productos?categoria=Anillos');
    assert.equal(await page.locator('.catalog-grid .product-tile').count(), 8);
    await capture(page, group, 'G04_Catalogo_filtrado');
    await page.locator('#catalog-q').focus();
    await page.locator('#search-overlay:visible').waitFor();
    await page.keyboard.press('Escape');
    assert.equal(await page.locator('#search-overlay').isVisible(), false);
    await page.locator('.catalog-grid [data-favorite-open]').first().click();
    await capture(page, group, 'G09B_Favoritos_catalogo');
    await page.keyboard.press('Escape');

    await page.goto(base + '/Productos/Detalle/1');
    await page.locator('[data-quantity-change="1"]').click();
    assert.equal(await page.locator('#quantity').inputValue(), '2');
    await capture(page, group, 'G05_Detalle');
    await page.locator('[data-gallery-open]').click();
    await page.locator('#gallery-overlay:visible').waitFor();
    await page.keyboard.press('Escape');
    await page.locator('.detail-heart').click();
    await capture(page, group, 'G09C_Favoritos_detalle');
    await page.locator('#favorite-overlay .explore-button').click();

    await page.locator('.buy-form button[type="submit"]').first().click();
    await page.waitForURL(/\/Carrito/);
    assert.match(await page.locator('.cart-line').innerText(), /Anillo Obsidiana/);
    await capture(page, group, 'G06_Carrito');
    await Promise.all([page.waitForNavigation(), page.locator('[data-cart-quantity-change="1"]').click()]);
    assert.equal(await page.locator('.qty-form input[type="number"]').inputValue(), '3');
    await Promise.all([page.waitForNavigation(), page.locator('.cart-line form:last-child button').click()]);
    assert.match(await page.locator('.empty-state').innerText(), /vacío/i);
    await page.goto(base + '/Productos/Detalle/1');
    await page.locator('.buy-form button[type="submit"]').first().click();
    await page.waitForURL(/\/Carrito/);
    await Promise.all([page.waitForNavigation(), page.locator('.cart-clear').click()]);
    assert.match(await page.locator('.empty-state').innerText(), /vacío/i);

    await page.goto(base + '/Cuenta/Login');
    await capture(page, group, 'G07_Login');
    assert.equal(await page.locator('[data-password-toggle="Password"]').innerText(), '');
    await page.locator('[data-password-toggle="Password"]').click();
    assert.equal(await page.locator('#Password').getAttribute('type'), 'text');
    assert.equal(await page.locator('[data-password-toggle="Password"]').getAttribute('aria-pressed'), 'true');
    await page.locator('[data-password-toggle="Password"]').click();
    assert.equal(await page.locator('#Password').getAttribute('type'), 'password');
    await page.goto(base + '/Cuenta/Registro');
    await capture(page, group, 'G08_Registro');
    assert.equal(await page.locator('[data-password-toggle]').count(), 2);
    await page.locator('[data-password-toggle="ConfirmPassword"]').click();
    assert.equal(await page.locator('#ConfirmPassword').getAttribute('type'), 'text');
    assert.equal(await page.locator('#Password').getAttribute('type'), 'password');
    await page.locator('[data-password-toggle="ConfirmPassword"]').click();
    await page.locator('#FullName').fill('Prueba Visual');
    await page.locator('#Email').fill('prueba@example.invalid');
    await page.locator('#Password').fill('abcdef12');
    await page.locator('#ConfirmPassword').fill('otra-clave');
    await page.locator('.auth-form button[type="submit"]').click();
    assert.match(await page.locator('[data-valmsg-for="ConfirmPassword"]').innerText(), /no coinciden/i);

    const missing = await page.goto(base + '/Productos/Detalle/999999');
    assert.equal(missing.status(), 404);

    const overflow = await page.evaluate(() => document.documentElement.scrollWidth - window.innerWidth);
    assert.ok(overflow <= 1, `${group}: desbordamiento horizontal ${overflow}px`);
    assert.deepEqual(failures, [], `${group}: errores JavaScript`);
    await context.close();
}

(async () => {
    const browser = await chromium.launch({ channel: 'msedge', headless: true });
    try {
        await checkViewport(browser, 'desktop', 1440, 900);
        await checkViewport(browser, 'mobile', 390, 844);
        console.log('Flujo invitado: navegador, interacciones y capturas correctas.');
        console.log(captureRoot);
    } finally {
        await browser.close();
    }
})().catch(error => { console.error(error); process.exitCode = 1; });
