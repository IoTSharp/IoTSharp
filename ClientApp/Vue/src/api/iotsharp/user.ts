import { BasicFetchResult, BasicPageParams } from '../model/baseModel';
import { BasicColumn } from '/@/components/Table/src/types/table';
import { defHttp } from '/@/utils/http/axios';

enum Api {
  List = '/Tenants',
  Save = '',
}
export const TenantListApi = (params: BasicPageParams) => {
  return defHttp.get<BasicFetchResult<TenantListItem>>({
    url: Api.List,
    params,
    headers: {
      ignoreCancelToken: true,
    },
  });
};

export interface TenantListItem {
  id: string;
  beginTime: string;
  endTime: string;
  phone: string;
  eMail: string;
  name: string;
  no: number;
  status: number;
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
      title: '姓名',
      dataIndex: 'name',
      width: 150,
    },
    {
      title: '地址',
      dataIndex: 'phone',
    },
    {
      title: '编号',
      dataIndex: 'eMail',
      width: 150,
      sorter: true,
      defaultHidden: true,
    },
    {
      title: '开始时间',
      width: 120,
      sorter: true,
      dataIndex: 'street',
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
