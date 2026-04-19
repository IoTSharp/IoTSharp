<template>
	<el-dialog v-model="dialogVisible" title="Edge 接入信息" width="860px" destroy-on-close>
		<div class="edge-onboarding">
			<section class="edge-onboarding__hero">
				<div>
					<div class="edge-onboarding__eyebrow">Edge Bootstrap</div>
					<h3>{{ edgeRef.name || '边缘节点' }}</h3>
					<p>当前平台先把边缘运行时纳入 Gateway 身份管理，再通过统一 Edge 契约完成注册、能力上报和心跳上报。</p>
				</div>
				<div class="edge-onboarding__hero-side">
					<div class="edge-onboarding__hero-item">
						<span>运行时模板</span>
						<strong>{{ runtimeTypeLabel }}</strong>
					</div>
					<div class="edge-onboarding__hero-item">
						<span>认证方式</span>
						<strong>{{ identityRef?.identityType || 'AccessToken' }}</strong>
					</div>
				</div>
			</section>

			<el-alert
				v-if="identityRef?.identityType && identityRef.identityType !== 'AccessToken'"
				title="当前节点不是 AccessToken 认证。第一版 Edge 注册契约优先使用 AccessToken，请改成 AccessToken 后再接入。"
				type="warning"
				:closable="false"
				show-icon
			/>

			<el-skeleton :loading="loading" animated>
				<template #template>
					<el-skeleton-item variant="rect" style="width: 100%; height: 180px; margin-bottom: 16px" />
					<el-skeleton-item variant="rect" style="width: 100%; height: 220px" />
				</template>

				<template #default>
					<section class="edge-onboarding__grid">
						<article class="edge-onboarding__card">
							<div class="edge-onboarding__card-header">
								<div>
									<h4>接入凭据</h4>
									<p>把 AccessToken 提供给 Gateway 或 PiXiu，用它调用 Edge 注册与心跳接口。</p>
								</div>
								<el-button size="small" plain :disabled="!accessToken" @click="copyValue(accessToken)">复制 Token</el-button>
							</div>
							<el-descriptions :column="1" border>
								<el-descriptions-item label="节点名称">{{ edgeRef.name || '--' }}</el-descriptions-item>
								<el-descriptions-item label="DeviceId">{{ edgeRef.id || '--' }}</el-descriptions-item>
								<el-descriptions-item label="AccessToken">{{ accessToken || '--' }}</el-descriptions-item>
								<el-descriptions-item label="Register">{{ registerUrl }}</el-descriptions-item>
								<el-descriptions-item label="Capabilities">{{ capabilitiesUrl }}</el-descriptions-item>
								<el-descriptions-item label="Heartbeat">{{ heartbeatUrl }}</el-descriptions-item>
							</el-descriptions>
						</article>

						<article class="edge-onboarding__card">
							<div class="edge-onboarding__card-header">
								<div>
									<h4>接入顺序</h4>
									<p>按下面 4 步就能把边缘运行时纳入控制面。</p>
								</div>
							</div>
							<ol class="edge-onboarding__steps">
								<li>在运行时保存当前 AccessToken。</li>
								<li>启动后调用 Register 上报 `runtimeType`、`instanceId`、版本和宿主信息。</li>
								<li>随后调用 Capabilities 上报协议和任务能力。</li>
								<li>定时调用 Heartbeat 维持在线状态，并回填健康度、运行时指标和 uptime。</li>
							</ol>
						</article>
					</section>

					<section class="edge-onboarding__samples">
						<article class="edge-onboarding__card">
							<div class="edge-onboarding__card-header">
								<div>
									<h4>Register Payload</h4>
									<p>首次接入时建议先上报运行时身份。</p>
								</div>
								<el-button size="small" plain @click="copyValue(registerPayloadText)">复制 JSON</el-button>
							</div>
							<pre>{{ registerPayloadText }}</pre>
						</article>

						<article class="edge-onboarding__card">
							<div class="edge-onboarding__card-header">
								<div>
									<h4>Capabilities Payload</h4>
									<p>声明协议、功能和任务支持范围。</p>
								</div>
								<el-button size="small" plain @click="copyValue(capabilitiesPayloadText)">复制 JSON</el-button>
							</div>
							<pre>{{ capabilitiesPayloadText }}</pre>
						</article>

						<article class="edge-onboarding__card">
							<div class="edge-onboarding__card-header">
								<div>
									<h4>Heartbeat Payload</h4>
									<p>保持状态活跃，便于控制台展示在线、健康和最近心跳时间。</p>
								</div>
								<el-button size="small" plain @click="copyValue(heartbeatPayloadText)">复制 JSON</el-button>
							</div>
							<pre>{{ heartbeatPayloadText }}</pre>
						</article>
					</section>
				</template>
			</el-skeleton>
		</div>
	</el-dialog>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { ElMessage } from 'element-plus';
import commonFunction from '/@/utils/commonFunction';
import { edgeApi } from '/@/api/edge';

const dialogVisible = ref(false);
const loading = ref(false);
const edgeRef = ref<Record<string, any>>({});
const identityRef = ref<Record<string, any> | null>(null);
const runtimeTypeRef = ref('gateway');

const { copyText } = commonFunction();

