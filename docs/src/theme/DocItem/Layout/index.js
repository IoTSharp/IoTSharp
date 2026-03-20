import React from 'react';
import clsx from 'clsx';
import {useWindowSize} from '@docusaurus/theme-common';
import {useDoc} from '@docusaurus/plugin-content-docs/client';
import DocItemPaginator from '@theme/DocItem/Paginator';
import DocVersionBanner from '@theme/DocVersionBanner';
import DocVersionBadge from '@theme/DocVersionBadge';
import DocItemFooter from '@theme/DocItem/Footer';
import DocItemTOCMobile from '@theme/DocItem/TOC/Mobile';
import DocItemTOCDesktop from '@theme/DocItem/TOC/Desktop';
import DocItemContent from '@theme/DocItem/Content';
import DocBreadcrumbs from '@theme/DocBreadcrumbs';
import ContentVisibility from '@theme/ContentVisibility';
import styles from './styles.module.css';
import heroStyles from '../../ExperienceHero.module.css';

function useDocTOC() {
  const {frontMatter, toc} = useDoc();
  const windowSize = useWindowSize();

  const hidden = frontMatter.hide_table_of_contents;
  const canRender = !hidden && toc.length > 0;

  const mobile = canRender ? <DocItemTOCMobile /> : undefined;
  const desktop =
    canRender && (windowSize === 'desktop' || windowSize === 'ssr') ? (
      <DocItemTOCDesktop />
    ) : undefined;

  return {hidden, mobile, desktop};
}

function DocHero() {
  const {metadata, frontMatter} = useDoc();
  const sectionBadges = metadata.id
    .split('/')
    .filter(Boolean)
    .slice(0, 3)
    .map((item) => item.replace(/-/g, ' '));

  return (
    <section className={clsx(heroStyles.heroShell, heroStyles.docHeader)}>
      <div className={heroStyles.heroGrid}>
        <div>
          <span className={heroStyles.eyebrow}>IoTSharp Documentation</span>
          <h1 className={heroStyles.title}>{metadata.title}</h1>
          <p className={heroStyles.lead}>
            {frontMatter.description ||
              metadata.description ||
              '围绕 IoTSharp 的安装、部署、平台使用与发布运维提供可执行说明。'}
          </p>
          <div className={heroStyles.badgeRow}>
            {sectionBadges.map((badge) => (
              <span key={badge} className={heroStyles.badge}>
                {badge}
              </span>
            ))}
          </div>
          <div className={heroStyles.metaLine}>
            <span>{metadata.permalink}</span>
            {frontMatter.sidebar_position ? <span>Step {frontMatter.sidebar_position}</span> : null}
            <span>Readable for operators and AI assistants</span>
          </div>
        </div>
        <div className={heroStyles.sideStack}>
          <aside className={heroStyles.surfaceCard}>
            <h3>你会在这里看到</h3>
            <ul>
              <li>明确的部署或使用路径</li>
              <li>配置项与环境变量约束</li>
              <li>适合发布与交付的操作建议</li>
            </ul>
          </aside>
          <aside className={heroStyles.surfaceCard}>
            <h3>阅读建议</h3>
            <ul>
              <li>先看顶部摘要，再按目录进入细节</li>
              <li>配置示例优先使用覆盖文件而不是直接改模板</li>
              <li>需要上下文时返回侧边栏查看相邻章节</li>
            </ul>
          </aside>
        </div>
      </div>
    </section>
  );
}

export default function DocItemLayout({children}) {
  const docTOC = useDocTOC();
  const {metadata} = useDoc();

  return (
    <div className="row">
      <div className={clsx('col', !docTOC.hidden && styles.docItemCol)}>
        <ContentVisibility metadata={metadata} />
        <DocVersionBanner />
        <div className={styles.docItemContainer}>
          <article>
            <DocBreadcrumbs />
            <DocVersionBadge />
            <DocHero />
            {docTOC.mobile}
            <DocItemContent>{children}</DocItemContent>
            <DocItemFooter />
          </article>
          <DocItemPaginator />
        </div>
      </div>
      {docTOC.desktop && (
        <div className="col col--3">
          <div className={heroStyles.tocCard}>{docTOC.desktop}</div>
        </div>
      )}
    </div>
  );
}
