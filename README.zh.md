![IOTSharp LOGO](docs/images/350x100.png)  

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

IoTSharp.SDKs  包括了 IoTSharp.Sdk.MQTT  IoTSharp.Sdk.Http  

 

## IoTSharp-C-Client-Sdk

IoTSharp-C-client-Sdk 是mqtt客户端， c语言编写的例子。 

 https://github.com/IoTSharp/IoTSharp-C-Client-Sdk

## paho.mqtt.c 的例子

这个跟 IoTSharp-C-Client-Sdk 一样， 但是使用了 paho.mqtt.c
 https://github.com/IoTSharp/IoTSharp.Edge.paho.mqtt.c

## IoTSharp.Edge.nanoFramework

IoTSharp.Edge.nanoFramework 是一个 nanoFramework's mqtt 客户端， 它允许在STM32 ！

  https://github.com/IoTSharp/IoTSharp.Edge.nanoFramework

更多信息请读这里 https://www.cnblogs.com/MysticBoy/p/13159648.html
官方网站为：  https://www.nanoframework.net/ 

##  IoTSharp.Edge.RT-Thread

IoTSharp.Edge.RT-Thread (STM32L4 + Wi-Fi, sensor, lcd, audio etc) 是一个国产实时操作系统RT-Thread的示例， 同时我们提供了两个图片， 供你参考。 项目链接如下 https://github.com/IoTSharp/IoTSharp.Edge.RT-Thread

|                                                              |                                                              |
| ------------------------------------------------------------ | ------------------------------------------------------------ |
| <img src="docs/images/20190615010003.jpg" alt="20190615010003.jpg" style="zoom: 67%;" /> | <img src="docs/images/20190615010115.jpg" alt="20190615010115.jpg" style="zoom: 50%;" /> |

IoTSharp 的软件生态

- MaiKeBing.CAP.ZeroMQ [![MaiKeBing.CAP.ZeroMQ](https://img.shields.io/nuget/v/MaiKeBing.CAP.ZeroMQ.svg)](https://www.nuget.org/packages/MaiKeBing.CAP.ZeroMQ/)
- MaiKeBing.CAP.LiteDB  [![MaiKeBing.CAP.LiteDB](https://img.shields.io/nuget/v/MaiKeBing.CAP.LiteDB.svg)](https://www.nuget.org/packages/MaiKeBing.CAP.LiteDB/)
- MaiKeBing.HostedService.ZeroMQ  [![MaiKeBing.HostedService.ZeroMQ](https://img.shields.io/nuget/v/MaiKeBing.HostedService.ZeroMQ.svg)](https://www.nuget.org/packages/MaiKeBing.HostedService.ZeroMQ/)
- IoTSharp.X509Extensions  [![IoTSharp.X509Extensions](https://img.shields.io/nuget/v/IoTSharp.X509Extensions.svg)](https://www.nuget.org/packages/IoTSharp.X509Extensions/)
- MQTTnet.AspNetCoreEx  [![MQTTnet.AspNetCoreEx](https://img.shields.io/nuget/v/MQTTnet.AspNetCoreEx.svg)](https://www.nuget.org/packages/MQTTnet.AspNetCoreEx/)
- Silkier    [![Silkier](https://img.shields.io/nuget/v/Silkier.svg)](https://www.nuget.org/packages/Silkier/) 
- Silkier.EFCore   [![Silkier.EFCore](https://img.shields.io/nuget/v/Silkier.EFCore.svg)](https://www.nuget.org/packages/Silkier.EFCore/)
- Silkier.AspNetCore  [![Silkier.AspNetCore](https://img.shields.io/nuget/v/Silkier.AspNetCore.svg)](https://www.nuget.org/packages/Silkier.AspNetCore/)
- SilkierQuartz   [![SilkierQuartz](https://img.shields.io/nuget/v/SilkierQuartz.svg)](https://www.nuget.org/packages/SilkierQuartz/)
- Maikebing.EntityFrameworkCore.Taos   [![Maikebing.EntityFrameworkCore.Taos](https://img.shields.io/nuget/v/Maikebing.EntityFrameworkCore.Taos.svg)](https://www.nuget.org/packages/Maikebing.EntityFrameworkCore.Taos/)
- IoTSharp.Sdk.Http   [![IoTSharp.Sdk.Http](https://img.shields.io/nuget/v/IoTSharp.Sdk.Http.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.Http/)
- IoTSharp.Sdk.MQTT   [![IoTSharp.Sdk.MQTT](https://img.shields.io/nuget/v/IoTSharp.Sdk.MQTT.svg)](https://www.nuget.org/packages/IoTSharp.Sdk.MQTT/)

## Support

| 公众号 |    [QQ群63631741](https://jq.qq.com/?_wv=1027&k=HJ7h3gbO)  |
| ------ | ---- |
| ![](docs/images/qrcode.jpg) | ![](docs/images/IoTSharpQQGruop.png) |

## dotNET China

[![DotNetChina](https://images.gitee.com/uploads/images/2021/0309/134044_9c191d7b_974299.png)](https://gitee.com/dotnetchina/IoTSharp)

## Contributing

 - If you'd like to contribute to IoTSharp, please take a look at our [Contributing Guide](contributing.md).
 - If you have a question or have found a bug,[ file an issue.](https://github.com/IoTSharp/IoTSharp/issues)
 - To learn about project priorities as well as status and ship dates see the [IoTShap roadmap](roadmap.md).

