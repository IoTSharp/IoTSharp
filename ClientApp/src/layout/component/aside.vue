<template>
	<div class="h100" v-show="drawer || !isTagsViewCurrenFull">
		<el-aside id="iotsharp-console-navigation" class="layout-aside" :class="setCollapseStyle">
			<div class="layout-aside__panel">
				<div class="layout-aside__brand" v-if="setShowLogo">
					<Logo original class="layout-aside__brand-logo" />
				</div>
				<el-scrollbar class="layout-aside__scroll flex-auto" ref="layoutAsideScrollbarRef" @mouseenter="onAsideEnterLeave(true)" @mouseleave="onAsideEnterLeave(false)">
					<Vertical :menuList="menuList" :expanded="drawer" @navigate="$emit('navigate')" />
				</el-scrollbar>
				<nav v-if="themeConfig.layout === 'defaults'" class="layout-aside__account" :class="{ 'layout-aside__account--compact': !drawer && themeConfig.isCollapse }" :aria-label="$t('message.personalCenter.navigation')">
					<RouterLink to="/profile" :title="$t('message.personalCenter.profile')" :aria-label="$t('message.personalCenter.profile')" @click="$emit('navigate')">
						<el-icon><User /></el-icon>
						<span>{{ $t('message.personalCenter.profile') }}</span>
					</RouterLink>
					<RouterLink to="/inbox" :title="$t('message.personalCenter.inbox')" :aria-label="$t('message.personalCenter.inbox')" @click="$emit('navigate')">
						<el-icon><Message /></el-icon>
						<span>{{ $t('message.personalCenter.inbox') }}</span>
					</RouterLink>
				</nav>
			</div>
		</el-aside>
	</div>
</template>

<script lang="ts">
import { toRefs, reactive, computed, watch, getCurrentInstance, onBeforeMount, onUnmounted, defineComponent } from 'vue';
import { Message, User } from '@element-plus/icons-vue';
import { storeToRefs } from 'pinia';
import pinia from '/@/stores/index';
import { useRoutesList } from '/@/stores/routesList';
import { useThemeConfig } from '/@/stores/themeConfig';
import { useTagsViewRoutes } from '/@/stores/tagsViewRoutes';
import Logo from '/@/layout/logo/index.vue';
import Vertical from '/@/layout/navMenu/vertical.vue';

