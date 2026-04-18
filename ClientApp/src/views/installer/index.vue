<template>
	<div class="installer-page">
		<div class="installer-page__glow"></div>
		<div class="installer-shell">
			<AuthShowcase
				eyebrow="Initialize Workspace"
				:title="showcaseTitle"
				:description="showcaseDescription"
				link-to="/"
				link-label="返回首页"
				:primary-card="showcasePrimaryCard"
				:metrics="showcaseMetrics"
				:tags="showcaseTags"
			/>

			<section class="installer-panel">
				<div class="installer-panel__header">
					<div class="installer-panel__eyebrow">Installer</div>
					<h2>{{ isInstalled ? '系统已完成初始化' : '安装向导' }}</h2>
					<p>
						{{
							isInstalled
								? '当前实例已经完成首次初始化，你可以直接进入登录页面继续使用新的控制台工作台。'
								: '填写管理员账号、邮箱与密码，完成 IoTSharp 的首次初始化，让登录入口、控制台与设备管理流程保持一致。'
						}}
					</p>
				</div>

				<div class="installer-panel__steps">
					<article v-for="item in setupSteps" :key="item.label" class="installer-step">
						<span>{{ item.label }}</span>
						<strong>{{ item.title }}</strong>
						<small>{{ item.description }}</small>
					</article>
				</div>

				<div v-if="isInstalled" class="installer-status">
					<div class="installer-status__head">
						<div>
							<div class="installer-status__title">IoTSharp 已完成安装</div>
							<p>基础配置、管理员入口与控制台访问路径已经就绪，现在可以直接进入登录页。</p>
						</div>
						<span class="installer-status__badge">Ready</span>
					</div>

					<div class="installer-status__grid">
						<div class="installer-status__item">
							<span>当前版本</span>
							<strong>{{ versionText }}</strong>
						</div>
						<div class="installer-status__item">
							<span>下一步</span>
							<strong>登录控制台</strong>
						</div>
					</div>

					<div class="installer-status__actions">
						<Button type="primary" class="installer-status__action" @click="router.replace({ name: 'login' })">
							前往登录
						</Button>
						<RouterLink class="installer-status__link" to="/">返回首页</RouterLink>
					</div>
				</div>

				<div v-else class="installer-form-card">
					<div class="installer-form-card__tip">
						<div class="installer-form-card__tip-title">首次部署</div>
						<div class="installer-form-card__tip-text">
							安装完成后，使用这里配置的管理员账号登录控制台；如果你已经注册过租户，可继续沿用同一套蓝白工作台体验。
						</div>
					</div>

					<FormCreate
						v-model:api="fApi"
						v-model="value"
						:rule="rules"
						:option="options"
						@submit="onSubmit"
					></FormCreate>

					<div class="installer-form-card__actions">
						<Button type="primary" class="installer-form-card__action" :loading="isSubmitting" @click="submitInstall">
							开始安装
						</Button>
					</div>
				</div>

				<div class="installer-panel__footer">
					<div>版本 {{ versionText }}</div>
					<div>{{ currentYear }} {{ pageTitle }}</div>
				</div>
			</section>
		</div>
	</div>
</template>

<script setup lang="ts">
import { computed, reactive, ref, type Ref, onMounted } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import formCreate, { type Api } from '/@/utils/formCreate';
import { ElButton as Button, ElMessage } from 'element-plus';
import { storeToRefs } from 'pinia';
import { initSysAdmin } from '/@/api/installer';
import { useAppInfo } from '/@/stores/appInfo';
import { useThemeConfig } from '/@/stores/themeConfig';
import { NextLoading } from '/@/utils/loading';
import AuthShowcase from '/@/views/login/component/AuthShowcase.vue';
import { installerFormRule } from './installer_form_rules';
import installerFormOption from './installer_form_option.json';

const FormCreate = formCreate.$form();
const router = useRouter();
const storesAppInfo = useAppInfo();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);

const fApi: Ref<Api | null> = ref(null);
const value = ref({});
const options = reactive(installerFormOption);
const isSubmitting = ref(false);
const pageTitle = computed(() => themeConfig.value.globalTitle || 'IoTSharp');
const currentYear = new Date().getFullYear();

const isInstalled = computed(() => Boolean(storesAppInfo.appInfo.installed));
const versionText = computed(() => storesAppInfo.appInfo.version || 'Latest');
const showcaseTitle = computed(() =>
	isInstalled.value ? `${pageTitle.value} 已完成初始化` : `完成 ${pageTitle.value} 首次初始化`
);
const showcaseDescription = computed(() =>
	isInstalled.value
		? '初始化已经完成，平台入口、管理员账号与控制台访问路径都已就绪。你可以从这里直接返回登录页，进入新的工作台布局。'
		: '把首次安装流程放进统一的未登录体验里，先完成管理员初始化，再进入登录入口和控制台，保持产品入口、认证与后台风格一致。'
);

