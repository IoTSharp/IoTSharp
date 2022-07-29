![IOTSharp LOGO](docs/static/img/350x100.png)  

[![Build status](https://ci.appveyor.com/api/projects/status/5o23f5vss89ct2lw/branch/master?svg=true)](https://ci.appveyor.com/project/MaiKeBing/iotsharp/branch/master)
![GitHub](https://img.shields.io/github/license/iotsharp/iotsharp.svg)
![.NET Core](https://github.com/IoTSharp/IoTSharp/workflows/.NET%20Core/badge.svg?branch=master)

IoTSharp 是一个 基于.Net Core 开源的物联网基础平台， 支持 HTTP、MQTT 、CoAp 协议， 属性数据和遥测数据协议简单类型丰富，简单设置即可将数据存储在PostgreSql、MySql、Oracle、SQLServer、Sqlite，是一个用于数据收集、处理、可视化与设备管理的 IoT 平台.



## 如何使用docker-compose  安装IoTSharp ?

 * [ZPT](https://github.com/IoTSharp/IoTSharp/tree/master/Deployments/zeromq_taos) 使用ZeroMQ 作为 EventBus, PostgreSQL 作为消息存储， 遥测数据使用  TDengine  

 * [ZPS](https://github.com/IoTSharp/IoTSharp/tree/master/Deployments/zeromq_sharding)  默认开发配置，  IoTSharp 和 PostgreSql, 遥测数据可以通过单表或者分表。 

 * [RMI](https://github.com/IoTSharp/IoTSharp/tree/master/Deployments/rabbit_mongo_influx) 使用Rabbitmq 作为 EventBus, Mongodb 作为消息存储， 遥测数据使用Influx 2.0 ，这个方案中遥测数据也可以使用TDengine

 更多的 [部署方案请点这里访问](https://github.com/IoTSharp/IoTSharp/tree/master/Deployments)


## 支持哪些数据库?

 *  [PostgreSql](IoTSharp/appsettings.PostgreSql.json) 测试环境是   PostgreSQL 11.3,支持分表.
 *  [MySql](IoTSharp/appsettings.MySql.json)  测试环境是MySQL 8.0.17,支持分表.
 *  [Oracle](IoTSharp/appsettings.Oracle.json)  测试环境是 Oracle Standard Edition 12c Release 2, 支持分表.请参见: https://github.com/MaksymBilenko/docker-oracle-12c
 *  [SQLServer](IoTSharp/appsettings.SQLServer.json) 测试环境是  Microsoft SQL Server 2016 (RTM-GDR) (KB4019088) - 13.0.1742.0 (X64),支持分表
 *  [Sqlite](IoTSharp/appsettings.Sqlite.json)  支持分表。
 *  [Cassandra](IoTSharp/appsettings.Cassandra.json)  

## 支持的消息队列中间件?

 *  RabbitMQ
 *  Kafka 
 *	InMemory 
 *	ZeroMQ 
 *	NATS 
 *	Pulsar 
 *	RedisStreams 
 *	AmazonSQS 
 *	AzureServiceBus 

## 持的事件存储支中间件。
* PostgreSql,
* MongoDB,
* InMemory,
* LiteDB,
* MySql,
* SqlServer

## 演示：
  http://139.9.232.10:2927

## 文档：
  https://docs.iotsharp.net/

## 如何使用 docker 安装IoTSharp?

  -  docker pull maikebing/iotsharp


## 如何使用Linux 安装?

 -  mkdir  /var/lib/iotsharp/
 -	cp ./*  /var/lib/iotsharp/
 -	chmod 777 /var/lib/iotsharp/IoTSharp
 -	cp  iotsharp.service   /etc/systemd/system/iotsharp.service
 -	sudo systemctl enable  /etc/systemd/system/iotsharp.service 
 -	sudo systemctl start  iotsharp.service 
 -	sudo journalctl -fu  iotsharp.service 



##  IoTSharp.SDKs

- IoTSharp.Sdk.Http   [![IoTSharp.Sdk.Http](https://img.shields.io/nuget/v/IoTSharp.Sdk.Http.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.Http/)
- IoTSharp.Sdk.MQTT   [![IoTSharp.Sdk.MQTT](https://img.shields.io/nuget/v/IoTSharp.Sdk.MQTT.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.MQTT/)

 

## IoTSharp-C-Client-Sdk

IoTSharp-C-client-Sdk 是mqtt客户端， c语言编写的例子。 

 [https://github.com/IoTSharp/IoTSharp.Sdks.MQTT-C](https://github.com/IoTSharp/IoTSharp.Sdks.MQTT-C)

## paho.mqtt.c 的例子

这个跟 IoTSharp-C-Client-Sdk 一样， 但是使用了 paho.mqtt.c
 https://github.com/IoTSharp/IoTSharp.Edge.paho.mqtt.c

## IoTSharp 的 nanoFramework 例子

IoTSharp.Edge.nanoFramework 是一个 nanoFramework's mqtt 客户端， 它允许在STM32 ！

  https://github.com/IoTSharp/IoTSharp.Edge.nanoFramework

更多信息请读这里 https://www.cnblogs.com/MysticBoy/p/13159648.html
官方网站为：  https://www.nanoframework.net/ 

##  IoTSharp 的RT-Thread 开发包

https://github.com/IoTSharp/iotsharp-rtthread-package

|  
IoTSharp 的软件生态

- MaiKeBing.CAP.ZeroMQ [![MaiKeBing.CAP.ZeroMQ](https://img.shields.io/nuget/v/MaiKeBing.CAP.ZeroMQ.svg)](https://www.nuget.org/packages/MaiKeBing.CAP.ZeroMQ/)
- MaiKeBing.CAP.LiteDB  [![MaiKeBing.CAP.LiteDB](https://img.shields.io/nuget/v/MaiKeBing.CAP.LiteDB.svg)](https://www.nuget.org/packages/MaiKeBing.CAP.LiteDB/)
- MaiKeBing.HostedService.ZeroMQ  [![MaiKeBing.HostedService.ZeroMQ](https://img.shields.io/nuget/v/MaiKeBing.HostedService.ZeroMQ.svg)](https://www.nuget.org/packages/MaiKeBing.HostedService.ZeroMQ/)
- IoTSharp.X509Extensions  [![IoTSharp.X509Extensions](https://img.shields.io/nuget/v/IoTSharp.X509Extensions.svg)](https://www.nuget.org/packages/IoTSharp.X509Extensions/)
- Silkier    [![Silkier](https://img.shields.io/nuget/v/Silkier.svg)](https://www.nuget.org/packages/Silkier/) 
- Silkier.EFCore   [![Silkier.EFCore](https://img.shields.io/nuget/v/Silkier.EFCore.svg)](https://www.nuget.org/packages/Silkier.EFCore/)
- Silkier.AspNetCore  [![Silkier.AspNetCore](https://img.shields.io/nuget/v/Silkier.AspNetCore.svg)](https://www.nuget.org/packages/Silkier.AspNetCore/)
- SilkierQuartz   [![SilkierQuartz](https://img.shields.io/nuget/v/SilkierQuartz.svg)](https://www.nuget.org/packages/SilkierQuartz/)
- IoTSharp.EntityFrameworkCore.Taos   [![IoTSharp.EntityFrameworkCore.Taos](https://img.shields.io/nuget/v/IoTSharp.EntityFrameworkCore.Taos.svg)](https://www.nuget.org/packages/IoTSharp.EntityFrameworkCore.Taos/)
- IoTSharp.Sdk.Http   [![IoTSharp.Sdk.Http](https://img.shields.io/nuget/v/IoTSharp.Sdk.Http.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.Http/)
- IoTSharp.Sdk.MQTT   [![IoTSharp.Sdk.MQTT](https://img.shields.io/nuget/v/IoTSharp.Sdk.MQTT.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.MQTT/)



## 贡献

[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](https://github.com/IoTSharp/IoTSharp/pulls)

如果你有兴趣贡献代码，可以创建[Pull Request](https://github.com/IoTSharp/IoTSharp/pulls), 或者[Bug Report](https://github.com/IoTSharp/IoTSharp/issues/new).

### 贡献者

这个项目的存在得益于所有的贡献者， 感谢他们。

<a href="https://github.com/IoTSharp/IoTSharp/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=IoTSharp/IoTSharp" />
</a>

## 捐赠

This project is an  Apache 2.0 licensed open source project. In order to achieve better and sustainable development of the project, we expect to gain more backers. We will use the proceeds for community operations and promotion. You can support us in any of the following ways:

- [OpenCollective](https://opencollective.com/IoTSharp)
- 微信![二维码](docs/static/img/maikebing_wxpay.png)

We will put the detailed donation records on the below!



|                                                        | 姓名                                  | Stars | 捐赠 | 留言 |
| ------------------------------------------------------------ | ------------------ | -------- | -------- | -------- |
| [![@iioter](https://avatars.githubusercontent.com/u/29589505?s=80&v=4)](https://github.com/iioter) | whd | ![GitHub User's stars](https://img.shields.io/github/stars/iioter?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge) | ￥1024 =120+100+292+512(码云共计四次) |  |
| [![@nnhy](https://avatars.githubusercontent.com/u/506367?s=80&v=4)](https://github.com/nnhy) | 大石头 | ![GitHub User's stars](https://img.shields.io/github/stars/nnhy?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge) |     ￥672=666+5（码云+公众号）     |            |
|  | 无敌飞行家 | ![GitHub User's stars](https://img.shields.io/github/stars/hehaoyu_2014?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge) | ￥5=5(公众号) |  |
|  | 匿名公司 |  | ￥1000=1000(微信转账) |  |
| [![@davidzhu001](https://avatars.githubusercontent.com/u/9436230?s=80&v=4)](https://github.com/davidzhu001)   | 农民也很疯狂 |  ![GitHub User's stars](https://img.shields.io/github/stars/davidzhu001?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge)| ￥400=200+200 微信转账 | |
| [![@280780363](https://avatars.githubusercontent.com/u/20083278?s=80&v=4)](https://github.com/280780363)   | 谷草 |  ![GitHub User's stars](https://img.shields.io/github/stars/280780363?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge)| ￥88 微信转账 | |





## 社区支持

If you encounter any problems in the process, feel free to ask for help via following channels. We also encourage experienced users to help newcomers.

- [![Discord Server](https://img.shields.io/discord/895689311612178442?color=%237289DA&label=IoTSharp&logo=discord&logoColor=white&style=flat-square)](https://discord.gg/My6PaTmUvu)

| 公众号 |    [QQ群63631741](https://jq.qq.com/?_wv=1027&k=HJ7h3gbO)  |  微信群  |
| ------ | ---- | ---- |
| ![](docs/static/img/qrcode.jpg) | ![](docs/static/img/IoTSharpQQGruop.png) | ![企业微信群](docs/static/img/qyqun.jpg) |





## dotNET China

[![DotNetChina](https://images.gitee.com/uploads/images/2021/0309/134044_9c191d7b_974299.png)](https://gitee.com/dotnetchina/IoTSharp)

