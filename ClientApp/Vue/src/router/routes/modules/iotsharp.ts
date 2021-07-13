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
        icon: 'simple-icons:about-dot-me',
        title: t('routes.iotsharp.tenant'),
      },
    },
    {
      path: 'customer',
      name: 'Customer',
      component: () => import('../../../views/iotsharp/customer/customerlist.vue'),
      meta: {
        title: t('routes.iotsharp.customer'),
        icon: 'simple-icons:about-dot-me',
      },
    },
    {
      path: 'device',
      name: 'Device',
      component: () => import('../../../views/iotsharp/device/devicelist.vue'),
      meta: {
        title: t('routes.iotsharp.device'),
        icon: 'simple-icons:about-dot-me',
      },
    },
    {
      path: 'devicegraph',
      name: 'devicegraph',
      component: () => import('../../../views/iotsharp/device/devicegraph.vue'),
      meta: {
        //  title: t('routes.iotsharp.device'),
        title: t('设计'),
        icon: 'simple-icons:about-dot-me',
      },
    },
    {
      path: 'user',
      name: 'User',
      component: () => import('../../../views/iotsharp/user/userlist.vue'),
      meta: {
        title: t('routes.iotsharp.user'),
        icon: 'simple-icons:about-dot-me',
      },
    },
  ],
};

export default iotsharp;
