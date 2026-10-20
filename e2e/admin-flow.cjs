const assert = require('node:assert/strict');
const fs = require('node:fs');
const path = require('node:path');
const { execFileSync } = require('node:child_process');
const { chromium } = require('playwright');

const base = process.env.BLACKCODE_URL || 'http://127.0.0.1:5088';
const email = `codex-admin-${Date.now()}@example.invalid`;
const testProductName = `Producto temporal ${Date.now()}`;
const password = 'ClavePrueba123';
const captureRoot = path.join(__dirname, '..', 'review', 'admin-stage');

function mysql(sql) {
    return execFileSync('docker', ['exec', 'blackcode-mysql', 'sh', '-c', `mysql -uroot -p"$MYSQL_ROOT_PASSWORD" blackcode_db -e "${sql}"`], { encoding: 'utf8', stdio: 'pipe' });
}

async function capture(page, group, name) {
    const folder = path.join(captureRoot, group);
    fs.mkdirSync(folder, { recursive: true });
    const overflow = await page.evaluate(() => document.documentElement.scrollWidth - innerWidth);
    assert.ok(overflow <= 1, `${group}/${name}: desbordamiento horizontal ${overflow}px`);
    await page.screenshot({ path: path.join(folder, `${name}.png`), fullPage: true });
}

