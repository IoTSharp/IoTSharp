<template>
	<div class="customer-page">
		<ConsoleCrudWorkspace
			eyebrow="Customer Workspace"
			title="客户管理"
			description="在租户视角下统一查看客户清单，并继续下钻到设备和用户管理，让组织层级在后台中保持清晰路径。"
			card-eyebrow="Customer Table"
			card-title="客户列表"
			card-description="支持名称搜索、编辑、新增，并能从客户直接进入设备管理和用户管理。"
			:badges="badges"
			:metrics="metrics"
		>
			<template #actions>
				<el-button type="primary" @click="refreshList">刷新列表</el-button>
			</template>

			<template #aside>
				<div class="customer-page__scope">
					<span>当前租户</span>
					<strong>{{ shortTenantId }}</strong>
					<small>客户列表与下游设备、用户入口已串联</small>
				</div>
			</template>

			<div class="customer-page__crud">
				<fs-crud ref="crudRef" v-bind="crudBinding">
					<template #actionbar-right>
						<div class="console-crud-tags">
							<span class="console-crud-tag">当前页 {{ overview.pageCount }} 个</span>
							<span class="console-crud-tag is-primary">电话资料 {{ overview.phoneCount }} 个</span>
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
import { storeToRefs } from 'pinia';
import { useRoute } from 'vue-router';
import ConsoleCrudWorkspace from '/@/components/console/ConsoleCrudWorkspace.vue';
import { useUserInfo } from '/@/stores/userInfo';
import { createCustomerListCrudOptions } from './crudOptions/customerListCrudOptions';

const stores = useUserInfo();
const route = useRoute();
const { userInfos } = storeToRefs(stores);

const crudRef = ref();
const crudBinding = ref();
const overview = reactive({
	total: 0,
	pageCount: 0,
	emailCount: 0,
	phoneCount: 0,
	lastRefresh: '',
});

const tenantId = route.query.id || userInfos.value.tenant.id;
const shortTenantId = computed(() => {
	const value = String(tenantId || '--');
	return value.length > 12 ? `${value.slice(0, 8)}...` : value;
});

const { crudExpose } = useExpose({ crudRef, crudBinding });
const { crudOptions } = createCustomerListCrudOptions({ expose: crudExpose }, tenantId, overview);

// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
useCrud({ expose: crudExpose, crudOptions });

const badges = computed(() => [
	`租户 ${shortTenantId.value}`,
	`当前页 ${overview.pageCount} 个`,
	overview.lastRefresh ? `最近同步 ${overview.lastRefresh}` : '等待首次同步',
]);

const metrics = computed(() => [
	{
		label: '客户总数',
		value: overview.total,
		hint: '当前租户下可管理的客户数量。',
		tone: 'primary' as const,
	},
	{
		label: '当前页客户',
		value: overview.pageCount,
		hint: '便于快速查看当前筛选页的客户范围。',
		tone: 'accent' as const,
	},
	{
		label: '邮箱资料',
		value: overview.emailCount,
		hint: '当前页已填写邮箱的客户数量。',
		tone: 'success' as const,
	},
	{
		label: '电话资料',
		value: overview.phoneCount,
		hint: '当前页已填写电话信息的客户数量。',
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
.customer-page {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.customer-page__scope {
	display: flex;
	flex-direction: column;
	align-items: flex-end;
	min-width: 180px;
	padding: 14px 16px;
	border-radius: 20px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(255, 255, 255, 0.78);
}

.customer-page__scope span {
	color: #64748b;
	font-size: 12px;
}

.customer-page__scope strong {
	margin-top: 8px;
	color: #123b6d;
	font-size: 24px;
	line-height: 1;
}

.customer-page__scope small {
	margin-top: 8px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
	text-align: right;
}

.customer-page__crud {
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
	.customer-page__scope {
		width: 100%;
		align-items: flex-start;
	}

	.customer-page__scope small {
		text-align: left;
	}

	.customer-page__crud {
		min-height: auto;
	}
}
</style>
