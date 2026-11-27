<template>
	<div class="user-page console-list-page">
		<ConsoleCrudWorkspace
			fast-crud
			eyebrow="User Workspace"
			title="用户管理"
			description="管理客户下的账号、联系信息和锁定状态。"
			card-eyebrow="User Table"
			card-title="用户列表"
			:summary="overview.lastRefresh ? `共 ${overview.total} 条` : undefined"
		>
			<template #actions>
				<el-button type="primary" @click="refreshList">刷新列表</el-button>
			</template>

			<fs-crud ref="crudRef" v-bind="crudBinding">
				<template #actionbar-right>
					<div class="console-crud-tags">
						<span class="console-crud-tag">当前页 {{ overview.pageCount }} 个</span>
						<span class="console-crud-tag is-primary">已锁定 {{ overview.lockedCount }} 个</span>
					</div>
				</template>
			</fs-crud>
		</ConsoleCrudWorkspace>
	</div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
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

const { crudExpose } = useExpose({ crudRef, crudBinding });
const { crudOptions } = createUserListCrudOptions({ expose: crudExpose }, customerId, overview);

// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
useCrud({ expose: crudExpose, crudOptions });

const refreshList = () => {
	crudExpose.doRefresh();
};

onMounted(() => {
	refreshList();
});
</script>
