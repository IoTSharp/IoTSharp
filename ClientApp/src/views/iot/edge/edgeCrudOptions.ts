import { edgeApi } from '/@/api/edge';
import dayjs from 'dayjs';
import { dict } from '@fast-crud/fast-crud';

export const createEdgeCrudOptions = function ({ expose, openOnboarding }, edgeDetailRef, overviewState?) {
	let records: any[] = [];
	const FsButton = {
		link: true,
	};

	const pageRequest = async (query) => {
		const params = {
			offset: query.page.currentPage - 1,
			limit: query.page.pageSize,
			name: query.form.name ?? undefined,
			runtimeType: query.form.runtimeType ?? undefined,
			status: query.form.status ?? undefined,
			healthy: query.form.healthy,
			active: query.form.active,
			version: query.form.version ?? undefined,
			platform: query.form.platform ?? undefined,
			sorter: query.sort?.prop ?? 'LastHeartbeatDateTime',
			sort: query.sort?.order === 'desc' ? 'desc' : query.sort?.order === 'asc' ? 'asc' : 'desc',
		};

		const res = await edgeApi().getEdgeList(params);
		records = res.data.rows ?? [];

		if (overviewState) {
			overviewState.total = res.data.total ?? 0;
			overviewState.pageCount = records.length;
			overviewState.healthyCount = records.filter((item) => item.healthy === true).length;
			overviewState.activeCount = records.filter((item) => item.active === true).length;
			overviewState.lastRefresh = dayjs().format('HH:mm:ss');
		}

		return {
			records,
			currentPage: query.page.currentPage,
			pageSize: query.page.pageSize,
			total: res.data.total,
		};
	};

	return {
		crudOptions: {
			request: {
				pageRequest,
			},
			pagination: {
				'page-sizes': [10, 20, 30, 40, 100],
			},
			search: {
				show: true,
			},
			table: {
				border: false,
			},
			rowHandle: {
				width: 150,
				buttons: {
					onboard: {
						icon: 'Connection',
						...FsButton,
						text: '接入',
						order: 0,
						onClick({ row }) {
							openOnboarding?.(row);
						},
					},
					view: {
						icon: 'View',
						...FsButton,
						text: '详情',
						order: 1,
						onClick({ row }) {
							edgeDetailRef.value.openDialog(row);
						},
					},
					edit: { show: false },
					remove: { show: false },
				},
			},
			columns: {
				name: {
					title: '名称',
					type: 'button',
					search: { show: true },
					column: {
						sortable: 'custom',
						component: {
							...FsButton,
							type: 'primary',
							on: {
								onClick({ row }) {
									edgeDetailRef.value.openDialog(row);
								},
							},
						},
					},
				},
				runtimeType: {
					title: 'RuntimeType',
					type: 'dict-select',
					search: { show: true },
					column: { sortable: 'custom', width: 120 },
					dict: dict({ data: [{ value: 'gateway', label: 'Gateway' }, { value: 'pixiu', label: 'PiXiu' }] }),
				},
				runtimeName: {
					title: 'RuntimeName',
					search: { show: false },
					column: { sortable: 'custom', width: 150 },
				},
				version: {
					title: 'Version',
					type: 'text',
					search: { show: true },
					column: { sortable: 'custom', width: 120 },
				},
				status: {
					title: 'Status',
					type: 'text',
					search: { show: true },
					column: { sortable: 'custom', width: 120 },
				},
				healthy: {
					title: 'Healthy',
					type: 'dict-select',
					search: { show: true },
					column: { sortable: 'custom', width: 100 },
					dict: dict({ data: [{ value: true, label: '健康' }, { value: false, label: '异常', color: 'danger' }] }),
				},
				active: {
					title: 'Active',
					type: 'dict-select',
					search: { show: true },
					column: { sortable: 'custom', width: 100 },
					dict: dict({ data: [{ value: true, label: '活跃' }, { value: false, label: '静默', color: 'warning' }] }),
				},
				lastTaskStatus: {
					title: '最近任务状态',
					search: { show: false },
					column: { width: 140 },
				},
				lastReceiptDateTime: {
					title: '最近回执时间',
					search: { show: false },
					column: {
						width: 180,
						formatter: (context) => (context.value ? dayjs(context.value).format('YYYY-MM-DD HH:mm:ss') : ''),
					},
				},
				lastHeartbeatDateTime: {
					title: 'LastHeartbeatDateTime',
					search: { show: false },
					column: {
						sortable: 'custom',
						width: 180,
						formatter: (context) => (context.value ? dayjs(context.value).format('YYYY-MM-DD HH:mm:ss') : ''),
					},
				},
				lastActivityDateTime: {
					title: 'LastActivityDateTime',
					search: { show: false },
					column: {
						sortable: 'custom',
						width: 180,
						formatter: (context) => (context.value ? dayjs(context.value).format('YYYY-MM-DD HH:mm:ss') : ''),
					},
				},
				hostName: {
					title: 'HostName',
					search: { show: false },
					column: { width: 150 },
				},
				ipAddress: {
					title: 'IpAddress',
					search: { show: false },
					column: { width: 140 },
				},
				platform: {
					title: 'Platform',
					type: 'text',
					search: { show: true },
					column: { width: 140 },
				},
			},
		},
	};
};
