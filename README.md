# IoTSharp 

[![Build status](https://ci.appveyor.com/api/projects/status/lfqpc9lrt99ar74h?svg=true)](https://ci.appveyor.com/project/MaiKeBing/iotsharp)

IoTSharp is an open-source IoT platform for data collection, processing, visualization, and device management.

![IotSharp Logo](doc/img/logo.png)

## Contributing
 - If you'd like to contribute to IoTSharp, please take a look at our [Contributing Guide](contributing.md).
 - If you have a question or have found a bug,[ file an issue.](https://github.com/IoTSharp/IoTSharp/issues)
 - To learn about project priorities as well as status and ship dates see the [IoTShap roadmap](roadmap.md).

## Support

 - [Stackoverflow](http://stackoverflow.com/questions/tagged/iotsharp)

## Documentation

## How to install

### Linux  
 -  mkdir  /var/iotsharp 
 -	cp ./*  /var/iotsharp/
 -	chmod 777 IoTSharp.Hub
 -	cp  iotsharp.service   /etc/systemd/system/iotsharp.service
 -	sudo systemctl enable  /etc/systemd/system/iotsharp.service 
 -	sudo systemctl start  iotsharp.service 
 -	sudo journalctl -fu  iotsharp.service 
 -	http://127.0.0.1:5000/ 

### Windows  
 - sc create iotsharp binPath= "D:\mqttchat\IoTSharp.Hub.exe" displayname= "IoTSharp"  start= auto