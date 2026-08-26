<template>
	<div class="entry-page">
		<header class="entry-header">
			<RouterLink class="entry-header__brand" to="/">
				<AppLogo />
			</RouterLink>
			<div class="entry-header__tools">
				<ThemePicker />
				<RouterLink class="entry-header__login" to="/login">登录</RouterLink>
			</div>
		</header>

		<main class="entry-main">
			<section class="entry-intro">
				<div class="entry-intro__copy">
					<h1>IoTSharp</h1>
					<p>统一管理物联网接入与采集、实时规则、边缘运维和受控发布。</p>
				</div>

				<div class="entry-layers" aria-label="平台能力">
					<div v-for="layer in platformLayers" :key="layer.title" class="entry-layer">
						<el-icon><component :is="layer.icon" /></el-icon>
						<div>
							<strong>{{ layer.title }}</strong>
							<span>{{ layer.description }}</span>
						</div>
					</div>
				</div>
			</section>

			<section class="entry-access" aria-label="实例入口">
				<div class="entry-access__status">
					<div>
						<span>实例状态</span>
						<strong>{{ isInstalled ? '已初始化' : '等待初始化' }}</strong>
					</div>
					<span class="status-indicator" :class="{ 'is-ready': isInstalled }"></span>
				</div>

				<dl class="entry-access__facts">
					<div>
						<dt>版本</dt>
						<dd>{{ versionText }}</dd>
					</div>
					<div>
						<dt>认证</dt>
						<dd>账号 + 安全校验</dd>
					</div>
					<div>
						<dt>工作区</dt>
						<dd>按租户与角色加载</dd>
					</div>
				</dl>

				<RouterLink v-if="isInstalled" class="entry-access__primary" to="/login">进入控制台</RouterLink>
				<RouterLink v-else class="entry-access__primary" to="/installer">开始初始化</RouterLink>
				<div class="entry-access__links">
					<RouterLink to="/signup">注册租户账号</RouterLink>
					<RouterLink to="/installer">系统初始化</RouterLink>
				</div>
			</section>
		</main>
	</div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { RouterLink } from 'vue-router';
import { Connection, Cpu, SetUp } from '@element-plus/icons-vue';
import AppLogo from '/@/components/AppLogo.vue';
import ThemePicker from '/@/components/theme/ThemePicker.vue';
import { useAppInfo } from '/@/stores/appInfo';

const storesAppInfo = useAppInfo();
const isInstalled = computed(() => Boolean(storesAppInfo.appInfo.installed));
const versionText = computed(() => storesAppInfo.appInfo.version || '--');
const platformLayers = [
	{ title: '接入与采集', description: 'Product、Asset、Device、Gateway、EdgeNode 与采集模板', icon: Connection },
	{ title: '实时规则', description: '面向实时事件的规则链、模拟、追踪和审计', icon: SetUp },
	{ title: '运维与发布', description: '配置版本、边缘任务、灰度发布、确认与回滚', icon: Cpu },
];
</script>

<style scoped lang="scss">
.entry-page {
	min-height: 100vh;
	background: var(--iotsharp-quiet-gradient);
	color: var(--iotsharp-text);
}

.entry-header {
	display: flex;
	height: 64px;
	align-items: center;
	justify-content: space-between;
	gap: 18px;
	padding: 0 28px;
	border-bottom: 1px solid rgba(var(--iotsharp-accent-rgb), 0.14);
	background: var(--iotsharp-topbar-gradient);
	backdrop-filter: blur(18px) saturate(1.15);
}

.entry-header__brand,
.entry-header__login,
.entry-access__primary,
.entry-access__links a {
	text-decoration: none;
}

.entry-header__tools {
	display: flex;
	align-items: center;
	gap: 8px;
}

.entry-header__login {
	display: inline-flex;
	height: 36px;
	align-items: center;
	padding: 0 14px;
	border-radius: 6px;
	background: var(--iotsharp-accent-gradient);
	color: #ffffff;
	font-size: 12px;
	font-weight: 650;
}

