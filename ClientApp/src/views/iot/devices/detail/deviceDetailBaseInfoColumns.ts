import { dict } from '@fast-crud/fast-crud';

export const deviceDetailBaseInfoColumns = {
	name: {
		title: '设备名称',
		type: 'button',
	},
	deviceType: {
		title: '设备类型',
		type: 'dict-select',
		dict: dict({
			data: [
				{ value: 'Gateway', label: '网关' },
				{ value: 'Device', label: '设备', color: 'warning' },
			],
		}),
	},

	active: {
		title: '活动状态',
		type: 'dict-switch',
		dict: dict({
			data: [
				{ value: true, label: '活动' },
				{ value: false, label: '静默', color: 'danger' },
			],
		}),
	},
	lastActivityDateTime: {
		title: '最后活动时间',
		type: 'text',
	},
	identityType: {
		title: '认证方式',
		type: 'dict-select',
		dict: dict({
			data: [
				{ value: 'AccessToken', label: 'AccessToken' },
				{ value: 'X509Certificate', label: 'X509Certificate' },
			],
		}),
	},
	identityId: {
		title: 'Token',
		type: 'text',
	},
	timeout: {
		title: '超时',
		type: 'text',
	},
};
