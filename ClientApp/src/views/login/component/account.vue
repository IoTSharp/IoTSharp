<template>
	<div class="account-login">
		<div class="account-login__tip">
			<div class="account-login__tip-title">管理员入口</div>
			<div class="account-login__tip-text">默认管理员账号已预填，你只需要输入初始化后的密码即可登录控制台。</div>
		</div>

		<el-form class="account-login__form" size="large" @submit.prevent="onSignIn">
			<el-form-item class="account-login__field account-login__field--1">
				<div class="account-login__label">用户名</div>
				<el-input
					v-model="ruleForm.userName"
					type="text"
					placeholder="请输入用户名"
					clearable
					autocomplete="username"
					@keyup.enter="onSignIn"
				>
					<template #prefix>
						<el-icon class="el-input__icon"><ele-User /></el-icon>
					</template>
				</el-input>
			</el-form-item>

			<el-form-item class="account-login__field account-login__field--2">
				<div class="account-login__label">密码</div>
				<el-input
					v-model="ruleForm.password"
					:type="isShowPassword ? 'text' : 'password'"
					placeholder="请输入密码"
					autocomplete="current-password"
					@keyup.enter="onSignIn"
				>
					<template #prefix>
						<el-icon class="el-input__icon"><ele-Unlock /></el-icon>
					</template>
					<template #suffix>
						<i
							class="iconfont el-input__icon account-login__password-toggle"
							:class="isShowPassword ? 'icon-yincangmima' : 'icon-xianshimima'"
							@click="isShowPassword = !isShowPassword"
						></i>
					</template>
				</el-input>
			</el-form-item>

			<el-form-item class="account-login__field account-login__field--3">
				<div class="account-login__actions">
					<div class="account-login__meta">
						<div>验证码校验通过后将自动进入首页。</div>
						<div class="account-login__meta-user">预设账号：{{ ruleForm.userName }}</div>
					</div>
					<el-button type="primary" class="account-login__submit" native-type="submit" :loading="loading.signIn">
						登录控制台
					</el-button>
				</div>
			</el-form-item>

			<div class="account-login__signup">
				<span>还没有账号？</span>
				<router-link to="/signup">
					<el-link type="primary" underline="never">立即注册</el-link>
				</router-link>
			</div>
		</el-form>

		<el-dialog v-model="dialogVisible" width="420px" align-center class="account-login__verify-dialog">
			<div class="account-login__verify-header">
				<h3>安全校验</h3>
				<p>请完成滑块验证后继续登录。</p>
			</div>
			<slide-verify
				ref="block"
				:imgs="imgs"
				slider-text="向右滑动完成验证"
				:accuracy="accuracy"
				@again="onAgain"
				@success="onSuccess"
				@fail="onFail"
				@refresh="onRefresh"
			></slide-verify>
		</el-dialog>
	</div>
</template>

<script setup lang="ts">
import Cookies from 'js-cookie';
import { computed, reactive, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ElMessage } from 'element-plus';
import { storeToRefs } from 'pinia';
import { useI18n } from 'vue-i18n';
import { useThemeConfig } from '/@/stores/themeConfig';
import { initFrontEndControlRoutes } from '/@/router/frontEnd';
import { initBackEndControlRoutes } from '/@/router/backEnd';
import { Session } from '/@/utils/storage';
import { formatAxis } from '/@/utils/formatTime';
import { NextLoading } from '/@/utils/loading';
import { useLoginApi } from '/@/api/login';
import SlideVerify, { SlideVerifyInstance } from 'vue3-slide-verify';
import 'vue3-slide-verify/dist/style.css';

const block = ref<SlideVerifyInstance>();
const { t } = useI18n();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
const route = useRoute();
const router = useRouter();

const dialogVisible = ref(false);
const isShowPassword = ref(false);
const accuracy = ref(3);
const msg = ref('');

const apiBaseUrl = (import.meta.env.VITE_API_URL || '').replace(/\/$/, '');
const imgs = [`${apiBaseUrl}/api/Captcha/Imgs`];
const ruleForm = reactive({
	userName: 'iotmaster@iotsharp.net',
	password: '',
	code: '1234',
});

const loading = reactive({
	signIn: false,
});

const currentTime = computed(() => formatAxis(new Date()));

