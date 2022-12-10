<template>
	<div>
		<el-card>
			<div class="z-crud">
				<fs-crud ref="crudRef" v-bind="crudBinding">

					<template #actionbar-right>
						<el-divider direction="vertical" />
						<el-button  @click="openCustomForm">规则下发</el-button>
					</template>
				</fs-crud>
			</div>
		</el-card>
		<DeviceDetail ref="deviceDetailRef"></DeviceDetail>

<addRules ref="addRulesRef"></addRules>

	</div>
</template>

<script lang="ts" setup>
import { useCrud } from '@fast-crud/fast-crud';
import { useExpose } from '@fast-crud/fast-crud';
import DeviceDetail from './DeviceDetail.vue';
import addRules from './addRules.vue';
import { createDeviceCrudOptions } from '/@/views/iot/devices/deviceCrudOptions';
import { useRoute } from 'vue-router';
import { useUserInfo } from '/@/stores/userInfo';
import { storeToRefs } from 'pinia';


const selectedItems = ref([]);
const stores = useUserInfo();
const route = useRoute();
const { userInfos } = storeToRefs(stores);
// 设备详情 ref
const deviceDetailRef = ref();
// crud组件的ref
const crudRef = ref();
// crud 配置的ref
const crudBinding = ref();
const customerId = route.query.id || userInfos.value.customer.id;


// 规则下发 ref
const addRulesRef = ref();
// 暴露的方法
const { crudExpose } = useExpose({ crudRef, crudBinding });
// 你的crud配置
const { crudOptions } = createDeviceCrudOptions({ expose: crudExpose }, customerId, deviceDetailRef,addRulesRef,selectedItems );
// 初始化crud配置
// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
const { resetCrudOptions } = useCrud({ expose: crudExpose, crudOptions });
// 你可以调用此方法，重新初始化crud配置
// resetCrudOptions(options)

const openCustomForm=()=>{
	addRulesRef.value.openDialog(selectedItems.value);
}

// 页面打开后获取列表数据
onMounted(() => {
	crudExpose.doRefresh();
});
</script>
<style lang="scss" scoped>
.z-crud {
	height: calc(100vh - 160px);
}
</style>
