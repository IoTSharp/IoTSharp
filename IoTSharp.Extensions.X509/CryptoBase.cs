#region License
/*
Copyright (c) 2018 Konrad Mattheis und Martin Berthold
Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
#endregion

namespace IoTSharp.Extensions.X509
{
    #region Usings
    using Org.BouncyCastle.Crypto;
    using Org.BouncyCastle.Crypto.Generators;
    using Org.BouncyCastle.Crypto.Parameters;
    using Org.BouncyCastle.Crypto.Prng;
    using Org.BouncyCastle.OpenSsl;
    using Org.BouncyCastle.Security;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    #endregion

    public class CryptoBase
    {
        public RsaPrivateCrtKeyParameters PrivateKey { get; private set; }
        public RsaKeyParameters PublicKey { get; private set; }
        public bool HasPrivateKey { get; private set; }

        private AsymmetricCipherKeyPair GetKeyPair()
        {
            var randomGenerator = new VmpcRandomGenerator();
            var secureRandom = new SecureRandom(randomGenerator);
            var keyGenerationParameters = new KeyGenerationParameters(secureRandom, 2048);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            return keyPairGenerator.GenerateKeyPair();
        }

        protected RSACryptoServiceProvider GetRsaProvider()
        {
            var rsaParameters = new RSAParameters();
            if (HasPrivateKey)
                rsaParameters = PemUtils.ToRSAParameters(PrivateKey);
            else
                rsaParameters = PemUtils.ToRSAParameters(PublicKey);
            var rsa = new RSACryptoServiceProvider();
            rsa.ImportParameters(rsaParameters);
            return rsa;
        }

        protected void GenerateKeys()
        {
            var pair = GetKeyPair();
            PrivateKey = pair.Private as RsaPrivateCrtKeyParameters;
            PublicKey = pair.Public as RsaKeyParameters;
        }

        protected void ReadKey(string keyPath)
        {
            ReadKey(new FileStream(keyPath, FileMode.Open, FileAccess.Read, FileShare.Read));
        }

        protected void ReadKey(Stream keyStream)
        {
            using (var reader = new StreamReader(keyStream, Encoding.ASCII))
            {
                var pemReader = new PemReader(reader);
                var pemObject = pemReader.ReadObject();
                if (pemObject is RsaKeyParameters)
                {
                    PublicKey = pemObject as RsaKeyParameters;
                }
                else if (pemObject is AsymmetricCipherKeyPair)
                {
                    var pair = pemObject as AsymmetricCipherKeyPair;
                    PrivateKey = pair.Private as RsaPrivateCrtKeyParameters;
                    PublicKey = pair.Public as RsaKeyParameters;
                    HasPrivateKey = true;
                }
                else
                    throw new Exception($"The key object {pemObject.GetType()} is unkown.");
            }
        }

        protected void SaveKey(string path, object key)
        {
            using (var writer = new StreamWriter(path, false, Encoding.ASCII))
            {
                var pemWriter = new PemWriter(writer);
                pemWriter.WriteObject(key);
                pemWriter.Writer.Flush();
            }
        }
    }
}