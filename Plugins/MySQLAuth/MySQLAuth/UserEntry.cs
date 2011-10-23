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
