import { IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';
interface QueryParam extends IListQueryParam{
    pi: number;
    ps: number;
    Name: string;
    sorter: string;
    status?: any;
    AckDateTime?: any;
    ClearDateTime?: any;
    StartDateTime?: any;
    EndDateTime?: any;
    AlarmType: string;
    OriginatorName: string;
    alarmStatus: string;
    OriginatorId: string;
    serverity: string;
    originatorType: string;
}
export function getAlarmList(query: QueryParam) {
    return request.post(`api/alarm/list`, query )
}
