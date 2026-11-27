<template>
	<main class="inbox-page console-list-page">
		<AccountPageNav />
		<header class="inbox-header">
			<div>
				<h1>{{ t('message.personalCenter.inbox') }}</h1>
				<p>{{ t('message.personalCenter.inboxDescription') }}</p>
			</div>
			<span v-if="hasLoaded" class="inbox-count" role="status">
				<el-icon aria-hidden="true"><Bell /></el-icon>
				{{ t('message.personalCenter.unreadCount', { count: unreadCount }) }}
			</span>
		</header>

		<div class="inbox-shell console-list-panel">
			<section class="inbox-list console-list-panel" :aria-label="t('message.personalCenter.messageList')" :aria-busy="loading">
				<div class="inbox-toolbar">
					<div class="inbox-filters" role="group" :aria-label="t('message.personalCenter.messageFilter')">
						<button type="button" :aria-pressed="!unreadOnly" :disabled="busy" @click="setFilter(false)">
							{{ t('message.personalCenter.allMessages') }}
						</button>
						<button type="button" :aria-pressed="unreadOnly" :disabled="busy" @click="setFilter(true)">
							{{ t('message.personalCenter.unreadMessages') }}
						</button>
					</div>
					<el-button
						text
						:icon="Check"
						:loading="markingAll"
						:disabled="busy || !unreadCount"
						@click="markAll"
					>
						{{ t('message.personalCenter.markAllRead') }}
					</el-button>
				</div>

				<div v-if="listFailed" class="inbox-state" role="alert">
					<p>{{ t('message.personalCenter.inboxLoadFailed') }}</p>
					<el-button :loading="loading" @click="loadInbox(page)">{{ t('message.personalCenter.retry') }}</el-button>
				</div>
				<p v-else-if="loading && !hasLoaded" class="inbox-state" role="status">{{ t('message.personalCenter.loading') }}</p>
				<div v-else-if="!messages.length" class="inbox-state" role="status">
					<el-icon aria-hidden="true"><Message /></el-icon>
					<p>{{ t(unreadOnly ? 'message.personalCenter.emptyUnread' : 'message.personalCenter.emptyInbox') }}</p>
				</div>
				<ul v-else class="inbox-items console-list-table">
					<li v-for="message in messages" :key="message.id">
						<button
							type="button"
							class="message-entry"
							:class="{ 'message-entry--unread': !message.readAt }"
							:aria-current="message.id === selected?.id ? 'true' : undefined"
							:disabled="busy"
							@click="openMessage(message)"
						>
							<span class="category-icon" :class="{ 'category-icon--release': message.category === 'release' }" aria-hidden="true">
								<el-icon><Box v-if="message.category === 'release'" /><Bell v-else /></el-icon>
							</span>
							<span class="message-entry__copy">
								<span class="message-entry__title">{{ message.title }}</span>
								<span class="message-entry__sender">{{ message.sender }}</span>
							</span>
							<span class="message-entry__meta">
								<time :datetime="message.createdAt">{{ formatTime(message.createdAt, false) }}</time>
								<span v-if="!message.readAt" class="unread-dot" :aria-label="t('message.personalCenter.unreadMessages')" role="img"></span>
							</span>
						</button>
					</li>
				</ul>

				<div v-if="hasLoaded && pageCount > 1 && !listFailed" class="inbox-pagination console-list-pagination">
					<span>{{ t('message.personalCenter.pageNumber', { page, total: pageCount }) }}</span>
					<div>
						<el-button size="small" :icon="ArrowLeft" :aria-label="t('message.personalCenter.previousPage')" :disabled="busy || page <= 1" @click="loadInbox(page - 1)" />
						<el-button size="small" :icon="ArrowRight" :aria-label="t('message.personalCenter.nextPage')" :disabled="busy || page >= pageCount" @click="loadInbox(page + 1)" />
					</div>
				</div>
				<p v-if="markAllFailed" class="inbox-error" role="alert">{{ t('message.personalCenter.markFailed') }}</p>
			</section>

			<section class="inbox-detail" aria-labelledby="inbox-detail-title">
				<template v-if="selected">
					<header class="inbox-detail__header">
						<span class="category-label" :class="{ 'category-label--release': selected.category === 'release' }">
							{{ categoryLabel(selected.category) }}
						</span>
						<h2 id="inbox-detail-title" ref="detailHeading" tabindex="-1">{{ selected.title }}</h2>
						<div class="inbox-detail__meta">
							<span>{{ selected.sender }}</span>
							<time :datetime="selected.createdAt">{{ formatTime(selected.createdAt, true) }}</time>
						</div>
					</header>
					<p class="inbox-detail__body">{{ selected.body }}</p>
					<div v-if="markFailed" class="inbox-detail__error">
						<p role="alert">{{ t('message.personalCenter.markFailed') }}</p>
						<el-button :loading="markingMessage" :disabled="busy" @click="markMessage(selected)">{{ t('message.personalCenter.retry') }}</el-button>
					</div>
				</template>
				<div v-else class="inbox-detail__empty">
					<span class="category-icon"><el-icon aria-hidden="true"><Reading /></el-icon></span>
					<h2 id="inbox-detail-title">{{ t('message.personalCenter.selectMessage') }}</h2>
				</div>
			</section>
		</div>
	</main>
