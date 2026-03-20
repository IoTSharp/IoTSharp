<template>
	<div class="installer-page">
		<div class="installer-page__mesh"></div>
		<div class="installer-shell">
			<section class="installer-showcase">
				<div class="installer-showcase__brand">
					<AppLogo />
					<span class="installer-showcase__badge">IoT Platform Installer</span>
				</div>
				<div class="installer-showcase__content">
					<p class="installer-showcase__eyebrow">Guided Installation</p>
					<h1>安装 {{ pageTitle }}</h1>
					<p class="installer-showcase__desc">
						用统一的安装向导完成管理员账号初始化、平台入口配置和首轮访问准备，让 IoTSharp 控制台与登录体验保持同一套界面语言。
					</p>
				</div>
				<div class="installer-showcase__grid">
					<article v-for="item in installerCards" :key="item.title" class="showcase-card">
						<div class="showcase-card__label">{{ item.title }}</div>
						<div class="showcase-card__value">{{ item.value }}</div>
						<p>{{ item.description }}</p>
					</article>
				</div>
				<div class="installer-showcase__footer">
					<div class="showcase-pill">
						<span class="showcase-pill__dot"></span>
						安装完成后可直接进入 IoTSharp 控制台
					</div>
					<div class="showcase-pill">
						<span class="showcase-pill__dot"></span>
						默认预填管理员账号，减少首次部署配置成本
					</div>
				</div>
			</section>

			<section class="installer-panel">
				<div class="installer-panel__header">
					<div class="installer-panel__eyebrow">Installer</div>
					<h2>{{ storesAppInfo.appInfo.installed ? '平台已安装' : '安装向导' }}</h2>
					<p>
						{{ storesAppInfo.appInfo.installed
							? '当前实例已经完成初始化。你可以直接跳转到登录界面继续访问平台。'
							: '填写管理员账号、邮箱和密码，完成 IoTSharp 首次安装。' }}
					</p>
				</div>

				<div v-if="storesAppInfo.appInfo.installed" class="installer-status">
					<div class="installer-status__title">IoTSharp 已完成安装</div>
					<p>系统版本 {{ storesAppInfo.appInfo.version }} 已就绪，现在可以直接进入登录页。</p>
					<Button type="primary" class="installer-status__action" @click="router.replace({ name: 'login' })">
						前往登录
					</Button>
				</div>

				<div v-else class="installer-form-card">
					<div class="installer-form-card__tip">
						<div class="installer-form-card__tip-title">首次部署</div>
						<div class="installer-form-card__tip-text">安装完成后将使用这里配置的管理员账号登录控制台。</div>
					</div>
					<FormCreate
						v-model:api="fApi"
						v-model="value"
						:rule="rules"
						:option="options"
						@submit="onSubmit"
					></FormCreate>
				</div>

				<div class="installer-panel__footer">
					<div>版本 {{ storesAppInfo.appInfo.version || '-' }}</div>
					<div>{{ currentYear }} {{ pageTitle }}</div>
				</div>
			</section>
		</div>
	</div>
</template>

<script setup lang="ts">
import { computed, reactive, ref, Ref, onMounted } from 'vue';
import formCreate, { Api } from '@form-create/element-ui';
import { ElButton as Button, ElMessage } from 'element-plus';
import { storeToRefs } from 'pinia';
import { useRouter } from 'vue-router';
import AppLogo from '/@/components/AppLogo.vue';
import { initSysAdmin } from '/@/api/installer';
import { useAppInfo } from '/@/stores/appInfo';
import { useThemeConfig } from '/@/stores/themeConfig';
import { NextLoading } from '/@/utils/loading';
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
const pageTitle = computed(() => themeConfig.value.globalTitle || 'IoTSharp');
const currentYear = new Date().getFullYear();

