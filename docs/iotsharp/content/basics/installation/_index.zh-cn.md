---
title: 安装
weight: 15
---

## 如何安装:

### Linux  

- mkdir  /var/iotsharp 
- cp ./*  /var/iotsharp/
- chmod 777 IoTSharp
- cp  iotsharp.service   /etc/systemd/system/iotsharp.service
- sudo systemctl enable  /etc/systemd/system/iotsharp.service 
- sudo systemctl start  iotsharp.service 
- sudo journalctl -fu  iotsharp.service 
- http://127.0.0.1:5000/ 

### Windows  

- sc create iotsharp binPath= "D:\iotsharp\IoTSharp.exe" displayname= "IoTSharp"  start= auto

注意: 后期我们将提供  apt-get 和 rpm 安装包 ， 