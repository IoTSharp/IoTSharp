import { defHttp } from '/@/utils/http/axios';
import { LoginParams, LoginResultModel, GetUserInfoModel } from './model/userModel';

import { ErrorMessageMode } from '/#/axios';

enum Api {
  Login = '/Account/Login',
  Logout = '/logout',
  GetUserInfo = '/Account/MyInfo',
  GetPermCode = '/Menu/getPermCode',
  CheckInstall = '/Installer/Instance',
  Register = '/Installer/Install',
}

/**
 * @description: user login api
 */
export function loginApi(params: LoginParams, mode: ErrorMessageMode = 'modal') {
  return defHttp.post<LoginResultModel>(
    {
      url: Api.Login,
      params,
    },
    {
      errorMessageMode: mode,
    }
  );
}

/**
 * @description: getUserInfo
 */

export function doLogout() {
  return defHttp.get({ url: Api.Logout });
}
export function getUserInfo() {
  return defHttp.get<GetUserInfoModel>({ url: Api.GetUserInfo });
}

export function getPermCode() {
  return defHttp.get<string[]>({ url: Api.GetPermCode });
}

export function CheckInstall() {
  return defHttp.get<any>({ url: Api.CheckInstall });
}
export function Register(params: any) {
  return defHttp.post<any>({ url: Api.Register, params: params });
}

export interface loginuserinfo {
  avatar: string;
  code: string;
  email: string;
  introduction: string;

  name: string;
  roles: string[];
  tenant: tenant;
}

export interface tenant {
  avatar: string;
  city: string;
  country: string;
  eMail: string;
  id: string;
  name: string;
  phone: string;
  province: string;
  street: string;
  zipCode: string;
}
