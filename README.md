![IOTSharp LOGO](docs/images/350x100.png)  

[![Build status](https://ci.appveyor.com/api/projects/status/5o23f5vss89ct2lw/branch/master?svg=true)](https://ci.appveyor.com/project/MaiKeBing/iotsharp/branch/master)
![GitHub](https://img.shields.io/github/license/iotsharp/iotsharp.svg)
![.NET Core](https://github.com/IoTSharp/IoTSharp/workflows/.NET%20Core/badge.svg?branch=master)


IoTSharp is an open-source IoT platform for data collection, processing, visualization, and device management.
IoTSharp 中文名称  貔貅物联网平台，用于数据收集通过规则引擎流转数据最后通过数据可视化管理数据结果，以及租户结构的资产管理。可用于实现自主可控的、自有机房、无需支付高额租赁费用的自有物联网平台 


## [Document](https://docs.iotsharp.io)

## IoTSharp's Clients 
 - Cicada    A desktop application   


## How to install with docker-compose  ?

```
 mkdir iotsharp
cd iotsharp 
wget https://raw.githubusercontent.com/IoTSharp/IoTSharp/master/docker-compose.yml
docker-compose up -d  
```

Demo url : http://139.9.232.10:2927


## How to install as docker ?

  -  docker pull iotsharp/iotsharp


## How to install as Linux service  ?

 -  mkdir  /var/lib/iotsharp/
 -	cp ./*  /var/lib/iotsharp/
 -	chmod 777 /var/lib/iotsharp/IoTSharp
 -	cp  iotsharp.service   /etc/systemd/system/iotsharp.service
 -	sudo systemctl enable  /etc/systemd/system/iotsharp.service 
 -	sudo systemctl start  iotsharp.service 
 -	sudo journalctl -fu  iotsharp.service 

# 关联项目

##  IoTSharp.SDKs

IoTSharp.SDKs  包含了 IoTSharp.Sdk.MQTT  IoTSharp.Sdk.Http 用于采集端或者边缘部分进行数据采集通过sdk发送给IoTSharp。 
https://github.com/IoTSharp/IoTSharp.SDKs
 

## IoTSharp-C-Client-Sdk

IoTSharp-C-client-Sdk is mqttt client, it is by   c;

 https://github.com/IoTSharp/IoTSharp-C-Client-Sdk

## paho.mqtt.c's demo 

It' like IoTSharp-C-Client-Sdk, but is use paho.mqtt.c
 https://github.com/IoTSharp/IoTSharp.Edge.paho.mqtt.c

## IoTSharp.Edge.nanoFramework

IoTSharp.Edge.nanoFramework is a nanoFramework's mqtt client , it run on STM32 ！

  https://github.com/IoTSharp/IoTSharp.Edge.nanoFramework

more info read https://www.cnblogs.com/MysticBoy/p/13159648.html
or click  https://www.nanoframework.net/

##  IoTSharp.Edge.RT-Thread

IoTSharp.Edge.RT-Thread (STM32L4 + Wi-Fi, sensor, lcd, audio etc)

https://github.com/IoTSharp/IoTSharp.Edge.RT-Thread




|                                                              |                                                              |
| ------------------------------------------------------------ | ------------------------------------------------------------ |
| <img src="docs/images/20190615010003.jpg" alt="20190615010003.jpg" style="zoom: 67%;" /> | <img src="docs/images/20190615010115.jpg" alt="20190615010115.jpg" style="zoom: 50%;" /> |

依赖生态
 
IoTSharp的依赖生态是为了实现IoTSharp而改进或者创建的项目。 这些项目是组成IoTSharp的关键部分。 与此同时， 针对其他项目也有一定需求。


- MaiKeBing.CAP.ZeroMQ [![MaiKeBing.CAP.ZeroMQ](https://img.shields.io/nuget/v/MaiKeBing.CAP.ZeroMQ.svg)](https://www.nuget.org/packages/MaiKeBing.CAP.ZeroMQ/)

  ZeroMQ（也称为 ØMQ，0MQ 或 zmq）是一个可嵌入的网络通讯库（对 Socket 进行了封装）。 它提供了携带跨越多种传输协议（如：进程内，进程间，TCP 和多播）的原子消息的 sockets 。 有了ZeroMQ，我们可以通过发布-订阅、任务分发、和请求-回复等模式来建立 N-N 的 socket 连接。 ZeroMQ 的异步 I / O 模型为我们提供可扩展的基于异步消息处理任务的多核应用程序。当前组件使用了NetMQ 为CAP提供了 发布-订阅， 推送-拉取两种消息模式。 示例请参见Sample.ZeroMQ.InMemory， 当测试 推送-拉取 消息模式时 ， 可以启动 Sample.ConsoleApp 可以测试负载均衡。

- MaiKeBing.CAP.LiteDB  [![MaiKeBing.CAP.LiteDB](https://img.shields.io/nuget/v/MaiKeBing.CAP.LiteDB.svg)](https://www.nuget.org/packages/MaiKeBing.CAP.LiteDB/)
   
   [LiteDB](http://www.litedb.org/)是一个小型的.NET平台开源的NoSQL类型的轻量级文件数据库。特点是小和快，dll文件只有200K大小，而且支持LINQ和命令行操作，数据库是一个单一文件，类似Sqlite。为CAP存储了本地文件的NoSQL存储方式， 示例请参见 Sample.LiteDB.InMemory
   

- MaiKeBing.HostedService.ZeroMQ  [![MaiKeBing.HostedService.ZeroMQ](https://img.shields.io/nuget/v/MaiKeBing.HostedService.ZeroMQ.svg)](https://www.nuget.org/packages/MaiKeBing.HostedService.ZeroMQ/)

  将ZeroMQ作为HostedService 运行， 通过配置可以实现Pub-Sub、Push-Pull 两种分发模式
 
- IoTSharp.X509Extensions  [![IoTSharp.X509Extensions](https://img.shields.io/nuget/v/IoTSharp.X509Extensions.svg)](https://www.nuget.org/packages/IoTSharp.X509Extensions/)

   We cloned q2g-helper-pem-nuget.removed     Nlog. Added self-signed X509 functions, all of which will be used in iot#, while hope is useful for you. ...

   https://github.com/IoTSharp/IoTSharp.X509Extensions


- MQTTnet.AspNetCoreEx  [![MQTTnet.AspNetCoreEx](https://img.shields.io/nuget/v/MQTTnet.AspNetCoreEx.svg)](https://www.nuget.org/packages/MQTTnet.AspNetCoreEx/)

Makes client connection authentication easier  ! 
 https://github.com/maikebing/MQTTnet.AspNetCoreEx

- Silkier    [![Silkier](https://img.shields.io/nuget/v/Silkier.svg)](https://www.nuget.org/packages/Silkier/) 

   Silkier is a common collection of extensions. For example, retry, partitioning in parallel,ObjectPool，RestClient's extension, LITTLE-ENDIAN and BIG-ENDIAN coversions and more .....


-  Silkier.EFCore   [![Silkier.EFCore](https://img.shields.io/nuget/v/Silkier.EFCore.svg)](https://www.nuget.org/packages/Silkier.EFCore/)

   Silkier.EFCore is an extension for EF.Core, and the main features include executing the original sql statement, converting the original sql statement to a tuple or a class or array or json object or DataTable


- Silkier.AspNetCore  [![Silkier.AspNetCore](https://img.shields.io/nuget/v/Silkier.AspNetCore.svg)](https://www.nuget.org/packages/Silkier.AspNetCore/)

   Silkier.AspNetCore have ConfigureWindowsServices and UseJsonToSettings and more ...

  https://github.com/maikebing/Silkier

- SilkierQuartz   [![SilkierQuartz](https://img.shields.io/nuget/v/SilkierQuartz.svg)](https://www.nuget.org/packages/SilkierQuartz/)

   SilkierQuartz can be used within your existing application with minimum effort as a Quartz.NET plugin when it automatically creates embedded web server. Or it can be plugged into your existing OWIN-based web application as a middleware.

https://github.com/maikebing/SilkierQuartz

- Maikebing.EntityFrameworkCore.Taos   [![Maikebing.EntityFrameworkCore.Taos](https://img.shields.io/nuget/v/Maikebing.EntityFrameworkCore.Taos.svg)](https://www.nuget.org/packages/Maikebing.EntityFrameworkCore.Taos/)

   Entity, Framework, EF, Core, Data, O/RM, entity-framework-core,TDengine

   https://github.com/maikebing/Maikebing.EntityFrameworkCore.Taos



## Support

| 公众号 |    QQ群  |
| ------ | ---- |
| ![](docs/images/qrcode.jpg) | ![](docs/images/IoTSharpQQGruop.png) |

## Contributing
 - If you'd like to contribute to IoTSharp, please take a look at our [Contributing Guide](contributing.md).
 - If you have a question or have found a bug,[ file an issue.](https://github.com/IoTSharp/IoTSharp/issues)
 - To learn about project priorities as well as status and ship dates see the [IoTShap roadmap](roadmap.md).

