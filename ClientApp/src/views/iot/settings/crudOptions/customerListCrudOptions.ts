import { customerApi } from '/@/api/customer';
import _ from 'lodash-es';
import { TableDataRow } from '../model/tenantListModel';
import { ElMessage } from 'element-plus';
import { useRouter } from 'vue-router';
export const createCustomerListCrudOptions = function ({ expose }, tenantId) {
	const router = useRouter();
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
		const res = await customerApi().customerList({ name, limit, offset, tenantId });
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
			await customerApi().putCustomer(form);
			return form;
		} catch (e) {
			ElMessage.error(e.response.msg);
		}
	};
	const delRequest = async ({ row }) => {
		try {
			await customerApi().deleteCustomer(row.id);
			_.remove(records, (item: TableDataRow) => {
				return item.id === row.id;
			});
		} catch (e) {
			ElMessage.error(e.response.msg);
		}
	};

	const addRequest = async ({ form }) => {
		try {
			await customerApi().postCustomer({
				...form,
				tenantId,
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
				width: 320,
				buttons: {
					view: {
						icon: 'View',
						...FsButton,
						show: false,
					},
					device: {
						text: '设备管理',
						title: '设备管理',
						icon: 'Cpu',
						order: 1,
						type: 'default',
						...FsButton,
						click: (e) => {
							router.push({
								path: '/iot/devices/devicelist',
								query: {
									id: e.row.id,
								},
							});
						},
					},
					custom: {
						text: '用户管理',
						title: '用户管理',
						icon: 'User',
						order: 1,
						type: 'info',
						...FsButton,
						click: (e) => {
							router.push({
								path: '/iot/settings/userlist',
								query: {
									id: e.row.id,
								},
							});
						},
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
					title: '名称',
					type: 'text',
					column: { width: 200 },
					search: { show: true },
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				email: {
					title: '邮件',
					type: 'text',
					column: { width: 180 },
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				phone: {
					title: '电话',
					column: { width: 150 },
					type: 'text',
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				country: {
					title: '国家',
					column: { width: 80 },
					type: 'text',
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				province: {
					title: '省',
					type: 'text',
					column: { width: 80 },
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				city: {
					title: '市',
					type: 'text',
					column: { width: 100 },
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				street: {
					title: '街道',
					column: { width: 180 },
					type: 'text',
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				address: {
					title: '地址',
					column: { width: 180 },
					type: 'text',
					addForm: {
						show: true,
						component: customSwitchComponent,
					},
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				zipCode: {
					title: '邮编',
					column: { width: 150 },
					type: 'text',
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
