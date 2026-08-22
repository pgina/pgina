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
using System.Security.Cryptography;
using log4net;

namespace OpenCredential.Plugin.DatabaseAuth
{
    /// <summary>
    /// Enumeration of supported password hash algorithms.
    /// </summary>
    public enum PasswordHashAlgorithm
    {
        NONE,
        MD5,
        SMD5,       // Salted MD5
        SHA1,
        SSHA1,      // Salted SHA1
        SHA256,
        SSHA256,    // Salted SHA256
        SHA384,
        SSHA384,    // Salted SHA384
        SHA512,
        SSHA512,    // Salted SHA512
        BCRYPT      // BCrypt - NEW in v4.1
    }

    /// <summary>
    /// Represents a user entry retrieved from the database.
    /// Handles password verification for all supported hash algorithms.
    /// Version: 4.1.0 - Added BCrypt support
    /// </summary>
    public class UserEntry
    {
        private static ILog m_logger = LogManager.GetLogger("UserEntry");

        public string Name { get; private set; }
        public PasswordHashAlgorithm HashAlg { get; private set; }
        public string Hash { get; private set; }
        public string StatusValue { get; private set; }
        public int FailedAttempts { get; private set; }
        public DateTime? BlockedUntilUtc { get; private set; }
        public bool IsLockedByDatabase { get; private set; }

        public UserEntry(
            string name,
            PasswordHashAlgorithm hashAlg,
            string hash,
            string statusValue = null,
            int failedAttempts = 0,
            DateTime? blockedUntilUtc = null,
            bool isLockedByDatabase = false)
        {
            Name = name;
            HashAlg = hashAlg;
            Hash = hash;
            StatusValue = statusValue;
            FailedAttempts = failedAttempts;
            BlockedUntilUtc = blockedUntilUtc;
            IsLockedByDatabase = isLockedByDatabase;
        }

        public bool IsCurrentlyLocked()
        {
            return IsLockedByDatabase || (BlockedUntilUtc.HasValue && BlockedUntilUtc.Value > DateTime.UtcNow);
        }

        /// <summary>
        /// Verifies a password against the stored hash.
        /// Supports BCrypt and legacy hash algorithms.
        /// </summary>
        public bool VerifyPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return false;

            try
            {
                switch (HashAlg)
                {
                    case PasswordHashAlgorithm.BCRYPT:
                        return VerifyBCrypt(password);
                    
                    case PasswordHashAlgorithm.NONE:
                        return password.Equals(Hash);
                    
                    case PasswordHashAlgorithm.MD5:
                        return VerifyMD5(password);
                    
                    case PasswordHashAlgorithm.SMD5:
                        return VerifySMD5(password);
                    
                    case PasswordHashAlgorithm.SHA1:
                        return VerifySHA1(password);
                    
                    case PasswordHashAlgorithm.SSHA1:
                        return VerifySSHA1(password);
                    
                    case PasswordHashAlgorithm.SHA256:
                        return VerifySHA256(password);
                    
                    case PasswordHashAlgorithm.SSHA256:
                        return VerifySSHA256(password);
                    
                    case PasswordHashAlgorithm.SHA384:
                        return VerifySHA384(password);
                    
                    case PasswordHashAlgorithm.SSHA384:
                        return VerifySSHA384(password);
                    
                    case PasswordHashAlgorithm.SHA512:
                        return VerifySHA512(password);
                    
                    case PasswordHashAlgorithm.SSHA512:
                        return VerifySSHA512(password);
                    
                    default:
                        m_logger.ErrorFormat("Unsupported hash algorithm: {0}", HashAlg);
                        return false;
                }
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Error verifying password for user {0}: {1}", Name, ex.Message);
                return false;
            }
        }

        #region BCrypt Verification
        
        private bool VerifyBCrypt(string password)
        {
            try
            {
                return BCryptHasher.Verify(password, Hash);
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("BCrypt verification error for user {0}: {1}", Name, ex.Message);
                return false;
            }
        }
        
        #endregion

        #region Legacy Hash Verification

        private bool VerifyMD5(string password)
        {
            string computedHash = ComputeMD5(password);
            return CompareHash(computedHash, Hash);
        }

