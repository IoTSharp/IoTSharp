<template>
	<main class="personal-page">
		<AccountPageNav />
		<h1>{{ t('message.personalCenter.profile') }}</h1>

		<section v-if="loadFailed && !profile" class="personal-state" role="alert">
			<p>{{ t('message.personalCenter.profileLoadFailed') }}</p>
			<el-button :loading="loading" @click="loadProfile">{{ t('message.personalCenter.retry') }}</el-button>
		</section>
		<p v-else-if="!profile" class="personal-state" role="status">{{ t('message.personalCenter.loading') }}</p>
		<template v-else>
			<div class="personal-grid">
				<section class="identity-panel" :aria-label="t('message.personalCenter.avatar')">
					<div class="identity-overview">
						<div class="identity-avatar">
							<img v-if="profile.hasCustomAvatar && profile.avatar" :src="profile.avatar" alt="" />
							<span v-else aria-hidden="true">{{ initials }}</span>
						</div>
						<div class="identity-meta">
							<h2>{{ profile.displayName || profile.userName }}</h2>
							<p v-if="profile.email">{{ profile.email }}</p>
							<p v-if="profile.introduction" class="identity-intro">{{ profile.introduction }}</p>
						</div>
					</div>
					<div class="identity-actions">
						<input ref="avatarInput" type="file" accept="image/png,image/jpeg,image/webp" hidden @change="chooseAvatar" />
						<el-button
							:disabled="profileSaving || avatarRemoving"
							:loading="avatarUploading"
							:icon="Upload"
							aria-describedby="personal-avatar-hint"
							@click="avatarInput?.click()"
						>
							{{ t('message.personalCenter.uploadAvatar') }}
						</el-button>
						<el-button
							v-if="profile.hasCustomAvatar"
							text
							:disabled="profileSaving || avatarUploading"
							:loading="avatarRemoving"
							@click="resetAvatar"
						>
							{{ t('message.personalCenter.removeAvatar') }}
						</el-button>
					</div>
					<p id="personal-avatar-hint" class="identity-hint">{{ t('message.personalCenter.avatarHint') }}</p>
					<p v-if="avatarError" class="personal-error" role="alert">{{ avatarError }}</p>
				</section>

				<section class="personal-panel access-panel" aria-labelledby="personal-access-title">
					<header class="panel-heading">
						<span class="panel-icon"><el-icon aria-hidden="true"><OfficeBuilding /></el-icon></span>
						<h2 id="personal-access-title">{{ t('message.personalCenter.access') }}</h2>
					</header>
					<dl class="access-fields">
						<div>
							<dt>{{ t('message.personalCenter.tenant') }}</dt>
							<dd>{{ profile.tenantName || '—' }}</dd>
						</div>
						<div>
							<dt>{{ t('message.personalCenter.customer') }}</dt>
							<dd>{{ profile.customerName || '—' }}</dd>
						</div>
						<div>
							<dt>{{ t('message.personalCenter.roles') }}</dt>
							<dd v-if="profile.roles.length" class="access-roles">
								<span v-for="role in profile.roles" :key="role">{{ role }}</span>
							</dd>
							<dd v-else>—</dd>
						</div>
					</dl>
				</section>

				<section class="personal-panel details-panel" aria-labelledby="personal-details-title">
					<header class="panel-heading">
						<span class="panel-icon"><el-icon aria-hidden="true"><User /></el-icon></span>
						<h2 id="personal-details-title">{{ t('message.personalCenter.details') }}</h2>
					</header>
					<form class="profile-form" @submit.prevent="submitProfile">
						<div class="profile-fields">
							<div class="profile-field">
								<label for="personal-display-name">{{ t('message.personalCenter.displayName') }}</label>
								<el-input id="personal-display-name" v-model="draft.displayName" required maxlength="80" autocomplete="name" :disabled="profileSaving" />
							</div>
							<div class="profile-field">
								<label for="personal-phone-number">{{ t('message.personalCenter.phoneNumber') }}</label>
								<el-input id="personal-phone-number" v-model="draft.phoneNumber" type="tel" maxlength="32" autocomplete="tel" :disabled="profileSaving" />
							</div>
							<div class="profile-field profile-field--wide">
								<label for="personal-email">{{ t('message.personalCenter.email') }}</label>
								<el-input id="personal-email" :model-value="profile.email || ''" readonly type="email" autocomplete="email" />
							</div>
							<div class="profile-field profile-field--wide">
								<label for="personal-introduction">{{ t('message.personalCenter.introduction') }}</label>
								<el-input id="personal-introduction" v-model="draft.introduction" :rows="3" maxlength="300" show-word-limit type="textarea" :disabled="profileSaving" />
							</div>
						</div>
						<div class="profile-form-actions">
							<el-button
								native-type="submit"
								type="primary"
								:disabled="!profileChanged || passwordSaving || avatarUploading || avatarRemoving"
								:loading="profileSaving"
							>
								{{ t('message.personalCenter.saveProfile') }}
							</el-button>
							<p v-if="profileError" class="personal-error" role="alert">{{ profileError }}</p>
						</div>
					</form>
				</section>

				<section class="personal-panel security-panel" aria-labelledby="personal-security-title">
					<header class="panel-heading">
						<span class="panel-icon"><el-icon aria-hidden="true"><Lock /></el-icon></span>
						<h2 id="personal-security-title">{{ t('message.personalCenter.security') }}</h2>
					</header>
					<form class="profile-form" @submit.prevent="submitPassword">
						<div class="profile-field">
							<label for="personal-current-password">{{ t('message.personalCenter.currentPassword') }}</label>
							<el-input id="personal-current-password" v-model="password.pass" type="password" show-password required autocomplete="current-password" :disabled="passwordSaving" />
						</div>
						<div class="profile-field">
							<label for="personal-new-password">{{ t('message.personalCenter.newPassword') }}</label>
							<el-input id="personal-new-password" v-model="password.passnew" type="password" show-password required minlength="7" autocomplete="new-password" :disabled="passwordSaving" />
						</div>
						<div class="profile-field">
							<label for="personal-confirm-password">{{ t('message.personalCenter.confirmPassword') }}</label>
							<el-input id="personal-confirm-password" v-model="password.passnewsecond" type="password" show-password required autocomplete="new-password" :disabled="passwordSaving" />
						</div>
						<div class="profile-form-actions">
							<el-button :disabled="profileSaving" :loading="passwordSaving" native-type="submit" type="primary">
								{{ t('message.personalCenter.changePassword') }}
							</el-button>
							<p v-if="passwordError" class="personal-error" role="alert">{{ passwordError }}</p>
						</div>
					</form>
				</section>
			</div>
		</template>
	</main>