</template>

<script setup lang="ts" name="personalInbox">
import { ArrowLeft, ArrowRight, Bell, Box, Check, Message, Reading } from '@element-plus/icons-vue';
import { computed, nextTick, onMounted, ref } from 'vue';
import { useI18n } from 'vue-i18n';
import { getInbox, inboxUnreadEvent, markInboxMessageRead, markInboxRead, type InboxMessage } from '/@/api/personalCenter';
import AccountPageNav from './AccountPageNav.vue';

const pageSize = 10;
const { t, locale } = useI18n();
const messages = ref<InboxMessage[]>([]);
const selected = ref<InboxMessage>();
const detailHeading = ref<HTMLElement>();
const unreadCount = ref(0);
const total = ref(0);
const page = ref(1);
const unreadOnly = ref(false);
const loading = ref(false);
const hasLoaded = ref(false);
const listFailed = ref(false);
const markingMessage = ref(false);
const markingAll = ref(false);
const markFailed = ref(false);
const markAllFailed = ref(false);
const busy = computed(() => loading.value || markingMessage.value || markingAll.value);
const pageCount = computed(() => Math.max(1, Math.ceil(total.value / pageSize)));
const dateFormat = computed(() => ({
	short: new Intl.DateTimeFormat(locale.value, { month: 'short', day: 'numeric' }),
	full: new Intl.DateTimeFormat(locale.value, { dateStyle: 'medium', timeStyle: 'short' }),
}));

function formatTime(value: string, full: boolean) {
	const parsed = new Date(value);
	if (Number.isNaN(parsed.getTime())) return '—';
	return dateFormat.value[full ? 'full' : 'short'].format(parsed);
}

function categoryLabel(category: string) {
	if (category === 'release') return t('message.personalCenter.categoryRelease');
	if (category === 'system') return t('message.personalCenter.categorySystem');
	return t('message.personalCenter.categoryOther');
}

/** 将同一服务端未读数同步给导航栏。 */
function updateUnreadCount(count: number) {
	unreadCount.value = count;
	window.dispatchEvent(new CustomEvent<number>(inboxUnreadEvent, { detail: count }));
}

async function loadInbox(targetPage: number) {
	if (loading.value) return;
	loading.value = true;
	listFailed.value = false;
	page.value = targetPage;
	try {
		const result = await getInbox(targetPage, pageSize, unreadOnly.value);
		messages.value = result.items;
		total.value = result.total;
		updateUnreadCount(result.unreadCount);
		hasLoaded.value = true;
	} catch {
		listFailed.value = true;
	} finally {
		loading.value = false;
	}
}

async function setFilter(unread: boolean) {
	if (busy.value || unread === unreadOnly.value) return;
	unreadOnly.value = unread;
	selected.value = undefined;
	markFailed.value = false;
	markAllFailed.value = false;
	await loadInbox(1);
}

async function openMessage(message: InboxMessage) {
	if (busy.value) return;
	selected.value = message;
	markFailed.value = false;
	await nextTick();
	detailHeading.value?.focus({ preventScroll: !window.matchMedia('(max-width: 780px)').matches });
	if (!message.readAt) await markMessage(message);
}

