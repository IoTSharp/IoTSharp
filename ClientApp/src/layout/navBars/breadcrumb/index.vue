<template>
	<div class="layout-navbars-breadcrumb-index">
		<div class="layout-navbars-breadcrumb-index__left">
			<Logo v-if="setIsShowLogo" />
			<button v-else-if="showCompactBrand" class="layout-navbars-breadcrumb-index__brand" @click="onThemeConfigChange">
				<AppLogo hideText class="layout-navbars-breadcrumb-index__brand-mark" />
				<div class="layout-navbars-breadcrumb-index__brand-copy">
					<span class="layout-navbars-breadcrumb-index__brand-title">{{ themeConfig.globalTitle }}</span>
					<small class="layout-navbars-breadcrumb-index__brand-subtitle">Industrial AI Console</small>
				</div>
			</button>
			<Breadcrumb />
		</div>
		<Horizontal :menuList="menuList" v-if="isLayoutTransverse" />
		<User />
	</div>
</template>

<script lang="ts">
import { computed, reactive, toRefs, onMounted, onUnmounted, getCurrentInstance, defineComponent } from 'vue';
import { useRoute } from 'vue-router';
import { storeToRefs } from 'pinia';
import { useRoutesList } from '/@/stores/routesList';
import { useThemeConfig } from '/@/stores/themeConfig';
import AppLogo from '/@/components/AppLogo.vue';
import Breadcrumb from '/@/layout/navBars/breadcrumb/breadcrumb.vue';
import User from '/@/layout/navBars/breadcrumb/user.vue';
import Logo from '/@/layout/logo/index.vue';
import Horizontal from '/@/layout/navMenu/horizontal.vue';

interface IndexState {
	menuList: object[];
}

export default defineComponent({
	name: 'layoutBreadcrumbIndex',
	components: { AppLogo, Breadcrumb, User, Logo, Horizontal },
	setup() {
		const { proxy } = <any>getCurrentInstance();
		const stores = useRoutesList();
		const storesThemeConfig = useThemeConfig();
		const { themeConfig } = storeToRefs(storesThemeConfig);
		const { routesList } = storeToRefs(stores);
		const route = useRoute();
		const state = reactive<IndexState>({
			menuList: [],
		});

		const setIsShowLogo = computed(() => {
			const { isShowLogo, layout } = themeConfig.value;
			return (isShowLogo && layout === 'classic') || (isShowLogo && layout === 'transverse');
		});

		// Keep a compact IoTSharp brand visible in the top bar for defaults and columns layouts.
		const showCompactBrand = computed(() => {
			const { layout } = themeConfig.value;
			return layout === 'defaults' || layout === 'columns';
		});

		const isLayoutTransverse = computed(() => {
			const { layout, isClassicSplitMenu } = themeConfig.value;
			return layout === 'transverse' || (isClassicSplitMenu && layout === 'classic');
		});

		const onThemeConfigChange = () => {
			const { layout } = themeConfig.value;
			if (layout !== 'defaults' && layout !== 'columns') return;
			themeConfig.value.isCollapse = !themeConfig.value.isCollapse;
		};

		const setFilterRoutes = () => {
			const { layout, isClassicSplitMenu } = themeConfig.value;
			if (layout === 'classic' && isClassicSplitMenu) {
				state.menuList = delClassicChildren(filterRoutesFun(routesList.value));
				const resData = setSendClassicChildren(route.path);
				proxy.mittBus.emit('setSendClassicChildren', resData);
			} else {
				state.menuList = filterRoutesFun(routesList.value);
			}
		};

		const delClassicChildren = (arr: Array<object>) => {
			arr.map((v: any) => {
				if (v.children) delete v.children;
			});
			return arr;
		};

		const filterRoutesFun = (arr: Array<string>) => {
			return arr
				.filter((item: any) => !item.meta.isHide)
				.map((item: any) => {
					item = Object.assign({}, item);
					if (item.children) item.children = filterRoutesFun(item.children);
					return item;
				});
		};

		const setSendClassicChildren = (path: string) => {
			const currentPathSplit = path.split('/');
			const currentData: any = {};
			filterRoutesFun(routesList.value).map((v, k) => {
				if (v.path === `/${currentPathSplit[1]}`) {
					v['k'] = k;
					currentData['item'] = [{ ...v }];
					currentData['children'] = [{ ...v }];
					if (v.children) currentData['children'] = v.children;
				}
			});
			return currentData;
		};

		onMounted(() => {
			setFilterRoutes();
			proxy.mittBus.on('getBreadcrumbIndexSetFilterRoutes', () => {
				setFilterRoutes();
			});
		});

		onUnmounted(() => {
			proxy.mittBus.off('getBreadcrumbIndexSetFilterRoutes', () => {});
		});

		return {
			setIsShowLogo,
			showCompactBrand,
			isLayoutTransverse,
			onThemeConfigChange,
			themeConfig,
			...toRefs(state),
		};
	},
});
</script>

<style scoped lang="scss">
.layout-navbars-breadcrumb-index {
	height: 50px;
	display: flex;
	align-items: center;
	gap: 12px;
	background: var(--next-bg-topBar);
	border-bottom: 1px solid var(--next-border-color-light);
}

.layout-navbars-breadcrumb-index__left {
	flex: 1;
	min-width: 0;
	display: flex;
	align-items: center;
	gap: 12px;
}

.layout-navbars-breadcrumb-index__brand {
	display: inline-flex;
	align-items: center;
	gap: 10px;
	min-width: 0;
	height: 42px;
	padding: 0 14px 0 12px;
	border: 1px solid rgba(59, 130, 246, 0.14);
	border-radius: 16px;
	background: linear-gradient(180deg, rgba(255, 255, 255, 0.94), rgba(248, 250, 252, 0.86));
	box-shadow: 0 10px 24px rgba(15, 23, 42, 0.08);
	cursor: pointer;
}

.layout-navbars-breadcrumb-index__brand-mark {
	flex-shrink: 0;
}

.layout-navbars-breadcrumb-index__brand-copy {
	display: flex;
	flex-direction: column;
	min-width: 0;
	text-align: left;
}

.layout-navbars-breadcrumb-index__brand-title {
	color: var(--iotsharp-brand-ink, #0f172a);
	font-size: 14px;
	font-weight: 700;
	line-height: 1.1;
	white-space: nowrap;
}

.layout-navbars-breadcrumb-index__brand-subtitle {
	margin-top: 2px;
	color: var(--iotsharp-brand-slate, #64748b);
	font-size: 10px;
	line-height: 1;
	letter-spacing: 0.1em;
	text-transform: uppercase;
	white-space: nowrap;
}

@media (max-width: 767px) {
	.layout-navbars-breadcrumb-index__brand {
		padding: 0 12px 0 10px;
		border-radius: 14px;
	}

	.layout-navbars-breadcrumb-index__brand-subtitle {
		display: none;
	}
}
</style>
