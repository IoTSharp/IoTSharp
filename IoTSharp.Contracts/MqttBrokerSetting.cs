using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace IoTSharp.Contracts
{
    public class MqttBrokerSetting
    {
        public int Port { get; set; } = 1883;
        public int TlsPort { get; set; } = 8883;
        public bool EnableTls { get; set; } = false;
        public SslProtocols SslProtocol { get; set; } = SslProtocols.Tls12;
        public bool PersistRetainedMessages { get; set; }

        X509Certificate2 _CACertificate;
        public X509Certificate2 CACertificate
        {
            get
            {
                if (_CACertificate == null && EnableTls)
                {
                    if (System.IO.File.Exists(CACertificateFile) && System.IO.File.Exists(CAPrivateKeyFile))
                    {
                        _CACertificate = X509Certificate2.CreateFromPemFile(CACertificateFile, CAPrivateKeyFile);
                    }
                }
                return _CACertificate;


            }
        }

        X509Certificate2 _BrokerCertificate;
        public X509Certificate2 BrokerCertificate
        {
            get
            {
                if (_BrokerCertificate == null && EnableTls)
                {
                    if (System.IO.File.Exists(CertificateFile) && System.IO.File.Exists(PrivateKeyFile))
                    {
                        _BrokerCertificate = X509Certificate2.CreateFromPemFile(CertificateFile, PrivateKeyFile);
                    }
                }
                return _BrokerCertificate;
            }
        }
        public string CACertificateFile { get; set; } = "security/ca.crt";
        public string CAPrivateKeyFile { get; set; } = "security/ca.key";
        public string CertificateFile { get; set; } = "security/server.crt";
        public string PrivateKeyFile { get; set; } = "security/server.key";
        public string DomainName { get; set; } = "http://localhost/";
    }
}
