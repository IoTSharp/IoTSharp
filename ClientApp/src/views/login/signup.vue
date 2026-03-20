<template>
	<div class="signup-page">
		<div class="signup-page__glow"></div>
		<div class="signup-shell">
			<section class="signup-showcase">
				<header class="signup-showcase__header">
					<RouterLink class="signup-showcase__brand" to="/">
						<AppLogo />
					</RouterLink>
					<RouterLink class="signup-showcase__back" to="/login">已有账号？去登录</RouterLink>
				</header>

				<div class="signup-showcase__body">
					<div class="signup-showcase__eyebrow">Create Workspace</div>
					<h1>创建你的 IoTSharp 工作空间</h1>
					<p>注册完成后即可获得租户、管理员账号与默认工作区，用于管理设备接入、业务规则和平台运营。</p>
				</div>

				<div class="signup-showcase__cards">
					<article v-for="item in showcaseCards" :key="item.title" class="signup-info-card">
						<div class="signup-info-card__title">{{ item.title }}</div>
						<strong>{{ item.value }}</strong>
						<p>{{ item.description }}</p>
					</article>
				</div>
			</section>

			<section class="signup-panel">
				<div class="signup-panel__header">
					<div class="signup-panel__eyebrow">Sign Up</div>
					<h2>注册账号</h2>
					<p>填写管理员、租户和联系信息。界面和登录页统一为同一套蓝白风格，方便作为正式首页和认证入口使用。</p>
				</div>

				<div class="signup-panel__form">
					<FormCreate ref="formRef" v-model:api="fApi" :rule="rules" :option="options" @submit="onSubmit"></FormCreate>
				</div>

				<div class="signup-panel__footer">
					<span>提交成功后将自动跳转到登录页。</span>
					<RouterLink to="/login">返回登录</RouterLink>
				</div>
			</section>
		</div>
	</div>
</template>

<script lang="ts" setup>
import { ref, type Ref } from 'vue';
import { useRouter, RouterLink } from 'vue-router';
import { ElMessage } from 'element-plus';
import formCreate, { Api } from '@form-create/element-ui';
import AppLogo from '/@/components/AppLogo.vue';
import { signup } from '/@/api/account';
import { signUpRule } from './signup_form_rules';
import { option } from './signup_form_option';

const router = useRouter();
const formRef = ref(null);
const options = ref(option);
const fApi: Ref<Api | null> = ref(null);
const FormCreate = formCreate.$form();

const showcaseCards = [
	{
		title: '统一接入',
		value: 'MQTT / HTTP',
		description: '连接设备、网关与第三方系统。',
	},
	{
		title: '多租户',
		value: 'Workspace',
		description: '一个注册流程直接创建租户环境。',
	},
	{
		title: '控制台',
		value: 'Blue UI',
		description: '登录后进入新的运营控制台布局。',
	},
];

const validatePassCheck = (rule: any, value: any, callback: any) => {
	if (value === '') {
		callback(new Error('请再次输入密码'));
	} else if (value !== fApi.value?.form.password) {
		callback(new Error('两次输入的密码不一致'));
	} else {
		callback();
	}
};

const rules = ref(
	signUpRule.map((item, index) => {
		if (index !== 3) return { ...item };
		return {
			...item,
			validate: [
				...(item.validate || []),
				{
					required: true,
					trigger: 'change',
					validator: validatePassCheck,
				},
			],
		};
	})
);

function onSubmit(data: any) {
	try {
		fApi.value?.validate(async (valid) => {
			if (valid) {
				try {
					await signup(data);
					ElMessage.success('注册成功');
					await router.replace({ name: 'login' });
				} catch (error: any) {
					ElMessage.error('注册失败');
					for (const item of Object.values(error?.response?.data?.errors || {})) {
						ElMessage.error(String(item));
					}
				}
			} else {
				ElMessage.error('请正确填写注册信息');
			}
		});
	} catch (error) {
		ElMessage.error('请正确填写注册信息');
	}
}
</script>

