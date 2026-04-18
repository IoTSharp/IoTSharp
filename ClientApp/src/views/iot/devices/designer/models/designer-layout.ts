export interface DesignerSectionSchema {
	id: string;
	title: string;
	description: string;
	protocols: string[];
	component: string;
}

export interface DesignerPageSchema {
	protocol: string;
	sections: DesignerSectionSchema[];
}

export const commonDesignerSections: DesignerSectionSchema[] = [
	{
		id: 'connection',
		title: '连接配置',
		description: '维护连接、超时、重试和协议连接参数。',
		protocols: ['Modbus', 'OpcUa', 'Bacnet', 'IEC104', 'Mqtt', 'Custom'],
		component: 'ConnectionConfigPanel',
	},
	{
		id: 'devices',
		title: '设备分组',
		description: '维护从站、命名空间分组或逻辑设备。',
		protocols: ['Modbus', 'OpcUa', 'Bacnet', 'IEC104', 'Mqtt', 'Custom'],
		component: 'DeviceGroupPanel',
	},
	{
		id: 'points',
		title: '点位清单',
		description: '维护采集点地址、类型、周期和启停。',
		protocols: ['Modbus', 'OpcUa', 'Bacnet', 'IEC104', 'Mqtt', 'Custom'],
		component: 'PointListPanel',
	},
	{
		id: 'transforms',
		title: '转换规则',
		description: '维护缩放、表达式、枚举和质量处理规则。',
		protocols: ['Modbus', 'OpcUa', 'Bacnet', 'IEC104', 'Mqtt', 'Custom'],
		component: 'TransformPanel',
	},
	{
		id: 'mapping',
		title: '平台映射',
		description: '维护属性/遥测目标、值类型和单位。',
		protocols: ['Modbus', 'OpcUa', 'Bacnet', 'IEC104', 'Mqtt', 'Custom'],
		component: 'PlatformMappingPanel',
	},
	{
		id: 'preview',
		title: '试读预览',
		description: '查看原始值、转换结果和上报预览。',
		protocols: ['Modbus', 'OpcUa', 'Bacnet', 'IEC104', 'Mqtt', 'Custom'],
		component: 'PreviewPanel',
	},
];

export const protocolDesignerLayouts: DesignerPageSchema[] = [
	{
		protocol: 'Modbus',
		sections: commonDesignerSections,
	},
	{
		protocol: 'OpcUa',
		sections: commonDesignerSections,
	},
];