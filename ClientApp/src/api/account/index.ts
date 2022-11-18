import request from '/@/utils/request';

export function ModifyMyInfo(params:any){
    return request({
        url: '/api/account/ModifyMyInfo',
        method: 'put',
        data: params
    });
}
