import { authDirective } from '/@/utils/authDirective';

import { IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';

/**
 * 设备api接口集合
 * @method devcieList 设备列表
 * @method getdevcie 获取设备
 * @method postdevcie 新增设备
 * @method putdevcie 修改设备
 * @method deletedevcie 删除设备
 */
export function deviceApi() {
	return {
		devcieList: (params: QueryParam) => {
			return request({
				url:
					'/api/Devices/Customers?offset=' +
					params.offset +
					'&limit=' +
					params.limit +
					'&sorter=&onlyActive=' +
					params.onlyActive +
					'&customerId=' +
					params.customerId +
					'&name=' +
					params.name +
					'&sort=',
				method: 'get',
				data: params,
			});
		},
		getdevcie: (deviceId: string) => {
			return request({
				url: '/api/Devices/' + deviceId,
				method: 'get',
			});
		},

		postdevcie: (params: any) => {
			return request({
				url: '/api/Devices',
				method: 'post',
				data: params,
			});
		},

		putdevcie: (params: any) => {
			return request({
				url: '/api/Devices/' + params.id,
				method: 'put',
				data: params,
			});
		},
		deletedevcie: (id: string) => {
			return request({
				url: '/api/Devices/' + id,
				method: 'delete',
			});
		},
		getDeviceAttributes: (deviceId: string) => {
			return request({
				url: '/api/Devices/' + deviceId + '/AttributeLatest',
				method: 'get',
			});
		},
		addDeviceAttributes: (deviceId: string, params: any) => {
			return request({
				url: '/api/Devices/' + deviceId + '/AddAttribute',
				method: 'post',
				data: params,
			});
		},
		// 获取设备规则
		getDeviceRules: (deviceId: string) => {
			return request({
				url: '/api/rules/getDeviceRules?deviceId=' + deviceId,
				method: 'get',
			});
		},
		// 设置规则
		setDeviceRules: (deviceId: string, ruleId: any) => {
			return request({
				url: '/api/rules/binddevice',
				method: 'post',
				data: {
					dev: [deviceId],
					rule: ruleId,
				},
			});
		},
		deleteDeviceRules: (deviceId: string, ruleId: string) => {
			return request({
				url: '/api/rules/DeleteDeviceRules',
				method: 'get',
				params: {
					deviceId,
					ruleId,
				},
			});
		},
	};
}

interface QueryParam extends IListQueryParam {
	onlyActive?: boolean;
	customerId?: string;
	name?: string;
}