const installerCards = computed(() => [
	{
		title: '管理员入口',
		value: '1 Step',
		description: '一次表单提交即可完成管理员账号初始化。',
	},
	{
		title: '访问准备',
		value: 'Ready',
		description: '安装后立即可切换到登录界面进入控制台。',
	},
	{
		title: '平台版本',
		value: storesAppInfo.appInfo.version || 'Latest',
		description: '当前实例版本信息会在安装页中同步展示。',
	},
]);

const rules = ref(JSON.parse(JSON.stringify(installerFormRule)));

const validatePassCheck = (rule: any, fieldValue: any, callback: any) => {
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

const onSubmit = async (data: any) => {
	try {
		fApi.value?.validate(async (valid) => {
			if (!valid) {
				ElMessage.error('请正确填写安装信息');
				return;
			}

			try {
				await initSysAdmin(data);
				ElMessage.success('安装完成，正在前往登录页');
				await router.replace({ name: 'login' });
			} catch (error) {
				ElMessage.error('安装失败，请稍后重试');
			}
		});
	} catch (error) {
		ElMessage.error('请正确填写安装信息');
	}
};

onMounted(() => {
	NextLoading.done();
});
</script>

<style scoped lang="scss">
.installer-page {
	--installer-panel-bg: rgba(255, 255, 255, 0.92);
	--installer-border: rgba(148, 163, 184, 0.18);
	width: 100%;
	height: 100%;
	position: relative;
	display: flex;
	align-items: center;
	justify-content: center;
	padding: 24px;
	background:
		radial-gradient(circle at top left, rgba(14, 165, 233, 0.16), transparent 28%),
		radial-gradient(circle at bottom right, rgba(59, 130, 246, 0.18), transparent 28%),
		linear-gradient(135deg, #f4f8ff 0%, #eef6fb 42%, #f8fbff 100%);
	overflow: hidden;
}

.installer-page__mesh {
	position: absolute;
	inset: 0;
	background-image:
		linear-gradient(rgba(148, 163, 184, 0.08) 1px, transparent 1px),
		linear-gradient(90deg, rgba(148, 163, 184, 0.08) 1px, transparent 1px);
	background-size: 48px 48px;
	mask-image: radial-gradient(circle at center, #000 35%, transparent 88%);
	pointer-events: none;
}

.installer-shell {
	position: relative;
	z-index: 1;
	display: grid;
	grid-template-columns: 1.2fr minmax(380px, 460px);
	width: min(1220px, 100%);
	min-height: min(760px, calc(100vh - 48px));
	border-radius: 32px;
	border: 1px solid rgba(255, 255, 255, 0.54);
	background: rgba(255, 255, 255, 0.36);
	box-shadow: 0 30px 70px rgba(15, 23, 42, 0.14);
	backdrop-filter: blur(20px);
	overflow: hidden;
}

.installer-showcase,
.installer-panel {
	padding: 40px;
}

.installer-showcase {
	position: relative;
	display: flex;
	flex-direction: column;
	justify-content: space-between;
	gap: 28px;
	background:
		radial-gradient(circle at top right, rgba(16, 185, 129, 0.16), transparent 24%),
		linear-gradient(155deg, #0f172a 0%, #10243a 48%, #0f3b53 100%);
	color: #f8fafc;

	&::after {
		position: absolute;
		right: -80px;
		top: -60px;
		width: 240px;
		height: 240px;
		content: '';
		border-radius: 50%;
		background: radial-gradient(circle, rgba(59, 130, 246, 0.3), transparent 68%);
	}
}

.installer-showcase__brand,
.installer-showcase__footer {
	position: relative;
	z-index: 1;
	display: flex;
	align-items: center;
	flex-wrap: wrap;
	gap: 14px;
}

.installer-showcase__badge,
.showcase-pill {
	display: inline-flex;
	align-items: center;
	gap: 8px;
	padding: 8px 12px;
	border-radius: 999px;
	border: 1px solid rgba(255, 255, 255, 0.12);
	background: rgba(255, 255, 255, 0.08);
	color: rgba(248, 250, 252, 0.92);
	font-size: 12px;
	font-weight: 600;
}

.installer-showcase__content {
	position: relative;
	z-index: 1;
	max-width: 640px;
}

.installer-showcase__eyebrow,
.installer-panel__eyebrow {
	margin-bottom: 12px;
	font-size: 12px;
	font-weight: 700;
	letter-spacing: 0.18em;
	text-transform: uppercase;
}

.installer-showcase__eyebrow {
	color: rgba(125, 211, 252, 0.9);
}

.installer-showcase h1 {
	margin: 0 0 18px;
	font-size: clamp(36px, 5vw, 54px);
	line-height: 1.02;
	letter-spacing: -0.05em;
}

.installer-showcase__desc {
	max-width: 620px;
	color: rgba(226, 232, 240, 0.88);
	font-size: 16px;
	line-height: 1.9;
}

.installer-showcase__grid {
	position: relative;
	z-index: 1;
	display: grid;
	grid-template-columns: repeat(3, minmax(0, 1fr));
	gap: 16px;
}

.showcase-card {
	padding: 20px;
	border-radius: 24px;
	border: 1px solid rgba(255, 255, 255, 0.08);
	background: rgba(255, 255, 255, 0.08);
	box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.08);

	p {
		margin: 10px 0 0;
		color: rgba(226, 232, 240, 0.78);
		font-size: 13px;
		line-height: 1.7;
	}
}

.showcase-card__label {
	color: rgba(191, 219, 254, 0.84);
	font-size: 13px;
}

.showcase-card__value {
	margin-top: 12px;
	font-size: 26px;
	font-weight: 700;
	letter-spacing: -0.04em;
}

.showcase-pill__dot {
	width: 8px;
	height: 8px;
	border-radius: 50%;
	background: #34d399;
}

.installer-panel {
	display: flex;
	flex-direction: column;
	justify-content: center;
	gap: 24px;
	background: var(--installer-panel-bg);
}

.installer-panel__header {
	h2 {
		margin: 0 0 12px;
		color: #0f172a;
		font-size: 32px;
		letter-spacing: -0.04em;
	}

	p {
		color: #64748b;
		font-size: 14px;
		line-height: 1.8;
	}
}

.installer-panel__eyebrow {
	color: #0f766e;
}

.installer-form-card,
.installer-status {
	padding: 22px;
	border-radius: 24px;
	border: 1px solid rgba(148, 163, 184, 0.18);
	background: rgba(255, 255, 255, 0.74);
	box-shadow: 0 18px 40px rgba(15, 23, 42, 0.08);
}

.installer-form-card__tip {
	margin-bottom: 18px;
	padding: 16px 18px;
	border-radius: 20px;
	border: 1px solid rgba(14, 165, 233, 0.16);
	background: linear-gradient(135deg, rgba(14, 165, 233, 0.08), rgba(16, 185, 129, 0.08));
}

.installer-form-card__tip-title,
.installer-status__title {
	color: #0f172a;
	font-size: 15px;
	font-weight: 700;
}

.installer-form-card__tip-text,
.installer-status p {
	margin-top: 6px;
	color: #64748b;
	font-size: 13px;
	line-height: 1.8;
}

.installer-status__action {
	width: 100%;
	height: 48px;
	margin-top: 18px;
	border-radius: 14px;
	letter-spacing: 0.08em;
	font-weight: 600;
	box-shadow: 0 16px 30px rgba(14, 116, 144, 0.16);
}

.installer-panel__footer {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 16px;
	padding-top: 18px;
	border-top: 1px solid var(--installer-border);
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
	box-shadow: 0 16px 30px rgba(14, 116, 144, 0.16);
}

@media (max-width: 1080px) {
	.installer-shell {
		grid-template-columns: 1fr;
		max-width: 620px;
	}

	.installer-showcase {
		padding-bottom: 28px;
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

	.installer-showcase,
	.installer-panel {
		padding: 24px;
	}

	.installer-showcase__grid {
		grid-template-columns: repeat(1, minmax(0, 1fr));
	}

	.installer-panel__footer {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