async function markMessage(message: InboxMessage) {
	if (busy.value || message.readAt) return;
	markingMessage.value = true;
	markFailed.value = false;
	try {
		const count = await markInboxMessageRead(message.id);
		updateUnreadCount(count);
		const readMessage = { ...message, readAt: new Date().toISOString() };
		selected.value = readMessage;
		if (unreadOnly.value) {
			// 已读消息离开筛选结果，如本页已空则回到仍有记录的末页。
			const target = Math.min(page.value, Math.max(1, Math.ceil(count / pageSize)));
			await loadInbox(target);
		} else {
			messages.value = messages.value.map((item) => item.id === message.id ? readMessage : item);
		}
	} catch {
		markFailed.value = true;
	} finally {
		markingMessage.value = false;
	}
}

async function markAll() {
	if (busy.value || !unreadCount.value) return;
	markingAll.value = true;
	markAllFailed.value = false;
	try {
		updateUnreadCount(await markInboxRead());
		if (selected.value && !selected.value.readAt) {
			selected.value = { ...selected.value, readAt: new Date().toISOString() };
			markFailed.value = false;
		}
		await loadInbox(unreadOnly.value ? 1 : page.value);
	} catch {
		markAllFailed.value = true;
	} finally {
		markingAll.value = false;
	}
}

onMounted(() => loadInbox(1));
</script>

