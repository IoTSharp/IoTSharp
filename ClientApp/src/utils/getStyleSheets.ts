import * as svg from '@element-plus/icons-vue';

// 内网模式不再从远程 CDN 读取阿里或 FontAwesome 字体图标。
const getAlicdnIconfont = () => {
	return Promise.resolve([]);
};

// 初始化获取 css 样式，获取 element plus 自带 svg 图标，增加了 ele- 前缀，使用时：ele-Aim
const getElementPlusIconfont = () => {
	const icons = svg as any;
	const sheetsIconList = [];
	for (const i in icons) {
		sheetsIconList.push(`ele-${icons[i].name}`);
	}
	return Promise.resolve(sheetsIconList);
};

const getAwesomeIconfont = () => {
	return Promise.resolve([]);
};

/**
 * 获取字体图标 `document.styleSheets`
 * @method ali 获取阿里字体图标 `<i class="iconfont 图标类名"></i>`
 * @method ele 获取 element plus 自带图标 `<i class="图标类名"></i>`
 * @method ali 获取 fontawesome 的图标 `<i class="fa 图标类名"></i>`
 */
const initIconfont = {
	// iconfont
	ali: () => {
		return getAlicdnIconfont();
	},
	// element plus
	ele: () => {
		return getElementPlusIconfont();
	},
	// fontawesome
	awe: () => {
		return getAwesomeIconfont();
	},
};

// 导出方法
export default initIconfont;
