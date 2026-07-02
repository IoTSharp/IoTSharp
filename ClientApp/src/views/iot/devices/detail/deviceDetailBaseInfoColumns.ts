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
	connected: {
		title: '在线状态',
		type: 'dict-switch',
		dict: dict({
			data: [
				{ value: true, label: '在线' },
				{ value: false, label: '离线', color: 'danger' },
			],
		}),
	},
	lastConnectDateTime: {
		title: '最后上线时间',
		type: 'text',
	},
	lastDisconnectDateTime: {
		title: '最后离线时间',
		type: 'text',
	},
	identityType: {
		title: '认证方式',
		type: 'dict-select',
		dict: dict({
			data: [
				{ value: 'AccessToken', label: 'AccessToken' },
				{ value: 'X509Certificate', label: 'X509 Certificate' },
			],
		}),
	},
	identityId: {
		title: 'Token',
		type: 'text',
	},
	timeout: {
		title: '超时时间',
		type: 'text',
	},
};