<style lang="scss" scoped>
.signup-page {
	position: relative;
	display: flex;
	align-items: center;
	justify-content: center;
	min-height: 100vh;
	padding: 24px;
	background:
		radial-gradient(circle at top left, rgba(96, 165, 250, 0.16), transparent 28%),
		radial-gradient(circle at bottom right, rgba(14, 165, 233, 0.18), transparent 26%),
		linear-gradient(180deg, #f4f9ff 0%, #eef6ff 48%, #fbfdff 100%);
	overflow: hidden;
}

.signup-page__glow {
	position: absolute;
	inset: 0;
	background-image:
		linear-gradient(rgba(148, 163, 184, 0.08) 1px, transparent 1px),
		linear-gradient(90deg, rgba(148, 163, 184, 0.08) 1px, transparent 1px);
	background-size: 46px 46px;
	mask-image: radial-gradient(circle at center, #000 45%, transparent 88%);
	pointer-events: none;
}

.signup-shell {
	position: relative;
	z-index: 1;
	display: grid;
	grid-template-columns: 1fr minmax(440px, 520px);
	width: min(1260px, 100%);
	min-height: min(820px, calc(100vh - 48px));
	border-radius: 34px;
	border: 1px solid rgba(255, 255, 255, 0.72);
	background: rgba(255, 255, 255, 0.42);
	box-shadow: 0 30px 80px rgba(15, 23, 42, 0.12);
	backdrop-filter: blur(20px);
	overflow: hidden;
}

.signup-showcase,
.signup-panel {
	padding: 36px 40px;
}

.signup-showcase {
	display: flex;
	flex-direction: column;
	justify-content: space-between;
	gap: 28px;
	background:
		radial-gradient(circle at top right, rgba(96, 165, 250, 0.22), transparent 26%),
		linear-gradient(160deg, #0f3463 0%, #13457a 46%, #155a93 100%);
	color: #eff6ff;

	:deep(.app-logo) {
		--app-logo-text: #ffffff;
		--app-logo-subtext: #bfdbfe;
	}
}

.signup-showcase__header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 14px;
}

.signup-showcase__brand,
.signup-showcase__back {
	text-decoration: none;
}

.signup-showcase__back {
	color: rgba(239, 246, 255, 0.9);
	font-size: 13px;
	font-weight: 600;
}

.signup-showcase__eyebrow,
.signup-panel__eyebrow {
	margin-bottom: 12px;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.signup-showcase__eyebrow {
	color: #93c5fd;
}

.signup-showcase__body {
	max-width: 620px;

	h1 {
		margin: 0 0 18px;
		font-size: clamp(38px, 5vw, 58px);
		line-height: 1;
		letter-spacing: -0.05em;
	}

	p {
		margin: 0;
		color: rgba(226, 232, 240, 0.86);
		font-size: 16px;
		line-height: 1.9;
	}
}

.signup-showcase__cards {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 16px;
}

.signup-info-card {
	padding: 18px;
	border-radius: 22px;
	background: rgba(255, 255, 255, 0.08);
	border: 1px solid rgba(255, 255, 255, 0.08);

	strong {
		display: block;
		margin-top: 12px;
		font-size: 26px;
		letter-spacing: -0.04em;
	}

	p {
		margin: 10px 0 0;
		color: rgba(226, 232, 240, 0.78);
		font-size: 13px;
		line-height: 1.7;
	}
}

.signup-info-card__title {
	color: rgba(191, 219, 254, 0.88);
	font-size: 13px;
}

.signup-panel {
	display: flex;
	flex-direction: column;
	justify-content: center;
	gap: 24px;
	background: rgba(255, 255, 255, 0.95);
}

.signup-panel__header {
	h2 {
		margin: 0 0 12px;
		color: #123b6d;
		font-size: 32px;
		letter-spacing: -0.04em;
	}

	p {
		margin: 0;
		color: #64748b;
		font-size: 14px;
		line-height: 1.85;
	}
}

.signup-panel__eyebrow {
	color: #2563eb;
}

.signup-panel__form {
	padding: 22px;
	border-radius: 26px;
	border: 1px solid rgba(226, 232, 240, 0.9);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.94), rgba(255, 255, 255, 0.98));
}

.signup-panel__footer {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
	padding-top: 18px;
	border-top: 1px solid rgba(226, 232, 240, 0.9);
	color: #64748b;
	font-size: 12px;

	a {
		color: #2563eb;
		text-decoration: none;
		font-weight: 600;
	}
}

:deep(.el-form-item) {
	margin-bottom: 16px;
}

:deep(.el-input__wrapper) {
	min-height: 46px;
	border-radius: 14px;
}

:deep(.el-button) {
	height: 48px;
	border-radius: 14px;
	box-shadow: 0 18px 32px rgba(37, 99, 235, 0.18);
}

:deep(.el-form-item__label) {
	display: none !important;
}

@media (max-width: 1080px) {
	.signup-shell {
		grid-template-columns: 1fr;
		max-width: 720px;
	}
}

@media (max-width: 767px) {
	.signup-page {
		padding: 0;
	}

	.signup-shell {
		min-height: 100vh;
		border-radius: 0;
	}

	.signup-showcase,
	.signup-panel {
		padding: 24px;
	}

	.signup-showcase__cards {
		grid-template-columns: 1fr;
	}

	.signup-panel__footer,
	.signup-showcase__header {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
