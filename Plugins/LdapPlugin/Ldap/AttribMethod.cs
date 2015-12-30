/*
	Copyright (c) 2014, pGina Team
	All rights reserved.

	Redistribution and use in source and binary forms, with or without
	modification, are permitted provided that the following conditions are met:
		* Redistributions of source code must retain the above copyright
		  notice, this list of conditions and the following disclaimer.
		* Redistributions in binary form must reproduce the above copyright
		  notice, this list of conditions and the following disclaimer in the
		  documentation and/or other materials provided with the distribution.
		* Neither the name of the pGina Team nor the names of its contributors
		  may be used to endorse or promote products derived from this software without
		  specific prior written permission.

	THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
	ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
	WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
	DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY
	DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
	(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
	LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
	ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
	(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
	SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.IO;
using log4net;

namespace pGina.Plugin.Ldap
{
    enum Methods
    {
        PLAIN,
        MD5,
        SMD5,
        SHA1,
        SSHA1,
        NTLM,
        LM,
        Timestamps,
        Timestampd,
        Timestampt,
        SHA256,
        SSHA256,
        SHA384,
        SSHA384,
        SHA512,
        SSHA512,
        ADPWD,
    }

    class AttribConvert
    {
        public static readonly List<string> Attribs = new List<string>(new string[] {
        // the name must match properties in UserInformation
                "Fullname",
                "usri4_max_storage",
                "usri4_profile",
                "usri4_home_dir_drive",
                "usri4_home_dir",
                "Email",
                "LoginScript",
                "pgSMB_Filename",
                "pgSMB_SMBshare",
                "script_authe_sys",
                "script_autho_sys",
                "script_gateway_sys",
                "script_notification_sys",
                "script_notification_usr",
                "script_changepwd_sys",
                "script_changepwd_usr"
            });
    }

    abstract class AttribMethod
    {
        internal ILog m_logger = LogManager.GetLogger("LdapHash");

        public string Name { get { return m_name; } }
        protected string m_name;

        public Methods Method { get { return m_method; } }
        protected Methods m_method;

        public static Dictionary<Methods, AttribMethod> methods;

        static AttribMethod()
        {
            methods = new Dictionary<Methods, AttribMethod>();
            methods.Add(Methods.PLAIN, new PasswordHashMethodPlain());
            methods.Add(Methods.MD5, new PasswordHashMethodMD5());
            methods.Add(Methods.SMD5, new PasswordHashMethodSMD5());
            methods.Add(Methods.SHA1, new PasswordHashMethodSHA1());
            methods.Add(Methods.SSHA1, new PasswordHashMethodSSHA1());
            methods.Add(Methods.SHA256, new PasswordHashMethodSHA256());
            methods.Add(Methods.SSHA256, new PasswordHashMethodSSHA256());
            methods.Add(Methods.SHA384, new PasswordHashMethodSHA384());
            methods.Add(Methods.SSHA384, new PasswordHashMethodSSHA384());
            methods.Add(Methods.SHA512, new PasswordHashMethodSHA512());
            methods.Add(Methods.SSHA512, new PasswordHashMethodSSHA512());
            methods.Add(Methods.NTLM, new PasswordHashMethodNTLM());
            methods.Add(Methods.LM, new PasswordHashMethodLM());
            methods.Add(Methods.ADPWD, new PasswordADPWD());
        }

        public abstract string hash(string pw);

        protected byte[] GenerateSalt(uint length)
        {
            byte[] salt = new byte[length];
            try
            {
                using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
                {
                    crypto.GetBytes(salt);
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("GenerateSalt Error:{0}", ex.Message);
            }
            return salt;
        }

        protected byte[] Appendbytes(byte[] source, byte[] append)
        {
            byte[] combine = new byte[source.Length + append.Length];
            try
            {
                Buffer.BlockCopy(source, 0, combine, 0, source.Length);
                Buffer.BlockCopy(append, 0, combine, source.Length, append.Length);
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("Appendbytes Error:{0}", ex.Message);
            }
            return combine;
        }
    }

    abstract class TimeMethod
    {
        internal ILog m_logger = LogManager.GetLogger("LdapTime");

        public string Name { get { return m_name; } }
        protected string m_name;

        public Methods Method { get { return m_method; } }
        protected Methods m_method;

        public static Dictionary<Methods, TimeMethod> methods;

        static TimeMethod()
        {
            methods = new Dictionary<Methods, TimeMethod>();
            methods.Add(Methods.Timestamps, new PasswordTimestampUNIX());
            methods.Add(Methods.Timestampd, new PasswordTimestampSHADOW());
            methods.Add(Methods.Timestampt, new PasswordTimestampNT());
        }

        public abstract string time();

        public abstract string time(TimeSpan add);
    }

    class PasswordADPWD : AttribMethod
    {
        public PasswordADPWD()
        {
            m_name = "Select to change an AD password";
            m_method = Methods.ADPWD;
        }

        public override string hash(string pw)
        {
            return pw;
        }
    }

    class PasswordHashMethodPlain : AttribMethod
    {
        public PasswordHashMethodPlain()
        {
            m_name = "Plain Text";
            m_method = Methods.PLAIN;
        }

        public override string hash(string pw)
        {
            return pw;
        }
    }

    class PasswordHashMethodMD5 : AttribMethod
    {
        public PasswordHashMethodMD5()
        {
            m_name = "MD5";
            m_method = Methods.MD5;
        }

        public override string hash(string password)
        {
            byte[] md5 = null;

            try
            {
                using (MD5 algorithm = new MD5CryptoServiceProvider())
                {
                    md5 = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("MD5 Error:", ex.Message);
            }

            return "{MD5}" + Convert.ToBase64String(md5);
        }
    }

    class PasswordHashMethodSMD5 : AttribMethod
    {
        public PasswordHashMethodSMD5()
        {
            m_name = "SMD5 (Salted MD5)";
            m_method = Methods.SMD5;
        }

        public override string hash(string password)
        {
            byte[] smd5 = null;
            byte[] salt = GenerateSalt(16);

            try
            {
                using (MD5 algorithm = new MD5CryptoServiceProvider())
                {
                    byte[] combine = Appendbytes(Encoding.UTF8.GetBytes(password), salt);
                    smd5 = algorithm.ComputeHash(combine);
                    smd5 = Appendbytes(smd5, salt);
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("SMD5 Error:", ex.Message);
            }

            return "{SMD5}" + Convert.ToBase64String(smd5);
        }
    }

    class PasswordHashMethodSHA1 : AttribMethod
    {
        public PasswordHashMethodSHA1()
        {
            m_name = "SHA1";
            m_method = Methods.SHA1;
        }

        public override string hash(string password)
        {
            byte[] sha = null;

            try
            {
                using (HashAlgorithm algorithm = new SHA1Managed())
                {
                    sha = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("SHA Error:", ex.Message);
            }

            return "{SHA}" + Convert.ToBase64String(sha);
        }
    }

    class PasswordHashMethodSSHA1 : AttribMethod
    {
        public PasswordHashMethodSSHA1()
        {
            m_name = "SSHA1 (Salted SHA1)";
            m_method = Methods.SSHA1;
        }

        public override string hash(string password)
        {
            byte[] ssha = null;
            byte[] salt = GenerateSalt(16);

            try
            {
                using (HashAlgorithm algorithm = new SHA1Managed())
                {
                    byte[] combine = Appendbytes(Encoding.UTF8.GetBytes(password), salt);
                    ssha = algorithm.ComputeHash(combine);
                    ssha = Appendbytes(ssha, salt);
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("SSHA Error:", ex.Message);
            }

            return "{SSHA}" + Convert.ToBase64String(ssha);
        }
    }

    class PasswordHashMethodSHA256 : AttribMethod
    {
        public PasswordHashMethodSHA256()
        {
            m_name = "SHA256";
            m_method = Methods.SHA256;
        }

        public override string hash(string password)
        {
            byte[] sha = null;

            try
            {
                using (HashAlgorithm algorithm = new SHA256Managed())
                {
                    sha = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("SHA Error:", ex.Message);
            }

            return "{SHA256}" + Convert.ToBase64String(sha);
        }
    }

    class PasswordHashMethodSSHA256 : AttribMethod
    {
        public PasswordHashMethodSSHA256()
        {
            m_name = "SSHA256 (Salted SHA256)";
            m_method = Methods.SSHA256;
        }

        public override string hash(string password)
        {
            byte[] ssha = null;
            byte[] salt = GenerateSalt(16);

            try
            {
                using (HashAlgorithm algorithm = new SHA256Managed())
                {
                    byte[] combine = Appendbytes(Encoding.UTF8.GetBytes(password), salt);
                    ssha = algorithm.ComputeHash(combine);
                    ssha = Appendbytes(ssha, salt);
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("SSHA Error:", ex.Message);
            }

            return "{SSHA256}" + Convert.ToBase64String(ssha);
        }
    }

    class PasswordHashMethodSHA384 : AttribMethod
    {
        public PasswordHashMethodSHA384()
        {
            m_name = "SHA384";
            m_method = Methods.SHA384;
        }

        public override string hash(string password)
        {
            byte[] sha = null;

            try
            {
                using (HashAlgorithm algorithm = new SHA384Managed())
                {
                    sha = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("SHA Error:", ex.Message);
            }

            return "{SHA384}" + Convert.ToBase64String(sha);
        }
    }

    class PasswordHashMethodSSHA384 : AttribMethod
    {
        public PasswordHashMethodSSHA384()
        {
            m_name = "SSHA384 (Salted SHA384)";
            m_method = Methods.SSHA384;
        }

        public override string hash(string password)
        {
            byte[] ssha = null;
            byte[] salt = GenerateSalt(16);

            try
            {
                using (HashAlgorithm algorithm = new SHA384Managed())
                {
                    byte[] combine = Appendbytes(Encoding.UTF8.GetBytes(password), salt);
                    ssha = algorithm.ComputeHash(combine);
                    ssha = Appendbytes(ssha, salt);
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("SSHA Error:", ex.Message);
            }

            return "{SSHA384}" + Convert.ToBase64String(ssha);
        }
    }

    class PasswordHashMethodSHA512 : AttribMethod
    {
        public PasswordHashMethodSHA512()
        {
            m_name = "SHA512";
            m_method = Methods.SHA512;
        }

        public override string hash(string password)
        {
            byte[] sha = null;

            try
            {
                using (HashAlgorithm algorithm = new SHA512Managed())
                {
                    sha = algorithm.ComputeHash(Encoding.UTF8.GetBytes(password));
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("SHA Error:", ex.Message);
            }

            return "{SHA512}" + Convert.ToBase64String(sha);
        }
    }

    class PasswordHashMethodSSHA512 : AttribMethod
    {
        public PasswordHashMethodSSHA512()
        {
            m_name = "SSHA512 (Salted SHA512)";
            m_method = Methods.SSHA512;
        }

        public override string hash(string password)
        {
            byte[] ssha = null;
            byte[] salt = GenerateSalt(16);

            try
            {
                using (HashAlgorithm algorithm = new SHA512Managed())
                {
                    byte[] combine = Appendbytes(Encoding.UTF8.GetBytes(password), salt);
                    ssha = algorithm.ComputeHash(combine);
                    ssha = Appendbytes(ssha, salt);
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("SSHA Error:", ex.Message);
            }

            return "{SSHA512}" + Convert.ToBase64String(ssha);
        }
    }

    class PasswordHashMethodNTLM : AttribMethod
    {
        #region advapi32.dll
        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CryptAcquireContext(ref IntPtr hProv, string pszContainer, string pszProvider, uint dwProvType, uint dwFlags);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool CryptCreateHash(IntPtr hProv, uint algId, IntPtr hKey, uint dwFlags, ref IntPtr phHash);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CryptHashData(IntPtr hHash, byte[] pbData, uint dataLen, uint flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool CryptGetHashParam(IntPtr hHash, Int32 dwParam, Byte[] pbData, ref Int32 pdwDataLen, Int32 dwFlags);

        [DllImport("Advapi32.dll", EntryPoint = "CryptReleaseContext", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool CryptReleaseContext(IntPtr hProv, Int32 dwFlags /* Reserved. Must be 0*/ );

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool CryptDestroyHash(IntPtr hHash);
        #endregion

        #region Structs/Enums

        /// <summary>
        /// Source: WinCrypt.h
        /// </summary>
        enum HashParameters
        {
            HP_ALGID = 0x0001,   // Hash algorithm
            HP_HASHVAL = 0x0002, // Hash value
            HP_HASHSIZE = 0x0004 // Hash value size
        }

        /// <summary>
        /// Source: WinCrypt.h
        /// </summary>
        public enum CryptAlgClass : uint
        {
            ALG_CLASS_ANY = (0),
            ALG_CLASS_SIGNATURE = (1 << 13),
            ALG_CLASS_MSG_ENCRYPT = (2 << 13),
            ALG_CLASS_DATA_ENCRYPT = (3 << 13),
            ALG_CLASS_HASH = (4 << 13),
            ALG_CLASS_KEY_EXCHANGE = (5 << 13),
            ALG_CLASS_ALL = (7 << 13)
        }

        /// <summary>
        /// Source: WinCrypt.h
        /// </summary>
        public enum CryptAlgType : uint
        {
            ALG_TYPE_ANY = (0),
            ALG_TYPE_DSS = (1 << 9),
            ALG_TYPE_RSA = (2 << 9),
            ALG_TYPE_BLOCK = (3 << 9),
            ALG_TYPE_STREAM = (4 << 9),
            ALG_TYPE_DH = (5 << 9),
            ALG_TYPE_SECURECHANNEL = (6 << 9)
        }

        /// <summary>
        /// Source: WinCrypt.h
        /// </summary>
        public enum CryptAlgSID : uint
        {
            ALG_SID_ANY = (0),
            ALG_SID_RSA_ANY = 0,
            ALG_SID_RSA_PKCS = 1,
            ALG_SID_RSA_MSATWORK = 2,
            ALG_SID_RSA_ENTRUST = 3,
            ALG_SID_RSA_PGP = 4,
            ALG_SID_DSS_ANY = 0,
            ALG_SID_DSS_PKCS = 1,
            ALG_SID_DSS_DMS = 2,
            ALG_SID_ECDSA = 3,
            ALG_SID_DES = 1,
            ALG_SID_3DES = 3,
            ALG_SID_DESX = 4,
            ALG_SID_IDEA = 5,
            ALG_SID_CAST = 6,
            ALG_SID_SAFERSK64 = 7,
            ALG_SID_SAFERSK128 = 8,
            ALG_SID_3DES_112 = 9,
            ALG_SID_CYLINK_MEK = 12,
            ALG_SID_RC5 = 13,
            ALG_SID_AES_128 = 14,
            ALG_SID_AES_192 = 15,
            ALG_SID_AES_256 = 16,
            ALG_SID_AES = 17,
            ALG_SID_SKIPJACK = 10,
            ALG_SID_TEK = 11,
            ALG_SID_RC2 = 2,
            ALG_SID_RC4 = 1,
            ALG_SID_SEAL = 2,
            ALG_SID_DH_SANDF = 1,
            ALG_SID_DH_EPHEM = 2,
            ALG_SID_AGREED_KEY_ANY = 3,
            ALG_SID_KEA = 4,
            ALG_SID_ECDH = 5,
            ALG_SID_MD2 = 1,
            ALG_SID_MD4 = 2,
            ALG_SID_MD5 = 3,
            ALG_SID_SHA = 4,
            ALG_SID_SHA1 = 4,
            ALG_SID_MAC = 5,
            ALG_SID_RIPEMD = 6,
            ALG_SID_RIPEMD160 = 7,
            ALG_SID_SSL3SHAMD5 = 8,
            ALG_SID_HMAC = 9,
            ALG_SID_TLS1PRF = 10,
            ALG_SID_HASH_REPLACE_OWF = 11,
            ALG_SID_SHA_256 = 12,
            ALG_SID_SHA_384 = 13,
            ALG_SID_SHA_512 = 14,
            ALG_SID_SSL3_MASTER = 1,
            ALG_SID_SCHANNEL_MASTER_HASH = 2,
            ALG_SID_SCHANNEL_MAC_KEY = 3,
            ALG_SID_PCT1_MASTER = 4,
            ALG_SID_SSL2_MASTER = 5,
            ALG_SID_TLS1_MASTER = 6,
            ALG_SID_SCHANNEL_ENC_KEY = 7,
            ALG_SID_ECMQV = 1
        }

        /// <summary>
        /// Source: WinCrypt.h
        /// </summary>
        public enum CryptAlg : uint
        {
            CALG_MD2 = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_MD2),
            CALG_MD4 = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_MD4),
            CALG_MD5 = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_MD5),
            CALG_SHA = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_SHA),
            CALG_SHA1 = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_SHA1),
            CALG_MAC = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_MAC),
            CALG_RSA_SIGN = (CryptAlgClass.ALG_CLASS_SIGNATURE | CryptAlgType.ALG_TYPE_RSA | CryptAlgSID.ALG_SID_RSA_ANY),
            CALG_DSS_SIGN = (CryptAlgClass.ALG_CLASS_SIGNATURE | CryptAlgType.ALG_TYPE_DSS | CryptAlgSID.ALG_SID_DSS_ANY),
            CALG_NO_SIGN = (CryptAlgClass.ALG_CLASS_SIGNATURE | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_ANY),
            CALG_RSA_KEYX = (CryptAlgClass.ALG_CLASS_KEY_EXCHANGE | CryptAlgType.ALG_TYPE_RSA | CryptAlgSID.ALG_SID_RSA_ANY),
            CALG_DES = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_DES),
            CALG_3DES_112 = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_3DES_112),
            CALG_3DES = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_3DES),
            CALG_DESX = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_DESX),
            CALG_RC2 = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_RC2),
            CALG_RC4 = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_STREAM | CryptAlgSID.ALG_SID_RC4),
            CALG_SEAL = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_STREAM | CryptAlgSID.ALG_SID_SEAL),
            CALG_DH_SF = (CryptAlgClass.ALG_CLASS_KEY_EXCHANGE | CryptAlgType.ALG_TYPE_DH | CryptAlgSID.ALG_SID_DH_SANDF),
            CALG_DH_EPHEM = (CryptAlgClass.ALG_CLASS_KEY_EXCHANGE | CryptAlgType.ALG_TYPE_DH | CryptAlgSID.ALG_SID_DH_EPHEM),
            CALG_AGREEDKEY_ANY = (CryptAlgClass.ALG_CLASS_KEY_EXCHANGE | CryptAlgType.ALG_TYPE_DH | CryptAlgSID.ALG_SID_AGREED_KEY_ANY),
            CALG_KEA_KEYX = (CryptAlgClass.ALG_CLASS_KEY_EXCHANGE | CryptAlgType.ALG_TYPE_DH | CryptAlgSID.ALG_SID_KEA),
            CALG_HUGHES_MD5 = (CryptAlgClass.ALG_CLASS_KEY_EXCHANGE | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_MD5),
            CALG_SKIPJACK = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_SKIPJACK),
            CALG_TEK = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_TEK),
            CALG_CYLINK_MEK = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_CYLINK_MEK),
            CALG_SSL3_SHAMD5 = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_SSL3SHAMD5),
            CALG_SSL3_MASTER = (CryptAlgClass.ALG_CLASS_MSG_ENCRYPT | CryptAlgType.ALG_TYPE_SECURECHANNEL | CryptAlgSID.ALG_SID_SSL3_MASTER),
            CALG_SCHANNEL_MASTER_HASH = (CryptAlgClass.ALG_CLASS_MSG_ENCRYPT | CryptAlgType.ALG_TYPE_SECURECHANNEL | CryptAlgSID.ALG_SID_SCHANNEL_MASTER_HASH),
            CALG_SCHANNEL_MAC_KEY = (CryptAlgClass.ALG_CLASS_MSG_ENCRYPT | CryptAlgType.ALG_TYPE_SECURECHANNEL | CryptAlgSID.ALG_SID_SCHANNEL_MAC_KEY),
            CALG_SCHANNEL_ENC_KEY = (CryptAlgClass.ALG_CLASS_MSG_ENCRYPT | CryptAlgType.ALG_TYPE_SECURECHANNEL | CryptAlgSID.ALG_SID_SCHANNEL_ENC_KEY),
            CALG_PCT1_MASTER = (CryptAlgClass.ALG_CLASS_MSG_ENCRYPT | CryptAlgType.ALG_TYPE_SECURECHANNEL | CryptAlgSID.ALG_SID_PCT1_MASTER),
            CALG_SSL2_MASTER = (CryptAlgClass.ALG_CLASS_MSG_ENCRYPT | CryptAlgType.ALG_TYPE_SECURECHANNEL | CryptAlgSID.ALG_SID_SSL2_MASTER),
            CALG_TLS1_MASTER = (CryptAlgClass.ALG_CLASS_MSG_ENCRYPT | CryptAlgType.ALG_TYPE_SECURECHANNEL | CryptAlgSID.ALG_SID_TLS1_MASTER),
            CALG_RC5 = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_RC5),
            CALG_HMAC = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_HMAC),
            CALG_TLS1PRF = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_TLS1PRF),
            CALG_HASH_REPLACE_OWF = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_HASH_REPLACE_OWF),
            CALG_AES_128 = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_AES_128),
            CALG_AES_192 = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_AES_192),
            CALG_AES_256 = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_AES_256),
            CALG_AES = (CryptAlgClass.ALG_CLASS_DATA_ENCRYPT | CryptAlgType.ALG_TYPE_BLOCK | CryptAlgSID.ALG_SID_AES),
            CALG_SHA_256 = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_SHA_256),
            CALG_SHA_384 = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_SHA_384),
            CALG_SHA_512 = (CryptAlgClass.ALG_CLASS_HASH | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_SHA_512),
            CALG_ECDH = (CryptAlgClass.ALG_CLASS_KEY_EXCHANGE | CryptAlgType.ALG_TYPE_DH | CryptAlgSID.ALG_SID_ECDH),
            CALG_ECMQV = (CryptAlgClass.ALG_CLASS_KEY_EXCHANGE | CryptAlgType.ALG_TYPE_ANY | CryptAlgSID.ALG_SID_ECMQV),
            CALG_ECDSA = (CryptAlgClass.ALG_CLASS_SIGNATURE | CryptAlgType.ALG_TYPE_DSS | CryptAlgSID.ALG_SID_ECDSA)
        }

        /// <summary>
        /// Source: WinCrypt.h
        /// </summary>
        public enum CryptProvType : uint
        {
            PROV_RSA_FULL = 1,
            PROV_RSA_SIG = 2,
            PROV_DSS = 3,
            PROV_FORTEZZA = 4,
            PROV_MS_EXCHANGE = 5,
            PROV_SSL = 6,
            PROV_RSA_SCHANNEL = 12,
            PROV_DSS_DH = 13,
            PROV_EC_ECDSA_SIG = 14,
            PROV_EC_ECNRA_SIG = 15,
            PROV_EC_ECDSA_FULL = 16,
            PROV_EC_ECNRA_FULL = 17,
            PROV_DH_SCHANNEL = 18,
            PROV_SPYRUS_LYNKS = 20,
            PROV_RNG = 21,
            PROV_INTEL_SEC = 22,
            PROV_REPLACE_OWF = 23,
            PROV_RSA_AES = 24
        }

        /// <summary>
        /// Source: WinCrypt.h
        /// </summary>
        public enum CryptContext : uint
        {
            CRYPT_VERIFYCONTEXT = 0xF0000000,
            CRYPT_NEWKEYSET = 0x00000008,
            CRYPT_DELETEKEYSET = 0x00000010,
            CRYPT_MACHINE_KEYSET = 0x00000020,
            CRYPT_SILENT = 0x00000040,
            CRYPT_DEFAULT_CONTAINER_OPTIONAL = 0x00000080
        }
        #endregion

        public PasswordHashMethodNTLM()
        {
            m_name = "NTLM Hash (sambaNTPassword, MD4)";
            m_method = Methods.NTLM;
        }

        public override string hash(string password)
        {
            IntPtr lHCryptprov = IntPtr.Zero;
            IntPtr lHHash = IntPtr.Zero;
            byte[] hdata = new byte[16];
            int length = 16;
            byte[] pass = Encoding.Unicode.GetBytes(password);

            try
            {
                if (CryptAcquireContext(ref lHCryptprov, null, null, (uint)CryptProvType.PROV_RSA_AES, (uint)CryptContext.CRYPT_VERIFYCONTEXT))
                {
                    if (CryptCreateHash(lHCryptprov, (uint)CryptAlg.CALG_MD4, IntPtr.Zero, 0, ref lHHash))
                    {
                        if (CryptHashData(lHHash, pass, (uint)pass.Length, 0))
                        {
                            if (!CryptGetHashParam(lHHash, (int)HashParameters.HP_HASHVAL, hdata, ref length, 0))
                            {
                                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                                m_logger.FatalFormat("CryptGetHashParam Error:{0}", errorMessage);
                            }
                        }
                        else
                        {
                            string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                            m_logger.FatalFormat("CryptHashData Error:{0}", errorMessage);
                        }
                    }
                    else
                    {
                        string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                        m_logger.FatalFormat("CryptCreateHash Error:{0}", errorMessage);
                    }
                }
                else
                {
                    string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).ToString();
                    m_logger.FatalFormat("CryptAcquireContext Error:{0}", errorMessage);
                }
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat(ex.Message);
            }
            finally
            {
                if (lHHash != IntPtr.Zero)
                    CryptDestroyHash(lHHash);
                if (lHCryptprov != IntPtr.Zero)
                    CryptReleaseContext(lHCryptprov, 0);
            }

            return BitConverter.ToString(hdata).Replace("-", "");
        }
    }

    class PasswordHashMethodLM : AttribMethod
    {
        public PasswordHashMethodLM()
        {
            m_name = "LM Hash (sambaLMPassword, DES)";
            m_method = Methods.LM;
        }

        public override string hash(string password)
        {
            string hash = null;
            password = password.ToUpper();
            if (password.Length > 14)
                password = password.Substring(0, 14);

            try
            {
                hash = BitConverter.ToString(LMhash.Compute(password)).Replace("-", "");
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("Compute Error:{0}", ex.Message);
            }

            return hash;
        }
    }

    class PasswordTimestampUNIX : TimeMethod
    {
        public PasswordTimestampUNIX()
        {
            m_name = "Timestamp sec since '70";
            m_method = Methods.Timestamps;
        }

        public override string time()
        {
            TimeSpan span = new TimeSpan();

            try
            {
                span = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0));
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("PasswordTimestampUNIX Error:{0}", ex.Message);
            }

            return Convert.ToInt32(span.TotalSeconds).ToString();
        }

        public override string time(TimeSpan add)
        {
            TimeSpan span = new TimeSpan();

            try
            {
                span = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0));
                span += add;

            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("PasswordTimestampUNIX Error:{0}", ex.Message);
            }

            return Convert.ToInt32(span.TotalSeconds).ToString();
        }
    }

    class PasswordTimestampSHADOW : TimeMethod
    {
        public PasswordTimestampSHADOW()
        {
            m_name = "Timestamp days since '70";
            m_method = Methods.Timestampd;
        }

        public override string time()
        {
            TimeSpan span = new TimeSpan();

            try
            {
                span = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0));
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("PasswordTimestampSHADOW Error:{0}", ex.Message);
            }

            return Convert.ToInt32(span.TotalDays).ToString();
        }

        public override string time(TimeSpan add)
        {
            TimeSpan span = new TimeSpan();

            try
            {
                span = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0));
                span += add;
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("PasswordTimestampSHADOW Error:{0}", ex.Message);
            }

            return Convert.ToInt32(span.TotalDays).ToString();
        }
    }

    class PasswordTimestampNT : TimeMethod
    {
        public PasswordTimestampNT()
        {
            m_name = "Timestamp ticks since 1601";
            m_method = Methods.Timestampt;
        }

        public override string time()
        {
            TimeSpan span = new TimeSpan();

            try
            {
                span = (DateTime.UtcNow - new DateTime(1601, 1, 1, 0, 0, 0, 0));
            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("PasswordTimestampNT Error:{0}", ex.Message);
            }

            return Convert.ToInt64(span.Ticks).ToString();
        }

        public override string time(TimeSpan add)
        {
            TimeSpan span = new TimeSpan();

            try
            {
                span = (DateTime.UtcNow - new DateTime(1601, 1, 1, 0, 0, 0, 0));
                span += add;

            }
            catch (Exception ex)
            {
                m_logger.FatalFormat("PasswordTimestampNT Error:{0}", ex.Message);
            }

            return Convert.ToInt64(span.Ticks).ToString();
        }
    }
}
