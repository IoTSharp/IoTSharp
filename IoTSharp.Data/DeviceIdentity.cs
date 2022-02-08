using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IoTSharp.Data
{
    public class DeviceIdentity
    {
        /// <summary>
        /// 认证方式ID
        /// </summary>
        [Key]
        public Guid Id { get; set; }
        /// <summary>
        /// 认证方式类型
        /// </summary>
        [Required]
        [EnumDataType(typeof(IdentityType))]
        public IdentityType IdentityType { get; set; }

        /// <summary>
        /// When <see cref="IdentityType"/> Is <see cref="IdentityType.AccessToken"/> ,this is a Token.
        /// When <see cref="IdentityType"/> Is <see cref="IdentityType.DevicePassword"/> ,this is a device name.
        /// When <see cref="IdentityType"/> Is <see cref="IdentityType.X509Certificate"/> ,this is X509 Certificate' Fingerprint.
        /// </summary>
        [Required]
        public string IdentityId { get; set; }

        /// <summary>
        /// When <see cref="IdentityType"/> Is <see cref="IdentityType.AccessToken"/> ,this is null.
        /// When <see cref="IdentityType"/> Is <see cref="IdentityType.DevicePassword"/> ,this is a password.
        /// When <see cref="IdentityType"/> Is <see cref="IdentityType.X509Certificate"/> ,this is X509 Certificate' PEM.
        /// </summary>
        public string IdentityValue { get; set; }
        [System.Text.Json.Serialization.JsonIgnore]
        [Newtonsoft.Json.JsonIgnore]
        [Required]
        public Device Device { get; set; }

        public Guid DeviceId { get; set; }
    }
}