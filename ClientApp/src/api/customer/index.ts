import { IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';

/**
 * 客户api接口集合
 * @method customerList 客户列表
 * @method getCustomer 获取客户
 * @method postCustomer 新增客户
 * @method putCustomer 修改客户
 * @method deleteCustomer 删除客户
 */
export function customerApi() {
	return {
		customerList: (params: QueryParam) => {
			return request({
				url: `/api/Customers/Tenant`,
				method: 'post',
				data: params,
			});
		},
		getCustomer: (customerId: string) => {
			return request({
				url: '/api/Customers/' + customerId,
				method: 'get',
			});
		},

		postCustomer: (params: any) => {
			return request({
				url: '/api/Customers',
				method: 'post',
				data: params,
			});
		},

		putCustomer: (params: any) => {
			return request({
				url: '/api/Customers/' + params.id,
				method: 'put',
				data: params,
			});
		},
		deleteCustomer: (id: string) => {
			return request({
				url: '/api/Customers/' + id,
				method: 'delete',
				data: {},
			});
		},
	};
}

export interface QueryParam extends IListQueryParam {
	name?: string;
	tenantId?: string;
}
