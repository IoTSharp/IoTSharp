<template>
	<div class="layout-logo" v-if="setShowLogo" @click="onThemeConfigChange">
		<AppLogo class="layout-logo__brand" />
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
	props: {
		alwaysExpanded: {
			type: Boolean,
			default: false,
		},
		disableToggle: {
			type: Boolean,
			default: false,
		},
	},
	setup(props) {
		const storesThemeConfig = useThemeConfig();
		const { themeConfig } = storeToRefs(storesThemeConfig);

		const setShowLogo = computed(() => {
			if (props.alwaysExpanded) return true;
			const { isCollapse, layout } = themeConfig.value;
			return !isCollapse || layout === 'classic' || document.body.clientWidth < 1000;
		});

		const onThemeConfigChange = () => {
			if (props.disableToggle) return false;
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
	width: auto;
	max-width: 100%;
	height: 64px;
	display: flex;
	align-items: center;
	justify-content: flex-start;
	padding: 0 6px 0 0;
	border-radius: 0;
	border: none;
	background: transparent;
	box-shadow: none;
	cursor: pointer;
	animation: logoAnimation 0.3s ease-in-out;

	&__brand {
		min-width: 0;
	}
}

.layout-logo-size {
	width: auto;
	max-width: 100%;
	height: 64px;
	display: flex;
	align-items: center;
	justify-content: center;
	cursor: pointer;
	animation: logoAnimation 0.3s ease-in-out;
	border-radius: 0;
	border: none;
	background: transparent;
	box-shadow: none;

	&__brand {
		transform: scale(0.96);
	}
}
</style>
