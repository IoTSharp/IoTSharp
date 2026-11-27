<template>
	<div class="customer-page console-list-page">
		<ConsoleCrudWorkspace
			fast-crud
			eyebrow="Customer Workspace"
			title="客户管理"
			description="管理租户下的客户，查看各客户的设备和用户。"
			card-eyebrow="Customer Table"
			card-title="客户列表"
			:summary="overview.lastRefresh ? `共 ${overview.total} 条` : undefined"
		>
			<template #actions>
				<el-button type="primary" @click="refreshList">刷新列表</el-button>
			</template>

			<fs-crud ref="crudRef" v-bind="crudBinding">
				<template #actionbar-right>
					<div class="console-crud-tags">
						<span class="console-crud-tag">当前页 {{ overview.pageCount }} 个</span>
						<span class="console-crud-tag is-primary">电话资料 {{ overview.phoneCount }} 个</span>
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

const { crudExpose } = useExpose({ crudRef, crudBinding });
const { crudOptions } = createCustomerListCrudOptions({ expose: crudExpose }, tenantId, overview);

// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
useCrud({ expose: crudExpose, crudOptions });

const refreshList = () => {
	crudExpose.doRefresh();
};

onMounted(() => {
	refreshList();
});
</script>
