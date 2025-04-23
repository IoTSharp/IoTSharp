<p align="left">
  <a href="https://iotsharp.net/">
    <img src="docs/static/img/logo_white.svg" width="360px" alt="IoTSharp logo" />
  </a>
</p>

[![Build status](https://ci.appveyor.com/api/projects/status/5o23f5vss89ct2lw/branch/master?svg=true)](https://ci.appveyor.com/project/MaiKeBing/iotsharp/branch/master)
![GitHub](https://img.shields.io/github/license/iotsharp/iotsharp.svg)
[![.NET Core build](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-build.yml)
[![pages-build-deployment](https://github.com/IoTSharp/IoTSharp/actions/workflows/pages/pages-build-deployment/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/pages/pages-build-deployment)
[![Building and Packaging](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-publish.yml/badge.svg)](https://github.com/IoTSharp/IoTSharp/actions/workflows/dotnet-publish.yml)
![Docker Cloud Build Status](https://img.shields.io/docker/cloud/build/maikebing/iotsharp?style=flat-square)
![Docker Pulls](https://img.shields.io/docker/pulls/maikebing/iotsharp)
![GitHub all releases](https://img.shields.io/github/downloads/iotsharp/iotsharp/total)
[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2FIoTSharp%2FIoTSharp.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FIoTSharp%2FIoTSharp?ref=badge_shield)
[![star](https://gitee.com/IoTSharp/IoTSharp/badge/star.svg?theme=gvp)](https://gitee.com/IoTSharp/IoTSharp/stargazers)
[![fork](https://gitee.com/IoTSharp/IoTSharp/badge/fork.svg?theme=gvp)](https://gitee.com/IoTSharp/IoTSharp/members)

IoTSharp 是一个开源的物联网基础平台，集设备属性数据管理、遥测数据监测、RPC多模式远程控制、规则链设计引擎等强大能力，依据数字孪生概念将可见与不可见的物理设备统一孪生到数字世界，在落地上IoTSharp结合了资产管理、产品化发展的理念，让平台应用更加贴合复杂的应用场景，在协议支持上支持HTTP、MQTT 、CoAp 等多种标准物联网协议接入和非标协议的转换。  



 * ## 愿项目用户:

   * 愿您行善，不作恶。

    * 愿您原谅自己，原谅别人。

    * 愿您自由分享，永远不要拿走超过您给予的。

       

## 支持的数据库:

 *  [PostgreSql](IoTSharp/appsettings.PostgreSql.json) 测试环境 PostgreSQL 11.3 支持分表
 *  [MySql](IoTSharp/appsettings.MySql.json) 测试环境 MySQL 8.0.17,支持分表.
 *  [Oracle](IoTSharp/appsettings.Oracle.json)  测试环境  Oracle Standard Edition 12c Release 2 on CentOS ,支持分表.  See also: https://github.com/MaksymBilenko/docker-oracle-12c
 *  [SQLServer](IoTSharp/appsettings.SQLServer.json)  Microsoft SQL Server 2016 (RTM-GDR) (KB4019088) - 13.0.1742.0 (X64)   ,Support for  sharding
 *  [Sqlite](IoTSharp/appsettings.Sqlite.json) 支持分表
 *  [Cassandra](IoTSharp/appsettings.Cassandra.json)  

## 支持的时序数据库:

 *  InfluxDB   
 *  IoTDB
 *  TDengine
 *  TimescaleDB
 *  PinusDB  
 *  同时也支持关系型数据库，并且可以单表或者分表.  


## 支持的消息中间件：

 *  RabbitMQ
 *  Kafka 
 *	InMemory 
 *	ZeroMQ 
 *	NATS 
 *	Pulsar 
 *	RedisStreams 
 *	AmazonSQS 
 *	AzureServiceBus 

## 支持的事件消息存储方式:
* PostgreSql,
* MongoDB,
* InMemory,
* LiteDB,
* MySql,
* SqlServer

## IoTSharp 在线 

 - [使用Vue3实现的IoTSharp前端](http://host.iotsharp.net)

 

## 如何部署?

- [使用Docker部署](https://iotsharp.net/docs/tutorial-basics/deploy_by_docker)
- [部署到Linux服务器或者树莓派](https://iotsharp.net/docs/tutorial-basics/deploy_linux)
- [部署到Windows](https://iotsharp.net/docs/tutorial-basics/deploy_win)

## 如何配置?

- [应用基本配置](https://iotsharp.net/docs/tutorial-basics/appsettings) 



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


## IoTSharp 的软件生态


- IoTSharp.Sdk.Http   [![IoTSharp.Sdk.Http](https://img.shields.io/nuget/v/IoTSharp.Sdk.Http.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Sdk.Http) ](https://www.nuget.org/packages/IoTSharp.Sdk.Http/)
- IoTSharp.Sdk.MQTT   [![IoTSharp.Sdk.MQTT](https://img.shields.io/nuget/v/IoTSharp.Sdk.MQTT.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Sdk.MQTT)  ](https://www.nuget.org/packages/IoTSharp.Sdk.MQTT/)
- MQTTnet.AspNetCore.Routing   [![MQTTnet.AspNetCore.Routing](https://img.shields.io/nuget/v/MQTTnet.AspNetCore.Routing.svg) ![Nuget](https://img.shields.io/nuget/dt/MQTTnet.AspNetCore.Routing)  ](https://www.nuget.org/packages/MQTTnet.AspNetCore.Routing/)
- IoTSharp.EntityFrameworkCore.Taos   [![IoTSharp.EntityFrameworkCore.Taos](https://img.shields.io/nuget/v/IoTSharp.EntityFrameworkCore.Taos.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.EntityFrameworkCore.Taos)  ](https://www.nuget.org/packages/IoTSharp.EntityFrameworkCore.Taos/)
- IoTSharp.X509Extensions [![IoTSharp.X509Extensions](https://img.shields.io/nuget/v/IoTSharp.X509Extensions.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.X509Extensions) ](https://www.nuget.org/packages/IoTSharp.X509Extensions/)
- IoTSharp.Extensions.RESTful  [![IoTSharp.Extensions.RESTful](https://img.shields.io/nuget/v/IoTSharp.Extensions.RESTful.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions.RESTful) ](https://www.nuget.org/packages/IoTSharp.Extensions.RESTful/)
- IoTSharp.Extensions.QuartzJobScheduler  [![IoTSharp.Extensions.QuartzJobScheduler](https://img.shields.io/nuget/v/IoTSharp.Extensions.QuartzJobScheduler.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions.QuartzJobScheduler) ](https://www.nuget.org/packages/IoTSharp.Extensions.QuartzJobScheduler/)
- IoTSharp.Extensions.EFCore  [![IoTSharp.Extensions.EFCore](https://img.shields.io/nuget/v/IoTSharp.Extensions.EFCore.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions.EFCore)  ](https://www.nuget.org/packages/IoTSharp.Extensions.EFCore/)
- IoTSharp.Extensions.BouncyCastle  [![IoTSharp.Extensions.BouncyCastle](https://img.shields.io/nuget/v/IoTSharp.Extensions.BouncyCastle.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions.BouncyCastle) ](https://www.nuget.org/packages/IoTSharp.Extensions.BouncyCastle/)
- IoTSharp.Extensions.AspNetCore  [![IoTSharp.Extensions.AspNetCore](https://img.shields.io/nuget/v/IoTSharp.Extensions.AspNetCore.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions.AspNetCore) ](https://www.nuget.org/packages/IoTSharp.Extensions.AspNetCore/)
- IoTSharp.Extensions  [![IoTSharp.Extensions](https://img.shields.io/nuget/v/IoTSharp.Extensions.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions)  ](https://www.nuget.org/packages/IoTSharp.Extensions/)


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
- [爱发电](https://afdian.net/a/maikebing)
- [捐赠者名单](BACKERS.md)
- 微信![二维码](docs/static/img/maikebing_wxpay.png)
 

## 社区支持

If you encounter any problems in the process, feel free to ask for help via following channels. We also encourage experienced users to help newcomers.

 
| 公众号 |    [QQ群63631741](https://jq.qq.com/?_wv=1027&k=HJ7h3gbO)  |  微信群  |
| ------ | ---- | ---- |
| ![](docs/static/img/qrcode.jpg) | ![](docs/static/img/IoTSharpQQGruop.png) | ![企业微信群](docs/static/img/qyqun.jpg) |



## dotNET China

[![DotNetChina](https://images.gitee.com/uploads/images/2021/0309/134044_9c191d7b_974299.png)](https://gitee.com/dotnetchina)

## 优秀开源社区
* [流之云](https://gitee.com/ntdgg) 信息化、数字化服务提供商
* [translate.js](https://gitee.com/mail_osc/translate) 网页自动翻译，页面无需另行改造，加入两行js即可让你的网页快速具备多国语言切换能力！
* [IoTGateway](https://gitee.com/iioter/iotgateway) IoTGateway是一个基于.Net6.0 开源的物联网网关，通过可视化配置，轻松的连接到你的任何设备和物联网平台。
