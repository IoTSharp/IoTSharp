import React from 'react';
import clsx from 'clsx';
import Layout from '@theme/Layout';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import styles from './index.module.css';
import HomepageFeatures from '../components/HomepageFeatures';

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <h1 className="hero__title">{siteConfig.title}</h1>
        <p className="hero__subtitle">{siteConfig.tagline}</p>
        <div className={styles.buttons}>
          <Link
            className="button button--secondary button--lg"
            to="/docs/intro">
            IoTSharp 参考手册 - 5min ⏱️
          </Link>
        </div>
      </div>
    </header>
  );
}

export default function Home() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title={` ${siteConfig.title}`}
      description="IoTSharp 是一个基于.Net 6.0 使用C#开发的数据收集、处理、可视化与设备管理的开源物联网(IoT)平台基础平台,支持 HTTP、MQTT 、CoAP等协议实现设备的数字孪生,且属性数据和遥测数据协议简单类型丰富,简易设置即可将数据存储在PostgreSql、MySql、Oracle、SQLServer、Sqlite 或者 InfluxDB 2.0;TDengine;TimescaleDB等时序数据库中。">
      <HomepageHeader /> 
      <main>
        <HomepageFeatures />
      </main>
    </Layout>
  );
}
