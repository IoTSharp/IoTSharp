<template>
	<el-card>
		<div class="z-crud">
			<fs-crud ref="crudRef" v-bind="crudBinding" />
		</div>
		<AssetDetail ref="assetDetailRef" />
	</el-card>
</template>

<script setup lang="ts">
import { useCrud } from '@fast-crud/fast-crud';
import { useExpose } from '@fast-crud/fast-crud';
import AssetDetail from './assetdetail.vue';
import { createAssetListCrudOptions } from './crudOptions/assetListCrudOptions';
const crudRef = ref();
const crudBinding = ref();
const assetDetailRef = ref();
const { crudExpose } = useExpose({ crudRef, crudBinding });
const { crudOptions } = createAssetListCrudOptions({ expose: crudExpose }, assetDetailRef);
// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
useCrud({ crudExpose, crudOptions });
onMounted(() => {
	crudExpose.doRefresh();
});
</script>

<style scoped lang="scss">
.z-crud {
	height: calc(100vh - 160px);
}
</style>
