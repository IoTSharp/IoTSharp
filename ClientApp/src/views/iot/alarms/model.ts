export interface TableDataRow {
	id?: string;
	ackDateTime?: string;
	alarmDetail?: string;
	alarmStatus?: string;
	alarmType?: string;
	clearDateTime?: string;
	endDateTime?: string;
	originator?: string;
	originatorId?: string;
	originatorType?: string;
	propagate?: string;
	serverity?: string;
}
export interface TableDataState {
	rows: Array<TableDataRow>;
	total: number;
	loading: boolean;
	param: {
		pageNum: number;
		pageSize: number;
	};
}
