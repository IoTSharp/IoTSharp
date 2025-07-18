<p align="left">
  <a href="https://iotsharp.net/">
    <img src="docs/static/img/logo_white.svg" width="360px" alt="IoTSharp logo" />
  </a>
</p>

[![star](https://gitcode.com/IoTSharp/IoTSharp/star/badge.svg)](https://gitcode.com/IoTSharp/IoTSharp)
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


IoTSharp is an open-source IoT platform for data collection, processing, visualization, and device management.

## Here is a blessing for all users of this project
 * May you do good and not evil.
 * May you find forgiveness for yourself and forgive others.
 * May you share freely, never taking more than you give.


## What databases are supported?

 *  [PostgreSql](IoTSharp/appsettings.PostgreSql.json) The test environment is  PostgreSQL 11.3,Support for  sharding.
 *  [MySql](IoTSharp/appsettings.MySql.json) The test environment is MySQL 8.0.17,Support for  sharding.
 *  [Oracle](IoTSharp/appsettings.Oracle.json)  The test environment is  Oracle Standard Edition 12c Release 2 on CentOS , Support for  sharding.  See also: https://github.com/MaksymBilenko/docker-oracle-12c
 *  [SQLServer](IoTSharp/appsettings.SQLServer.json)  Microsoft SQL Server 2016 (RTM-GDR) (KB4019088) - 13.0.1742.0 (X64)   ,Support for  sharding
 *  [Sqlite](IoTSharp/appsettings.Sqlite.json) Support for  sharding
 *  [Cassandra](IoTSharp/appsettings.Cassandra.json)  

## What time series databases are supported??

 *  InfluxDB   
 *  IoTDB
 *  TDengine
 *  TimescaleDB
 *  PinusDB  
 *  Relational databases are also supported, either single tables or sharding.  


## What EventBus Message Queue  are supported?

 *  RabbitMQ
 *  Kafka 
 *	InMemory 
 *	ZeroMQ 
 *	NATS 
 *	Pulsar 
 *	RedisStreams 
 *	AmazonSQS 
 *	AzureServiceBus 

## What EventBus Store are supported?
* PostgreSql,
* MongoDB,
* InMemory,
* LiteDB,
* MySql,
* SqlServer


## IoTSharp  Online        

 - [IoTSharp front-end implemented using Vue3](https://host.iotsharp.net)


## How to deploy?

- [Deploy by Docker](https://iotsharp.net/docs/tutorial-basics/deploy_by_docker)
- [Deploy to Linux](https://iotsharp.net/docs/tutorial-basics/deploy_linux)
- [Deployed to Windows](https://iotsharp.net/docs/tutorial-basics/deploy_win)

## How to configure?

- [AppSettings](https://iotsharp.net/docs/tutorial-basics/appsettings) 

##  IoTSharp.SDKs

- IoTSharp.Sdk.Http   [![IoTSharp.Sdk.Http](https://img.shields.io/nuget/v/IoTSharp.Sdk.Http.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.Http/)
- IoTSharp.Sdk.MQTT   [![IoTSharp.Sdk.MQTT](https://img.shields.io/nuget/v/IoTSharp.Sdk.MQTT.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.MQTT/)

 

## IoTSharp-C-Client-Sdk

IoTSharp-C-client-Sdk is mqttt client, write by   c;

 - [Github IoTSharp.Sdks.MQTT-C](https://github.com/IoTSharp/IoTSharp.Sdks.MQTT-C)
 - [Gitee IoTSharp.Sdks.MQTT-C](https://gitee.com/IoTSharp/IoTSharp.Sdks.MQTT-C)
 - [GitCode IoTSharp.Sdks.MQTT-C](https://gitcode.com/IoTSharp/IoTSharp.Sdks.MQTT-C)

## paho.mqtt.c's demo 

It' like IoTSharp-C-Client-Sdk, but is use paho.mqtt.c

 - [Github IoTSharp.Edge.paho.mqtt.c](https://github.com/IoTSharp/IoTSharp.Edge.paho.mqtt.c)
 - [Gitee IoTSharp.Edge.paho.mqtt.c]( https://gitee.com/IoTSharp/IoTSharp.Edge.paho.mqtt.c)
 - [GitCode IoTSharp.Edge.paho.mqtt.c]( https://gitcode.com/IoTSharp/IoTSharp.Edge.paho.mqtt.c)


## IoTSharp for nanoFramework

IoTSharp.Edge.nanoFramework is a nanoFramework's mqtt client , it run on STM32 ！

 - [Github IoTSharp.Edge.nanoFramework](https://github.com/IoTSharp/IoTSharp.Edge.nanoFramework)
 - [Gitee IoTSharp.Edge.nanoFramework]( https://gitee.com/IoTSharp/IoTSharp.Edge.nanoFramework)
 - [GitCode IoTSharp.Edge.nanoFramework]( https://gitcode.com/IoTSharp/IoTSharp.Edge.nanoFramework)
 

more info read https://www.cnblogs.com/MysticBoy/p/13159648.html
or click  https://www.nanoframework.net/

## IoTSharp for RTthread Package

 - [Github](https://github.com/IoTSharp/iotsharp-rtthread-package)
 - [Gitee](https://gitee.com/IoTSharp/iotsharp-rtthread-package)
 - [GitCode](https://gitcode.com/IoTSharp/iotsharp-rtthread-package)




## IoTSharp's ecosystem

- SilkierQuartz   [![SilkierQuartz](https://img.shields.io/nuget/v/SilkierQuartz.svg) ![Nuget](https://img.shields.io/nuget/dt/SilkierQuartz)  ](https://www.nuget.org/packages/SilkierQuartz/)
- IoTSharp.Numerics   [![IoTSharp.Numerics](https://img.shields.io/nuget/v/IoTSharp.Numerics.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Numerics)  ](https://www.nuget.org/packages/IoTSharp.Numerics/)
- Extensions.Configuration.GitRepository   [![Extensions.Configuration.GitRepository](https://img.shields.io/nuget/v/Extensions.Configuration.GitRepository.svg) ![Nuget](https://img.shields.io/nuget/dt/Extensions.Configuration.GitRepository)  ](https://www.nuget.org/packages/Extensions.Configuration.GitRepository/)
- IoTSharp.HealthChecks.InfluxDB   [![IoTSharp.HealthChecks.InfluxDB](https://img.shields.io/nuget/v/IoTSharp.HealthChecks.InfluxDB.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.HealthChecks.InfluxDB)  ](https://www.nuget.org/packages/IoTSharp.HealthChecks.InfluxDB/)
- IoTSharp.HealthChecks.Cassandra   [![IoTSharp.HealthChecks.Cassandra](https://img.shields.io/nuget/v/IoTSharp.HealthChecks.Cassandra.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.HealthChecks.Cassandra)  ](https://www.nuget.org/packages/IoTSharp.HealthChecks.Cassandra/)
- IoTSharp.HealthChecks.Taos   [![IoTSharp.HealthChecks.Taos](https://img.shields.io/nuget/v/IoTSharp.HealthChecks.Taos.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.HealthChecks.Taos)  ](https://www.nuget.org/packages/IoTSharp.HealthChecks.Taos/)
- IoTSharp.HealthChecks.IoTDB   [![IoTSharp.HealthChecks.IoTDB](https://img.shields.io/nuget/v/IoTSharp.HealthChecks.IoTDB.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.HealthChecks.IoTDB)  ](https://www.nuget.org/packages/IoTSharp.HealthChecks.IoTDB/)
- MQTTnet.AspNetCore.Routing   [![MQTTnet.AspNetCore.Routing](https://img.shields.io/nuget/v/MQTTnet.AspNetCore.Routing.svg) ![Nuget](https://img.shields.io/nuget/dt/MQTTnet.AspNetCore.Routing)  ](https://www.nuget.org/packages/MQTTnet.AspNetCore.Routing/)
- IoTSharp.EntityFrameworkCore.Taos   [![IoTSharp.EntityFrameworkCore.Taos](https://img.shields.io/nuget/v/IoTSharp.EntityFrameworkCore.Taos.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.EntityFrameworkCore.Taos)  ](https://www.nuget.org/packages/IoTSharp.EntityFrameworkCore.Taos/)
- IoTSharp.Sdk.Http   [![IoTSharp.Sdk.Http](https://img.shields.io/nuget/v/IoTSharp.Sdk.Http.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Sdk.Http) ](https://www.nuget.org/packages/IoTSharp.Sdk.Http/)
- IoTSharp.Sdk.MQTT   [![IoTSharp.Sdk.MQTT](https://img.shields.io/nuget/v/IoTSharp.Sdk.MQTT.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Sdk.MQTT)  ](https://www.nuget.org/packages/IoTSharp.Sdk.MQTT/)
- IoTSharp.X509Extensions [![IoTSharp.X509Extensions](https://img.shields.io/nuget/v/IoTSharp.X509Extensions.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.X509Extensions) ](https://www.nuget.org/packages/IoTSharp.X509Extensions/)
- IoTSharp.Extensions.RESTful  [![IoTSharp.Extensions.RESTful](https://img.shields.io/nuget/v/IoTSharp.Extensions.RESTful.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions.RESTful) ](https://www.nuget.org/packages/IoTSharp.Extensions.RESTful/)
- IoTSharp.Extensions.QuartzJobScheduler  [![IoTSharp.Extensions.QuartzJobScheduler](https://img.shields.io/nuget/v/IoTSharp.Extensions.QuartzJobScheduler.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions.QuartzJobScheduler) ](https://www.nuget.org/packages/IoTSharp.Extensions.QuartzJobScheduler/)
- IoTSharp.Extensions.EFCore  [![IoTSharp.Extensions.EFCore](https://img.shields.io/nuget/v/IoTSharp.Extensions.EFCore.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions.EFCore)  ](https://www.nuget.org/packages/IoTSharp.Extensions.EFCore/)
- IoTSharp.Extensions.BouncyCastle  [![IoTSharp.Extensions.BouncyCastle](https://img.shields.io/nuget/v/IoTSharp.Extensions.BouncyCastle.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions.BouncyCastle) ](https://www.nuget.org/packages/IoTSharp.Extensions.BouncyCastle/)
- IoTSharp.Extensions.AspNetCore  [![IoTSharp.Extensions.AspNetCore](https://img.shields.io/nuget/v/IoTSharp.Extensions.AspNetCore.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions.AspNetCore) ](https://www.nuget.org/packages/IoTSharp.Extensions.AspNetCore/)
- IoTSharp.Extensions  [![IoTSharp.Extensions](https://img.shields.io/nuget/v/IoTSharp.Extensions.svg) ![Nuget](https://img.shields.io/nuget/dt/IoTSharp.Extensions)  ](https://www.nuget.org/packages/IoTSharp.Extensions/)




## Contributing

[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](https://github.com/IoTSharp/IoTSharp/pulls)

If you would like to contribute, feel free to create a [Pull Request](https://github.com/IoTSharp/IoTSharp/pulls), or give us [Bug Report](https://github.com/IoTSharp/IoTSharp/issues/new).

### Contributors

This project exists thanks to all the people who contribute.

<a href="https://github.com/IoTSharp/IoTSharp/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=IoTSharp/IoTSharp" />
</a>

## Star History

[![Star History Chart](https://api.star-history.com/svg?repos=IoTSharp/IoTSharp&type=Date)](https://www.star-history.com/#IoTSharp/IoTSharp&Date)

## Donation

This project is an  Apache 2.0 licensed open source project. In order to achieve better and sustainable development of the project, we expect to gain more backers. We will use the proceeds for community operations and promotion. You can support us in any of the following ways:

- [OpenCollective](https://opencollective.com/IoTSharp)
- [爱发电](https://afdian.net/a/maikebing)
- [捐赠者名单](BACKERS.md)
- 微信![二维码](docs/static/img/maikebing_wxpay.png)


## Community Support

If you encounter any problems in the process, feel free to ask for help via following channels. We also encourage experienced users to help newcomers.

| 公众号 |    [QQ群63631741](https://jq.qq.com/?_wv=1027&k=HJ7h3gbO)  |  微信群  |
| ------ | ---- | ---- |
| ![](docs/static/img/qrcode.jpg) | ![](docs/static/img/IoTSharpQQGruop.png) | ![企业微信群](docs/static/img/qyqun.jpg) |

## dotNET China

[![DotNetChina](https://images.gitee.com/uploads/images/2021/0309/134044_9c191d7b_974299.png)](https://gitee.com/dotnetchina/IoTSharp)

## 优秀开源社区

* [流之云](https://gitee.com/ntdgg) 信息化、数字化服务提供商
* [translate.js](https://gitee.com/mail_osc/translate) 网页自动翻译，页面无需另行改造，加入两行js即可让你的网页快速具备多国语言切换能力！
* [IoTGateway](https://gitee.com/iioter/iotgateway) IoTGateway是一个基于.Net6.0 开源的物联网网关，通过可视化配置，轻松的连接到你的任何设备和物联网平台。
