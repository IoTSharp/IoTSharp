// @ts-check

/** @type {import('@docusaurus/plugin-content-docs').SidebarsConfig} */
const sidebars = {
  tutorialSidebar: [
    'intro',
    {
      type: 'category',
      label: '平台概览',
      items: ['overview/product-overview', 'overview/architecture'],
    },
    {
      type: 'category',
      label: '快速开始',
      items: [
        'getting-started/installation-options',
        'getting-started/installer',
        'getting-started/first-login',
      ],
    },
    {
      type: 'category',
      label: '部署与配置',
      items: [
        'deployment/docker',
        'deployment/docker-desktop-extension',
        'deployment/windows-service',
        'deployment/linux-service',
        'configuration/appsettings',
      ],
    },
    {
      type: 'category',
      label: '用户手册',
      items: [
        'user-guide/dashboard',
        'user-guide/devices-and-gateways',
        'user-guide/rules-and-scenarios',
        'user-guide/alarms',
        'user-guide/products-and-assets',
        'user-guide/system-management',
        'user-guide/profile',
      ],
    },
    {
      type: 'category',
      label: '集成接口',
      items: ['integrations/protocols', 'integrations/web-api'],
    },
    {
      type: 'category',
      label: '运维发布',
      items: ['operations/release-distribution-plan', 'operations/openclow-sqlite-runbook', 'operations/troubleshooting'],
    },
  ],
};

module.exports = sidebars;
