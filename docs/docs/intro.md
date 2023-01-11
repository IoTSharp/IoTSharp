---
sidebar_position: 1
---

# IoTSharp 简介

让我们探索一下  **IoTSharp 在五分钟内**.

##  IoTSharp 是什么？

IoTSharp 是一个开源的物联网基础平台，集设备属性数据管理、遥测数据监测、RPC多模式远程控制、规则链设计引擎等强大能力，依据数字孪生概念将可见与不可见的物理设备统一孪生到数字世界，在落地上IoTSharp结合了资产管理、产品化发展的理念，让平台应用更加贴合复杂的应用场景，在协议支持上支持HTTP、MQTT 、CoAp 等多种标准物联网协议接入和非标协议的转换。 


### IoTSharp的系统必备有哪些?

- [Docker]](https://www.docker.com/) 最新版本
  - 如果你要部署IoTSharp, 我们首先推荐的是docker， 以及Docker-Compose, 通过我们推荐的[docker-compose.yml](https://github.com/IoTSharp/IoTSharp/raw/master/Deployments/rabbit_mongo_influx/docker-compose.yml) 你可以直接部署成功，而不用煞费周折的部署环境。
- 关系型数据库   用来存储基础数据和属性数据。
  - PostgreSQL 验证过的版本为  PostgreSQL 11.3,12.x等。 
  - MySql   验证过的版本为 MySQL 8.0.17  
  - Oracle  验证过的版本为  Oracle Standard Edition 12c Release 2  ， 操作系统为Cent OS 7 
  - Sqlite  程序内置，均验证。 小项目推荐。 
  - SQLServer  验证过的版本为 Microsoft SQL Server 2016 (RTM-GDR) (KB4019088) - 13.0.1742.0 (X64)  
  - InMemory 通过EF 的内存数据库，一般用于测试 。 
  - Cassandra 现在开始我们通过 [EFCore.Cassandra](https://github.com/simpleidserver/EFCore.Cassandra) 支持了Cassandra。 
- 时序数据库    用来存储遥测数据并提供遥测数据的查询统计等等。 
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

### 如何使用docker-compose  安装IoTSharp ?

  * [RMI](https://github.com/IoTSharp/IoTSharp/tree/master/Deployments/rabbit_mongo_influx) 使用Rabbitmq 作为 EventBus, Mongodb 作为消息存储， 遥测数据使用Influx 2.0 ，这个方案中遥测数据也可以使用TDengine

  * [ZPT](https://github.com/IoTSharp/IoTSharp/tree/master/Deployments/zeromq_taos) 使用ZeroMQ 作为 EventBus, PostgreSQL 作为消息存储， 遥测数据使用  TDengine  

  * [ZPS](https://github.com/IoTSharp/IoTSharp/tree/master/Deployments/zeromq_sharding)  默认开发配置，  IoTSharp 和 PostgreSql, 遥测数据可以通过单表或者分表。 


## 初次使用

* 初始租户和管理员、用户注册信息在系统发现你未初始化时自动跳转到安装界面， 填写完成后， 系统会初始化权限、基础数据等种子数据。 
* X509 CA证书用于通过证书进行保障安全通讯和关系验证， 第一次时需要调用高级管理员权限办法并写入系统， 尤其是当你使用了非管理员用户进行启动IoTSharp 时 ， 因此， 需要确保第一次生成使用高权限， 后续使用低权限。 

 
## 关于赞助

* 我们接受资金以及任何方式的的捐赠，但并不意味着我们会为您承诺或担保任何事情， 也并不意味着对你使用IoTSharp带来的负面影响负有责任。 所有你使用IoTSharp造成的任何损失以及任何关联的责任等我们均不会有任何责任和义务承担，你需要为你做的决定而负责。 
* IoTSharp开源并不等于你可以用他申报项目、申请专利、提供云服务、重新包装等某种其他方式来获利但对IoTSharp毫无建树。 我们讨厌这种自私行为。 

 
