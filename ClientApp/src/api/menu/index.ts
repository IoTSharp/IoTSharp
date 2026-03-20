import request from '/@/utils/request';

/**
 * Menu API helpers for backend-driven navigation.
 * `getMenuTest` points to a local mock so the project no longer depends on template assets.
 */
export function useMenuApi() {
	return {
		getMenuAdmin: (params?: object) => {
			return request({
				url: '/api/Menu/GetProfile',
				method: 'get',
				params,
			});
		},
		getMenuTest: (params?: object) => {
			return request({
				url: '/mock/menu/testMenu.json',
				method: 'get',
				params,
			});
		},
	};
}
