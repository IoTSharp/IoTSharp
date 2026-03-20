# IoTSharp Docs

IoTSharp 帮助手册站点基于 Docusaurus 构建，面向 GitHub Pages 与独立域名发布。

## 技术基线

- Docusaurus `3.9.2`
- Node.js `20+`
- npm `10+`

## 本地运行

```bash
cd docs
npm install
npm run start
```

默认会启动本地开发站点，可用于实时预览主题和文档变更。

## 生产构建

```bash
cd docs
npm run build
```

## 本地预览构建产物

```bash
cd docs
npm run serve
```

## GitHub Pages

站点支持通过以下环境变量切换发布地址：

- `DOCS_SITE_URL`
- `DOCS_BASE_URL`

默认面向 `https://iotsharp.net/`，在 GitHub Pages 工作流中可以覆盖为对应的 Pages 地址。
