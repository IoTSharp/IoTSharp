<template>
	<div class="layout-shell-topbar">
		<div class="layout-shell-topbar__brand">
			<Logo v-if="setIsShowLogo" alwaysExpanded disableToggle class="layout-shell-topbar__logo" />
			<button v-if="!isLayoutTransverse" type="button" class="layout-shell-topbar__toggle" :title="navigationLabel" :aria-label="navigationLabel" :aria-expanded="isNavigationExpanded" @click="onToggleCollapse">
				<SvgIcon :name="isMobileNavigation || !isNavigationExpanded ? 'ele-Expand' : 'ele-Fold'" :size="16" />
			</button>
			<div class="layout-shell-topbar__context">
				<strong class="layout-shell-topbar__title">{{ currentSection }}</strong>
			</div>
		</div>
		<Horizontal v-if="isLayoutTransverse" :menuList="menuList" class="layout-shell-topbar__nav" />
		<User />
	</div>
</template>

<script lang="ts">
import { computed, defineComponent, inject, onMounted, onUnmounted, reactive, toRefs } from 'vue';
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
import { consoleNavigationKey } from '/@/layout/consoleNavigation';

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
		const navigation = inject(consoleNavigationKey, undefined);
		const isMobileNavigation = computed(() => navigation?.isMobile.value ?? false);
		const isNavigationExpanded = computed(() => isMobileNavigation.value ? navigation!.open.value : !themeConfig.value.isCollapse);
		const navigationLabel = computed(() => t(isNavigationExpanded.value ? 'message.consoleLayout.closeNavigation' : 'message.consoleLayout.openNavigation'));
		const state = reactive({
			menuList: [] as any[],
		});

		const setIsShowLogo = computed(() => {
			const { isShowLogo, layout } = themeConfig.value;
			return isShowLogo && ['classic', 'transverse'].includes(layout);
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
			if (navigation?.isMobile.value) {
				navigation.toggle();
				return;
			}
			themeConfig.value.isCollapse = !themeConfig.value.isCollapse;
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
			isMobileNavigation,
			isNavigationExpanded,
			navigationLabel,
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
	gap: 14px;
	padding: 0 var(--iotsharp-page-gutter);
	background: var(--iotsharp-topbar-gradient);
	border-bottom: 1px solid var(--iotsharp-border);
}

.layout-shell-topbar__brand {
	display: flex;
	align-items: center;
	gap: 14px;
	min-width: 0;
	flex: 0 1 auto;
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
	flex: 0 0 auto;
	border: 1px solid rgba(var(--iotsharp-accent-rgb), 0.18);
	border-radius: 6px;
	background: var(--iotsharp-surface-soft);
	color: var(--iotsharp-text-soft);
	cursor: pointer;
	transition:
		border-color 0.2s ease,
		color 0.2s ease,
		background-color 0.2s ease;

	&:hover {
		border-color: var(--iotsharp-accent);
		background: var(--iotsharp-surface);
		color: var(--iotsharp-accent);
	}
}

.layout-shell-topbar__context {
	display: flex;
	align-items: baseline;
	gap: 10px;
	min-width: 0;
}

.layout-shell-topbar__title {
	overflow: hidden;
	color: var(--iotsharp-ink);
	font-size: 16px;
	font-weight: 600;
	line-height: 1.2;
	text-overflow: ellipsis;
	white-space: nowrap;
}

.layout-shell-topbar__nav {
	flex: 1;
	min-width: 0;
}

@media (max-width: 767px) {
	.layout-shell-topbar {
		padding: 0 12px;
		gap: 8px;
	}

	.layout-shell-topbar__brand {
		gap: 8px;
	}

	.layout-shell-topbar__title {
		font-size: 14px;
	}
}
</style>
