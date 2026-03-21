<template>
	<div class="tenant-page">
		<ConsoleCrudWorkspace
			eyebrow="Tenant Workspace"
			title="租户管理"
			description="把平台级租户清单收进统一的工作台页头中，方便继续进入客户管理、用户管理和设备接入链路。"
			card-eyebrow="Tenant Table"
			card-title="租户列表"
			card-description="支持名称搜索、编辑和新增操作，并可继续下钻到客户管理页面。"
			:badges="badges"
			:metrics="metrics"
		>
			<template #actions>
				<el-button type="primary" @click="refreshList">刷新列表</el-button>
			</template>

			<template #aside>
				<div class="tenant-page__scope">
					<span>平台级管理</span>
					<strong>{{ overview.total }}</strong>
					<small>租户可继续进入客户管理链路</small>
				</div>
			</template>

			<div class="tenant-page__crud">
				<fs-crud ref="crudRef" v-bind="crudBinding">
					<template #actionbar-right>
						<div class="console-crud-tags">
							<span class="console-crud-tag">当前页 {{ overview.pageCount }} 个</span>
							<span class="console-crud-tag is-primary">邮箱资料 {{ overview.emailCount }} 个</span>
						</div>
					</template>
				</fs-crud>
			</div>
		</ConsoleCrudWorkspace>
	</div>
</template>

<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import { useCrud, useExpose } from '@fast-crud/fast-crud';
import ConsoleCrudWorkspace from '/@/components/console/ConsoleCrudWorkspace.vue';
import { createTenantListCrudOptions } from './crudOptions/tenantListCrudOptions';

const crudRef = ref();
const crudBinding = ref();
const overview = reactive({
	total: 0,
	pageCount: 0,
	emailCount: 0,
	phoneCount: 0,
	lastRefresh: '',
});

const { crudExpose } = useExpose({ crudRef, crudBinding });
const { crudOptions } = createTenantListCrudOptions({ expose: crudExpose }, overview);

// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
useCrud({ expose: crudExpose, crudOptions });

const badges = computed(() => [
	'平台级视图',
	`每页 ${overview.pageCount || 0} 条`,
	overview.lastRefresh ? `最近同步 ${overview.lastRefresh}` : '等待首次同步',
]);

const metrics = computed(() => [
	{
		label: '租户总数',
		value: overview.total,
		hint: '平台当前已创建的租户数量。',
		tone: 'primary' as const,
	},
	{
		label: '当前页租户',
		value: overview.pageCount,
		hint: '便于快速浏览当前列表页的管理对象。',
		tone: 'accent' as const,
	},
	{
		label: '邮箱资料',
		value: overview.emailCount,
		hint: '当前页已填写邮箱的租户数量。',
		tone: 'success' as const,
	},
	{
		label: '联系电话',
		value: overview.phoneCount,
		hint: '当前页已填写电话信息的租户数量。',
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
.tenant-page {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.tenant-page__scope {
	display: flex;
	flex-direction: column;
	align-items: flex-end;
	min-width: 160px;
	padding: 14px 16px;
	border-radius: 20px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(255, 255, 255, 0.78);
}

.tenant-page__scope span {
	color: #64748b;
	font-size: 12px;
}

.tenant-page__scope strong {
	margin-top: 8px;
	color: #123b6d;
	font-size: 30px;
	line-height: 1;
}

.tenant-page__scope small {
	margin-top: 8px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
	text-align: right;
}

.tenant-page__crud {
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
	.tenant-page__scope {
		width: 100%;
		align-items: flex-start;
	}

	.tenant-page__scope small {
		text-align: left;
	}

	.tenant-page__crud {
		min-height: auto;
	}
}
</style>
