import React from 'react';
import clsx from 'clsx';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import HomepageFeatures from '../components/HomepageFeatures';
import styles from './index.module.css';

const signalCards = [
  { label: '设备接入', value: 'HTTP / MQTT / CoAP' },
  { label: '核心能力', value: 'Telemetry / Rule Chain / Alarm' },
  { label: '交付方式', value: 'Docker / Service / Installer' },
];

const panelBadges = ['HTTP', 'MQTT', 'CoAP', 'Multi-Tenant', 'Asset'];

const panelHighlights = [
  {
    value: '7+',
    label: '数据库模板',
    detail: 'Sqlite、PostgreSql、MySql、SQLServer、Oracle、ClickHouse、Cassandra',
  },
  {
    value: '3',
    label: '主要交付路径',
    detail: 'Docker、Windows Service、Linux Service',
  },
  {
    value: '.NET 10',
    label: '平台主应用',
    detail: 'ASP.NET Core 主站 + Vue 3 控制台 + 文档与安装配套',
  },
];

const panelMatrix = [
  { title: '平台能力', items: ['设备接入', '遥测与属性', '规则链与告警'] },
  { title: '交付配套', items: ['Installer', 'Docker Desktop Extension', 'OpenClaw Runbook'] },
];

function SignalCard({ label, value }) {
  return (
    <div className={styles.signalCard}>
      <span>{label}</span>
      <strong>{value}</strong>
    </div>
  );
}

function HeroPanel() {
  return (
    <div className={styles.heroPanel}>
      <div className={styles.heroPanelGlow} />
      <div className={styles.heroPanelInner}>
        <div className={styles.radarStack}>
          <div className={styles.radarCore} />
          <div className={styles.radarRing} />
          <div className={styles.radarRingWide} />
        </div>
        <div className={styles.previewFrame}>
          <div className={styles.previewToolbar}>
            <span />
            <span />
            <span />
          </div>
          <div className={styles.previewCanvas}>
            <div className={styles.previewHeadline}>IoTSharp Docs Control Deck</div>
            <div className={styles.previewLead}>
              IoTSharp 是一个面向工业与企业场景的开源 IoT 平台，聚焦设备接入、遥测处理、规则链编排、可视化管理、多租户运营与产品化交付。
            </div>
            <div className={styles.previewBadgeRow}>
              {panelBadges.map((badge) => (
                <span key={badge} className={styles.previewBadge}>
                  {badge}
                </span>
              ))}
            </div>
            <div className={styles.previewBars}>
              {panelHighlights.map((item) => (
                <article key={item.label} className={styles.previewStatCard}>
                  <strong>{item.value}</strong>
                  <span>{item.label}</span>
                  <p>{item.detail}</p>
                </article>
              ))}
            </div>
            <div className={styles.previewGrid}>
              {panelMatrix.map((group) => (
                <section key={group.title} className={styles.previewInfoCard}>
                  <h3>{group.title}</h3>
                  <ul>
                    {group.items.map((item) => (
                      <li key={item}>{item}</li>
                    ))}
                  </ul>
                </section>
              ))}
            </div>
            <div className={styles.previewFooter}>
              <span>开源工业 IoT 平台</span>
              <span>设备、规则链、资产与交付一体化</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

function HomepageHeader() {
  const { siteConfig } = useDocusaurusContext();

  return (
    <header className={clsx('hero', styles.heroBanner)}>
      <div className="container">
        <div className={styles.heroLayout}>
          <div className={styles.heroCopy}>
            <span className={styles.kicker}>Open Source Industrial IoT Platform</span>
            <h1 className={styles.heroTitle}>{siteConfig.title}</h1>
            <p className={styles.heroSubtitle}>{siteConfig.tagline}</p>
            <p className={styles.heroDescription}>
              文档首页直接围绕 IoTSharp 本身展开，而不是泛化展示模板。这里重点覆盖设备与网关接入、
              遥测与告警、规则链处理、多租户管理，以及 Docker、服务化与 Installer 等真实交付路径。
            </p>
            <div className={styles.actions}>
              <Link className={clsx('button button--lg', styles.primaryButton)} to="/docs/intro">
                进入文档中心
              </Link>
              <Link
                className={clsx('button button--lg button--outline', styles.secondaryButton)}
                to="/docs/getting-started/installation-options"
              >
                查看安装路径
              </Link>
            </div>
            <div className={styles.signalGrid}>
              {signalCards.map((card) => (
                <SignalCard key={card.label} {...card} />
              ))}
            </div>
          </div>
          <HeroPanel />
        </div>
      </div>
    </header>
  );
}

export default function Home() {
  const { siteConfig } = useDocusaurusContext();

  return (
    <Layout
      title={`${siteConfig.title} 文档中心`}
      description="IoTSharp 文档中心，覆盖安装、部署、平台使用、集成开发、AI 协作与发布运维。"
    >
      <HomepageHeader />
      <main className={styles.mainContent}>
        <HomepageFeatures />
      </main>
    </Layout>
  );
}
