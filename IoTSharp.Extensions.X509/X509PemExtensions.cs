#region License

/*
Copyright (c) 2018 Konrad Mattheis und Martin Berthold
Copyright (c) 2018 MaiKeBing IoT#
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

#endregion

namespace IoTSharp.Extensions.X509
{
    #region Usings

    using Org.BouncyCastle.Crypto.Parameters;
    using System;
    using System.IO;
    using System.Security.Cryptography.X509Certificates;

    #endregion

    public static class X509PemExtensions
    {
        #region Public Methods

        public static void SavePem(this X509Certificate2 @this, out string cert, out string privateKey)
        {
            cert = string.Empty;
            privateKey = string.Empty;
            try
            {
                if (@this.HasPrivateKey)
                {
                    var p = @this.GetRSAPrivateKey().ExportParameters(true);
                    var key = new RsaPrivateCrtKeyParameters(
                        new Org.BouncyCastle.Math.BigInteger(1, p.Modulus), new Org.BouncyCastle.Math.BigInteger(1, p.Exponent), new Org.BouncyCastle.Math.BigInteger(1, p.D),
                        new Org.BouncyCastle.Math.BigInteger(1, p.P), new Org.BouncyCastle.Math.BigInteger(1, p.Q), new Org.BouncyCastle.Math.BigInteger(1, p.DP), new Org.BouncyCastle.Math.BigInteger(1, p.DQ),
                        new Org.BouncyCastle.Math.BigInteger(1, p.InverseQ));
                    using (var stringWriter = new StringWriter())
                    {
                        var pemWriter = new Org.BouncyCastle.OpenSsl.PemWriter(stringWriter);
                        pemWriter.WriteObject(key);
                        privateKey = stringWriter.GetStringBuilder().ToString();
                    }
                }
                cert = PemCertificateHelper.ExportCertificateToPEM(@this);
            }
            catch (Exception ex)
            {
                throw new Exception($"Certificate could not be saved.  ", ex);
            }
        }

        public static void SavePem(this X509Certificate2 @this, string certFile, string privateKeyFile = null)
        {
            SavePem(@this, out string cert, out string privateKey);
            File.WriteAllText(certFile, cert);
            File.WriteAllText(privateKeyFile, privateKey);
        }

        public static X509Certificate2 LoadPem(this X509Certificate2 @this, string certFile, string privateKeyFile = null, string password = null)
        {
            try
            {
                return PemCertificateHelper.ReadPemCertificateWithPrivateKey(certFile, privateKeyFile, password);
            }
            catch (Exception ex)
            {
                throw new Exception($"Pem certificate {certFile} could not be loaded", ex);
            }
        }

        public static X509Certificate2 LoadPem(this X509Certificate2 @this, byte[] certBuffer, byte[] privateKeyBuffer = null)
        {
            try
            {
                return PemCertificateHelper.ReadPemCertificateWithPrivateKey(certBuffer, privateKeyBuffer);
            }
            catch (Exception ex)
            {
                throw new Exception($"Pem certificate buffer could not be loaded", ex);
            }
        }

        #endregion
    }
}