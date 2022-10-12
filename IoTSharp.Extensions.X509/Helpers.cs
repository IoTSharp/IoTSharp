#region License
/*
Copyright (c) 2018 Konrad Mattheis und Martin Berthold
Copyright (c) 2018 MaiKeBing
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion

namespace IoTSharp.Extensions.X509
{
    #region Usings
    using Org.BouncyCastle.Asn1;
    using Org.BouncyCastle.Asn1.Pkcs;
    using Org.BouncyCastle.Asn1.X509;
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Generators;
    using Org.BouncyCastle.Crypto.Operators;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.Crypto.Prng;
    using Org.BouncyCastle.Math;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.Pkcs;
    using Org.BouncyCastle.Security;
    using Org.BouncyCastle.X509;
    using Org.BouncyCastle.X509.Extension;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;
    #endregion
    public class PasswordFinder : IPasswordFinder
    {
        string passwrd_;
        public PasswordFinder(string passwrd)
        {
            passwrd_ = passwrd;
        }
        public char[] GetPassword()
        {
            return passwrd_.ToCharArray();
        }
    }
    #region Helper Classes
    public   static class PemCertificateHelper
    {
   

        #region Properties & Variables
        public static AsymmetricCipherKeyPair userKeyPair = null;
        #endregion

        #region Private Methods
        public static AsymmetricCipherKeyPair ReadPrivateKey(string privateKeyFile, PasswordFinder password = null) => ReadPrivateKey(File.ReadAllBytes(privateKeyFile), password);

        public static AsymmetricCipherKeyPair ReadPrivateKey(byte[] privateKeyBuffer, PasswordFinder password = null)
        {
            try
            {
                if (privateKeyBuffer == null)
                    throw new Exception("The key buffer is null.");
                var reader = new StreamReader(new MemoryStream(privateKeyBuffer));
                return (AsymmetricCipherKeyPair)new PemReader(reader, password).ReadObject();
            }
            catch (Exception ex)
            {
                throw new Exception($"The file {privateKeyBuffer} is not a private key.", ex);
            }
        }
        #endregion

        #region Public Methods
        public static X509Certificate2 GenerateSelfSignedCertificate(string subjectName, string issuerName, int keyStrength)
        {
            try
            {
                // Generating Random Numbers
                var randomGenerator = new VmpcRandomGenerator();
                var random = new SecureRandom(randomGenerator);

                // The Certificate Generator
                var certificateGenerator = new X509V3CertificateGenerator();

                // Serial Number
                var serialNumber = BigInteger.ProbablePrime(128, new Random());
                certificateGenerator.SetSerialNumber(serialNumber);

                // Issuer and Subject Name
                var subjectDN = new X509Name(subjectName);
                var issuerDN = new X509Name(issuerName);
                certificateGenerator.SetIssuerDN(issuerDN);
                certificateGenerator.SetSubjectDN(subjectDN);

                // Valid For
                var notBefore = DateTime.UtcNow.Date.AddYears(-1);
                var notAfter = notBefore.AddYears(10);
                certificateGenerator.SetNotBefore(notBefore);
                certificateGenerator.SetNotAfter(notAfter);

                // Subject Public Key
                var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
                var keyPairGenerator = new RsaKeyPairGenerator();
                keyPairGenerator.Init(keyGenerationParameters);

                if (userKeyPair == null)
                    userKeyPair = keyPairGenerator.GenerateKeyPair();

                certificateGenerator.SetPublicKey(userKeyPair.Public);

                //Extented
                certificateGenerator.AddExtension(X509Extensions.SubjectKeyIdentifier, false,
                                                  new SubjectKeyIdentifierStructure(userKeyPair.Public));
                certificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, false,
                                                  new AuthorityKeyIdentifier(SubjectPublicKeyInfoFactory
                                                  .CreateSubjectPublicKeyInfo(userKeyPair.Public)));
                var valueData = Encoding.ASCII.GetBytes("Client");
                certificateGenerator.AddExtension("1.3.6.1.5.5.7.13.3", false, valueData);

                // Generating the Certificate
                var issuerKeyPair = userKeyPair;

                // Signature Algorithm
                ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512WITHRSA", userKeyPair.Private, random);

                // selfsign certificate
                var certificate = certificateGenerator.Generate(signatureFactory);

                // correcponding private key
                var info = PrivateKeyInfoFactory.CreatePrivateKeyInfo(userKeyPair.Private);

                // merge into X509Certificate2
                var x509 = new X509Certificate2(certificate.GetEncoded());

                var seq = (Asn1Sequence)info.ParsePrivateKey();
                if (seq.Count != 9)
                    throw new Exception("malformed sequence in RSA private key");

                var rsa = RsaPrivateKeyStructure.GetInstance(seq);
                var rsaparams = new RsaPrivateCrtKeyParameters(rsa.Modulus, rsa.PublicExponent, rsa.PrivateExponent,
                                                               rsa.Prime1, rsa.Prime2, rsa.Exponent1, rsa.Exponent2,
                                                               rsa.Coefficient);

                x509 = x509.CopyWithPrivateKey(PemUtils.ToRSA(rsaparams));

                return x509;
            }
            catch (Exception ex)
            {
                throw new Exception ($"The Method \"{nameof(GenerateSelfSignedCertificate)}\" has failed.",ex);
            }
        }

        public static string ExportCertificateToPEM(X509Certificate2 cert)
        {
            try
            {
                var builder = new StringBuilder();
                builder.AppendLine("-----BEGIN CERTIFICATE-----");
                builder.AppendLine(Convert.ToBase64String(cert.Export(X509ContentType.Cert),
                                   Base64FormattingOptions.InsertLineBreaks));
                builder.AppendLine("-----END CERTIFICATE-----");
                return builder.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception( $"The Method \"{nameof(ExportCertificateToPEM)}\" has failed.",ex);
            }
        }

        public static string ExportKeyToPEM(AsymmetricKeyParameter key)
        {
            try
            {
                var textWriter = new StringWriter();
                var pemWriter = new PemWriter(textWriter);
                pemWriter.WriteObject(key);
                pemWriter.Writer.Flush();
                string result = textWriter.ToString();
                pemWriter.Writer.Close();
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"The Method \"{nameof(ExportKeyToPEM)}\" has failed.", ex);
            }
        }
        public static X509Certificate2 ReadPemCertificateWithPrivateKey(byte[] certificateBuffer, byte[] privateKeyBuffer,string password=null)
        {
            try
            {
                var x509Cert = new X509Certificate2(certificateBuffer,password);
                if (privateKeyBuffer!=null)
                    x509Cert = AddPemPrivateKeyToCertificate(x509Cert, privateKeyBuffer,password);
                return x509Cert;
            }
            catch (Exception ex)
            {
                throw new Exception($"The Method \"{nameof(ReadPemCertificateWithPrivateKey)}\" has failed.",ex);
       
            }
        }
        public static X509Certificate2 ReadPemCertificateWithPrivateKey(string certificateFile, string privateKeyFile, string password)
                    => ReadPemCertificateWithPrivateKey(File.ReadAllBytes(certificateFile), File.ReadAllBytes(privateKeyFile), password);



        public static X509Certificate2 AddPemPrivateKeyToCertificate(X509Certificate2 certificate, string privateKeyFile, string password = null)
                        => AddPemPrivateKeyToCertificate(certificate, System.IO.File.ReadAllBytes(privateKeyFile), password);

        public static X509Certificate2 AddPemPrivateKey(this X509Certificate2 certificate, string privateKeyFile, string password = null)
                        => AddPemPrivateKeyToCertificate(certificate, System.IO.File.ReadAllBytes(privateKeyFile), password);

        public static X509Certificate2 AddPemPrivateKey(this X509Certificate2 certificate, byte[] privateKeyBuffer, string password = null)
                    => AddPemPrivateKeyToCertificate(certificate, privateKeyBuffer, password);
        private static X509Certificate2 _AddPrivateKeyOrReCreateCert(X509Certificate2 certificate, AsymmetricCipherKeyPair keyPair,string password=null)
        {
#if NETCOREAPP || NET472
            var rsaPrivateKey = PemUtils.ToRSA(keyPair.Private as RsaPrivateCrtKeyParameters);
            certificate = certificate.CopyWithPrivateKey(rsaPrivateKey);
#elif NET46 || NET40
                certificate = certificate.CopyWithPrivateKey(keyPair, password);
#endif
            return certificate;
        }

        public static X509Certificate2 CopyWithPrivateKey( this X509Certificate2 certificate, AsymmetricCipherKeyPair rsa,string password )
        {
            return new X509Certificate2(CreatePfxFile(new X509CertificateParser().ReadCertificate(certificate.RawData), rsa.Private), password, System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.Exportable);
        }
        private static byte[] CreatePfxFile(Org.BouncyCastle.X509.X509Certificate certificate, AsymmetricKeyParameter privateKey, string password = null)
        {
            // create certificate entry
            var certEntry = new X509CertificateEntry(certificate);
            string friendlyName = certificate.SubjectDN.ToString();

            // get bytes of private key.
            PrivateKeyInfo keyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKey);
            byte[] keyBytes = keyInfo.ToAsn1Object().GetEncoded();

            var builder = new Pkcs12StoreBuilder();
            builder.SetUseDerEncoding(true);
            var store = builder.Build();
            // create store entry
            store.SetKeyEntry("", new AsymmetricKeyEntry(privateKey), new X509CertificateEntry[] { certEntry });
            byte[] pfxBytes = null;
            using (MemoryStream stream = new MemoryStream())
            {
                store.Save(stream, password?.ToCharArray(), new SecureRandom());
                pfxBytes = stream.ToArray();
            }
            var result = Pkcs12Utilities.ConvertToDefiniteLength(pfxBytes);
            return result;
        }
        public static X509Certificate2 AddPemPrivateKeyToCertificate(X509Certificate2 certificate, byte[] privateKeyBuffer,string password=null)
        {
            try
            {
                var keyPair = ReadPrivateKey(privateKeyBuffer, password != null ? new PasswordFinder(password) : null);
                certificate = _AddPrivateKeyOrReCreateCert(certificate, keyPair, password);

                return certificate;
            }
            catch (Exception ex)
            {
                throw new Exception( $"The Method \"{nameof(AddPemPrivateKeyToCertificate)}\" has failed.",ex);
            }
        }
        #endregion
    }

  
    class QlikClientCertificate
    {

#region Enums
        public enum PemStringType
        {
            Certificate,
            RsaPrivateKey
        }
#endregion

#region Properties & Variables
        private string PublicCertificate { get; set; }
        private string PrivateKey { get; set; }
        private string Password { get; set; }
        private bool IsSingleFile { get; set; }
        public static string DefaultFolder => @"C:\ProgramData\Qlik\Sense\Repository\Exported Certificates\.Local Certificates";
#endregion

#region Constructor
        public QlikClientCertificate(string certKeyFilePath, string password)
        {
            PublicCertificate = File.ReadAllText(certKeyFilePath);
            IsSingleFile = true;
            Password = password;
        }

        public QlikClientCertificate(string certPath, string keyPath, string password)
        {
            if (!File.Exists(certPath))
                throw new Exception($"The client certificate {certPath} was not found.");

            if (!File.Exists(keyPath))
                throw new Exception($"The client key {keyPath} was not found.");

            PublicCertificate = File.ReadAllText(certPath);
            PrivateKey = File.ReadAllText(keyPath);
            IsSingleFile = false;
            Password = password;
        }
#endregion

#region Static Helper Functions
        //This function parses an integer size from the reader using the ASN.1 format
        private static int DecodeIntegerSize(System.IO.BinaryReader rd)
        {
            var count = -1;

            var byteValue = rd.ReadByte();
            if (byteValue != 0x02)
                return 0;

            byteValue = rd.ReadByte();
            if (byteValue == 0x81)
                count = rd.ReadByte();
            else if (byteValue == 0x82)
            {
                var hi = rd.ReadByte();
                var lo = rd.ReadByte();
                count = BitConverter.ToUInt16(new[] { lo, hi }, 0);
            }
            else
                count = byteValue;        // we already have the data size

            //remove high order zeros in data
            while (rd.ReadByte() == 0x00)
                count -= 1;

            rd.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }

        private static byte[] GetBytesFromPEM(string pemString, PemStringType type)
        {
            string header;
            string footer;

            switch (type)
            {
                case PemStringType.Certificate:
                    header = "-----BEGIN CERTIFICATE-----";
                    footer = "-----END CERTIFICATE-----";
                    break;
                case PemStringType.RsaPrivateKey:
                    header = "-----BEGIN RSA PRIVATE KEY-----";
                    footer = "-----END RSA PRIVATE KEY-----";
                    break;
                default:
                    return null;
            }

            var start = pemString.IndexOf(header) + header.Length;
            var end = pemString.IndexOf(footer, start) - start;
            return Convert.FromBase64String(pemString.Substring(start, end));
        }

        private static byte[] AlignBytes(byte[] inputBytes, int alignSize)
        {
            var inputBytesSize = inputBytes.Length;
            if ((alignSize != -1) && (inputBytesSize < alignSize))
            {
                var buf = new byte[alignSize];
                for (int i = 0; i < inputBytesSize; ++i)
                    buf[i + (alignSize - inputBytesSize)] = inputBytes[i];

                return buf;
            }
            else
            {
                //Already aligned, or doesn't need alignment
                return inputBytes;
            }
        }

        //This helper function parses an RSA private key using the ASN.1 format
        private static RSACryptoServiceProvider DecodeRsaPrivateKey(byte[] privateKeyBytes)
        {
            var ms = new MemoryStream(privateKeyBytes);
            var rd = new BinaryReader(ms);

            try
            {
                var shortValue = rd.ReadUInt16();
                switch (shortValue)
                {
                    case 0x8130:
                        // If true, data is little endian since the proper logical seq is 0x30 0x81
                        rd.ReadByte(); //advance 1 byte
                        break;
                    case 0x8230:
                        rd.ReadInt16();  //advance 2 bytes
                        break;
                    default:
                        return null;
                }

                shortValue = rd.ReadUInt16();
                if (shortValue != 0x0102) // (version number)
                    return null;

                var byteValue = rd.ReadByte();
                if (byteValue != 0x00)
                    return null;
                var rsa = new RSACryptoServiceProvider();
 
                var rsAparams = new RSAParameters()
                {
                    Modulus = rd.ReadBytes(DecodeIntegerSize(rd)),
                };

                // Argh, this is a pain.  From emperical testing it appears to be that RSAParameters doesn't like byte buffers that
                // have their leading zeros removed.  The RFC doesn't address this area that I can see, so it's hard to say that this
                // is a bug, but it sure would be helpful if it allowed that. So, there's some extra code here that knows what the
                // sizes of the various components are supposed to be.  Using these sizes we can ensure the buffer sizes are exactly
                // what the RSAParameters expect.  Thanks, Microsoft.
                var traits = new RSAParameterTraits(rsAparams.Modulus.Length * 8);
                rsAparams.Modulus = AlignBytes(rsAparams.Modulus, traits.size_Mod);
                rsAparams.Exponent = AlignBytes(rd.ReadBytes(DecodeIntegerSize(rd)), traits.size_Exp);
                rsAparams.D = AlignBytes(rd.ReadBytes(DecodeIntegerSize(rd)), traits.size_D);
                rsAparams.P = AlignBytes(rd.ReadBytes(DecodeIntegerSize(rd)), traits.size_P);
                rsAparams.Q = AlignBytes(rd.ReadBytes(DecodeIntegerSize(rd)), traits.size_Q);
                rsAparams.DP = AlignBytes(rd.ReadBytes(DecodeIntegerSize(rd)), traits.size_DP);
                rsAparams.DQ = AlignBytes(rd.ReadBytes(DecodeIntegerSize(rd)), traits.size_DQ);
                rsAparams.InverseQ = AlignBytes(rd.ReadBytes(DecodeIntegerSize(rd)), traits.size_InvQ);
                rsa.ImportParameters(rsAparams);
                return rsa;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                rd.Close();
            }
        }
#endregion

#region Public Methods
        public X509Certificate2 GetCertificateFromPEM(string friendlyName = "QlikClient")
        {
            try
            {
                var certBuffer = GetBytesFromPEM(PublicCertificate, PemStringType.Certificate);
                var keyBuffer = new byte[0];
                if (IsSingleFile)
                    keyBuffer = GetBytesFromPEM(PublicCertificate, PemStringType.RsaPrivateKey);
                else
                    keyBuffer = GetBytesFromPEM(PrivateKey, PemStringType.RsaPrivateKey);

                var newCertificate = new X509Certificate2(certBuffer, Password);
                var rsaPrivateKey = DecodeRsaPrivateKey(keyBuffer);
                newCertificate = newCertificate.CopyWithPrivateKey(rsaPrivateKey);
                newCertificate.FriendlyName = friendlyName;
                return newCertificate;
            }
            catch (Exception ex)
            {
                throw new Exception($"The Method \"{nameof(GetCertificateFromPEM)}\" has failed.",ex);
            }
        }
#endregion
    }

    class RSAParameterTraits
    {

#region Fields
        public int size_Mod = -1;
        public int size_Exp = -1;
        public int size_D = -1;
        public int size_P = -1;
        public int size_Q = -1;
        public int size_DP = -1;
        public int size_DQ = -1;
        public int size_InvQ = -1;
#endregion

#region Public Methods
        public RSAParameterTraits(int modulusLengthInBits)
        {
            try
            {
                // The modulus length is supposed to be one of the common lengths, which is the commonly referred to strength of the key,
                // like 1024 bit, 2048 bit, etc.  It might be a few bits off though, since if the modulus has leading zeros it could show
                // up as 1016 bits or something like that.
                var assumedLength = -1;
                var logbase = Math.Log(modulusLengthInBits, 2);
                if (logbase == (int)logbase)
                {
                    // It's already an even power of 2
                    assumedLength = modulusLengthInBits;
                }
                else
                {
                    // It's not an even power of 2, so round it up to the nearest power of 2.
                    assumedLength = (int)(logbase + 1.0);
                    assumedLength = (int)(Math.Pow(2, assumedLength));
                }

                switch (assumedLength)
                {
                    case 1024:
                        size_Mod = 0x80;
                        size_Exp = -1;
                        size_D = 0x80;
                        size_P = 0x40;
                        size_Q = 0x40;
                        size_DP = 0x40;
                        size_DQ = 0x40;
                        size_InvQ = 0x40;
                        break;
                    case 2048:
                        size_Mod = 0x100;
                        size_Exp = -1;
                        size_D = 0x100;
                        size_P = 0x80;
                        size_Q = 0x80;
                        size_DP = 0x80;
                        size_DQ = 0x80;
                        size_InvQ = 0x80;
                        break;
                    case 4096:
                        size_Mod = 0x200;
                        size_Exp = -1;
                        size_D = 0x200;
                        size_P = 0x100;
                        size_Q = 0x100;
                        size_DP = 0x100;
                        size_DQ = 0x100;
                        size_InvQ = 0x100;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"The Method \"{nameof(RSAParameterTraits)}\" has failed.",ex);
            }
        }
#endregion
    }
#endregion
}