using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;

namespace IoTSharp.Extensions.BouncyCastle
{
    /// <summary>
    /// https://github.com/winphone3721/WNSLP.Toolkits.RSA
    /// </summary>
    public static class RSAExtensions
    {

        /// <summary>
        ///  把java的私钥转换成.net的xml格式
        /// </summary>
        /// <param name="rsa"></param>
        /// <param name="privateJavaKey"></param>
        /// <returns></returns>
        public static string ConvertToXmlPrivateKey(this RSA rsa, string privateJavaKey)
        {
            RsaPrivateCrtKeyParameters privateKeyParam = (RsaPrivateCrtKeyParameters)PrivateKeyFactory.CreateKey(Convert.FromBase64String(privateJavaKey));
            string xmlPrivateKey = string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                         Convert.ToBase64String(privateKeyParam.Modulus.ToByteArrayUnsigned()),
                         Convert.ToBase64String(privateKeyParam.PublicExponent.ToByteArrayUnsigned()),
                         Convert.ToBase64String(privateKeyParam.P.ToByteArrayUnsigned()),
                         Convert.ToBase64String(privateKeyParam.Q.ToByteArrayUnsigned()),
                         Convert.ToBase64String(privateKeyParam.DP.ToByteArrayUnsigned()),
                         Convert.ToBase64String(privateKeyParam.DQ.ToByteArrayUnsigned()),
                         Convert.ToBase64String(privateKeyParam.QInv.ToByteArrayUnsigned()),
                         Convert.ToBase64String(privateKeyParam.Exponent.ToByteArrayUnsigned()));
            return xmlPrivateKey;
        }
        /// <summary>
        /// RSA加载JAVA  PrivateKey
        /// </summary>
        /// <param name="privateJavaKey">java提供的第三方私钥</param>
        /// <returns></returns>
        public static void FromPrivateKeyJavaString(this RSA rsa, string privateJavaKey)
        {
            string xmlPrivateKey = rsa.ConvertToXmlPrivateKey(privateJavaKey);
            rsa.FromXmlString(xmlPrivateKey);
        }

