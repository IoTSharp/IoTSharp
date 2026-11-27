import request from '/@/utils/request';
interface TenantApplication {
    email: string;
    password: string;
    tenantName: string;
    phoneNumber: string;
    verificationCode: string;
}
export function ModifyMyInfo(params:any){
    return request({
        url: '/api/account/ModifyMyInfo',
        method: 'put',
        data: params
    });
}
// 租户注册
export function signup(params: TenantApplication) {
    return request.post(`/api/account/create`, params)
}
export function sendTenantVerificationCode(email: string) {
    return request.post<unknown, { data: number }>('/api/account/sendTenantVerificationCode', { email });
}
export function ModifyMyPassword(params:any){
    return request({
        url: '/api/account/ModifyMyPassword',
        method: 'put',
        data: params
    });
}
