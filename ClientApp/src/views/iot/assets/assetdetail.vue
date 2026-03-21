<template>
	<div>
		<el-drawer
			v-model="drawer"
			size="82%"
			class="asset-detail-drawer"
			append-to-body
			destroy-on-close
		>
			<ConsoleDrawerWorkspace
				eyebrow="Asset Detail"
				:title="assetName"
				description="把资产基础信息、属性关系和遥测视图收拢到同一工作台里，减少在列表和多个抽屉之间来回切换。"
				:badges="badges"
				:metrics="metrics"
			>
				<template #actions>
					<el-button plain @click="activeTabName = 'props'">查看属性</el-button>
					<el-button type="primary" @click="activeTabName = 'telemetry'">查看遥测</el-button>
				</template>

				<section class="asset-detail__overview">
					<article class="asset-card asset-card--main">
						<div class="asset-card__header">
							<div>
								<h3>资产概览</h3>
								<p>集中展示资产标识、类型、名称和基础说明，方便在配置关系前快速确认上下文。</p>
							</div>
						</div>
						<AdvancedKeyValue
							:obj="assetRef"
							:config="assetColumns"
							:hide-key-list="[]"
							:label-width="150"
						/>
					</article>

					<article class="asset-card">
						<div class="asset-card__header">
							<div>
								<h3>运营提示</h3>
								<p>把资产类型、说明状态和当前查看重点整理在一起，方便运维同学快速定位。</p>
							</div>
						</div>
						<div class="asset-highlight-list">
							<div v-for="item in highlightItems" :key="item.label" class="asset-highlight-item">
								<span>{{ item.label }}</span>
								<strong>{{ item.value }}</strong>
								<small>{{ item.hint }}</small>
							</div>
						</div>
					</article>
				</section>

				<section class="asset-tabs-card">
					<el-tabs v-model="activeTabName" class="asset-tabs">
						<el-tab-pane name="basic">
							<template #label>
								<span class="asset-tab-label">
									<el-icon><InfoFilled /></el-icon>
									基础信息
								</span>
							</template>
							<div class="asset-tab-pane">
								<div class="asset-description-card">
									<span>资产说明</span>
									<p>{{ descriptionText }}</p>
								</div>
							</div>
						</el-tab-pane>

						<el-tab-pane name="props">
							<template #label>
								<span class="asset-tab-label">
									<el-icon><Grid /></el-icon>
									属性关系
								</span>
							</template>
							<div class="asset-tab-pane">
								<div class="asset-tab-panel">
									<div class="asset-tab-panel__header">
										<div>
											<h3>属性工作区</h3>
											<p>查看与当前资产相关的属性键、数据类型和最近值，维持和设备详情一致的工作台节奏。</p>
										</div>
									</div>
									<AssetDetailProps :asset-id="assetRef?.id" />
								</div>
							</div>
						</el-tab-pane>

						<el-tab-pane name="telemetry">
							<template #label>
								<span class="asset-tab-label">
									<el-icon><DataAnalysis /></el-icon>
									遥测关系
								</span>
							</template>
							<div class="asset-tab-pane">
								<div class="asset-tab-panel">
									<div class="asset-tab-panel__header">
										<div>
											<h3>遥测工作区</h3>
											<p>在同一抽屉内持续查看关系数据，避免从资产列表跳转到单独页面后失去上下文。</p>
										</div>
									</div>
									<AssetDetailTelemetry :asset-id="assetRef?.id" />
								</div>
							</div>
						</el-tab-pane>
					</el-tabs>
				</section>
			</ConsoleDrawerWorkspace>
		</el-drawer>
	</div>
</template>

<script lang="ts" setup>
import { computed, ref } from 'vue';
import { DataAnalysis, Grid, InfoFilled } from '@element-plus/icons-vue';
import AdvancedKeyValue from '/@/components/AdvancedKeyValue/AdvancedKeyValue.vue';
import ConsoleDrawerWorkspace from '/@/components/console/ConsoleDrawerWorkspace.vue';
import AssetDetailProps from '/@/views/iot/assets/detail/AssetDetailProps.vue';
import AssetDetailTelemetry from '/@/views/iot/assets/detail/AssetDetailTelemetry.vue';

const assetColumns = {
	id: {
		title: '资产 ID',
		type: 'text',
	},
	name: {
		title: '资产名称',
		type: 'text',
	},
	assetType: {
		title: '资产类型',
		type: 'dict-select',
		dict: {
			data: [
				{ value: 'Gateway', label: '网关资产' },
				{ value: 'Device', label: '设备资产' },
			],
		},
	},
	description: {
		title: '描述',
		type: 'text',
	},
};

const drawer = ref(false);
const activeTabName = ref('basic');
const assetRef = ref<Record<string, any>>({});

const assetName = computed(() => assetRef.value?.name || '资产详情');
const assetTypeText = computed(() => {
	if (assetRef.value?.assetType === 'Gateway') return '网关资产';
	if (assetRef.value?.assetType === 'Device') return '设备资产';
	return assetRef.value?.assetType || '未分类';
});
const descriptionText = computed(() => assetRef.value?.description || '暂未填写资产说明，建议补充用途和归属，便于后续协作。');
const shortIdentity = computed(() => {
	const assetId = assetRef.value?.id;
	if (!assetId) return '--';
	return `${String(assetId).slice(0, 12)}...`;
});

