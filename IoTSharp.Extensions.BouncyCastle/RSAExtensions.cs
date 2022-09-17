using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.ComponentModel;

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
}
