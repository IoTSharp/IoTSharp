<template>
	<div class="iotsharp-shell">
		<el-container class="layout-container iotsharp-shell__container">
			<el-drawer
				v-if="isMobile"
				v-model="mobileOpen"
				direction="ltr"
				size="min(280px, 88vw)"
				class="iotsharp-navigation-drawer"
				:title="t('message.consoleLayout.navigation')"
			>
				<Aside drawer @navigate="mobileOpen = false" />
			</el-drawer>
			<Aside v-else />
			<el-container class="iotsharp-shell__body" direction="vertical">
				<Header class="iotsharp-shell__header" />
				<el-container class="flex-center iotsharp-shell__workspace">
					<Main />
				</el-container>
			</el-container>
			<el-backtop target=".layout-main .el-scrollbar__wrap"></el-backtop>
		</el-container>
	</div>
</template>

<script setup lang="ts" name="layoutDefaults">
import { provide, ref, watch } from 'vue';
import { useMediaQuery } from '@vueuse/core';
import { useI18n } from 'vue-i18n';
import { useRoute } from 'vue-router';
import Aside from '/@/layout/component/aside.vue';
import Header from '/@/layout/component/header.vue';
import Main from '/@/layout/component/main.vue';
import { consoleNavigationKey } from '/@/layout/consoleNavigation';

const isMobile = useMediaQuery('(max-width: 1000px)');
const mobileOpen = ref(false);
const route = useRoute();
const { t } = useI18n();

provide(consoleNavigationKey, {
	isMobile,
	open: mobileOpen,
	toggle: () => { mobileOpen.value = !mobileOpen.value; },
});

watch([isMobile, () => route.fullPath], () => { mobileOpen.value = false; });
</script>

<style scoped lang="scss">
.iotsharp-shell {
	min-height: 100%;
	background: var(--iotsharp-page);
}

.iotsharp-shell__container {
	height: 100vh;
	height: 100dvh;
	min-height: 0;
	background: var(--iotsharp-page);
}

.iotsharp-shell__header {
	flex: 0 0 auto;
}

.iotsharp-shell__body {
	flex: 1 1 auto;
	min-width: 0;
	min-height: 0;
	background: var(--iotsharp-page);
}

.iotsharp-shell__workspace {
	flex: 1 1 auto;
	min-width: 0;
	min-height: 0;
	padding: 0;
	background: var(--iotsharp-page);
}

:deep(.iotsharp-navigation-drawer.el-drawer) {
	border-radius: 0;
	background: var(--iotsharp-nav-gradient);
}

:deep(.iotsharp-navigation-drawer .el-drawer__header) {
	background: transparent;
	border-color: var(--iotsharp-nav-divider);
	color: var(--iotsharp-nav-ink);
}

:deep(.iotsharp-navigation-drawer .el-drawer__title),
:deep(.iotsharp-navigation-drawer .el-drawer__close-btn) {
	color: var(--iotsharp-nav-ink);
}

:deep(.iotsharp-navigation-drawer .el-drawer__body) {
	min-height: 0;
	overflow: hidden;
}
</style>
