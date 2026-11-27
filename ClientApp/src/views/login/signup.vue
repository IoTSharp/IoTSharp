<template>
	<main class="tenant-application">
		<div class="tenant-application__card">
			<header>
				<RouterLink to="/" aria-label="IoTSharp 首页"><AppLogo /></RouterLink>
				<RouterLink to="/login">返回登录</RouterLink>
			</header>
			<h1>申请创建租户</h1>
			<p>验证邮箱后即可开通租户。开通后，租户管理员在控制台创建内部账号。</p>
			<el-form ref="formRef" :model="form" :rules="rules" label-position="top" @submit.prevent="submit">
				<el-form-item label="租户名称" prop="tenantName">
					<el-input v-model.trim="form.tenantName" autocomplete="organization" maxlength="100" />
				</el-form-item>
				<el-form-item label="租户管理员邮箱" prop="email">
					<el-input v-model.trim="form.email" autocomplete="email" inputmode="email" maxlength="254" />
				</el-form-item>
				<el-form-item label="邮箱验证码" prop="verificationCode">
					<div class="tenant-application__code">
						<el-input v-model.trim="form.verificationCode" autocomplete="one-time-code" inputmode="numeric" maxlength="6" />
						<el-button :disabled="!!resendTimer || submitting" :loading="sending" @click="sendCode">
							{{ resendTimer ? '验证码已发送' : '发送验证码' }}
						</el-button>
					</div>
				</el-form-item>
				<el-form-item label="密码" prop="password">
					<el-input v-model="form.password" type="password" show-password autocomplete="new-password" maxlength="100"
						placeholder="至少 6 位，含大小写字母、数字和特殊字符" />
				</el-form-item>
				<el-form-item label="手机号" prop="phoneNumber">
					<el-input v-model.trim="form.phoneNumber" autocomplete="tel" inputmode="tel" maxlength="16" />
				</el-form-item>
				<el-button class="tenant-application__submit" type="primary" native-type="submit" :disabled="sending" :loading="submitting">
					申请创建租户
				</el-button>
			</el-form>
		</div>
	</main>
</template>

<script setup lang="ts">
import { onBeforeUnmount, reactive, ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { ElMessage, type FormInstance, type FormRules } from 'element-plus';
import AppLogo from '/@/components/AppLogo.vue';
import { sendTenantVerificationCode, signup } from '/@/api/account';

const router = useRouter();
const formRef = ref<FormInstance>();
const sending = ref(false);
const submitting = ref(false);
const resendTimer = ref<number>();
const form = reactive({ tenantName: '', email: '', verificationCode: '', password: '', phoneNumber: '' });
const rules: FormRules<typeof form> = {
	tenantName: [{ required: true, message: '请输入租户名称', trigger: 'blur' }],
	email: [{ required: true, type: 'email', message: '请输入有效的邮箱地址', trigger: 'blur' }],
	verificationCode: [{ required: true, pattern: /^\d{6}$/, message: '请输入 6 位邮箱验证码', trigger: 'blur' }],
	password: [{ required: true, min: 6, message: '请输入至少 6 位密码', trigger: 'blur' }],
	phoneNumber: [{ required: true, pattern: /^\+?\d{7,15}$/, message: '请输入有效的手机号', trigger: 'blur' }],
};

onBeforeUnmount(() => window.clearTimeout(resendTimer.value));

async function sendCode() {
	if (sending.value || resendTimer.value || !await formRef.value?.validateField('email').catch(() => false)) return;
	sending.value = true;
	try {
		const result = await sendTenantVerificationCode(form.email);
		resendTimer.value = window.setTimeout(() => { resendTimer.value = undefined; }, result.data * 1000);
		ElMessage.success('验证码已发送，10 分钟内有效');
	} catch {
		// 请求通知由公共客户端处理。
	} finally {
		sending.value = false;
	}
}

async function submit() {
	if (submitting.value || sending.value || !await formRef.value?.validate().catch(() => false)) return;
	submitting.value = true;
	try {
		await signup({ ...form });
		ElMessage.success('租户已开通，请使用管理员邮箱登录');
		await router.replace({ name: 'login' });
	} catch {
		// 请求通知由公共客户端处理。
	} finally {
		submitting.value = false;
	}
}
</script>

<style scoped>
.tenant-application {
	display: grid;
	place-items: center;
	min-height: 100dvh;
	padding: 24px 16px;
	background: var(--iotsharp-quiet-gradient);
}
.tenant-application__card {
	width: min(100%, 480px);
	padding: clamp(22px, 5vw, 36px);
	border: 1px solid var(--iotsharp-border);
	border-radius: var(--iotsharp-radius-panel);
	background: var(--iotsharp-surface);
	box-shadow: var(--iotsharp-shadow-panel);
}
header, .tenant-application__code {
	display: flex;
	align-items: center;
	justify-content: space-between;
	width: 100%;
	gap: 10px;
}
header a {
	color: var(--iotsharp-accent);
	text-decoration: none;
}
h1 {
	margin: 24px 0 8px;
	color: var(--iotsharp-ink);
	font-size: 26px;
}
p {
	margin: 0 0 22px;
	color: var(--iotsharp-text-muted);
	line-height: 1.6;
}
.tenant-application__submit {
	width: 100%;
	min-height: 40px;
	margin-top: 4px;
}
</style>
