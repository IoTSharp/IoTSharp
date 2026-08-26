import { defineStore } from 'pinia';

export const LOCKED_CONSOLE_LAYOUT = 'defaults';

export const defaultThemeConfig: ThemeConfigState['themeConfig'] = {
	isDrawer: false,
	primary: '#1E5B4F',
	isIsDark: false,
	topBar: '#ffffff',
	topBarColor: '#183B36',
	isTopBarColorGradual: false,
	menuBar: '#183B36',
	menuBarColor: 'rgba(255, 255, 255, 0.76)',
	menuBarActiveColor: 'rgba(255, 255, 255, 0.14)',
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
	isTagsview: true,
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
	globalComponentSize: 'default',
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
		globalComponentSize: defaultThemeConfig.globalComponentSize,
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
