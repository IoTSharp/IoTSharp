import { appmessage, IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';
interface QueryParam extends IListQueryParam {
    offset: number;
    limit: number;
    Name?: string;
    sorter?: string;
    status?: any;
    AckDateTime?: any;
    ClearDateTime?: any;
    StartDateTime?: any;
    EndDateTime?: any;
    AlarmType?: string;
    OriginatorName?: string;
    alarmStatus?: string;
    OriginatorId?: string;
    serverity?: string;
    originatorType?: string;
}
export function getAlarmList(query: QueryParam) {
    return request.post(`api/alarm/list`, query)
}



export function clear(id: string) {
    return request.post(`api/alarm/clearAlarm`, {
        id: id
    })
}



export function acquire(id: string) {
    return request.post(`api/alarm/ackAlarm`, {
        id: id
    })
}
