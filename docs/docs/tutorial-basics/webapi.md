---
sidebar_position: 5
---

# webapi查询遥测

浏览器访问 `http://localhost:5000/swagger/index.html`


## 登录
![登录](/img/iotsharp/webapi-login.png)
复制返回的access_token

## 认证
![认证](/img/iotsharp/webapi-auth.png)

## 获取设备id
![获取设备id](/img/iotsharp/webapi-deviceid.png)


## 获取设备最新遥测数据
调用`/api/Devices/{deviceId}/TelemetryLatest`

![获取设备最新遥测数据](/img/iotsharp/webapi-telemetry.png)