const onSignIn = async () => {
	if (!ruleForm.userName || !ruleForm.password) {
		ElMessage.warning('请输入用户名和密码');
		return;
	}
	dialogVisible.value = true;
};

const onAgain = () => {
	msg.value = '检测到异常滑动，请重试';
	block.value?.refresh();
};

const onSuccess = async (times: number) => {
	msg.value = `验证通过，用时 ${(times / 1000).toFixed(1)} 秒`;
	loading.signIn = true;
	try {
		const res: any = await useLoginApi().signIn({
			password: ruleForm.password,
			userName: ruleForm.userName,
		});

		if (res.code === 10000 && res.data?.token?.access_token) {
			Session.set('token', res.data.token.access_token);
			Cookies.set('userName', ruleForm.userName);
			dialogVisible.value = false;

			if (!themeConfig.value.isRequestRoutes) {
				await initFrontEndControlRoutes();
			}
			else {
				await initBackEndControlRoutes();
			}

			signInSuccess();
			block.value?.refresh();
			return;
		}

		loading.signIn = false;
		dialogVisible.value = false;
		ElMessage.error(res.msg || '用户名或密码错误');
		block.value?.refresh();
	}
	catch (error) {
		loading.signIn = false;
		dialogVisible.value = false;
		block.value?.refresh();
	}
};

const onFail = () => {
	msg.value = '验证未通过，请重试';
};

const onRefresh = () => {
	msg.value = '验证码已刷新';
};

const signInSuccess = () => {
	const signInText = t('message.signInText');
	const redirect = route.query?.redirect as string | undefined;
	const params = route.query?.params as string | undefined;

	if (redirect) {
		router.push({
			path: redirect,
			query: params ? JSON.parse(params) : '',
		});
	}
	else {
		router.push('/');
	}

	ElMessage.success(`${currentTime.value}，${signInText}`);
	NextLoading.start();
};
</script>

<style scoped lang="scss">
.account-login {
	display: flex;
	flex-direction: column;
	gap: 22px;
}

.account-login__tip {
	padding: 16px 18px;
	border-radius: 20px;
	border: 1px solid rgba(14, 165, 233, 0.16);
	background: linear-gradient(135deg, rgba(14, 165, 233, 0.08), rgba(16, 185, 129, 0.08));
}

.account-login__tip-title {
	color: #0f172a;
	font-size: 14px;
	font-weight: 700;
}

.account-login__tip-text {
	margin-top: 6px;
	color: #64748b;
	font-size: 13px;
	line-height: 1.7;
}

.account-login__form {
	display: flex;
	flex-direction: column;
	gap: 2px;
}

.account-login__field {
	margin-bottom: 12px;
}

.account-login__label {
	margin-bottom: 10px;
	color: #334155;
	font-size: 13px;
	font-weight: 600;
}

.account-login__actions {
	display: flex;
	flex-direction: column;
	gap: 16px;
	width: 100%;
}

.account-login__meta {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	color: #64748b;
	font-size: 12px;
	flex-wrap: wrap;
}

.account-login__meta-user {
	padding: 6px 10px;
	border-radius: 999px;
	background: rgba(15, 23, 42, 0.05);
	color: #475569;
}

.account-login__submit {
	width: 100%;
	height: 48px;
	border-radius: 14px;
	letter-spacing: 0.08em;
	font-weight: 600;
	box-shadow: 0 16px 30px rgba(14, 116, 144, 0.16);
}

.account-login__signup {
	display: flex;
	align-items: center;
	justify-content: center;
	gap: 6px;
	margin-top: 4px;
	color: #64748b;
	font-size: 13px;
}

.account-login__password-toggle {
	cursor: pointer;

	&:hover {
		color: #0f766e;
	}
}

.account-login__verify-header {
	margin-bottom: 16px;

	h3 {
		margin: 0 0 8px;
		color: #0f172a;
		font-size: 20px;
	}

	p {
		color: #64748b;
		font-size: 13px;
	}
}

@for $i from 1 through 3 {
	.account-login__field--#{$i} {
		opacity: 0;
		animation-name: error-num;
		animation-duration: 0.5s;
		animation-fill-mode: forwards;
		animation-delay: calc($i / 10) + s;
	}
}

@media (max-width: 767px) {
	.account-login__meta {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
