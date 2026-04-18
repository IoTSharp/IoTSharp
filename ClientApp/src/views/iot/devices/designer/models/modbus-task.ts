import type { CollectionPointModel, ProtocolConnectionModel, ProtocolDeviceModel } from './collection-task';

export type ModbusAreaType = 'Coil' | 'DiscreteInput' | 'HoldingRegister' | 'InputRegister';
export type ModbusByteOrder = 'AB' | 'BA' | 'ABCD' | 'BADC' | 'CDAB' | 'DCBA';
export type ModbusWordOrder = 'AB' | 'BA' | 'ABCD' | 'BADC' | 'CDAB' | 'DCBA';

export interface ModbusConnectionModel extends ProtocolConnectionModel {
	protocol: 'Modbus';
	transport: 'Tcp' | 'RtuOverTcp' | 'SerialRtu';
	protocolOptions?: {
		maxBatchRegisters?: number;
		maxConcurrentRequests?: number;
		baudRate?: number;
		dataBits?: number;
		stopBits?: number;
		parity?: 'None' | 'Odd' | 'Even';
		stationName?: string;
	};
}

export interface ModbusDeviceModel extends ProtocolDeviceModel {
	protocolOptions?: {
		slaveId: number;
		pollingGroup?: string;
	};
}

export interface ModbusPointOptionsModel {
	slaveId: number;
	functionCode: 1 | 2 | 3 | 4 | 5 | 6 | 15 | 16;
	area: ModbusAreaType;
	registerCount: number;
	byteOrder?: ModbusByteOrder;
	wordOrder?: ModbusWordOrder;
	bitOffset?: number;
	bitLength?: number;
	stringEncoding?: 'ASCII' | 'UTF8' | 'GBK';
	displayFormat?: string;
}

export interface ModbusPointModel extends CollectionPointModel {
	protocolOptions: ModbusPointOptionsModel;
	scale?: number;
	offset?: number;
	expression?: string;
	enumMapping?: Record<string, string>;
	qualityPolicy?: Record<string, any>;
	reportPolicy?: Record<string, any>;
}

export interface LegacyModbusMappingModel {
	_id?: string;
	id?: string;
	code?: number;
	dataName?: string;
	dataType?: string;
	dataCatalog?: string;
	funCode?: string;
	address?: number;
	length?: number;
	dataFormat?: string;
	codePage?: number;
}

export interface ModbusDesignerUpgradePlan {
	keep: Array<'_id' | 'id' | 'dataName' | 'address' | 'length'>;
	replace: Record<string, string | string[]>;
	add: string[];
}

export const modbusDesignerUpgradePlan: ModbusDesignerUpgradePlan = {
	keep: ['_id', 'id', 'dataName', 'address', 'length'],
	replace: {
		code: 'slaveId',
		funCode: ['area', 'functionCode'],
		dataType: 'rawValueType',
		dataCatalog: 'targetType',
		dataFormat: ['byteOrder', 'wordOrder', 'displayFormat'],
		codePage: 'stringEncoding',
	},
	add: [
		'pointKey',
		'pointName',
		'readPeriodMs',
		'bitOffset',
		'bitLength',
		'scale',
		'offset',
		'expression',
		'targetName',
		'targetValueType',
		'displayName',
		'unit',
		'precision',
		'description',
		'previewRawValue',
		'previewTransformedValue',
	],
};