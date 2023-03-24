<p align="left">
  <a href="https://iotsharp.io/">
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


## IoTSharp Demo       

 - [IoTSharp front-end implemented using Vue3](http://demo.iotsharp.net)


##  IoTSharp cloud  
  https://cloud.iotsharp.net/

## doc
  https://docs.iotsharp.net/

## How to deploy?

- [Deploy by Docker](https://docs.iotsharp.net/docs/tutorial-basics/deploy_by_docker)
- [Deploy to Linux](https://docs.iotsharp.net/docs/tutorial-basics/deploy_linux)
- [Deployed to Windows](https://docs.iotsharp.net/docs/tutorial-basics/deploy_win)

## How to configure?

- [AppSettings](https://docs.iotsharp.net/docs/tutorial-basics/appsettings) 

##  IoTSharp.SDKs

- IoTSharp.Sdk.Http   [![IoTSharp.Sdk.Http](https://img.shields.io/nuget/v/IoTSharp.Sdk.Http.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.Http/)
- IoTSharp.Sdk.MQTT   [![IoTSharp.Sdk.MQTT](https://img.shields.io/nuget/v/IoTSharp.Sdk.MQTT.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.MQTT/)

 

## IoTSharp-C-Client-Sdk

IoTSharp-C-client-Sdk is mqttt client, write by   c;

 [https://github.com/IoTSharp/IoTSharp.Sdks.MQTT-C](https://github.com/IoTSharp/IoTSharp.Sdks.MQTT-C)

## paho.mqtt.c's demo 

It' like IoTSharp-C-Client-Sdk, but is use paho.mqtt.c
 https://github.com/IoTSharp/IoTSharp.Edge.paho.mqtt.c

## IoTSharp for nanoFramework

IoTSharp.Edge.nanoFramework is a nanoFramework's mqtt client , it run on STM32 ！

  https://github.com/IoTSharp/IoTSharp.Edge.nanoFramework

more info read https://www.cnblogs.com/MysticBoy/p/13159648.html
or click  https://www.nanoframework.net/

## IoTSharp for RTthread Package

https://github.com/IoTSharp/iotsharp-rtthread-package



## IoTSharp's ecosystem

- MaiKeBing.CAP.ZeroMQ [![MaiKeBing.CAP.ZeroMQ](https://img.shields.io/nuget/v/MaiKeBing.CAP.ZeroMQ.svg)](https://www.nuget.org/packages/MaiKeBing.CAP.ZeroMQ/)
- MaiKeBing.CAP.LiteDB  [![MaiKeBing.CAP.LiteDB](https://img.shields.io/nuget/v/MaiKeBing.CAP.LiteDB.svg)](https://www.nuget.org/packages/MaiKeBing.CAP.LiteDB/)
- MaiKeBing.HostedService.ZeroMQ  [![MaiKeBing.HostedService.ZeroMQ](https://img.shields.io/nuget/v/MaiKeBing.HostedService.ZeroMQ.svg)](https://www.nuget.org/packages/MaiKeBing.HostedService.ZeroMQ/)
- IoTSharp.X509Extensions  [![IoTSharp.X509Extensions](https://img.shields.io/nuget/v/IoTSharp.X509Extensions.svg)](https://www.nuget.org/packages/IoTSharp.X509Extensions/)
- IoTSharp.EntityFrameworkCore.Taos   [![IoTSharp.EntityFrameworkCore.Taos](https://img.shields.io/nuget/v/IoTSharp.EntityFrameworkCore.Taos.svg)](https://www.nuget.org/packages/IoTSharp.EntityFrameworkCore.Taos/)
- IoTSharp.Sdk.Http   [![IoTSharp.Sdk.Http](https://img.shields.io/nuget/v/IoTSharp.Sdk.Http.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.Http/)
- IoTSharp.Sdk.MQTT   [![IoTSharp.Sdk.MQTT](https://img.shields.io/nuget/v/IoTSharp.Sdk.MQTT.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.MQTT/)

## Contributing

[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg?style=flat-square)](https://github.com/IoTSharp/IoTSharp/pulls)

If you would like to contribute, feel free to create a [Pull Request](https://github.com/IoTSharp/IoTSharp/pulls), or give us [Bug Report](https://github.com/IoTSharp/IoTSharp/issues/new).

### Contributors

This project exists thanks to all the people who contribute.

<a href="https://github.com/IoTSharp/IoTSharp/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=IoTSharp/IoTSharp" />
</a>

## Donation

This project is an  Apache 2.0 licensed open source project. In order to achieve better and sustainable development of the project, we expect to gain more backers. We will use the proceeds for community operations and promotion. You can support us in any of the following ways:

- [OpenCollective](https://opencollective.com/IoTSharp)
- 微信![二维码](docs/static/img/maikebing_wxpay.png)

We will put the detailed donation records on the below!


|                                                        | Name                                  | Stars | Donations | Message |
| ------------------------------------------------------------ | ------------------ | -------- | -------- | -------- |
| [![@iioter](https://avatars.githubusercontent.com/u/29589505?s=80&v=4)](https://github.com/iioter) | whd | ![GitHub User's stars](https://img.shields.io/github/stars/iioter?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge) | ￥1024 =120+100+292+512(码云共计四次) |  |
| [![@nnhy](https://avatars.githubusercontent.com/u/506367?s=80&v=4)](https://github.com/nnhy) | 大石头 | ![GitHub User's stars](https://img.shields.io/github/stars/nnhy?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge) |     ￥672=666+5（码云+公众号）     |            |
|  | 无敌飞行家 | ![GitHub User's stars](https://img.shields.io/github/stars/hehaoyu_2014?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge) | ￥5=5(公众号) |  |
| [![@davidzhu001](https://avatars.githubusercontent.com/u/9436230?s=80&v=4)](https://github.com/davidzhu001)   | 农民也很疯狂 |  ![GitHub User's stars](https://img.shields.io/github/stars/davidzhu001?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge)| ￥400=200+200 微信转账 | |
| [![@280780363](https://avatars.githubusercontent.com/u/20083278?s=80&v=4)](https://github.com/280780363)   | 谷草 |  ![GitHub User's stars](https://img.shields.io/github/stars/280780363?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge)| ￥88 微信转账 | |
| [![@Jackson-918](https://avatars.githubusercontent.com/u/44353254?s=80&v=4)](https://github.com/Jackson-918)   | Jackson-918 |  ![GitHub User's stars](https://img.shields.io/github/stars/Jackson-918?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge)| ￥60 微信转账 | |
| [![@OweQian](https://avatars.githubusercontent.com/u/25632596?s=80&v=4)](https://github.com/OweQian)   | OweQian |  ![GitHub User's stars](https://img.shields.io/github/stars/OweQian?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge)| ￥100 微信转账 | |
| [![@uncle-stone](https://foruda.gitee.com/avatar/1670051844286871132/2372506_iotsharp_1670051843.png!avatar100)](https://gitee.com/uncle-stone)   | 石头叔叔 |  | ￥99 微信转账 | |
|  | 匿名公司 |  | ￥1000=1000(微信转账) |  |
|  | *阵 |  | ￥50(微信转账) |  |
|  | *守 |  | ￥10(微信转账) | 加油iotsharp! |
| [![@24008550](https://avatars.githubusercontent.com/u/2004038?s=80&v=4)](https://github.com/24008550) | 马超 | ![GitHub User's stars](https://img.shields.io/github/stars/24008550?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge) | ￥200（微信红包） |  |
| [![@joyinan](https://foruda.gitee.com/avatar/1676895845500193295/12369_joyinan_1578914838.png!avatar200)](https://gitee.com/joyinan) | joyinan | ![Gitee User's stars](https://img.shields.io/gitee/stars/joyinan?affiliations=OWNER%2CCOLLABORATOR%2CORGANIZATION_MEMBER&style=for-the-badge) | ￥100（微信红包） |  |




## Community Support

If you encounter any problems in the process, feel free to ask for help via following channels. We also encourage experienced users to help newcomers.

| 公众号 |    [QQ群63631741](https://jq.qq.com/?_wv=1027&k=HJ7h3gbO)  |  微信群  |
| ------ | ---- | ---- |
| ![](docs/static/img/qrcode.jpg) | ![](docs/static/img/IoTSharpQQGruop.png) | ![企业微信群](docs/static/img/qyqun.jpg) |

## dotNET China

[![DotNetChina](https://images.gitee.com/uploads/images/2021/0309/134044_9c191d7b_974299.png)](https://gitee.com/dotnetchina/IoTSharp)

## 优秀开源社区
* [LinkWeChat](https://gitee.com/LinkWeChat/link-wechat) LinkWeChat 是基于企业微信的开源 SCRM 系统，是企业私域流量管理与营销的综合解决方案。
* [IoTSharp](https://gitee.com/IoTSharp) IoTSharp 是一个 基于.Net Core 开源的物联网基础平台， 支持 HTTP、MQTT 、CoAp 协议
* [流之云](https://gitee.com/ntdgg) 信息化、数字化服务提供商
* [translate.js](https://gitee.com/mail_osc/translate) 网页自动翻译，页面无需另行改造，加入两行js即可让你的网页快速具备多国语言切换能力！
* [IoTGateway](https://gitee.com/iioter/iotgateway) IoTGateway是一个基于.Net6.0 开源的物联网网关，通过可视化配置，轻松的连接到你的任何设备和物联网平台。
