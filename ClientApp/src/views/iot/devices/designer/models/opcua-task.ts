import type { CollectionPointModel, ProtocolConnectionModel, ProtocolDeviceModel } from './collection-task';

export interface OpcUaConnectionModel extends ProtocolConnectionModel {
	protocol: 'OpcUa';
	transport?: 'Tcp';
	protocolOptions?: {
		endpointUrl: string;
		securityPolicy?: string;
		securityMode?: 'None' | 'Sign' | 'SignAndEncrypt';
		username?: string;
		useAnonymous?: boolean;
		sessionTimeoutMs?: number;
	};
}

export interface OpcUaDeviceModel extends ProtocolDeviceModel {
	protocolOptions?: {
		namespaceUri?: string;
		namespaceIndex?: number;
		browseRoot?: string;
	};
}

export interface OpcUaPointOptionsModel {
		nodeId: string;
		browsePath?: string;
		attributeId?: number;
		samplingIntervalMs?: number;
		publishingIntervalMs?: number;
		deadband?: number;
		subscriptionMode?: 'Subscribe' | 'Poll';
}

export interface OpcUaPointModel extends CollectionPointModel {
	protocolOptions: OpcUaPointOptionsModel;
	expression?: string;
	qualityPolicy?: Record<string, any>;
	reportPolicy?: Record<string, any>;
}