import request from '/@/utils/request';

interface Result<T> {
	data: T;
}

export interface PersonalProfile {
	userName: string | null;
	email: string | null;
	phoneNumber: string | null;
	displayName: string;
	introduction: string;
	tenantName: string | null;
	customerName: string | null;
	roles: string[];
	avatar: string;
	hasCustomAvatar: boolean;
}

export interface PersonalProfileUpdate {
	displayName: string;
	introduction: string;
	phoneNumber: string;
}

export interface InboxMessage {
	id: string;
	title: string;
	body: string;
	category: string;
	sender: string;
	createdAt: string;
	readAt: string | null;
}

interface InboxPage {
	items: InboxMessage[];
	total: number;
	unreadCount: number;
}

interface UnreadCount {
	unreadCount: number;
}

const base = '/api/personal-center';

export const inboxUnreadEvent = 'iotsharp:inbox-unread-changed';

export async function getPersonalProfile() {
	const result = await request.get<unknown, Result<PersonalProfile>>(`${base}/profile`);
	return result.data;
}

export async function savePersonalProfile(profile: PersonalProfileUpdate) {
	const result = await request.put<unknown, Result<PersonalProfile>>(`${base}/profile`, profile);
	return result.data;
}

export async function uploadPersonalAvatar(file: File) {
	const form = new FormData();
	form.append('file', file);
	const result = await request.post<unknown, Result<PersonalProfile>>(`${base}/avatar`, form, {
		headers: { 'Content-Type': 'multipart/form-data' },
	});
	return result.data;
}

export async function removePersonalAvatar() {
	const result = await request.delete<unknown, Result<PersonalProfile>>(`${base}/avatar`);
	return result.data;
}

export async function getInbox(page: number, pageSize: number, unreadOnly: boolean) {
	const result = await request.get<unknown, Result<InboxPage>>(`${base}/inbox`, { params: { page, pageSize, unreadOnly } });
	return result.data;
}

export async function getInboxUnreadCount() {
	const result = await request.get<unknown, Result<UnreadCount>>(`${base}/inbox/unread-count`);
	return result.data.unreadCount;
}

export async function markInboxMessageRead(id: string) {
	const result = await request.put<unknown, Result<UnreadCount>>(`${base}/inbox/${encodeURIComponent(id)}/read`);
	return result.data.unreadCount;
}

export async function markInboxRead() {
	const result = await request.put<unknown, Result<UnreadCount>>(`${base}/inbox/read-all`);
	return result.data.unreadCount;
}
