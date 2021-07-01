import { BasicFetchResult, BasicPageParams } from '../model/baseModel';
import { BasicColumn } from '/@/components/Table/src/types/table';
import { defHttp } from '/@/utils/http/axios';

enum Api {
  List = '/api/Devices',
  Save = '',
}
export const TenantListApi = (params: BasicPageParams) => {
  return defHttp.get<BasicFetchResult<DeviceItem>>({
    url: Api.List,
    params,
    headers: {
      ignoreCancelToken: true,
    },
  });
};

export interface DeviceItem {
  id: string;
  name: string;
  deviceType: string;
  online: string;
  lastActive: string;
  timeout: string;
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
