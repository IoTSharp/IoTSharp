import { IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';

/**
 * 租户api接口集合
 * @method ruleList 租户列表
 * @method getrule 获取租户
 * @method postrule 新增租户
 * @method putrule 修改租户
 * @method deleterule 删除租户
 */
 export function ruleApi() {
	return {
		ruleList: (params: QueryParam) => {
			return request({
				url: '/api/rules/Index',
				method: 'post',
				data: params,
			});
		},
		getrule: (ruleId:string) => {
			return request({
				url: '/api/rules/Get?id='+ruleId,
				method: 'get',
			});
		},

		postrule: (params: any) => {
			return request({
				url: '/api/rules/Save',
				method: 'post',
				data: params,
			});
		},

        putrule: (params: any) => {
			return request({
				url: '/api/rules/Update',
				method: 'post',
				data: params,
			});
		},
        deleterule: (id: string) => {
			return request({
				url: '/api/rules/Delete?id='+id,
				method: 'get',
				data: {},
			});
		},
		getexecutors: () => {
			return request({
				url: '/api/rules/getexecutors',
				method: 'get',
				data: {},
			});
		},


		getDiagram: (id: string) => {
			return request({
				url: '/api/rules/GetDiagram?id'+id,
				method: 'get',
				data: {},
			});
		},
	};
}

interface QueryParam extends IListQueryParam{
	name?:string
}
