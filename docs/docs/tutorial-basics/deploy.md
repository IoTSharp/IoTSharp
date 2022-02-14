---
sidebar_position: 4
---

# 如何部署IoTSharp?

本教程使用**rabbit_mongo_influx** 方式进行部署

## 修改docker-compose.yml文件

:::danger 注意
`influxdb_cli`容器的相关配置，通过此命令行容器，初始化influxdb, 初始化之前需要提前设置好token等，如果自己自行配置， 则直接删除此配置即可。 
:::

```yml title="/IoTSharp/Deployments/rabbit_mongo_influx/docker-compose.yml"
influxdb_cli:
    links:
        - influx
    image: quay.io/influxdb/influxdb:v2.0.4
    entrypoint: influx setup --bucket iotsharp-bucket -t iotsharp-token -o iotsharp --username=root --password=1-q2-w3-e4-r5-t --host=http://influx:8086 -f
    restart: on-failure:20
    depends_on:
         - influx
```


## 启动容器

进入`/IoTSharp/Deployments/rabbit_mongo_influx`，执行
```bash docker-compose
docker-compose up -d
```
看到下图说明运行成功
![docker-compose启动成功](/img/iotsharp/docker-run.png)

## 初始化influxdb

浏览器访问 `http://localhost:8086/`，初始化influxdb

![初始化influxdb](/img/iotsharp/influxdb-ini.png)

Org: `iotsharp`  Bucket: `iotsharp-bucket`

然后点`Config Later`

## 创建token
![添加token](/img/iotsharp/influxdb-addtoken.png)

## 复制token
![复制token](/img/iotsharp/influxdb-copytoken.png)

## 修改配置文件

```yml title="/IoTSharp/Deployments/rabbit_mongo_influx/appsettings.Production.json"
"TelemetryStorage":"http://influx:8086/?org=iotsharp&bucket=iotsharp-bucket&token=iotsharp-token&&latest=-72h",
```
将**iotsharp-token**修改为你的真实token


## 重启IoTSharp容器

命令行执行
```bash docker
docker restart iotsharp
```

## 注册

Chrome浏览器访问 `http://localhost:2927/`

![注册](/img/iotsharp/iotsharp-regeist.png)


## 访问
注册后登入进入首页
![访问](/img/iotsharp/iotsharp-dashboard.png)

