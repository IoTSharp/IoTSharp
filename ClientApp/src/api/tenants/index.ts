import { IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';

/**
 * 租户api接口集合
 * @method tenantList 租户列表
 * @method gettenant 获取租户
 * @method posttenant 新增租户
 * @method puttenant 修改租户
 * @method deletetenant 删除租户
 */
export function tenantApi() {
	return {
		tenantList: (params: QueryParam) => {
			return request({
				url: '/api/Tenants',
				method: 'get',
				params,
			});
		},
		gettenant: (tenantId: string) => {
			return request({
				url: '/api/Tenants/' + tenantId,
				method: 'get',
			});
		},

		posttenant: (params: any) => {
			return request({
				url: '/api/Tenants',
				method: 'post',
				data: params,
			});
		},

		puttenant: (params: any) => {
			return request({
				url: '/api/Tenants/' + params.id,
				method: 'put',
				data: params,
			});
		},
		deletetenant: (id: string) => {
			return request({
				url: '/api/Tenants/' + id,
				method: 'delete',
				data: {},
			});
		},
	};
}

export interface QueryParam extends IListQueryParam {
	name?: string;
}
