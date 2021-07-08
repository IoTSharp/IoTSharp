import { BasicFetchResult, BasicPageParams } from '../model/baseModel';
import { BasicColumn } from '/@/components/Table/src/types/table';
import { defHttp } from '/@/utils/http/axios';

enum Api {
  List = '/Devices/Customers',
  Save = '/Devices',
  Update = '/Devices',
  Delete = '/Devices',
  Get = '/Devices',
  AttributeLatestList = '/api​/Devices​/{deviceId}​/AttributeLatest',
  TelemetryLatestList = '/api/Devices/{deviceId}/TelemetryLatest',
  GetIdentity = '/Devices/{deviceId}/Identity',
  SetAttribute = '/api/Devices/{access_token}/Attributes',
}
export const DeviceListApi = (params: BasicPageParams) => {
  return defHttp.get<BasicFetchResult<DeviceItem>>({
    url: Api.List + '/' + params.customerId,
    params,
    headers: {
      ignoreCancelToken: true,
    },
  });
};

export const SetAttribute = (params: BasicPageParams, accesstoken: string) => {
  return defHttp.post({
    url: 'Devices/' + accesstoken + '/Attributes',
    params,
    headers: {
      ignoreCancelToken: true,
    },
  });
};

export const GetIdentity = (id: any) => {
  return defHttp.get({
    url: '/Devices/' + id + '/Identity',
    headers: {
      ignoreCancelToken: true,
    },
  });
};
export const Get = (id: any) => {
  return defHttp.get({
    url: Api.Get + '/' + id,
    headers: {
      ignoreCancelToken: true,
    },
  });
};

export const Update = (params: any) => {
  return defHttp.put({
    url: Api.Update + '/' + params.id,
    params,
    headers: {
      ignoreCancelToken: true,
    },
  });
};

export const Save = (params: any) => {
  return defHttp.post({
    url: Api.Save,
    params,
    headers: {
      ignoreCancelToken: true,
    },
  });
};
export const Delete = (params: any) => {
  return defHttp.delete({
    url: Api.Delete + '/' + params.id,
    headers: {
      ignoreCancelToken: true,
    },
  });
};

export const GetAttributeLatest = (params: any) => {
  return defHttp.get<BasicFetchResult<AttributeItem>>({
    url: '/Devices/' + params.id + '/AttributeLatest',
    // params,
    headers: {
      ignoreCancelToken: true,
    },
  });
};
export const SaveAttribute = (params: any) => {
  return defHttp.get<BasicFetchResult<AttributeItem>>({
    url: '/Devices/' + params.id + '/AttributeLatest',
    // params,
    headers: {
      ignoreCancelToken: true,
    },
  });
};
export const GetTelemetryLatest = (params: any) => {
  return defHttp.get<BasicFetchResult<TelemetryItem>>({
    url: '/Devices/' + params.id + '/TelemetryLatest',
    // params,
    headers: {
      ignoreCancelToken: true,
    },
  });
};
export interface AttributeItem {
  keyName: string;
  dataSide: string;
  dateTime: string;
  value: string;
}

export interface TelemetryItem {
  keyName: string;
  dateTime: string;
  value: string;
}
export interface DeviceItem {
  id: string;
  name: string;
  deviceType: string;
  online: string;
  lastActive: string;
  timeout: string;
  Telemetrys: TelemetryItem[];
  Attributes: AttributeItem[];
}

export function getBasicColumns(): BasicColumn[] {
  return [
    {
      title: 'ID',
      dataIndex: 'id',
      fixed: 'left',
      width: 200,
    },
    {
      title: '名称',
      dataIndex: 'name',
      width: 150,
    },
    {
      title: '类型',
      dataIndex: 'deviceType',
    },
    {
      title: '是否在线',
      dataIndex: 'online',
      width: 150,
      sorter: true,
      defaultHidden: true,
    },
    {
      title: '最后活动时间',
      width: 120,
      sorter: true,
      dataIndex: 'lastActive',
    },
  ];
}

export function getBasicShortColumns(): BasicColumn[] {
  return [
    {
      title: 'ID',
      width: 150,
      dataIndex: 'id',
      sorter: true,
      sortOrder: 'ascend',
    },
    {
      title: '姓名',
      dataIndex: 'name',
      width: 120,
    },
    {
      title: '地址',
      dataIndex: 'phone',
    },
    {
      title: '编号',
      dataIndex: 'eMail',
      width: 80,
    },
  ];
}
