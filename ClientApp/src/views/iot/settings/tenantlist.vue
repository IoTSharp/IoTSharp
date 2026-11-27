<template>
	<div class="tenant-page console-list-page">
		<ConsoleCrudWorkspace
			fast-crud
			eyebrow="Tenant Workspace"
			title="租户管理"
			description="管理租户资料，并进入租户的客户清单。"
			card-eyebrow="Tenant Table"
			card-title="租户列表"
			:summary="overview.lastRefresh ? `共 ${overview.total} 条` : undefined"
		>
			<template #actions>
				<el-button type="primary" @click="refreshList">刷新列表</el-button>
			</template>

			<fs-crud ref="crudRef" v-bind="crudBinding">
				<template #actionbar-right>
					<div class="console-crud-tags">
						<span class="console-crud-tag">当前页 {{ overview.pageCount }} 个</span>
						<span class="console-crud-tag is-primary">邮箱资料 {{ overview.emailCount }} 个</span>
					</div>
				</template>
			</fs-crud>
		</ConsoleCrudWorkspace>
	</div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
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

const refreshList = () => {
	crudExpose.doRefresh();
};

onMounted(() => {
	refreshList();
});
</script>
