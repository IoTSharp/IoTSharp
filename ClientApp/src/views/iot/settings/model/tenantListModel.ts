// 定义接口来定义对象的类型
export interface TableDataRow {
    address?: string;
    city?: string;
    deviceType?: string;
    country?: string;
    eMail?: string;
    id?: string;
    name?: string;
    phone?: string;
    province?: string;
    street?: string;
    zipCode?: string;
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