const badges = computed(() => [
	assetTypeText.value,
	assetRef.value?.id ? `ID ${String(assetRef.value.id).slice(0, 8)}` : '未同步',
]);

const metrics = computed(() => [
	{
		label: '资产类型',
		value: assetTypeText.value,
		hint: '当前资产在平台中的归类方式。',
		tone: 'primary' as const,
	},
	{
		label: '说明状态',
		value: assetRef.value?.description ? '已完善' : '待补充',
		hint: '完善描述能减少跨团队沟通成本。',
		tone: 'accent' as const,
	},
	{
		label: '查看面板',
		value: '3 个',
		hint: '基础信息、属性关系和遥测关系集中呈现。',
		tone: 'success' as const,
	},
	{
		label: '标识预览',
		value: shortIdentity.value,
		hint: '完整标识已在基础信息卡片中保留。',
		tone: 'warning' as const,
	},
]);

const highlightItems = computed(() => [
	{
		label: '当前焦点',
		value: activeTabName.value === 'props' ? '属性关系' : activeTabName.value === 'telemetry' ? '遥测关系' : '基础信息',
		hint: '切换标签即可在同一抽屉内继续查看。',
	},
	{
		label: '说明状态',
		value: assetRef.value?.description ? '内容完整' : '建议补充',
		hint: '补充用途、位置或归属后更利于运营排查。',
	},
	{
		label: '完整标识',
		value: assetRef.value?.id || '--',
		hint: '可用于排障时快速和后端记录对齐。',
	},
]);

const openDialog = (asset: Record<string, any>) => {
	drawer.value = true;
	activeTabName.value = 'basic';
	assetRef.value = { ...(asset ?? {}) };
};

const closeDialog = () => {
	drawer.value = false;
};

defineExpose({
	openDialog,
	closeDialog,
});
</script>

<style lang="scss" scoped>
:deep(.asset-detail-drawer .el-drawer__header) {
	margin-bottom: 0;
	padding-bottom: 0;
}

:deep(.asset-detail-drawer .el-drawer__body) {
	padding: 18px;
	background: linear-gradient(180deg, #f8fbff 0%, #f3f7fc 100%);
}

.asset-detail__overview {
	display: grid;
	grid-template-columns: 1.65fr 1fr;
	gap: 18px;
}

.asset-card,
.asset-tabs-card,
.asset-description-card,
.asset-tab-panel {
	border: 1px solid rgba(226, 232, 240, 0.92);
	background:
		radial-gradient(circle at top right, rgba(59, 130, 246, 0.08), transparent 30%),
		linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 250, 252, 0.96));
	box-shadow: 0 18px 42px rgba(15, 23, 42, 0.05);
}

.asset-card {
	padding: 22px;
	border-radius: 24px;
}

.asset-card--main :deep(.z-object-detail) {
	padding: 0;
}

.asset-card__header,
.asset-tab-panel__header {
	margin-bottom: 14px;

	h3 {
		margin: 0 0 8px;
		color: #123b6d;
		font-size: 20px;
		letter-spacing: -0.04em;
	}

	p {
		margin: 0;
		color: #64748b;
		font-size: 13px;
		line-height: 1.75;
	}
}

.asset-highlight-list {
	display: grid;
	gap: 12px;
}

.asset-highlight-item {
	padding: 16px 18px;
	border-radius: 18px;
	border: 1px solid rgba(191, 219, 254, 0.72);
	background: rgba(255, 255, 255, 0.78);
}

.asset-highlight-item span {
	display: block;
	color: #64748b;
	font-size: 12px;
}

.asset-highlight-item strong {
	display: block;
	margin-top: 10px;
	color: #123b6d;
	font-size: 18px;
	font-weight: 700;
	word-break: break-all;
}

.asset-highlight-item small {
	display: block;
	margin-top: 8px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
}

.asset-tabs-card {
	padding: 18px 20px;
	border-radius: 26px;
}

.asset-tab-label {
	display: inline-flex;
	align-items: center;
	gap: 8px;
	font-weight: 600;
}

.asset-tab-pane {
	padding-top: 12px;
}

.asset-description-card {
	padding: 20px 22px;
	border-radius: 20px;
}

.asset-description-card span {
	display: inline-flex;
	padding: 0 10px;
	height: 30px;
	align-items: center;
	border-radius: 999px;
	background: rgba(219, 234, 254, 0.9);
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
}

.asset-description-card p {
	margin: 16px 0 0;
	color: #475569;
	font-size: 14px;
	line-height: 1.9;
}

.asset-tab-panel {
	padding: 20px;
	border-radius: 22px;
}

.asset-tab-panel :deep(.z-crud) {
	height: calc(100vh - 430px);
}

@media (max-width: 1280px) {
	.asset-detail__overview {
		grid-template-columns: 1fr;
	}
}

@media (max-width: 767px) {
	:deep(.asset-detail-drawer) {
		width: 100% !important;
	}

	:deep(.asset-detail-drawer .el-drawer__body) {
		padding: 12px;
	}

	.asset-card,
	.asset-tabs-card,
	.asset-tab-panel,
	.asset-description-card {
		padding: 16px;
		border-radius: 20px;
	}

	.asset-tab-panel :deep(.z-crud) {
		height: calc(100vh - 500px);
	}
}
</style>
