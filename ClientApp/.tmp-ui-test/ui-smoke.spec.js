const { test, expect } = require('@playwright/test');
const fs = require('fs');
const path = require('path');

const APP_URL = 'http://127.0.0.1:27915';
const ARTIFACT_DIR = path.join(__dirname, 'artifacts');

const BIG_IMAGE_BASE64 =
	'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f4DE432X1eN7Xvqlvbu12Pi+ivtD/h3J/wBVC/8AKJ/90Uf8O5P+qhf+UT/7or9Y/wBeOHv+gn/ySf8A8ifI/wBhZj/z6/GP+Z8X0V9of8O5P+qhf+UT/wC6KP8Ah3J/1UL/AMon/wB0Uf68cPf9BP8A5JP/AORD+wsx/wCfX4x/zPi+ivtD/h3J/wBVC/8AKJ/90Uf8O5P+qhf+UT/7oo/144e/6Cf/ACSf/wAiH9hZj/z6/GP+Z8X0V9of8O5P+qhf+UT/AO6KP+Hcn/VQv/KJ/wDdFH+vHD3/AEE/+ST/APkQ/sLMf+fX4x/zPi+ivtD/AIdyf9VC/wDKJ/8AdFH/AA7k/wCqhf8AlE/+6KP9eOHv+gn/AMkn/wDIh/YWY/8APr8Y/wCZ8X0V9of8O5P+qhf+UT/7oo/4dyf9VC/8on/3RR/rxw9/0E/+ST/+RD+wsx/59fjH/M+L6K+0P+Hcn/VQv/KJ/wDdFH/DuT/qoX/lE/8Auij/AF44e/6Cf/JJ/wDyIf2FmP8Az6/GP+Z8X0V9of8ADuT/AKqF/wCUT/7oo/4dyf8AVQv/ACif/dFH+vHD3/QT/wCST/8AkQ/sLMf+fX4x/wAz4vor7Q/4dyf9VC/8on/3RR/w7k/6qF/5RP8A7oo/144e/wCgn/ySf/yIf2FmP/Pr8Y/5nxfRX2h/w7k/6qF/5RP/ALoo/wCHcn/VQv8Ayif/AHRR/rxw9/0E/wDkk/8A5EP7CzH/AJ9fjH/M+L6K+0P+Hcn/AFUL/wAon/3RR/w7k/6qF/5RP/uij/Xjh7/oJ/8AJJ//ACIf2FmP/Pr8Y/5nxfRX2h/w7k/6qF/5RP8A7oo/4dyf9VC/8on/AN0V7R+zl+zl/wAM/wD/AAkP/FQ/29/a32f/AJcvs3leV5v/AE0fdnzfbGO+ePk+KuKsnzLJ6+FwtfmnLlsuWa2nFvVxS2T6nr5TlONw2NhVqwtFX6rs10Z7RRRRX89n6KFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFABRRRQAUUUUAFFFFAH/9k=';
const SMALL_IMAGE_BASE64 =
	'iVBORw0KGgoAAAANSUhEUgAAADgAAAA4CAYAAACohjseAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAABeSURBVGhD7c8hEQAgAMBAypCM+IQAT4NxL97Mbcy1z8/GG35jsM5gncE6g3UG6wzWGawzWGewzmCdwTqDdQbrDNYZrDNYZ7DOYJ3BOoN1BusM1hmsM1hnsM5gncG6C3uD9jS3WERyAAAAAElFTkSuQmCC';

fs.mkdirSync(ARTIFACT_DIR, { recursive: true });

test.use({
	channel: 'msedge',
	headless: true,
	viewport: { width: 1440, height: 960 },
});

function slugify(input) {
	return input.toLowerCase().replace(/[^a-z0-9]+/g, '-').replace(/^-|-$/g, '');
}

function buildMenuTree() {
	return [
		{
			vpath: '/console',
			routename: 'settingsmnt',
			text: 'Console Root',
			children: [
				{
					vpath: '/dashboard',
					routename: 'dashboard',
					text: 'Dashboard Group',
					children: [
						{
							vpath: '/dashboard',
							routename: 'dashboard',
							text: 'Control Overview',
							children: [],
						},
					],
				},
				{
					vpath: '/iot/settings/userlist',
					routename: 'usermnt',
					text: 'User Management',
					children: [],
				},
			],
		},
	];
}

async function fulfillJson(route, body, status = 200) {
	await route.fulfill({
		status,
		contentType: 'application/json; charset=utf-8',
		body: JSON.stringify(body),
	});
}

