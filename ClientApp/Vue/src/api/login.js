import request from '@/utils/request'

const userApi = {
  Login: '/Account/Login',
  Logout: '/auth/logout',
  ForgePassword: '/auth/forge-password',
  Register: '/Installer/Install',
  twoStepCode: '/auth/2step-code',
  SendSms: '/account/sms',
  SendSmsErr: '/account/sms_err',
  // get my info
  UserInfo: '/user/info',
  UserMenu: '/user/nav',
  CheckInstall: '/installer/instance'
}

/**
 * login func
 * parameter: {
 *     username: '',
 *     password: '',
 *     remember_me: true,
 *     captcha: '12345'
 * }
 * @param parameter
 * @returns {*}
 */
export function login (parameter) {
  return request({
    url: userApi.Login,
    method: 'post',
    data: parameter
  })
}

export function getSmsCaptcha (parameter) {
  return request({
    url: userApi.SendSms,
    method: 'post',
    data: parameter
  })
}

export function getInfo () {
  return request({
    url: userApi.UserInfo,
    method: 'get',
    headers: {
      'Content-Type': 'application/json;charset=UTF-8'
    }
  })
}

export function getCurrentUserNav () {
  return request({
    url: userApi.UserMenu,
    method: 'get'
  })
}

export function logout () {
  return request({
    url: userApi.Logout,
    method: 'post',
    headers: {
      'Content-Type': 'application/json;charset=UTF-8'
    }
  })
}

/**
 * get user 2step code open?
 * @param parameter {*}
 */
export function get2step (parameter) {
  return request({
    url: userApi.twoStepCode,
    method: 'post',
    data: parameter
  })
}

export function checkInstall () {
  return request({
    url: userApi.CheckInstall,
    method: 'get',
    headers: {
      'Content-Type': 'application/json;charset=UTF-8'
    }
  })
}

export function Install (params) {
  console.log(params)
  return request({
    url: userApi.Register,
    method: 'post',
    data: params,
    headers: {
      'Content-Type': 'application/json;charset=UTF-8'
    }
  })
}
