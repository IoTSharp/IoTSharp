
export class appmessage<T> {
    code?: number;
    msg?: string;
    data?: T;
}
export interface pageddata<T> {
    rows: T[];
    total: number;
}


export interface IListQueryParam {
    offset?: number
    limit?: number
    sort?: string
}
