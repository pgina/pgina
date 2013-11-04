/*
	Copyright (c) 2013, pGina Team
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
using Org.BouncyCastle.Crypto.Digests;

namespace pGina.Plugin.Ldap
{
    enum HashMethod
    {
        PLAIN,
        SHA1,
        SSHA1,
        NT_HASH,
    }

    abstract class PasswordHashMethod
    {
        public string Name { get { return m_name; } }
        protected string m_name;

        public HashMethod Method { get { return m_method; } }
        protected HashMethod m_method;

        private static Random rand = new Random();

        public static Dictionary<HashMethod, PasswordHashMethod> methods;

        static PasswordHashMethod()
        {
            methods = new Dictionary<HashMethod, PasswordHashMethod>();
            methods.Add(HashMethod.PLAIN, new PasswordHashMethodPlain());
            methods.Add(HashMethod.SHA1, new PasswordHashMethodSHA1());
            methods.Add(HashMethod.SSHA1, new PasswordHashMethodSSHA1());
            methods.Add(HashMethod.NT_HASH, new PasswordHashMethodNTHash());
            
        }

        public abstract string hash(string pw);

        protected byte[] randomSalt(int len)
        {
            byte[] salt = new byte[len];

            for (int i = 0; i < len; i++)
            {
                salt[i] = (byte)rand.Next(256);
            }
            return salt;
        }
    }

    class PasswordHashMethodPlain : PasswordHashMethod
    {
        public PasswordHashMethodPlain()
        {
            m_name = "Plain Text";
            m_method = HashMethod.PLAIN;
        }

        public override string hash(string pw)
        {
            return pw;
        }
    }

    class PasswordHashMethodSHA1 : PasswordHashMethod
    {
        public PasswordHashMethodSHA1()
        {
            m_name = "SHA1";
            m_method = HashMethod.SHA1;
        }

        public override string hash(string pw)
        {
            Sha1Digest digest = new Sha1Digest();

            byte[] password = Encoding.UTF8.GetBytes(pw);
            digest.BlockUpdate(password, 0, password.Length);
            int digestSize = digest.GetDigestSize();
            byte[] hashData = new byte[digestSize];
            digest.DoFinal(hashData, 0);

            return "{SHA}" + Convert.ToBase64String(hashData);
        }
    }

    class PasswordHashMethodSSHA1 : PasswordHashMethod
    {
        public PasswordHashMethodSSHA1()
        {
            m_name = "SSHA1 (Salted SHA1)";
            m_method = HashMethod.SSHA1;
        }

        public override string hash(string pw)
        {
            Sha1Digest digest = new Sha1Digest();

            // 8 byte random salt
            byte[] salt = randomSalt(8);

            // Generate the hash of the password + salt
            byte[] password = Encoding.UTF8.GetBytes(pw);
            digest.BlockUpdate(password, 0, password.Length);
            digest.BlockUpdate(salt, 0, salt.Length);
            int digestSize = digest.GetDigestSize();
            byte[] hashData = new byte[digestSize];
            digest.DoFinal(hashData, 0);

            // Put the salt on the end of the hashed password + salt
            byte[] result = new byte[hashData.Length + salt.Length];
            hashData.CopyTo(result, 0);
            salt.CopyTo(result, hashData.Length);

            // Return in base 64
            return "{SSHA}" + Convert.ToBase64String(result);
        }
    }

    class PasswordHashMethodNTHash : PasswordHashMethod
    {
        public PasswordHashMethodNTHash()
        {
            m_name = "NT Hash (sambaNTPassword, MD4)";
            m_method = HashMethod.NT_HASH;
        }

        public override string hash(string pw)
        {
            MD4Digest digest = new MD4Digest();

            // We want Unicode here (UTF-16)
            byte[] password = Encoding.Unicode.GetBytes(pw);

            digest.BlockUpdate(password, 0, password.Length);
            int digestSize = digest.GetDigestSize();
            byte[] hashData = new byte[digestSize];
            digest.DoFinal(hashData, 0);

            // Convert to HEX and return
            return string.Concat(hashData.Select(b => b.ToString("X2")));
        }
    }
}
