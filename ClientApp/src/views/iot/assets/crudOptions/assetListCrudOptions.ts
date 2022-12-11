import { assetApi } from '/@/api/asset';
import _ from 'lodash-es';
import { TableDataRow } from '../model/assetList';
import { ElMessage } from 'element-plus';
import { dict } from '@fast-crud/fast-crud';
export const createAssetListCrudOptions = function ({ expose }, assetDetailRef) {
	let records: any[] = [];
	const FsButton = {
		link: true,
	};
	const customSwitchComponent = {
		activeColor: 'var(--el-color-primary)',
		inactiveColor: 'var(el-switch-of-color)',
	};
	const pageRequest = async (query) => {
		let {
			form: { name },
			page: { currentPage: offset, pageSize: limit },
		} = query;
		offset = offset === 1 ? 0 : offset - 1;
		const res = await assetApi().assetList({ name, limit, offset });
		return {
			records: res.data.rows,
			currentPage: 1,
			pageSize: 20,
			total: res.data.total,
		};
	};
	const editRequest = async ({ form, row }) => {
		form.id = row.id;
		try {
			await assetApi().putAsset(form);
			return form;
		} catch (e) {
			ElMessage.error(e.response.msg);
		}
	};
	const delRequest = async ({ row }) => {
		try {
			await assetApi().deleteAsset(row.id);
			_.remove(records, (item: TableDataRow) => {
				return item.id === row.id;
			});
		} catch (e) {
			ElMessage.error(e.response.msg);
		}
	};

	const addRequest = async ({ form }) => {
		try {
			await assetApi().postAsset({
				...form,
			});
			records.push(form);
			return form;
		} catch (e) {
			ElMessage.error(e.response.msg);
		}
	};
	return {
		crudOptions: {
			request: {
				pageRequest,
				addRequest,
				delRequest,
				editRequest,
			},
			table: {
				border: false,
			},
			form: {
				labelWidth: '80px',
			},
			search: {
				show: true,
			},
			rowHandle: {
				width: 200,
				buttons: {
					view: {
						icon: 'View',
						...FsButton,
						show: false,
					},
					edit: {
						icon: 'EditPen',
						...FsButton,
						order: 2,
					},
					remove: {
						icon: 'Delete',
						...FsButton,
						order: 3,
					},
				},
			},
			columns: {
				name: {
					title: '资产名称',
					type: 'button',
					search: { show: true },
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					column: {
						component: {
							...FsButton,
							type: 'primary',
							on: {
								onClick({ row }) {
									assetDetailRef.value.openDialog(row);
								},
							},
						},
					},
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				assetType: {
					title: '类型',
					type: 'dict-select',
					column: { width: 180 },
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					dict: dict({
						data: [
							{ value: 'Gateway', label: '网关' },
							{ value: 'Device', label: '设备', color: 'warning' },
						],
					}),
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				description: {
					title: '描述',
					column: { width: 150 },
					type: 'textarea',
					form: {
						col: {
							span: 24,
							style: { gridColumn: 'span 2' }, // grid 模式控制跨列
						},
					},
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
			},
		},
	};
};
