<template>
	<div class="account-login">
		<div class="account-login__tip">
			<div class="account-login__tip-title">管理员入口</div>
			<div class="account-login__tip-text">默认管理员账号已预填，输入初始化后的密码并完成拼图验证后即可进入控制台。</div>
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
						<div>登录前需要完成一次滑块拼图验证，验证通过后会自动登录。</div>
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

		<el-dialog
			v-model="dialogVisible"
			width="480px"
			align-center
			class="account-login__verify-dialog"
			:close-on-click-modal="!loading.signIn"
			:close-on-press-escape="!loading.signIn"
			:show-close="!loading.signIn"
			@closed="onDialogClosed"
		>
			<div class="account-login__verify-header">
				<h3>安全校验</h3>
				<p>拖动滑块，让拼图块回到缺口位置，系统会在松手后自动验证并尝试登录。</p>
			</div>

			<div class="captcha-shell" v-loading="captcha.loading || loading.signIn">
				<div v-if="captcha.error" class="captcha-shell__error">{{ captcha.error }}</div>

				<div v-if="captcha.bigImage" class="captcha-shell__board" :style="boardStyle">
					<img class="captcha-shell__background" :src="captcha.bigImage" alt="captcha background" @load="onBackgroundLoad" />
					<img class="captcha-shell__piece" :src="captcha.smallImage" :style="pieceStyle" alt="captcha piece" @load="onPieceLoad" />
				</div>

				<div class="captcha-shell__actions">
					<el-slider
						v-model="captcha.sliderValue"
						:max="sliderMax"
						:show-tooltip="false"
						:disabled="captcha.loading || loading.signIn || !captcha.ready"
						@change="onCaptchaRelease"
					/>
					<div class="captcha-shell__footer">
						<span>{{ captcha.ready ? '拖动下方滑块完成拼图' : '正在载入拼图资源' }}</span>
						<el-button link type="primary" :disabled="captcha.loading || loading.signIn" @click="refreshCaptcha">刷新拼图</el-button>
					</div>
				</div>
			</div>
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
import { useCaptchaApi } from '/@/api/captcha';
import { useLoginApi } from '/@/api/login';

const CAPTCHA_SUCCESS_CODE = 10000;
const LOGIN_ERROR_CODE = 10001;

interface CaptchaState {
	clientId: string;
	bigImage: string;
	smallImage: string;
	yHeight: number;
	imageWidth: number;
	imageHeight: number;
	pieceWidth: number;
	pieceHeight: number;
	sliderValue: number;
	loading: boolean;
	ready: boolean;
	error: string;
}

const { t } = useI18n();
const storesThemeConfig = useThemeConfig();
const { themeConfig } = storeToRefs(storesThemeConfig);
const route = useRoute();
const router = useRouter();

const dialogVisible = ref(false);
const isShowPassword = ref(false);
const ruleForm = reactive({
	userName: 'iotmaster@iotsharp.net',
	password: '',
});

const loading = reactive({
	signIn: false,
});

const captcha = reactive<CaptchaState>({
	clientId: '',
	bigImage: '',
	smallImage: '',
	yHeight: 0,
	imageWidth: 0,
	imageHeight: 0,
	pieceWidth: 0,
	pieceHeight: 0,
	sliderValue: 0,
	loading: false,
	ready: false,
	error: '',
});

const currentTime = computed(() => formatAxis(new Date()));
const sliderMax = computed(() => Math.max(captcha.imageWidth - captcha.pieceWidth, 0));
const boardStyle = computed(() => ({
	width: `${captcha.imageWidth || 360}px`,
	height: `${captcha.imageHeight || 180}px`,
}));
const pieceStyle = computed(() => ({
	width: `${captcha.pieceWidth || 56}px`,
	height: `${captcha.pieceHeight || 56}px`,
	transform: `translate3d(${Math.min(captcha.sliderValue, sliderMax.value)}px, ${captcha.yHeight}px, 0)`,
}));

const createCaptchaClientId = () => {
	if (globalThis.crypto?.randomUUID) return globalThis.crypto.randomUUID();
	return `captcha-${Date.now()}-${Math.random().toString(36).slice(2, 10)}`;
};

const resetCaptchaState = () => {
	captcha.clientId = '';
	captcha.bigImage = '';
	captcha.smallImage = '';
	captcha.yHeight = 0;
	captcha.imageWidth = 0;
	captcha.imageHeight = 0;
	captcha.pieceWidth = 0;
	captcha.pieceHeight = 0;
	captcha.sliderValue = 0;
	captcha.loading = false;
	captcha.ready = false;
	captcha.error = '';
};

const updateCaptchaReady = () => {
	captcha.ready = Boolean(captcha.bigImage && captcha.smallImage && captcha.imageWidth && captcha.imageHeight && captcha.pieceWidth && captcha.pieceHeight);
};

