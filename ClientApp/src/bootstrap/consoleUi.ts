import { defineAsyncComponent } from 'vue';
import type { App } from 'vue';
import * as svg from '@element-plus/icons-vue';

const SvgIcon = defineAsyncComponent(() => import('/@/components/svgIcon/index.vue'));

let consoleUiInstalled = false;

export const installConsoleUi = (app: App) => {
	if (consoleUiInstalled) return;

	const icons = svg as Record<string, any>;
	for (const key in icons) {
		app.component(`ele-${icons[key].name}`, icons[key]);
	}

	app.component('SvgIcon', SvgIcon);
	consoleUiInstalled = true;
};
