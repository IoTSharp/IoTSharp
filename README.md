![IOTSharp LOGO](docs/images/350x100.png)  

[![Build status](https://ci.appveyor.com/api/projects/status/5o23f5vss89ct2lw/branch/master?svg=true)](https://ci.appveyor.com/project/MaiKeBing/iotsharp/branch/master)
![GitHub](https://img.shields.io/github/license/iotsharp/iotsharp.svg)


IoTSharp is an open-source IoT platform for data collection, processing, visualization, and device management.
IoTSharp 中文名称  貔貅物联网平台，用于数据收集通过规则引擎流转数据最后通过数据可视化管理数据结果，以及租户结构的资产管理。可用于实现自主可控的、自有机房、无需支付高额租赁费用的自有物联网平台 

## Contributing
 - If you'd like to contribute to IoTSharp, please take a look at our [Contributing Guide](contributing.md).
 - If you have a question or have found a bug,[ file an issue.](https://github.com/IoTSharp/IoTSharp/issues)
 - To learn about project priorities as well as status and ship dates see the [IoTShap roadmap](roadmap.md).

## Support

 - [Stackoverflow](http://stackoverflow.com/questions/tagged/iotsharp)

## [Document](https://docs.iotsharp.io)

## IoTSharp's Clients and Sdks
 - Cicada    A desktop application  https://github.com/IoTSharp/Cicada
 - C Client SDK for IoTSharp  https://github.com/IoTSharp/IoTSharp-C-Client-Sdk
 - C# Client SDK for IoTSharp  https://github.com/IoTSharp/IoTSharp.SDKs 




## How to install ?
 -  mkdir  /var/lib/iotsharp/
 -	cp ./*  /var/lib/iotsharp/
 -	chmod 777 /var/lib/iotsharp/IoTSharp
 -	cp  iotsharp.service   /etc/systemd/system/iotsharp.service
 -	sudo systemctl enable  /etc/systemd/system/iotsharp.service 
 -	sudo systemctl start  iotsharp.service 
 -	sudo journalctl -fu  iotsharp.service 
 -	http://127.0.0.1:80/ 
 -  

##  IoTSharp.Edge.RT-Thread

IoTSharp.Edge.RT-Thread (STM32L4 + Wi-Fi, sensor, lcd, audio etc)

https://github.com/IoTSharp/IoTSharp.Edge.RT-Thread

![20190615010003.jpg](docs/images/20190615010003.jpg)

![20190615010115.jpg](docs/images/20190615010115.jpg)