</template>

<script setup lang="ts" name="profile">
import { Lock, OfficeBuilding, Upload, User } from '@element-plus/icons-vue';
import { ElMessage } from 'element-plus';
import { computed, onMounted, reactive, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { ModifyMyPassword } from '/@/api/account';
import {
	getPersonalProfile,
	removePersonalAvatar,
	savePersonalProfile,
	uploadPersonalAvatar,
	type PersonalProfile,
	type PersonalProfileUpdate,
} from '/@/api/personalCenter';
import { useUserInfo } from '/@/stores/userInfo';
import AccountPageNav from './AccountPageNav.vue';

const avatarTypes = ['image/png', 'image/jpeg', 'image/webp'];
const avatarMaxBytes = 2 * 1024 * 1024;

const { t } = useI18n();
const userStore = useUserInfo();
const avatarInput = ref<HTMLInputElement>();
const profile = ref<PersonalProfile>();
const draft = reactive<PersonalProfileUpdate>({ displayName: '', introduction: '', phoneNumber: '' });
const password = reactive({ pass: '', passnew: '', passnewsecond: '' });
const loading = ref(false);
const loadFailed = ref(false);
const profileSaving = ref(false);
const passwordSaving = ref(false);
const avatarUploading = ref(false);
const avatarRemoving = ref(false);
const profileError = ref('');
const passwordError = ref('');
const avatarError = ref('');

const initials = computed(() => {
	const name = profile.value?.displayName || profile.value?.userName || profile.value?.email || 'IoTSharp';
	return Array.from(name.trim()).slice(0, 2).join('').toLocaleUpperCase();
});
const profileChanged = computed(() => (
	draft.displayName.trim() !== profile.value?.displayName ||
	draft.introduction.trim() !== profile.value?.introduction ||
	draft.phoneNumber.trim() !== (profile.value?.phoneNumber || '')
));

/** 接收最新的资料，并按操作类型决定是否更新正在编辑的表单。 */
function applyProfile(next: PersonalProfile, syncDraft = true) {
	profile.value = next;
	userStore.applySelfProfile(next.displayName || next.userName || '', next.avatar || '');
	if (syncDraft) {
		draft.displayName = next.displayName;
		draft.introduction = next.introduction;
		draft.phoneNumber = next.phoneNumber || '';
	}
}

async function loadProfile() {
	if (loading.value) return;
	loading.value = true;
	loadFailed.value = false;
	try {
		applyProfile(await getPersonalProfile());
	} catch {
		loadFailed.value = true;
	} finally {
		loading.value = false;
	}
}

async function submitProfile() {
	if (!profileChanged.value || profileSaving.value || passwordSaving.value || avatarUploading.value || avatarRemoving.value) return;
	profileError.value = '';
	const payload = {
		displayName: draft.displayName.trim(),
		introduction: draft.introduction.trim(),
		phoneNumber: draft.phoneNumber.trim(),
	};
	if (!payload.displayName) {
		profileError.value = t('message.personalCenter.nameRequired');
		return;
	}
	profileSaving.value = true;
	try {
		applyProfile(await savePersonalProfile(payload));
		ElMessage.success(t('message.personalCenter.profileSaved'));
	} catch {
		profileError.value = t('message.personalCenter.profileSaveFailed');
	} finally {
		profileSaving.value = false;
	}
}

async function chooseAvatar(event: Event) {
	const input = event.target as HTMLInputElement;
	const file = input.files?.[0];
	input.value = '';
	if (!file || avatarUploading.value || avatarRemoving.value || profileSaving.value) return;
	avatarError.value = '';
	if (file.size === 0 || file.size > avatarMaxBytes || (file.type && !avatarTypes.includes(file.type))) {
		avatarError.value = t('message.personalCenter.avatarHint');
		return;
	}
	avatarUploading.value = true;
	try {
		applyProfile(await uploadPersonalAvatar(file), false);
		ElMessage.success(t('message.personalCenter.avatarSaved'));
	} catch {
		avatarError.value = t('message.personalCenter.avatarUploadFailed');
	} finally {
		avatarUploading.value = false;
	}
}

async function resetAvatar() {
	if (avatarUploading.value || avatarRemoving.value || profileSaving.value) return;
	avatarError.value = '';
	avatarRemoving.value = true;
	try {
		applyProfile(await removePersonalAvatar(), false);
		ElMessage.success(t('message.personalCenter.avatarRemoved'));
	} catch {
		avatarError.value = t('message.personalCenter.avatarRemoveFailed');
	} finally {
		avatarRemoving.value = false;
	}
}

async function submitPassword() {
	if (passwordSaving.value || profileSaving.value) return;
	passwordError.value = '';
	if (password.passnew !== password.passnewsecond) {
		passwordError.value = t('message.personalCenter.passwordMismatch');
		return;
	}
	passwordSaving.value = true;
	try {
		await ModifyMyPassword({ ...password });
		Object.assign(password, { pass: '', passnew: '', passnewsecond: '' });
		ElMessage.success(t('message.personalCenter.passwordSaved'));
	} catch {
		passwordError.value = t('message.personalCenter.passwordSaveFailed');
	} finally {
		passwordSaving.value = false;
	}
}

onMounted(loadProfile);
</script>

<style scoped lang="scss">
.personal-page {
	--account-accent: var(--iotsharp-profile-accent, #236551);
	--account-on-accent: var(--iotsharp-profile-on-accent, #fff);
	--account-surface: var(--iotsharp-profile-surface, #e5f4ee);
	--security-accent: var(--iotsharp-security-accent, #965526);
	--security-surface: var(--iotsharp-security-surface, #fff1df);
	--panel-border: var(--iotsharp-border, #dfe7e4);
	--panel-surface: var(--iotsharp-surface, #fff);
	width: min(100%, 1180px);
	min-width: 0;
	margin: 0 auto;
	padding: 0;
	color: var(--iotsharp-text, #23332f);

	h1 {
		margin: 0 0 10px;
		font-size: 22px;
		font-weight: 740;
		line-height: 1.18;
		letter-spacing: -0.035em;
	}
}

.personal-grid {
	display: grid;
	grid-template-columns: minmax(0, 1.55fr) minmax(0, 1fr);
	align-items: stretch;
	gap: 10px;
}

.identity-panel,
.personal-panel,
.personal-state {
	min-width: 0;
	padding: clamp(12px, 1.5vw, 18px);
	border: 1px solid var(--panel-border);
	border-radius: var(--iotsharp-radius-panel, 8px);
	background: var(--panel-surface);
}

.identity-panel {
	border-color: color-mix(in srgb, var(--account-accent) 24%, var(--panel-border));
	background: linear-gradient(135deg, var(--account-surface), var(--panel-surface) 112%);
}

.identity-overview,
.panel-heading,
.identity-actions,
.profile-form-actions {
	display: flex;
	flex-wrap: wrap;
	align-items: center;
	gap: 11px;
}

.identity-overview {
	flex-wrap: nowrap;
	gap: clamp(12px, 2.2vw, 20px);
}

.identity-avatar {
	display: grid;
	flex: none;
	width: clamp(56px, 7vw, 82px);
	aspect-ratio: 1;
	place-items: center;
	overflow: hidden;
	border: 3px solid var(--panel-surface);
	border-radius: 50%;
	background: var(--account-accent);
	color: var(--account-on-accent);
	font-size: clamp(20px, 3vw, 29px);
	font-weight: 700;

	img {
		width: 100%;
		height: 100%;
		object-fit: cover;
	}
}

.identity-meta {
	min-width: 0;
	overflow-wrap: anywhere;

	h2 {
		margin: 0;
		color: var(--account-accent);
		font-size: clamp(19px, 2.4vw, 29px);
		letter-spacing: -0.025em;
		line-height: 1.25;
	}

	p {
		margin: 5px 0 0;
		color: var(--iotsharp-text-soft, #52635f);
		font-size: 13px;
		line-height: 1.5;
	}

	.identity-intro {
		color: var(--iotsharp-text, #23332f);
		white-space: pre-wrap;
	}
}

.identity-actions {
	margin-top: 14px;

	.el-button + .el-button {
		margin-left: 0;
	}
}

.identity-hint,
.personal-error {
	margin: 8px 0 0;
	font-size: 12px;
	line-height: 1.5;
}

.identity-hint {
	color: var(--iotsharp-text-soft, #52635f);
}

.personal-error {
	color: var(--iotsharp-danger, #bd3e3e);
}

.panel-heading {
	margin-bottom: 17px;

	h2 {
		margin: 0;
		font-size: 16px;
		font-weight: 700;
	}
}

.personal-panel {
	--panel-accent: var(--account-accent);
	--panel-on-accent: var(--account-on-accent);
	--panel-tint: var(--account-surface);
}

.access-panel {
	--panel-accent: var(--iotsharp-info, #24566b);
	--panel-tint: var(--iotsharp-info-surface, #e8f3f7);
}

.security-panel {
	--panel-accent: var(--security-accent);
	--panel-on-accent: var(--iotsharp-on-accent, #fff);
	--panel-tint: var(--security-surface);
}

.panel-icon {
	display: grid;
	width: 33px;
	aspect-ratio: 1;
	place-items: center;
	border-radius: 7px;
	background: var(--panel-tint);
	color: var(--panel-accent);
	font-size: 16px;
}

.access-fields {
	display: grid;
	gap: 11px;
	margin: 0;
	font-size: 13px;

	> div {
		display: grid;
		grid-template-columns: minmax(90px, 0.8fr) minmax(0, 1.2fr);
		align-items: baseline;
		gap: 8px;
	}

	dt {
		color: var(--iotsharp-text-soft, #52635f);
	}

	dd {
		margin: 0;
		overflow-wrap: anywhere;
	}
}

.access-roles {
	display: flex;
	flex-wrap: wrap;
	gap: 5px;

	span {
		padding: 2px 7px;
		border-radius: 4px;
		background: var(--panel-tint);
		color: var(--panel-accent);
		font-size: 12px;
	}
}

.profile-form,
.profile-fields {
	display: grid;
	gap: 13px;
}

.profile-fields {
	grid-template-columns: repeat(2, minmax(0, 1fr));
}

.profile-field {
	display: grid;
	min-width: 0;
	gap: 5px;
	font-size: 13px;

	label {
		font-weight: 550;
	}
}

.profile-field--wide {
	grid-column: 1 / -1;
}

.profile-form-actions {
	.el-button--primary {
		--el-button-text-color: var(--panel-on-accent);
		--el-button-hover-text-color: var(--panel-on-accent);
		--el-button-active-text-color: var(--panel-on-accent);
		--el-button-bg-color: var(--panel-accent);
		--el-button-border-color: var(--panel-accent);
		--el-button-hover-bg-color: color-mix(in srgb, var(--panel-accent) 86%, var(--panel-on-accent));
		--el-button-hover-border-color: var(--el-button-hover-bg-color);
		--el-button-active-bg-color: var(--panel-accent);
		--el-button-active-border-color: var(--panel-accent);
	}

	.personal-error {
		margin: 0;
	}
}

.personal-state {
	p {
		margin: 0 0 10px;
	}
}

@media (max-width: 780px) {
	.personal-grid {
		grid-template-columns: minmax(0, 1fr);
	}
}

@media (max-width: 460px) {
	.profile-fields {
		grid-template-columns: minmax(0, 1fr);
	}
}
</style>
