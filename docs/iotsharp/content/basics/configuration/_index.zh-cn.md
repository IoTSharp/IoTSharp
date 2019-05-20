---
date: 2016-04-09T16:50:16+02:00
title: 配置
weight: 20
---

appsettings.json 配置说明

 

数据库部分:

DataBase 用于指定目前应用使用ConnectionStrings中的哪个连接字符串，原则上我们支持 sqlite  sqlserver 和 pgsql ， 但我们推荐pgsql



  "DataBase": "npgsql", 
  "ConnectionStrings": {
    "mssql": "Server=localhost;Database=IoTSharp;Trusted_Connection=True;MultipleActiveResultSets=true",
    "npgsql": "Server=localhost;Database=IoTSharp;Username=postgres;Password=future;",
    "sqlite": "Data Source=:memory:"
  } 

JWT 的key 设置 和过期时间设置

  "JwtKey": "kissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissme",
  "JwtExpireDays": 1,
  "JwtIssuer": "IoTSharp.Hub",
  "AllowedHosts": "*",



健康检查页面设置， 目前尚未开发完成

  "HealthChecks-UI": {
    "HealthChecks": [
      {
        "Name": "HTTP-Api-Basic",
        "Uri": "http://localhost:5000/health"
      }
    ],
    "Webhooks": [
      {
        "Name": "",
        "Uri": "",
        "Payload": "",
        "RestoredPayload": ""
      }
    ],
    "EvaluationTimeOnSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}