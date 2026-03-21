<template>
	<div class="user-page">
		<ConsoleCrudWorkspace
			eyebrow="User Workspace"
			title="用户管理"
			description="把客户下的账号管理统一到新的工作台结构里，便于查看锁定状态、失败次数和联系方式等关键信息。"
			card-eyebrow="User Table"
			card-title="用户列表"
			card-description="支持用户名搜索、编辑、新增与删除，同时保留锁定状态切换等原有管理能力。"
			:badges="badges"
			:metrics="metrics"
		>
			<template #actions>
				<el-button type="primary" @click="refreshList">刷新列表</el-button>
			</template>

			<template #aside>
				<div class="user-page__scope">
					<span>当前客户</span>
					<strong>{{ shortCustomerId }}</strong>
					<small>登录、锁定与联系方式都可在此统一查看</small>
				</div>
			</template>

			<div class="user-page__crud">
				<fs-crud ref="crudRef" v-bind="crudBinding">
					<template #actionbar-right>
						<div class="console-crud-tags">
							<span class="console-crud-tag">当前页 {{ overview.pageCount }} 个</span>
							<span class="console-crud-tag is-primary">已锁定 {{ overview.lockedCount }} 个</span>
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
import { createUserListCrudOptions } from './crudOptions/userListCrudOptions';

const stores = useUserInfo();
const route = useRoute();
const { userInfos } = storeToRefs(stores);

const crudRef = ref();
const crudBinding = ref();
const overview = reactive({
	total: 0,
	pageCount: 0,
	lockedCount: 0,
	failedCount: 0,
	contactCount: 0,
	lastRefresh: '',
});

const customerId = route.query.id || userInfos.value.customer.id;
const shortCustomerId = computed(() => {
	const value = String(customerId || '--');
	return value.length > 12 ? `${value.slice(0, 8)}...` : value;
});

const { crudExpose } = useExpose({ crudRef, crudBinding });
const { crudOptions } = createUserListCrudOptions({ expose: crudExpose }, customerId, overview);

// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
useCrud({ expose: crudExpose, crudOptions });

const badges = computed(() => [
	`客户 ${shortCustomerId.value}`,
	`失败累计 ${overview.failedCount}`,
	overview.lastRefresh ? `最近同步 ${overview.lastRefresh}` : '等待首次同步',
]);

const metrics = computed(() => [
	{
		label: '用户总数',
		value: overview.total,
		hint: '当前客户下可登录控制台的账号数量。',
		tone: 'primary' as const,
	},
	{
		label: '当前页用户',
		value: overview.pageCount,
		hint: '当前列表页展示的用户数量。',
		tone: 'accent' as const,
	},
	{
		label: '锁定账号',
		value: overview.lockedCount,
		hint: '当前页处于锁定状态的账号数量。',
		tone: 'warning' as const,
	},
	{
		label: '联系方式',
		value: overview.contactCount,
		hint: '当前页已填写手机号的用户数量。',
		tone: 'success' as const,
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
.user-page {
	display: flex;
	flex-direction: column;
	gap: 18px;
}

.user-page__scope {
	display: flex;
	flex-direction: column;
	align-items: flex-end;
	min-width: 180px;
	padding: 14px 16px;
	border-radius: 20px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	background: rgba(255, 255, 255, 0.78);
}

.user-page__scope span {
	color: #64748b;
	font-size: 12px;
}

.user-page__scope strong {
	margin-top: 8px;
	color: #123b6d;
	font-size: 24px;
	line-height: 1;
}

.user-page__scope small {
	margin-top: 8px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
	text-align: right;
}

.user-page__crud {
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
	.user-page__scope {
		width: 100%;
		align-items: flex-start;
	}

	.user-page__scope small {
		text-align: left;
	}

	.user-page__crud {
		min-height: auto;
	}
}
</style>
