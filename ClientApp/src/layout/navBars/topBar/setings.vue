<template>
	<div class="layout-breadcrumb-seting">
		<el-drawer
			:title="$t('message.layout.configTitle')"
			v-model="getThemeConfig.isDrawer"
			direction="rtl"
			destroy-on-close
			size="320px"
			@close="onDrawerClose"
		>
			<el-scrollbar class="layout-breadcrumb-seting-bar">
				<el-alert
					class="layout-breadcrumb-seting__notice"
					title="Console shell layout and palette are locked for consistency. Safe preferences remain available."
					type="info"
					:closable="false"
				/>

				<el-divider content-position="left">{{ $t('message.layout.threeTitle') }}</el-divider>
				<div class="layout-breadcrumb-seting-bar-flex">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.threeIsCollapse') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isCollapse" size="small" @change="onThemeConfigChange"></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.threeIsUniqueOpened') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isUniqueOpened" size="small" @change="setLocalThemeConfig"></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.threeIsFixedHeader') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isFixedHeader" size="small" @change="onIsFixedHeaderChange"></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.threeIsLockScreen') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isLockScreen" size="small" @change="setLocalThemeConfig"></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt11">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.threeLockScreenTime') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-input-number
							v-model="getThemeConfig.lockScreenTime"
							controls-position="right"
							:min="1"
							:max="9999"
							size="default"
							style="width: 96px"
							@change="setLocalThemeConfig"
						/>
					</div>
				</div>

				<el-divider content-position="left">{{ $t('message.layout.fourTitle') }}</el-divider>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.fourIsShowLogo') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isShowLogo" size="small" @change="onIsShowLogoChange"></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.fourIsBreadcrumb') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isBreadcrumb" size="small" @change="onIsBreadcrumbChange"></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.fourIsBreadcrumbIcon') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isBreadcrumbIcon" size="small" @change="setLocalThemeConfig"></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.fourIsTagsview') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isTagsview" size="small" @change="setLocalThemeConfig"></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.fourIsTagsviewIcon') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isTagsviewIcon" size="small" @change="setLocalThemeConfig"></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.fourIsCacheTagsView') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isCacheTagsView" size="small" @change="setLocalThemeConfig"></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15" :class="{ 'is-disabled': state.isMobile }">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.fourIsSortableTagsView') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch
							v-model="getThemeConfig.isSortableTagsView"
							:disabled="state.isMobile"
							size="small"
							@change="onSortableTagsViewChange"
						></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.fourIsShareTagsView') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isShareTagsView" size="small" @change="onShareTagsViewChange"></el-switch>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.fourIsFooter') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-switch v-model="getThemeConfig.isFooter" size="small" @change="setLocalThemeConfig"></el-switch>
					</div>
				</div>

				<el-divider content-position="left">{{ $t('message.layout.fiveTitle') }}</el-divider>
				<div class="layout-breadcrumb-seting-bar-flex mt15">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.fiveTagsStyle') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-select v-model="getThemeConfig.tagsStyle" placeholder="Select" size="default" style="width: 96px" @change="setLocalThemeConfig">
							<el-option label="Style 1" value="tags-style-one"></el-option>
							<el-option label="Style 4" value="tags-style-four"></el-option>
							<el-option label="Style 5" value="tags-style-five"></el-option>
						</el-select>
					</div>
				</div>
				<div class="layout-breadcrumb-seting-bar-flex mt15 mb27">
					<div class="layout-breadcrumb-seting-bar-flex-label">{{ $t('message.layout.fiveAnimation') }}</div>
					<div class="layout-breadcrumb-seting-bar-flex-value">
						<el-select v-model="getThemeConfig.animation" placeholder="Select" size="default" style="width: 96px" @change="setLocalThemeConfig">
							<el-option label="slide-right" value="slide-right"></el-option>
							<el-option label="slide-left" value="slide-left"></el-option>
							<el-option label="opacitys" value="opacitys"></el-option>
						</el-select>
					</div>
				</div>

				<div class="copy-config">
					<el-alert :title="$t('message.layout.tipText')" type="warning" :closable="false"></el-alert>
					<el-button size="default" class="copy-config-btn" type="primary" ref="copyConfigBtnRef" @click="onCopyConfigClick">
						<el-icon class="mr5">
							<ele-CopyDocument />
						</el-icon>
						{{ $t('message.layout.copyText') }}
					</el-button>
					<el-button size="default" class="copy-config-btn-reset" type="info" @click="onResetConfigClick">
						<el-icon class="mr5">
							<ele-RefreshRight />
						</el-icon>
						{{ $t('message.layout.resetText') }}
					</el-button>
				</div>
			</el-scrollbar>
		</el-drawer>
	</div>
</template>

<script setup lang="ts" name="layoutBreadcrumbSeting">
import { computed, nextTick, onMounted, onUnmounted, reactive } from 'vue';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { useThemeConfig, LOCKED_CONSOLE_LAYOUT } from '/@/stores/themeConfig';
import { useChangeColor } from '/@/utils/theme';
import { Local } from '/@/utils/storage';
import commonFunction from '/@/utils/commonFunction';
import other from '/@/utils/other';
import mittBus from '/@/utils/mitt';

