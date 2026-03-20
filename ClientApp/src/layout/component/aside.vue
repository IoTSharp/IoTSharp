<template>
	<div class="h100" v-show="!isTagsViewCurrenFull">
		<el-aside class="layout-aside" :class="setCollapseStyle">
			<div class="layout-aside__panel">
				<div class="layout-aside__brand" v-if="setShowLogo">
					<Logo />
					<div class="layout-aside__meta" v-if="!themeConfig.isCollapse || clientWidth < 1000">
						<span>Mission Control</span>
						<small>{{ menuSummary }}</small>
					</div>
				</div>
				<el-scrollbar class="layout-aside__scroll flex-auto" ref="layoutAsideScrollbarRef" @mouseenter="onAsideEnterLeave(true)" @mouseleave="onAsideEnterLeave(false)">
					<Vertical :menuList="menuList" />
				</el-scrollbar>
				<div class="layout-aside__footer" v-if="clientWidth >= 1000 && !themeConfig.isCollapse">
					<span class="layout-aside__footer-dot"></span>
					<div>
						<div class="layout-aside__footer-title">{{ themeConfig.globalTitle }}</div>
						<small class="layout-aside__footer-copy">Always-on operations workspace</small>
					</div>
				</div>
			</div>
		</el-aside>
	</div>
</template>

<script lang="ts">
import { toRefs, reactive, computed, watch, getCurrentInstance, onBeforeMount, defineComponent } from 'vue';
import { storeToRefs } from 'pinia';
import pinia from '/@/stores/index';
import { useRoutesList } from '/@/stores/routesList';
import { useThemeConfig } from '/@/stores/themeConfig';
import { useTagsViewRoutes } from '/@/stores/tagsViewRoutes';
import Logo from '/@/layout/logo/index.vue';
import Vertical from '/@/layout/navMenu/vertical.vue';

export default defineComponent({
	name: 'layoutAside',
	components: { Logo, Vertical },
	setup() {
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
			let { layout, isShowLogo } = themeConfig.value;
			return (isShowLogo && layout === 'defaults') || (isShowLogo && layout === 'columns');
		});

		const menuSummary = computed(() => `${state.menuList.length} sections online`);

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

		onBeforeMount(() => {
			initMenuFixed(document.body.clientWidth);
			setFilterRoutes();
			proxy.mittBus.on('setSendColumnsChildren', (res: any) => {
				state.menuList = res.children;
			});
			proxy.mittBus.on('setSendClassicChildren', (res: any) => {
				let { layout, isClassicSplitMenu } = themeConfig.value;
				if (layout === 'classic' && isClassicSplitMenu) {
					state.menuList = [];
					state.menuList = res.children;
				}
			});
			proxy.mittBus.on('getBreadcrumbIndexSetFilterRoutes', () => {
				setFilterRoutes();
			});
			proxy.mittBus.on('layoutMobileResize', (res: any) => {
				initMenuFixed(res.clientWidth);
				closeLayoutAsideMobileMode();
			});
		});

		return {
			setCollapseStyle,
			setShowLogo,
			menuSummary,
			themeConfig,
			isTagsViewCurrenFull,
			onAsideEnterLeave,
			...toRefs(state),
		};
	},
});
</script>

<style lang="scss" scoped>
.layout-aside__panel {
	display: flex;
	flex-direction: column;
	height: 100%;
	padding: 18px 14px 8px;
	background:
		linear-gradient(180deg, var(--iotsharp-surface-panel), var(--iotsharp-surface-panel-alt)),
		radial-gradient(circle at top, rgba(14, 165, 233, 0.12), transparent 32%);
	border-right: 1px solid var(--iotsharp-border-soft);
}

.layout-aside__brand {
	display: flex;
	align-items: center;
	gap: 12px;
	margin-bottom: 16px;
}

.layout-aside__meta {
	display: flex;
	flex-direction: column;
	min-width: 0;

	span {
		color: var(--iotsharp-brand-ink);
		font-size: 13px;
		font-weight: 700;
	}

	small {
		margin-top: 4px;
		color: var(--iotsharp-brand-slate);
		font-size: 11px;
		letter-spacing: 0.08em;
		text-transform: uppercase;
	}
}

.layout-aside__scroll {
	flex: 1 1 auto;
	min-height: 0;
	border-radius: var(--iotsharp-radius-card);
	background: rgba(255, 255, 255, 0.54);
	box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.5);
}

.layout-aside__footer {
	display: flex;
	align-items: center;
	gap: 10px;
	margin-top: auto;
	margin-bottom: 2px;
	padding: 14px 12px;
	border-radius: 18px;
	background: rgba(15, 23, 42, 0.92);
	color: #e2e8f0;
}

.layout-aside__footer-dot {
	width: 10px;
	height: 10px;
	border-radius: 999px;
	background: linear-gradient(135deg, #38bdf8, #34d399);
	box-shadow: 0 0 0 6px rgba(52, 211, 153, 0.12);
}

.layout-aside__footer-title {
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.08em;
	text-transform: uppercase;
}

.layout-aside__footer-copy {
	display: block;
	margin-top: 3px;
	color: rgba(226, 232, 240, 0.7);
	font-size: 11px;
}
</style>
