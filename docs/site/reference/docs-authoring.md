---
layout: default
title: 文档维护
description: IoTSharp 文档站的维护方式与本地命令。
---

# 文档维护

## 目录

JekyllNet 站点源目录为 `docs/site`，建议按以下方式维护：

- `index.md`：站点首页
- `guide/*`：安装、部署、开发
- `reference/*`：NuGet、CI/CD、运维、文档维护
- `_layouts/*`：HTML 布局
- `assets/*`：CSS 与静态资源

## 本地命令

```bash
cd /home/runner/work/IoTSharp/IoTSharp
dotnet tool restore
dotnet jekyllnet build --source ./docs/site --destination ./docs/site/_site
dotnet jekyllnet serve --source ./docs/site --destination ./docs/site/_site --port 5055
```

## 发布

`docs-deploy.yml` 会在 `master` 分支的文档变更上：

1. 使用 `JekyllNet/action@v2.5` 构建站点
2. 上传 `docs/site/_site`
3. 部署到 GitHub Pages
