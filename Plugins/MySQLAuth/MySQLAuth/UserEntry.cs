using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace pGina.Plugin.MySQLAuth
{
    enum PasswordHashAlgorithm
    {
        NONE, MD5, SHA1, SHA256, SHA384, SHA512
    }

    class UserEntry
    {
        public string Name { get; set;  }
        public PasswordHashAlgorithm HashAlg { get; set; }
        public string HashedPassword { get; set; }

        public bool VerifyPassword( string plainText )
        {
            if (plainText != null)
            {
                string hashed = HashPlainText(plainText);
                if (HashAlg == PasswordHashAlgorithm.NONE)
                    return hashed.Equals(plainText);
                else
                    return StringComparer.OrdinalIgnoreCase.Equals(hashed, HashedPassword);
            }
            else
            {
                return false;
            }
        }

        public string HashPlainText( string plainText )
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plainText);
            byte[] result = null;
            HashAlgorithm hasher = null;
            
            switch (HashAlg)
            {
                case PasswordHashAlgorithm.NONE:
                    return plainText;
                case PasswordHashAlgorithm.MD5:
                    using (hasher = MD5.Create())
                    {
                        result = hasher.ComputeHash(bytes);
                    }
                    break;
                case PasswordHashAlgorithm.SHA1:
                    using (hasher = SHA1.Create())
                    {
                        result = hasher.ComputeHash(bytes);
                    }
                    break;
                case PasswordHashAlgorithm.SHA256:
                    using (hasher = SHA256.Create())
                    {
                        result = hasher.ComputeHash(bytes);
                    }
                    break;

                case PasswordHashAlgorithm.SHA512:
                    using (hasher = SHA512.Create())
                    {
                        result = hasher.ComputeHash(bytes);
                    }
                    break;

                case PasswordHashAlgorithm.SHA384:
                    using (hasher = SHA384.Create())
                    {
                        result = hasher.ComputeHash(bytes);
                    }
                    break;
            }

            if (result != null)
            {
                return ToHexString(result);
            }
            else
            {
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
    }
}
