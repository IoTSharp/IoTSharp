import request from '/@/utils/request';

const apiBaseURL = (import.meta.env.VITE_API_URL || '').replace(/\/$/, '');

const postJson = async (url: string, body: object) => {
	const response = await fetch(`${apiBaseURL}${url}`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify(body),
	});
	if (!response.ok) {
		throw await response.json();
	}
	return response.json();
};

/**
 * Account-related APIs.
 * Login uses a raw fetch so the page can distinguish captcha failures from invalid credentials.
 */
export function useLoginApi() {
	return {
		signIn: (params: object) => postJson('/api/Account/Login', params),
		signOut: (params: object) => {
			return request({
				url: '/api/Account/Logout',
				method: 'post',
				data: params,
			});
		},
		GetUserInfo: (params: object) => {
			return request({
				url: '/api/Account/MyInfo',
				method: 'get',
				data: params,
			});
		},
	};
}
