using HslCommunication;
using IoT.Things.ModBus.Models;
using IoTSharp.EdgeSdk.MQTT;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IoT.Things.ModBus.Jobs
{
    [DisallowConcurrentExecution]
    public class Slaver : IJob
    {
        private readonly MQTTClient _mqtt;
        private ILogger _logger;
        private readonly AppSettings appSettings;
        private const ushort _MAXREADLEN = 32;

        public Slaver(MQTTClient mqtt, IOptions<AppSettings> options, ILogger<Slaver> logger)
        {
            _mqtt = mqtt;
            _logger = logger;
            appSettings = options.Value;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var _modbusdata = context.Trigger.JobDataMap.GetString(nameof(ModBusConfig));
            if (!string.IsNullOrEmpty(_modbusdata))
            {

                var modbusconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, ModBusConfig>>(_modbusdata);
                foreach (var kvmb in modbusconfig)
                {
                    var keyname = kvmb.Key;
                    var mbconfig = kvmb.Value;
                    var _modbusuri = mbconfig.ModBusUri;
                    try
                    {
                       
                        _logger.LogInformation($"{keyname}  {mbconfig.ModBusUri}");
                        var ok = Task.Run(async () =>
                        {
                            try
                            {
                                JobResult result = new JobResult() { Code = 0, Message = "OK" };
                                string keynamejob = context.JobDetail.Key.Name;
                                var _modbus = new HslCommunication.ModBus.ModbusTcpNet(_modbusuri.Host, _modbusuri.Port, byte.Parse(_modbusuri.AbsolutePath.Trim('/','\\')));
                                {
                                    _modbus.UseSynchronousNet = true;
                                    var info = _modbus.ConnectServer();
                                    if (!info.IsSuccess) info = _modbus.ConnectServer();
                                    if (!info.IsSuccess) info = _modbus.ConnectServer();
                                    if (info.IsSuccess)
                                    {
                                        var _datatype = mbconfig.ValueType ?? "UInt16";
                                        var readmethod =  _modbus.GetType().GetMethod($"Read{_datatype}Async",new Type[] { typeof(string),typeof(ushort)});
                                     
                                        ushort address = ushort.Parse(mbconfig.Address);
                                        var length = mbconfig.Lenght+ address;
                                        for (ushort i = address; i < length; i += _MAXREADLEN)
                                        {
                                            ushort tmpmax = (ushort)(i + _MAXREADLEN > length ? length - i : _MAXREADLEN);
                                            var dataresult = await   (dynamic)readmethod.Invoke(_modbus, new object[] { i.ToString(), tmpmax });
                                            if (!dataresult.IsSuccess) dataresult = await (Task<OperateResult<object[]>>)readmethod.Invoke(_modbus, new object[] { i.ToString(), tmpmax });
                                            if (!dataresult.IsSuccess) dataresult = await (Task<OperateResult<object[]>>)readmethod.Invoke(_modbus, new object[] { i.ToString(), tmpmax });
                                            _logger.LogInformation($"JobId: {keynamejob}  ReadRanage:{i}->{tmpmax + i})  Max:{length} Data: {Newtonsoft.Json.JsonConvert.SerializeObject(dataresult)}");
                                            if (dataresult.IsSuccess && dataresult.Content != null && dataresult.Content.Length > 0)
                                            {
                                                Dictionary<string, object> keys = new Dictionary<string, object>();
                                                for (int xxx = 0; xxx < dataresult.Content.Length; xxx++)
                                                {
                                                    keys.Add($"{mbconfig.KeyNameOrPrefix}{xxx + i}", dataresult.Content[xxx]);
                                                }
                                                if (mbconfig.DataType.StartsWith("attribute"))
                                                {
                                                    await _mqtt.UploadAttributeAsync(keys);
                                                }
                                                else
                                                {
                                                    await _mqtt.UploadTelemetryDataAsync(keys);
                                                }
                                                var resultxx1 = 0;
                                                if (resultxx1 == 0)
                                                {
                                                    result.Code = 0;
                                                    result.Message = "数据推送完成";
                                                    _logger.LogInformation($"{keynamejob}执行完成...");
                                                }
                                                else
                                                {
                                                    result.Code = -8;
                                                    result.Message = $"{keynamejob}有{resultxx1}个数据处理失败";
                                                    _logger.LogInformation(result.Message);
                                                }
                                            }
                                            else
                                            {
                                                await _mqtt.UploadTelemetryDataAsync(new { ReadInt16_IsSuccess = info.IsSuccess, ReadInt16_Message = info.Message, ReadInt16_ErrorCode = info.ErrorCode, ReadInt16_Count = tmpmax });
                                                if (dataresult != null)
                                                {
                                                    string mesg = $"服务器 {keynamejob}数据读取失败,{ Newtonsoft.Json.JsonConvert.SerializeObject(dataresult)}";
                                                    _logger.LogInformation(mesg);
                                                    result.Code = -7;
                                                    result.Message = mesg;
                                                }
                                                else
                                                {
                                                    result.Code = -6;
                                                    result.Message = $"服务器 {keynamejob}数据读取失败,结果为空 ";
                                                    _logger.LogInformation(result.Message);
                                                }
                                            }
                                        }
                                        _modbus.ConnectClose();
                                    }
                                    else
                                    {
                                        result.Code = -5;
                                        result.Message = $"服务器 {keynamejob}服务器 ";
                                        if (info != null) _logger.LogInformation($"服务器{keynamejob}连接失败:{  Newtonsoft.Json.JsonConvert.SerializeObject(info)}");
                                    }
                                }
                                if (result.Code == 0)
                                {
                                    string msg = $" {keyname}执行结果:{ result.Code}-{result.Message}";
                                    _logger.LogInformation(msg);
                                }
                                else
                                {
                                    string msg = $"{keyname}数据处理异常:{result.Code}-{result.Message} ";
                                    _logger.LogInformation(msg);
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogInformation($" {keyname} 遇到问题{ex.Message} , {ex.StackTrace}");
                            }
                        }).Wait(TimeSpan.FromSeconds(60));
                        if (!ok)
                        {
                            string message = $" {keyname}超时";
                            _logger.LogInformation(message);
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogInformation($"{keyname}  {ex.Message}");
                    }
                }
            }
            return Task.CompletedTask;
        }


    }
}