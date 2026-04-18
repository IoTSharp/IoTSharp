export type CollectionProtocolType = 'Unknown' | 'Modbus' | 'OpcUa' | 'Bacnet' | 'IEC104' | 'Mqtt' | 'Custom';

export type CollectionTargetType = 'Telemetry' | 'Attribute' | 'AlarmInput' | 'CommandFeedback';

export type CollectionValueType = 'Boolean' | 'Int32' | 'Int64' | 'Double' | 'Decimal' | 'String' | 'Enum' | 'Json';

export type CollectionTransformType =
	| 'Scale'
	| 'Offset'
	| 'Expression'
	| 'EnumMap'
	| 'BitExtract'
	| 'WordSwap'
	| 'ByteSwap'
	| 'Clamp'
	| 'DefaultOnError';

export type ReportTriggerType = 'Always' | 'OnChange' | 'Deadband' | 'Interval' | 'QualityChange';

export interface CollectionTaskModel {
	id?: string;
	taskKey: string;
	protocol: CollectionProtocolType;
	version: number;
	edgeNodeId?: string;
	connection: ProtocolConnectionModel;
	devices: ProtocolDeviceModel[];
	reportPolicy: ReportPolicyModel;
}

export interface ProtocolConnectionModel {
	connectionKey: string;
	connectionName: string;
	protocol: CollectionProtocolType;
	transport?: string;
	host?: string;
	port?: number;
	serialPort?: string;
	timeoutMs: number;
	retryCount: number;
	protocolOptions?: Record<string, any>;
}

export interface ProtocolDeviceModel {
	deviceKey: string;
	deviceName: string;
	enabled: boolean;
	externalKey?: string;
	protocolOptions?: Record<string, any>;
	points: CollectionPointModel[];
}

export interface CollectionPointModel {
	pointKey: string;
	pointName: string;
	sourceType: string;
	address: string;
	rawValueType: string;
	length: number;
	polling: PollingPolicyModel;
	transforms: ValueTransformModel[];
	mapping: PlatformMappingModel;
	protocolOptions?: Record<string, any>;
	description?: string;
	previewRawValue?: unknown;
	previewTransformedValue?: unknown;
}

export interface PollingPolicyModel {
	readPeriodMs: number;
	group?: string;
}

export interface ValueTransformModel {
	transformType: CollectionTransformType;
	order: number;
	parameters?: Record<string, any>;
}

export interface PlatformMappingModel {
	targetType: CollectionTargetType;
	targetName: string;
	valueType: CollectionValueType;
	displayName?: string;
	unit?: string;
	group?: string;
	precision?: number;
}

export interface ReportPolicyModel {
	defaultTrigger: ReportTriggerType;
	deadband?: number;
	includeQuality: boolean;
	includeTimestamp: boolean;
}

export interface TaskPreviewRequestModel {
	protocol: CollectionProtocolType;
	connection: ProtocolConnectionModel;
	device: ProtocolDeviceModel;
	point: CollectionPointModel;
}

export interface TaskPreviewResponseModel {
	success: boolean;
	rawValue?: unknown;
	transformedValue?: unknown;
	valueType?: CollectionValueType;
	qualityStatus?: 'Good' | 'Uncertain' | 'Bad' | 'CommError' | 'InvalidData';
	errorCode?: string;
	errorMessage?: string;
}