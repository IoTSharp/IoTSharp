import { deviceApi } from '/@/api/devices';
import _ from 'lodash-es';
import { compute, dict } from '@fast-crud/fast-crud';
import { TableDataRow } from '/@/views/iot/devices/model';
// eslint-disable-next-line no-unused-vars
export const createDevicePropsCrudOptions = function ({ expose }, deviceId) {
	const deviceId_param = deviceId;
	let records: any[] = [];
	const FsButton = {
		link: true,
	};
	const customSwitchComponent = {
		activeColor: 'var(--el-color-primary)',
		inactiveColor: 'var(el-switch-of-color)',
	};
	const pageRequest = async (query) => {
		console.log(`%c-createDevicePropsCrudOptions@devicePropsCrudOptions:7`, 'color:white;font-size:16px;background:blue;font-weight: bold;', deviceId)
		const res = await deviceApi().getDeviceAttributes(deviceId_param);
		records = res.data;
		return {
			records,
			currentPage: 1,
			pageSize: 20,
			total: res.data.total,
		};
	};
	const delRequest = async ({ row }) => {
		try {
			await deviceApi().deletedevcie(row.id);
			_.remove(records, (item: TableDataRow) => {
				return item.id === row.id;
			});
		} catch (e) {
			ElMessage.error(e.response.msg);
		}
	};

	const addRequest = async ({ form }) => {
		try {
			form.deviceId = deviceId;
			form.value = null;
			await deviceApi().addDeviceAttributes(deviceId, form);
			records.push(form);
			return form;
		} catch (e) {
			ElMessage.error(e.response.msg);
		}
	};
	return {
		deviceId,
		crudOptions: {
			request: {
				pageRequest,
				addRequest,
				delRequest,
			},
			table: {
				border: false,
			},
			form: {
				labelWidth: '130px', //
			},
			search: {
				show: false,
			},
			toolbar: {
				buttons: {
					search: {
						show: false,
					},
				},
			},
			pagination: {
				show: false,
			},
			rowHandle: {
				width: 100,
				dropdown: {
					more: {
						//更多按钮配置
						text: '属性',
						...FsButton,
						icon: 'operation',
					},
				},
				buttons: {
					view: { show: false },
					edit: { show: false },
					remove: {
						icon: 'Delete',
						...FsButton,
						order: 5,
					}, //删除按钮
				},
			},
			columns: {
				keyName: {
					title: '属性名称',
					type: 'text',
					column: {
						width: 260,
					},
				},
				dataType: {
					title: '数据类型',
					type: 'dict-select',
					dict: dict({
						data: [
							{ value: 'Boolean', label: 'Boolean' },
							{ value: 'String', label: 'String' },
							{ value: 'Long', label: 'Long' },
							{ value: 'Double', label: 'Double' },
							{ value: 'Json', label: 'Json' },
							{ value: 'XML', label: 'XML' },
							{ value: 'Binary', label: 'Binary' },
							{ value: 'DateTime', label: 'DateTime' },
						],
					}),
				},
				dataSide: {
					title: '数据侧',
					type: 'dict-select',
					dict: dict({
						data: [
							{ value: 'AnySide', label: 'AnySide', color: 'warning' },
							{ value: 'ClientSide', label: 'ClientSide' },
							{ value: 'ServerSide', label: 'ServerSide', color: 'info' },
						],
					}),
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					editForm: {
						show: false,
						component: customSwitchComponent,
					},
				},
				lastActivityDateTime: {
					title: '时间',
					type: 'text',
					search: { show: false },
					addForm: {
						show: false,
					},
					editForm: {
						show: false,
					},
				},
			},
		},
	};
};