        /// <summary>
        /// 把java的公钥转换成.net的xml格式
        /// </summary>
        /// <param name="privateKey">java提供的第三方公钥</param>
        /// <returns></returns>
        public static string ConvertToXmlPublicJavaKey(this RSA rsa, string publicJavaKey)
        {
            RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicJavaKey));
            string xmlpublicKey = string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
              Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
              Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
            return xmlpublicKey;
        }

        /// <summary>
        /// 把java的私钥转换成.net的xml格式
        /// </summary>
        /// <param name="privateKey">java提供的第三方公钥</param>
        /// <returns></returns>
        public static void FromPublicKeyJavaString(this RSA rsa, string publicJavaKey)
        {
            string xmlpublicKey = rsa.ConvertToXmlPublicJavaKey(publicJavaKey);
            rsa.FromXmlString(xmlpublicKey);
        }
        ///// <summary>
        ///// RSA公钥格式转换，java->.net
        ///// </summary>
        ///// <param name="publicKey">java生成的公钥</param>
        ///// <returns></returns>
        //private static string ConvertJavaPublicKeyToDotNet(this RSA rsa,string publicKey)
        //{           
        //    RsaKeyParameters publicKeyParam = (RsaKeyParameters)PublicKeyFactory.CreateKey(Convert.FromBase64String(publicKey));
        //    return string.Format("<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent></RSAKeyValue>",
        //        Convert.ToBase64String(publicKeyParam.Modulus.ToByteArrayUnsigned()),
        //        Convert.ToBase64String(publicKeyParam.Exponent.ToByteArrayUnsigned()));
        //}

        /// <summary>Extension method for initializing a RSACryptoServiceProvider from PEM data string.</summary>

            #region Methods

            /// <summary>Extension method which initializes an RSACryptoServiceProvider from a DER public key blob.</summary>
            public static void LoadPublicKeyDER(this RSACryptoServiceProvider provider, byte[] DERData)
            {
                byte[] RSAData = GetRSAFromDER(DERData);
                byte[] publicKeyBlob = GetPublicKeyBlobFromRSA(RSAData);
                provider.ImportCspBlob(publicKeyBlob);
            }

            /// <summary>Extension method which initializes an RSACryptoServiceProvider from a DER private key blob.</summary>
            public static void LoadPrivateKeyDER(this RSACryptoServiceProvider provider, byte[] DERData)
            {
                byte[] privateKeyBlob = GetPrivateKeyDER(DERData);
                provider.ImportCspBlob(privateKeyBlob);
            }

            /// <summary>Extension method which initializes an RSACryptoServiceProvider from a PEM public key string.</summary>
            public static void LoadPublicKeyPEM(this RSACryptoServiceProvider provider, string sPEM)
            {
                byte[] DERData = GetDERFromPEM(sPEM);
                LoadPublicKeyDER(provider, DERData);
            }

            /// <summary>Extension method which initializes an RSACryptoServiceProvider from a PEM private key string.</summary>
            public static void LoadPrivateKeyPEM(this RSACryptoServiceProvider provider, string sPEM)
            {
                byte[] DERData = GetDERFromPEM(sPEM);
                LoadPrivateKeyDER(provider, DERData);
            }

            /// <summary>Returns a public key blob from an RSA public key.</summary>
            internal static byte[] GetPublicKeyBlobFromRSA(byte[] RSAData)
            {
                byte[] data = null;
                UInt32 dwCertPublicKeyBlobSize = 0;
                if (CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING,
                    new IntPtr((int)CRYPT_OUTPUT_TYPES.RSA_CSP_PUBLICKEYBLOB), RSAData, (UInt32)RSAData.Length, CRYPT_DECODE_FLAGS.NONE,
                    data, ref dwCertPublicKeyBlobSize))
                {
                    data = new byte[dwCertPublicKeyBlobSize];
                    if (!CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING,
                        new IntPtr((int)CRYPT_OUTPUT_TYPES.RSA_CSP_PUBLICKEYBLOB), RSAData, (UInt32)RSAData.Length, CRYPT_DECODE_FLAGS.NONE,
                        data, ref dwCertPublicKeyBlobSize))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                else
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                return data;
            }

            /// <summary>Converts DER binary format to a CAPI CRYPT_PRIVATE_KEY_INFO structure.</summary>
            internal static byte[] GetPrivateKeyDER(byte[] DERData)
            {
                byte[] data = null;
                UInt32 dwRSAPrivateKeyBlobSize = 0;
                IntPtr pRSAPrivateKeyBlob = IntPtr.Zero;
                if (CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING, new IntPtr((int)CRYPT_OUTPUT_TYPES.PKCS_RSA_PRIVATE_KEY),
                    DERData, (UInt32)DERData.Length, CRYPT_DECODE_FLAGS.NONE, data, ref dwRSAPrivateKeyBlobSize))
                {
                    data = new byte[dwRSAPrivateKeyBlobSize];
                    if (!CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING, new IntPtr((int)CRYPT_OUTPUT_TYPES.PKCS_RSA_PRIVATE_KEY),
                        DERData, (UInt32)DERData.Length, CRYPT_DECODE_FLAGS.NONE, data, ref dwRSAPrivateKeyBlobSize))
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                else
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                return data;
            }

            /// <summary>Converts DER binary format to a CAPI CERT_PUBLIC_KEY_INFO structure containing an RSA key.</summary>
            internal static byte[] GetRSAFromDER(byte[] DERData)
            {
                byte[] data = null;
                byte[] publicKey = null;
                CERT_PUBLIC_KEY_INFO info;
                UInt32 dwCertPublicKeyInfoSize = 0;
                IntPtr pCertPublicKeyInfo = IntPtr.Zero;
                if (CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING, new IntPtr((int)CRYPT_OUTPUT_TYPES.X509_PUBLIC_KEY_INFO),
                    DERData, (UInt32)DERData.Length, CRYPT_DECODE_FLAGS.NONE, data, ref dwCertPublicKeyInfoSize))
                {
                    data = new byte[dwCertPublicKeyInfoSize];
                    if (CryptDecodeObject(CRYPT_ENCODING_FLAGS.X509_ASN_ENCODING | CRYPT_ENCODING_FLAGS.PKCS_7_ASN_ENCODING, new IntPtr((int)CRYPT_OUTPUT_TYPES.X509_PUBLIC_KEY_INFO),
                        DERData, (UInt32)DERData.Length, CRYPT_DECODE_FLAGS.NONE, data, ref dwCertPublicKeyInfoSize))
                    {
                        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
                        try
                        {
                            info = (CERT_PUBLIC_KEY_INFO)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(CERT_PUBLIC_KEY_INFO));
                            publicKey = new byte[info.PublicKey.cbData];
                            Marshal.Copy(info.PublicKey.pbData, publicKey, 0, publicKey.Length);
                        }
                        finally
                        {
                            handle.Free();
                        }
                    }
                    else
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                }
                else
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                return publicKey;
            }

            /// <summary>Extracts the binary data from a PEM file.</summary>
            internal static byte[] GetDERFromPEM(string sPEM)
            {
                UInt32 dwSkip, dwFlags;
                UInt32 dwBinarySize = 0;

                if (!CryptStringToBinary(sPEM, (UInt32)sPEM.Length, CRYPT_STRING_FLAGS.CRYPT_STRING_BASE64HEADER, null, ref dwBinarySize, out dwSkip, out dwFlags))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                byte[] decodedData = new byte[dwBinarySize];
                if (!CryptStringToBinary(sPEM, (UInt32)sPEM.Length, CRYPT_STRING_FLAGS.CRYPT_STRING_BASE64HEADER, decodedData, ref dwBinarySize, out dwSkip, out dwFlags))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                return decodedData;
            }

            #endregion Methods

            #region P/Invoke Constants

            /// <summary>Enumeration derived from Crypto API.</summary>
            internal enum CRYPT_ACQUIRE_CONTEXT_FLAGS : uint
            {
                CRYPT_NEWKEYSET = 0x8,
                CRYPT_DELETEKEYSET = 0x10,
                CRYPT_MACHINE_KEYSET = 0x20,
                CRYPT_SILENT = 0x40,
                CRYPT_DEFAULT_CONTAINER_OPTIONAL = 0x80,
                CRYPT_VERIFYCONTEXT = 0xF0000000
            }

            /// <summary>Enumeration derived from Crypto API.</summary>
            internal enum CRYPT_PROVIDER_TYPE : uint
            {
                PROV_RSA_FULL = 1
            }

            /// <summary>Enumeration derived from Crypto API.</summary>
            internal enum CRYPT_DECODE_FLAGS : uint
            {
                NONE = 0,
                CRYPT_DECODE_ALLOC_FLAG = 0x8000
            }

            /// <summary>Enumeration derived from Crypto API.</summary>
            internal enum CRYPT_ENCODING_FLAGS : uint
            {
                PKCS_7_ASN_ENCODING = 0x00010000,
                X509_ASN_ENCODING = 0x00000001,
            }

            /// <summary>Enumeration derived from Crypto API.</summary>
            internal enum CRYPT_OUTPUT_TYPES : int
            {
                X509_PUBLIC_KEY_INFO = 8,
                RSA_CSP_PUBLICKEYBLOB = 19,
                PKCS_RSA_PRIVATE_KEY = 43,
                PKCS_PRIVATE_KEY_INFO = 44
            }

            /// <summary>Enumeration derived from Crypto API.</summary>
            internal enum CRYPT_STRING_FLAGS : uint
            {
                CRYPT_STRING_BASE64HEADER = 0,
                CRYPT_STRING_BASE64 = 1,
                CRYPT_STRING_BINARY = 2,
                CRYPT_STRING_BASE64REQUESTHEADER = 3,
                CRYPT_STRING_HEX = 4,
                CRYPT_STRING_HEXASCII = 5,
                CRYPT_STRING_BASE64_ANY = 6,
                CRYPT_STRING_ANY = 7,
                CRYPT_STRING_HEX_ANY = 8,
                CRYPT_STRING_BASE64X509CRLHEADER = 9,
                CRYPT_STRING_HEXADDR = 10,
                CRYPT_STRING_HEXASCIIADDR = 11,
                CRYPT_STRING_HEXRAW = 12,
                CRYPT_STRING_NOCRLF = 0x40000000,
                CRYPT_STRING_NOCR = 0x80000000
            }

            #endregion P/Invoke Constants

            #region P/Invoke Structures

            /// <summary>Structure from Crypto API.</summary>
            [StructLayout(LayoutKind.Sequential)]
            internal struct CRYPT_OBJID_BLOB
            {
                internal UInt32 cbData;
                internal IntPtr pbData;
            }

            /// <summary>Structure from Crypto API.</summary>
            [StructLayout(LayoutKind.Sequential)]
            internal struct CRYPT_ALGORITHM_IDENTIFIER
            {
                internal IntPtr pszObjId;
                internal CRYPT_OBJID_BLOB Parameters;
            }

            /// <summary>Structure from Crypto API.</summary>
            [StructLayout(LayoutKind.Sequential)]
            struct CRYPT_BIT_BLOB
            {
                internal UInt32 cbData;
                internal IntPtr pbData;
                internal UInt32 cUnusedBits;
            }

            /// <summary>Structure from Crypto API.</summary>
            [StructLayout(LayoutKind.Sequential)]
            struct CERT_PUBLIC_KEY_INFO
            {
                internal CRYPT_ALGORITHM_IDENTIFIER Algorithm;
                internal CRYPT_BIT_BLOB PublicKey;
            }

            #endregion P/Invoke Structures

            #region P/Invoke Functions

            /// <summary>Function for Crypto API.</summary>
            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CryptDestroyKey(IntPtr hKey);

            /// <summary>Function for Crypto API.</summary>
            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CryptImportKey(IntPtr hProv, byte[] pbKeyData, UInt32 dwDataLen, IntPtr hPubKey, UInt32 dwFlags, ref IntPtr hKey);

            /// <summary>Function for Crypto API.</summary>
            [DllImport("advapi32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CryptReleaseContext(IntPtr hProv, Int32 dwFlags);

            /// <summary>Function for Crypto API.</summary>
            [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CryptAcquireContext(ref IntPtr hProv, string pszContainer, string pszProvider, CRYPT_PROVIDER_TYPE dwProvType, CRYPT_ACQUIRE_CONTEXT_FLAGS dwFlags);

            /// <summary>Function from Crypto API.</summary>
            [DllImport("crypt32.dll", SetLastError = true, CharSet = CharSet.Auto)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CryptStringToBinary(string sPEM, UInt32 sPEMLength, CRYPT_STRING_FLAGS dwFlags, [Out] byte[] pbBinary, ref UInt32 pcbBinary, out UInt32 pdwSkip, out UInt32 pdwFlags);

            /// <summary>Function from Crypto API.</summary>
            [DllImport("crypt32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CryptDecodeObjectEx(CRYPT_ENCODING_FLAGS dwCertEncodingType, IntPtr lpszStructType, byte[] pbEncoded, UInt32 cbEncoded, CRYPT_DECODE_FLAGS dwFlags, IntPtr pDecodePara, ref byte[] pvStructInfo, ref UInt32 pcbStructInfo);

            /// <summary>Function from Crypto API.</summary>
            [DllImport("crypt32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool CryptDecodeObject(CRYPT_ENCODING_FLAGS dwCertEncodingType, IntPtr lpszStructType, byte[] pbEncoded, UInt32 cbEncoded, CRYPT_DECODE_FLAGS flags, [In, Out] byte[] pvStructInfo, ref UInt32 cbStructInfo);

            #endregion P/Invoke Functions
        }

    /************************************************************
   * 关于hashAlgorithm参数值有：MD5、SHA1、SHA256、SHA384、SHA512
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

    public abstract partial class RSAHelper
    {
        #region  加密


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

            //☆☆☆☆.NET 4.6以后特有☆☆☆☆
            //HashAlgorithmName hashName = new System.Security.Cryptography.HashAlgorithmName(hashAlgorithm);
            //RSAEncryptionPadding padding = RSAEncryptionPadding.OaepSHA512;//RSAEncryptionPadding.CreateOaep(hashName);//.NET 4.6以后特有               
            //cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), padding);
            //☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆

            //☆☆☆☆.NET 4.6以前请用此段代码☆☆☆☆
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

            //☆☆☆☆.NET 4.6以后特有☆☆☆☆
            //HashAlgorithmName hashName = new System.Security.Cryptography.HashAlgorithmName(hashAlgorithm);
            //RSAEncryptionPadding padding = RSAEncryptionPadding.OaepSHA512;//RSAEncryptionPadding.CreateOaep(hashName);//.NET 4.6以后特有               
            //cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), padding);
            //☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆

            //☆☆☆☆.NET 4.6以前请用此段代码☆☆☆☆
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

            //☆☆☆☆.NET 4.6以后特有☆☆☆☆
            //HashAlgorithmName hashName = new System.Security.Cryptography.HashAlgorithmName(hashAlgorithm);
            //RSAEncryptionPadding padding = RSAEncryptionPadding.OaepSHA512;//RSAEncryptionPadding.CreateOaep(hashName);//.NET 4.6以后特有               
            //cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), padding);
            //☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆

            //☆☆☆☆.NET 4.6以前请用此段代码☆☆☆☆
            cipherbytes = rsa.Encrypt(Encoding.GetEncoding(encoding).GetBytes(data), false);

            return Convert.ToBase64String(cipherbytes);
        }
        #endregion

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
            //☆☆☆☆.NET 4.6以后特有☆☆☆☆
            //RSAEncryptionPadding padding = RSAEncryptionPadding.CreateOaep(new System.Security.Cryptography.HashAlgorithmName(hashAlgorithm));//.NET 4.6以后特有        
            //cipherbytes = rsa.Decrypt(Encoding.GetEncoding(encoding).GetBytes(data), padding);
            //☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆

            //☆☆☆☆.NET 4.6以前请用此段代码☆☆☆☆
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
            //☆☆☆☆.NET 4.6以后特有☆☆☆☆
            //RSAEncryptionPadding padding = RSAEncryptionPadding.CreateOaep(new System.Security.Cryptography.HashAlgorithmName(hashAlgorithm));//.NET 4.6以后特有        
            //cipherbytes = rsa.Decrypt(Encoding.GetEncoding(encoding).GetBytes(data), padding);
            //☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆

            //☆☆☆☆.NET 4.6以前请用此段代码☆☆☆☆
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
            //☆☆☆☆.NET 4.6以后特有☆☆☆☆
            //RSAEncryptionPadding padding = RSAEncryptionPadding.CreateOaep(new System.Security.Cryptography.HashAlgorithmName(hashAlgorithm));//.NET 4.6以后特有        
            //cipherbytes = rsa.Decrypt(Encoding.GetEncoding(encoding).GetBytes(data), padding);
            //☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆☆

            //☆☆☆☆.NET 4.6以前请用此段代码☆☆☆☆
            cipherbytes = rsa.Decrypt(Convert.FromBase64String(data), false);

            return Encoding.GetEncoding(encoding).GetString(cipherbytes);
        }
        #endregion


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
            //RSAPKCS1SignatureFormatter RSAFormatter = new RSAPKCS1SignatureFormatter(rsa);
            ////设置签名的算法为MD5 MD5withRSA 签名
            //RSAFormatter.SetHashAlgorithm(hashAlgorithm);


            var dataBytes = Encoding.GetEncoding(encoding).GetBytes(data);
            var HashbyteSignature = rsa.SignData(dataBytes, hashAlgorithm);
            return Convert.ToBase64String(HashbyteSignature);

            //byte[] HashbyteSignature = ConvertToRgbHash(data, encoding);

            //byte[] dataBytes =Encoding.GetEncoding(encoding).GetBytes(data);
            //HashbyteSignature = rsa.SignData(dataBytes, hashAlgorithm);
            //return Convert.ToBase64String(HashbyteSignature);
            //执行签名 
            //EncryptedSignatureData = RSAFormatter.CreateSignature(HashbyteSignature);
            //return Convert.ToBase64String(RSAFormatter.CreateSignature(HashbyteSignature));
            //return result.Replace("=", String.Empty).Replace('+', '-').Replace('/', '_');
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

        #endregion

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

            //return SignatureDeformatter(publicKeyJava, data, signature);

            //return CheckSign(publicKeyJava, data, signature);

            //return rsa.VerifyData(Encoding.GetEncoding(encoding).GetBytes(data), "MD5", Encoding.GetEncoding(encoding).GetBytes(signature));
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

        #region 签名验证-方法二
        /// <summary>
        /// 签名验证
        /// </summary>
        /// <param name="publicKey">公钥</param>
        /// <param name="p_strHashbyteDeformatter">待验证的用户名</param>
        /// <param name="signature">注册码</param>
        /// <returns>签名是否符合</returns>
        public static bool SignatureDeformatter(string publicKey, string data, string signature, string hashAlgorithm = "MD5")
        {
            try
            {
                byte[] rgbHash = ConvertToRgbHash(data);
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                //导入公钥，准备验证签名
                rsa.FromPublicKeyJavaString(publicKey);

                RSAPKCS1SignatureDeformatter deformatter = new RSAPKCS1SignatureDeformatter(rsa);
                deformatter.SetHashAlgorithm("MD5");
                byte[] rgbSignature = Convert.FromBase64String(signature);
                if (deformatter.VerifySignature(rgbHash, rgbSignature))
                {
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 签名数据转化为RgbHash
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        public static byte[] ConvertToRgbHash(string data, string encoding = "UTF-8")
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes_md5_in = Encoding.GetEncoding(encoding).GetBytes(data);
                return md5.ComputeHash(bytes_md5_in);
            }
        }
        #endregion

        #region 签名验证-方法三
        /// <summary>
        /// 验证签名
        /// </summary>
        /// <param name="data">原始数据</param>
        /// <param name="sign">签名</param>
        /// <returns></returns>
        public static bool CheckSign(string publicKey, string data, string sign, string encoding = "UTF-8")
        {

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.FromPublicKeyJavaString(publicKey);
            MD5 md5 = MD5.Create();

            byte[] Data = Encoding.GetEncoding(encoding).GetBytes(data);
            byte[] rgbSignature = Convert.FromBase64String(sign);
            if (rsa.VerifyData(Data, md5, rgbSignature))
            {
                return true;
            }
            return false;
        }
        #endregion
        #endregion

    }
    /*******************************************************
     * 关于：此段代码专门针对“私钥加密,公钥解密。”
     * 私钥加密,公钥解密。
     * 私钥加密,公钥解密。
     * 私钥加密,公钥解密。
     * 公钥加密解密C#先天不支持。有些java的代码不按常理出牌。     * 
     * C#为什么不支持，问微软。（个人认为是安全性，公钥是谁都可以持有，私钥只有自己有）
     * 这里使用BouncyCastle第三方开源库从java移植过来的，兼容性不是问题。
     * 
     * gzy整理
     */
    public partial class RSAHelper
    {
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
        #endregion

        #region  公钥解密
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
        #endregion
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
            //return Encoding.GetEncoding(encoding).GetString(signer.GenerateSignature()); //签名结果 非Base64String
            return Convert.ToBase64String(signer.GenerateSignature());
        }
        #endregion
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
            //byte[] signatureByte = Encoding.GetEncoding(encoding).GetBytes(signature);// 非Base64String
            byte[] signatureByte = Convert.FromBase64String(signature);
            return signer.VerifySignature(signatureByte);
        }
        #endregion

    }
}
