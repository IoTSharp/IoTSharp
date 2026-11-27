<template>
	<ConsoleCrudWorkspace eyebrow="" title="系统配置" description="" card-eyebrow="" card-title="平台邮件发送"
		card-description="用于发送租户申请邮箱验证码。">
		<el-form v-if="loaded" class="email-form" :model="form" label-position="top" @submit.prevent="save">
			<el-form-item label="SMTP 服务器" required><el-input v-model.trim="form.host" maxlength="253" required /></el-form-item>
			<el-form-item label="端口" required><el-input-number v-model="form.port" :min="1" :max="65535" :controls="false" /></el-form-item>
			<el-form-item label="加密方式">
				<el-radio-group v-model="form.implicitTls">
					<el-radio :value="true">SSL/TLS</el-radio>
					<el-radio :value="false">STARTTLS</el-radio>
				</el-radio-group>
			</el-form-item>
			<el-form-item label="SMTP 账号" required><el-input v-model.trim="form.username" maxlength="254" required /></el-form-item>
			<el-form-item label="发件邮箱" required><el-input v-model.trim="form.senderAddress" type="email" maxlength="254" required /></el-form-item>
			<el-form-item label="SMTP 授权码" :required="!hasPassword">
				<el-input v-model="form.password" type="password" autocomplete="new-password" maxlength="1024"
					:required="!hasPassword" :placeholder="hasPassword ? '已配置，留空沿用原授权码' : '请输入 SMTP 授权码'" />
			</el-form-item>
			<el-button type="primary" native-type="submit" :loading="saving">保存邮件配置</el-button>
		</el-form>
		<el-skeleton v-else-if="loading" :rows="3" animated />
		<el-button v-else @click="load">重新加载</el-button>
	</ConsoleCrudWorkspace>
</template>

<script setup lang="ts">
import { onMounted, reactive, ref } from 'vue';
import { ElMessage } from 'element-plus';
import ConsoleCrudWorkspace from '/@/components/console/ConsoleCrudWorkspace.vue';
import request from '/@/utils/request';

const form = reactive({ host: '', port: 465, implicitTls: true, username: '', senderAddress: '', password: '' });
type EmailSettings = Omit<typeof form, 'password'> & { hasPassword: boolean };
const endpoint = '/api/SystemSettings/email';
const hasPassword = ref(false);
const loaded = ref(false);
const loading = ref(false);
const saving = ref(false);

function setForm(settings: EmailSettings) {
	const { hasPassword: configured, ...fields } = settings;
	Object.assign(form, fields, { password: '' });
	hasPassword.value = configured;
}

async function load() {
	loading.value = true;
	try {
		const { data } = await request.get<unknown, { data: EmailSettings }>(endpoint);
		setForm(data);
		loaded.value = true;
	} catch {
		// 请求通知由公共客户端处理。
	} finally {
		loading.value = false;
	}
}

async function save() {
	if (saving.value) return;
	if (!form.port) {
		ElMessage.error('请输入 SMTP 端口');
		return;
	}
	saving.value = true;
	try {
		const { data } = await request.put<unknown, { data: EmailSettings }>(endpoint, { ...form });
		setForm(data);
		ElMessage.success('邮件配置已保存');
	} catch {
		// 请求通知由公共客户端处理。
	} finally {
		saving.value = false;
	}
}

onMounted(load);
</script>

<style scoped>
.email-form {
	max-width: 480px;
}
</style>
