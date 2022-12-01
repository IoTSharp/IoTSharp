---
sidebar_position: 3
---

# MQTT使用X509加密通讯和设备认证

 IoTSharp支持MQTT协议通过 TLS 1.2 加密通讯， 并可以通过X509证书进行设备认证登录。 

## 配置

在 appsettings.Production.json中需要 指定域名， 并设置EnableTls为true

~~~ json
  "MqttBroker":
  {
	  "DomainName":"http://demo.iotsharp.net:2927/",
	  "EnableTls":"true"
  }
~~~

如果是docker 务必映射目录  ， 下面的例子是将配置文件和 证书目录映射在当前文件夹。 

```
    volumes:
       - "./appsettings.Production.json:/app/appsettings.Production.json"
       - "./data/security:/app/security"
```       

## 自签发根证书和设备证书

用系统管理员账号打开设置->证书， 点击签发 ， 刷新界面， 显示指纹信息后， 说明签发成功。 
新建一个证书， 选择X509 验证方式， 点击下载证书界面， 点击生成证书， 然后下载。 

##  使用mqtt.fx 通过x509证书连接
新建连接， 设置端口为 8883 ， 选择 SSL/TLS ,并选择启用， 选择TLS 版本为 1.2   ， 并选择 Self Sigend certificates , 依次选择根证书ca.crt ， 设备证书 client.crt，以及设备私钥 client.ke , 必须选择 PEM format   点击确认。 连接即可。 如下图所示:

![](/img/iotsharp/mqtt_x509.png)

有关 MQTT如果发送遥测和属性数据 ， 请参考 相关章节。 
