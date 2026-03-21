import { nextTick } from 'vue';
import { storeToRefs } from 'pinia';
import router from '/@/router/index';
import pinia from '/@/stores/index';
import { useThemeConfig } from '/@/stores/themeConfig';
import { i18n } from '/@/i18n/index';
import { Local } from '/@/utils/storage';
import { verifyUrl } from '/@/utils/toolsValidate';

export function useTitle() {
	const stores = useThemeConfig(pinia);
	const { themeConfig } = storeToRefs(stores);

	nextTick(() => {
		let webTitle = '';
		const globalTitle: string = themeConfig.value.globalTitle;
		const { path, meta } = router.currentRoute.value;

		if (path === '/login') {
			webTitle = meta.title as string;
		} else {
			webTitle = setTagsViewNameI18n(router.currentRoute.value);
		}

		document.title = `${webTitle} - ${globalTitle}` || globalTitle;
	});
}

export function setTagsViewNameI18n(item: any) {
	let tagsViewName = '';
	const { query, params, meta } = item;
	const pattern = /^\{("(zh-cn|en|zh-tw)":"[^,]+",?){1,3}}$/;

	if (query?.tagsViewName || params?.tagsViewName) {
		if (pattern.test(query?.tagsViewName) || pattern.test(params?.tagsViewName)) {
			const urlTagsParams = (query?.tagsViewName && JSON.parse(query?.tagsViewName)) || (params?.tagsViewName && JSON.parse(params?.tagsViewName));
			tagsViewName = urlTagsParams[i18n.global.locale.value];
		} else {
			tagsViewName = query?.tagsViewName || params?.tagsViewName;
		}
	} else {
		tagsViewName = i18n.global.t(meta.title);
	}

	return tagsViewName;
}

export const lazyImg = (el: string, arr: EmptyArrayType) => {
	const io = new IntersectionObserver((entries) => {
		entries.forEach((entry: any) => {
			if (!entry.isIntersecting) return;

			const { img, key } = entry.target.dataset;
			entry.target.src = img;
			entry.target.onload = () => {
				io.unobserve(entry.target);
				arr[key].loading = false;
			};
		});
	});

	nextTick(() => {
		document.querySelectorAll(el).forEach((img) => io.observe(img));
	});
};

export const globalComponentSize = (): string => {
	const stores = useThemeConfig(pinia);
	const { themeConfig } = storeToRefs(stores);
	return Local.get('themeConfig')?.globalComponentSize || themeConfig.value?.globalComponentSize;
};

export function deepClone(obj: EmptyObjectType) {
	let newObj: EmptyObjectType;
	try {
		newObj = obj.push ? [] : {};
	} catch (error) {
		newObj = {};
	}

	for (const attr in obj) {
		if (obj[attr] && typeof obj[attr] === 'object') {
			newObj[attr] = deepClone(obj[attr]);
		} else {
			newObj[attr] = obj[attr];
		}
	}

	return newObj;
}

export function isMobile() {
	return Boolean(
		navigator.userAgent.match(
			/('phone|pad|pod|iPhone|iPod|ios|iPad|Android|Mobile|BlackBerry|IEMobile|MQQBrowser|JUC|Fennec|wOSBrowser|BrowserNG|WebOS|Symbian|Windows Phone')/i
		)
	);
}

export function handleEmpty(list: EmptyArrayType) {
	const arr = [];
	for (const i in list) {
		const values = [];
		for (const j in list[i]) {
			values.push(list[i][j]);
		}
		const emptyCount = values.filter((item) => item === '').length;
		if (emptyCount !== values.length) {
			arr.push(list[i]);
		}
	}
	return arr;
}

export function handleOpenLink(val: RouteItem) {
	const { origin, pathname } = window.location;
	router.push(val.path);
	if (verifyUrl(val.meta?.isLink as string)) window.open(val.meta?.isLink);
	else window.open(`${origin}${pathname}#${val.meta?.isLink}`);
}

const other = {
	useTitle: () => {
		useTitle();
	},
	setTagsViewNameI18n(route: RouteToFrom) {
		return setTagsViewNameI18n(route);
	},
	lazyImg: (el: string, arr: EmptyArrayType) => {
		lazyImg(el, arr);
	},
	globalComponentSize: () => {
		return globalComponentSize();
	},
	deepClone: (obj: EmptyObjectType) => {
		return deepClone(obj);
	},
	isMobile: () => {
		return isMobile();
	},
	handleEmpty: (list: EmptyArrayType) => {
		return handleEmpty(list);
	},
	handleOpenLink: (val: RouteItem) => {
		handleOpenLink(val);
	},
};

export default other;

export function sleep(ms: number) {
	return new Promise((resolve) => setTimeout(resolve, ms));
}