const apiBaseUrl = computed(() => {
	const baseUrl = (import.meta.env.VITE_API_URL || window.location.origin || '').replace(/\/$/, '');
	return baseUrl || window.location.origin;
});
const accessToken = computed(() => identityRef.value?.identityId || edgeRef.value?.accessToken || '');
const runtimeType = computed(() => String(edgeRef.value?.runtimeType || runtimeTypeRef.value || 'gateway').toLowerCase());
const runtimeTypeLabel = computed(() => (runtimeType.value === 'pixiu' ? 'PiXiu' : 'Gateway'));
const registerUrl = computed(() => `${apiBaseUrl.value}/api/Edge/${accessToken.value || '{accessToken}'}/Register`);
const capabilitiesUrl = computed(() => `${apiBaseUrl.value}/api/Edge/${accessToken.value || '{accessToken}'}/Capabilities`);
const heartbeatUrl = computed(() => `${apiBaseUrl.value}/api/Edge/${accessToken.value || '{accessToken}'}/Heartbeat`);

const registerPayloadText = computed(() =>
	JSON.stringify(
		{
			runtimeType: runtimeType.value,
			runtimeName: edgeRef.value?.name || `${runtimeType.value}-runtime`,
			version: '1.0.0',
			instanceId: 'site-edge-001',
			platform: 'linux/amd64',
			hostName: 'edge-host',
			ipAddress: '192.168.1.10',
			metadata: {
				siteCode: 'site-001',
				source: 'iotsharp-console',
			},
		},
		null,
		2
	)
);
const capabilitiesPayloadText = computed(() =>
	JSON.stringify(
		{
			protocols: runtimeType.value === 'pixiu' ? ['modbus-tcp', 'opcua'] : ['modbus-tcp', 'mqtt'],
			features: ['register', 'heartbeat', 'task-receipt'],
			tasks: ['ConfigPush', 'ConfigPullRequest', 'HealthProbe'],
			metadata: {
				buffering: runtimeType.value === 'pixiu',
				diagnostics: true,
			},
		},
		null,
		2
	)
);
const heartbeatPayloadText = computed(() =>
	JSON.stringify(
		{
			timestamp: '2026-04-19T08:00:00Z',
			status: 'Running',
			healthy: true,
			uptimeSeconds: 120,
			ipAddress: '192.168.1.10',
			metrics: {
				cpu: 0.28,
				memoryMb: 256,
				queueDepth: 0,
			},
		},
		null,
		2
	)
);

const copyValue = async (value?: string) => {
	if (!value) {
		ElMessage.warning('当前没有可复制的内容');
		return;
	}

	await copyText(value);
};

const openDialog = async (edge: Record<string, any>, runtimeTypeHint = 'gateway') => {
	edgeRef.value = { ...(edge ?? {}) };
	runtimeTypeRef.value = runtimeTypeHint;
	dialogVisible.value = true;
	identityRef.value = null;

	if (!edgeRef.value?.id) {
		return;
	}

	loading.value = true;
	try {
		const response = await edgeApi().getDeviceIdentity(edgeRef.value.id);
		identityRef.value = response.data ?? null;
	}
	catch {
		ElMessage.error('读取接入身份失败');
	}
	finally {
		loading.value = false;
	}
};

defineExpose({
	openDialog,
});
</script>

<style lang="scss" scoped>
.edge-onboarding {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.edge-onboarding__hero,
.edge-onboarding__card {
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.94));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.edge-onboarding__hero {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 18px;
	padding: 22px;
	border-radius: 24px;
}

.edge-onboarding__eyebrow {
	margin-bottom: 10px;
	color: #0f766e;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.16em;
	text-transform: uppercase;
}

.edge-onboarding__hero h3,
.edge-onboarding__card h4 {
	margin: 0;
	color: #123b6d;
}

.edge-onboarding__hero p,
.edge-onboarding__card p {
	margin: 10px 0 0;
	color: #64748b;
	font-size: 13px;
	line-height: 1.7;
}

.edge-onboarding__hero-side {
	display: grid;
	gap: 12px;
	min-width: 180px;
}

.edge-onboarding__hero-item {
	padding: 14px 16px;
	border-radius: 18px;
	background: rgba(255, 255, 255, 0.82);
	border: 1px solid rgba(191, 219, 254, 0.92);
}

.edge-onboarding__hero-item span {
	display: block;
	color: #64748b;
	font-size: 12px;
}

.edge-onboarding__hero-item strong {
	display: block;
	margin-top: 8px;
	color: #123b6d;
	font-size: 18px;
}

.edge-onboarding__grid,
.edge-onboarding__samples {
	display: grid;
	gap: 18px;
}

.edge-onboarding__grid {
	grid-template-columns: repeat(2, minmax(0, 1fr));
}

.edge-onboarding__card {
	padding: 18px;
	border-radius: 22px;
}

.edge-onboarding__card-header {
	display: flex;
	align-items: flex-start;
	justify-content: space-between;
	gap: 16px;
	margin-bottom: 14px;
}

.edge-onboarding__steps {
	margin: 0;
	padding-left: 20px;
	color: #475569;
	line-height: 1.9;
}

pre {
	margin: 0;
	padding: 14px;
	border-radius: 18px;
	background: #0f172a;
	color: #dbeafe;
	overflow: auto;
	font-size: 12px;
	line-height: 1.7;
}

@media (max-width: 960px) {
	.edge-onboarding__hero,
	.edge-onboarding__card-header {
		flex-direction: column;
	}

	.edge-onboarding__grid {
		grid-template-columns: 1fr;
	}
}
</style>
