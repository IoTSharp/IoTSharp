import request from '@/utils/request'

export function getDevices(id) {
  return request({
    url: '/Devices/Customers/' + id,
    method: 'get'
  })
}

export function postDevice(data) {
  return request({
    url: '/Devices',
    method: 'post',
    data
  })
}

export function fetchArticle(id) {
  return request({
    url: '/article/detail',
    method: 'get',
    params: { id }
  })
}

export function fetchPv(pv) {
  return request({
    url: '/article/pv',
    method: 'get',
    params: { pv }
  })
}


export function updateArticle(data) {
  return request({
    url: '/article/update',
    method: 'post',
    data
  })
}
