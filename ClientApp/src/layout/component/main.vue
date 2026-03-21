<template>
	<el-main class="layout-main iotsharp-main">
		<el-scrollbar ref="layoutScrollbarRef" class="iotsharp-main__scrollbar" :class="{ 'layout-scrollbar': !isImmersiveRoute }">
			<div class="iotsharp-main__body" :class="{ 'iotsharp-main__body--immersive': isImmersiveRoute }">
				<Breadcrumb v-if="!isImmersiveRoute" class="iotsharp-main__breadcrumb" />
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
import LayoutParentView from '/@/layout/routerView/parent.vue';
import Footer from '/@/layout/footer/index.vue';

export default defineComponent({
	name: 'layoutMain',
	components: { Breadcrumb, LayoutParentView, Footer },
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
			...toRefs(state),
		};
	},
});
</script>

<style scoped lang="scss">
.iotsharp-main {
	min-width: 0;
	min-height: 0;
	padding: 0;
	background: transparent;
}

.iotsharp-main__scrollbar {
	height: 100%;
	min-height: 0;
}

.iotsharp-main__body {
	min-width: 0;
	min-height: 100%;
	padding: 18px 20px 28px;
	background: transparent;
}

.iotsharp-main__body--immersive {
	padding: 0;
}

.iotsharp-main__breadcrumb {
	margin-bottom: 14px;
}

.iotsharp-main__content {
	min-width: 0;
}

@media (max-width: 767px) {
	.iotsharp-main__body {
		padding: 12px 12px 20px;
	}
}
</style>
