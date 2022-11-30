<template>
	<el-card class="box-card">
		<div class="text item" v-if="form?.enableTls && !form?.caCertificate">
			<el-button type="primary" plain @click="initCertificate">初始化根证书</el-button>
		</div>
		<div class="text item"><span class="label">域名: </span>{{ form.domain }}</div>
		<div class="text item"><span class="label">Ca指纹: </span>{{ form.caThumbprint }}</div>
		<div class="text item"><span class="label">Broker指纹: </span> {{ form.brokerThumbprint }}</div>
	</el-card>
</template>

<script lang="ts">
import { reactive, toRefs, onMounted, defineComponent } from 'vue';
import { getSysInfo, initSysCertificate } from '/@/api/installer';
export default defineComponent({
	name: 'certification',
	components: {},
	setup() {
		const state = reactive({
			form: {
				domain: '',
				caThumbprint: '',
				brokerThumbprint: '',
				caCertificate: '',
				enableTls: false,
				installed: false,
				version: '',
			},
		});
		const getData = () => {
			getSysInfo().then((res) => {
				state.form = res.data;
			});
		};
		const initCertificate = () => {
			initSysCertificate().then(() => {
				getData();
			});
		};
		onMounted(() => {
			getData();
		});
		return {
			...toRefs(state),
			initCertificate,
		};
	},
});
</script>

<style scoped>
.text {
	font-size: 14px;
}

.item {
	padding: 15px 0;
}

.label {
	width: 80px;
	display: inline-block;
	text-align: right;
	margin-right: 10px;
}
.box-card {
	width: 100%;
}
</style>
