<template>
	<component :is="themeConfig.layout" />
</template>

<script lang="ts">
import { onBeforeMount, onUnmounted, getCurrentInstance, defineComponent, defineAsyncComponent } from 'vue';
import { storeToRefs } from 'pinia';
import { useThemeConfig, LOCKED_CONSOLE_LAYOUT } from '/@/stores/themeConfig';
import { Local } from '/@/utils/storage';

export default defineComponent({
	name: 'layout',
	components: {
		defaults: defineAsyncComponent(() => import('/@/layout/main/defaults.vue')),
		classic: defineAsyncComponent(() => import('/@/layout/main/classic.vue')),
		transverse: defineAsyncComponent(() => import('/@/layout/main/transverse.vue')),
		columns: defineAsyncComponent(() => import('/@/layout/main/columns.vue')),
	},
	setup() {
		const { proxy } = <any>getCurrentInstance();
		const storesThemeConfig = useThemeConfig();
		const { themeConfig } = storeToRefs(storesThemeConfig);
		// 窗口大小改变时(适配移动端)
		const onLayoutResize = () => {
			const clientWidth = document.body.clientWidth;
			Local.set('oldLayout', LOCKED_CONSOLE_LAYOUT);
			themeConfig.value.layout = LOCKED_CONSOLE_LAYOUT;
			if (clientWidth < 1000) themeConfig.value.isCollapse = false;
			proxy.mittBus.emit('layoutMobileResize', {
				layout: LOCKED_CONSOLE_LAYOUT,
				clientWidth,
			});
		};
		// 页面加载前
		onBeforeMount(() => {
			onLayoutResize();
			window.addEventListener('resize', onLayoutResize);
		});
		// 页面卸载时
		onUnmounted(() => {
			window.removeEventListener('resize', onLayoutResize);
		});
		return {
			themeConfig,
		};
	},
});
</script>
