import Cookies from 'js-cookie';

/**
 * Thin wrapper around localStorage so IoTSharp can keep its own namespace.
 */
export const Local = {
	setKey(key: string) {
		// @ts-ignore
		// Prefix persisted keys to avoid collisions across sibling apps.
		return `${__IOTSHARP_APP_ID__}:${key}`;
	},
	set<T>(key: string, val: T) {
		window.localStorage.setItem(Local.setKey(key), JSON.stringify(val));
	},
	get(key: string) {
		let json = <string>window.localStorage.getItem(Local.setKey(key));
		return JSON.parse(json);
	},
	remove(key: string) {
		window.localStorage.removeItem(Local.setKey(key));
	},
	clear() {
		window.localStorage.clear();
	},
};

/**
 * Session-scoped storage that keeps auth and transient UI state together.
 */
export const Session = {
	set<T>(key: string, val: T) {
		if (key === 'token') return Cookies.set(key, val);
		window.sessionStorage.setItem(Local.setKey(key), JSON.stringify(val));
	},
	get(key: string) {
		if (key === 'token') return Cookies.get(key);
		let json = <string>window.sessionStorage.getItem(Local.setKey(key));
		return JSON.parse(json);
	},
	remove(key: string) {
		if (key === 'token') return Cookies.remove(key);
		window.sessionStorage.removeItem(Local.setKey(key));
	},
	clear() {
		Cookies.remove('token');
		window.sessionStorage.clear();
	},
};
