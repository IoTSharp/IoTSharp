export type UiTheme = {
	id: string;
	name: string;
	description: string;
	dark: string;
	light: string;
	ink: string;
	surface: string;
	deep: string;
	glow: string;
	signal: string;
	signalSoft: string;
	contrast: string;
	regions?: ThemeRegions;
};

type ThemeRegions = {
	mode: 'light' | 'dark';
	navigation: string;
	topbar: string;
	panel: string;
	mutedPanel: string;
	border: string;
	textSoft: string;
	textMuted: string;
	onAccent: string;
	profile: [accent: string, tint: string];
	inbox: [accent: string, tint: string];
	security: [accent: string, tint: string];
	data: [accent: string, tint: string];
	operation: [accent: string, tint: string];
};

const coordinatedThemes: UiTheme[] = [
	{
		id: 'deep-sea', name: '深海蓝绿', description: '海军蓝导航 · 青绿资料 · 钴蓝信箱',
		dark: '#087F74', light: '#DCE9EF', ink: '#163140', surface: '#EAF1F4', deep: '#21586C',
		glow: '#D9E8F5', signal: '#315EB2', signalSoft: '#E8EFFC', contrast: '#865925',
		regions: {
			mode: 'light', navigation: '#112C39', topbar: '#F5FAFC', panel: '#FFFFFF', mutedPanel: '#F2F6F8',
			border: '#D5E1E7', textSoft: '#415B69', textMuted: '#516D7A', onAccent: '#FFFFFF',
			profile: ['#087F74', '#E1F4EE'], inbox: ['#315EB2', '#E8EFFC'], security: ['#69478E', '#F0EAF8'],
			data: ['#315EB2', '#E8EFFC'], operation: ['#865925', '#F8EEDF'],
		},
	},
	{
		id: 'warm-stone', name: '暖岩铜绿', description: '暖褐导航 · 森林绿资料 · 陶土信箱',
		dark: '#3C6B4E', light: '#EADFD1', ink: '#382D25', surface: '#F5EEE4', deep: '#32553F',
		glow: '#EDDACA', signal: '#9A492F', signalSoft: '#F8E9DE', contrast: '#5E467D',
		regions: {
			mode: 'light', navigation: '#4E4038', topbar: '#FBF5EC', panel: '#FFFCF7', mutedPanel: '#F6F0E8',
			border: '#E0D2C3', textSoft: '#625247', textMuted: '#705D50', onAccent: '#FFFFFF',
			profile: ['#3C6B4E', '#E9F0E3'], inbox: ['#9A492F', '#F8E9DE'], security: ['#5E467D', '#F0E8F5'],
			data: ['#5E467D', '#F0E8F5'], operation: ['#345E7A', '#E5EFF5'],
		},
	},
	{
		id: 'night-indigo', name: '夜间蓝紫', description: '墨蓝导航 · 亮青资料 · 淡紫信箱',
		dark: '#58D7D1', light: '#26334F', ink: '#EDF3FF', surface: '#101828', deep: '#79BDF0',
		glow: '#30466B', signal: '#B8A4FC', signalSoft: '#30294A', contrast: '#FFCE84',
		regions: {
			mode: 'dark', navigation: '#11182C', topbar: '#151F34', panel: '#192438', mutedPanel: '#202D43',
			border: '#34435A', textSoft: '#C3CDE0', textMuted: '#A1AEC4', onAccent: '#101828',
			profile: ['#58D7D1', '#173841'], inbox: ['#B8A4FC', '#30294A'], security: ['#FFCE84', '#3B3027'],
			data: ['#B8A4FC', '#30294A'], operation: ['#FFCE84', '#3B3027'],
		},
	},
];

export const uiThemes = coordinatedThemes;

const storageKey = 'iotsharp-ui-theme';
const textureStorageKey = 'iotsharp-ui-texture';
const defaultThemeId = 'deep-sea';

const findTheme = (id?: string | null) => uiThemes.find((theme) => theme.id === id) ?? uiThemes.find((theme) => theme.id === defaultThemeId)!;

const hexToRgb = (hex: string) => {
	const value = hex.replace('#', '');
	return [Number.parseInt(value.slice(0, 2), 16), Number.parseInt(value.slice(2, 4), 16), Number.parseInt(value.slice(4, 6), 16)];
};

const mix = (foreground: string, background: string, foregroundWeight: number) => {
	const fg = hexToRgb(foreground);
	const bg = hexToRgb(background);
	const weight = foregroundWeight / 100;
	return `#${fg.map((channel, index) => Math.round(channel * weight + bg[index] * (1 - weight)).toString(16).padStart(2, '0')).join('')}`;
};

