<template>
	<div class="layout-navbars-breadcrumb-index">
		<div class="layout-navbars-breadcrumb-index__left">
			<Logo v-if="setIsShowLogo" alwaysExpanded disableToggle class="layout-navbars-breadcrumb-index__logo" />
			<div class="layout-navbars-breadcrumb-index__divider"></div>
			<Breadcrumb />
		</div>
		<Horizontal v-if="isLayoutTransverse" :menuList="menuList" />
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
			return isShowLogo && ['classic', 'transverse', 'defaults'].includes(layout);
		});

		const isLayoutTransverse = computed(() => {
			const { layout, isClassicSplitMenu } = themeConfig.value;
			return layout === 'transverse' || (layout === 'classic' && isClassicSplitMenu);
		});

		const filterRoutesFun = (arr: Array<string>) => {
			return arr
				.filter((item: any) => !item.meta.isHide)
				.map((item: any) => {
					item = Object.assign({}, item);
					if (item.children) item.children = filterRoutesFun(item.children);
					return item;
				});
		};

		const delClassicChildren = (arr: Array<object>) => {
			arr.map((v: any) => {
				if (v.children) delete v.children;
			});
			return arr;
		};

		const setSendClassicChildren = (path: string) => {
			const currentPathSplit = path.split('/');
			const currentData: any = {};
			filterRoutesFun(routesList.value).map((v: any, k: number) => {
				if (v.path === `/${currentPathSplit[1]}`) {
					v.k = k;
					currentData.item = [{ ...v }];
					currentData.children = v.children ? v.children : [{ ...v }];
				}
			});
			return currentData;
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
			isLayoutTransverse,
			...toRefs(state),
		};
	},
});
</script>

<style scoped lang="scss">
.layout-navbars-breadcrumb-index {
	height: 72px;
	display: flex;
	align-items: center;
	gap: 16px;
	padding: 0 24px;
	background: rgba(255, 255, 255, 0.95);
	border-bottom: 1px solid rgba(224, 232, 242, 0.9);
	box-shadow: 0 10px 30px rgba(15, 23, 42, 0.04);
}

.layout-navbars-breadcrumb-index__left {
	flex: 1;
	min-width: 0;
	display: flex;
	align-items: center;
	gap: 18px;
}

.layout-navbars-breadcrumb-index__logo {
	flex-shrink: 0;
}

.layout-navbars-breadcrumb-index__divider {
	width: 1px;
	height: 34px;
	background: linear-gradient(180deg, rgba(191, 219, 254, 0), rgba(191, 219, 254, 0.9), rgba(191, 219, 254, 0));
	flex-shrink: 0;
}

@media (max-width: 767px) {
	.layout-navbars-breadcrumb-index {
		height: 68px;
		padding: 0 14px;
	}

	.layout-navbars-breadcrumb-index__divider {
		display: none;
	}
}
</style>
