<template>
	<el-header class="layout-header iotsharp-header" :height="setHeaderHeight" v-show="!isTagsViewCurrenFull">
		<NavBarsIndex />
	</el-header>
</template>

<script lang="ts">
import { computed, defineComponent } from 'vue';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import { useTagsViewRoutes } from '/@/stores/tagsViewRoutes';
import NavBarsIndex from '/@/layout/navBars/index.vue';

export default defineComponent({
	name: 'layoutHeader',
	components: { NavBarsIndex },
	setup() {
		const storesTagsViewRoutes = useTagsViewRoutes();
		const storesThemeConfig = useThemeConfig();
		const { themeConfig } = storeToRefs(storesThemeConfig);
		const { isTagsViewCurrenFull } = storeToRefs(storesTagsViewRoutes);

		const setHeaderHeight = computed(() => {
			const { isTagsview, layout } = themeConfig.value;
			if (isTagsview && !['classic', 'defaults'].includes(layout)) return '104px';
			return '64px';
		});

		return {
			setHeaderHeight,
			isTagsViewCurrenFull,
		};
	},
});
</script>

<style scoped lang="scss">
.iotsharp-header {
	padding: 0;
	background: transparent;
}
</style>
