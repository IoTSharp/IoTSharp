<template>
	<el-card>
		<div class="z-crud">
			<fs-crud ref="crudRef" v-bind="crudBinding" />
		</div>
	</el-card>
</template>

<script setup lang="ts">
import { useCrud } from '@fast-crud/fast-crud';
import { useExpose } from '@fast-crud/fast-crud';
import { createUserListCrudOptions } from './crudOptions/userListCrudOptions';
import { storeToRefs } from 'pinia';
import { useUserInfo } from '/@/stores/userInfo';
import { useRoute } from 'vue-router';
const stores = useUserInfo();
const route = useRoute();
const { userInfos } = storeToRefs(stores);
const crudRef = ref();
const crudBinding = ref();
const { crudExpose } = useExpose({ crudRef, crudBinding });
const customerId = route.query.id || userInfos.value.customer.id;
const { crudOptions } = createUserListCrudOptions({ expose: crudExpose }, customerId);
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
