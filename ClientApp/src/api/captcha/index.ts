const apiBaseURL = (import.meta.env.VITE_API_URL || '').replace(/\/$/, '');

const getJson = async (url: string) => {
	const response = await fetch(`${apiBaseURL}${url}`, {
		method: 'GET',
		headers: { Accept: 'application/json' },
	});
	if (!response.ok) {
		throw await response.json();
	}
	return response.json();
};

/**
 * Captcha endpoints used by the login screen.
 */
export function useCaptchaApi() {
	return {
		getChallenge: (clientId: string) => getJson(`/api/Captcha/Index?clientid=${encodeURIComponent(clientId)}`),
	};
}
