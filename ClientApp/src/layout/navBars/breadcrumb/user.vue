<template>
	<div class="layout-navbars-breadcrumb-user">
		<button type="button" class="topbar-tool" title="搜索菜单" @click="onSearchClick">
			<el-icon><SearchIcon /></el-icon>
		</button>

		<el-dropdown trigger="click" @command="onLanguageChange">
			<button type="button" class="topbar-tool" title="切换语言">
				<el-icon><Connection /></el-icon>
			</button>
			<template #dropdown>
				<el-dropdown-menu>
					<el-dropdown-item command="zh-cn" :disabled="disabledI18n === 'zh-cn'">简体中文</el-dropdown-item>
					<el-dropdown-item command="en" :disabled="disabledI18n === 'en'">English</el-dropdown-item>
					<el-dropdown-item command="zh-tw" :disabled="disabledI18n === 'zh-tw'">繁體中文</el-dropdown-item>
				</el-dropdown-menu>
			</template>
		</el-dropdown>

		<button type="button" class="topbar-tool topbar-tool--desktop" :title="isScreenfull ? '退出全屏' : '进入全屏'" @click="onScreenfullClick">
			<el-icon><FullScreen /></el-icon>
		</button>

		<ThemePicker />

		<el-dropdown trigger="click" @command="onHandleCommandClick">
			<button type="button" class="account-trigger" aria-label="账号菜单">
				<img v-if="userInfos.photo" :src="userInfos.photo" class="account-trigger__photo" alt="" />
				<span v-else class="account-trigger__initial">{{ userInitial }}</span>
				<span class="account-trigger__name">{{ displayName }}</span>
				<el-icon><ArrowDown /></el-icon>
			</button>
			<template #dropdown>
				<el-dropdown-menu>
					<el-dropdown-item command="/dashboard">控制台首页</el-dropdown-item>
					<el-dropdown-item command="/profile">个人中心</el-dropdown-item>
					<el-dropdown-item divided command="logOut">退出登录</el-dropdown-item>
				</el-dropdown-menu>
			</template>
		</el-dropdown>

		<Search ref="searchRef" />
	</div>
</template>

<script lang="ts">
import { computed, defineComponent, getCurrentInstance, onMounted, onUnmounted, reactive, ref, toRefs } from 'vue';
import { useRouter } from 'vue-router';
import { ArrowDown, Connection, FullScreen, Search as SearchIcon } from '@element-plus/icons-vue';
import { ElMessage, ElMessageBox } from 'element-plus';
import screenfull from 'screenfull';
import { useI18n } from 'vue-i18n';
import { storeToRefs } from 'pinia';
import { useUserInfo } from '/@/stores/userInfo';
import { useThemeConfig } from '/@/stores/themeConfig';
import other from '/@/utils/other';
import { Local, Session } from '/@/utils/storage';
import Search from '/@/layout/navBars/breadcrumb/search.vue';
import ThemePicker from '/@/components/theme/ThemePicker.vue';

