import { appmessage, IListQueryParam } from '../iapiresult';
import request from '/@/utils/request';
interface QueryParam extends IListQueryParam {
    offset: number;
    limit: number;
    name?: string;
}
export function getProductList(query: QueryParam) {
    return request({
        url: '/api/Products/list',
        method: 'get',
        params: {
            pi: 0,
            ps: 20,
            status: null,
            offset: query.offset,
            limit: query.limit,
            sorter: '',
            name: query.name ?? '',
        },
    })
}
export function getProduct(id: string) {
    return request.get('/api/Products/get?id=' + id)
}
export function updateProduct(product: any) {
    return request.put('/api/Products/update', product)
}
export function saveProduct(product: any) {
    return request.post('/api/Products/save', product)
}
export function editProductData(product: any) {
    return request.post('/api/Products/editProductData', product)
}
export function getProductData(id: string) {
    return request.get('/api/Products/GetProductData?productId=' + id)
}
export function editProductDictionary(data: any) {
    return request.post(`/api/Products/editProductDictionary`, data)
}


export function getProductDictionary(id: string) {
    return request.get(`/api/Products/getProductDictionary?productId=` + id)
}
export function createDevice(id: string, data: any) {
    return request.post<appmessage<any>>(`/api/Devices/product/` + id, data)
}

export function deleteProduct(id: string) {
    return request.get(`/api/Products/delete?productId=` + id)
}

/**
 * 获取产品的数据映射关系
 */
export function getProductDataMappings(productId: string) {
    return request.get(`/api/Products/GetDataMappings?productId=` + productId)
}

/**
 * 保存产品的数据映射关系（全量替换）
 */
export function saveProductDataMappings(data: { productId: string; mappings: ProductDataMappingDto[] }) {
    return request.post(`/api/Products/SaveDataMappings`, data)
}

export interface ProductDataMappingDto {
    id: string;
    productKeyName: string;
    dataCatalog: string; // 'TelemetryData' | 'AttributeData'
    deviceId: string;
    deviceKeyName: string;
    description?: string;
}
