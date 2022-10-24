import { IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';

/**
 * 登录api接口集合
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
				url: '/api/Devices/Customers?offset='+params.offset+'&limit='+params.limit+'&sorter=&onlyActive=false&customerId=81a02fb6-914e-41dc-a1b6-98ffb8d31fc0&name=&sort=',
				method: 'get',
				data: params,
			});
		},
		getdevcie: (deviceId:string) => {
			return request({
				url: '/api/Devices/'+deviceId,
				method: 'get',
			});
		},

		postdevcie: (params: object) => {
			return request({
				url: '/api/Devices',
				method: 'post',
				data: params,
			});
		},

        putdevcie: (params: object) => {
			return request({
				url: '/api/Devices',
				method: 'put',
				data: params,
			});
		},
        deletedevcie: (params: object) => {
			return request({
				url: '/api/Devices',
				method: 'delete',
				data: params,
			});
		},
	};
}


interface QueryParam extends IListQueryParam{
	onlyActive?:boolean
	customerId?:string
	name?:string
}
