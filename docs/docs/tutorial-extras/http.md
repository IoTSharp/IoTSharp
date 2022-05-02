---
sidebar_position: 2
---

# Http协议

 IoTSharp支持Http协议上传遥测数据， 可以通过标准接口， 也可以通过在属性里设置 映射方式 接收数据， 也可以通过规则链进行解析数据然后再推送的指定数据。 

##  IoTSharp直连设备和常规网关设备的数据上传



##  PushDataToMap
PushDataToMap  是个特定网关接口，通过设置网关的属性值让接口自动解析下面的数据， 如果是文本包含字符串json也可以通过配置进行处理。 
http 请求 
```shell
curl --location --request POST  'http://iot.qhse.cn:2927/api/Devices/{网关Token}/PushDataToMap/json' \
--header 'Content-Type: application/json' \
--data-raw '{
  "dev_id": "2021082640",
  "volt": 0,
  "sample_time": "2022-03-17 15:21:47",
  "datas": [
    {
      "point_name": "2",
      "mon_type": 4,
      "dev_type": 10,
      "depth": 0,
      "data1": 0
    }
  ]
}'
```

样例数据 如下：
```json
{"dataType":"1","dataJson":"{\"serialNumber\":\"12003378\",\"uploadDate\":\"2022-03-20 22:22:19\",\"pm25\":21,\"pm10\":26,\"windSpeed\":25,\"windDirection\":14,\"noise\":46,\"temperature\":60,\"humidity\":900,\"sprayStatus\":0,\"warnReason\":64}"}
```
 
 属性配置参考
 1. _map_to_attribute_{映射到子设备的属性名称}   属性值为在当前json中的路径。 
 例如: 
 ```
_map_to_attribute_currentAllowWeight	currentAllowWeight	 
_map_to_attribute_driverCardNo	driverCardNo	 
_map_to_attribute_version	version	
``` 

2. _map_to_devname  指定设备的名称使用哪个字段 
3. _map_to_devname_format 指定设备名称的格式，可用的变量有 $devname  和 $subdevname， 格式内容为	hf_crane_$devname 
4. _map_to_jsontext_in_json	指定如果是文本类型的将自动将字符串转为json，示例中为： dataJson	 
5. _map_to_telemetry_{映射到子设备中的遥测名称}	   
```
_map_to_telemetry_windSpeed	windSpeed	 
```
 6. _map_to_subdevname 二级关联
 7. _map_to_data_in_array 如果子设备数据在某个数组占用， 则这里指定。 

 注意， 当_map_to_data_in_array 指定了字段时 ， 使用  '@' 的字段指定的是 根json路径， 不使用 则表示当前数组元素中的路径。 
 _map_to_data_in_array和_map_to_jsontext_in_json只能使用其中一种， 不能重合。 




