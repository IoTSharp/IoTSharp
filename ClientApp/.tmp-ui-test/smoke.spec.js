const { test, expect } = require('@playwright/test');

test.use({
	channel: 'msedge',
	headless: true,
});

test('landing page opens', async ({ page }) => {
	await page.goto('http://127.0.0.1:27915/#/');
	await expect(page).toHaveURL(/#\/$/);
});
