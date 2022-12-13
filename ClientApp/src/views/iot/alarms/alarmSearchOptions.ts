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
		key: 'All',
	},
	{
		value: '0',
		label: '未知',
		key: 'Unknown',
	},
	{
		value: '1',
		label: '设备',
		key: 'Device',
	},
	{
		value: '2',
		label: '网关',
		key: 'Gateway',
	},
	{
		value: '3',
		label: '资产',
		key: 'Asset',
	},
];

export const serverityBadge = new Map([
	['Indeterminate', { text: '不确定', color: 'var(--el-color-info)' }],
	['Warning', { text: '警告', color: 'var(--el-color-warning)' }],
	['Minor', { text: '次要', color: 'var(--el-color-primary)' }],
	['Major', { text: '主要', color: 'var(--el-color-primary-dark-2)' }],
	['Critical', { text: '错误', color: 'var(--el-color-error)' }],
]);

export const alarmStatusTAG = new Map([
	['Active_UnAck', { text: '激活未应答', color: 'var(--el-color-error)' }],
	['Active_Ack', { text: '激活已应答', color: 'var(--el-color-primary)' }],
	['Cleared_UnAck', { text: '清除未应答', color: 'var(--el-color-warning)' }],
	['Cleared_Act', { text: '清除已应答', color: 'var(--el-color-success)' }],
]);

export const originatorTypeTAG = new Map([
	['Unknow', { text: '未知', color: 'var(--el-color-info)' }],
	['Device', { text: '设备', color: 'var(--el-color-primary)' }],
	['Gateway', { text: '网关', color: 'var(--el-color-warning)' }],
	['Asset', { text: '资产', color: 'var(--el-color-success)' }],
]);
