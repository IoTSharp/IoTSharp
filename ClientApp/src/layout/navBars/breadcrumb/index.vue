<template>
	<div class="layout-shell-topbar">
		<div class="layout-shell-topbar__brand">
			<Logo v-if="setIsShowLogo" alwaysExpanded disableToggle class="layout-shell-topbar__logo" />
			<button v-if="!isLayoutTransverse" type="button" class="layout-shell-topbar__toggle" @click="onToggleCollapse">
				<SvgIcon :name="themeConfig.isCollapse ? 'ele-Expand' : 'ele-Fold'" :size="16" />
			</button>
			<div class="layout-shell-topbar__context">
				<span class="layout-shell-topbar__label">IoT Platform Console</span>
				<strong class="layout-shell-topbar__title">{{ currentSection }}</strong>
			</div>
		</div>
		<Horizontal v-if="isLayoutTransverse" :menuList="menuList" class="layout-shell-topbar__nav" />
		<User />
	</div>
</template>

<script lang="ts">
import { computed, defineComponent, onMounted, onUnmounted, reactive, toRefs } from 'vue';
import { useRoute } from 'vue-router';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { Local } from '/@/utils/storage';
import mittBus from '/@/utils/mitt';
import { useRoutesList } from '/@/stores/routesList';
import { useThemeConfig } from '/@/stores/themeConfig';
import User from '/@/layout/navBars/breadcrumb/user.vue';
import Logo from '/@/layout/logo/index.vue';
import Horizontal from '/@/layout/navMenu/horizontal.vue';

export default defineComponent({
	name: 'layoutBreadcrumbIndex',
	components: { User, Logo, Horizontal },
	setup() {
		const stores = useRoutesList();
		const storesThemeConfig = useThemeConfig();
		const { themeConfig } = storeToRefs(storesThemeConfig);
		const { routesList } = storeToRefs(stores);
		const route = useRoute();
		const { t } = useI18n();
		const state = reactive({
			menuList: [] as any[],
		});

		const setIsShowLogo = computed(() => {
			const { isShowLogo, layout } = themeConfig.value;
			return isShowLogo && ['classic', 'transverse', 'defaults'].includes(layout);
		});

		const isLayoutTransverse = computed(() => {
			const { layout, isClassicSplitMenu } = themeConfig.value;
			return layout === 'transverse' || (layout === 'classic' && isClassicSplitMenu);
		});

		const currentSection = computed(() => {
			const tagsViewName = route.meta.tagsViewName as string | undefined;
			if (tagsViewName) return tagsViewName;

			const title = route.meta.title as string | undefined;
			if (!title) return 'Workspace';

			return title.startsWith('message.') ? t(title) : title;
		});

		const onToggleCollapse = () => {
			themeConfig.value.isCollapse = !themeConfig.value.isCollapse;
			Local.remove('themeConfig');
			Local.set('themeConfig', themeConfig.value);
		};

		const filterRoutesFun = (arr: any[]) => {
			return arr
				.filter((item: any) => !item.meta?.isHide)
				.map((item: any) => {
					item = Object.assign({}, item);
					if (item.children) item.children = filterRoutesFun(item.children);
					return item;
				});
		};

		const delClassicChildren = (arr: any[]) => {
			arr.forEach((item: any) => {
				if (item.children) delete item.children;
			});
			return arr;
		};

		const setSendClassicChildren = (path: string) => {
			const currentPathSplit = path.split('/');
			const currentData: any = { children: [] };

			filterRoutesFun(routesList.value).forEach((item: any) => {
				if (item.path === `/${currentPathSplit[1]}`) {
					currentData.item = { ...item };
					currentData.children = item.children ? item.children : [{ ...item }];
				}
			});

			return currentData;
		};

		const setFilterRoutes = () => {
			const { layout, isClassicSplitMenu } = themeConfig.value;
			if (layout === 'classic' && isClassicSplitMenu) {
				state.menuList = delClassicChildren(filterRoutesFun(routesList.value));
				mittBus.emit('setSendClassicChildren', setSendClassicChildren(route.path));
				return;
			}
			state.menuList = filterRoutesFun(routesList.value);
		};

		const onRefreshRoutes = () => {
			setFilterRoutes();
		};

		onMounted(() => {
			setFilterRoutes();
			mittBus.on('getBreadcrumbIndexSetFilterRoutes', onRefreshRoutes);
		});

		onUnmounted(() => {
			mittBus.off('getBreadcrumbIndexSetFilterRoutes', onRefreshRoutes);
		});

		return {
			themeConfig,
			setIsShowLogo,
			isLayoutTransverse,
			currentSection,
			onToggleCollapse,
			...toRefs(state),
		};
	},
});
</script>

<style scoped lang="scss">
.layout-shell-topbar {
	height: 64px;
	display: flex;
	align-items: center;
	gap: 20px;
	padding: 0 22px 0 18px;
	background: #ffffff;
	border-bottom: 1px solid #e5e6eb;
}

.layout-shell-topbar__brand {
	display: flex;
	align-items: center;
	gap: 14px;
	min-width: 0;
	flex-shrink: 0;
}

.layout-shell-topbar__logo {
	flex-shrink: 0;
}

.layout-shell-topbar__toggle {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 34px;
	height: 34px;
	border: 1px solid #e5e6eb;
	border-radius: 10px;
	background: #ffffff;
	color: #4e5969;
	cursor: pointer;
	transition:
		border-color 0.2s ease,
		color 0.2s ease,
		background-color 0.2s ease;

	&:hover {
		border-color: #bedaff;
		background: #f7fbff;
		color: #165dff;
	}
}

.layout-shell-topbar__context {
	display: flex;
	flex-direction: column;
	min-width: 0;
}

.layout-shell-topbar__label {
	color: #86909c;
	font-size: 11px;
	font-weight: 600;
	letter-spacing: 0.08em;
	text-transform: uppercase;
	white-space: nowrap;
}

.layout-shell-topbar__title {
	color: #1d2129;
	font-size: 15px;
	font-weight: 600;
	line-height: 1.2;
	white-space: nowrap;
}

.layout-shell-topbar__nav {
	flex: 1;
	min-width: 0;
}

@media (max-width: 1100px) {
	.layout-shell-topbar__context {
		display: none;
	}
}

@media (max-width: 767px) {
	.layout-shell-topbar {
		height: 60px;
		padding: 0 12px;
		gap: 12px;
	}
}
</style>
