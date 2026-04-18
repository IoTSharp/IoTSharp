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

export interface EdgeTaskAddressPayload {
	targetType: 'EdgeNode' | 'GatewayRuntime' | 'PixiuRuntime' | 'DeviceScope';
	deviceId?: string;
	accessToken?: string;
	runtimeType?: string;
	instanceId?: string;
	targetKey: string;
}

export interface EdgeTaskRequestPayload {
	contractVersion: string;
	taskId: string;
	taskType: 'ConfigPush' | 'ConfigPullRequest' | 'PackageDownload' | 'PackageApply' | 'RestartRuntime' | 'HealthProbe';
	address: EdgeTaskAddressPayload;
	createdAt: string;
	expireAt?: string;
	parameters?: Record<string, unknown>;
	metadata?: Record<string, string>;
}

export interface EdgeTaskListQueryParam extends IListQueryParam {
	name?: string;
	status?: string;
	runtimeType?: string;
}

export interface EdgeTaskTimelineNode {
	category: string;
	status: string;
	message: string;
	at: string;
	payload: string;
}

export interface EdgeTaskTimeline {
	deviceId: string;
	deviceName: string;
	taskId: string;
	runtimeType: string;
	instanceId: string;
	currentStatus: string;
	lastUpdatedAt: string;
	events: EdgeTaskTimelineNode[];
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
		getLatestReceipt: (deviceId: string) => {
			return request({
				url: `/api/EdgeTask/Receipt/${deviceId}`,
				method: 'get',
			});
		},
		getReceiptHistory: (deviceId: string) => {
			return request({
				url: `/api/EdgeTask/History/${deviceId}`,
				method: 'get',
			});
		},
		getStateMachine: () => {
			return request({
				url: '/api/EdgeTask/StateMachine',
				method: 'get',
			});
		},
		submitTask: (payload: EdgeTaskRequestPayload) => {
			return request({
				url: '/api/EdgeTask/Dispatch',
				method: 'post',
				data: payload,
			});
		},
		getTaskList: (params: EdgeTaskListQueryParam) => {
			return request({
				url: '/api/EdgeTask/List',
				method: 'get',
				params,
			});
		},
	};
}