
import { appmessage, IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';
interface QueryParam extends IListQueryParam {
    offset: number;
    limit: number;

    name:string;

}
export function getProduceList(query: QueryParam) {
    // ?offset=0&limit=20&pi=0&ps=20&name=&sorter=&status=null
    return request.get('api/produces/list?pi=0&ps=20&status=null&offset=' + query.offset + '&limit=' + query.limit + '&sorter=&name='+query.name)
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