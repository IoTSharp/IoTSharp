import React from 'react';
import Link from '@docusaurus/Link';
import styles from './HomepageFeatures.module.css';

const featureGroups = [
  {
    eyebrow: '部署与交付',
    title: '从体验安装到企业发布，文档路径更清晰',
    description:
      '围绕 Installer、Docker、Windows 服务、Linux 服务、Docker Desktop Extension 与发布分发计划重组文档，让首次上手、正式部署和后续升级都能按一条路径走通。',
    links: [
      { label: '安装方式', to: '/docs/getting-started/installation-options' },
      { label: 'Installer 初始化', to: '/docs/getting-started/installer' },
      { label: 'Docker Desktop Extension', to: '/docs/deployment/docker-desktop-extension' },
    ],
  },
  {
    eyebrow: '平台能力',
    title: '把设备、规则链、告警与资产串成完整业务闭环',
    description:
      '文档菜单直接对应产品主导航，设备接入、规则链、场景联动、告警处理、产品模型和资产管理都能在同一套信息结构里找到。',
    links: [
      { label: '设备与网关', to: '/docs/user-guide/devices-and-gateways' },
      { label: '规则链与场景', to: '/docs/user-guide/rules-and-scenarios' },
      { label: '告警中心', to: '/docs/user-guide/alarms' },
    ],
  },
  {
    eyebrow: 'AI 协作',
    title: '让 AI 助手更容易理解 IoTSharp 的安装与配置',
    description:
      '补充面向 OpenClow 等助手的可执行运行手册、推荐提示词和数据库切换规则，帮助用户把 AI 直接接入到 SQLite 体验安装和后续配置流程中。',
    links: [
      { label: 'OpenClow 运行手册', to: '/docs/operations/openclow-sqlite-runbook' },
      { label: '应用配置', to: '/docs/configuration/appsettings' },
      { label: '发布分发计划', to: '/docs/operations/release-distribution-plan' },
    ],
  },
];

const quickCards = [
  {
    badge: '01',
    title: '首次部署',
    text: '针对新环境准备，优先覆盖最省事的安装路径、Installer 初始化和首次登录。',
    to: '/docs/getting-started/installation-options',
  },
  {
    badge: '02',
    title: '生产运行',
    text: '聚焦 Docker、Windows 服务、Linux 服务和配置模板，方便环境落地与排障。',
    to: '/docs/deployment/docker',
  },
  {
    badge: '03',
    title: '平台使用',
    text: '从仪表盘到设备、规则链、告警和资产，完整映射产品菜单。',
    to: '/docs/user-guide/dashboard',
  },
  {
    badge: '04',
    title: '集成开发',
    text: '协议接入与 Web API 文档集中收口，便于边开发边对照查阅。',
    to: '/docs/integrations/web-api',
  },
];

function FeatureSection({ eyebrow, title, description, links }) {
  return (
    <article className={styles.featurePanel}>
      <span className={styles.eyebrow}>{eyebrow}</span>
      <h2>{title}</h2>
      <p>{description}</p>
      <div className={styles.featureLinks}>
        {links.map((link) => (
          <Link key={link.to} className={styles.featureLink} to={link.to}>
            {link.label}
          </Link>
        ))}
      </div>
    </article>
  );
}

function QuickCard({ badge, title, text, to }) {
  return (
    <Link className={styles.quickCard} to={to}>
      <span className={styles.quickBadge}>{badge}</span>
      <h3>{title}</h3>
      <p>{text}</p>
    </Link>
  );
}

export default function HomepageFeatures() {
  return (
    <section className={styles.wrapper}>
      <div className={styles.gridBackdrop} />
      <div className={styles.sectionHeader}>
        <span className={styles.sectionKicker}>Documentation Matrix</span>
        <h2>科技感外观之下，仍然保持工程上的可维护与可扩展</h2>
        <p>
          这套模版不是一次性视觉皮肤，而是围绕 IoTSharp 的产品导航、部署流程和 AI
          协作文档能力重新整理的文档站框架。
        </p>
      </div>

      <div className={styles.featureGrid}>
        {featureGroups.map((feature) => (
          <FeatureSection key={feature.title} {...feature} />
        ))}
      </div>

      <div className={styles.quickSection}>
        <div className={styles.quickHeader}>
          <span className={styles.sectionKicker}>Quick Routes</span>
          <h2>把用户最常走的四条路径直接铺到首页</h2>
        </div>
        <div className={styles.quickGrid}>
          {quickCards.map((card) => (
            <QuickCard key={card.title} {...card} />
          ))}
        </div>
      </div>
    </section>
  );
}
