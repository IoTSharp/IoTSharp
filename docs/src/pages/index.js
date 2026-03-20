import React from 'react';
import clsx from 'clsx';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import HomepageFeatures from '../components/HomepageFeatures';
import styles from './index.module.css';

const signalCards = [
  { label: 'Documentation Surface', value: 'Docs + Blog + Pages' },
  { label: 'Deployment Paths', value: 'Docker / Service / Extension' },
  { label: 'AI Friendly', value: 'Installer + OpenClaw Runbook' },
];

const panelBadges = ['Installer', 'Docker', 'Rules Engine', 'Telemetry', 'Multi-Tenant'];

const panelHighlights = [
  {
    value: '7+',
    label: 'Database Templates',
    detail: 'Sqlite / PostgreSql / MySql / SQLServer / Oracle / ClickHouse / Cassandra',
  },
  {
    value: '3',
    label: 'Delivery Modes',
    detail: 'Docker, service install, Docker Desktop Extension',
  },
  {
    value: 'AI',
    label: 'Operator Friendly',
    detail: 'OpenClaw runbook and installer-driven onboarding',
  },
];

const panelMatrix = [
  { title: '部署路径', items: ['Docker', 'Windows Service', 'Linux Service'] },
  { title: '产品能力', items: ['Device Access', 'Rules Chain', 'Alarm Center'] },
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
              面向工业场景的 IoT 平台文档中枢，覆盖安装、部署、接入、规则链、资产管理与 AI 辅助配置。
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
              <span>Docs rebuilt for GitHub Pages</span>
              <span>Dark tech theme + higher information density</span>
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
            <span className={styles.kicker}>Industrial IoT Documentation</span>
            <h1 className={styles.heroTitle}>{siteConfig.title}</h1>
            <p className={styles.heroSubtitle}>{siteConfig.tagline}</p>
            <p className={styles.heroDescription}>
              以科技感更强的界面承载 IoTSharp 的安装、部署、平台使用、AI 协作与发布文档，
              同时保持清晰的信息结构和稳定的 GitHub Pages 发布能力。
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
