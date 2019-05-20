---
title: Installation
weight: 15
---

## How to install

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

Later we will provide apt-get and rpm