export default defineComponent({
	name: 'layoutAside',
	components: { Logo, Vertical, Message, User },
	props: { drawer: Boolean },
	emits: ['navigate'],
	setup(props) {
		const { proxy } = <any>getCurrentInstance();
		const stores = useRoutesList();
		const storesThemeConfig = useThemeConfig();
		const storesTagsViewRoutes = useTagsViewRoutes();
		const { routesList } = storeToRefs(stores);
		const { themeConfig } = storeToRefs(storesThemeConfig);
		const { isTagsViewCurrenFull } = storeToRefs(storesTagsViewRoutes);
		const state = reactive({
			menuList: [] as any[],
			clientWidth: 0,
		});

		const setCollapseStyle = computed(() => {
			const { layout, isCollapse, menuBar } = themeConfig.value;
			if (props.drawer) return 'layout-aside--drawer';
			if (layout === 'defaults') return isCollapse ? 'layout-aside--compact' : 'layout-aside--expanded';
			const asideBrTheme = ['#FFFFFF', '#FFF', '#fff', '#ffffff'];
			const asideBrColor = asideBrTheme.includes(menuBar) ? 'layout-el-aside-br-color' : '';
			if (state.clientWidth <= 1000) {
				if (isCollapse) {
					document.body.setAttribute('class', 'el-popup-parent--hidden');
					const asideEle = document.querySelector('.layout-container') as HTMLElement;
					const modeDivs = document.createElement('div');
					modeDivs.setAttribute('class', 'layout-aside-mobile-mode');
					asideEle.appendChild(modeDivs);
					modeDivs.addEventListener('click', closeLayoutAsideMobileMode);
					return [asideBrColor, 'layout-aside-mobile', 'layout-aside-mobile-open'];
				}
				closeLayoutAsideMobileMode();
				return [asideBrColor, 'layout-aside-mobile', 'layout-aside-mobile-close'];
			}
			if (layout === 'columns') {
				return isCollapse ? [asideBrColor, 'layout-aside-pc-1'] : [asideBrColor, 'layout-aside-pc-220'];
			}
			return isCollapse ? [asideBrColor, 'layout-aside-pc-64'] : [asideBrColor, 'layout-aside-pc-220'];
		});

		const closeLayoutAsideMobileMode = () => {
			const el = document.querySelector('.layout-aside-mobile-mode');
			el?.setAttribute('style', 'animation: error-img-two 0.3s');
			setTimeout(() => {
				el?.parentNode?.removeChild(el);
			}, 300);
			const clientWidth = document.body.clientWidth;
			if (clientWidth < 1000) themeConfig.value.isCollapse = false;
			document.body.setAttribute('class', '');
		};

		const setShowLogo = computed(() => {
			const { layout, isShowLogo } = themeConfig.value;
			return !props.drawer && isShowLogo && ['defaults', 'columns'].includes(layout);
		});

		const setFilterRoutes = () => {
			if (themeConfig.value.layout === 'columns') return false;
			state.menuList = filterRoutesFun(routesList.value);
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

		const initMenuFixed = (clientWidth: number) => {
			state.clientWidth = clientWidth;
		};

		const onAsideEnterLeave = (bool: Boolean) => {
			let { layout } = themeConfig.value;
			if (layout !== 'columns') return false;
			if (!bool) proxy.mittBus.emit('restoreDefault');
			stores.setColumnsMenuHover(bool);
		};

		watch(themeConfig.value, (val) => {
			if (val.isShowLogoChange !== val.isShowLogo) {
				if (!proxy.$refs.layoutAsideScrollbarRef) return false;
				proxy.$refs.layoutAsideScrollbarRef.update();
			}
		});

		watch(
			pinia.state,
			(val) => {
				let { layout, isClassicSplitMenu } = val.themeConfig.themeConfig;
				if (layout === 'classic' && isClassicSplitMenu) return false;
				setFilterRoutes();
			},
			{
				deep: true,
			}
		);

		const setColumnsChildren = (res: any) => { state.menuList = res.children; };
		const setClassicChildren = (res: any) => {
			const { layout, isClassicSplitMenu } = themeConfig.value;
			if (layout === 'classic' && isClassicSplitMenu) state.menuList = res.children;
		};
		const resizeLegacyAside = (res: { clientWidth: number }) => {
			initMenuFixed(res.clientWidth);
			closeLayoutAsideMobileMode();
		};

		onBeforeMount(() => {
			initMenuFixed(document.body.clientWidth);
			setFilterRoutes();
			if (themeConfig.value.layout === 'defaults') return;
			proxy.mittBus.on('setSendColumnsChildren', setColumnsChildren);
			proxy.mittBus.on('setSendClassicChildren', setClassicChildren);
			proxy.mittBus.on('getBreadcrumbIndexSetFilterRoutes', setFilterRoutes);
			proxy.mittBus.on('layoutMobileResize', resizeLegacyAside);
		});

		onUnmounted(() => {
			proxy.mittBus.off('setSendColumnsChildren', setColumnsChildren);
			proxy.mittBus.off('setSendClassicChildren', setClassicChildren);
			proxy.mittBus.off('getBreadcrumbIndexSetFilterRoutes', setFilterRoutes);
			proxy.mittBus.off('layoutMobileResize', resizeLegacyAside);
		});

		return {
			setCollapseStyle,
			setShowLogo,
			themeConfig,
			isTagsViewCurrenFull,
			onAsideEnterLeave,
			...toRefs(state),
		};
	},
});
</script>

<style lang="scss" scoped>
.layout-aside--expanded {
	width: var(--iotsharp-nav-width);
	transition: width 180ms ease;
}

.layout-aside--compact {
	width: 64px;
	transition: width 180ms ease;
}

.layout-aside--drawer {
	width: 100%;
}

.layout-aside__panel {
	display: flex;
	flex-direction: column;
	height: 100%;
	padding: 0 10px 10px;
	background: var(--iotsharp-nav-gradient);
	border-inline-end: 1px solid var(--iotsharp-nav-divider);
}

.layout-aside__brand {
	display: flex;
	align-items: center;
	justify-content: flex-start;
	height: 64px;
	flex: 0 0 64px;
	padding: 0 10px;
	border-bottom: 1px solid var(--iotsharp-nav-divider);
	background: transparent;
	--app-logo-text: #ffffff;
	--app-logo-subtext: rgba(255, 255, 255, 0.82);
}

.layout-aside__brand-logo {
	width: 100%;
}

.layout-aside__scroll {
	flex: 1 1 auto;
	min-height: 0;
	padding: 10px 4px 12px;
	background: transparent;
}

.layout-aside--compact .layout-aside__panel {
	padding-inline: 7px;
}

.layout-aside--compact .layout-aside__brand {
	justify-content: center;
	padding-inline: 0;
}

.layout-aside--compact .layout-aside__scroll {
	padding-inline: 0;
}

.layout-aside__account {
	display: grid;
	gap: 3px;
	margin: 0 4px;
	padding-top: 10px;
	border-top: 1px solid var(--iotsharp-nav-divider);

	a {
		display: flex;
		align-items: center;
		gap: 12px;
		min-height: 40px;
		padding: 0 10px;
		border-radius: var(--iotsharp-radius-control);
		color: var(--iotsharp-nav-ink);
		text-decoration: none;
		white-space: nowrap;
	}

	a:hover,
	a:focus-visible { background: var(--iotsharp-nav-hover); }

	a:focus-visible {
		outline-color: var(--iotsharp-nav-indicator);
		outline-offset: -3px;
	}

	a.router-link-exact-active {
		background: var(--iotsharp-nav-active);
		box-shadow: inset 3px 0 var(--iotsharp-nav-indicator);
		font-weight: 650;
	}

	.el-icon {
		flex: 0 0 auto;
		font-size: 16px;
	}
}

.layout-aside__account--compact {
	a { justify-content: center; padding: 0; }
	span { display: none; }
}
</style>
