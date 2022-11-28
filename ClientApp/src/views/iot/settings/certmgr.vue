<template>
	<el-form :model="form" label-width="120px">
		<el-form-item label="域名">
			<el-input v-model="form.domain" />
		</el-form-item>
		<el-form-item label="Ca指纹">
			<el-input v-model="form.caThumbprint" />
		</el-form-item>
		<el-form-item label="Broker指纹">
			<el-input v-model="form.brokerThumbprint" />
		</el-form-item>
	</el-form>
</template>

<script lang="ts">
import { reactive, toRefs, onMounted, defineComponent } from 'vue';
import { getSysInfo } from '/@/api/installer';
export default defineComponent({
	name: 'certification',
	components: {},
	setup() {
		const state = reactive({
			form: {
				domain: '',
				caThumbprint: '',
				brokerThumbprint: '',
			},
		});
		const getData = () => {
			getSysInfo().then((res) => {
				state.form = res.data;
			});
		};
		onMounted(() => {
			getData();
		});
		return {
			...toRefs(state),
		};
	},
});
</script>
