import React from 'react';
import clsx from 'clsx';
import styles from './HomepageFeatures.module.css';

const FeatureList = [
  {
    title: '按产品导航组织',
    Svg: require('../../static/img/undraw_docusaurus_mountain.svg').default,
    description: (
      <>
        文档目录直接对应安装、仪表盘、设备、规则链、告警、资产与系统管理，便于新用户按页面上手。
      </>
    ),
  },
  {
    title: '适合 GitHub Pages 发布',
    Svg: require('../../static/img/undraw_docusaurus_tree.svg').default,
    description: (
      <>
        站点配置支持通过环境变量切换 `url` 与 `baseUrl`，既能部署到自定义域名，也能直接发布到 GitHub Pages。
      </>
    ),
  },
  {
    title: '覆盖运维与集成',
    Svg: require('../../static/img/undraw_docusaurus_react.svg').default,
    description: (
      <>
        除了页面功能说明，还补充了 Docker、Windows 服务、Linux 部署、协议接入、Web API 与发布分发说明。
      </>
    ),
  },
];

function Feature({ Svg, title, description }) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <Svg className={styles.featureSvg} alt={title} />
      </div>
      <div className="text--center padding-horiz--md">
        <h3>{title}</h3>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures() {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
