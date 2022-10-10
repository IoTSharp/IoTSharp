---
sidebar_position: 8
---

# 设备连通性

IoTSharp设备状态服务负责监视设备连接状态并触发推送到规则链的设备连接事件。 你可以设置对应规则链来处理这些事件。 

支持事件如下:

- Connect   -  设备连接到IoTSharp时触发。 
- Disconnect   - 设备断开IoTSharp时触发
- Activity   -   在设备的超时时间范围内触发一次，如果正好有数据的话就会触发
- Inactivity  - 当超过设备的超时时间范围后，如果依然没数据， 就会触发。 

设备状态服务负责维护以下服务端属性:

- Active - 表示当前设备状态为True或False;
- LastConnectDateTime - 表示设备最后一次连接到IoTSharp的时间
- LastDisconnectDateTime - 表示设备与IoTSharp断开连接的最后时间
- LastActivityDateTime - 表示设备上次推送遥测属性更新或rpc命令的时间
- InactivityAlarmDateTime - 表示设备在超过设备超时时间范围后依然没有收到数据的最后时间。 
