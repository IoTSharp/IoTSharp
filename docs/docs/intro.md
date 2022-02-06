---
sidebar_position: 1
---

# 参考手册

让我们探索一下  **IoTSharp 在五分钟内**.

##  IoTSharp 是什么？

 IoTSharp 是一个基于.Net 6.0 使用C#开发的数据收集、处理、可视化与设备管理的开源物联网(IoT)平台基础平台,支持 HTTP、MQTT 、CoAP等协议实现设备的数字孪生,且属性数据和遥测数据协议简单类型丰富,简易设置即可将数据存储在PostgreSql、MySql、Oracle、SQLServer、Sqlite 或者 InfluxDB 2.0;TDengine;TimescaleDB等时序数据库中。


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

 
 