<template>
	<div class="layout-navbars-breadcrumb-user" :style="{ flex: layoutUserFlexNum }">
		<el-dropdown trigger="click" @command="onComponentSizeChange">
			<div class="layout-navbars-breadcrumb-user__icon">
				<i class="iconfont icon-ziti" :title="$t('message.user.title0')"></i>
			</div>
			<template #dropdown>
				<el-dropdown-menu>
					<el-dropdown-item command="large" :disabled="disabledSize === 'large'">{{ $t('message.user.dropdownLarge') }}</el-dropdown-item>
					<el-dropdown-item command="default" :disabled="disabledSize === 'default'">{{ $t('message.user.dropdownDefault') }}</el-dropdown-item>
					<el-dropdown-item command="small" :disabled="disabledSize === 'small'">{{ $t('message.user.dropdownSmall') }}</el-dropdown-item>
				</el-dropdown-menu>
			</template>
		</el-dropdown>

		<el-dropdown trigger="click" @command="onLanguageChange">
			<div class="layout-navbars-breadcrumb-user__icon">
				<i class="iconfont" :class="disabledI18n === 'en' ? 'icon-fuhao-yingwen' : 'icon-fuhao-zhongwen'" :title="$t('message.user.title1')"></i>
			</div>
			<template #dropdown>
				<el-dropdown-menu>
					<el-dropdown-item command="zh-cn" :disabled="disabledI18n === 'zh-cn'">简体中文</el-dropdown-item>
					<el-dropdown-item command="en" :disabled="disabledI18n === 'en'">English</el-dropdown-item>
					<el-dropdown-item command="zh-tw" :disabled="disabledI18n === 'zh-tw'">繁體中文</el-dropdown-item>
				</el-dropdown-menu>
			</template>
		</el-dropdown>

		<div class="layout-navbars-breadcrumb-user__icon" @click="onSearchClick">
			<el-icon :title="$t('message.user.title2')">
				<ele-Search />
			</el-icon>
		</div>

		<div class="layout-navbars-breadcrumb-user__icon" @click="onLayoutSetingClick">
			<i class="iconfont icon-skin" :title="$t('message.user.title3')"></i>
		</div>

		<div class="layout-navbars-breadcrumb-user__icon">
			<el-popover placement="bottom" trigger="click" transition="el-zoom-in-top" :width="300" :persistent="false">
				<template #reference>
					<el-badge :is-dot="true">
						<el-icon :title="$t('message.user.title4')">
							<ele-Bell />
						</el-icon>
					</el-badge>
				</template>
				<UserNews />
			</el-popover>
		</div>

		<div class="layout-navbars-breadcrumb-user__icon" @click="onScreenfullClick">
			<i
				class="iconfont"
				:title="isScreenfull ? $t('message.user.title6') : $t('message.user.title5')"
				:class="!isScreenfull ? 'icon-fullscreen' : 'icon-tuichuquanping'"
			></i>
		</div>

		<el-dropdown @command="onHandleCommandClick">
			<span class="layout-navbars-breadcrumb-user__link">
				<img :src="userInfos.photo" class="layout-navbars-breadcrumb-user__photo" />
				<span class="layout-navbars-breadcrumb-user__name">{{ userInfos.userName === '' ? 'common' : userInfos.userName }}</span>
				<el-icon class="el-icon--right">
					<ele-ArrowDown />
				</el-icon>
			</span>
			<template #dropdown>
				<el-dropdown-menu>
					<el-dropdown-item command="/home">{{ $t('message.user.dropdown1') }}</el-dropdown-item>
					<el-dropdown-item command="/profile">{{ $t('message.user.dropdown2') }}</el-dropdown-item>
					<el-dropdown-item command="iotsharp">{{ $t('message.user.dropdown6') }}</el-dropdown-item>
					<el-dropdown-item command="docs">{{ $t('message.user.dropdown3') }}</el-dropdown-item>
					<el-dropdown-item command="github">{{ $t('message.user.dropdown4') }}</el-dropdown-item>
					<el-dropdown-item divided command="logOut">{{ $t('message.user.dropdown5') }}</el-dropdown-item>
				</el-dropdown-menu>
			</template>
		</el-dropdown>

		<Search ref="searchRef" />
	</div>
</template>

<script lang="ts">
import { ref, getCurrentInstance, computed, reactive, toRefs, onMounted, defineComponent } from 'vue';
import { useRouter } from 'vue-router';
import { ElMessageBox, ElMessage } from 'element-plus';
import screenfull from 'screenfull';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { useUserInfo } from '/@/stores/userInfo';
import { useThemeConfig } from '/@/stores/themeConfig';
import other from '/@/utils/other';
import { Session, Local } from '/@/utils/storage';
import UserNews from '/@/layout/navBars/breadcrumb/userNews.vue';
import Search from '/@/layout/navBars/breadcrumb/search.vue';

