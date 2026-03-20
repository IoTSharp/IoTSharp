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
  { label: 'AI Friendly', value: 'Installer + OpenClow Runbook' },
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
            <div className={styles.previewBars}>
              <i />
              <i />
              <i />
            </div>
            <div className={styles.previewGrid}>
              <div />
              <div />
              <div />
              <div />
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
