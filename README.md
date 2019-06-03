# IoTSharp 

[![Build status](https://ci.appveyor.com/api/projects/status/5o23f5vss89ct2lw/branch/master?svg=true)](https://ci.appveyor.com/project/MaiKeBing/iotsharp/branch/master)
![GitHub](https://img.shields.io/github/license/iotsharp/iotsharp.svg)
![GitHub package.json version](https://img.shields.io/github/package-json/v/iotsharp/iotsharp-ui.svg?label=IoTSharp-UI%20Version)

IoTSharp is an open-source IoT platform for data collection, processing, visualization, and device management.


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


![IotSharp Logo](docs/images/iot_sharp_logo.png)

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

