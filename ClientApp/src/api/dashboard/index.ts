import request from '/@/utils/request';



export function getKanban () {
    return request({
        url: '/api/home/kanban',
        method: 'get',
    })
}
export function getHealthChecks() {
    return request({
        url: '/api/healthChecks',
        method: 'get',
    })
}

export function getMessageInfo() {
    return request({
        url: '/api/Metrics/EventBus',
        method: 'get',
    })
}
