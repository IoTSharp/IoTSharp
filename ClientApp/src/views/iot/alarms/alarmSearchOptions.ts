export const alarmStatusOptions = [
	{
		value: '-1',
		label: '全部',
	},
	{
		value: '0',
		label: '激活未应答',
	},
	{
		value: '1',
		label: '激活已应答',
	},
	{
		value: '2',
		label: '清除未应答',
	},
	{
		value: '3',
		label: '清除已应答',
	},
];

export const serverityOptions = [
	{
		value: '-1',
		label: '全部',
	},
	{
		value: '0',
		label: '不确定',
	},
	{
		value: '1',
		label: '警告',
	},
	{
		value: '2',
		label: '次要',
	},
	{
		value: '3',
		label: '重要',
	},
	{
		value: '4',
		label: '错误',
	},
];

export const originatorTypeOptions = [
	{
		value: '-1',
		label: '全部',
	},
	{
		value: '0',
		label: '未知',
	},
	{
		value: '1',
		label: '设备',
	},
	{
		value: '2',
		label: '网关',
	},
	{
		value: '3',
		label: '资产',
	},
];

export const serverityBadge = new Map([
	['Indeterminate', { text: '不确定', color: '#8c8c8c' }],
	['Warning', { text: '警告', color: '#faad14' }],
	['Minor', { text: '次要', color: '#bae637' }],
	['Major', { text: '主要', color: '#1890ff' }],
	['Critical', { text: '错误', color: '#f5222d' }],
]);

export const alarmStatusTAG = new Map([
	['Active_UnAck', { text: '激活未应答', color: '#ffa39e' }],
	['Active_Ack', { text: '激活已应答', color: '#f759ab' }],
	['Cleared_UnAck', { text: '清除未应答', color: '#87e8de' }],
	['Cleared_Act', { text: '清除已应答', color: '#7cb305' }],
]);

export const originatorTypeTAG = new Map([
	['Unknow', { text: '未知', color: '#ffa39e' }],
	['Device', { text: '设备', color: '#f759ab' }],
	['Gateway', { text: '网关', color: '#87e8de' }],
	['Asset', { text: '资产', color: '#d3f261' }],
]);