<style scoped lang="scss">
.inbox-page {
	--inbox-accent: var(--iotsharp-inbox-accent, #30639d);
	--inbox-surface: var(--iotsharp-inbox-surface, #eaf2fd);
	--release-accent: var(--iotsharp-success, #157347);
	--release-surface: var(--iotsharp-success-surface, #e8f5ed);
	--panel-surface: var(--iotsharp-surface, #fff);
	--panel-border: var(--iotsharp-border, #dfe7e4);
	--text-secondary: var(--iotsharp-text-soft, #52635f);
	width: min(100%, 1180px);
	min-width: 0;
	margin: 0 auto;
	padding: 0;
	color: var(--iotsharp-text, #23332f);
}

.inbox-header {
	display: flex;
	align-items: center;
	justify-content: space-between;
	gap: 12px;
	margin-bottom: 8px;

	h1 {
		margin: 0;
		font-size: 22px;
		font-weight: 740;
		line-height: 1.18;
		letter-spacing: -0.035em;
	}

	p {
		margin: 3px 0 0;
		color: var(--text-secondary);
		font-size: 13px;
	}
}

.inbox-count {
	display: flex;
	flex: none;
	align-items: center;
	gap: 6px;
	padding: 8px 12px;
	border: 1px solid color-mix(in srgb, var(--inbox-accent) 18%, var(--panel-border));
	border-radius: 6px;
	background: var(--inbox-surface);
	color: var(--inbox-accent);
	font-size: 13px;
	font-weight: 700;
}

.inbox-page .inbox-shell {
	display: grid;
	grid-template-columns: minmax(0, 1.15fr) minmax(0, 1fr);
	gap: 10px;
}

.inbox-list,
.inbox-detail {
	min-width: 0;
	border: 1px solid var(--panel-border);
	border-radius: var(--iotsharp-radius-panel, 8px);
	background: var(--panel-surface);
}

.inbox-toolbar,
.inbox-pagination {
	display: flex;
	flex-wrap: wrap;
	align-items: center;
	justify-content: space-between;
	gap: 6px 10px;
	padding: 7px 10px;
}

.inbox-toolbar {
	flex: 0 0 auto;
	border-bottom: 1px solid var(--panel-border);
}

.inbox-filters {
	display: flex;
	gap: 2px;
	padding: 3px;
	border-radius: 6px;
	background: var(--iotsharp-surface-muted, #f4f7f6);

	button {
		padding: 6px 9px;
		border: 1px solid transparent;
		border-radius: 4px;
		background: transparent;
		color: var(--text-secondary);
		font: inherit;
		font-size: 13px;
		cursor: pointer;
	}

	button[aria-pressed='true'] {
		border-color: color-mix(in srgb, var(--inbox-accent) 18%, var(--panel-border));
		background: var(--inbox-surface);
		color: var(--inbox-accent);
		font-weight: 700;
	}

	button:disabled {
		cursor: default;
	}
}

.inbox-items {
	margin: 0;
	padding: 0;
	list-style: none;

	li + li {
		border-top: 1px solid var(--panel-border);
	}
}

.message-entry {
	display: grid;
	width: 100%;
	grid-template-columns: 35px minmax(0, 1fr) auto;
	align-items: center;
	gap: 10px;
	padding: 8px 10px;
	border: 0;
	border-inline-start: 3px solid transparent;
	background: transparent;
	color: inherit;
	font: inherit;
	text-align: start;
	cursor: pointer;

	&:focus-visible {
		outline-offset: -3px;
	}

	&:hover,
	&[aria-current='true'] {
		background: var(--inbox-surface);
	}

	&[aria-current='true'] {
		border-inline-start-color: var(--inbox-accent);
	}

	&--unread .message-entry__title {
		font-weight: 750;
	}

	&:disabled {
		cursor: default;
	}
}

.category-icon,
.category-label {
	background: var(--inbox-surface);
	color: var(--inbox-accent);

	&--release {
		background: var(--release-surface);
		color: var(--release-accent);
	}
}

.category-icon {
	display: grid;
	width: 35px;
	aspect-ratio: 1;
	place-items: center;
	border-radius: 8px;
	font-size: 17px;
}

.message-entry__copy,
.message-entry__meta {
	display: grid;
	min-width: 0;
	gap: 4px;
}

.message-entry__title,
.message-entry__sender {
	overflow: hidden;
	text-overflow: ellipsis;
	white-space: nowrap;
}

.message-entry__title {
	font-size: 14px;
	font-weight: 550;
}

.message-entry__sender,
.message-entry__meta,
.inbox-pagination {
	color: var(--text-secondary);
	font-size: 12px;
}

.message-entry__meta {
	justify-items: end;
	align-self: start;
	white-space: nowrap;
}

.unread-dot {
	width: 7px;
	height: 7px;
	border-radius: 50%;
	background: var(--inbox-accent);
}

.inbox-pagination {
	border-top: 1px solid var(--panel-border);
}

.inbox-state,
.inbox-detail__empty {
	display: grid;
	min-height: 175px;
	margin: 0;
	padding: 20px;
	place-content: center;
	justify-items: center;
	gap: 10px;
	color: var(--text-secondary);
	text-align: center;

	p,
	h2 {
		margin: 0;
		font-size: 14px;
	}

	> .el-icon {
		color: var(--inbox-accent);
		font-size: 25px;
	}
}

.inbox-detail {
	min-height: 0;
	overflow: auto;
}

.inbox-detail__header {
	padding: clamp(15px, 2.2vw, 23px);
	border-bottom: 1px solid var(--panel-border);
	border-radius: var(--iotsharp-radius-panel, 8px) var(--iotsharp-radius-panel, 8px) 0 0;
	background: linear-gradient(135deg, var(--inbox-surface), var(--panel-surface));

	h2 {
		margin: 10px 0 6px;
		overflow-wrap: anywhere;
		font-size: clamp(17px, 2.1vw, 23px);
		line-height: 1.4;
	}
}

.inbox-detail__meta {
	display: flex;
	flex-wrap: wrap;
	gap: 5px 12px;
	color: var(--text-secondary);
	font-size: 12px;
}

.category-label {
	display: inline-block;
	padding: 3px 7px;
	border-radius: 4px;
	font-size: 12px;
	font-weight: 650;
}

.inbox-detail__body,
.inbox-detail__error,
.inbox-error {
	margin: 0;
	padding: clamp(15px, 2.2vw, 23px);
	overflow-wrap: anywhere;
	line-height: 1.65;
}

.inbox-detail__body {
	white-space: pre-wrap;
}

.inbox-detail__error,
.inbox-error {
	color: var(--iotsharp-danger, #bd3e3e);
	font-size: 13px;
}

.inbox-detail__error p {
	margin: 0 0 8px;
}

.inbox-detail__empty {
	min-height: 248px;
}

@media (max-width: 780px) {
	.inbox-page .inbox-shell {
		grid-template-columns: minmax(0, 1fr);
		grid-template-rows: minmax(min-content, 1fr) auto;
	}

	.inbox-detail,
	.inbox-detail__empty {
		min-height: 140px;
	}

}
</style>
