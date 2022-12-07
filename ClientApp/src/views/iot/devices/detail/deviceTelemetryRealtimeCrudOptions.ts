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
		return formatToDateTime(cellValue);
	};
	const pageRequest = async (query) => {
		const res = await deviceApi().getDeviceLatestTelemetry(deviceId_param);
		state.telemetryKeys = res.data.filter((x) => typeof x.value === 'number').map((c) => c.keyName); // DeviceDetailTelemetry 组件状态， 要传到遥测历史组件
		let keys = state.telemetryKeys.join(','); // 在pageRequest 参数中使用的 keys
		const end = dateUtil();
		const begin = end.subtract(3, 'day'); // 获取三天前的时间点

		let average = deviceApi().getDeviceTelemetryData(deviceId, {
			keys: keys,
			begin,
			end,
			every: 3 + '.00:00:00:000',
			aggregate: 'Mean',
		});
		let max = deviceApi().getDeviceTelemetryData(deviceId, {
			keys: keys,
			begin,
			end,
			every: 3 + '.00:00:00:000',
			aggregate: 'Max',
		});
		let min = deviceApi().getDeviceTelemetryData(deviceId, {
			keys: keys,
			begin,
			end,
			every: 3 + '.00:00:00:000',
			aggregate: 'Min',
		});
		const [averageRes, maxRes, minRes] = await Promise.all([average, max, min]);
		console.log(`%cpageRequest@deviceTelemetryRealtimeCrudOptions:48`, 'color:white;font-size:16px;background:green;font-weight: bold;', [
			averageRes,
			maxRes,
			minRes,
		]);
		records = res.data;
		records.map((telemetryItem) => {
			telemetryItem.average = averageRes.data.find((x) => x.keyName === telemetryItem.keyName)?.value;
			telemetryItem.max = maxRes.data.find((x) => x.keyName === telemetryItem.keyName).value;
			telemetryItem.min = minRes.data.find((x) => x.keyName === telemetryItem.keyName).value;
			return telemetryItem;
		});
		console.log(`%c-pageRequest@deviceTelemetryRealtimeCrudOptions:24`, 'color:white;font-size:16px;background:blue;font-weight: bold;', records);
		return {
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
					custom: {
						text: '遥测历史', //fs-button组件的参数
						show: true, //是否显示此按钮
						type: 'primary',
						click() {
							state.currentPageState = 'history';
						}, //点击事件，默认打开添加对话框
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
				average: {
					title: '平均值(3天)',
					type: 'text',
				},
				min: {
					title: '最小值(3天)',
					type: 'text',
				},
				max: {
					title: '最大值(3天)',
					type: 'text',
				},
			},
		},
	};
};