const { locale } = useI18n();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
const { copyText } = commonFunction();
const { getLightColor, getDarkColor } = useChangeColor();
const state = reactive({
	isMobile: false,
});

const getThemeConfig = computed(() => themeConfig.value);

const setLocalThemeConfig = () => {
	Local.remove('themeConfig');
	Local.set('themeConfig', getThemeConfig.value);
};

const setLocalThemeConfigStyle = () => {
	Local.set('themeConfigStyle', document.documentElement.style.cssText);
};

const syncPrimaryColor = () => {
	document.documentElement.style.setProperty('--el-color-primary-dark-2', `${getDarkColor(getThemeConfig.value.primary, 0.1)}`);
	document.documentElement.style.setProperty('--el-color-primary', getThemeConfig.value.primary);
	for (let i = 1; i <= 9; i++) {
		document.documentElement.style.setProperty(`--el-color-primary-light-${i}`, `${getLightColor(getThemeConfig.value.primary, i / 10)}`);
	}
};

const syncShellColors = () => {
	const colorKeys = ['topBar', 'topBarColor', 'menuBar', 'menuBarColor', 'menuBarActiveColor', 'columnsMenuBar', 'columnsMenuBarColor'] as const;
	colorKeys.forEach((key) => {
		document.documentElement.style.setProperty(`--next-bg-${key}`, getThemeConfig.value[key]);
	});
	document.documentElement.style.setProperty('--next-bg-menuBar-light-1', getLightColor(getThemeConfig.value.menuBar, 0.05));
	document.documentElement.style.setProperty('--el-menu-bg-color', getThemeConfig.value.menuBar);
};

const syncLockedShell = () => {
	getThemeConfig.value.layout = LOCKED_CONSOLE_LAYOUT;
	getThemeConfig.value.isClassicSplitMenu = false;
	syncPrimaryColor();
	syncShellColors();
	setLocalThemeConfig();
	setLocalThemeConfigStyle();
};

const onThemeConfigChange = () => {
	syncLockedShell();
};

const onIsFixedHeaderChange = () => {
	getThemeConfig.value.isFixedHeaderChange = !getThemeConfig.value.isFixedHeader;
	setLocalThemeConfig();
};

const onIsShowLogoChange = () => {
	getThemeConfig.value.isShowLogoChange = !getThemeConfig.value.isShowLogo;
	setLocalThemeConfig();
};

const onIsBreadcrumbChange = () => {
	setLocalThemeConfig();
};

const onSortableTagsViewChange = () => {
	mittBus.emit('openOrCloseSortable');
	setLocalThemeConfig();
};

const onShareTagsViewChange = () => {
	mittBus.emit('openShareTagsView');
	setLocalThemeConfig();
};

const onDrawerClose = () => {
	getThemeConfig.value.isFixedHeaderChange = false;
	getThemeConfig.value.isShowLogoChange = false;
	getThemeConfig.value.isDrawer = false;
	syncLockedShell();
};

const openDrawer = () => {
	getThemeConfig.value.isDrawer = true;
};

const onCopyConfigClick = () => {
	const copyThemeConfig = {
		...getThemeConfig.value,
		isDrawer: false,
	};
	copyText(JSON.stringify(copyThemeConfig)).then(() => {
		getThemeConfig.value.isDrawer = false;
	});
};

const onResetConfigClick = () => {
	Local.clear();
	window.location.reload();
	// @ts-ignore
	Local.set('version', __IOTSHARP_VERSION__);
};

onMounted(() => {
	nextTick(() => {
		state.isMobile = other.isMobile();
		syncLockedShell();
		mittBus.on('layoutMobileResize', (res: LayoutMobileResize) => {
			getThemeConfig.value.layout = LOCKED_CONSOLE_LAYOUT;
			getThemeConfig.value.isDrawer = false;
			if (res.clientWidth < 1000) getThemeConfig.value.isCollapse = false;
			state.isMobile = other.isMobile();
			syncLockedShell();
		});
		if (Local.get('themeConfig')) locale.value = Local.get('themeConfig').globalI18n;
	});
});

onUnmounted(() => {
	mittBus.off('layoutMobileResize', () => {});
});

defineExpose({
	openDrawer,
});
</script>

<style scoped lang="scss">
.layout-breadcrumb-seting-bar {
	height: calc(100vh - 50px);
	padding: 0 16px;

	:deep(.el-scrollbar__view) {
		overflow-x: hidden !important;
	}
}

.layout-breadcrumb-seting__notice {
	margin: 14px 0 18px;
}

.layout-breadcrumb-seting-bar-flex {
	display: flex;
	align-items: center;
	margin-bottom: 5px;
	gap: 12px;

	&.is-disabled {
		opacity: 0.5;
	}

	&-label {
		flex: 1;
		color: var(--el-text-color-primary);
	}

	&-value {
		flex-shrink: 0;
	}
}

.copy-config {
	padding-bottom: 24px;

	.copy-config-btn,
	.copy-config-btn-reset {
		width: 100%;
		margin-top: 12px;
	}
}
</style>
