namespace IoTSharp.Dtos
{

    public class InstanceDto
    {
        /// <summary>
        /// 系统版本
        /// </summary>
        public string Version { get;  set; }
        /// <summary>
        /// 是否被安装
        /// </summary>
        public bool Installed { get;  set; }
        public bool CACertificate { get;  set; }

        /// <summary>
        /// http://localhost/  
        /// </summary>
        public string Domain { get; set; }
        public string BrokerThumbprint { get;   set; }
        public string CAThumbprint { get;  set; }
    }

}