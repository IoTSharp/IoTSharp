using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Security.Cryptography;
using System.Text;

namespace IoTSharp.Extensions.BouncyCastle
{
    /************************************************************
   * 关于hashAlgorithm参数值有： SHA1、SHA256、SHA384、SHA512
   * 重要的事情说三遍，不懂的自己恶补去。
   * RSA加密解密：私钥解密，公钥加密。
   * RSA数字签名-俗称加签验签：私钥加签，公钥验签。 
   * RSA加密解密：私钥解密，公钥加密。
   * RSA数字签名-俗称加签验签：私钥加签，公钥验签。 
   * RSA加密解密：私钥解密，公钥加密。
   * RSA数字签名-俗称加签验签：私钥加签，公钥验签。 
   * ☆☆☆☆【注意这里所有的加密结果及加签结果都是base64的】☆☆☆☆☆
   *
   *
   *
   *
   *
   * gzy整理
   */

    public class RSAHelper
    {
        #region 加密

        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="publicKeyJava"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string EncryptJava(string publicKeyJava, string data, string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromPublicKeyJavaString(publicKeyJava);
            cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), false);
            return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>
        /// RSA加密
        /// </summary>
        /// <param name="publicKeyCSharp"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string EncryptCSharp(string publicKeyCSharp, string data, string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(publicKeyCSharp);
            cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), false);
            return Convert.ToBase64String(cipherbytes);
        }

        /// <summary>
        /// RSA加密PEM秘钥
        /// </summary>
        /// <param name="publicKeyPEM"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string EncryptPEM(string publicKeyPEM, string data, string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.LoadPublicKeyPEM(publicKeyPEM);
            cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), false);
            return Convert.ToBase64String(cipherbytes);
        }

        #endregion 加密

        #region 解密

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="privateKeyJava"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string DecryptJava(string privateKeyJava, string data, string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromPrivateKeyJavaString(privateKeyJava);
            cipherbytes = rsa.Decrypt(Convert.FromBase64String(data), false);
            return Encoding.GetEncoding(encoding).GetString(cipherbytes);
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="privateKeyCSharp"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string DecryptCSharp(string privateKeyCSharp, string data, string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.FromXmlString(privateKeyCSharp);
            cipherbytes = rsa.Decrypt(Convert.FromBase64String(data), false);
            return Encoding.GetEncoding(encoding).GetString(cipherbytes);
        }

        /// <summary>
        /// RSA解密
        /// </summary>
        /// <param name="privateKeyPEM"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string DecryptPEM(string privateKeyPEM, string data, string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            byte[] cipherbytes;
            rsa.LoadPrivateKeyPEM(privateKeyPEM);
            cipherbytes = rsa.Decrypt(Convert.FromBase64String(data), false);
            return Encoding.GetEncoding(encoding).GetString(cipherbytes);
        }

        #endregion 解密

        #region 加签

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="privateKeyJava">私钥</param>
        /// <param name="data">待签名的内容</param>
        /// <returns></returns>
        public static string RSASignJava(string data, string privateKeyJava, string hashAlgorithm = "MD5", string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromPrivateKeyJavaString(privateKeyJava);//加载私钥
            var dataBytes = Encoding.GetEncoding(encoding).GetBytes(data);
            var HashbyteSignature = rsa.SignData(dataBytes, hashAlgorithm);
            return Convert.ToBase64String(HashbyteSignature);
        }

        /// <summary>
        /// RSA签名
        /// </summary>
        /// <param name="privateKeyPEM">私钥</param>
        /// <param name="data">待签名的内容</param>
        /// <returns></returns>
        public static string RSASignPEM(string data, string privateKeyPEM, string hashAlgorithm = "MD5", string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.LoadPrivateKeyPEM(privateKeyPEM);//加载私钥
            var dataBytes = Encoding.GetEncoding(encoding).GetBytes(data);
            var HashbyteSignature = rsa.SignData(dataBytes, hashAlgorithm);
            return Convert.ToBase64String(HashbyteSignature);
        }

        /// <summary>
        /// RSA签名CSharp
        /// </summary>
        /// <param name="privateKeyCSharp">私钥</param>
        /// <param name="data">待签名的内容</param>
        /// <returns></returns>
        public static string RSASignCSharp(string data, string privateKeyCSharp, string hashAlgorithm = "MD5", string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(privateKeyCSharp);//加载私钥
            var dataBytes = Encoding.GetEncoding(encoding).GetBytes(data);
            var HashbyteSignature = rsa.SignData(dataBytes, hashAlgorithm);
            return Convert.ToBase64String(HashbyteSignature);
        }

        #endregion 加签

        #region 验签

        /// <summary>
        /// 验证签名-方法一
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static bool VerifyJava(string data, string publicKeyJava, string signature, string hashAlgorithm = "MD5", string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            //导入公钥，准备验证签名
            rsa.FromPublicKeyJavaString(publicKeyJava);
            //返回数据验证结果
            byte[] Data = Encoding.GetEncoding(encoding).GetBytes(data);
            byte[] rgbSignature = Convert.FromBase64String(signature);

            return rsa.VerifyData(Data, hashAlgorithm, rgbSignature);
        }

        /// <summary>
        /// 验证签名PEM
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static bool VerifyPEM(string data, string publicKeyPEM, string signature, string hashAlgorithm = "MD5", string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            //导入公钥，准备验证签名
            rsa.LoadPublicKeyPEM(publicKeyPEM);
            //返回数据验证结果
            byte[] Data = Encoding.GetEncoding(encoding).GetBytes(data);
            byte[] rgbSignature = Convert.FromBase64String(signature);

            return rsa.VerifyData(Data, hashAlgorithm, rgbSignature);
        }

        /// <summary>
        /// 验证签名CSharp
        /// </summary>
        /// <param name="data"></param>
        /// <param name="signature"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static bool VerifyCSharp(string data, string publicKeyCSharp, string signature, string hashAlgorithm = "MD5", string encoding = "UTF-8")
        {
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            //导入公钥，准备验证签名
            rsa.LoadPublicKeyPEM(publicKeyCSharp);
            //返回数据验证结果
            byte[] Data = Encoding.GetEncoding(encoding).GetBytes(data);
            byte[] rgbSignature = Convert.FromBase64String(signature);

            return rsa.VerifyData(Data, hashAlgorithm, rgbSignature);
        }

        

        #endregion 验签

 
        #region 私钥加密

        /// <summary>
        /// 基于BouncyCastle的RSA私钥加密
        /// </summary>
        /// <param name="privateKeyJava"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string EncryptPrivateKeyJava(string privateKeyJava, string data, string encoding = "UTF-8")
        {
            RsaKeyParameters privateKeyParam = (RsaKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKeyJava));
            byte[] cipherbytes = Encoding.GetEncoding(encoding).GetBytes(data);
            RsaEngine rsa = new RsaEngine();
            rsa.Init(true, privateKeyParam);//参数true表示加密/false表示解密。
            cipherbytes = rsa.ProcessBlock(cipherbytes, 0, cipherbytes.Length);
            return Convert.ToBase64String(cipherbytes);
        }

        #endregion 私钥加密

        #region 公钥解密

        /// <summary>
        /// 基于BouncyCastle的RSA公钥解密
        /// </summary>
        /// <param name="publicKeyJava"></param>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string DecryptPublicKeyJava(string publicKeyJava, string data, string encoding = "UTF-8")
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKeyJava));
            byte[] cipherbytes = Convert.FromBase64String(data);
            RsaEngine rsa = new RsaEngine();
            rsa.Init(false, publicKeyParam);//参数true表示加密/false表示解密。
            cipherbytes = rsa.ProcessBlock(cipherbytes, 0, cipherbytes.Length);
            return Encoding.GetEncoding(encoding).GetString(cipherbytes);
        }

        #endregion 公钥解密

        #region 加签

        /// <summary>
        /// 基于BouncyCastle的RSA签名
        /// </summary>
        /// <param name="data"></param>
        /// <param name="privateKeyJava"></param>
        /// <param name="hashAlgorithm">JAVA的和.NET的不一样，如：MD5(.NET)等同于MD5withRSA(JAVA)</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static string RSASignJavaBouncyCastle(string data, string privateKeyJava, string hashAlgorithm = "MD5withRSA", string encoding = "UTF-8")
        {
            RsaKeyParameters privateKeyParam = (RsaKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateKeyJava));
            ISigner signer = SignerUtilities.GetSigner(hashAlgorithm);
            signer.Init(true, privateKeyParam);//参数为true验签，参数为false加签
            var dataByte = Encoding.GetEncoding(encoding).GetBytes(data);
            signer.BlockUpdate(dataByte, 0, dataByte.Length);
            return Convert.ToBase64String(signer.GenerateSignature());
        }

        #endregion 加签

        #region 验签

        /// <summary>
        /// 基于BouncyCastle的RSA签名
        /// </summary>
        /// <param name="data">源数据</param>
        /// <param name="publicKeyJava"></param>
        /// <param name="signature">base64签名</param>
        /// <param name="hashAlgorithm">JAVA的和.NET的不一样，如：MD5(.NET)等同于MD5withRSA(JAVA)</param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static bool VerifyJavaBouncyCastle(string data, string publicKeyJava, string signature, string hashAlgorithm = "MD5withRSA", string encoding = "UTF-8")
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKeyJava));
            ISigner signer = SignerUtilities.GetSigner(hashAlgorithm);
            signer.Init(false, publicKeyParam);
            byte[] dataByte = Encoding.GetEncoding(encoding).GetBytes(data);
            signer.BlockUpdate(dataByte, 0, dataByte.Length);
            byte[] signatureByte = Convert.FromBase64String(signature);
            return signer.VerifySignature(signatureByte);
        }
        #endregion 验签
    }
}