export default defineComponent({
	name: 'layoutBreadcrumbUser',
	components: { UserNews, Search },
	setup() {
		const { t, messages } = useI18n();
		const { proxy } = <any>getCurrentInstance();
		const router = useRouter();
		const stores = useUserInfo();
		const storesThemeConfig = useThemeConfig();
		const { userInfos } = storeToRefs(stores);
		const { themeConfig } = storeToRefs(storesThemeConfig);
		const searchRef = ref();
		const state = reactive({
			isScreenfull: false,
			disabledI18n: 'zh-cn',
			disabledSize: 'large',
		});

		const layoutUserFlexNum = computed(() => {
			const { layout, isClassicSplitMenu } = themeConfig.value;
			const layoutArr: string[] = ['defaults', 'columns'];
			if (layoutArr.includes(layout) || (layout === 'classic' && !isClassicSplitMenu)) return '1';
			return '';
		});

		const onScreenfullClick = () => {
			if (!screenfull.isEnabled) {
				ElMessage.warning('当前环境暂不支持全屏');
				return;
			}
			screenfull.toggle();
			screenfull.on('change', () => {
				state.isScreenfull = screenfull.isFullscreen;
			});
		};

		const onLayoutSetingClick = () => {
			proxy.mittBus.emit('openSetingsDrawer');
		};

		const onHandleCommandClick = (path: string) => {
			if (path === 'logOut') {
				ElMessageBox({
					closeOnClickModal: false,
					closeOnPressEscape: false,
					title: t('message.user.logOutTitle'),
					message: t('message.user.logOutMessage'),
					showCancelButton: true,
					confirmButtonText: t('message.user.logOutConfirm'),
					cancelButtonText: t('message.user.logOutCancel'),
					buttonSize: 'default',
					beforeClose: (action, instance, done) => {
						if (action === 'confirm') {
							instance.confirmButtonLoading = true;
							instance.confirmButtonText = t('message.user.logOutExit');
							setTimeout(() => {
								done();
								setTimeout(() => {
									instance.confirmButtonLoading = false;
								}, 300);
							}, 700);
						} else {
							done();
						}
					},
				})
					.then(() => {
						Session.clear();
						window.location.reload();
					})
					.catch(() => {});
			} else if (path === 'iotsharp') {
				window.open('https://iotsharp.net/');
			} else if (path === 'github') {
				window.open('https://github.com/IoTSharp');
			} else if (path === 'docs') {
				window.open('http://docs.iotsharp.net/');
			} else {
				router.push(path);
			}
		};

		const onSearchClick = () => {
			searchRef.value.openSearch();
		};

		const onComponentSizeChange = (size: string) => {
			Local.remove('themeConfig');
			themeConfig.value.globalComponentSize = size;
			Local.set('themeConfig', themeConfig.value);
			initComponentSize();
			window.location.reload();
		};

		const setI18nConfig = async (locale: string) => {
			proxy.mittBus.emit('getI18nConfig', messages.value[locale]);
		};

		const initI18n = () => {
			switch (Local.get('themeConfig').globalI18n) {
				case 'zh-cn':
					state.disabledI18n = 'zh-cn';
					setI18nConfig('zh-cn');
					break;
				case 'en':
					state.disabledI18n = 'en';
					setI18nConfig('en');
					break;
				case 'zh-tw':
					state.disabledI18n = 'zh-tw';
					setI18nConfig('zh-tw');
					break;
			}
		};

		const onLanguageChange = (lang: string) => {
			Local.remove('themeConfig');
			themeConfig.value.globalI18n = lang;
			Local.set('themeConfig', themeConfig.value);
			proxy.$i18n.locale = lang;
			initI18n();
			other.useTitle();
		};

		const initComponentSize = () => {
			switch (Local.get('themeConfig').globalComponentSize) {
				case 'large':
					state.disabledSize = 'large';
					break;
				case 'default':
					state.disabledSize = 'default';
					break;
				case 'small':
					state.disabledSize = 'small';
					break;
			}
		};

		onMounted(() => {
			if (Local.get('themeConfig')) {
				initI18n();
				initComponentSize();
			}
		});

		return {
			userInfos,
			onLayoutSetingClick,
			onHandleCommandClick,
			onScreenfullClick,
			onSearchClick,
			onComponentSizeChange,
			onLanguageChange,
			searchRef,
			layoutUserFlexNum,
			...toRefs(state),
		};
	},
});
</script>

<style scoped lang="scss">
.layout-navbars-breadcrumb-user {
	display: flex;
	align-items: center;
	justify-content: flex-end;
	gap: 8px;
}

.layout-navbars-breadcrumb-user__icon {
	width: 38px;
	height: 38px;
	display: inline-flex;
	align-items: center;
	justify-content: center;
	border-radius: 12px;
	border: 1px solid transparent;
	background: #f7fbff;
	color: #5d728a;
	cursor: pointer;
	transition:
		background 0.2s ease,
		border-color 0.2s ease,
		color 0.2s ease;

	&:hover {
		background: #eef5ff;
		border-color: #d9e7ff;
		color: #2563eb;
	}
}

.layout-navbars-breadcrumb-user__link {
	height: 40px;
	display: inline-flex;
	align-items: center;
	gap: 8px;
	padding: 0 12px 0 6px;
	border-radius: 999px;
	border: 1px solid rgba(223, 231, 241, 0.96);
	background: #f9fbff;
	color: #3d5269;
	font-size: 13px;
	font-weight: 600;
	white-space: nowrap;
}

.layout-navbars-breadcrumb-user__photo {
	width: 28px;
	height: 28px;
	border-radius: 50%;
}

.layout-navbars-breadcrumb-user__name {
	max-width: 120px;
	overflow: hidden;
	text-overflow: ellipsis;
}

:deep(.el-dropdown) {
	color: inherit;
}

:deep(.el-badge) {
	height: 38px;
	display: inline-flex;
	align-items: center;
}

:deep(.el-badge__content.is-fixed) {
	top: 8px;
}

@media (max-width: 767px) {
	.layout-navbars-breadcrumb-user {
		gap: 6px;
	}

	.layout-navbars-breadcrumb-user__link {
		padding-right: 8px;
	}

	.layout-navbars-breadcrumb-user__name {
		display: none;
	}
}
</style>
