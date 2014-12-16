/*
	Copyright (c) 2011, pGina Team
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
using System.Security.Cryptography;

using log4net;

namespace pGina.Plugin.MySQLAuth
{
    enum PasswordHashAlgorithm
    {
        NONE, MD5, SHA1, SHA256, SHA384, SHA512, SMD5, SSHA1, SSHA256, SSHA384, SSHA512
    }

    class UserEntry
    {
        private ILog m_logger = LogManager.GetLogger("MySQLAuth.UserEntry");

        private string m_hashedPass;
        private PasswordHashAlgorithm m_hashAlg;
        private string m_name;
        private byte[] m_passBytes;

        public string Name { get { return m_name; } }
        public PasswordHashAlgorithm HashAlg { get { return m_hashAlg; } }
        private string HashedPassword { get { return m_hashedPass; } }

        public UserEntry(string uname, PasswordHashAlgorithm alg, string hashedPass)
        {
            m_name = uname;
            m_hashAlg = alg;
            m_hashedPass = hashedPass;
            if (m_hashAlg != PasswordHashAlgorithm.NONE)
                m_passBytes = this.Decode(m_hashedPass);
            else
                m_passBytes = null;
        }

        private byte[] Decode( string hash )
        {
            int encInt = Settings.Store.HashEncoding;
            Settings.HashEncoding encoding = (Settings.HashEncoding)encInt;
            if (encoding == Settings.HashEncoding.HEX)
                return FromHexString(hash);
            else if (encoding == Settings.HashEncoding.BASE_64)
                return Convert.FromBase64String(hash);
            else
            {
                m_logger.ErrorFormat("Unrecognized hash encoding!  This shouldn't happen.");
                throw new Exception("Unrecognized hash encoding.");
            }
        }

        public bool VerifyPassword( string plainText )
        {
            if (plainText != null)
            {
                // If hash algorithm is NONE, just compare the strings
                if (HashAlg == PasswordHashAlgorithm.NONE)
                    return plainText.Equals(HashedPassword);

                // Is it a salted hash?
                if (HashAlg == PasswordHashAlgorithm.SMD5 ||
                    HashAlg == PasswordHashAlgorithm.SSHA1 ||
                    HashAlg == PasswordHashAlgorithm.SSHA256 ||
                    HashAlg == PasswordHashAlgorithm.SSHA384 ||
                    HashAlg == PasswordHashAlgorithm.SSHA512)
                {
                    return VerifySaltedPassword(plainText);
                }

                // If we're here, we have an unsalted hash to compare with, hash and compare
                // the hashed bytes.
                byte[] hashedPlainText = HashPlainText(plainText);
                if (hashedPlainText != null)
                    return hashedPlainText.SequenceEqual(m_passBytes);
                else
                    return false;
            }
            else
            {
                return false;
            }
        }

        private bool VerifySaltedPassword(string plainText)
        {
            using (HashAlgorithm hasher = GetHasher())
            {
                if (hasher != null)
                {
                    if (hasher.HashSize % 8 != 0)
                        m_logger.ErrorFormat("WARNING: hash size is not a multiple of 8.  Hashes may not be evaluated correctly!");

                    int hashSizeBytes = hasher.HashSize / 8;

                    if( m_passBytes.Length > hashSizeBytes )
                    {
                        // Get the salt
                        byte[] salt = new byte[m_passBytes.Length - hashSizeBytes];
                        Array.Copy(m_passBytes, hashSizeBytes, salt, 0, salt.Length);
                        m_logger.DebugFormat("Found {1} byte salt: [{0}]", string.Join(",", salt), salt.Length);
                        
                        // Get the hash
                        byte[] hashedPassAndSalt = new byte[hashSizeBytes];
                        Array.Copy(m_passBytes, 0, hashedPassAndSalt, 0, hashSizeBytes);

                        // Build an array with the plain text and the salt
                        byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
                        byte[] plainTextAndSalt = new byte[salt.Length + plainTextBytes.Length];
                        plainTextBytes.CopyTo(plainTextAndSalt, 0);
                        salt.CopyTo(plainTextAndSalt, plainTextBytes.Length);

                        // Compare the byte arrays
                        byte[] hashedPlainTextAndSalt = hasher.ComputeHash(plainTextAndSalt);
                        return hashedPlainTextAndSalt.SequenceEqual(hashedPassAndSalt);
                    }
                    else
                    {
                        m_logger.ErrorFormat("Found hash of length {0}, expected at least {1} bytes.",
                            m_passBytes.Length, hashSizeBytes);
                        throw new Exception("Hash length is too short, no salt found.");
                    }
                }
            }

            return false;
        }

        private byte[] HashPlainText( string plainText )
        {
            if (HashAlg == PasswordHashAlgorithm.NONE)
                throw new Exception("Tried to hash a password when algorithm is NONE.");

            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            byte[] result = null;
            using (HashAlgorithm hasher = GetHasher())
            {
                if( hasher != null )
                    result = hasher.ComputeHash(bytes);
            }

            return result;
        }

        private HashAlgorithm GetHasher()
        {
            switch (HashAlg)
            {
                case PasswordHashAlgorithm.NONE:
                    return null;
                case PasswordHashAlgorithm.MD5:
                case PasswordHashAlgorithm.SMD5:
                    return MD5.Create();
                case PasswordHashAlgorithm.SHA1:
                case PasswordHashAlgorithm.SSHA1:
                    return SHA1.Create();
                case PasswordHashAlgorithm.SHA256:
                case PasswordHashAlgorithm.SSHA256:
                    return SHA256.Create();
                case PasswordHashAlgorithm.SHA512:
                case PasswordHashAlgorithm.SSHA512:
                    return SHA512.Create();
                case PasswordHashAlgorithm.SHA384:
                case PasswordHashAlgorithm.SSHA384:
                    return SHA384.Create();
                default:
                    m_logger.ErrorFormat("Unrecognized hash algorithm!");
                    return null;
            }
        }

        private string ToHexString(byte[] bytes)
        {
            StringBuilder builder = new StringBuilder();
            foreach( byte b in bytes )
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }

        private byte[] FromHexString(string hex)
        {
            byte[] bytes = null;
            if (hex.Length % 2 != 0)
            {
                hex = hex + "0";
                bytes = new byte[hex.Length + 1 / 2];
            }
            else
            {
                bytes = new byte[hex.Length / 2];
            }

            for (int i = 0; i < hex.Length / 2; i++ )
            {
                bytes[i] = Convert.ToByte(hex.Substring(i*2, 2), 16);
            }

            return bytes;
        }
    }
}
