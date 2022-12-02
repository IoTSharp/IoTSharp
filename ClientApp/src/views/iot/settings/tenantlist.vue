<template>
	<div class="z-crud">
		<fs-crud ref="crudRef" v-bind="crudBinding" />
	</div>
</template>

<script lang="ts">
import { useCrud } from '@fast-crud/fast-crud';
import { useExpose } from '@fast-crud/fast-crud';
import { createTenantListCrudOptions } from './crudOptions/tenantListCrudOptions';
export default defineComponent({
	name: 'TenantList',
	setup() {
		const crudRef = ref();
		const crudBinding = ref();
		const { crudExpose } = useExpose({ crudRef, crudBinding });
		// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
		const { crudOptions } = createTenantListCrudOptions({ crudExpose });
		// eslint-disable-next-line @typescript-eslint/no-unused-vars,no-unused-vars
		useCrud({ crudExpose, crudOptions });
		onMounted(() => {
			crudExpose.doRefresh();
		});
		return {
			crudBinding,
			crudRef,
		};
	},
});
</script>

<style scoped lang="scss">
.z-crud {
	height: calc(100vh - 160px);
}
</style>
