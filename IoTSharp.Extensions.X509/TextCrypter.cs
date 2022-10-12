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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    #endregion

    public class TextCrypter : CryptoBase
    {
        public TextCrypter()
        {
            GenerateKeys();
        }

        public TextCrypter(string keyPath)
        {
            ReadKey(keyPath);
        }

        public string EncryptText(string text)
        {
            try
            {
                var rsaProvider = GetRsaProvider();
                var data = Encoding.UTF8.GetBytes(text);
                var encryptedData = rsaProvider.Encrypt(data, true);
                var base64Encrypted = Convert.ToBase64String(encryptedData);
                return base64Encrypted;
            }
            catch (Exception ex)
            {
                throw new Exception("The text could not be encrypt.", ex);
            }
        }

        public string DecryptText(string base64EncryptedText)
        {
            try
            {
                if (!HasPrivateKey)
                    throw new Exception("No private key found.");

                var rsaProvider = GetRsaProvider();
                var resultBytes = Convert.FromBase64String(base64EncryptedText);
                var decryptedBytes = rsaProvider.Decrypt(resultBytes, true);
                var decryptedData = Encoding.UTF8.GetString(decryptedBytes);
                return decryptedData.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception("The base64 text could not decrypt.", ex);
            }
        }

        public void SavePrivateKey(string path)
        {
            try
            {
                SaveKey(path, PrivateKey);
            }
            catch (Exception ex)
            {
                throw new Exception("The private key could not saved.", ex);
            }
        }

        public void SavePublicKey(string path)
        {
            try
            {
                SaveKey(path, PublicKey);
            }
            catch (Exception ex)
            {
                throw new Exception("The public key could not saved.", ex);
            }
        }
    }
}
