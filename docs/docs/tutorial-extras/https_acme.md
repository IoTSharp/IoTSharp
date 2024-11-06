---
sidebar_position: 2
---

# 使用HTTPS证书并自动续期 

环境变量使用 IOTSHARP_ACME 时 ， 程序会自动使用 LettuceEncrypt  在 Let's Encrypt申请证书， 同时可以使用Dns01通过阿里云来验证DNS.
 
 appsettings.Production.json中的示例如下：

 ``` json
   "LettuceEncrypt": {
    "AcceptTermsOfService": true,
    "AllowedChallengeTypes": "Dns01",
    "EmailAddress": "mysticboy@live.com",
    "RenewDaysInAdvance": "3.00:00:00",
    "DomainNames": [
      "host.iotsharp.net"
    ]
  },
  "AliDns": {
    "AccessKeyId": "",
    "AccessKeySecret": "",
    "DomainName": "iotsharp.net"
  }


 ```

 另外， 环境变量中需要做如下配置:
 ``` 
       ASPNETCORE_HTTPS_PORTS: 8081 
       ASPNETCORE_HTTP_PORTS: 8080
       IOTSHARP_ACME: true

 ```

:::danger 注意
  https证书跟连接设备的自签名证书不同， 此证书用于浏览器、webapi 在公网环境中部署时启用https， 
  设备的自签名X509通讯是用于tcp 加密通讯。 
:::