const assert = require('node:assert/strict');
const os = require('node:os');
const path = require('node:path');
const { chromium } = require('playwright');

const base = process.env.BLACKCODE_URL || 'http://127.0.0.1:5088';

async function prices(page) {
    return page.locator('.product-tile-copy p').allTextContents().then(values =>
        values.map(value => Number(value.replace(/[^\d,.]/g, '').replace(',', '.'))));
}

async function exercise(browser, viewport) {
    const context = await browser.newContext({ viewport });
    const page = await context.newPage();
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    try {
        await page.goto(base + '/Productos');
        assert.equal(await page.locator('.catalog-select-trigger').count(), 4);
        if (process.env.CAPTURE_CATALOG)
            await page.screenshot({ path: path.join(os.tmpdir(), `blackcode-catalog-${viewport.width}.png`), fullPage: true });
        const initialPrices = (await prices(page)).sort((a, b) => a - b);
        assert.ok(initialPrices.length > 0);

        const category = page.locator('#filter-category + .catalog-select-trigger');
        await category.click();
        assert.equal(await category.getAttribute('aria-expanded'), 'true');
        if (process.env.CAPTURE_CATALOG)
            await page.screenshot({ path: path.join(os.tmpdir(), `blackcode-catalog-menu-${viewport.width}.png`) });
        await page.locator('#filter-category-options').getByRole('option', { name: 'Cadenas' }).click();
        assert.equal(await page.locator('#filter-category').inputValue(), 'Cadenas');
        await page.locator('#catalog-filters button[type="submit"]').click();
        await page.waitForURL(/categoria=Cadenas/);
        const categories = await page.locator('.product-tile-copy span').allTextContents();
        assert.ok(categories.length > 0 && categories.every(value => value === 'Cadenas'));

        await page.goto(base + '/Productos');
        const min = initialPrices[Math.floor(initialPrices.length / 4)];
        const max = initialPrices[Math.floor(initialPrices.length * 3 / 4)];
        await page.locator('[data-price-max-range]').evaluate(input => {
            input.value = '80';
            input.dispatchEvent(new Event('input', { bubbles: true }));
        });
        assert.equal(await page.locator('#filter-price-max').inputValue(), '80');
        await page.locator('#filter-price-min').fill(String(min));
        await page.locator('#filter-price-max').fill(String(max));
        await page.locator('#catalog-filters button[type="submit"]').click();
        await page.waitForURL(url => url.searchParams.get('precioMin') === String(min)
            && url.searchParams.get('precioMax') === String(max));
        const filteredPrices = await prices(page);
        assert.ok(filteredPrices.length > 0 && filteredPrices.every(value => value >= min && value <= max));

        const sort = page.locator('#catalog-sort + .catalog-select-trigger');
        await sort.focus();
        await sort.press('ArrowDown');
        await page.keyboard.press('ArrowDown');
        await page.keyboard.press('ArrowDown');
        await page.keyboard.press('Enter');
        await page.waitForURL(url => url.searchParams.get('ordenar') === 'precio-desc');
        const sortedPrices = await prices(page);
        assert.deepEqual(sortedPrices, [...sortedPrices].sort((a, b) => b - a));
        assert.ok(sortedPrices.every(value => value >= min && value <= max),
            `El orden perdió el filtro de precio: ${page.url()} (${sortedPrices.join(', ')})`);

        const overflow = await page.evaluate(() => document.documentElement.scrollWidth - innerWidth);
        assert.ok(overflow <= 1, `Desbordamiento horizontal: ${overflow}px`);
        assert.deepEqual(errors, []);
    } finally {
        await context.close();
    }
}

(async () => {
    const browser = await chromium.launch({ channel: 'msedge', headless: true });
    try {
        await exercise(browser, { width: 1440, height: 900 });
        await exercise(browser, { width: 390, height: 844 });
        console.log('Catálogo: categorías, rango de precio y orden correctos en escritorio y móvil.');
    } finally {
        await browser.close();
    }
})().catch(error => { console.error(error); process.exitCode = 1; });
