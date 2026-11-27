<template>
	<div class="asset-page console-list-page">
		<ConsoleCrudWorkspace
			fast-crud
			eyebrow="Asset Workspace"
			title="资产管理"
			description="按名称查找资产，管理资产资料。"
			card-eyebrow="Asset Table"
			card-title="资产列表"
			:summary="overview.lastRefresh ? `共 ${overview.total} 条` : undefined"
		>
			<template #actions>
				<el-button type="primary" @click="refreshList">刷新列表</el-button>
			</template>

			<fs-crud ref="crudRef" v-bind="crudBinding">
				<template #actionbar-right>
					<div class="console-crud-tags">
						<span class="console-crud-tag">当前页 {{ overview.pageCount }} 个</span>
						<span class="console-crud-tag is-primary">类型 {{ overview.typeCount }} 类</span>
					</div>
				</template>
			</fs-crud>
		</ConsoleCrudWorkspace>

		<AssetDetail ref="assetDetailRef" />
	</div>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
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

const refreshList = () => {
	crudExpose.doRefresh();
};

onMounted(() => {
	refreshList();
});
</script>
