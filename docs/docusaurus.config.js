// @ts-check

const { themes } = require('prism-react-renderer');

const lightTheme = themes.github;
const darkTheme = themes.dracula;

const siteUrl = process.env.DOCS_SITE_URL || 'https://iotsharp.net';
const baseUrl = process.env.DOCS_BASE_URL || '/';

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'IoTSharp',
  tagline: '开源物联网平台，覆盖安装、接入、规则链、资产与运维发布。',
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
    navbar: {
      title: 'IoTSharp',
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
        { to: '/blog', label: '博客', position: 'left' },
        {
          href: 'http://iotsharp.online',
          label: '在线体验',
          position: 'right',
        },
        {
          href: 'https://space.bilibili.com/496905613',
          label: '视频教程',
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
          title: '文档',
          items: [
            { label: '快速开始', to: '/docs/getting-started/installation-options' },
            { label: '用户手册', to: '/docs/user-guide/dashboard' },
            { label: '运维发布', to: '/docs/operations/release-distribution-plan' },
          ],
        },
        {
          title: '社区',
          items: [
            { label: 'GitHub', href: 'https://github.com/IoTSharp/IoTSharp' },
            { label: 'Gitee', href: 'https://gitee.com/IoTSharp/IoTSharp' },
            { label: 'QQ群', href: 'https://jq.qq.com/?_wv=1027&k=u1ZzTmVd' },
          ],
        },
        {
          title: '联系',
          items: [
            { label: '企业微信', href: 'https://work.weixin.qq.com/ca/cawcde2aa597e1ddf7' },
            { label: '官网', href: 'https://iotsharp.net' },
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} IoTSharp.`,
    },
    prism: {
      theme: lightTheme,
      darkTheme,
      additionalLanguages: ['bash', 'diff', 'json', 'powershell'],
    },
  },
};

module.exports = config;
