import type { App } from 'vue';
import type { RouteLocationNormalizedLoaded, Router } from 'vue-router';

const publicRouteNames = new Set(['landing', 'login', 'signup', 'installer', 'profile', 'notFound', 'noPower']);
const publicRoutePaths = new Set(['/', '/login', '/signup', '/installer', '/profile', '/401']);

let consolePluginsInstalled = false;
let consolePluginsPromise: Promise<void> | undefined;

export const routeRequiresConsoleFeatures = (route: RouteLocationNormalizedLoaded) => {
	const routeName = typeof route.name === 'string' ? route.name : '';
	if (publicRouteNames.has(routeName) || publicRoutePaths.has(route.path)) return false;

	if (route.matched.some((record) => record.name === 'console-shell' || record.path === '/console')) {
		return true;
	}

	return route.path.startsWith('/dashboard') || route.path.startsWith('/iot/');
};

const ensureConsolePlugins = async (app: App) => {
	if (consolePluginsInstalled) return;
	if (!consolePluginsPromise) {
		consolePluginsPromise = (async () => {
			const { installConsolePlugins } = await import('/@/bootstrap/consolePlugins');
			installConsolePlugins(app);
			consolePluginsInstalled = true;
		})().catch((error) => {
			consolePluginsPromise = undefined;
			throw error;
		});
	}

	await consolePluginsPromise;
};

export const ensureRouteFeatureInstall = async (app: App, route: RouteLocationNormalizedLoaded) => {
	if (routeRequiresConsoleFeatures(route)) {
		await ensureConsolePlugins(app);
	}
};

export const setupRouteFeatureGuards = (app: App, router: Router) => {
	router.beforeEach(async (to) => {
		await ensureRouteFeatureInstall(app, to);
	});
};
