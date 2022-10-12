#region License

/*
Copyright (c) 2018 MaiKeBing IoT#
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

#endregion

using System;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace IoTSharp.Extensions.X509
{
    public static class X509Self
    {
        public static X509Certificate2 CreateCA(string name)
        {
            X509Certificate2 x509 = null;
            // Creates a certificate roughly equivalent to
            // makecert -r -n "{name}" -a sha256 -cy authority
            //
            using (RSA rsa = RSA.Create())
            {
                var request = new CertificateRequest(name, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                request.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(true, false, 0, true));

                // makecert will add an authority key identifier extension, which .NET doesn't
                // have out of the box.
                //
                // It does not add a subject key identifier extension, so we won't, either.
                x509 = request.CreateSelfSigned(
                    DateTimeOffset.UtcNow,
                    // makecert's fixed default end-date.
                    new DateTimeOffset(2039, 12, 31, 23, 59, 59, TimeSpan.Zero));
            }
            return x509;
        }
    }

    public static class X509SelfExtensions
    {
        /// <summary>
        /// nistP384
        /// </summary>
        /// <param name="issuer"></param>
        /// <param name="name"></param>
        /// <param name="altNames"></param>
        /// <returns></returns>
        public static X509Certificate2 CreateTlsClient(this X509Certificate2 issuer, string name, SubjectAlternativeNameBuilder altNames)
        {
            using (ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP384))
            {
                var request = new CertificateRequest(name, ecdsa, HashAlgorithmName.SHA384);

                request.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(false, false, 0, false));
                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment |  X509KeyUsageFlags.DataEncipherment   , false));
                request.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection { new Oid("1.3.6.1.5.5.7.3.1"),new Oid("1.3.6.1.5.5.7.3.2") }, false));

                if (altNames != null)
                {
                    request.CertificateExtensions.Add(altNames.Build());
                }
                X509Certificate2 signedCert = request.Create(
                    issuer, DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10),

                    Guid.NewGuid().ToByteArray());

                return signedCert.CopyWithPrivateKey(ecdsa);
            }
        }

        public static X509Certificate2 CreateTlsClientRSA(this X509Certificate2 issuer, string name, Guid guid, System.Net.IPAddress iPAddress, TimeSpan timeSpan)
        {
            var altNames = new SubjectAlternativeNameBuilder();
            altNames.AddIpAddress(iPAddress);
            return CreateTlsClientRSA(issuer, name, guid, altNames);
        }

        public static X509Certificate2 CreateTlsClientRSA(this X509Certificate2 issuer, string name, SubjectAlternativeNameBuilder altNames) => CreateTlsClientRSA(issuer, name, Guid.NewGuid(), altNames);

        public static X509Certificate2 CreateTlsClientRSA(this X509Certificate2 issuer, string name, Guid guid, SubjectAlternativeNameBuilder altNames)
        {
            using (RSA ecdsa = RSA.Create(2048))
            {
                var request = new CertificateRequest(name, ecdsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
                request.CertificateExtensions.Add(
                    new X509BasicConstraintsExtension(false, false, 0, false));
                request.CertificateExtensions.Add(
                    new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DataEncipherment, false));
                request.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection { new Oid("1.3.6.1.5.5.7.3.1"), new Oid("1.3.6.1.5.5.7.3.2") }, false));
                if (altNames != null)
                {
                    request.CertificateExtensions.Add(altNames.Build());
                }
                var notbefor = issuer.NotBefore;
                var notAfter = issuer.NotAfter;
                X509Certificate2 signedCert = request.Create(issuer, DateTimeOffset.Now, DateTimeOffset.Now.AddSeconds(notAfter.Subtract(DateTime.Now).TotalSeconds), guid.ToByteArray());
                return signedCert.CopyWithPrivateKey(ecdsa);
            }
        }

        public static X509Certificate2 BuildLocalhostTlsSelfSignedServer()
        {
            SubjectAlternativeNameBuilder sanBuilder = new SubjectAlternativeNameBuilder();
            sanBuilder.AddIpAddress(IPAddress.Loopback);
            sanBuilder.AddIpAddress(IPAddress.IPv6Loopback);
            sanBuilder.AddDnsName("localhost");
            sanBuilder.AddDnsName("localhost.localdomain");
            sanBuilder.AddDnsName(Environment.MachineName);

            using (RSA rsa = RSA.Create())
            {
                var request = new CertificateRequest("CN=localhost", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                request.CertificateExtensions.Add(
                    new X509EnhancedKeyUsageExtension(
                        new OidCollection { new Oid("1.3.6.1.5.5.7.3.1"), }, false));

                request.CertificateExtensions.Add(sanBuilder.Build());

                return request.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(10));
            }
        }

        /// <summary>
        /// 创建续订
        /// </summary>
        /// <param name="newKey"></param>
        /// <param name="currentCert"></param>
        /// <returns></returns>
        public static byte[] CreateCertificateRenewal(this X509Certificate2 currentCert, RSA newKey)
        {
            // Getting, and persisting, `newKey` is out of scope here.

            var request = new CertificateRequest(
                currentCert.SubjectName,
                newKey,
                HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            foreach (X509Extension extension in currentCert.Extensions)
            {
                request.CertificateExtensions.Add(extension);
            }
            // Send this to the CA you're requesting to sign your certificate.
            return request.CreateSigningRequest();
        }
        /// <summary>
        ///  如果遇到 “安全包中没有可用的凭证” ， 使用 这个转换一次。 
        /// </summary>
        /// <param name="sslCert"></param>
        /// <seealso cref="https://github.com/dotnet/runtime/issues/23749#issuecomment-747407051"/>
        /// <returns></returns>
        public static X509Certificate2 ToPkcs12(this X509Certificate2 sslCert)
        {
            // work around for Windows (WinApi) problems with PEMS, still in .NET 5
            return new X509Certificate2(sslCert.Export(X509ContentType.Pkcs12));
        }
    }
}