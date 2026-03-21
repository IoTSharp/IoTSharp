<template>
	<div class="asset-page">
		<ConsoleCrudWorkspace
			eyebrow="Asset Workspace"
			title="资产管理"
			description="延续新的工作台风格来查看资产清单、类型和描述信息，同时保留资产详情抽屉与 CRUD 管理能力。"
			card-eyebrow="Asset Table"
			card-title="资产列表"
			card-description="支持名称搜索、详情查看和资产编辑，适合与设备、规则页面保持一致的管理节奏。"
			:badges="badges"
			:metrics="metrics"
		>
			<template #actions>
				<el-button type="primary" @click="refreshList">刷新列表</el-button>
			</template>

			<template #aside>
				<div class="asset-page__scope">
					<span>资产工作区</span>
					<strong>{{ overview.total }}</strong>
					<small>资产详情入口与列表视图已统一整理</small>
				</div>
			</template>

			<div class="asset-page__crud">
				<fs-crud ref="crudRef" v-bind="crudBinding">
					<template #actionbar-right>
						<div class="console-crud-tags">
							<span class="console-crud-tag">当前页 {{ overview.pageCount }} 个</span>
							<span class="console-crud-tag is-primary">类型 {{ overview.typeCount }} 类</span>
						</div>
					</template>
				</fs-crud>
			</div>
		</ConsoleCrudWorkspace>

		<AssetDetail ref="assetDetailRef" />
	</div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useCrud, useExpose } from '@fast-crud/fast-crud';
import ConsoleCrudWorkspace from '/@/components/console/ConsoleCrudWorkspace.vue';
import AssetDetail from './assetdetail.vue';
import { createAssetListCrudOptions } from './crudOptions/assetListCrudOptions';

const crudRef = ref();
const crudBinding = ref();
const assetDetailRef = ref();
const overview = reactive({
	total: 0,
	pageCount: 0,
	typeCount: 0,
	describedCount: 0,
	lastRefresh: '',
});

const { crudExpose } = useExpose({ crudRef, crudBinding });
const { crudOptions } = createAssetListCrudOptions({ expose: crudExpose }, assetDetailRef, overview);

// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
useCrud({ expose: crudExpose, crudOptions });

const badges = computed(() => [
	'全局资产视图',
	`已写描述 ${overview.describedCount} 个`,
	overview.lastRefresh ? `最近同步 ${overview.lastRefresh}` : '等待首次同步',
]);

const metrics = computed(() => [
	{
		label: '资产总数',
		value: overview.total,
		hint: '当前平台中的资产总量。',
		tone: 'primary' as const,
	},
	{
		label: '当前页资产',
		value: overview.pageCount,
		hint: '当前页已加载的资产数量。',
		tone: 'accent' as const,
	},
	{
		label: '资产类型',
		value: overview.typeCount,
		hint: '当前页涉及的资产类型数量。',
		tone: 'success' as const,
	},
	{
		label: '描述完整',
		value: overview.describedCount,
		hint: '当前页已填写描述的资产数量。',
		tone: 'warning' as const,
	},
]);

const refreshList = () => {
	crudExpose.doRefresh();
};

onMounted(() => {
	refreshList();
});
</script>

<style scoped lang="scss">
.asset-page {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.asset-page__scope {
	display: flex;
	flex-direction: column;
	align-items: flex-end;
	min-width: 160px;
	padding: 14px 16px;
	border-radius: 20px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(255, 255, 255, 0.78);
}

.asset-page__scope span {
	color: #64748b;
	font-size: 12px;
}

.asset-page__scope strong {
	margin-top: 8px;
	color: #123b6d;
	font-size: 30px;
	line-height: 1;
}

.asset-page__scope small {
	margin-top: 8px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
	text-align: right;
}

.asset-page__crud {
	min-height: 520px;
}

:deep(.fs-crud) {
	--el-fill-color-blank: transparent;
}

:deep(.fs-crud .el-card) {
	box-shadow: none;
}

:deep(.fs-crud .el-table) {
	border-radius: 20px;
	overflow: hidden;
}

:deep(.fs-crud .el-table th.el-table__cell) {
	background: #f8fbff;
}

:deep(.fs-crud .el-pagination) {
	margin-top: 18px;
}

@media (max-width: 767px) {
	.asset-page__scope {
		width: 100%;
		align-items: flex-start;
	}

	.asset-page__scope small {
		text-align: left;
	}

	.asset-page__crud {
		min-height: auto;
	}
}
</style>
