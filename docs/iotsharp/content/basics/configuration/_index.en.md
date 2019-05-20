---
date: 2016-04-09T16:50:16+02:00
title: Configuration
weight: 20
---

appsettings.json Configuration Instructions

 

DataBase 

DataBase is used to specify which connection string in ConnectionStrings is currently used by the app, and in principle we support SQLite SQL Server and Pgsql, but we recommend Pgsql



  "DataBase": "npgsql", 
  "ConnectionStrings": {
    "mssql": "Server=localhost;Database=IoTSharp;Trusted_Connection=True;MultipleActiveResultSets=true",
    "npgsql": "Server=localhost;Database=IoTSharp;Username=postgres;Password=future;",
    "sqlite": "Data Source=:memory:"
  } 

Key settings and Expiration time settings for JWT

  "JwtKey": "kissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissmekissme",
  "JwtExpireDays": 1,
  "JwtIssuer": "IoTSharp.Hub",
  "AllowedHosts": "*",



Health check settings, which have not yet been developed

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