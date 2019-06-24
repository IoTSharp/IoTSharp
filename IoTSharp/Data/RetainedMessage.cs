using MQTTnet;
using MQTTnet.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    [Serializable]
    public class RetainedMessage
    {


        // user-defined conversion from Fraction to double
        public static implicit operator MqttApplicationMessage(RetainedMessage f)
        {
            return new MqttApplicationMessage() { Payload = f.Payload, Retain = true, Topic = f.Topic, QualityOfServiceLevel = f.QualityOfServiceLevel };
        }
        public static implicit operator RetainedMessage(MqttApplicationMessage f)
        {
            return new RetainedMessage(f);
        }
        public byte[] Payload { get; set; }
        public MqttQualityOfServiceLevel QualityOfServiceLevel { get; set; }
        public string Topic { get; set; }
        public bool Retain { get; set; }
        MD5 MD5 = MD5.Create();
        public RetainedMessage()
        {

        }
        public RetainedMessage(MqttApplicationMessage retained)
        {
            Topic = retained.Topic;
            QualityOfServiceLevel = retained.QualityOfServiceLevel;
            Payload = retained.Payload;
            Retain = retained.Retain;
            List<byte> lst = new List<byte>(Payload);
            lst.AddRange(System.Text.Encoding.UTF8.GetBytes(Topic));
            Id = BitConverter.ToString(MD5.ComputeHash(lst.ToArray())).Replace("-", "");
        }
        [Key]
        public string Id { get; set; }
    }
}
