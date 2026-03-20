import React from 'react';
import clsx from 'clsx';
import {HtmlClassNameProvider, ThemeClassNames} from '@docusaurus/theme-common';
import {
  BlogPostProvider,
  useBlogPost,
} from '@docusaurus/plugin-content-blog/client';
import BlogLayout from '@theme/BlogLayout';
import BlogPostItem from '@theme/BlogPostItem';
import BlogPostPaginator from '@theme/BlogPostPaginator';
import BlogPostPageMetadata from '@theme/BlogPostPage/Metadata';
import BlogPostPageStructuredData from '@theme/BlogPostPage/StructuredData';
import TOC from '@theme/TOC';
import ContentVisibility from '@theme/ContentVisibility';
import styles from '../ExperienceHero.module.css';

function formatReadingTime(readingTime) {
  if (!readingTime) {
    return 'Quick Read';
  }
  return `${Math.max(1, Math.round(readingTime))} min read`;
}

function BlogPostHero() {
  const {metadata} = useBlogPost();
  const tags = metadata.tags || [];
  const summary = metadata.description || '围绕 IoTSharp 的一次产品、社区或交付更新。';

  return (
    <section className={styles.heroShell}>
      <div className={styles.heroGrid}>
        <div>
          <span className={styles.eyebrow}>IoTSharp Insight</span>
          <h1 className={styles.title}>{metadata.title}</h1>
          <p className={styles.lead}>{summary}</p>
          <div className={styles.metaLine}>
            <span>{metadata.formattedDate || 'Latest Update'}</span>
            <span>{formatReadingTime(metadata.readingTime)}</span>
            {metadata.authors?.length ? <span>{metadata.authors[0].name}</span> : null}
          </div>
          <div className={styles.badgeRow}>
            {tags.length > 0 ? (
              tags.map((tag) => (
                <span key={tag.label} className={styles.badge}>
                  {tag.label}
                </span>
              ))
            ) : (
              <span className={styles.badge}>IoTSharp</span>
            )}
          </div>
        </div>
        <div className={styles.sideStack}>
          <aside className={styles.surfaceCard}>
            <h3>这篇文章适合谁</h3>
            <ul>
              <li>准备了解平台新特性的使用者</li>
              <li>需要跟进部署与交付节奏的维护者</li>
              <li>希望快速抓住重点的社区贡献者</li>
            </ul>
          </aside>
          <aside className={styles.surfaceCard}>
            <h3>阅读导向</h3>
            <ul>
              <li>先看标题与摘要，快速建立上下文</li>
              <li>结合 TOC 直达你关心的段落</li>
              <li>需要操作细节时回到使用手册正文</li>
            </ul>
          </aside>
        </div>
      </div>
    </section>
  );
}

function BlogPostPageContent({sidebar, children}) {
  const {metadata, toc} = useBlogPost();
  const {nextItem, prevItem, frontMatter} = metadata;
  const {
    hide_table_of_contents: hideTableOfContents,
    toc_min_heading_level: tocMinHeadingLevel,
    toc_max_heading_level: tocMaxHeadingLevel,
  } = frontMatter;

  return (
    <BlogLayout
      sidebar={sidebar}
      toc={
        !hideTableOfContents && toc.length > 0 ? (
          <div className={styles.tocCard}>
            <TOC
              toc={toc}
              minHeadingLevel={tocMinHeadingLevel}
              maxHeadingLevel={tocMaxHeadingLevel}
            />
          </div>
        ) : undefined
      }>
      <ContentVisibility metadata={metadata} />
      <BlogPostHero />
      <div className={styles.articleCard}>
        <BlogPostItem>{children}</BlogPostItem>
      </div>
      {(nextItem || prevItem) && (
        <BlogPostPaginator nextItem={nextItem} prevItem={prevItem} />
      )}
    </BlogLayout>
  );
}

export default function BlogPostPage(props) {
  const BlogPostContent = props.content;
  return (
    <BlogPostProvider content={props.content} isBlogPostPage>
      <HtmlClassNameProvider
        className={clsx(
          ThemeClassNames.wrapper.blogPages,
          ThemeClassNames.page.blogPostPage,
        )}>
        <BlogPostPageMetadata />
        <BlogPostPageStructuredData />
        <BlogPostPageContent sidebar={props.sidebar}>
          <BlogPostContent />
        </BlogPostPageContent>
      </HtmlClassNameProvider>
    </BlogPostProvider>
  );
}
