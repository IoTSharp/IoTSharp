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
				data: params,
			});
		},
		gettenant: (tenantId: string) => {
			return request({
				url: '/api/tenants/' + tenantId,
				method: 'get',
			});
		},

		posttenant: (params: any) => {
			return request({
				url: '/api/tenants',
				method: 'post',
				data: params,
			});
		},

		puttenant: (params: any) => {
			return request({
				url: '/api/tenants/' + params.id,
				method: 'put',
				data: params,
			});
		},
		deletetenant: (id: string) => {
			return request({
				url: '/api/tenants/' + id,
				method: 'delete',
				data: {},
			});
		},
	};
}

interface QueryParam extends IListQueryParam {
	name?: string;
}
