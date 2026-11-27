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

type AlarmBadgeTone = 'warning' | 'info' | 'danger' | 'success';
type AlarmBadge = { text: string; tone?: AlarmBadgeTone };

export const serverityBadge = new Map<string, AlarmBadge>([
	['Indeterminate', { text: '不确定' }],
	['Warning', { text: '警告', tone: 'warning' }],
	['Minor', { text: '次要', tone: 'info' }],
	['Major', { text: '重要', tone: 'warning' }],
	['Critical', { text: '错误', tone: 'danger' }],
]);

export const alarmStatusTAG = new Map<string, AlarmBadge>([
	['Active_UnAck', { text: '激活未应答', tone: 'warning' }],
	['Active_Ack', { text: '激活已应答', tone: 'info' }],
	['Cleared_UnAck', { text: '清除未应答', tone: 'warning' }],
	['Cleared_Act', { text: '清除已应答', tone: 'success' }],
]);

export const originatorTypeTAG = new Map(originatorTypeOptions.slice(1).map(({ key, label }) => [key, label]));
originatorTypeTAG.set('Unknow', '未知');
