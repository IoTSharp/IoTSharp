import { deviceApi } from '/@/api/devices';
import _ from 'lodash-es';
import { dict } from '@fast-crud/fast-crud';
import { h, type Ref } from 'vue';
import type { TableDataRow } from '/@/views/iot/devices/model';
import dayjs from 'dayjs';

export interface DeviceListFilters {
	name: string;
	connected: boolean;
}

export interface DeviceListOverview {
	total: number;
	pageCount: number;
	onlineCount: number;
	loaded: boolean;
	appliedFilters: DeviceListFilters;
}

export function createDeviceCrudOptions(
	customerId: Readonly<Ref<string | undefined>>,
	openDeviceDetails: (device: TableDataRow) => void,
	selectedItems: Ref<TableDataRow[]>,
	overviewState: DeviceListOverview
) {
	let records: TableDataRow[] = [];
	const FsButton = {
		link: true,
	};
	const customSwitchComponent = {
		activeColor: 'var(--el-color-primary)',
	};

	const onSelectionChange = (selection: TableDataRow[]) => {
		selectedItems.value = selection;
	};

	/** 按服务端支持的名称、精确 ID 和仅在线条件请求设备分页。 */
	const pageRequest = async (query) => {
		const filters: DeviceListFilters = {
			name: query.form.name?.trim() || '',
			connected: query.form.connected === true,
		};
		const params = {
			offset: query.page.currentPage - 1,
			limit: query.page.pageSize,
			onlyConnected: filters.connected,
			customerId: customerId.value,
			name: filters.name,
		};

		const res = await deviceApi().devcieList(params);
		records = res.data.rows ?? [];
		overviewState.total = res.data.total ?? 0;
		overviewState.pageCount = records.length;
		overviewState.onlineCount = records.filter((item) => item.connected === true).length;
		overviewState.loaded = true;
		overviewState.appliedFilters = filters;
		return {
			records,
			currentPage: params.offset + 1,
			pageSize: params.limit,
			total: overviewState.total,
		};
	};
	const editRequest = async ({ form, row }) => {
		const newItem = _.clone(form);
		newItem.id = row.id;
		const target = _.find(records, (item: TableDataRow) => {
			return row.id === item.id;
		});
		try {
			await deviceApi().putdevcie(newItem);
			_.merge(target, form);
			return target;
		} catch (e) {
			ElMessage.error(e.response.msg);
		}
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
		await deviceApi().postdevcie(form);
		records.push(form);
		return form;
	};
	return {
		crudOptions: {
			request: {
				pageRequest,
				addRequest,
				editRequest,
				delRequest,
			},

			search: { show: false },
			toolbar: {
				buttons: {
					search: { show: false },
					refresh: { show: false },
				},
			},

			pagination: {
				'page-sizes': [10, 20, 30, 40, 100, 250],
				layout: 'total, sizes, prev, pager, next',
				pagerCount: 5,
				small: true,
			},

			table: {
				border: false,
				size: 'small',
				onSelectionChange,
			},
			form: {
				labelWidth: '130px', //
			},
			rowHandle: {
				width: 150,
				dropdown: {
					more: {
						//更多按钮配置
						text: '属性',
						...FsButton,
						icon: 'operation',
					},
				},

				buttons: {
					view: {
						show: false,
					},
					edit: {
						icon: 'editPen',
						...FsButton,
						order: 1,
					}, //编辑按钮
					remove: {
						icon: 'Delete',
						...FsButton,
						order: 5,
					}, //删除按钮
				},
			},
			columns: {
				$checked: {
					title: '选择',
					form: { show: false },
					column: {
						type: 'selection',
						align: 'center',
						width: 42,
						columnSetDisabled: true,
					},
				},
				name: {
					title: '设备名称',
					type: 'text',
					search: { show: false },
					column: {
						minWidth: 190,
						cellRender: ({ row }: { row: TableDataRow }) => {
							const name = row.name || row.id || '设备详情';
							return h('button', {
								type: 'button',
								class: 'device-table__name',
								title: name,
								onClick: () => openDeviceDetails(row),
							}, name);
						},
					},
				},
				deviceType: {
					title: '设备类型',
					type: 'dict-select',
					search: { show: false },
					dict: dict({
						data: [
							{ value: 'Gateway', label: '网关' },
							{ value: 'Device', label: '设备' },
						],
					}),

					column: { width: 84 },
				},

				productName: {
					title: '产品',
					type: 'text',
					form: { show: false },
					search: { show: false },
					column: {
						minWidth: 124,
						showOverflowTooltip: true,
						formatter: ({ value }) => value || '—',
					},
				},
				connected: {
					title: '连接状态',
					type: 'dict-switch',
					search: { show: false },
					dict: dict({
						data: [
							{ value: true, label: '在线' },
							{ value: false, label: '离线', color: 'danger' },
						],
					}),
					column: {
						width: 96,
						cellRender: ({ value }) => {
							const status = value === true ? 'online' : value === false ? 'offline' : 'unknown';
							const label = { online: '在线', offline: '离线', unknown: '未知' };
							return h('span', { class: ['console-status', 'console-status--' + status] }, label[status]);
						},
					},
					viewForm: {
						show: true,
						component: customSwitchComponent,
					},
					addForm: {
						show: false,
						component: customSwitchComponent,
					},
					editForm: {
						show: false,
						component: customSwitchComponent,
					},
				},
				active: {
					title: '在线状态',
					type: 'dict-switch',
					search: { show: false },
					dict: dict({
						data: [
							{ value: true, label: '在线' },
							{ value: false, label: '离线', color: 'danger' },
						],
					}),
					column: { width: '80px', show: false },
					viewForm: {
						show: false,
						component: customSwitchComponent,
					},
					addForm: {
						show: false,
						component: customSwitchComponent,
					},
					editForm: {
						show: false,
						component: customSwitchComponent,
					},
				},
				lastConnectDateTime: {
					title: '最后上线时间',
					type: 'datetime',
					column: {
						formatter: (context) => {
							if(context.value){
								return	dayjs.tz(context.value, "Asia/Shanghai").add(8, 'hour').format('YYYY-MM-DD HH:mm:ss');
							}else{
								return '';
							}
						},
						show: false,
					},
					search: { show: false },
					addForm: {
						show: false,
					},
					editForm: {
						show: false,
					},
				},
				identityType: {
					title: '认证方式',
					type: 'dict-select',
					search: { show: false },
					column: { minWidth: 130, showOverflowTooltip: true },
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
					search: { show: false },
					column: {
						show: false,
					},
					viewForm: {
						show: false,
					},
					addForm: {
						show: false,
					},
					editForm: {
						show: false,
					},
				},
				timeout: {
					column: {
						show: false,
					},
					title: '超时',
					form: {
						value: 300,
					},
					type: 'text',
					search: { show: false },
				},
			},
		},
	};
}
