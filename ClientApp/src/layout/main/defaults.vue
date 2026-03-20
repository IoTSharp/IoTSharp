<template>
	<div class="iotsharp-shell">
		<div class="iotsharp-shell__mesh"></div>
		<el-container class="layout-container iotsharp-shell__container">
			<Aside />
			<el-container class="flex-center iotsharp-shell__workspace" :class="{ 'layout-backtop': !isFixedHeader }">
				<Header v-if="isFixedHeader" />
				<el-scrollbar ref="layoutDefaultsScrollbarRef" class="iotsharp-shell__scrollbar" :class="{ 'layout-backtop': isFixedHeader }">
					<Header v-if="!isFixedHeader" />
					<Main />
				</el-scrollbar>
			</el-container>
			<el-backtop target=".layout-backtop .el-scrollbar__wrap"></el-backtop>
		</el-container>
	</div>
</template>

<script lang="ts">
import { computed, getCurrentInstance, watch, defineComponent } from 'vue';
import { useRoute } from 'vue-router';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import Aside from '/@/layout/component/aside.vue';
import Header from '/@/layout/component/header.vue';
import Main from '/@/layout/component/main.vue';

export default defineComponent({
	name: 'layoutDefaults',
	components: { Aside, Header, Main },
	setup() {
		const { proxy } = <any>getCurrentInstance();
		const route = useRoute();
		const storesThemeConfig = useThemeConfig();
		const { themeConfig } = storeToRefs(storesThemeConfig);
		const isFixedHeader = computed(() => {
			return themeConfig.value.isFixedHeader;
		});
		// 监听路由的变化
		watch(
			() => route.path,
			() => {
				proxy.$refs.layoutDefaultsScrollbarRef.wrapRef.scrollTop = 0;
			}
		);
		return {
			isFixedHeader,
		};
	},
});
</script>

<style scoped lang="scss">
.iotsharp-shell {
	position: relative;
	min-height: 100%;
	background: var(--iotsharp-shell-bg);
}

.iotsharp-shell__mesh {
	position: absolute;
	inset: 0;
	background-image:
		linear-gradient(rgba(148, 163, 184, 0.05) 1px, transparent 1px),
		linear-gradient(90deg, rgba(148, 163, 184, 0.05) 1px, transparent 1px);
	background-size: 40px 40px;
	mask-image: radial-gradient(circle at center, #000 30%, transparent 88%);
	pointer-events: none;
}

.iotsharp-shell__container {
	position: relative;
	z-index: 1;
	min-height: 100vh;
}

.iotsharp-shell__workspace {
	padding: 0 18px 18px 12px;
}

.iotsharp-shell__scrollbar {
	flex: 1;
	border-radius: var(--iotsharp-radius-shell);
	background: rgba(255, 255, 255, 0.44);
	backdrop-filter: blur(18px);
	box-shadow: var(--iotsharp-shadow-shell);
}

@media (max-width: 767px) {
	.iotsharp-shell__workspace {
		padding: 0 10px 10px;
	}

	.iotsharp-shell__scrollbar {
		border-radius: 22px;
	}
}
</style>
