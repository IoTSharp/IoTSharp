import request from '/@/utils/request';
import { IListQueryParam } from '../iapiresult';

export type ReleasePlanType = 'SoftwareUpdate' | 'ConfigurationRollout' | 'DeviceScriptOta' | 'FirmwareOta';
export type ReleasePlanStatus =
	| 'Draft'
	| 'WaitingConfirmation'
	| 'Running'
	| 'Paused'
	| 'Succeeded'
	| 'PartiallySucceeded'
	| 'Failed'
	| 'Cancelled'
	| 'RollingBack'
	| 'RolledBack'
	| 'RollbackFailed';
export type ReleaseConfirmationPolicy = 'None' | 'ManualBeforeStart' | 'ManualBetweenBatches';
export type ReleaseTargetType = 'EdgeNode' | 'Gateway' | 'Device' | 'AssetScope' | 'DeviceScope';
export type ReleaseTaskStatus = 'Pending' | 'Sent' | 'Accepted' | 'Running' | 'Succeeded' | 'Failed' | 'TimedOut' | 'Cancelled' | 'RolledBack' | 'RollbackFailed';

export interface ReleaseTask {
	id: string;
	targetType: ReleaseTargetType;
	targetId?: string;
	edgeNodeId?: string;
	gatewayId?: string;
	targetKey: string;
	runtimeType: string;
	instanceId: string;
	batchNo: number;
	status: ReleaseTaskStatus;
	isRollback: boolean;
	edgeTaskId?: string;
	message: string;
	progress?: number;
	createdAt: string;
	updatedAt: string;
	dispatchedAt?: string;
	completedAt?: string;
}

export interface ReleasePlan {
	id: string;
	name: string;
	description: string;
	planType: ReleasePlanType;
	status: ReleasePlanStatus;
	packageId?: string;
	rollbackPackageId?: string;
	configurationVersionId?: string;
	confirmationPolicy: ReleaseConfirmationPolicy;
	batchSize: number;
	continueOnFailure: boolean;
	totalTaskCount: number;
	pendingTaskCount: number;
	runningTaskCount: number;
	succeededTaskCount: number;
	failedTaskCount: number;
	currentBatchNo: number;
	createdAt: string;
	updatedAt: string;
	startedAt?: string;
	completedAt?: string;
	createdBy: string;
	updatedBy: string;
	tasks?: ReleaseTask[];
}

export interface ReleaseTarget {
	targetType: ReleaseTargetType;
	targetId: string;
	runtimeType?: string;
	instanceId?: string;
	targetKey?: string;
}

export interface ReleasePlanCreateRequest {
	name: string;
	description?: string;
	planType: ReleasePlanType;
	packageId?: string;
	configurationVersionId?: string;
	rollbackPackageId?: string;
	rollbackConfigurationVersionId?: string;
	confirmationPolicy: ReleaseConfirmationPolicy;
	strategy: { batchSize: number; continueOnFailure: boolean };
	autoStart: boolean;
	targets: ReleaseTarget[];
}

export interface ReleasePlanActionRequest {
	reason?: string;
	rollbackPackageId?: string;
	rollbackConfigurationVersionId?: string;
	force?: boolean;
}

export interface ReleasePackage {
	id: string;
	packageType: string;
	packageKey: string;
	name: string;
	version: string;
	targetRuntimeType: string;
	sha256: string;
	createdAt: string;
}

export interface CollectionConfigurationVersion {
	id: string;
	version: number;
	configurationHash: string;
	createdAt: string;
}

export function releaseApi() {
	return {
		listPlans: (params: IListQueryParam & { name?: string; status?: ReleasePlanStatus; planType?: ReleasePlanType }) =>
			request({ url: '/api/ReleaseCenter/Plans', method: 'get', params }),
			getPlan: (id: string) => request({ url: `/api/ReleaseCenter/Plans/${id}`, method: 'get' }),
			createPlan: (payload: ReleasePlanCreateRequest) => request({ url: '/api/ReleaseCenter/Plans', method: 'post', data: payload }),
			startPlan: (id: string, payload: ReleasePlanActionRequest = {}) => request({ url: `/api/ReleaseCenter/Plans/${id}/Start`, method: 'post', data: payload }),
			confirmPlan: (id: string, payload: ReleasePlanActionRequest = {}) => request({ url: `/api/ReleaseCenter/Plans/${id}/Confirm`, method: 'post', data: payload }),
			pausePlan: (id: string, payload: ReleasePlanActionRequest = {}) => request({ url: `/api/ReleaseCenter/Plans/${id}/Pause`, method: 'post', data: payload }),
			resumePlan: (id: string, payload: ReleasePlanActionRequest = {}) => request({ url: `/api/ReleaseCenter/Plans/${id}/Resume`, method: 'post', data: payload }),
			rollbackPlan: (id: string, payload: ReleasePlanActionRequest = {}) => request({ url: `/api/ReleaseCenter/Plans/${id}/Rollback`, method: 'post', data: payload }),
			listReceipts: (id: string) => request({ url: `/api/ReleaseCenter/Plans/${id}/Receipts`, method: 'get' }),
			listPackages: (params: IListQueryParam & { name?: string; version?: string; runtimeType?: string }) => request({ url: '/api/ReleasePackages', method: 'get', params }),
			listConfigurationVersions: (gatewayId: string) => request({ url: `/api/Edge/${gatewayId}/CollectionConfigVersions`, method: 'get', params: { offset: 0, limit: 100 } }),
	};
}
