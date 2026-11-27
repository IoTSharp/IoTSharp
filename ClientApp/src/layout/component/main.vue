<template>
	<el-main class="layout-main iotsharp-main" :class="{ 'iotsharp-main--immersive': isImmersiveRoute }">
		<el-scrollbar ref="layoutScrollbarRef" class="iotsharp-main__scrollbar" :class="{ 'layout-scrollbar': !isImmersiveRoute }">
			<div class="iotsharp-main__body" :class="{ 'iotsharp-main__body--immersive': isImmersiveRoute }">
				<TagsView v-if="showPageTabs && !isImmersiveRoute" variant="page" class="iotsharp-main__tabs" />
				<Breadcrumb v-else-if="!isImmersiveRoute" class="iotsharp-main__breadcrumb" />
				<div class="iotsharp-main__content">
					<LayoutParentView />
				</div>
			</div>
			<Footer v-if="themeConfig.isFooter" />
		</el-scrollbar>
	</el-main>
</template>

<script lang="ts">
import { computed, defineComponent, getCurrentInstance, onMounted, reactive, toRefs, watch } from 'vue';
import { useRoute } from 'vue-router';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import { NextLoading } from '/@/utils/loading';
import Breadcrumb from '/@/layout/navBars/breadcrumb/breadcrumb.vue';
import TagsView from '/@/layout/navBars/tagsView/tagsView.vue';
import LayoutParentView from '/@/layout/routerView/parent.vue';
import Footer from '/@/layout/footer/index.vue';

export default defineComponent({
	name: 'layoutMain',
	components: { Breadcrumb, TagsView, LayoutParentView, Footer },
	setup() {
		const { proxy } = <any>getCurrentInstance();
		const storesThemeConfig = useThemeConfig();
		const { themeConfig } = storeToRefs(storesThemeConfig);
		const route = useRoute();
		const state = reactive({
			currentRouteMeta: {} as any,
		});

		const isImmersiveRoute = computed(() => {
			return !!(state.currentRouteMeta.isLink && state.currentRouteMeta.isIframe);
		});

		const showPageTabs = computed(() => {
			return themeConfig.value.layout === 'defaults' && themeConfig.value.isTagsview;
		});

		const initGetMeta = () => {
			state.currentRouteMeta = route.meta;
		};

		onMounted(async () => {
			initGetMeta();
			NextLoading.done();
		});

		watch(
			() => route.path,
			() => {
				state.currentRouteMeta = route.meta;
				if (proxy.$refs.layoutScrollbarRef?.wrapRef) proxy.$refs.layoutScrollbarRef.wrapRef.scrollTop = 0;
				proxy.$refs.layoutScrollbarRef?.update();
			}
		);

		watch(
			themeConfig,
			() => {
				proxy.$refs?.layoutScrollbarRef?.update();
			},
			{
				deep: true,
			}
		);

		return {
			themeConfig,
			isImmersiveRoute,
			showPageTabs,
			...toRefs(state),
		};
	},
});
</script>

<style scoped lang="scss">
.iotsharp-main {
	position: relative;
	isolation: isolate;
	min-width: 0;
	min-height: 0;
	padding: 0;
	background: transparent;
}

.iotsharp-main:not(.iotsharp-main--immersive)::before {
	position: absolute;
	z-index: -1;
	inset: 0 0 auto auto;
	width: min(70%, 850px);
	height: min(360px, 55vh);
	background: var(--iotsharp-canvas-texture, none);
	mask-image: linear-gradient(to right, transparent, #000 52%), linear-gradient(to bottom, #000 18%, transparent);
	mask-composite: intersect;
	pointer-events: none;
	content: '';
}

:global(html[data-ui-texture='off'] .iotsharp-main::before) {
	display: none;
}

.iotsharp-main__scrollbar {
	height: 100%;
	min-height: 0;
}

.iotsharp-main__scrollbar > :deep(.el-scrollbar__wrap > .el-scrollbar__view) {
	display: flex;
	flex-direction: column;
	min-block-size: 100%;
}

.iotsharp-main__body {
	display: flex;
	flex: 1 0 auto;
	flex-direction: column;
	width: 100%;
	max-width: var(--iotsharp-content-max-width);
	min-width: 0;
	margin-inline: auto;
	padding: 12px var(--iotsharp-page-gutter) 16px;
	background: transparent;
}

.iotsharp-main__body--immersive {
	max-width: none;
	padding: 0;
}

.iotsharp-main__tabs,
.iotsharp-main__breadcrumb {
	flex: 0 0 auto;
	margin-bottom: 8px;
}

.iotsharp-main__content,
.iotsharp-main__content > :deep(.iotsharp-route-view) {
	display: flex;
	flex: 1 0 auto;
	flex-direction: column;
	min-width: 0;
}

.iotsharp-main__content > :deep(.iotsharp-route-view > .iotsharp-route-page) {
	flex: 1 0 auto;
	min-inline-size: 0;
}

@media (min-width: 768px) and (min-height: 660px) {
	.iotsharp-main:not(.iotsharp-main--immersive):has(.iotsharp-route-view > .console-list-page) {
		.iotsharp-main__scrollbar > :deep(.el-scrollbar__wrap > .el-scrollbar__view) {
			block-size: 100%;
		}

		.iotsharp-main__body,
		.iotsharp-main__content,
		.iotsharp-main__content > :deep(.iotsharp-route-view),
		.iotsharp-main__content > :deep(.iotsharp-route-view > .console-list-page) {
			flex: 1 1 0;
			min-block-size: 0;
		}
	}
}

@media (max-width: 767px) {
	.iotsharp-main:not(.iotsharp-main--immersive)::before {
		width: 100%;
		height: 200px;
		opacity: 0.75;
	}

	.iotsharp-main__body {
		padding: 8px 10px 12px;
	}

	.iotsharp-main__body--immersive { padding: 0; }
}

@media (forced-colors: active) {
	.iotsharp-main::before { display: none; }
}
</style>
