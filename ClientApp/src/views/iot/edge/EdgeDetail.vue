<template>
	<div>
		<el-drawer v-model="drawer" size="72%" class="edge-detail-drawer" append-to-body destroy-on-close>
			<ConsoleDrawerWorkspace
				eyebrow="Edge Detail"
				:title="edgeName"
				description="集中查看 Edge 节点的基础信息、能力声明、元数据和运行指标，作为第一版对接与排障窗口。"
				:badges="badges"
				:metrics="metrics"
			>
				<template #actions>
					<el-button type="primary" @click="reload">刷新</el-button>
				</template>

				<section class="edge-detail__overview">
					<article class="edge-card edge-card--main">
						<div class="edge-card__header">
							<div>
								<h3>节点概览</h3>
								<p>展示 Edge 节点第一版管理范围内的基础运行时字段，便于核对注册与心跳状态。</p>
							</div>
						</div>
						<el-descriptions :column="2" border>
							<el-descriptions-item label="Name">{{ edgeRef.name || '--' }}</el-descriptions-item>
							<el-descriptions-item label="RuntimeType">{{ edgeRef.runtimeType || '--' }}</el-descriptions-item>
							<el-descriptions-item label="RuntimeName">{{ edgeRef.runtimeName || '--' }}</el-descriptions-item>
							<el-descriptions-item label="Version">{{ edgeRef.version || '--' }}</el-descriptions-item>
							<el-descriptions-item label="Status">{{ edgeRef.status || '--' }}</el-descriptions-item>
							<el-descriptions-item label="Healthy">{{ formatBoolean(edgeRef.healthy) }}</el-descriptions-item>
							<el-descriptions-item label="Active">{{ formatBoolean(edgeRef.active) }}</el-descriptions-item>
							<el-descriptions-item label="Platform">{{ edgeRef.platform || '--' }}</el-descriptions-item>
							<el-descriptions-item label="HostName">{{ edgeRef.hostName || '--' }}</el-descriptions-item>
							<el-descriptions-item label="IpAddress">{{ edgeRef.ipAddress || '--' }}</el-descriptions-item>
							<el-descriptions-item label="LastHeartbeatDateTime">{{ formatDateTime(edgeRef.lastHeartbeatDateTime) }}</el-descriptions-item>
							<el-descriptions-item label="LastActivityDateTime">{{ formatDateTime(edgeRef.lastActivityDateTime) }}</el-descriptions-item>
						</el-descriptions>
					</article>

					<article class="edge-card">
						<div class="edge-card__header">
							<div>
								<h3>对接提示</h3>
								<p>第一版只覆盖查询、筛选和详情，不承载配置下发与任务执行操作。</p>
							</div>
						</div>
						<ul class="edge-tip-list">
							<li>能力声明用于确认 Gateway 或 PiXiu 的协议与任务支持范围。</li>
							<li>metadata 保留部署与环境上下文，避免塞入高频或大体量运行数据。</li>
							<li>metrics 仅做详情展示，不参与第一版筛选与排序。</li>
						</ul>
					</article>
				</section>

				<section class="edge-json-panels">
					<article class="edge-card">
						<div class="edge-card__header"><h3>Capabilities</h3></div>
						<pre>{{ prettyJson(edgeRef.capabilities) }}</pre>
					</article>
					<article class="edge-card">
						<div class="edge-card__header"><h3>Metadata</h3></div>
						<pre>{{ prettyJson(edgeRef.metadata) }}</pre>
					</article>
					<article class="edge-card">
						<div class="edge-card__header"><h3>Metrics</h3></div>
						<pre>{{ prettyJson(edgeRef.metrics) }}</pre>
					</article>
				</section>
			</ConsoleDrawerWorkspace>
		</el-drawer>
	</div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import dayjs from 'dayjs';
import ConsoleDrawerWorkspace from '/@/components/console/ConsoleDrawerWorkspace.vue';
import { edgeApi } from '/@/api/edge/index';

const drawer = ref(false);
const edgeRef = ref<Record<string, any>>({});

const edgeName = computed(() => edgeRef.value?.name || 'Edge 详情');
const badges = computed(() => [edgeRef.value?.runtimeType || 'unknown', edgeRef.value?.status || 'unknown']);
const metrics = computed(() => [
	{ label: 'Runtime', value: edgeRef.value?.runtimeName || '--', hint: '运行时实例名称', tone: 'primary' as const },
	{ label: 'Version', value: edgeRef.value?.version || '--', hint: '运行时版本', tone: 'success' as const },
	{ label: 'Healthy', value: formatBoolean(edgeRef.value?.healthy), hint: '最近健康状态', tone: 'accent' as const },
	{ label: 'Active', value: formatBoolean(edgeRef.value?.active), hint: '最近活跃状态', tone: 'warning' as const },
]);

const formatDateTime = (value?: string) => (value ? dayjs(value).format('YYYY-MM-DD HH:mm:ss') : '--');
const formatBoolean = (value?: boolean | null) => (value === null || value === undefined ? '--' : value ? '是' : '否');
const prettyJson = (raw?: string) => {
	if (!raw) {
		return '{}';
	}

	try {
		return JSON.stringify(JSON.parse(raw), null, 2);
	} catch {
		return raw;
	}
};

const reload = async () => {
	if (!edgeRef.value?.id) {
		return;
	}

	const res = await edgeApi().getEdgeDetail(edgeRef.value.id);
	edgeRef.value = res.data ?? {};
};

const openDialog = async (edge: Record<string, any>) => {
	edgeRef.value = { ...(edge ?? {}) };
	drawer.value = true;
	await reload();
};

defineExpose({
	openDialog,
});
</script>

<style lang="scss" scoped>
.edge-detail__overview,
.edge-json-panels {
	display: grid;
	gap: 18px;
}

.edge-detail__overview {
	grid-template-columns: 1.7fr 1fr;
	margin-bottom: 18px;
}

.edge-json-panels {
	grid-template-columns: repeat(3, minmax(0, 1fr));
}

.edge-card {
	padding: 18px;
	border-radius: 24px;
	border: 1px solid rgba(226, 232, 240, 0.92);
	background: linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.96));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.edge-card__header {
	margin-bottom: 14px;
}

.edge-card__header h3 {
	margin: 0;
	color: #123b6d;
}

.edge-card__header p {
	margin: 8px 0 0;
	color: #64748b;
	font-size: 13px;
	line-height: 1.7;
}

.edge-tip-list {
	margin: 0;
	padding-left: 18px;
	color: #475569;
	line-height: 1.8;
}

pre {
	margin: 0;
	padding: 14px;
	border-radius: 18px;
	background: #0f172a;
	color: #dbeafe;
	overflow: auto;
	min-height: 220px;
	font-size: 12px;
	line-height: 1.6;
}

@media (max-width: 1200px) {
	.edge-detail__overview,
	.edge-json-panels {
		grid-template-columns: 1fr;
	}
}
</style>