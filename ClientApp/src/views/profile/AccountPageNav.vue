<template>
	<nav class="account-nav" :aria-label="t('message.personalCenter.navigation')">
		<RouterLink to="/profile" class="account-nav__link account-nav__link--profile">
			<el-icon aria-hidden="true"><User /></el-icon>
			{{ t('message.personalCenter.profile') }}
		</RouterLink>
		<RouterLink to="/inbox" class="account-nav__link account-nav__link--inbox">
			<el-icon aria-hidden="true"><Message /></el-icon>
			{{ t('message.personalCenter.inbox') }}
		</RouterLink>
	</nav>
</template>

<script setup lang="ts">
import { Message, User } from '@element-plus/icons-vue';
import { useI18n } from 'vue-i18n';

const { t } = useI18n();
</script>

<style scoped lang="scss">
.account-nav {
	display: flex;
	gap: 8px;
	margin-bottom: 8px;
	border-bottom: 1px solid var(--iotsharp-border, #dfe7e4);
}

.account-nav__link {
	display: inline-flex;
	align-items: center;
	gap: 8px;
	padding: 7px 10px;
	border-bottom: 2px solid transparent;
	color: var(--iotsharp-text-soft, #52635f);
	font-size: 14px;
	font-weight: 550;
	text-decoration: none;

	&:hover,
	&.router-link-exact-active {
		color: var(--nav-ink);
		background: var(--nav-surface);
	}

	&.router-link-exact-active {
		border-color: var(--nav-accent);
		font-weight: 700;
	}
}

.account-nav__link--profile {
	--nav-accent: var(--iotsharp-profile-accent, #236551);
	--nav-ink: var(--nav-accent);
	--nav-surface: var(--iotsharp-profile-surface, #e5f4ee);
}

.account-nav__link--inbox {
	--nav-accent: var(--iotsharp-inbox-accent, #30639d);
	--nav-ink: var(--nav-accent);
	--nav-surface: var(--iotsharp-inbox-surface, #eaf2fd);
}
</style>
