import { defineStore } from 'pinia';
import {AppInfo} from "/@/stores/interface";
export const useAppInfo = defineStore('appInfo', {
    state: () => ({
		appInfo: {} as AppInfo,
	}),
    actions: {
        setAppInfo(data: AppInfo) {
            this.appInfo = data
        },
    },
});
