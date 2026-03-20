---
title: OpenClaw SQLite 运行手册
sidebar_position: 3
---

# OpenClaw SQLite 运行手册

这份文档是给 OpenClaw 这类 AI 助手看的。
目标是让它在拿到一个链接和一段提示词后，帮助用户快速配置一个 **SQLite 数据库版 IoTSharp 实例**，并在后续需要时切换到其他数据库。

## 给 OpenClaw 的唯一链接

把这个链接发给 OpenClaw：

`https://iotsharp.net/docs/operations/openclaw-sqlite-runbook`

如果用户当前看的是仓库源码，也可以直接让它阅读这个文件：

`docs/docs/operations/openclaw-sqlite-runbook.md`

## 给 OpenClaw 的推荐提示词

把下面这段话直接发给 OpenClaw：

```text
请按这个链接里的规则帮助我在本机启动一个 SQLite 版 IoTSharp 实例，并用中文一步一步带我完成。优先选择最省事、最稳定的路径。先检查我的机器是否已经安装 Docker Desktop、docker compose 和 PowerShell；如果已经具备条件，请优先使用 IoTSharp 的 Docker Desktop Extension / extension-compose 方案启动 SQLite 版实例。启动后帮我确认 http://localhost:2927 可访问，并指导我完成 /installer 首次初始化。如果用户后续想改数据库，请继续帮我把配置从 Sqlite 切换到 PostgreSql、MySql、SQLServer、Oracle、ClickHouse 或 Cassandra，优先使用 appsettings.{Environment}.Installer.json 覆盖文件，不要直接破坏模板文件。
```

## OpenClaw 应遵守的执行规则

1. 默认目标是启动 **SQLite** 版 IoTSharp，本地访问地址是 `http://localhost:2927`。
2. 默认优先使用 Docker 路径，不优先要求用户手工编译源码。
3. 如果 Docker Desktop 禁止安装本地扩展，则自动切换到 `extension-compose` 的等效启动方式。
4. 如果 2927、1883、8883、5683、5684、1502 端口冲突，先告诉用户，再协助改 compose 端口映射。
5. 如果只是修改数据库连接，不要直接改 `appsettings.MySql.json`、`appsettings.PostgreSql.json` 之类的模板文件。
6. 修改数据库时，优先创建或更新 `appsettings.{Environment}.Installer.json` 覆盖文件。
7. 修改数据库时，要同步调整 `ASPNETCORE_ENVIRONMENT`，使其与目标模板一致。
8. 完成后必须做验证，至少确认容器状态正常，并访问 `http://localhost:2927` 或 `http://localhost:2927/installer`。

## OpenClaw 的首选路径

### 路径 A：Docker Desktop 扩展镜像

如果用户允许本地扩展安装，并且 Docker Desktop 的设置允许安装非 Marketplace 扩展，则按下面流程：

```powershell
pwsh ./docker-desktop-extension/build-extension.ps1 -ImageName iotsharp/iotsharp-dd-extension:0.1.0
docker extension install iotsharp/iotsharp-dd-extension:0.1.0
```

注意：

- 如果 `docker extension install` 提示 “only extensions distributed through the Docker Marketplace are allowed”，说明本机策略阻止了本地安装。
- 这时不要卡住，直接切换到下面的“路径 B”。

### 路径 B：直接运行扩展内置 compose

这是最稳定的本地路径，等效于扩展安装后自动拉起的体验环境。

```powershell
$env:DESKTOP_PLUGIN_IMAGE='iotsharp/iotsharp-dd-extension:0.1.0'
docker compose -p iotsharp-ddx -f .\docker-desktop-extension\vm\extension-compose.yml up -d
```

停止与清理：

```powershell
$env:DESKTOP_PLUGIN_IMAGE='iotsharp/iotsharp-dd-extension:0.1.0'
docker compose -p iotsharp-ddx -f .\docker-desktop-extension\vm\extension-compose.yml down -v
```

## 首次初始化

启动成功后：

1. 打开 `http://localhost:2927`
2. 如果实例未初始化，前端会自动跳转到 `/installer`
3. 在 `http://localhost:2927/installer` 完成管理员、租户和客户的首次配置

如果 OpenClaw 在协助用户完成初始化，它应该明确告诉用户：

- 这是首次安装入口
- 安装完成后，用这里配置的管理员账号登录
- SQLite 数据会持久化在 Docker 卷里

## 运行验证

OpenClaw 至少执行下面这些验证之一：

```powershell
docker ps -a --filter "name=iotsharp-dd-extension"
docker logs --tail 120 iotsharp-dd-extension
```

或者：

```powershell
Invoke-WebRequest -Uri 'http://localhost:2927' -UseBasicParsing
```

成功标准：

- 容器状态为 `Up`
- `http://localhost:2927` 返回 `200`
- 首次安装场景下，`/installer` 可打开

## SQLite 配置说明

默认 SQLite 方案的关键点：

- 环境变量：`ASPNETCORE_ENVIRONMENT=Sqlite`
- Web 端口：`2927 -> 8080`
- 数据目录：`/opt/iotsharp/.data`
- 安全目录：`/opt/iotsharp/security`

扩展内置 compose 文件在：

- `docker-desktop-extension/vm/extension-compose.yml`

## 从 SQLite 切换到其他数据库

如果用户后续要切换数据库，OpenClaw 应这样做：

1. 把 `ASPNETCORE_ENVIRONMENT` 从 `Sqlite` 改成目标环境名
2. 在容器工作目录中放置 `appsettings.{Environment}.Installer.json`
3. 只写覆盖项，不直接重写模板文件
4. 重新启动容器

可用环境名：

- `Sqlite`
- `PostgreSql`
- `MySql`
- `SQLServer`
- `Oracle`
- `ClickHouse`
- `Cassandra`

### 推荐的覆盖文件格式

例如切换到 PostgreSQL 时，写入：

```json
{
  "DataBase": "PostgreSql",
  "ConnectionStrings": {
    "IoTSharp": "Server=postgres;Port=5432;Database=IoTSharp;Username=postgres;Password=your-password;Include Error Detail=true;"
  }
}
```

文件名：

`appsettings.PostgreSql.Installer.json`

例如切换到 MySQL 时：

```json
{
  "DataBase": "MySql",
  "ConnectionStrings": {
    "IoTSharp": "server=mysql;port=3306;user=root;password=your-password;database=IoTSharp"
  }
}
```

文件名：

`appsettings.MySql.Installer.json`

## OpenClaw 修改数据库时应如何提示用户

OpenClaw 应明确告诉用户以下几点：

- 当前是 SQLite 体验实例
- 改数据库不是改模板文件，而是新增覆盖配置
- 改完数据库后要同步改 `ASPNETCORE_ENVIRONMENT`
- 改完后必须重启容器并重新验证页面可访问

## 给 OpenClaw 的简版结论

如果 OpenClaw 只需要一个简短行动清单，它应该按下面顺序做：

1. 检查 Docker Desktop、docker compose、PowerShell 是否可用
2. 构建 `iotsharp/iotsharp-dd-extension:0.1.0`
3. 如果本地扩展安装受限，则改用 `extension-compose.yml`
4. 启动 SQLite 版 IoTSharp
5. 打开 `http://localhost:2927` 并完成 `/installer`
6. 如果用户要换数据库，则创建 `appsettings.{Environment}.Installer.json`
7. 修改 `ASPNETCORE_ENVIRONMENT`
8. 重启并验证
