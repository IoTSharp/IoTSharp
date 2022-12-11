<template>
	<div>
		<el-drawer v-model="drawer" :title="dialogTitle" size="70%">
			<el-tabs v-model="activeTabName" class="demo-tabs" stretch>
				<el-tab-pane label="详情" name="basic">
					<div class="z-tab-container">
						<AdvancedKeyValue :obj="assetRef" :config="columns" :label-width="160"></AdvancedKeyValue>
					</div>
				</el-tab-pane>
				<el-tab-pane label="属性" name="props">
					<div class="z-tab-container">
						<AssetDetailProps :assetId="assetRef.id"></AssetDetailProps>
					</div>
				</el-tab-pane>
				<el-tab-pane label="遥测" name="telemetry">
					<div class="z-tab-container">
						<AssetDetailTelemetry :assetId="assetRef.id"></AssetDetailTelemetry>
					</div>
				</el-tab-pane>
			</el-tabs>
		</el-drawer>
	</div>
</template>
<script lang="ts" setup>
import { createAssetListCrudOptions } from './crudOptions/assetListCrudOptions';
import AdvancedKeyValue from '/@/components/AdvancedKeyValue/AdvancedKeyValue.vue';
import AssetDetailProps from '/@/views/iot/assets/detail/AssetDetailProps.vue';
import AssetDetailTelemetry from '/@/views/iot/assets/detail/AssetDetailTelemetry.vue';

const drawer = ref(false);
const dialogTitle = ref(`资产详情`);
const activeTabName = ref('basic');
const assetRef = ref();
const {
	crudOptions: { columns },
} = createAssetListCrudOptions({ expose: null });
const openDialog = (device: any) => {
	drawer.value = true;
	assetRef.value = device;
	dialogTitle.value = `${assetRef.value.name}`;
};
defineExpose({
	openDialog,
});
</script>
<style lang="scss" scoped>
.z-tab-container {
	padding: 0 16px 16px;
}
</style>
