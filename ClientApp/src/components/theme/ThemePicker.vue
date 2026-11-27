<template>
	<el-popover v-model:visible="isOpen" placement="bottom-end" :width="408" trigger="click" popper-class="iotsharp-theme-popover">
		<template #reference>
			<button class="theme-picker__trigger" type="button" :aria-expanded="isOpen" :aria-label="labels.title" :title="labels.title">
				<span class="theme-picker__swatch" :style="swatchStyle(currentTheme)" aria-hidden="true"></span>
				<span>{{ labels.title }}</span>
			</button>
		</template>

		<div class="theme-picker__panel">
			<div class="theme-picker__heading">
				<strong>{{ labels.heading }}</strong>
			</div>
			<div class="theme-picker__grid theme-picker__grid--regions">
				<button
					v-for="theme in regionThemes"
					:key="theme.id"
					type="button"
					class="theme-picker__option"
					:class="{ 'is-selected': theme.id === currentTheme.id }"
					:aria-pressed="theme.id === currentTheme.id"
					@click="selectTheme(theme.id)"
				>
					<span class="theme-picker__swatch theme-picker__swatch--large" :style="swatchStyle(theme)" aria-hidden="true"></span>
					<span class="theme-picker__copy">
						<strong>{{ theme.name }}</strong>
						<small>{{ theme.description }}</small>
					</span>
					<el-icon v-if="theme.id === currentTheme.id" class="theme-picker__check"><Check /></el-icon>
				</button>
			</div>
			<label v-if="showTexture" class="theme-picker__texture">
				<input v-model="textureVisible" type="checkbox" @change="saveUiTexture(textureVisible)" />
				<span>{{ labels.texture }}</span>
			</label>
		</div>
	</el-popover>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { Check } from '@element-plus/icons-vue';
import { useI18n } from 'vue-i18n';
import { getUiTexture, getUiTheme, saveUiTexture, saveUiTheme, uiThemes, type UiTheme } from '/@/theme/themes';

defineProps({ showTexture: Boolean });
const isOpen = ref(false);
const textureVisible = ref(getUiTexture());
const currentTheme = ref(getUiTheme());
const regionThemes = uiThemes;
const { locale } = useI18n();
const labels = computed(() => {
	if (locale.value === 'en') return { title: 'Theme', heading: 'Regional palettes', texture: 'Show workspace texture' };
	if (locale.value === 'zh-tw') return { title: '主題', heading: '分區配色', texture: '顯示工作區紋理' };
	return { title: '主题', heading: '分区配色', texture: '显示工作区纹理' };
});

const swatchStyle = (theme: UiTheme) => ({
	'--swatch-dark': theme.regions?.navigation ?? theme.dark,
	'--swatch-light': theme.regions?.panel ?? theme.light,
	'--swatch-signal': theme.regions?.profile[0] ?? theme.signal,
	'--swatch-contrast': theme.regions?.inbox[0] ?? theme.contrast,
});

const selectTheme = (id: string) => {
	currentTheme.value = saveUiTheme(id);
	isOpen.value = false;
};
</script>

<style scoped lang="scss">
.theme-picker__trigger {
	display: inline-flex;
	height: 36px;
	align-items: center;
	gap: 8px;
	padding: 0 10px;
	border: 1px solid rgba(var(--iotsharp-accent-rgb), 0.2);
	border-radius: 6px;
	background: var(--iotsharp-surface-soft);
	color: var(--iotsharp-ink);
	font: inherit;
	font-size: 13px;
	font-weight: 600;
	cursor: pointer;
	transition: background-color 160ms ease, border-color 160ms ease, box-shadow 160ms ease;
}

.theme-picker__trigger:hover,
.theme-picker__trigger:focus-visible {
	border-color: var(--iotsharp-accent);
	background: var(--iotsharp-surface);
	box-shadow: 0 6px 16px rgba(22, 36, 32, 0.08);
}

.theme-picker__swatch {
	display: inline-block;
	width: 18px;
	height: 18px;
	flex: 0 0 auto;
	border: 1px solid rgba(15, 23, 42, 0.12);
	border-radius: 50%;
	background: linear-gradient(135deg, var(--swatch-dark) 0 40%, var(--swatch-light) 40% 70%, var(--swatch-signal) 70% 86%, var(--swatch-contrast) 86% 100%);
	box-shadow: inset 0 0 0 2px rgba(255, 255, 255, 0.76);
}

.theme-picker__panel {
	color: var(--iotsharp-ink);
}

.theme-picker__heading {
	display: flex;
	align-items: baseline;
	justify-content: space-between;
	gap: 12px;
	padding: 2px 4px 10px;
	border-bottom: 1px solid var(--iotsharp-border);
}

.theme-picker__heading strong {
	font-size: 14px;
}

.theme-picker__heading span {
	color: var(--iotsharp-text-muted);
	font-size: 11px;
}

.theme-picker__grid {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 4px;
	padding-top: 6px;
}

.theme-picker__grid--regions {
	grid-template-columns: 1fr;
}

.theme-picker__texture {
	display: flex;
	align-items: center;
	gap: 9px;
	min-height: 38px;
	margin-top: 6px;
	padding-inline: 8px;
	border-top: 1px solid var(--iotsharp-border);
	font-size: 13px;
	cursor: pointer;

	input {
		width: 15px;
		height: 15px;
		accent-color: var(--iotsharp-accent);
	}
}

.theme-picker__option {
	display: grid;
	grid-template-columns: 28px minmax(0, 1fr) 16px;
	align-items: center;
	gap: 8px;
	min-height: 54px;
	padding: 6px 8px;
	border: 0;
	border-radius: 6px;
	background: transparent;
	color: inherit;
	font: inherit;
	text-align: left;
	cursor: pointer;
}

.theme-picker__option:hover,
.theme-picker__option:focus-visible,
.theme-picker__option.is-selected {
	background: var(--iotsharp-selection);
}

.theme-picker__swatch--large {
	width: 26px;
	height: 26px;
}

.theme-picker__copy {
	display: grid;
	min-width: 0;
	gap: 2px;
}

.theme-picker__copy strong {
	font-size: 12px;
	font-weight: 650;
}

.theme-picker__copy small {
	overflow: hidden;
	color: var(--iotsharp-text-muted);
	font-size: 10px;
	text-overflow: ellipsis;
	white-space: nowrap;
}

.theme-picker__check {
	color: var(--iotsharp-accent);
}

@media (max-width: 600px) {
	.theme-picker__trigger > span:last-child {
		display: none;
	}

	.theme-picker__trigger {
		width: 36px;
		justify-content: center;
		padding: 0;
	}
}
</style>
