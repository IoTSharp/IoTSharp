<template>
	<div class="auth-page">
		<div class="auth-shell">
			<AuthShowcase
				eyebrow="控制台入口"
				:title="pageTitle"
				:description="showcaseDescription"
				link-to="/"
				link-label="返回入口"
				:primary-card="showcasePrimaryCard"
				:metrics="showcaseMetrics"
				:tags="showcaseTags"
			/>

			<section class="auth-panel">
				<header class="auth-panel__header">
					<div>
						<h2>登录控制台</h2>
						<p>输入账号与密码，完成安全校验后进入授权工作区。</p>
					</div>
					<ThemePicker />
				</header>

				<Account />

				<footer class="auth-panel__footer">
					<span>首次登录后请更新默认密码。</span>
					<span>{{ currentYear }} {{ pageTitle }}</span>
				</footer>
			</section>
		</div>
	</div>
</template>

<script setup lang="ts">
import { computed, onMounted } from 'vue';
import { storeToRefs } from 'pinia';
import { useThemeConfig } from '/@/stores/themeConfig';
import { NextLoading } from '/@/utils/loading';
import Account from '/@/views/login/component/account.vue';
import AuthShowcase from '/@/views/login/component/AuthShowcase.vue';
import ThemePicker from '/@/components/theme/ThemePicker.vue';

const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
const pageTitle = computed(() => themeConfig.value.globalTitle || 'IoTSharp');
const currentYear = new Date().getFullYear();
const showcaseDescription = '统一进入接入采集、实时规则、运维发布和平台治理工作区。';
const showcasePrimaryCard = {
	label: '访问范围',
	value: '按权限加载',
	title: 'IoTSharp 控制平面',
	description: '认证成功后，导航与数据范围由当前用户所属租户和角色决定。',
};
const showcaseMetrics = [
	{ label: '接入与采集', value: 'Product / Asset / Device', description: '管理能力模板、业务对象和运行实例。', tone: 'primary' as const },
	{ label: '实时规则', value: 'RuleChain', description: '处理实时事件、规则链与审计记录。', tone: 'accent' as const },
	{ label: '运维与发布', value: 'Edge / Release', description: '管理边缘运行时、任务和发布闭环。', tone: 'success' as const },
];
const showcaseTags = ['租户隔离', '权限', '审计', '运行证据'];

onMounted(() => NextLoading.done());
</script>

<style scoped lang="scss">
.auth-page {
	display: flex;
	min-height: 100vh;
	align-items: center;
	justify-content: center;
	padding: 24px;
	background: var(--iotsharp-quiet-gradient);
	overflow-x: hidden;
	overflow-y: auto;
}

.auth-shell {
	display: grid;
	grid-template-columns: minmax(420px, 1.05fr) minmax(400px, 0.95fr);
	width: min(1080px, 100%);
	min-height: min(680px, calc(100vh - 48px));
	overflow: hidden;
	border: 1px solid rgba(var(--iotsharp-accent-rgb), 0.18);
	border-radius: 8px;
	background: rgba(255, 255, 255, 0.66);
	box-shadow: var(--iotsharp-shadow-float);
	backdrop-filter: blur(20px) saturate(1.15);
}

.auth-panel {
	display: flex;
	flex-direction: column;
	justify-content: center;
	gap: 24px;
	padding: 34px 38px;
	background: rgba(255, 255, 255, 0.94);
}

.auth-panel__header {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 14px;
}

.auth-panel__header h2 {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 24px;
	font-weight: 700;
	letter-spacing: 0;
}

.auth-panel__header p {
	max-width: 48ch;
	margin: 7px 0 0;
	color: var(--iotsharp-text-soft);
	font-size: 12px;
	line-height: 1.65;
}

.auth-panel__footer {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	padding-top: 14px;
	border-top: 1px solid var(--iotsharp-border);
	color: var(--iotsharp-text-muted);
	font-size: 10px;
}

@media (max-width: 920px) {
	.auth-shell {
		grid-template-columns: 1fr;
		max-width: 680px;
	}
}

@media (max-width: 620px) {
	.auth-page {
		padding: 0;
	}

	.auth-shell {
		min-height: 100vh;
		border: 0;
		border-radius: 0;
	}

	.auth-panel {
		padding: 24px 20px;
	}

	.auth-panel__footer {
		align-items: flex-start;
		flex-direction: column;
	}
}
</style>
