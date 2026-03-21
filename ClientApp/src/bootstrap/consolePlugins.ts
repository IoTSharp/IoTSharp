import type { App } from 'vue';
import { FastCrud } from '@fast-crud/fast-crud';
import '@fast-crud/fast-crud/dist/style.css';
import ui from '@fast-crud/ui-element';
import { installConsoleUi } from '/@/bootstrap/consoleUi';

let consolePluginsInstalled = false;

export const installConsolePlugins = (app: App) => {
	if (consolePluginsInstalled) return;

	installConsoleUi(app);
	app.use(ui);
	app.use(FastCrud);
	consolePluginsInstalled = true;
};
