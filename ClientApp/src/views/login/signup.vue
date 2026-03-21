<template>
	<div class="signup-page">
		<div class="signup-page__glow"></div>
		<div class="signup-shell">
			<AuthShowcase
				eyebrow="Create Workspace"
				title="创建你的 IoTSharp 工作空间"
				description="注册完成后即可获得租户、管理员账号与默认工作区，用于管理设备接入、业务规则和平台运营。"
				link-to="/login"
				link-label="已有账号？去登录"
				:primary-card="showcasePrimaryCard"
				:metrics="showcaseMetrics"
				:tags="showcaseTags"
			/>

			<section class="signup-panel">
				<div class="signup-panel__header">
					<div class="signup-panel__eyebrow">Sign Up</div>
					<h2>注册账号</h2>
					<p>填写管理员、租户和联系信息。注册成功后会自动跳转到登录页，进入新的控制台工作台。</p>
				</div>

				<div class="signup-panel__steps">
					<div v-for="item in setupSteps" :key="item.label" class="signup-step">
						<span>{{ item.label }}</span>
						<strong>{{ item.title }}</strong>
						<small>{{ item.description }}</small>
					</div>
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
import { signup } from '/@/api/account';
import AuthShowcase from '/@/views/login/component/AuthShowcase.vue';
import { signUpRule } from './signup_form_rules';
import { option } from './signup_form_option';

const router = useRouter();
const formRef = ref(null);
const options = ref(option);
const fApi: Ref<Api | null> = ref(null);
const FormCreate = formCreate.$form();

const showcasePrimaryCard = {
	label: 'Workspace Setup',
	value: 'Tenant + Admin',
	title: '一次注册完成基础工作区初始化',
	description: '创建租户、管理员与默认工作区，为后续设备接入和规则配置准备好基础环境。',
};

const showcaseMetrics = [
	{
		label: '接入方式',
		value: 'MQTT / HTTP',
		description: '后续可继续接入设备、网关和第三方系统。',
		tone: 'accent' as const,
	},
	{
		label: '租户模型',
		value: 'Workspace',
		description: '通过注册流程直接创建独立的管理空间。',
		tone: 'primary' as const,
	},
	{
		label: '控制台',
		value: 'Blue UI',
		description: '注册完成后进入统一的蓝白控制台体验。',
		tone: 'success' as const,
	},
];

const showcaseTags = ['租户初始化', '管理员账号', '控制台工作台', '设备运营'];

const setupSteps = [
	{ label: '01', title: '填写租户信息', description: '先确定管理员和租户基础资料，方便后续隔离管理。' },
	{ label: '02', title: '设置登录密码', description: '注册时完成密码配置，成功后即可直接去登录页验证。' },
	{ label: '03', title: '进入控制台', description: '登录后先查看工作台首页，再继续设备接入和规则编排。' },
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

.signup-panel {
	display: flex;
	flex-direction: column;
	justify-content: center;
	gap: 24px;
	padding: 36px 40px;
	background: rgba(255, 255, 255, 0.95);
}

.signup-panel__eyebrow {
	margin-bottom: 12px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.signup-panel__header h2 {
	margin: 0 0 12px;
	color: #123b6d;
	font-size: 32px;
	letter-spacing: -0.04em;
}

.signup-panel__header p {
	margin: 0;
	color: #64748b;
	font-size: 14px;
	line-height: 1.85;
}

.signup-panel__steps {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 12px;
}

.signup-step {
	padding: 14px 16px;
	border-radius: 18px;
	border: 1px solid rgba(226, 232, 240, 0.9);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(255, 255, 255, 0.98));
}

.signup-step span {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	width: 30px;
	height: 30px;
	border-radius: 999px;
	background: rgba(37, 99, 235, 0.08);
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
}

.signup-step strong {
	display: block;
	margin-top: 12px;
	color: #123b6d;
	font-size: 15px;
	font-weight: 700;
}

.signup-step small {
	display: block;
	margin-top: 6px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
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
}

.signup-panel__footer a {
	color: #2563eb;
	text-decoration: none;
	font-weight: 600;
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

	.signup-panel {
		padding: 24px;
	}

	.signup-panel__steps {
		grid-template-columns: 1fr;
	}

	.signup-panel__footer {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
