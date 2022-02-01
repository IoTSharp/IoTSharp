// @ts-check
// Note: type annotations allow type checking and IDEs autocompletion

const lightCodeTheme = require('prism-react-renderer/themes/github');
const darkCodeTheme = require('prism-react-renderer/themes/dracula');

/** @type {import('@docusaurus/types').Config} */
const config = {
  title: 'IoTSharp 在线文档',
  tagline: 'IoTSharp is an open-source IoT platform for data collection, processing, visualization, and device management.',
  url: 'https://docs.iotsharp.io',
  baseUrl: '/',
  onBrokenLinks: 'throw',
  onBrokenMarkdownLinks: 'warn',
  favicon: 'img/favicon.ico',
  organizationName: 'IoTSharp', // Usually your GitHub org/user name.
  projectName: 'docs', // Usually your repo name.

  presets: [
    [
      'classic',
      /** @type {import('@docusaurus/preset-classic').Options} */
      ({
        docs: {
          sidebarPath: require.resolve('./sidebars.js'),
          // Please change this to your repo.
          editUrl: 'https://github.com/IoTSharp/docs/',
        },
        blog: {
          showReadingTime: true,
          // Please change this to your repo.
          editUrl:
            'https://github.com/IoTSharp/docs/blob/main/',
        },
        theme: {
          customCss: require.resolve('./src/css/custom.css'),
        },
      }),
    ],
  ],

  themeConfig:
    /** @type {import('@docusaurus/preset-classic').ThemeConfig} */
    ({
      navbar: {
        title: 'IoTSharp 在线文档',
        logo: {
          alt: 'IoT在线文档',
          src: 'img/iotsharp.png',
        },
        items: [
          {
            type: 'doc',
            docId: 'intro',
            position: 'left',
            label: '参考手册',
          },
          {to: '/blog', label: '博客', position: 'left'},
          {
            href: 'https://github.com/IoTSharp/IoTSharp',
            label: 'GitHub',
            position: 'right',
          },
          {
            href: 'https://gitee.com/dotnetchina/IoTSharp',
            label: 'Gitee',
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
              {
                label: '简介',
                to: '/docs/intro',
              },
            ],
          },
          {
            title: '社区',
            items: [
              {
                label: 'QQ群',
                href: 'https://jq.qq.com/?_wv=1027&k=u1ZzTmVd',
              },
              {
                label: 'Discord',
                href: 'https://discord.gg/My6PaTmUvu',
              },
              {
                label: '微博',
                href: 'https://weibo.com/iotsharp',
              },
            ],
          },
          {
            title: '其他',
            items: [
              {
                label: 'GitHub',
                href: 'https://github.com/IoTSharp/IoTSharp',
              },
              {
                label: 'Gitee',
                href: 'https://gitee.com/IoTSharp/IoTSharp',
              },
              {
                label: ' 冀ICP备18039206号-2',
                href: 'https://beian.miit.gov.cn/',
              },
            ],
          },
        ],
        copyright: `Copyright © ${new Date().getFullYear()} IoTSharp. Built with Docusaurus.`,
      },
      prism: {
        theme: lightCodeTheme,
        darkTheme: darkCodeTheme,
      },
    }),
};

module.exports = config;