async function fulfillCss(route, body = '') {
	await route.fulfill({
		status: 200,
		contentType: 'text/css; charset=utf-8',
		body,
	});
}

async function installMocks(page, options = {}) {
	const { installed = true } = options;
	const unhandledApiRequests = [];
	const apiHitCounts = {};

	await page.route('**/*', async (route) => {
		const request = route.request();
		const url = new URL(request.url());
		const method = request.method().toUpperCase();
		const pathname = url.pathname;
		const hitKey = `${method} ${pathname}`;

		if (url.hostname === 'at.alicdn.com' || url.hostname === 'cdn.jsdelivr.net') {
			return fulfillCss(route);
		}

		if (!pathname.startsWith('/api/')) {
			return route.continue();
		}

		apiHitCounts[hitKey] = (apiHitCounts[hitKey] || 0) + 1;

		if (pathname === '/api/Installer/Instance' && method === 'GET') {
			return fulfillJson(route, {
				code: 10000,
				data: {
					installed,
					version: '3.5.0',
					name: 'IoTSharp',
				},
			});
		}

		if (pathname === '/api/Installer/Install' && method === 'POST') {
			return fulfillJson(route, {
				code: 10000,
				data: true,
				msg: 'Installed',
			});
		}

		if (pathname === '/api/account/create' && method === 'POST') {
			return fulfillJson(route, {
				code: 10000,
				data: true,
				msg: 'Created',
			});
		}

		if (pathname === '/api/Captcha/Index' && method === 'GET') {
			return fulfillJson(route, {
				code: 10000,
				data: {
					bigImage: BIG_IMAGE_BASE64,
					smallImage: SMALL_IMAGE_BASE64,
					yheight: 62,
				},
			});
		}

		if (pathname === '/api/Account/Login' && method === 'POST') {
			return fulfillJson(route, {
				code: 10000,
				msg: 'Login succeeded',
				data: {
					token: {
						access_token: 'mock-token',
					},
				},
			});
		}

		if (pathname === '/api/Account/MyInfo' && method === 'GET') {
			return fulfillJson(route, {
				code: 10000,
				data: {
					name: 'iotmaster@iotsharp.net',
					avatar: '',
					roles: 'admin',
					customer: { id: 'customer-001', name: 'Demo Customer' },
					tenant: { id: 'tenant-001', name: 'Demo Tenant' },
				},
			});
		}

		if (pathname === '/api/Menu/GetProfile' && method === 'GET') {
			return fulfillJson(route, {
				code: 10000,
				data: {
					menu: buildMenuTree(),
				},
			});
		}

		if (pathname === '/api/home/kanban' && method === 'GET') {
			return fulfillJson(route, {
				code: 10000,
				data: {
					eventCount: 25872,
					onlineDeviceCount: 1128,
					attributesDataCount: 84563,
					deviceCount: 1276,
					alarmsCount: 12,
					userCount: 24,
					produceCount: 18,
					rulesCount: 42,
				},
			});
		}

		if (pathname === '/api/healthChecks' && method === 'GET') {
			return fulfillJson(route, [
				{
					entries: {
						mqtt: {
							status: 'Healthy',
							description: 'Broker connection is healthy',
							duration: '00:00:12',
						},
						storage: {
							status: 'Healthy',
							description: 'Telemetry storage is available',
							duration: '00:00:08',
						},
						rules: {
							status: 'Healthy',
							description: 'Rules engine is active',
							duration: '00:00:05',
						},
					},
				},
			]);
		}

		if (pathname === '/api/Metrics/EventBus' && method === 'GET') {
			return fulfillJson(route, {
				code: 10000,
				data: {
					servers: 2,
					subscribers: 18,
					publishedSucceeded: 18642,
					receivedSucceeded: 7048,
					publishedFailed: 3,
					receivedFailed: 2,
					dayHour: ['00', '04', '08', '12', '16', '20'],
					publishSuccessed: [820, 1160, 3540, 4820, 4210, 4092],
					publishFailed: [0, 1, 0, 1, 0, 1],
					subscribeSuccessed: [180, 240, 920, 1780, 1886, 2042],
					subscribeFailed: [0, 0, 1, 0, 1, 0],
				},
			});
		}

		if (pathname === '/api/Account/List' && method === 'GET') {
			return fulfillJson(route, {
				code: 10000,
				data: {
					rows: [
						{
							id: 'user-001',
							tenantName: 'Demo Tenant',
							customerName: 'Demo Customer',
							userName: 'iotmaster@iotsharp.net',
							email: 'iotmaster@iotsharp.net',
							phoneNumber: '13800000000',
							lockoutEnabled: false,
							accessFailedCount: 0,
						},
						{
							id: 'user-002',
							tenantName: 'Demo Tenant',
							customerName: 'Demo Customer',
							userName: 'operator@iotsharp.net',
							email: 'operator@iotsharp.net',
							phoneNumber: '13900000000',
							lockoutEnabled: true,
							accessFailedCount: 2,
						},
					],
					total: 2,
				},
			});
		}

		unhandledApiRequests.push({
			method,
			pathname,
			search: url.search,
		});
		return fulfillJson(route, {
			code: 10000,
			data: {},
		});
	});

	return { unhandledApiRequests, apiHitCounts };
}

