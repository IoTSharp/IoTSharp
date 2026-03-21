import { ElMessage } from 'element-plus';

const HEX_COLOR_REGEXP = /^\#?[0-9A-Fa-f]{6}$/;
const RGB_COLOR_REGEXP = /^\d{1,3}$/;

export const hexToRgb = (str: string): number[] | '' => {
	let hexs: string[] | null = null;
	if (!HEX_COLOR_REGEXP.test(str)) {
		ElMessage.warning('输入错误的hex');
		return '';
	}

	str = str.replace('#', '');
	hexs = str.match(/../g);
	if (!hexs) return '';

	return hexs.map((item) => parseInt(item, 16));
};

export const rgbToHex = (r: number, g: number, b: number): string => {
	if (!RGB_COLOR_REGEXP.test(String(r)) || !RGB_COLOR_REGEXP.test(String(g)) || !RGB_COLOR_REGEXP.test(String(b))) {
		ElMessage.warning('输入错误的rgb颜色值');
		return '';
	}

	const hexs = [r, g, b].map((item) => {
		const hex = item.toString(16);
		return hex.length === 1 ? `0${hex}` : hex;
	});

	return `#${hexs.join('')}`;
};

export const getDarkColor = (color: string, level: number): string => {
	if (!HEX_COLOR_REGEXP.test(color)) {
		ElMessage.warning('输入错误的hex颜色值');
		return '';
	}

	const rgb = hexToRgb(color);
	if (!rgb) return '';

	return rgbToHex(
		Math.floor(rgb[0] * (1 - level)),
		Math.floor(rgb[1] * (1 - level)),
		Math.floor(rgb[2] * (1 - level))
	);
};

export const getLightColor = (color: string, level: number): string => {
	if (!HEX_COLOR_REGEXP.test(color)) {
		ElMessage.warning('输入错误的hex颜色值');
		return '';
	}

	const rgb = hexToRgb(color);
	if (!rgb) return '';

	return rgbToHex(
		Math.floor((255 - rgb[0]) * level + rgb[0]),
		Math.floor((255 - rgb[1]) * level + rgb[1]),
		Math.floor((255 - rgb[2]) * level + rgb[2])
	);
};

export function useChangeColor() {
	return {
		hexToRgb,
		rgbToHex,
		getDarkColor,
		getLightColor,
	};
}