const showcasePrimaryCard = computed(() => ({
	label: 'Bootstrap',
	value: isInstalled.value ? 'Ready' : '1 Form',
	title: isInstalled.value ? '当前实例已经可以直接进入登录流程' : '一次初始化完成管理员入口与平台基础配置',
	description: isInstalled.value
		? '系统已经写入基础配置，可直接登录查看新的控制台工作台、设备接入与平台概览。'
		: '填写管理员信息后，系统会完成首次部署所需的基础数据初始化，减少第一次进入后台前的额外配置。',
}));

const showcaseMetrics = computed(() => [
	{
		label: '初始化状态',
		value: isInstalled.value ? 'Completed' : 'Pending',
		description: isInstalled.value ? '当前实例已完成首次部署，可直接进入登录页。' : '安装完成后会自动转入控制台登录入口。',
		tone: 'primary' as const,
	},
	{
		label: '管理员入口',
		value: 'Admin',
		description: '首次安装创建的管理员账号，将用于后续登录控制台与系统配置。',
		tone: 'accent' as const,
	},
	{
		label: '当前版本',
		value: versionText.value,
		description: '安装页会同步显示当前实例版本，便于核对部署状态。',
		tone: 'success' as const,
	},
]);

const showcaseTags = ['首次初始化', '管理员入口', '统一认证体验', '控制台工作台'];

const setupSteps = [
	{ label: '01', title: '配置管理员账号', description: '设置用于登录控制台的管理员邮箱、账号与密码。' },
	{ label: '02', title: '完成系统初始化', description: '提交后自动写入基础配置，准备好平台入口与工作台。' },
	{ label: '03', title: '进入控制台', description: '安装完成后直接跳转登录页，继续进入新的后台布局。' },
];

const rules = ref(JSON.parse(JSON.stringify(installerFormRule)));

const validatePassCheck = (rule: unknown, fieldValue: string, callback: (error?: Error) => void) => {
	if (fieldValue === '') {
		callback(new Error('请再次输入密码'));
	} else if (fieldValue !== fApi.value?.form.password) {
		callback(new Error('两次输入的密码不一致'));
	} else {
		callback();
	}
};

rules.value[3].validate?.push({
	required: true,
	trigger: 'change',
	validator: validatePassCheck,
});

const onSubmit = async (data: unknown) => {
   if (isSubmitting.value) {
		return;
	}

	try {
        isSubmitting.value = true;
		fApi.value?.validate(async (valid: boolean) => {
			if (!valid) {
               isSubmitting.value = false;
				ElMessage.error('请正确填写安装信息');
				return;
			}

			try {
				await initSysAdmin(data);
				ElMessage.success('安装完成，正在前往登录页');
				await router.replace({ name: 'login' });
			} catch (error) {
				ElMessage.error('安装失败，请稍后重试');
           } finally {
				isSubmitting.value = false;
			}
		});
	} catch (error) {
       isSubmitting.value = false;
		ElMessage.error('请正确填写安装信息');
	}
};

const submitInstall = async () => {
	await onSubmit(fApi.value?.form ?? value.value);
};

onMounted(() => {
	NextLoading.done();
});
</script>

<style scoped lang="scss">
.installer-page {
	position: relative;
	display: flex;
	align-items: flex-start;
	justify-content: center;
	width: 100%;
	height: 100%;
	min-height: 100%;
	padding: 24px;
	background:
		radial-gradient(circle at top left, rgba(96, 165, 250, 0.16), transparent 28%),
		radial-gradient(circle at bottom right, rgba(14, 165, 233, 0.18), transparent 28%),
		linear-gradient(180deg, #f3f8ff 0%, #eef6ff 46%, #fbfdff 100%);
	overflow-x: hidden;
	overflow-y: auto;

	:deep(.app-logo) {
		--app-logo-text: #ffffff;
		--app-logo-subtext: rgba(191, 219, 254, 0.9);
	}
}

.installer-page__glow {
	position: absolute;
	inset: 0;
	background:
		linear-gradient(rgba(148, 163, 184, 0.08) 1px, transparent 1px),
		linear-gradient(90deg, rgba(148, 163, 184, 0.08) 1px, transparent 1px);
	background-size: 48px 48px;
	mask-image: radial-gradient(circle at center, #000 42%, transparent 88%);
	pointer-events: none;
}

.installer-shell {
	position: relative;
	z-index: 1;
	display: grid;
	grid-template-columns: 1.08fr minmax(420px, 500px);
	width: min(1260px, 100%);
	min-height: calc(100% - 48px);
	border-radius: 34px;
	border: 1px solid rgba(255, 255, 255, 0.72);
	background: rgba(255, 255, 255, 0.42);
	box-shadow: 0 30px 80px rgba(15, 23, 42, 0.12);
	backdrop-filter: blur(20px);
	overflow: visible;
}

.installer-panel {
	display: flex;
	flex-direction: column;
    justify-content: flex-start;
	gap: 24px;
	padding: 36px 40px;
	background: rgba(255, 255, 255, 0.95);
}

.installer-panel__eyebrow {
	margin-bottom: 12px;
	color: #2563eb;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.installer-panel__header h2 {
	margin: 0 0 12px;
	color: #123b6d;
	font-size: 32px;
	letter-spacing: -0.04em;
}

.installer-panel__header p {
	margin: 0;
	color: #64748b;
	font-size: 14px;
	line-height: 1.85;
}

.installer-panel__steps {
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 12px;
}

.installer-step {
	padding: 14px 16px;
	border-radius: 18px;
	border: 1px solid rgba(226, 232, 240, 0.9);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.96), rgba(255, 255, 255, 0.98));
}

