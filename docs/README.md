# IoTSharp Docs

IoTSharp 文档站点使用 Docusaurus 构建。

## 本地运行

```bash
cd docs
npm install
npm run start
```

## 生产构建

```bash
cd docs
npm run build
```

## GitHub Pages

站点支持通过环境变量切换部署地址：

- `DOCS_SITE_URL`
- `DOCS_BASE_URL`

默认面向 `https://iotsharp.net/`，在 GitHub Pages 工作流中会覆盖为对应 Pages 地址。
