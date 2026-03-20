<template>
	<div class="layout-navbars-breadcrumb-index">
		<div class="layout-navbars-breadcrumb-index__left">
			<Logo v-if="setIsShowLogo" />
			<Breadcrumb />
		</div>
		<div class="layout-navbars-breadcrumb-index__status" v-if="!isLayoutTransverse">
			<div class="shell-pill shell-pill--brand">IoTSharp Console</div>
			<div class="shell-pill">{{ currentSection }}</div>
			<div class="shell-pill shell-pill--version">v{{ appVersion }}</div>
		</div>
		<Horizontal :menuList="state.menuList" v-if="isLayoutTransverse" />
		<User />
	</div>
</template>

<script setup lang="ts" name="layoutBreadcrumbIndex">
import { defineAsyncComponent, computed, reactive, onMounted, onUnmounted } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { useRoutesList } from '/@/stores/routesList';
import { useThemeConfig } from '/@/stores/themeConfig';
import { useAppInfo } from '/@/stores/appInfo';
import mittBus from '/@/utils/mitt';

const Breadcrumb = defineAsyncComponent(() => import('/@/layout/navBars/topBar/breadcrumb.vue'));
const User = defineAsyncComponent(() => import('/@/layout/navBars/topBar/user.vue'));
const Logo = defineAsyncComponent(() => import('/@/layout/logo/index.vue'));
const Horizontal = defineAsyncComponent(() => import('/@/layout/navMenu/horizontal.vue'));

const stores = useRoutesList();
const storesThemeConfig = useThemeConfig();
const storesAppInfo = useAppInfo();
const { themeConfig } = storeToRefs(storesThemeConfig);
const { routesList } = storeToRefs(stores);
const route = useRoute();
const { t } = useI18n();
const state = reactive({
	menuList: [] as RouteItems,
});

const setIsShowLogo = computed(() => {
	let { isShowLogo, layout } = themeConfig.value;
	return (isShowLogo && layout === 'classic') || (isShowLogo && layout === 'transverse');
});

const isLayoutTransverse = computed(() => {
	let { layout, isClassicSplitMenu } = themeConfig.value;
	return layout === 'transverse' || (isClassicSplitMenu && layout === 'classic');
});

const currentSection = computed(() => {
	const tagsViewName = route.meta.tagsViewName as string | undefined;
	if (tagsViewName) return tagsViewName;
	const title = route.meta.title as string | undefined;
	if (!title) return 'Workspace';
	return title.startsWith('message.') ? t(title) : title;
});

const appVersion = computed(() => storesAppInfo.appInfo.version || __NEXT_VERSION__);

const setFilterRoutes = () => {
	let { layout, isClassicSplitMenu } = themeConfig.value;
	if (layout === 'classic' && isClassicSplitMenu) {
		state.menuList = delClassicChildren(filterRoutesFun(routesList.value));
		const resData = setSendClassicChildren(route.path);
		mittBus.emit('setSendClassicChildren', resData);
	} else {
		state.menuList = filterRoutesFun(routesList.value);
	}
};

const delClassicChildren = <T extends ChilType>(arr: T[]): T[] => {
	arr.map((v: T) => {
		if (v.children) delete v.children;
	});
	return arr;
};

const filterRoutesFun = <T extends RouteItem>(arr: T[]): T[] => {
	return arr
		.filter((item: T) => !item.meta?.isHide)
		.map((item: T) => {
			item = Object.assign({}, item);
			if (item.children) item.children = filterRoutesFun(item.children);
			return item;
		});
};

const setSendClassicChildren = (path: string) => {
	const currentPathSplit = path.split('/');
	let currentData: MittMenu = { children: [] };
	filterRoutesFun(routesList.value).map((v: RouteItem, k: number) => {
		if (v.path === `/${currentPathSplit[1]}`) {
			v['k'] = k;
			currentData['item'] = { ...v };
			currentData['children'] = [{ ...v }];
			if (v.children) currentData['children'] = v.children;
		}
	});
	return currentData;
};

onMounted(() => {
	setFilterRoutes();
	mittBus.on('getBreadcrumbIndexSetFilterRoutes', () => {
		setFilterRoutes();
	});
});

onUnmounted(() => {
	mittBus.off('getBreadcrumbIndexSetFilterRoutes', () => {});
});
</script>

<style scoped lang="scss">
.layout-navbars-breadcrumb-index {
	min-height: 64px;
	display: flex;
	align-items: center;
	gap: 18px;
	padding: 10px 16px;
	border-radius: 24px;
	border: 1px solid rgba(255, 255, 255, 0.66);
	background: linear-gradient(180deg, rgba(255, 255, 255, 0.95), rgba(248, 250, 252, 0.88));
	box-shadow: 0 18px 36px rgba(15, 23, 42, 0.08);
	backdrop-filter: blur(20px);
}

.layout-navbars-breadcrumb-index__left {
	flex: 1;
	min-width: 0;
	display: flex;
	align-items: center;
	gap: 12px;
}

.layout-navbars-breadcrumb-index__status {
	display: flex;
	align-items: center;
	gap: 10px;
}

.shell-pill {
	display: inline-flex;
	align-items: center;
	height: 34px;
	padding: 0 14px;
	border-radius: 999px;
	border: 1px solid rgba(148, 163, 184, 0.16);
	background: rgba(248, 250, 252, 0.88);
	color: #475569;
	font-size: 12px;
	font-weight: 600;
	letter-spacing: 0.02em;
	white-space: nowrap;
}

.shell-pill--brand {
	background: linear-gradient(135deg, rgba(14, 165, 233, 0.12), rgba(16, 185, 129, 0.12));
	color: #0f766e;
}

.shell-pill--version {
	color: #0f172a;
}

@media (max-width: 1200px) {
	.layout-navbars-breadcrumb-index__status {
		display: none;
	}
}

@media (max-width: 767px) {
	.layout-navbars-breadcrumb-index {
		padding: 8px 12px;
		border-radius: 20px;
	}
}
</style>
