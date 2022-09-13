---
sidebar_position: 4
---

# 在Linux中部署IoTSharp

本教程使用Sqlite 为数据存储 方式进行部署

# 下载

首先在 https://github.com/IoTSharp/IoTSharp/releases 或者 https://gitee.com/IoTSharp/IoTSharp/releases 中下载最新版本的安装包， 常用系统中压缩包选择 [IoTSharp.Release.linux-x64.zip](https://github.com/IoTSharp/IoTSharp/releases/download/v2.8/IoTSharp.Release.linux-x64.zip)  , 如果是树莓派版本则下载 [IoTSharp.Release.linux-arm64.zip](https://github.com/IoTSharp/IoTSharp/releases/download/v2.8/IoTSharp.Release.linux-arm64.zip)   至本地。 

# 直接启动

解压压缩包后， 我们可以看到里面 有一个 IoTSharp 文件， 使用 chmod 777 IoTSharp ， 然后 在命令行使用 ./IoTSharp 即可。 启动后， 即可在浏览器中打开 http://localhost:2927 来访问。 

# 注册为服务

IoTSharp 已经支持了Linux 服务的方式运行， 按照下面的步骤可以将IoTSharp 注册为LInux daemon  

 -  mkdir  /var/lib/iotsharp/   # 创建运行目录 
 -  cp ./*  /var/lib/iotsharp/   # 将所有文件拷贝至目标目录
 -  chmod 777 /var/lib/iotsharp/IoTSharp  # 设置IoTSharp 的可执行权限
 -  cp  iotsharp.service   /etc/systemd/system/iotsharp.service  # 将服务文件拷贝至系统
 -  sudo systemctl enable  /etc/systemd/system/iotsharp.service   # 启用服务
 -  sudo systemctl start  iotsharp.service   # 启动此服务
 -  sudo journalctl -fu  iotsharp.service   # 查看该服务日志 



## 注册

Chrome浏览器访问 `http://localhost:2927/`

![注册](/img/iotsharp/iotsharp-regeist.png)


## 访问
注册后登入进入首页
![访问](/img/iotsharp/iotsharp-dashboard.png)

