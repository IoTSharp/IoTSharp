// 定义接口来定义对象的类型
export interface TableDataRow {
	assetType?: string;
	description?: string;
	id?: string;
	name?: string;
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