(async () => {
    const browser = await chromium.launch({ channel: 'msedge', headless: true });
    try {
        const registration = await browser.newContext({ viewport: { width: 1440, height: 900 } });
        const page = await registration.newPage();
        await page.goto(base + '/Cuenta/Registro');
        await page.locator('#FullName').fill('Admin Temporal');
        await page.locator('#Email').fill(email);
        await page.locator('#Password').fill(password);
        await page.locator('#ConfirmPassword').fill(password);
        await page.locator('.auth-form button[type="submit"]').click();
        await page.waitForURL(base + '/');
        await page.goto(base + '/Admin');
        assert.match(page.url(), /AccesoDenegado/);
        await registration.close();

        mysql(`INSERT INTO AspNetUserRoles (UserId, RoleId) SELECT u.Id, r.Id FROM AspNetUsers u CROSS JOIN AspNetRoles r WHERE u.Email = '${email}' AND r.Name = 'Admin';`);

        for (const [group, viewport] of [['desktop', { width: 1440, height: 900 }], ['mobile', { width: 390, height: 844 }]]) {
            const context = await browser.newContext({ viewport });
            const admin = await context.newPage();
            const errors = [];
            admin.on('pageerror', error => errors.push(error.message));
            await admin.goto(base + '/Cuenta/Login');
            await admin.locator('#Email').fill(email);
            await admin.locator('#Password').fill(password);
            await admin.locator('.auth-form button[type="submit"]').click();
            await admin.waitForURL(base + '/');
            await admin.goto(base + '/Admin');
            assert.match(await admin.locator('.admin-main h1').innerText(), /Resumen/);
            assert.equal(await admin.locator('.admin-page .site-header').count(), 0);
            if (group === 'mobile') {
                await admin.locator('[data-admin-menu-toggle]').click();
                assert.equal(await admin.locator('#admin-navigation').isVisible(), true);
                assert.equal(await admin.locator('[data-admin-menu-toggle]').getAttribute('aria-expanded'), 'true');
                await admin.locator('[data-admin-menu-toggle]').click();
            }
            await capture(admin, group, 'A01_Resumen');
            await admin.goto(base + '/Admin/Productos');
            assert.equal(await admin.locator('.inventory-table tbody tr').count(), 13);
            await capture(admin, group, 'A01b_Productos');
            await admin.goto(base + '/Admin/Clientes');
            assert.match(await admin.locator('.admin-main').innerText(), /Admin Temporal/);
            await admin.goto(base + '/Admin/Reportes');
            assert.match(await admin.locator('.admin-main').innerText(), /DESCARGAR CSV/);
            await admin.goto(base + '/Admin/Inventario');
            assert.equal(await admin.locator('.inventory-table tbody tr').count(), 13);
            assert.equal(await admin.locator('.inventory-table tbody tr').first().locator('td').nth(3).innerText(), 'Sin definir');
            await capture(admin, group, 'A02_Inventario');
            if (group === 'desktop') {
                await admin.evaluate(() => window.scrollTo(0, 900));
                const menuPosition = await admin.locator('.admin-sidebar nav').boundingBox();
                assert.ok(menuPosition && menuPosition.y >= -1 && menuPosition.y < 100, 'El menú lateral debe seguir visible al desplazarse');
            }
            await admin.goto(base + '/Admin/Inventario/1');
            assert.equal(await admin.locator('#Quantity').inputValue(), '');
            await admin.locator('#Quantity').fill('-1');
            await admin.locator('#Reason').fill('Prueba de validación');
            await admin.locator('.account-form button[type="submit"]').click();
            assert.equal(await admin.locator('#Quantity').inputValue(), '-1');
            assert.deepEqual(errors, []);
            await context.close();
        }
        const context = await browser.newContext();
        const admin = await context.newPage();
        await admin.goto(base + '/Cuenta/Login');
        await admin.locator('#Email').fill(email);
        await admin.locator('#Password').fill(password);
        await admin.locator('.auth-form button[type="submit"]').click();
        await admin.waitForURL(base + '/');
        await admin.goto(base + '/Admin/Productos/Nuevo');
        await admin.locator('#Name').fill(testProductName);
        await admin.locator('#Category').fill('Pruebas');
        await admin.locator('#Price').fill('1.00');
        await admin.locator('#Description').fill('Producto para prueba automatizada');
        await admin.locator('#Material').fill('Prueba');
        await admin.locator('#Finish').fill('Prueba');
        await admin.locator('#SizesCsv').fill('Única');
        await admin.locator('#ImageUrl').fill('/images/products/anillo-obsidiana.jpg');
        await admin.locator('.account-form button[type="submit"]').click();
        await admin.waitForURL(base + '/Admin/Productos');
        const productId = Number(mysql(`SELECT Id FROM Products WHERE Name = '${testProductName}';`).trim().split(/\s+/).at(-1));
        assert.ok(productId > 0);
        await admin.goto(base + `/Admin/Productos/${productId}/Editar`);
        await admin.locator('#Price').fill('2.50');
        await admin.locator('.account-form button[type="submit"]').click();
        await admin.waitForURL(base + '/Admin/Productos');
        assert.match(mysql(`SELECT Price FROM Products WHERE Id = ${productId};`), /2\.50/);
        const csvResponse = await admin.request.get(base + '/Admin/Reportes/Productos.csv');
        assert.equal(csvResponse.status(), 200);
        assert.match(await csvResponse.text(), new RegExp(testProductName));
        const jsonResponse = await admin.request.get(base + '/Admin/Reportes/Productos.json');
        assert.equal(jsonResponse.status(), 200);
        assert.ok((await jsonResponse.json()).some(p => p.Name === testProductName));
        await admin.goto(base + `/Admin/Inventario/${productId}`);
        await admin.locator('#Quantity').fill('3');
        await admin.locator('#MinimumQuantity').fill('2');
        await admin.locator('#Reason').fill('Conteo de prueba');
        await admin.locator('.account-form button[type="submit"]').click();
        await admin.waitForURL(/\/Admin\/Inventario$/);
        const row = admin.locator('.inventory-table tbody tr').filter({ hasText: testProductName });
        assert.equal(await row.locator('td').nth(3).innerText(), '3');
        assert.equal(await row.locator('td').nth(4).innerText(), '2');
        assert.match(await admin.locator('.stock-history').innerText(), /Conteo de prueba/);
        await context.close();
        console.log('Administración: permisos, productos, cuentas, reportes e inventario correctos en escritorio y móvil.');
    } finally {
        await browser.close();
        mysql(`DELETE FROM Products WHERE Name = '${testProductName}';`);
        mysql(`DELETE FROM AspNetUsers WHERE Email = '${email}';`);
    }
})().catch(error => { console.error(error); process.exitCode = 1; });