function attachDiagnostics(page) {
	const diagnostics = {
		consoleErrors: [],
		pageErrors: [],
		requestFailures: [],
	};

	page.on('console', (message) => {
		if (message.type() === 'error') diagnostics.consoleErrors.push(message.text());
	});

	page.on('pageerror', (error) => {
		diagnostics.pageErrors.push(error.message);
	});

	page.on('requestfailed', (request) => {
		const url = request.url();
		const errorText = request.failure()?.errorText || 'unknown';
		if (errorText === 'net::ERR_ABORTED') return;
		if (!url.startsWith(APP_URL) && !url.includes('/api/')) return;
		diagnostics.requestFailures.push({
			url,
			errorText,
		});
	});

	return diagnostics;
}

async function saveDiagnostics(testInfo, diagnostics, unhandledApiRequests) {
	const filePath = path.join(ARTIFACT_DIR, `${slugify(testInfo.title)}-diagnostics.json`);
	fs.writeFileSync(
		filePath,
		JSON.stringify(
			{
				...diagnostics,
				unhandledApiRequests,
			},
			null,
			2
		)
	);
}

async function assertHealthy(testInfo, diagnostics, unhandledApiRequests) {
	await saveDiagnostics(testInfo, diagnostics, unhandledApiRequests);
	expect(diagnostics.pageErrors, 'page errors should stay empty').toEqual([]);
	expect(diagnostics.consoleErrors, 'console errors should stay empty').toEqual([]);
	expect(diagnostics.requestFailures, 'request failures should stay empty').toEqual([]);
	expect(unhandledApiRequests, 'every API request should be mocked explicitly').toEqual([]);
}

async function saveShot(page, name) {
	await page.screenshot({
		path: path.join(ARTIFACT_DIR, name),
		fullPage: true,
	});
}

async function gotoHash(page, hashPath) {
	await page.goto(`${APP_URL}/#${hashPath}`);
}

async function dragCaptchaSlider(page) {
	const runway = page.locator('.account-login__verify-dialog .el-slider__runway').first();

	await expect(runway).toBeVisible();

	const runwayBox = await runway.boundingBox();

	if (!runwayBox) {
		throw new Error('Captcha slider geometry is unavailable');
	}

	await runway.click({
		position: {
			x: Math.max(runwayBox.width - 8, 8),
			y: runwayBox.height / 2,
		},
	});
}

async function seedAuthCookie(page) {
	await page.context().addCookies([
		{
			name: 'token',
			value: 'mock-token',
			url: APP_URL,
		},
	]);
}

async function stubWindowOpen(page) {
	await page.addInitScript(() => {
		window.__openedUrls = [];
		window.open = (url, target = '', features = '') => {
			window.__openedUrls.push({
				url: String(url),
				target: String(target),
				features: String(features),
			});
			return null;
		};
	});
}

async function latestOpenedUrl(page) {
	return page.evaluate(() => {
		const items = window.__openedUrls || [];
		return items.length ? items[items.length - 1].url : null;
	});
}

