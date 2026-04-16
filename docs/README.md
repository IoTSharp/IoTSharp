# IoTSharp Docs

IoTSharp 文档站现已切换为基于 `JekyllNet` 的 GitHub Pages 流程，站点源目录位于 `docs/site`。

## 技术基线

- .NET SDK `10.0.x`
- `JekyllNet` 本地工具 `0.2.5`
- GitHub Pages + `JekyllNet/action@v2.5`

## 本地构建

```bash
cd /home/runner/work/IoTSharp/IoTSharp
dotnet tool restore
dotnet jekyllnet build --source ./docs/site --destination ./docs/site/_site
```

## 本地预览

```bash
cd /home/runner/work/IoTSharp/IoTSharp
dotnet tool restore
dotnet jekyllnet serve --source ./docs/site --destination ./docs/site/_site --port 5055
```

默认预览地址：<http://localhost:5055>

## GitHub Pages

文档工作流使用 `JekyllNet/action@v2.5` 构建，并通过官方 Pages 工作流上传和部署 `docs/site/_site`。

站点默认域名为：

- <https://iotsharp.net/>

## 文档结构

- `docs/site/index.md`：文档首页
- `docs/site/guide/*`：安装、部署、开发入门
- `docs/site/reference/*`：NuGet、CI/CD、发布与文档维护说明
