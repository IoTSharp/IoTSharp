import { accountApi } from '/@/api/user';
import _ from 'lodash-es';
import { TableDataRow } from '../model/userListModel';
import { ElMessage } from 'element-plus';
import { compute } from '@fast-crud/fast-crud';
export const createUserListCrudOptions = function ({ expose }, customerId) {
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
			form: { userName: name },
			page: { currentPage: offset, pageSize: limit },
		} = query;
		offset = offset === 1 ? 0 : offset - 1;
		const res = await accountApi().accountList({ name, limit, offset, customerId });
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
			await accountApi().putAccount(form);
			return form;
		} catch (e) {
			ElMessage.error(e.response.msg);
		}
	};
	const delRequest = async ({ row }) => {
		try {
			await accountApi().deleteAccount(row.id);
			_.remove(records, (item: TableDataRow) => {
				return item.id === row.id;
			});
		} catch (e) {
			ElMessage.error(e.response.msg);
		}
	};

	return {
		crudOptions: {
			request: {
				pageRequest,
				delRequest,
				editRequest,
			},
			table: {
				border: false,
			},
			actionbar: {
				show: false,
			},
			form: {
				labelWidth: '80px', //
			},
			search: {
				show: true,
			},
			rowHandle: {
				width: 250,
				buttons: {
					view: {
						icon: 'View',
						...FsButton,
						show: false,
					},
					edit: {
						icon: 'EditPen',
						...FsButton,
						order: 1,
					},
					remove: {
						icon: 'Delete',
						...FsButton,
						order: 2,
					}, //删除按钮
				},
			},
			columns: {
				id: {
					title: 'Id',
					type: 'text',
					column: { width: 300 },
					editForm: {
						show: false,
					},
				},
				userName: {
					title: '名称',
					type: 'text',
					column: { width: 200 },
					search: { show: true }, //显示查询
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				email: {
					title: '邮件',
					column: { width: 200 },
					type: 'text',
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				phoneNumber: {
					title: '电话',
					column: { width: 180 },
					type: 'text',
					editForm: {
						show: true,
						component: customSwitchComponent,
					},
				},
				accessFailedCount: {
					title: '登录失败次数',
					column: { width: 150 },
					type: 'text',
					editForm: {
						show: false,
					},
				},
				lockoutEnabled: {
					title: '锁定',
					column: {
						width: 180,
						component: {
							name: 'fs-dict-switch',
							show: true,
							onChange: compute((context) => {
								return async () => {
									const { id: Id, lockoutEnabled } = context.row;
									await accountApi().updateAccountStatus({
										Id,
										opt: lockoutEnabled ? 'Lock' : 'Unlock',
									});
								};
							}),
						},
					},
					editForm: {
						show: false,
					},
				},
				lockoutEnd: {
					title: '锁定截止时间',
					column: { width: 150 },
					type: 'text',
					editForm: {
						show: false,
					},
				},
			},
		},
	};
};
