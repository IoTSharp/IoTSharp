<template>
	<ConsoleCrudWorkspace eyebrow="" title="租户设置" description="" card-eyebrow="" card-title="租户资料">
		<el-form v-if="tenant" v-loading="saving" class="settings-form" :model="tenant" label-position="top" @submit.prevent="save">
			<el-form-item label="租户名称" required>
				<el-input v-model.trim="tenant.name" maxlength="100" required />
			</el-form-item>
			<el-form-item label="租户联系邮箱">
				<el-input v-model.trim="tenant.email" type="email" maxlength="254" />
			</el-form-item>
			<el-form-item label="联系电话">
				<el-input v-model.trim="tenant.phone" type="tel" />
			</el-form-item>
			<el-form-item label="国家或地区"><el-input v-model.trim="tenant.country" /></el-form-item>
			<el-form-item label="省份"><el-input v-model.trim="tenant.province" /></el-form-item>
			<el-form-item label="城市"><el-input v-model.trim="tenant.city" /></el-form-item>
			<el-form-item label="街道"><el-input v-model.trim="tenant.street" /></el-form-item>
			<el-form-item label="详细地址"><el-input v-model.trim="tenant.address" /></el-form-item>
			<el-form-item label="邮政编码"><el-input-number v-model="tenant.zipCode" :min="0" :max="999999999" :controls="false" /></el-form-item>
			<div class="settings-form__actions">
				<el-button type="primary" native-type="submit" :loading="saving">保存租户设置</el-button>
			</div>
		</el-form>
		<el-skeleton v-else-if="loading" :rows="3" animated />
		<el-button v-else @click="load">重新加载</el-button>
	</ConsoleCrudWorkspace>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { ElMessage } from 'element-plus';
import ConsoleCrudWorkspace from '/@/components/console/ConsoleCrudWorkspace.vue';
import request from '/@/utils/request';

interface TenantProfile {
	id: string;
	name: string;
	email: string;
	phone: string;
	country: string;
	province: string;
	city: string;
	street: string;
	address: string;
	zipCode: number;
}
const tenant = ref<TenantProfile>();
const loading = ref(false);
const saving = ref(false);
const endpoint = '/api/Tenants/current';

async function load() {
	loading.value = true;
	try {
		tenant.value = (await request.get<unknown, { data: TenantProfile }>(endpoint)).data;
	} catch {
		// 请求通知由公共客户端处理。
	} finally {
		loading.value = false;
	}
}

async function save() {
	if (!tenant.value || saving.value) return;
	saving.value = true;
	try {
		tenant.value = (await request.put<unknown, { data: TenantProfile }>(endpoint, tenant.value)).data;
		ElMessage.success('租户设置已保存');
	} catch {
		// 请求通知由公共客户端处理。
	} finally {
		saving.value = false;
	}
}

onMounted(load);
</script>

<style scoped>
.settings-form {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	column-gap: 16px;
	max-width: 720px;
}
.settings-form__actions {
	grid-column: 1 / -1;
}
@media (max-width: 640px) {
	.settings-form { grid-template-columns: 1fr; }
}
</style>
