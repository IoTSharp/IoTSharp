import { defineStore } from 'pinia';

export const LOCKED_CONSOLE_LAYOUT = 'defaults';

export const defaultThemeConfig: ThemeConfigState['themeConfig'] = {
	isDrawer: false,
	primary: '#165dff',
	isIsDark: false,
	topBar: '#ffffff',
	topBarColor: '#1d2129',
	isTopBarColorGradual: false,
	menuBar: '#ffffff',
	menuBarColor: '#4e5969',
	menuBarActiveColor: 'rgba(22, 93, 255, 0.1)',
	isMenuBarColorGradual: false,
	columnsMenuBar: '#ffffff',
	columnsMenuBarColor: '#4e5969',
	isColumnsMenuBarColorGradual: false,
	isColumnsMenuHoverPreload: false,
	isCollapse: false,
	isUniqueOpened: true,
	isFixedHeader: true,
	isFixedHeaderChange: false,
	isClassicSplitMenu: false,
	isLockScreen: false,
	lockScreenTime: 30,
	isShowLogo: true,
	isShowLogoChange: false,
	isBreadcrumb: true,
	isTagsview: false,
	isBreadcrumbIcon: false,
	isTagsviewIcon: false,
	isCacheTagsView: false,
	isSortableTagsView: true,
	isShareTagsView: false,
	isFooter: false,
	isGrayscale: false,
	isInvert: false,
	isWartermark: false,
	wartermarkText: 'IoTSharp',
	tagsStyle: 'tags-style-one',
	animation: 'slide-right',
	columnsAsideStyle: 'columns-round',
	columnsAsideLayout: 'columns-vertical',
	layout: LOCKED_CONSOLE_LAYOUT,
	isRequestRoutes: true,
	globalTitle: 'IoTSharp',
	globalViceTitle: 'IoTSharp',
	globalViceTitleMsg: 'Open and extensible IoT platform',
	globalI18n: 'zh-cn',
	globalComponentSize: 'large',
};

const normalizeThemeConfig = (config: Partial<ThemeConfigState['themeConfig']> = {}): ThemeConfigState['themeConfig'] => {
	const nextConfig = {
		...defaultThemeConfig,
		...config,
	};

	return {
		...nextConfig,
		isDrawer: false,
		primary: defaultThemeConfig.primary,
		isIsDark: defaultThemeConfig.isIsDark,
		topBar: defaultThemeConfig.topBar,
		topBarColor: defaultThemeConfig.topBarColor,
		isTopBarColorGradual: defaultThemeConfig.isTopBarColorGradual,
		menuBar: defaultThemeConfig.menuBar,
		menuBarColor: defaultThemeConfig.menuBarColor,
		menuBarActiveColor: defaultThemeConfig.menuBarActiveColor,
		isMenuBarColorGradual: defaultThemeConfig.isMenuBarColorGradual,
		columnsMenuBar: defaultThemeConfig.columnsMenuBar,
		columnsMenuBarColor: defaultThemeConfig.columnsMenuBarColor,
		isColumnsMenuBarColorGradual: defaultThemeConfig.isColumnsMenuBarColorGradual,
		isColumnsMenuHoverPreload: defaultThemeConfig.isColumnsMenuHoverPreload,
		isClassicSplitMenu: false,
		isGrayscale: false,
		isInvert: false,
		isWartermark: false,
		layout: LOCKED_CONSOLE_LAYOUT,
	};
};

export const useThemeConfig = defineStore('themeConfig', {
	state: (): ThemeConfigState => ({
		themeConfig: { ...defaultThemeConfig },
	}),
	actions: {
		setThemeConfig(data: Partial<ThemeConfigState> | Partial<ThemeConfigState['themeConfig']>) {
			const nextConfig = data && 'themeConfig' in data ? data.themeConfig : data;
			this.themeConfig = normalizeThemeConfig(nextConfig);
		},
	},
});
