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
};

export const uiThemes: UiTheme[] = [
	{ id: 'ocean-blue', name: '远洋蓝', description: '清晰稳定的站级视图', dark: '#0E4C7A', light: '#DCECF7', ink: '#12324A', surface: '#F5FAFD', deep: '#092F50', glow: '#B9E5FA', signal: '#2C93D1', signalSoft: '#E5F4FC', contrast: '#E0A85A' },
	{ id: 'pine-ops', name: '松针绿', description: '强调运维状态的默认主题', dark: '#1E5B4F', light: '#DCEEE7', ink: '#183B36', surface: '#F5FBF8', deep: '#143E35', glow: '#BCE6D4', signal: '#47B59A', signalSoft: '#E4F5EF', contrast: '#D6964C' },
	{ id: 'steel-lane', name: '车道钢青', description: '适合现场连续工作的钢青色', dark: '#24566B', light: '#DCECF3', ink: '#193B49', surface: '#F5FAFC', deep: '#173C4A', glow: '#B8DFEE', signal: '#4EABC7', signalSoft: '#E5F4F8', contrast: '#D9865F' },
	{ id: 'sage-quiet', name: '静谧鼠尾草', description: '低饱和的长时间工作主题', dark: '#526B55', light: '#E3ECD8', ink: '#304237', surface: '#F8FBF5', deep: '#3B5140', glow: '#D2E6C7', signal: '#77AA7D', signalSoft: '#EDF6E7', contrast: '#C58A58' },
	{ id: 'plum-editorial', name: '编辑紫', description: '适合配置、审阅与内容工作', dark: '#6D4B78', light: '#F0E1F2', ink: '#49324F', surface: '#FCF8FC', deep: '#53385D', glow: '#E6C8EA', signal: '#A66AB7', signalSoft: '#F6EAF7', contrast: '#DB9A67' },
	{ id: 'mist-teal', name: '雾霭青', description: '轻盈并保持数据对比度', dark: '#3D7180', light: '#DCEFF1', ink: '#28525E', surface: '#F5FBFC', deep: '#2F5C67', glow: '#B9E4E6', signal: '#53B9B3', signalSoft: '#E5F7F5', contrast: '#E0A058' },
	{ id: 'terracotta', name: '陶土棕', description: '温暖可靠的业务台面', dark: '#8A5A42', light: '#F3E2D5', ink: '#5E3E2F', surface: '#FDF9F5', deep: '#68412F', glow: '#F1CDB5', signal: '#C77C5A', signalSoft: '#FBEDE5', contrast: '#7B6BB8' },
	{ id: 'forest', name: '深林绿', description: '适合高密度监控与现场视图', dark: '#2F5B48', light: '#DCECE4', ink: '#214336', surface: '#F5FBF8', deep: '#214235', glow: '#B8DFCC', signal: '#4DAA82', signalSoft: '#E6F5ED', contrast: '#D69B4A' },
	{ id: 'cranberry', name: '蔓越莓', description: '具有更强层级提示的主题', dark: '#922D42', light: '#F8DCE2', ink: '#5E2432', surface: '#FFF8F9', deep: '#6E2335', glow: '#F2B6C2', signal: '#CA5F7A', signalSoft: '#FBE9ED', contrast: '#D79B52' },
	{ id: 'amber', name: '琥珀金', description: '适合发布与复核场景', dark: '#8A5D2D', light: '#F6E7C8', ink: '#5D421F', surface: '#FFFBF3', deep: '#68471F', glow: '#F0D59B', signal: '#D39C3D', signalSoft: '#FFF4DC', contrast: '#477B9B' },
	{ id: 'slate', name: '岩板蓝灰', description: '克制专业的后台主题', dark: '#3F4D66', light: '#E2E7F0', ink: '#2B3548', surface: '#F7F9FC', deep: '#303B50', glow: '#C8D4E7', signal: '#6D8BC1', signalSoft: '#EDF1F8', contrast: '#C17F64' },
	{ id: 'rosewood', name: '玫瑰木', description: '柔和且具有辨识度', dark: '#7B3F45', light: '#F2DFE0', ink: '#542B30', surface: '#FFF8F8', deep: '#613237', glow: '#E9BFC2', signal: '#BE6A71', signalSoft: '#F9E9EA', contrast: '#C29A52' },
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
		'--iotsharp-theme-deep': theme.deep,
		'--iotsharp-theme-glow': theme.glow,
		'--iotsharp-theme-signal': theme.signal,
		'--iotsharp-theme-signal-soft': theme.signalSoft,
		'--iotsharp-theme-contrast': theme.contrast,
		'--iotsharp-accent': theme.dark,
		'--iotsharp-accent-rgb': `${red}, ${green}, ${blue}`,
		'--iotsharp-selection': theme.light,
		'--iotsharp-page': theme.surface,
		'--iotsharp-ink': theme.ink,
		'--iotsharp-nav-gradient': `linear-gradient(155deg, ${theme.deep} 0%, ${theme.dark} 52%, ${theme.signal} 135%)`,
		'--iotsharp-topbar-gradient': `linear-gradient(112deg, rgba(255, 255, 255, 0.94) 0%, ${theme.light}B8 52%, ${theme.glow}66 100%)`,
		'--iotsharp-accent-gradient': `linear-gradient(135deg, ${theme.dark} 0%, ${theme.deep} 100%)`,
		'--iotsharp-quiet-gradient': `linear-gradient(135deg, ${theme.light} 0%, ${theme.surface} 72%, ${theme.glow}55 100%)`,
		'--iotsharp-page-gradient': `radial-gradient(circle at 12% 16%, ${theme.glow}66, transparent 28%), radial-gradient(circle at 88% 82%, ${theme.light}99, transparent 32%), linear-gradient(145deg, ${theme.surface} 0%, #ffffff 52%, ${theme.light}66 100%)`,
		'--iotsharp-panel-gradient': `linear-gradient(135deg, #ffffff 0%, ${theme.signalSoft} 100%)`,
		'--iotsharp-hero-gradient': `radial-gradient(circle at 92% 12%, ${theme.glow}88, transparent 26%), linear-gradient(135deg, ${theme.light} 0%, ${theme.surface} 72%)`,
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