.entry-main {
	display: grid;
	grid-template-columns: minmax(0, 1.5fr) minmax(320px, 0.65fr);
	width: min(1080px, calc(100% - 40px));
	min-height: calc(100vh - 64px);
	margin: 0 auto;
	align-items: center;
	gap: 60px;
	padding: 42px 0;
}

.entry-intro__copy {
	max-width: 68ch;
}

.entry-intro h1 {
	margin: 0;
	color: var(--iotsharp-ink);
	font-size: 48px;
	font-weight: 720;
	letter-spacing: 0;
	line-height: 1.08;
}

.entry-intro__copy p {
	margin: 15px 0 0;
	color: var(--iotsharp-text-soft);
	font-size: 17px;
	line-height: 1.75;
}

.entry-layers {
	display: grid;
	margin-top: 34px;
	border-top: 1px solid rgba(var(--iotsharp-accent-rgb), 0.16);
}

.entry-layer {
	display: grid;
	grid-template-columns: 36px minmax(0, 1fr);
	align-items: center;
	gap: 12px;
	min-height: 70px;
	border-bottom: 1px solid rgba(var(--iotsharp-accent-rgb), 0.16);
}

.entry-layer > .el-icon {
	width: 34px;
	height: 34px;
	border-radius: 6px;
	background: var(--iotsharp-selection);
	color: var(--iotsharp-accent);
}

.entry-layer div {
	display: grid;
	gap: 3px;
}

.entry-layer strong {
	color: var(--iotsharp-ink);
	font-size: 13px;
}

.entry-layer span {
	color: var(--iotsharp-text-muted);
	font-size: 11px;
	line-height: 1.5;
}

.entry-access {
	padding: 22px;
	border: 1px solid rgba(var(--iotsharp-accent-rgb), 0.18);
	border-radius: 8px;
	background: rgba(255, 255, 255, 0.86);
	box-shadow: var(--iotsharp-shadow-float);
	backdrop-filter: blur(18px) saturate(1.1);
}

.entry-access__status {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 14px;
	padding-bottom: 16px;
	border-bottom: 1px solid var(--iotsharp-border);
}

.entry-access__status div {
	display: grid;
	gap: 3px;
}

.entry-access__status span,
.entry-access__facts dt {
	color: var(--iotsharp-text-muted);
	font-size: 10px;
}

.entry-access__status strong {
	color: var(--iotsharp-ink);
	font-size: 15px;
}

.status-indicator {
	width: 9px;
	height: 9px;
	border-radius: 50%;
	background: var(--iotsharp-warning);
	box-shadow: 0 0 0 4px var(--iotsharp-warning-surface);
}

.status-indicator.is-ready {
	background: var(--iotsharp-success);
	box-shadow: 0 0 0 4px var(--iotsharp-success-surface);
}

.entry-access__facts {
	display: grid;
	margin: 5px 0 18px;
}

.entry-access__facts div {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	padding: 10px 0;
	border-bottom: 1px solid var(--iotsharp-border);
}

.entry-access__facts dd {
	margin: 0;
	color: var(--iotsharp-text);
	font-size: 11px;
	font-weight: 600;
	text-align: right;
}

.entry-access__primary {
	display: flex;
	height: 42px;
	align-items: center;
	justify-content: center;
	border-radius: 6px;
	background: var(--iotsharp-accent-gradient);
	color: #ffffff;
	font-size: 13px;
	font-weight: 650;
	box-shadow: 0 10px 22px rgba(var(--iotsharp-accent-rgb), 0.18);
}

.entry-access__links {
	display: flex;
	justify-content: space-between;
	gap: 12px;
	margin-top: 13px;
}

.entry-access__links a {
	color: var(--iotsharp-accent);
	font-size: 10px;
	font-weight: 600;
}

@media (max-width: 820px) {
	.entry-main {
		grid-template-columns: 1fr;
		align-items: start;
		gap: 30px;
		padding: 36px 0;
	}

	.entry-intro h1 {
		font-size: 38px;
	}
}

@media (max-width: 520px) {
	.entry-header {
		padding: 0 14px;
	}

	.entry-main {
		width: calc(100% - 24px);
	}

	.entry-intro__copy p {
		font-size: 14px;
	}
}
</style>
