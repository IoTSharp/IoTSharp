import deviceIcon from '~icons/carbon/iot-platform';
import telemetryIcon from '~icons/carbon/ibm-cloud-pak-data';
import eventIcon from '~icons/carbon/ibm-cloud-event-streams';
import warningIcon from '~icons/ic/round-warning';
import userIcon from '~icons/ic/baseline-supervisor-account';
import productIcon from '~icons/ic/outline-category';
import ruleIcon from '~icons/carbon/flow-modeler';

export type HomeCardMetricKey =
	| 'deviceCount'
	| 'onlineDeviceCount'
	| 'attributesDataCount'
	| 'eventCount'
	| 'alarmsCount'
	| 'userCount'
	| 'produceCount'
	| 'rulesCount';

export interface HomeCardItemConfig {
	key: HomeCardMetricKey;
	label: string;
	description: string;
	icon: any;
	accentColor: string;
	iconBackgroundColor: string;
}

export const homeCardItemsConfig: HomeCardItemConfig[] = [
	{
		key: 'deviceCount',
		label: '设备总量',
		description: '当前纳管终端规模',
		icon: deviceIcon,
		accentColor: '#2563eb',
		iconBackgroundColor: 'linear-gradient(135deg, #2563eb 0%, #0ea5e9 100%)',
	},
	{
		key: 'onlineDeviceCount',
		label: '在线设备',
		description: '当前连接状态稳定设备',
		icon: deviceIcon,
		accentColor: '#059669',
		iconBackgroundColor: 'linear-gradient(135deg, #059669 0%, #10b981 100%)',
	},
	{
		key: 'attributesDataCount',
		label: '属性数据',
		description: '近 24 小时采集数据量',
		icon: telemetryIcon,
		accentColor: '#0284c7',
		iconBackgroundColor: 'linear-gradient(135deg, #0284c7 0%, #38bdf8 100%)',
	},
	{
		key: 'eventCount',
		label: '平台事件',
		description: '近 24 小时业务事件量',
		icon: eventIcon,
		accentColor: '#7c3aed',
		iconBackgroundColor: 'linear-gradient(135deg, #7c3aed 0%, #a855f7 100%)',
	},
	{
		key: 'alarmsCount',
		label: '告警设备',
		description: '需要优先关注的设备',
		icon: warningIcon,
		accentColor: '#ea580c',
		iconBackgroundColor: 'linear-gradient(135deg, #ea580c 0%, #f59e0b 100%)',
	},
	{
		key: 'userCount',
		label: '系统用户',
		description: '当前协作管理用户数',
		icon: userIcon,
		accentColor: '#0f766e',
		iconBackgroundColor: 'linear-gradient(135deg, #0f766e 0%, #14b8a6 100%)',
	},
	{
		key: 'produceCount',
		label: '产品模型',
		description: '平台产品与模板规模',
		icon: productIcon,
		accentColor: '#4f46e5',
		iconBackgroundColor: 'linear-gradient(135deg, #4f46e5 0%, #818cf8 100%)',
	},
	{
		key: 'rulesCount',
		label: '自动化规则',
		description: '已发布规则链与动作数',
		icon: ruleIcon,
		accentColor: '#65a30d',
		iconBackgroundColor: 'linear-gradient(135deg, #65a30d 0%, #84cc16 100%)',
	},
];