export default defineComponent({
	name: 'layoutBreadcrumbUser',
	components: { ArrowDown, Connection, FullScreen, SearchIcon, Search, ThemePicker },
	setup() {
		const { messages } = useI18n();
		const { proxy } = <any>getCurrentInstance();
		const router = useRouter();
		const stores = useUserInfo();
		const storesThemeConfig = useThemeConfig();
		const { userInfos } = storeToRefs(stores);
		const { themeConfig } = storeToRefs(storesThemeConfig);
		const searchRef = ref();
		const state = reactive({ isScreenfull: false, disabledI18n: 'zh-cn' });

		const displayName = computed(() => userInfos.value.userName || 'IoTSharp 用户');
		const userInitial = computed(() => displayName.value.trim().slice(0, 1).toUpperCase());

		const onScreenfullChange = () => {
			state.isScreenfull = screenfull.isFullscreen;
		};

		const onScreenfullClick = () => {
			if (!screenfull.isEnabled) {
				ElMessage.warning('当前浏览器不支持全屏');
				return;
			}
			void screenfull.toggle();
		};

		const onHandleCommandClick = async (path: string) => {
			if (path !== 'logOut') {
				await router.push(path);
				return;
			}

			try {
				await ElMessageBox.confirm('退出后需要重新验证身份才能进入控制台。', '退出登录', {
					confirmButtonText: '退出',
					cancelButtonText: '取消',
					type: 'warning',
				});
				Session.clear();
				window.location.reload();
			} catch {
				// 用户取消退出时保持当前工作上下文。
			}
		};

		const onSearchClick = () => searchRef.value?.openSearch?.();

		const setI18nConfig = (locale: string) => {
			proxy.mittBus.emit('getI18nConfig', messages.value[locale]);
		};

		const syncLanguage = () => {
			state.disabledI18n = themeConfig.value.globalI18n || 'zh-cn';
			setI18nConfig(state.disabledI18n);
		};

		const onLanguageChange = (language: string) => {
			themeConfig.value.globalI18n = language;
			Local.set('themeConfig', themeConfig.value);
			proxy.$i18n.locale = language;
			syncLanguage();
			other.useTitle();
		};

		onMounted(() => {
			syncLanguage();
			if (screenfull.isEnabled) screenfull.on('change', onScreenfullChange);
		});

		onUnmounted(() => {
			if (screenfull.isEnabled) screenfull.off('change', onScreenfullChange);
		});

		return {
			userInfos,
			displayName,
			userInitial,
			onHandleCommandClick,
			onScreenfullClick,
			onSearchClick,
			onLanguageChange,
			searchRef,
			...toRefs(state),
		};
	},
});
</script>

<style scoped lang="scss">
.layout-navbars-breadcrumb-user {
	display: flex;
	flex: 1;
	align-items: center;
	justify-content: flex-end;
	gap: 7px;
	min-width: 0;
}

.topbar-tool,
.account-trigger {
	display: inline-flex;
	height: 36px;
	align-items: center;
	justify-content: center;
	border: 1px solid rgba(var(--iotsharp-accent-rgb), 0.18);
	border-radius: 6px;
	background: rgba(255, 255, 255, 0.68);
	color: var(--iotsharp-text-soft);
	font: inherit;
	cursor: pointer;
	transition: background-color 160ms ease, border-color 160ms ease, color 160ms ease, box-shadow 160ms ease;
}

.topbar-tool {
	width: 36px;
}

.topbar-tool:hover,
.account-trigger:hover,
.topbar-tool:focus-visible,
.account-trigger:focus-visible {
	border-color: var(--iotsharp-accent);
	background: #ffffff;
	color: var(--iotsharp-accent);
	box-shadow: 0 6px 16px rgba(22, 36, 32, 0.08);
}

.account-trigger {
	gap: 8px;
	max-width: 190px;
	padding: 0 9px 0 5px;
	color: var(--iotsharp-ink);
}

.account-trigger__photo,
.account-trigger__initial {
	display: inline-flex;
	width: 26px;
	height: 26px;
	flex: 0 0 auto;
	align-items: center;
	justify-content: center;
	border-radius: 50%;
	background: var(--iotsharp-accent-gradient);
	color: #ffffff;
	font-size: 11px;
	font-weight: 700;
	object-fit: cover;
}

.account-trigger__name {
	overflow: hidden;
	font-size: 12px;
	font-weight: 600;
	text-overflow: ellipsis;
	white-space: nowrap;
}

@media (max-width: 760px) {
	.layout-navbars-breadcrumb-user {
		gap: 5px;
	}

	.topbar-tool--desktop,
	.account-trigger__name {
		display: none;
	}

	.account-trigger {
		width: 36px;
		padding: 0;
	}

	.account-trigger > .el-icon {
		display: none;
	}
}

@media (max-width: 470px) {
	.topbar-tool:nth-of-type(2) {
		display: none;
	}
}
</style>
