// 定义接口来定义对象的类型
export interface TableDataRow {
    active?: boolean;
    customerId?: string;
    deviceType?: string;
    id?: string;
    identityId?: string;
    identityType?: string;
    identityValue?: string;
    lastActivityDateTime?: string;
    name?: string;
    owner?: string;
    tenantId?: string;
    tenantName?: string;
    timeout?: number;
    children?: TableDataRow[];
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
