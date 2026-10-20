const assert = require('node:assert/strict');
const { execFileSync } = require('node:child_process');
const fs = require('node:fs');
const path = require('node:path');
const { chromium } = require('playwright');

const base = process.env.BLACKCODE_URL || 'http://127.0.0.1:5088';
const password = 'ClavePrueba123';

function mysql(sql) {
    execFileSync('docker', ['exec', 'blackcode-mysql', 'sh', '-c', `mysql -uroot -p"$MYSQL_ROOT_PASSWORD" blackcode_db -e "${sql}"`], { stdio: 'pipe' });
}

async function addProductAndCheckout(page) {
    await page.goto(base + '/Productos/Detalle/1');
    await page.locator('.buy-form button[name="buyNow"]').click();
    await page.waitForURL(/\/Checkout$/);
}

async function verifyOrder(page, street) {
    const match = page.url().match(/\/Checkout\/Pagar\/(\d+)/);
    assert.ok(match, `No llegó al pago: ${page.url()}`);
    await page.goto(base + `/Pedidos/Detalle/${match[1]}`);
    assert.match(await page.locator('.info-box').first().innerText(), new RegExp(street));
    assert.equal(await page.locator('.account-sidebar nav a[aria-current="page"]').innerText(), 'Mis pedidos');
}

