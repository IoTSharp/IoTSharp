import { defineStore } from 'pinia';
import Cookies from 'js-cookie';
import { UserInfosStates } from './interface';
import { Session } from '/@/utils/storage';
import { useLoginApi } from '../api/login';
import { resolve } from 'dns/promises';

/**
 * 用户信息
 * @methods setUserInfos 设置用户信息
 */
export const useUserInfo = defineStore('userInfo', {
	state: (): UserInfosStates => ({
		userInfos: {
			userName: '',
			photo: '',
			time: 0,
			roles: [],
			authBtnList: [],
			customer: {},
			tenant: {}
		},
	}),
	actions: {
		async setUserInfos() {
			// 存储用户信息到浏览器缓存
			if (Session.get('userInfo')) {
				this.userInfos = Session.get('userInfo');
			} else {
				const userInfos: any = await this.getApiUserInfo();
				this.userInfos = userInfos;
				Session.set('userInfo', userInfos);
			}
		},
		// 模拟接口数据
		// https://gitee.com/lyt-top/vue-next-admin/issues/I5F1HP
		async getApiUserInfo() {
			return useLoginApi()
				.GetUserInfo({})
				.then((res) => {
					const userInfos = {
						userName: res.data.name,
						photo: res.data.avatar,
						time: new Date().getTime(),
						roles: [res.data.roles],
						authBtnList: ['btn.add', 'btn.del', 'btn.edit', 'btn.link'],
						customer: res.data.customer,
						tenant: res.data.tenant,
					};
					return userInfos;
				})
				.catch();
		},
	},
});
