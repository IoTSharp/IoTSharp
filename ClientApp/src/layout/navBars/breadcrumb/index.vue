<template>
	<div class="layout-navbars-breadcrumb-index">
		<div class="layout-navbars-breadcrumb-index__left">
			<Logo v-if="setIsShowLogo" :alwaysExpanded="isDashboardHome" :disableToggle="isDashboardHome" />
			<Breadcrumb v-if="!isDashboardHome" />
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
import Breadcrumb from '/@/layout/navBars/breadcrumb/breadcrumb.vue';
import User from '/@/layout/navBars/breadcrumb/user.vue';
import Logo from '/@/layout/logo/index.vue';
import Horizontal from '/@/layout/navMenu/horizontal.vue';

interface IndexState {
	menuList: object[];
}

export default defineComponent({
	name: 'layoutBreadcrumbIndex',
	components: { Breadcrumb, User, Logo, Horizontal },
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
			return (isShowLogo && layout === 'classic') || (isShowLogo && layout === 'transverse') || (isShowLogo && layout === 'defaults');
		});

		const isDashboardHome = computed(() => {
			const { layout } = themeConfig.value;
			return layout === 'defaults' && route.path === '/dashboard';
		});

		const isLayoutTransverse = computed(() => {
			const { layout, isClassicSplitMenu } = themeConfig.value;
			return layout === 'transverse' || (isClassicSplitMenu && layout === 'classic');
		});

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
			isDashboardHome,
			isLayoutTransverse,
			themeConfig,
			...toRefs(state),
		};
	},
});
</script>

<style scoped lang="scss">
.layout-navbars-breadcrumb-index {
	height: 56px;
	display: flex;
	align-items: center;
	gap: 12px;
	padding: 0 18px;
	background: rgba(255, 255, 255, 0.98);
	border: 1px solid rgba(227, 234, 244, 0.96);
	border-bottom: none;
	border-radius: 24px 24px 0 0;
	box-shadow: 0 14px 36px rgba(15, 23, 42, 0.04);
}

.layout-navbars-breadcrumb-index__left {
	flex: 1;
	min-width: 0;
	display: flex;
	align-items: center;
	gap: 12px;
}

@media (max-width: 767px) {
	.layout-navbars-breadcrumb-index {
		padding: 0 12px;
	}
}
</style>
