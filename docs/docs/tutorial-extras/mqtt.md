---
sidebar_position: 1
---

#  MQTT协议

本文章介绍如何通过MQTT进行遥测和属性数据上传以及RPC控制的实现。 

##  发送属性和遥测数据

设备分直连设备和网关设备， 发送数据方式也有所不同。 telemetry 为 遥测， attributes表示属性，客户端上传的属性在服务器端永远为 ClientSide ， 即客户端侧属性。 不推荐在服务端修改。 

### 直连设备发送遥测数据

``` 
devices/me/telemetry
devices/me/attributes
```
 
### 网关设备发送遥测数据
```
devices/{设备名称}/telemetry
devices/{设备名称}/attributes
```
 {设备名称} 则是隶属当前网关设备处理的设备名称。 
 MQTT的负载为如下格式

 ```json
 {
   "stringvalue":"here is strvalue",
   "intvalue":234
 }
 ```


### 网关设备批量发送设备遥测数据和属性数据

``` 
gateway/telemetry
gateway/attributes
```
批量发送数据格式如下：
```json
{
    "subdevice1": [
        {
            "ts": 637834108219892435,
            "devicestatus": 0,
            "values": {
                "string": "this string",
                "float": 22.222
            }
        }
    ],
    "subdevice2": [
        {
            "ts": 637834108219892435,
            "devicestatus": 0,
            "values": {
                "intvalue": 22
            }
        }
    ]
}

```
这里是一个C#合成批量上传的例子
```cs
 Dictionary<string, List<Playload>> pairs = new Dictionary<string, List<Playload>>();
            var plst = new List<Playload>();
            var values = new Dictionary<string, object>();
            values.Add("string", "this string");
            values.Add("intvalue",22);
            plst.Add(new Playload() { DeviceStatus = DeviceStatus.Good, Ticks = DateTime.Now.Ticks, Values = values });
            values.Add("float", 22.222);
            plst.Add(new Playload() { DeviceStatus = DeviceStatus.Bad, Ticks = DateTime.Now.Ticks, Values = values });
            pairs.Add("subdevice1",plst);
            pairs.Add("subdevice2", plst);
         var str=    Newtonsoft.Json.JsonConvert.SerializeObject(pairs);
            Console.WriteLine(str); 
```

 
 ##  订阅属性

 ###  直连设备请求属性

发布请求
```
  devices/me/attributes/request/{请求唯一标识}
```
订阅结果
```
 devices/me/attributes/response/{请求唯一标识}
```


 ###  网关设备请求属性

发布请求
```
  devices/{设备名称}/attributes/request/{请求唯一标识}
```
订阅结果
```
 devices/{设备名称}/attributes/response/{请求唯一标识}
```

## 发起上行RPC远程控制

上行RPC控制是指 终端设备远程调用服务端的内容， IoTSharp 收到此请求则调用规则链， 你可以在规则链中处理此请求， 关于规则链请查看相关章节。 

```
  devices/{设备名称}/rpc/request/{方法名称}
```

设备名称这里如果是直连设备， 则为me， 如果是网关设备， 则是设备名称， {方法名称}用于区别调用了何种方法， 这里交由规则链处理。 通过规则链， 你可以调用 内部服务， 也可以调用外部服务等， 发挥你的想象就好。 


##  发起下行RPC远程控制

下行RPC是指平台端或者第三方服务调用IoTSharp进行远程控制终端设备的方法。 

通过MQTT发起时， 先订阅response， 然后发布request。 
```
 devices/{设备名称}/rpc/request/{方法名称}/{请求唯一标识}
 devices/{设备名称}/rpc/response/{方法名称}/{请求唯一标识}
```

第三方服务调用时我们建议通过通过 Web Api 发起， 示例如下

```sh
curl -X 'POST' \
  'https://cloud.iotsharp.net/api/Devices/{设备的TOKEN}/Rpc/{方法名称}?timeout={超时时间}' \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '"{参数， 可以是json}"'

```