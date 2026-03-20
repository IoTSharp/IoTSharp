<template>
	<div class="layout-logo" v-if="setShowLogo" @click="onThemeConfigChange">
		<AppLogo class="layout-logo__brand" />
		<div class="layout-logo__copy">
			<span class="layout-logo__title">{{ themeConfig.globalTitle }}</span>
			<span class="layout-logo__subtitle">Industrial AI Console</span>
		</div>
	</div>
	<div class="layout-logo-size" v-else @click="onThemeConfigChange">
		<AppLogo hideText class="layout-logo-size__brand" />
	</div>
</template>

<script lang="ts">
import { computed, defineComponent } from 'vue';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import AppLogo from '/@/components/AppLogo.vue';

export default defineComponent({
	name: 'layoutLogo',
	components: { AppLogo },
	setup() {
		const storesThemeConfig = useThemeConfig();
		const { themeConfig } = storeToRefs(storesThemeConfig);
		// 设置 logo 的显示。classic 经典布局默认显示 logo
		const setShowLogo = computed(() => {
			let { isCollapse, layout } = themeConfig.value;
			return !isCollapse || layout === 'classic' || document.body.clientWidth < 1000;
		});
		// logo 点击实现菜单展开/收起
		const onThemeConfigChange = () => {
			if (themeConfig.value.layout === 'transverse') return false;
			themeConfig.value.isCollapse = !themeConfig.value.isCollapse;
		};
		return {
			setShowLogo,
			themeConfig,
			onThemeConfigChange,
		};
	},
});
</script>

<style scoped lang="scss">
.layout-logo {
	width: 220px;
	height: 72px;
	display: flex;
	align-items: center;
	justify-content: flex-start;
	padding: 0 18px;
	gap: 12px;
	border-radius: 24px;
	border: 1px solid rgba(148, 163, 184, 0.12);
	background: linear-gradient(180deg, rgba(255, 255, 255, 0.96), rgba(248, 250, 252, 0.9));
	box-shadow: 0 18px 36px rgba(15, 23, 42, 0.08);
	cursor: pointer;
	animation: logoAnimation 0.3s ease-in-out;
	overflow: hidden;
	&__brand {
		flex-shrink: 0;
	}
	&__copy {
		display: flex;
		flex-direction: column;
		min-width: 0;
	}
	&__title {
		color: #0f172a;
		font-size: 15px;
		font-weight: 700;
		line-height: 1.1;
		white-space: nowrap;
	}
	&__subtitle {
		margin-top: 4px;
		color: #64748b;
		font-size: 11px;
		letter-spacing: 0.12em;
		text-transform: uppercase;
	}
}
.layout-logo-size {
	width: 100%;
	height: 72px;
	display: flex;
	align-items: center;
	justify-content: center;
	cursor: pointer;
	animation: logoAnimation 0.3s ease-in-out;
	border-radius: 20px;
	border: 1px solid rgba(148, 163, 184, 0.12);
	background: linear-gradient(180deg, rgba(255, 255, 255, 0.96), rgba(248, 250, 252, 0.9));
	box-shadow: 0 18px 36px rgba(15, 23, 42, 0.08);
	&__brand {
		transform: scale(0.96);
	}
}
</style>
