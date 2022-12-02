import { IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';

/**
 * 资产管理api接口集合
 * @method assetList 资产列表
 * @method postAsset 新增资产
 * @method putAsset 修改资产
 * @method deleteAsset 删除资产
 */
export function assetApi() {
	return {
		assetList: (params: QueryParam) => {
			return request({
				url: `/api/asset/list`,
				method: 'get',
				params,
			});
		},

		postAsset: (params: any) => {
			return request({
				url: '/api/Asset/Save',
				method: 'post',
				data: params,
			});
		},

		putAsset: (params: any) => {
			return request({
				url: '/api/Asset/Update',
				method: 'put',
				data: params,
			});
		},
		deleteAsset: (id: string) => {
			return request({
				url: '/api/Asset/Delete' + id,
				method: 'delete',
				data: {},
			});
		},
	};
}

export interface QueryParam extends IListQueryParam {
	name?: string;
}