const refreshCaptcha = async () => {
	captcha.loading = true;
	captcha.ready = false;
	captcha.error = '';
	captcha.sliderValue = 0;
	captcha.imageWidth = 0;
	captcha.imageHeight = 0;
	captcha.pieceWidth = 0;
	captcha.pieceHeight = 0;
	captcha.clientId = createCaptchaClientId();

	try {
		const res: any = await useCaptchaApi().getChallenge(captcha.clientId);
		const code = res.code ?? res.Code;
		if (code !== CAPTCHA_SUCCESS_CODE) {
			throw new Error(res.msg ?? res.Msg ?? '验证码加载失败。');
		}
		const data = res.data ?? res.Data;
		const bigImage = data?.bigImage ?? data?.BigImage;
		const smallImage = data?.smallImage ?? data?.SmallImage;
		captcha.bigImage = bigImage ? `data:image/jpeg;base64,${bigImage}` : '';
		captcha.smallImage = smallImage ? `data:image/png;base64,${smallImage}` : '';
		captcha.yHeight = data?.yheight ?? data?.Yheight ?? 0;
	} catch (error) {
		captcha.error = '验证码加载失败，请刷新后重试。';
	}

	captcha.loading = false;
};

const openCaptchaDialog = async () => {
	dialogVisible.value = true;
	await refreshCaptcha();
};

const onBackgroundLoad = (event: Event) => {
	const image = event.target as HTMLImageElement;
	captcha.imageWidth = image.naturalWidth || image.width;
	captcha.imageHeight = image.naturalHeight || image.height;
	updateCaptchaReady();
};

const onPieceLoad = (event: Event) => {
	const image = event.target as HTMLImageElement;
	captcha.pieceWidth = image.naturalWidth || image.width;
	captcha.pieceHeight = image.naturalHeight || image.height;
	updateCaptchaReady();
};

const onDialogClosed = () => {
	if (!loading.signIn) resetCaptchaState();
};

const onSignIn = async () => {
	if (!ruleForm.userName || !ruleForm.password) {
		ElMessage.warning('请输入用户名和密码。');
		return;
	}

	await openCaptchaDialog();
};

const onCaptchaRelease = async (value: number) => {
	if (!captcha.ready || loading.signIn) return;
	await submitSignIn(Math.round(value));
};

const submitSignIn = async (captchaMove: number) => {
	loading.signIn = true;
	captcha.error = '';

	try {
		// Login is submitted only after the puzzle move is captured so the backend can enforce captcha validation.
		const res: any = await useLoginApi().signIn({
			password: ruleForm.password,
			userName: ruleForm.userName,
			captchaClientId: captcha.clientId,
			captchaMove,
		});
		const code = res.code ?? res.Code;
		const message = res.msg ?? res.Msg ?? '登录失败，请重试。';
		const token = res.data?.token?.access_token ?? res.Data?.Token?.access_token;

		if (code === CAPTCHA_SUCCESS_CODE && token) {
			Session.set('token', token);
			Cookies.set('userName', ruleForm.userName);
			dialogVisible.value = false;

			if (!themeConfig.value.isRequestRoutes) {
				await initFrontEndControlRoutes();
			} else {
				await initBackEndControlRoutes();
			}

			signInSuccess();
			return;
		}

		if (code === LOGIN_ERROR_CODE) {
			dialogVisible.value = false;
			ElMessage.error(message);
			return;
		}

		captcha.error = message;
		await refreshCaptcha();
	} catch (error: any) {
		const code = error?.code ?? error?.Code;
		const message = error?.msg ?? error?.Msg ?? '登录失败，请稍后重试。';

		if (code === LOGIN_ERROR_CODE) {
			dialogVisible.value = false;
			ElMessage.error(message);
		} else {
			captcha.error = message;
			await refreshCaptcha();
		}
	} finally {
		loading.signIn = false;
	}
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
	} else {
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
		line-height: 1.7;
	}
}

.captcha-shell {
	display: flex;
	flex-direction: column;
	gap: 16px;
}

.captcha-shell__error {
	padding: 10px 12px;
	border-radius: 14px;
	background: rgba(239, 68, 68, 0.08);
	color: #b91c1c;
	font-size: 13px;
}

.captcha-shell__board {
	position: relative;
	overflow: hidden;
	margin: 0 auto;
	border-radius: 20px;
	border: 1px solid rgba(148, 163, 184, 0.16);
	background: linear-gradient(180deg, rgba(248, 250, 252, 0.9), rgba(241, 245, 249, 0.9));
	box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.6);
}

.captcha-shell__background,
.captcha-shell__piece {
	user-select: none;
	-webkit-user-drag: none;
}

.captcha-shell__background {
	display: block;
	width: 100%;
	height: 100%;
	object-fit: cover;
}

.captcha-shell__piece {
	position: absolute;
	left: 0;
	top: 0;
	pointer-events: none;
	filter: drop-shadow(0 10px 24px rgba(15, 23, 42, 0.22));
	will-change: transform;
}

.captcha-shell__actions {
	padding: 2px 4px 0;
}

.captcha-shell__footer {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	margin-top: 8px;
	color: #64748b;
	font-size: 12px;
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
	.account-login__meta,
	.captcha-shell__footer {
		flex-direction: column;
		align-items: flex-start;
	}
}
</style>