async function exercise(browser, viewport, group) {
    const email = `codex-checkout-${Date.now()}-${group}@example.invalid`;
    const context = await browser.newContext({ viewport });
    const page = await context.newPage();
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    try {
        await page.goto(base + '/Cuenta/Registro');
        await page.locator('#FullName').fill('Prueba Checkout');
        await page.locator('#Email').fill(email);
        await page.locator('#Password').fill(password);
        await page.locator('#ConfirmPassword').fill(password);
        await page.locator('.auth-form button[type="submit"]').click();
        await page.waitForURL(base + '/');

        await addProductAndCheckout(page);
        assert.equal(await page.locator('.checkout-no-address').count(), 1);
        await page.locator('#ShippingName').fill('Prueba Checkout');
        await page.locator('#ShippingPhone').fill('987654321');
        await page.locator('#ShippingCity').fill('Lima');
        await page.locator('#ShippingAddress').fill('Calle Primera 123');
        await page.locator('#ShippingDistrict').fill('Miraflores');
        await page.locator('#SaveNewAddress').check();
        assert.equal(await page.locator('[data-save-address-label]').isVisible(), true);
        await page.locator('#NewAddressLabel').fill('Casa');
        await page.locator('.checkout-shipping-form button[type="submit"]').click();
        await page.waitForURL(/\/Checkout\/Pagar\/\d+$/);
        const screenshotDirectory = path.join(__dirname, '..', 'review', 'checkout-stage', group);
        fs.mkdirSync(screenshotDirectory, { recursive: true });
        await page.screenshot({ path: path.join(screenshotDirectory, 'Pago.png'), fullPage: true });
        await verifyOrder(page, 'Calle Primera 123');
        await page.goto(base + '/MiCuenta/Direcciones');
        assert.equal(await page.locator('.address-card').count(), 1);
        assert.match(await page.locator('.address-card').innerText(), /PRINCIPAL/);

        await addProductAndCheckout(page);
        assert.equal(await page.locator('[data-shipping-address] input:checked').count(), 1);
        assert.equal(await page.locator('[data-new-address-fields]').isVisible(), false);
        await page.locator('#ShippingAddress').evaluate(input => { input.value = 'Dirección alterada'; });
        await page.locator('.checkout-shipping-form button[type="submit"]').click();
        await page.waitForURL(/\/Checkout\/Pagar\/\d+$/);
        await verifyOrder(page, 'Calle Primera 123');

        await addProductAndCheckout(page);
        await page.locator('.shipping-choice-new input').check();
        await page.locator('#ShippingName').fill('Otra Persona');
        await page.locator('#ShippingPhone').fill('987654322');
        await page.locator('#ShippingCity').fill('Lima');
        await page.locator('#ShippingAddress').fill('Calle Segunda 456');
        await page.locator('#ShippingDistrict').fill('Barranco');
        assert.equal(await page.locator('#SaveNewAddress').isChecked(), false);
        await page.locator('.checkout-shipping-form button[type="submit"]').click();
        await page.waitForURL(/\/Checkout\/Pagar\/\d+$/);
        await verifyOrder(page, 'Calle Segunda 456');
        await page.goto(base + '/MiCuenta/Direcciones');
        assert.equal(await page.locator('.address-card').count(), 1);
        await addProductAndCheckout(page);
        await page.locator('.checkout-shipping-form button[type="submit"]').click();
        await page.waitForURL(/\/Checkout\/Pagar\/\d+$/);
        const paymentUrl = page.url();
        await page.locator('input[name="Method"][value="YapePlin"]').check();
        assert.match(await page.locator('.pay-panel[data-method="YapePlin"]').innerText(), /no est[aá] conectada a Yape ni Plin/i);
        assert.equal(await page.locator('.checkout-confirm-button').getAttribute('form'), 'checkout-payment-form');
        await page.locator('input[name="Method"][value="Tarjeta"]').check();
        await page.locator('#CardNumber').fill('4000000000000002');
        await page.locator('#CardHolder').fill('Prueba Checkout');
        await page.locator('#CardExpiry').fill('12/29');
        await page.locator('#CardCvv').fill('123');
        await page.locator('.checkout-confirm-button').click();
        assert.equal(page.url(), paymentUrl);
        assert.match(await page.locator('.form-errors').innerText(), /rechazado/i);
        await page.locator('#CardNumber').fill('4111111111111111');
        await page.locator('#CardHolder').fill('Prueba Checkout');
        await page.locator('#CardExpiry').fill('12/29');
        await page.locator('#CardCvv').fill('123');
        await page.locator('.checkout-confirm-button').click();
        await page.waitForURL(/\/Checkout\/Confirmacion\/\d+$/);
        assert.match(await page.locator('.confirm-hero').innerText(), /pagado/i);

        await addProductAndCheckout(page);
        await page.locator('.checkout-shipping-form button[type="submit"]').click();
        await page.waitForURL(/\/Checkout\/Pagar\/\d+$/);
        assert.match(await page.locator('.payment-deadline').innerText(), /3 d[ií]as para pagar/i);
        page.once('dialog', dialog => dialog.accept());
        await page.locator('.checkout-cancel-form button[type="submit"]').click();
        await page.waitForURL(/\/Pedidos\/Detalle\/\d+$/);
        assert.match(await page.locator('.account-section-heading').innerText(), /cancelado/i);
        assert.equal(await page.locator('a:has-text("COMPLETAR PAGO")').count(), 0);

        await addProductAndCheckout(page);
        await page.locator('.checkout-shipping-form button[type="submit"]').click();
        await page.waitForURL(/\/Checkout\/Pagar\/\d+$/);
        const expiredId = Number(page.url().match(/\/Checkout\/Pagar\/(\d+)$/)[1]);
        mysql(`UPDATE Orders SET CreatedAt = DATE_SUB(UTC_TIMESTAMP(), INTERVAL 4 DAY) WHERE Id = ${expiredId};`);
        await page.goto(base + `/Checkout/Pagar/${expiredId}`);
        await page.waitForURL(new RegExp(`/Pedidos/Detalle/${expiredId}$`));
        assert.match(await page.locator('.account-section-heading').innerText(), /cancelado/i);
        assert.match(await page.locator('.timeline').innerText(), /vencer los 3 d[ií]as/i);
        assert.deepEqual(errors, []);
    } finally {
        await context.close();
        mysql(`DELETE FROM Orders WHERE UserId IN (SELECT Id FROM AspNetUsers WHERE Email = '${email}');`);
        mysql(`DELETE FROM AspNetUsers WHERE Email = '${email}';`);
    }
}

(async () => {
    const browser = await chromium.launch({ channel: 'msedge', headless: true });
    try {
        await exercise(browser, { width: 1440, height: 900 }, 'desktop');
        await exercise(browser, { width: 390, height: 844 }, 'mobile');
        console.log('Checkout: direcciones, pagos de prueba, cancelación y vencimiento correctos en escritorio y móvil.');
    } finally {
        await browser.close();
    }
})().catch(error => { console.error(error); process.exitCode = 1; });
