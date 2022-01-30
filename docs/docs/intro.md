---
sidebar_position: 1
---

# 参考手册

让我们探索一下  **IoTSharp 在五分钟内**.

## 开始了解

Get started by **creating a new site**.

Or **try Docusaurus immediately** with **[docusaurus.new](https://docusaurus.new)**.

### 需要那些依赖?

- [Docker]](https://www.docker.com/) 最新版本
  - 如果你要部署IoTSharp, 我们首先推荐的是docker， 以及Docker-Compose, 通过我们推荐的[docker-compose.yml](https://github.com/IoTSharp/IoTSharp/raw/master/Deployments/rabbit_mongo_influx/docker-compose.yml) 你可以直接部署成功，而不用煞费周折的部署环境。
- 关系型数据库  
  - PostgreSQL 验证过的版本为  PostgreSQL 11.3,12.x等。 
  - MySql   验证过的版本为 MySQL 8.0.17  
  - Oracle  验证过的版本为  Oracle Standard Edition 12c Release 2  ， 操作系统为Cent OS 7 
  - Sqlite  程序内置，均验证。 小项目推荐。 
  - SQLServer  验证过的版本为 Microsoft SQL Server 2016 (RTM-GDR) (KB4019088) - 13.0.1742.0 (X64)  
  - InMemory 通过EF 的内存数据库，一般用于测试 。 
- 时序数据库
  - 通过EFCore 使用关系型数据库来存储带有时间戳的数据，虽然不推荐，但不妨是一种小型项目的最佳选择。 
  - InfluxDB 2.x 我们致力于推荐的时序数据库， InfluxDB集成非常好用的可视化工具， 除了不符合信创没有任何可挑剔的。
  - TDengine  我们致力于推荐的国产时序数据库， 甚至为了支持它我花了大量时间编写他的提供程序 [Maikebing.EntityFrameworkCore.Taos](https://github.com/maikebing/Maikebing.EntityFrameworkCore.Taos)
  - PinusDB  国产松果时序数据库， 简单易用， 我们也为他编写了提供程序， [PinusDB.Data](https://github.com/maikebing/PinusDB.Data) 
  - TimescaleDB  基于PostgreSQL的时序数据库， 你可以直接选择它来当时序数据库也可以当关系型数据库， 一次搞定。 
  - 关系数据库 分区法 ， 我们有支持这种方式，但始终不推荐，除非你想只想用一个数据库且通过分区就能搞定你的数据量。 
  - SingleTable  通过EF的的单表存储。 通过单表， 我们就不需要依赖于数据库或者分区等等。 小项目推荐。 
- 消息队列  我们是通过CAP项目来实现的，因此它支持的理论上我们都支持。 
  - RabbitMQ 我们推荐的。 
  - Kafka   测试似乎正常。 
  - ZeroMQ  针对出门的ZeroMQ , 我们编写了MaiKeBing.CAP.ZeroMQ 和 MaiKeBing.HostedService.ZeroMQ  以支持它。 
  - InMemory 通过它可以不需要依赖任何外接， 这是CAP提供的一种途径。 小项目推荐。 
- 消息队列存储
  - PostgreSql 如果全称用PostgreSQL 可以考虑。 
  - MongoDB  我们推荐的
  - LiteDB  .Net 编写的NoSQL 项目， 小项目推荐， 
  - InMemory 存储在内存， 不依赖于外接。 小项目推荐。 

## 如何部署？

Generate a new Docusaurus site using the **classic template**.

The classic template will automatically be added to your project after you run the command:

```bash
npm init docusaurus@latest my-website classic
```

You can type this command into Command Prompt, Powershell, Terminal, or any other integrated terminal of your code editor.

The command also installs all necessary dependencies you need to run Docusaurus.

## Start your site

Run the development server:

```bash
cd my-website
npm run start
```

The `cd` command changes the directory you're working with. In order to work with your newly created Docusaurus site, you'll need to navigate the terminal there.

The `npm run start` command builds your website locally and serves it through a development server, ready for you to view at http://localhost:3000/.

Open `docs/intro.md` (this page) and edit some lines: the site **reloads automatically** and displays your changes.
