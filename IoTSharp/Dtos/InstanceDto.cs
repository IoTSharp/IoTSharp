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


        public string Domain { get; set; }
    }

}