test('public pages click through landing, login, and signup', async ({ page }, testInfo) => {
	const diagnostics = attachDiagnostics(page);
	const { unhandledApiRequests } = await installMocks(page, { installed: true });

	await gotoHash(page, '/');
	await expect(page).toHaveURL(/#\/$/);
	await expect(page.locator('.landing-page')).toBeVisible();
	await saveShot(page, 'landing-page.png');

	await page.locator('.landing-header__login').click();
	await expect(page).toHaveURL(/#\/login$/);
	await expect(page.locator('.auth-page')).toBeVisible();
	await saveShot(page, 'login-page.png');

	await page.locator('.account-login__signup > a').click();
	await expect(page).toHaveURL(/#\/signup$/);
	await expect(page.locator('.signup-page')).toBeVisible();
	await saveShot(page, 'signup-page.png');

	await assertHealthy(testInfo, diagnostics, unhandledApiRequests);
});

test('installer page loads and submits', async ({ page }, testInfo) => {
	const diagnostics = attachDiagnostics(page);
	const { unhandledApiRequests } = await installMocks(page, { installed: false });

	await gotoHash(page, '/installer');
	await expect(page).toHaveURL(/#\/installer$/);
	await expect(page.locator('.installer-page')).toBeVisible();
	await saveShot(page, 'installer-page.png');

	const inputs = page.locator('.installer-form-card input');
	await expect(inputs).toHaveCount(4);

	await inputs.nth(0).fill('iotmaster@iotsharp.net');
	await inputs.nth(1).fill('iotmaster@iotsharp.net');
	await inputs.nth(2).fill('MockPassword!123');
	await inputs.nth(3).fill('MockPassword!123');

	await page.locator('.installer-form-card .el-button--primary').click();
	await expect(page).toHaveURL(/#\/login$/);

	await assertHealthy(testInfo, diagnostics, unhandledApiRequests);
});

test('console flow logs in and navigates into a console workspace', async ({ page }, testInfo) => {
	const diagnostics = attachDiagnostics(page);
	const { unhandledApiRequests } = await installMocks(page, { installed: true });

	await gotoHash(page, '/login');
	await expect(page.locator('.auth-page')).toBeVisible();
	await page.locator('.account-login__field--2 input').fill('MockPassword!123');
	await page.locator('.account-login__submit').click();

	await expect(page.locator('.account-login__verify-dialog')).toBeVisible();
	await dragCaptchaSlider(page);

	await expect(page).toHaveURL(/#\/dashboard$/);
	await expect(page.locator('.workspace-page')).toBeVisible();
	await saveShot(page, 'dashboard-page.png');

	await expect(page.locator('.quick-grid .quick-item')).toHaveCount(4);
	await expect(page.locator('.quick-grid .quick-item').filter({ hasText: 'User Management' })).toBeVisible();
	await page.locator('.layout-aside .el-menu-item').filter({ hasText: 'User Management' }).click();

	await expect(page).toHaveURL(/#\/iot\/settings\/userlist$/);
	await expect(page.locator('.user-page')).toBeVisible();
	await saveShot(page, 'user-management-page.png');

	await page.locator('.layout-aside .el-menu-item').filter({ hasText: 'Control Overview' }).click();
	await expect(page).toHaveURL(/#\/dashboard$/);
	await expect(page.locator('.workspace-page')).toBeVisible();

	await assertHealthy(testInfo, diagnostics, unhandledApiRequests);
});

test('dashboard quick actions refresh, open links, and navigate to route', async ({ page }, testInfo) => {
	const diagnostics = attachDiagnostics(page);
	const { unhandledApiRequests, apiHitCounts } = await installMocks(page, { installed: true });

	await stubWindowOpen(page);
	await seedAuthCookie(page);
	await gotoHash(page, '/dashboard');
	await expect(page).toHaveURL(/#\/dashboard$/);
	await expect(page.locator('.workspace-page')).toBeVisible();
	await expect(page.locator('.quick-grid .quick-item')).toHaveCount(4);

	expect(apiHitCounts['GET /api/home/kanban']).toBe(1);
	expect(apiHitCounts['GET /api/healthChecks']).toBe(1);
	expect(apiHitCounts['GET /api/Metrics/EventBus']).toBe(1);

	await page.locator('.quick-grid .quick-item').filter({ hasText: '刷新数据' }).click();
	await expect.poll(() => apiHitCounts['GET /api/home/kanban'] || 0).toBe(2);
	await expect.poll(() => apiHitCounts['GET /api/healthChecks'] || 0).toBe(2);
	await expect.poll(() => apiHitCounts['GET /api/Metrics/EventBus'] || 0).toBe(2);

	await page.locator('.quick-grid .quick-item').filter({ hasText: '文档中心' }).click();
	await expect.poll(() => latestOpenedUrl(page)).toBe('http://docs.iotsharp.net/');

	await page.locator('.quick-grid .quick-item').filter({ hasText: '项目仓库' }).click();
	await expect.poll(() => latestOpenedUrl(page)).toBe('https://github.com/IoTSharp');

	await page.locator('.quick-grid .quick-item').filter({ hasText: 'User Management' }).click();
	await expect(page).toHaveURL(/#\/iot\/settings\/userlist$/);
	await expect(page.locator('.user-page')).toBeVisible();
	await saveShot(page, 'quick-action-user-management-page.png');

	await assertHealthy(testInfo, diagnostics, unhandledApiRequests);
});
