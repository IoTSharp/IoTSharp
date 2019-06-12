import request from '@/utils/request'

export function login(data) {
  return request({
    url: '/Account/Login',
    method: 'post',
    data
  })
}

export function getInfo(token) {
  return request({
    url: '/Account/MyInfo',
    method: 'get',
    params: { token }
  })
}

export function register(data) {
  console.log(data)
  return request({
    url: '/Account/Register',
    method: 'post',
    data
  })
}

export function logout() {
  return request({
    url: '/Account/Logout',
    method: 'post'
  })
}
