
import { appmessage, IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';
interface QueryParam extends IListQueryParam {
    offset: number;
    limit: number;

}
export function getProduceList(query: QueryParam) {
    return request.get('api/produces/list?offset=' + query.offset + '&limit=' + query.limit + '&sorter=')
}



export function getProduce(id: string) {
    return request.get('api/produces/get?id=' + id)
}

export function updateProduce(product: any) {
    return request.put('api/produces/update', product)
}
export function saveProduce(product: any) {
    return request.post('api/produces/save', product)
}

export function editProduceData(product: any) {
    return request.post('api/produces/editProduceData', product)
}
export function getProduceData(id: string) {
    return request.get('api/produces/GetProduceData?produceId=+' + id)
}
export function editProduceDictionary(data: any) {
    return request.post(`api/produces/editProduceDictionary`, data)
}
export function getProduceDictionary(id: string) {
    return request.get(`api/produces/getProduceDictionary?produceId=` + id)
}