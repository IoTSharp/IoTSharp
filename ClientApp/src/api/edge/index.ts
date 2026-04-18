import request from '/@/utils/request';
import { IListQueryParam } from '../iapiresult';

export interface EdgeNodeQueryParam extends IListQueryParam {
	name?: string;
	runtimeType?: string;
	status?: string;
	healthy?: boolean;
	active?: boolean;
	version?: string;
	platform?: string;
	sorter?: string;
}

export function edgeApi() {
	return {
		getEdgeList: (params: EdgeNodeQueryParam) => {
			return request({
				url: '/api/Edge',
				method: 'get',
				params,
			});
		},
		getEdgeDetail: (id: string) => {
			return request({
				url: `/api/Edge/${id}`,
				method: 'get',
			});
		},
	};
}