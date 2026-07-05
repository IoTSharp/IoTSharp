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
	sort?: string;
}

export interface EdgeRuntimeStatus {
	contractVersion: string;
	edgeNodeId: string;
	gatewayId: string;
	active: boolean;
	lastActivityDateTime?: string;
	runtimeType: string;
	runtimeName: string;
	version: string;
	instanceId: string;
	platform: string;
	hostName: string;
	ipAddress: string;
	status: string;
	healthy?: boolean | null;
	uptimeSeconds?: number | null;
	lastHeartbeatDateTime?: string;
	lastRegistrationDateTime?: string;
	updatedAt?: string;
	metadata: Record<string, unknown>;
	metrics: Record<string, unknown>;
}

export interface EdgeTaskCapability {
	taskType: string;
	contractVersion: string;
	supportsProgress: boolean;
	supportsCancellation: boolean;
	metadata: Record<string, unknown>;
}

export interface EdgeContractCompatibility {
	contractName: string;
	contractVersion: string;
	minPlatformVersion: string;
	maxPlatformVersion: string;
	deprecated: boolean;
	metadata: Record<string, unknown>;
}

export interface EdgeCapability {
	contractVersion: string;
	edgeNodeId: string;
	gatewayId: string;
	runtimeType: string;
	runtimeName: string;
	version: string;
	instanceId: string;
	reportedAt?: string;
	updatedAt?: string;
	protocols: string[];
	supportedProtocols: string[];
	supportedPointTypes: string[];
	supportedTransforms: string[];
	supportedReportTriggers: string[];
	features: string[];
	tasks: string[];
	taskCapabilities: EdgeTaskCapability[];
	compatibleContracts: EdgeContractCompatibility[];
	metadata: Record<string, unknown>;
}

export interface EdgeTaskAddressPayload {
	targetType: 'EdgeNode' | 'GatewayRuntime' | 'DeviceScope';
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

export type EdgeCollectionAssignmentStatus = 'Pending' | 'Active' | 'Superseded' | 'Revoked';

export interface EdgeCollectionAssignment {
	contractVersion: string;
	id: string;
	targetType: 'EdgeNode' | 'GatewayRuntime' | 'DeviceScope';
	gatewayId: string;
	edgeNodeId?: string;
	targetKey: string;
	runtimeType: string;
	instanceId: string;
	configurationVersion: number;
	configurationHash: string;
	taskCount: number;
	status: EdgeCollectionAssignmentStatus;
	sourceType: string;
	sourceId: string;
	sourceVersion: string;
	assignedAt: string;
	lastPulledAt?: string;
	revokedAt?: string;
	createdAt: string;
	updatedAt: string;
	createdBy: string;
	updatedBy: string;
	metadata: Record<string, unknown>;
}

export interface EdgeCollectionAssignmentQueryParam extends IListQueryParam {
	gatewayId?: string;
	edgeNodeId?: string;
	targetType?: string;
	status?: EdgeCollectionAssignmentStatus;
	configurationVersion?: number;
	runtimeType?: string;
	targetKey?: string;
	sourceType?: string;
	sorter?: string;
	sort?: string;
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

export interface EdgeTaskReceipt {
	contractVersion: string;
	taskId: string;
	targetType: 'EdgeNode' | 'GatewayRuntime' | 'DeviceScope';
	targetKey: string;
	runtimeType: string;
	instanceId: string;
	status: string;
	message: string;
	reportedAt: string;
	progress?: number | null;
	result?: Record<string, unknown>;
	metadata?: Record<string, string>;
}

export interface EdgeTaskHistoryRecord {
	key: string;
	at: string;
	payload: string;
	status: string;
}

export interface EdgeTaskStateMachine {
	contractVersion: string;
	states: string[];
	transitions: Record<string, string[]>;
	terminalStates: string[];
}

export interface EdgeNodeCreatePayload {
	name: string;
	timeout: number;
	deviceType: 'Gateway';
	identityType: 'AccessToken' | 'DevicePassword' | 'ProductToken' | 'X509Certificate';
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
		getRuntimeStatus: (id: string) => {
			return request({
				url: `/api/Edge/${id}/RuntimeStatus`,
				method: 'get',
			});
		},
		getCapability: (id: string) => {
			return request({
				url: `/api/Edge/${id}/Capability`,
				method: 'get',
			});
		},
		getCollectionAssignments: (id: string, params?: EdgeCollectionAssignmentQueryParam) => {
			return request({
				url: `/api/Edge/${id}/CollectionAssignments`,
				method: 'get',
				params,
			});
		},
		createEdgeNode: (payload: EdgeNodeCreatePayload) => {
			return request({
				url: '/api/Devices',
				method: 'post',
				data: payload,
			});
		},
		getDeviceIdentity: (id: string) => {
			return request({
				url: `/api/Devices/${id}/Identity`,
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
