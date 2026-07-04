<template>
	<div class="auth-page">
		<div class="auth-page__aurora"></div>
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
				<div class="auth-panel__header">
					<div class="auth-panel__eyebrow">Sign In</div>
					<h2>登录到 {{ pageTitle }}</h2>
					<p>输入账号和密码，完成验证码后进入控制台。</p>
				</div>

				<div class="auth-panel__highlights">
					<div v-for="item in panelHighlights" :key="item.label" class="auth-panel__highlight">
						<span>{{ item.label }}</span>
						<strong>{{ item.value }}</strong>
						<small>{{ item.hint }}</small>
					</div>
				</div>

				<Account />

				<div class="auth-panel__footer">
					<div>建议首次登录后立即修改默认密码。</div>
					<div>{{ currentYear }} {{ pageTitle }}</div>
				</div>
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

const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);

const pageTitle = computed(() => themeConfig.value.globalTitle || 'IoTSharp');
const currentYear = new Date().getFullYear();
const showcaseDescription = '用于管理员、租户和运维人员进入 IoTSharp 控制台。';

const showcasePrimaryCard = {
	label: '当前入口',
	value: '控制台',
	title: '账号登录',
	description: '认证成功后按账号权限进入对应工作区。',
};

const showcaseMetrics = [
	{
		label: '账号类型',
		value: '管理员',
		description: '支持管理员和授权用户登录。',
		tone: 'primary' as const,
	},
	{
		label: '校验方式',
		value: '验证码',
		description: '提交前需要完成一次交互校验。',
		tone: 'accent' as const,
	},
	{
		label: '登录结果',
		value: '按权限进入',
		description: '菜单和数据范围由当前账号权限决定。',
		tone: 'success' as const,
	},
];

const showcaseTags = ['认证', '权限', '审计', '工作区'];

const panelHighlights = [
	{ label: '登录方式', value: '账号密码', hint: '使用已分配的控制台账号。' },
	{ label: '安全校验', value: '滑块拼图', hint: '提交前完成验证码校验。' },
	{ label: '进入后', value: '控制台', hint: '按权限加载菜单和数据。' },
];

onMounted(() => {
	NextLoading.done();
});
</script>

<style scoped lang="scss">
.auth-page {
	position: relative;
	height: 100vh;
	min-height: 100vh;
	padding: 24px;
	background:
		radial-gradient(circle at top left, rgba(14, 165, 233, 0.18), transparent 26%),
		radial-gradient(circle at bottom right, rgba(59, 130, 246, 0.18), transparent 28%),
		linear-gradient(180deg, #f3f8ff 0%, #edf6ff 44%, #f9fcff 100%);
	overflow-x: hidden;
	overflow-y: auto;
	overscroll-behavior-y: contain;
	scrollbar-gutter: stable;
	-webkit-overflow-scrolling: touch;
}

.auth-page__aurora {
	position: absolute;
	inset: 0;
	background:
		linear-gradient(rgba(148, 163, 184, 0.08) 1px, transparent 1px),
		linear-gradient(90deg, rgba(148, 163, 184, 0.08) 1px, transparent 1px);
	background-size: 48px 48px;
	mask-image: radial-gradient(circle at center, #000 42%, transparent 88%);
	pointer-events: none;
}

.auth-shell {
	position: relative;
	z-index: 1;
	display: grid;
	grid-template-columns: 1.15fr minmax(400px, 480px);
	width: min(1240px, 100%);
	margin: 0 auto;
	min-height: min(780px, calc(100vh - 48px));
	border-radius: 34px;
	border: 1px solid rgba(255, 255, 255, 0.72);
	background: rgba(255, 255, 255, 0.38);
	box-shadow: 0 30px 80px rgba(15, 23, 42, 0.12);
	backdrop-filter: blur(18px);
	overflow: hidden;
}

.auth-panel {
	display: flex;
	flex-direction: column;
	justify-content: center;
	gap: 24px;
	padding: 36px 40px;
	background: rgba(255, 255, 255, 0.95);
}

.auth-panel__eyebrow {
	margin-bottom: 12px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.auth-panel__header h2 {
	margin: 0 0 12px;
	color: #123b6d;
	font-size: 32px;
	letter-spacing: -0.04em;
}

.auth-panel__header p {
	margin: 0;
	color: #64748b;
	font-size: 14px;
	line-height: 1.85;
}

.auth-panel__highlights {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 12px;
}

.auth-panel__highlight {
	padding: 14px 16px;
	border-radius: 18px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(255, 255, 255, 0.98));
}

.auth-panel__highlight span {
	display: block;
	color: #64748b;
	font-size: 12px;
}

.auth-panel__highlight strong {
	display: block;
	margin-top: 10px;
	color: #123b6d;
	font-size: 18px;
	font-weight: 700;
	letter-spacing: -0.03em;
}

.auth-panel__highlight small {
	display: block;
	margin-top: 6px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
}

.auth-panel__footer {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
	padding-top: 18px;
	border-top: 1px solid rgba(226, 232, 240, 0.9);
	color: #64748b;
	font-size: 12px;
}

@media (max-width: 1080px) {
	.auth-shell {
		grid-template-columns: 1fr;
		max-width: 720px;
	}
}

@media (max-width: 767px) {
	.auth-page {
		padding: 0;
	}

	.auth-shell {
		min-height: 100vh;
		border-radius: 0;
	}

	.auth-panel {
		padding: 24px;
	}

	.auth-panel__highlights {
		grid-template-columns: 1fr;
	}

	.auth-panel__footer {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
