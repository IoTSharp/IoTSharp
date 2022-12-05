// 定义接口来定义对象的类型
export interface TableDataRow {
	accessFailedCount?: number;
	email?: string;
	id?: string;
	lockoutEnabled?: boolean;
	lockoutEnd?: string;
	phoneNumber?: string;
	roles?: string[];
	userName?: string;
}

export interface TableDataState {
	tableData: {
		rows: Array<TableDataRow>;
		total: number;
		loading: boolean;
		param: {
			pageNum: number;
			pageSize: number;
		};
	};
}