        private bool VerifySMD5(string password)
        {
            return VerifySaltedHash(password, Hash, 16, MD5.Create());
        }

        private bool VerifySHA1(string password)
        {
            string computedHash = ComputeSHA1(password);
            return CompareHash(computedHash, Hash);
        }

        private bool VerifySSHA1(string password)
        {
            return VerifySaltedHash(password, Hash, 20, SHA1.Create());
        }

        private bool VerifySHA256(string password)
        {
            string computedHash = ComputeSHA256(password);
            return CompareHash(computedHash, Hash);
        }

        private bool VerifySSHA256(string password)
        {
            return VerifySaltedHash(password, Hash, 32, SHA256.Create());
        }

        private bool VerifySHA384(string password)
        {
            string computedHash = ComputeSHA384(password);
            return CompareHash(computedHash, Hash);
        }

        private bool VerifySSHA384(string password)
        {
            return VerifySaltedHash(password, Hash, 48, SHA384.Create());
        }

        private bool VerifySHA512(string password)
        {
            string computedHash = ComputeSHA512(password);
            return CompareHash(computedHash, Hash);
        }

        private bool VerifySSHA512(string password)
        {
            return VerifySaltedHash(password, Hash, 64, SHA512.Create());
        }

        #endregion

        #region Hash Computation Helpers

        private string ComputeMD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BytesToHex(hash);
            }
        }

        private string ComputeSHA1(string input)
        {
            using (var sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BytesToHex(hash);
            }
        }

        private string ComputeSHA256(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BytesToHex(hash);
            }
        }

        private string ComputeSHA384(string input)
        {
            using (var sha384 = SHA384.Create())
            {
                byte[] hash = sha384.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BytesToHex(hash);
            }
        }

        private string ComputeSHA512(string input)
        {
            using (var sha512 = SHA512.Create())
            {
                byte[] hash = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BytesToHex(hash);
            }
        }

        private bool VerifySaltedHash(string password, string storedHash, int hashLength, HashAlgorithm hasher)
        {
            try
            {
                // Decode stored hash (base64)
                byte[] storedBytes = Convert.FromBase64String(storedHash);
                
                if (storedBytes.Length < hashLength)
                {
                    m_logger.ErrorFormat("Invalid salted hash length for user {0}", Name);
                    return false;
                }

                // Extract salt (bytes after hash)
                byte[] salt = new byte[storedBytes.Length - hashLength];
                Array.Copy(storedBytes, hashLength, salt, 0, salt.Length);

                // Compute hash of password with salt
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
                Array.Copy(passwordBytes, saltedPassword, passwordBytes.Length);
                Array.Copy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                byte[] computedHash = hasher.ComputeHash(saltedPassword);

                // Compare
                for (int i = 0; i < hashLength; i++)
                {
                    if (computedHash[i] != storedBytes[i])
                        return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Error verifying salted hash for user {0}: {1}", Name, ex.Message);
                return false;
            }
        }

        private string BytesToHex(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        private string BytesToBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes);
        }

        private bool CompareHash(string hexHash, string storedHash)
        {
            if (string.IsNullOrEmpty(hexHash) || string.IsNullOrEmpty(storedHash))
                return false;

            if (Settings.GetHashEncoding() == Settings.HashEncoding.BASE_64)
            {
                try
                {
                    byte[] bytes = HexToBytes(hexHash);
                    return string.Equals(BytesToBase64(bytes), storedHash, StringComparison.Ordinal);
                }
                catch (FormatException ex)
                {
                    m_logger.ErrorFormat("Invalid hash format for user {0}: {1}", Name, ex.Message);
                    return false;
                }
            }

            return StringEquals(hexHash, storedHash);
        }

        private byte[] HexToBytes(string hex)
        {
            if (string.IsNullOrEmpty(hex) || hex.Length % 2 != 0)
                throw new FormatException("Hex hash has an invalid length.");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        private bool StringEquals(string a, string b)
        {
            if (string.IsNullOrEmpty(a) || string.IsNullOrEmpty(b))
                return false;

            // Case-insensitive comparison for hex hashes
            return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
}