export function applyUiTheme(id: string) {
	const theme = findTheme(id);
	const regions = theme.regions;
	const isDark = regions?.mode === 'dark';
	const panel = regions?.panel ?? '#FFFFFF';
	const mutedPanel = regions?.mutedPanel ?? '#F4F7F6';
	const topbar = regions?.topbar ?? '#FFFFFF';
	const onAccent = regions?.onAccent ?? '#FFFFFF';
	const [profileAccent, profileSurface] = regions?.profile ?? ['#087569', '#E1F4EE'];
	const [inboxAccent, inboxSurface] = regions?.inbox ?? ['#315EB2', '#E8EFFC'];
	const [securityAccent, securitySurface] = regions?.security ?? ['#69478E', '#F0EAF8'];
	const [dataAccent, dataSurface] = regions?.data ?? ['#315EB2', '#E8EFFC'];
	const [operationAccent, operationSurface] = regions?.operation ?? ['#865925', '#F8EEDF'];
	const [red, green, blue] = hexToRgb(theme.dark);
	const variables: Record<string, string> = {
		'--iotsharp-theme-dark': theme.dark,
		'--iotsharp-theme-light': theme.light,
		'--iotsharp-theme-ink': theme.ink,
		'--iotsharp-theme-surface': theme.surface,
		'--iotsharp-theme-deep': theme.deep,
		'--iotsharp-theme-glow': theme.glow,
		'--iotsharp-theme-signal': theme.signal,
		'--iotsharp-theme-signal-soft': theme.signalSoft,
		'--iotsharp-theme-contrast': theme.contrast,
		'--iotsharp-accent': theme.dark,
		'--iotsharp-accent-rgb': `${red}, ${green}, ${blue}`,
		'--iotsharp-on-accent': onAccent,
		'--iotsharp-selection': theme.light,
		'--iotsharp-page': theme.surface,
		'--iotsharp-ink': theme.ink,
		'--iotsharp-text': regions ? theme.ink : '#23332F',
		'--iotsharp-text-soft': regions?.textSoft ?? '#52635F',
		'--iotsharp-text-muted': regions?.textMuted ?? '#657570',
		'--iotsharp-surface': panel,
		'--iotsharp-surface-soft': `${panel}DB`,
		'--iotsharp-surface-muted': mutedPanel,
		'--iotsharp-border': regions?.border ?? '#DFE7E4',
		'--iotsharp-border-strong': regions ? mix(theme.ink, regions.border, 22) : '#C9D5D1',
		'--iotsharp-nav-gradient': regions ? regions.navigation : `linear-gradient(155deg, ${theme.deep} 0%, ${theme.dark} 52%, ${theme.signal} 135%)`,
		'--iotsharp-nav-indicator': isDark ? profileAccent : theme.glow,
		'--iotsharp-topbar-gradient': regions ? topbar : `linear-gradient(112deg, ${topbar}F0 0%, ${theme.light}B8 52%, ${theme.glow}66 100%)`,
		'--iotsharp-accent-gradient': `linear-gradient(135deg, ${theme.dark} 0%, ${theme.deep} 100%)`,
		'--iotsharp-quiet-gradient': `linear-gradient(135deg, ${theme.light} 0%, ${theme.surface} 72%, ${theme.glow}55 100%)`,
		'--iotsharp-page-gradient': `radial-gradient(circle at 12% 16%, ${theme.glow}66, transparent 28%), radial-gradient(circle at 88% 82%, ${theme.light}99, transparent 32%), linear-gradient(145deg, ${theme.surface} 0%, ${panel} 52%, ${theme.light}66 100%)`,
		'--iotsharp-panel-gradient': `linear-gradient(135deg, ${panel} 0%, ${theme.signalSoft} 100%)`,
		'--iotsharp-hero-gradient': `radial-gradient(circle at 92% 12%, ${theme.glow}88, transparent 26%), linear-gradient(135deg, ${theme.light} 0%, ${theme.surface} 72%)`,
		'--iotsharp-profile-accent': profileAccent,
		'--iotsharp-profile-surface': profileSurface,
		'--iotsharp-profile-on-accent': onAccent,
		'--iotsharp-inbox-accent': inboxAccent,
		'--iotsharp-inbox-surface': inboxSurface,
		'--iotsharp-inbox-on-accent': onAccent,
		'--iotsharp-security-accent': securityAccent,
		'--iotsharp-security-surface': securitySurface,
		'--iotsharp-data-accent': dataAccent,
		'--iotsharp-data-surface': dataSurface,
		'--iotsharp-operation-accent': operationAccent,
		'--iotsharp-operation-surface': operationSurface,
		'--el-color-primary': theme.dark,
		'--el-color-primary-rgb': `${red}, ${green}, ${blue}`,
		'--el-color-primary-dark-2': mix(theme.dark, isDark ? '#FFFFFF' : '#000000', 82),
		'--el-color-primary-light-3': mix(theme.dark, panel, 70),
		'--el-color-primary-light-5': mix(theme.dark, panel, 50),
		'--el-color-primary-light-7': mix(theme.dark, panel, 30),
		'--el-color-primary-light-8': mix(theme.dark, panel, 20),
		'--el-color-primary-light-9': mix(theme.dark, panel, 10),
	};

	const root = document.documentElement;
	Object.entries(variables).forEach(([name, value]) => root.style.setProperty(name, value));
	root.dataset.uiTheme = theme.id;
	root.classList.toggle('dark', isDark);
	return theme;
}

export function getUiTheme() {
	try {
		return findTheme(window.localStorage.getItem(storageKey));
	} catch {
		return findTheme(defaultThemeId);
	}
}

export function saveUiTheme(id: string) {
	const theme = applyUiTheme(id);
	try {
		window.localStorage.setItem(storageKey, theme.id);
	} catch {
		// 禁用本地存储时，当前会话仍可正常切换主题。
	}
	window.dispatchEvent(new CustomEvent('iotsharp-theme-change', { detail: { id: theme.id } }));
	return theme;
}

/** 读取控制台背景纹理偏好，未选择时启用主题纹理。 */
export function getUiTexture() {
	try {
		return window.localStorage.getItem(textureStorageKey) !== 'off';
	} catch {
		return true;
	}
}

/** 更新当前工作区背景并记住浏览器的选择。 */
export function saveUiTexture(visible: boolean) {
	document.documentElement.dataset.uiTexture = visible ? 'on' : 'off';
	try {
		window.localStorage.setItem(textureStorageKey, visible ? 'on' : 'off');
	} catch {
		// 存储受限时仍可在当前页面切换纹理。
	}
}

export function initializeUiTheme() {
	document.documentElement.dataset.uiTexture = getUiTexture() ? 'on' : 'off';
	return applyUiTheme(getUiTheme().id);
}
