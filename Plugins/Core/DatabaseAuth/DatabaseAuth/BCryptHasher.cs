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
using log4net;

namespace OpenCredential.Plugin.DatabaseAuth
{
    /// <summary>
    /// Wrapper class for BCrypt password hashing.
    /// Uses BCrypt.Net-Next library for BCrypt implementation.
    /// Version: 4.1.0
    /// </summary>
    public static class BCryptHasher
    {
        private static ILog m_logger = LogManager.GetLogger("BCryptHasher");

        /// <summary>
        /// Hashes a password using BCrypt with the specified work factor.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="workFactor">The BCrypt work factor (4-12). Higher is more secure but slower.</param>
        /// <returns>BCrypt hash string.</returns>
        public static string HashPassword(string password, int workFactor = 10)
        {
            if (string.IsNullOrEmpty(password))
            {
                throw new ArgumentException("Password cannot be null or empty", nameof(password));
            }

            // Ensure work factor is within valid range
            if (workFactor < 4) workFactor = 4;
            if (workFactor > 12) workFactor = 12;

            try
            {
                string hash = BCrypt.Net.BCrypt.HashPassword(password, workFactor);
                m_logger.DebugFormat("Generated BCrypt hash with work factor {0}", workFactor);
                return hash;
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Error generating BCrypt hash: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Verifies a password against a BCrypt hash.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="hash">The BCrypt hash to verify against.</param>
        /// <returns>True if the password matches the hash.</returns>
        public static bool Verify(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
            {
                return false;
            }

            try
            {
                // Validate hash format
                if (!IsValidBCryptHash(hash))
                {
                    m_logger.WarnFormat("Invalid BCrypt hash format: {0}", hash.Substring(0, Math.Min(10, hash.Length)) + "...");
                    return false;
                }

                bool result = BCrypt.Net.BCrypt.Verify(password, hash);
                m_logger.DebugFormat("BCrypt verification result: {0}", result);
                return result;
            }
            catch (Exception ex)
            {
                m_logger.ErrorFormat("Error verifying BCrypt hash: {0}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Checks if a hash string is a valid BCrypt hash format.
        /// </summary>
        /// <param name="hash">The hash string to validate.</param>
        /// <returns>True if the hash appears to be a valid BCrypt hash.</returns>
        public static bool IsValidBCryptHash(string hash)
        {
            if (string.IsNullOrEmpty(hash))
                return false;

            // BCrypt hashes are 60 characters and start with $2a$, $2b$, or $2y$
            if (hash.Length != 60)
                return false;

            return hash.StartsWith("$2a$") || hash.StartsWith("$2b$") || hash.StartsWith("$2y$");
        }

        /// <summary>
        /// Gets the work factor from a BCrypt hash.
        /// </summary>
        /// <param name="hash">The BCrypt hash.</param>
        /// <returns>The work factor, or -1 if the hash is invalid.</returns>
        public static int GetWorkFactor(string hash)
        {
            if (!IsValidBCryptHash(hash))
                return -1;

            try
            {
                // Work factor is in positions 4-6 of the hash (after $2a$)
                string workFactorStr = hash.Substring(4, 2);
                return int.Parse(workFactorStr);
            }
            catch
            {
                return -1;
            }
        }
    }
}