.installer-step span {
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

.installer-step strong {
	display: block;
	margin-top: 12px;
	color: #123b6d;
	font-size: 15px;
	font-weight: 700;
}

.installer-step small {
	display: block;
	margin-top: 6px;
	color: #7c8da1;
	font-size: 12px;
	line-height: 1.6;
}

.installer-form-card,
.installer-status {
	padding: 22px;
	border-radius: 26px;
	border: 1px solid rgba(226, 232, 240, 0.9);
	background: linear-gradient(180deg, rgba(248, 251, 255, 0.94), rgba(255, 255, 255, 0.98));
}

.installer-form-card__tip {
	margin-bottom: 20px;
	padding: 16px 18px;
	border-radius: 20px;
	border: 1px solid rgba(37, 99, 235, 0.12);
	background: linear-gradient(135deg, rgba(37, 99, 235, 0.08), rgba(14, 165, 233, 0.08));
}

.installer-form-card__actions {
	margin-top: 20px;
}

.installer-form-card__action {
	width: 100%;
	height: 48px;
	border-radius: 14px;
	letter-spacing: 0.08em;
	font-weight: 600;
	box-shadow: 0 18px 32px rgba(37, 99, 235, 0.18);
}

.installer-form-card__tip-title,
.installer-status__title {
	color: #123b6d;
	font-size: 15px;
	font-weight: 700;
}

.installer-form-card__tip-text,
.installer-status p {
	margin-top: 6px;
	color: #64748b;
	font-size: 13px;
	line-height: 1.75;
}

.installer-status__head,
.installer-status__actions,
.installer-panel__footer {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
}

.installer-status__badge {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	height: 34px;
	padding: 0 14px;
	border-radius: 999px;
	background: rgba(22, 163, 74, 0.08);
	color: #15803d;
	font-size: 12px;
	font-weight: 700;
	white-space: nowrap;
}

.installer-status__grid {
	display: grid;
	grid-template-columns: repeat(2, minmax(0, 1fr));
	gap: 12px;
	margin-top: 18px;
}

.installer-status__item {
	padding: 16px;
	border-radius: 18px;
	border: 1px solid rgba(226, 232, 240, 0.9);
	background: rgba(255, 255, 255, 0.9);
}

.installer-status__item span {
	display: block;
	color: #64748b;
	font-size: 12px;
}

.installer-status__item strong {
	display: block;
	margin-top: 10px;
	color: #123b6d;
	font-size: 18px;
	font-weight: 700;
}

.installer-status__actions {
	margin-top: 18px;
	align-items: stretch;
}

.installer-status__action {
	flex: 1;
	height: 48px;
	border-radius: 14px;
	letter-spacing: 0.08em;
	font-weight: 600;
	box-shadow: 0 18px 32px rgba(37, 99, 235, 0.18);
}

.installer-status__link {
	display: inline-flex;
	align-items: center;
	justify-content: center;
	min-width: 112px;
	padding: 0 16px;
	border-radius: 14px;
	border: 1px solid rgba(191, 219, 254, 0.9);
	color: #2563eb;
	text-decoration: none;
	font-size: 14px;
	font-weight: 600;
}

.installer-panel__footer {
	padding-top: 18px;
	border-top: 1px solid rgba(226, 232, 240, 0.9);
	color: #64748b;
	font-size: 12px;
}

:deep(.el-form-item) {
	margin-bottom: 16px;
}

:deep(.el-form--large.el-form--label-top .el-form-item .el-form-item__label) {
	margin-bottom: 8px !important;
	color: #334155;
	font-size: 13px;
	font-weight: 600;
}

:deep(.el-input__wrapper) {
	min-height: 46px;
	border-radius: 14px;
}

:deep(.fc-form .el-button) {
	width: 100%;
	height: 48px;
	border-radius: 14px;
	letter-spacing: 0.08em;
	font-weight: 600;
	box-shadow: 0 18px 32px rgba(37, 99, 235, 0.18);
}

@media (max-width: 1080px) {
	.installer-shell {
		grid-template-columns: 1fr;
		max-width: 720px;
	}
}

@media (max-width: 767px) {
	.installer-page {
		padding: 0;
	}

	.installer-shell {
		min-height: 100vh;
		border-radius: 0;
	}

	.installer-panel {
		padding: 24px;
	}

	.installer-panel__steps,
	.installer-status__grid {
		grid-template-columns: 1fr;
	}

	.installer-status__head,
	.installer-status__actions,
	.installer-panel__footer {
		flex-direction: column;
		align-items: flex-start;
	}

	.installer-status__action,
	.installer-status__link {
		width: 100%;
	}
}
</style>
