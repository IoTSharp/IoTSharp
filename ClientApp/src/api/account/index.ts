import request from '/@/utils/request';
interface signUpForm {
    email: string;
    customerName: string;
    password: string;
    tenantEMail: string;
    tenantName: string;
    phoneNumber?: any;
    customerEMail: string;
}
export function ModifyMyInfo(params:any){
    return request({
        url: '/api/account/ModifyMyInfo',
        method: 'put',
        data: params
    });
}
// 租户注册
export function signup(params:signUpForm) {
    return request.post(`/api/account/create`, params)
}
export function ModifyMyPassword(params:any){
    return request({
        url: '/api/account/ModifyMyPassword',
        method: 'put',
        data: params
    });
}
