// @ts-check

const { themes } = require('prism-react-renderer');

const lightTheme = themes.github;
const darkTheme = themes.vsDark;

const siteUrl = process.env.DOCS_SITE_URL || 'https://iotsharp.net';
const baseUrl = process.env.DOCS_BASE_URL || '/';

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'IoTSharp',
  tagline: '面向工业与企业场景的现代化 IoT 平台文档中心，覆盖安装、接入、规则链、资产与发布运维。',
  url: siteUrl,
  baseUrl,
  onBrokenLinks: 'throw',
  favicon: 'img/favicon.ico',
  trailingSlash: false,
  organizationName: 'IoTSharp',
  projectName: 'IoTSharp',
  deploymentBranch: 'gh-pages',
  i18n: {
    defaultLocale: 'zh-Hans',
    locales: ['zh-Hans'],
  },
  markdown: {
    hooks: {
      onBrokenMarkdownLinks: 'warn',
    },
  },
  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          editUrl: 'https://github.com/IoTSharp/IoTSharp/tree/master/docs',
          exclude: ['**/tutorial-basics/**', '**/tutorial-extras/**'],
        },
        blog: {
          showReadingTime: true,
          editUrl: 'https://github.com/IoTSharp/IoTSharp/tree/master/docs/blog',
          onInlineAuthors: 'ignore',
          onUntruncatedBlogPosts: 'ignore',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      },
    ],
  ],
  themeConfig: {
    image: 'img/iotsharp.png',
    colorMode: {
      defaultMode: 'dark',
      disableSwitch: false,
      respectPrefersColorScheme: false,
    },
    docs: {
      sidebar: {
        hideable: true,
        autoCollapseCategories: false,
      },
    },
    announcementBar: {
      id: 'iotsharp_docs_refresh',
      content:
        'IoTSharp 文档站已升级到全新 Docusaurus 框架与科技感界面，欢迎从 Installer、部署与 AI 协作文档开始浏览。',
      isCloseable: true,
      backgroundColor: '#081527',
      textColor: '#bff7ff',
    },
    navbar: {
      title: 'IoTSharp',
      hideOnScroll: true,
      logo: {
        alt: 'IoTSharp',
        src: 'img/iotsharp.png',
      },
      items: [
        {
          type: 'doc',
          docId: 'intro',
          position: 'left',
          label: '使用手册',
        },
        {
          type: 'doc',
          docId: 'getting-started/installation-options',
          position: 'left',
          label: '快速开始',
        },
        { to: '/blog', label: '博客', position: 'left' },
        {
          href: 'https://iotsharp.net',
          label: '官网',
          position: 'right',
        },
        {
          href: 'http://iotsharp.online',
          label: '在线体验',
          position: 'right',
        },
        {
          href: 'https://github.com/IoTSharp/IoTSharp',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: '文档路径',
          items: [
            { label: '安装方式', to: '/docs/getting-started/installation-options' },
            { label: 'Installer 初始化', to: '/docs/getting-started/installer' },
            { label: 'Docker Desktop Extension', to: '/docs/deployment/docker-desktop-extension' },
          ],
        },
        {
          title: '平台能力',
          items: [
            { label: '设备与网关', to: '/docs/user-guide/devices-and-gateways' },
            { label: '规则链与场景', to: '/docs/user-guide/rules-and-scenarios' },
            { label: 'OpenClow 运行手册', to: '/docs/operations/openclow-sqlite-runbook' },
          ],
        },
        {
          title: '社区与资源',
          items: [
            { label: 'GitHub', href: 'https://github.com/IoTSharp/IoTSharp' },
            { label: 'Gitee', href: 'https://gitee.com/IoTSharp/IoTSharp' },
            { label: '官网', href: 'https://iotsharp.net' },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} IoTSharp.`,
    },
    prism: {
      theme: lightTheme,
      darkTheme,
      additionalLanguages: ['bash', 'diff', 'json', 'powershell', 'csharp', 'docker'],
    },
  },
};

module.exports = config;
