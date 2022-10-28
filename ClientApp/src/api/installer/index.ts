import request from '/@/utils/request';
export function getSysInfo(){
    return request({
        url: '/api/Installer/Instance',
        method: 'get'
    });
}
export function initSysAdmin(data:any){
    return request({
        url: '/api/Installer/Install',
        method: 'post',
        data
    });
}
