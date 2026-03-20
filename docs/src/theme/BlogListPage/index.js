import React from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import {
  PageMetadata,
  HtmlClassNameProvider,
  ThemeClassNames,
} from '@docusaurus/theme-common';
import BlogLayout from '@theme/BlogLayout';
import BlogListPaginator from '@theme/BlogListPaginator';
import SearchMetadata from '@theme/SearchMetadata';
import BlogPostItems from '@theme/BlogPostItems';
import BlogListPageStructuredData from '@theme/BlogListPage/StructuredData';
import styles from '../ExperienceHero.module.css';

function BlogListPageMetadata(props) {
  const {metadata} = props;
  const {
    siteConfig: {title: siteTitle},
  } = useDocusaurusContext();
  const {blogDescription, blogTitle, permalink} = metadata;
  const isBlogOnlyMode = permalink === '/';
  const title = isBlogOnlyMode ? siteTitle : blogTitle;
  return (
    <>
      <PageMetadata title={title} description={blogDescription} />
      <SearchMetadata tag="blog_posts_list" />
    </>
  );
}

function BlogHero({metadata, items}) {
  const latestItems = items.slice(0, 3);
  const tagCount = new Set(
    items.flatMap((item) => (item.content.metadata.tags || []).map((tag) => tag.label)),
  ).size;

  return (
    <section className={styles.heroShell}>
      <div className={styles.heroGrid}>
        <div>
          <span className={styles.eyebrow}>IoTSharp Blog Signal</span>
          <h1 className={styles.title}>{metadata.blogTitle || 'IoTSharp 博客'}</h1>
          <p className={styles.lead}>
            这里集中展示 IoTSharp 的版本动态、社区活动、部署实践与产品演进记录，
            让博客首页看起来更像一块实时更新的发布情报面板，而不是普通文章列表。
          </p>
          <div className={styles.badgeRow}>
            <span className={styles.badge}>Release Notes</span>
            <span className={styles.badge}>Community</span>
            <span className={styles.badge}>Deployment</span>
            <span className={styles.badge}>AI Ops</span>
          </div>
          <div className={styles.metrics}>
            <div className={styles.metricCard}>
              <strong>{items.length}</strong>
              <span>Published Posts</span>
              <p>沉淀版本动态、活动消息、视频内容和平台交付经验。</p>
            </div>
            <div className={styles.metricCard}>
              <strong>{tagCount || 1}</strong>
              <span>Topic Signals</span>
              <p>用标签把发布、部署、社区和使用经验连接成连续的信息流。</p>
            </div>
            <div className={styles.metricCard}>
              <strong>Docs+</strong>
              <span>Knowledge Hub</span>
              <p>博客与使用手册互相跳转，形成产品叙事和操作说明的闭环。</p>
            </div>
          </div>
        </div>
        <div className={styles.sideStack}>
          <aside className={styles.surfaceCard}>
            <h3>最近更新</h3>
            <ul>
              {latestItems.map((item) => (
                <li key={item.content.metadata.permalink}>
                  <Link className={styles.panelLink} to={item.content.metadata.permalink}>
                    {item.content.metadata.title}
                  </Link>
                </li>
              ))}
            </ul>
          </aside>
          <aside className={styles.surfaceCard}>
            <h3>内容导向</h3>
            <ul>
              <li>产品发布与版本说明</li>
              <li>IoTSharp 部署与交付经验</li>
              <li>社区活动与演示资料</li>
            </ul>
          </aside>
        </div>
      </div>
    </section>
  );
}

function BlogListPageContent(props) {
  const {metadata, items, sidebar} = props;
  return (
    <BlogLayout sidebar={sidebar}>
      <BlogHero metadata={metadata} items={items} />
      <BlogPostItems items={items} />
      <BlogListPaginator metadata={metadata} />
    </BlogLayout>
  );
}

export default function BlogListPage(props) {
  return (
    <HtmlClassNameProvider
      className={clsx(
        ThemeClassNames.wrapper.blogPages,
        ThemeClassNames.page.blogListPage,
      )}>
      <BlogListPageMetadata {...props} />
      <BlogListPageStructuredData {...props} />
      <BlogListPageContent {...props} />
    </HtmlClassNameProvider>
  );
}
