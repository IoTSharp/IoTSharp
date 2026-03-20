import React from 'react';
import Link from '@docusaurus/Link';
import styles from './HomepageFeatures.module.css';

const featureGroups = [
  {
    eyebrow: '设备与数据',
    title: '从设备接入到遥测处理，围绕 IoTSharp 的核心链路展开',
    description:
      'IoTSharp 面向 HTTP、MQTT、CoAP 等协议接入，围绕设备、网关、遥测、属性、告警和资产建立统一的数据与管理模型，首页内容优先突出这些真实能力。',
    links: [
      { label: '设备与网关', to: '/docs/user-guide/devices-and-gateways' },
      { label: '告警中心', to: '/docs/user-guide/alarms' },
      { label: '产品与资产', to: '/docs/user-guide/products-and-assets' },
    ],
  },
  {
    eyebrow: '规则与自动化',
    title: '规则链、场景联动与业务动作是平台的重要中枢',
    description:
      'IoTSharp 不只是采集数据，还通过规则链、脚本执行和场景联动把设备数据转成通知、自动化动作和业务流程，这部分是平台价值的核心之一。',
    links: [
      { label: '规则链与场景', to: '/docs/user-guide/rules-and-scenarios' },
      { label: '协议与接入', to: '/docs/integrations/protocols' },
      { label: 'Web API', to: '/docs/integrations/web-api' },
    ],
  },
  {
    eyebrow: '部署与交付',
    title: '文档首页同时服务于首次安装、正式部署与持续运维',
    description:
      '项目当前支持 Docker、Windows Service、Linux Service、Installer、Docker Desktop Extension 等多种交付方式，同时补充了 OpenClaw 协作安装与数据库切换文档。',
    links: [
      { label: '安装方式', to: '/docs/getting-started/installation-options' },
      { label: 'Installer 初始化', to: '/docs/getting-started/installer' },
      { label: 'Docker Desktop Extension', to: '/docs/deployment/docker-desktop-extension' },
      { label: '应用配置', to: '/docs/configuration/appsettings' },
      { label: 'OpenClaw 运行手册', to: '/docs/operations/openclaw-sqlite-runbook' },
    ],
  },
];

const quickCards = [
  {
    badge: '01',
    title: '首次部署',
    text: '从安装方式、Installer 初始化到首次登录，快速拉起一个可用的 IoTSharp 实例。',
    to: '/docs/getting-started/installation-options',
  },
  {
    badge: '02',
    title: '生产运行',
    text: '覆盖 Docker、Windows Service、Linux Service 与配置模板，贴近真实部署场景。',
    to: '/docs/deployment/docker',
  },
  {
    badge: '03',
    title: '平台使用',
    text: '从仪表盘到设备、规则链、告警、产品与资产，完整对应平台主菜单。',
    to: '/docs/user-guide/dashboard',
  },
  {
    badge: '04',
    title: '配置与协作',
    text: '聚焦应用配置、数据库模板、OpenClaw 协作和发布运维，不写无关宣传词。',
    to: '/docs/configuration/appsettings',
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
        <span className={styles.sectionKicker}>IoTSharp Overview</span>
        <h2>首页先讲 IoTSharp 是什么，再讲怎样部署、怎样使用</h2>
        <p>
          这里不再放和项目无关的泛化描述，而是直接承接 README 里的项目介绍，
          围绕设备接入、遥测处理、规则链、多租户和交付方式组织内容。
        </p>
      </div>

      <div className={styles.featureGrid}>
        {featureGroups.map((feature) => (
          <FeatureSection key={feature.title} {...feature} />
        ))}
      </div>

      <div className={styles.quickSection}>
        <div className={styles.quickHeader}>
          <span className={styles.sectionKicker}>Common Paths</span>
          <h2>把用户最常见的四条实际路径直接放到首页</h2>
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
