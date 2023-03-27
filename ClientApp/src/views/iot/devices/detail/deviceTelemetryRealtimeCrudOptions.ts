import { deviceApi } from '/@/api/devices';
import { dateUtil, formatToDateTime } from '/@/utils/dateUtil';
// eslint-disable-next-line no-unused-vars
export const createDeviceTelemetryRealtimeCrudOptions = function ({ expose }, deviceId, state) {
	const deviceId_param = deviceId;
	let records: any[] = [];
	const FsButton = {
		link: true,
	};
	const formatColumnDataTime = (row, column, cellValue, index) => {
		return formatToDateTime(row.value);
	};
	const pageRequest = async (query) => {
		const res = await deviceApi().getDeviceLatestTelemetry(deviceId_param);
		state.telemetryKeys = res.data.filter((x) => typeof x.value === 'number').map((c) => c.keyName); // DeviceDetailTelemetry 组件状态， 要传到遥测历史组件
		records = res.data;
		return {
			total:records.length,
			pageSize:records.length,
			currentPage:0,
			records,
		};
	};
	return {
		deviceId,
		crudOptions: {
			actionbar: {
				buttons: {
					add: {
						show: false,
					},
				},
			},
			request: {
				pageRequest,
			},
			table: {
				border: false,
			},
			form: {
				labelWidth: '130px', 
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
				show: false,
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
					remove: { show: false }, //删除按钮
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
				// dataType: {
				// 	title: '数据类型',
				// 	type: 'dict-select',
				// 	dict: dict({
				// 		data: [
				// 			{ value: 'Boolean', label: 'Boolean' },
				// 			{ value: 'String', label: 'String' },
				// 			{ value: 'Long', label: 'Long' },
				// 			{ value: 'Double', label: 'Double' },
				// 			{ value: 'Json', label: 'Json' },
				// 			{ value: 'XML', label: 'XML' },
				// 			{ value: 'Binary', label: 'Binary' },
				// 			{ value: 'DateTime', label: 'DateTime' },
				// 		],
				// 	}),
				// },
				dateTime: {
					title: '时间',
					type: 'text',
					column: {
						formatter: formatColumnDataTime,
					},
					addForm: {
						show: false,
					},
					editForm: {
						show: false,
					},
				},
				value: {
					title: '值',
					type: 'text',
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
