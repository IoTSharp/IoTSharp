import { appmessage, IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';
interface QueryParam extends IListQueryParam {
    offset: number;
    limit: number;
   
}
export function getProduceList(query: QueryParam) {
    return request.get('api/produces/list?offset='+query.offset+'&limit='+query.limit+'&sorter=')
}



export function editProduceDictionary(data: any) {
    return request.post(`api/produces/editProduceDictionary`, data)
}
export function getProduce(id: string) {
    return request.get('api/produces/get?id='+id)
}



export function acquire(id: string) {
    return request.post(`api/alarm/ackAlarm`, {
        id: id
    })
}
