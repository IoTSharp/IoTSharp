import { assetApi } from '/@/api/asset';
import { formatToDateTime } from '/@/utils/dateUtil';
// eslint-disable-next-line no-unused-vars
export const createDeviceTelemetryRealtimeCrudOptions = function ({ expose }, assetId, state) {
	let records: any[] = [];
	const FsButton = {
		link: true,
	};
	const formatColumnDataTime = (row, column, cellValue) => {
		return formatToDateTime(cellValue);
	};
	const pageRequest = async () => {
		const res = await assetApi().relations({assetId});
		records = res.data.rows;
		return {
			records,
		};
	};
	return {
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
			},
		},
	};
};
