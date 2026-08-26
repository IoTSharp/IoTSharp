export type UiTheme = {
	id: string;
	name: string;
	description: string;
	dark: string;
	light: string;
	ink: string;
	surface: string;
};

export const uiThemes: UiTheme[] = [
	{ id: 'ocean-blue', name: '远洋蓝', description: '清晰稳定的站级视图', dark: '#0E4C7A', light: '#DCECF7', ink: '#12324A', surface: '#F5FAFD' },
	{ id: 'pine-ops', name: '松针绿', description: '强调运维状态的默认主题', dark: '#1E5B4F', light: '#DCEEE7', ink: '#183B36', surface: '#F5FBF8' },
	{ id: 'steel-lane', name: '车道钢青', description: '适合现场连续工作的钢青色', dark: '#24566B', light: '#DCECF3', ink: '#193B49', surface: '#F5FAFC' },
	{ id: 'sage-quiet', name: '静谧鼠尾草', description: '低饱和的长时间工作主题', dark: '#526B55', light: '#E3ECD8', ink: '#304237', surface: '#F8FBF5' },
	{ id: 'plum-editorial', name: '编辑紫', description: '适合配置、审阅与内容工作', dark: '#6D4B78', light: '#F0E1F2', ink: '#49324F', surface: '#FCF8FC' },
	{ id: 'mist-teal', name: '雾霭青', description: '轻盈并保持数据对比度', dark: '#3D7180', light: '#DCEFF1', ink: '#28525E', surface: '#F5FBFC' },
	{ id: 'terracotta', name: '陶土棕', description: '温暖可靠的业务台面', dark: '#8A5A42', light: '#F3E2D5', ink: '#5E3E2F', surface: '#FDF9F5' },
	{ id: 'forest', name: '深林绿', description: '适合高密度监控与现场视图', dark: '#2F5B48', light: '#DCECE4', ink: '#214336', surface: '#F5FBF8' },
	{ id: 'cranberry', name: '蔓越莓', description: '具有更强层级提示的主题', dark: '#922D42', light: '#F8DCE2', ink: '#5E2432', surface: '#FFF8F9' },
	{ id: 'amber', name: '琥珀金', description: '适合发布与复核场景', dark: '#8A5D2D', light: '#F6E7C8', ink: '#5D421F', surface: '#FFFBF3' },
	{ id: 'slate', name: '岩板蓝灰', description: '克制专业的后台主题', dark: '#3F4D66', light: '#E2E7F0', ink: '#2B3548', surface: '#F7F9FC' },
	{ id: 'rosewood', name: '玫瑰木', description: '柔和且具有辨识度', dark: '#7B3F45', light: '#F2DFE0', ink: '#542B30', surface: '#FFF8F8' },
];

const storageKey = 'iotsharp-ui-theme';
const defaultThemeId = 'pine-ops';

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

const setVariable = (name: string, value: string) => document.documentElement.style.setProperty(name, value);

export function applyUiTheme(id: string) {
	const theme = findTheme(id);
	const [red, green, blue] = hexToRgb(theme.dark);
	const variables: Record<string, string> = {
		'--iotsharp-theme-dark': theme.dark,
		'--iotsharp-theme-light': theme.light,
		'--iotsharp-theme-ink': theme.ink,
		'--iotsharp-theme-surface': theme.surface,
		'--iotsharp-accent': theme.dark,
		'--iotsharp-accent-rgb': `${red}, ${green}, ${blue}`,
		'--iotsharp-selection': theme.light,
		'--iotsharp-page': theme.surface,
		'--iotsharp-ink': theme.ink,
		'--iotsharp-nav-gradient': `linear-gradient(180deg, ${theme.ink} 0%, ${theme.dark} 48%, ${mix(theme.dark, theme.ink, 42)} 100%)`,
		'--iotsharp-topbar-gradient': `linear-gradient(112deg, rgba(255, 255, 255, 0.94) 0%, ${theme.light}B8 52%, rgba(255, 255, 255, 0.9) 100%)`,
		'--iotsharp-accent-gradient': `linear-gradient(135deg, ${theme.dark} 0%, ${theme.ink} 100%)`,
		'--iotsharp-quiet-gradient': `linear-gradient(135deg, ${theme.light} 0%, ${theme.surface} 72%)`,
		'--el-color-primary': theme.dark,
		'--el-color-primary-rgb': `${red}, ${green}, ${blue}`,
		'--el-color-primary-dark-2': mix(theme.dark, '#000000', 82),
		'--el-color-primary-light-3': mix(theme.dark, '#ffffff', 70),
		'--el-color-primary-light-5': mix(theme.dark, '#ffffff', 50),
		'--el-color-primary-light-7': mix(theme.dark, '#ffffff', 30),
		'--el-color-primary-light-8': mix(theme.dark, '#ffffff', 20),
		'--el-color-primary-light-9': mix(theme.dark, '#ffffff', 10),
	};

	Object.entries(variables).forEach(([name, value]) => setVariable(name, value));
	document.documentElement.dataset.uiTheme = theme.id;
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

export function initializeUiTheme() {
	return applyUiTheme(getUiTheme().id);
}
