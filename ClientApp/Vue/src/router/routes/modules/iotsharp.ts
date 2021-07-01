import type { AppRouteModule } from '/@/router/types';

import { LAYOUT } from '/@/router/constant';
import { t } from '/@/hooks/web/useI18n';

const iotsharp: AppRouteModule = {
  path: '/iotsharp',
  name: 'Iotsharp',
  component: LAYOUT,
  redirect: '/iotsharp/tenant',
  meta: {
    orderNo: 1111,
    icon: 'simple-icons:about-dot-me',
    // title: t('routes.dashboard.about'),
    title: 'IotSharp',
  },
  children: [
    {
      path: 'tenant',
      name: 'Tenant',
      component: () => import('../../../views/iotsharp/tenant/tenantlist.vue'),
      meta: {
        //  title: t('routes.dashboard.about'),
        icon: 'simple-icons:about-dot-me',
        title: '租户管理',
        // title: t('routes.dashboard.about'), translate someting
      },
    },
    {
      path: 'customer',
      name: 'Customer',
      component: () => import('../../../views/iotsharp/customer/customerlist.vue'),
      meta: {
        //  title: t('routes.dashboard.about'),
        icon: 'simple-icons:about-dot-me',
        title: '客户管理',
      },
    },
    {
      path: 'device',
      name: 'Device',
      component: () => import('../../../views/iotsharp/device/devicelist.vue'),
      meta: {
        //  title: t('routes.dashboard.about'),
        icon: 'simple-icons:about-dot-me',
        title: '设备管理',
      },
    },
    {
      path: 'user',
      name: 'User',
      component: () => import('../../../views/iotsharp/user/userlist.vue'),
      meta: {
        //  title: t('routes.dashboard.about'),
        icon: 'simple-icons:about-dot-me',
        title: '人员管理',
      },
    },
  ],
};

export default iotsharp;
