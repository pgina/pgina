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
using System.ComponentModel;
using System.Threading;

using log4net;

using Abstractions.WindowsApi;

namespace pGina.Plugin.MySqlLogger
{
    class SessionCache
    {
        private Dictionary<int, string> m_cache;
        private static ILog m_logger = log4net.LogManager.GetLogger("SessionCache");

        public SessionCache()
        {
            m_cache = new Dictionary<int, string>();
        }

        public void Add(int sessId, string userName)
        {
            lock(this)
            {
                if (!m_cache.ContainsKey(sessId))
                    m_cache.Add(sessId, userName);
                else
                    m_cache[sessId] = userName;
            }
        }

        /// <summary>
        /// Tries to get the username from the cache.  If not found, it tries to get
        /// the username using the pInvoke call.  If the pInvoke call succeeds, it 
        /// adds the username to the cache.  If all attempts fail, returns null.
        /// </summary>
        /// <param name="sessId">The session ID.</param>
        /// <returns>The username or null if not found in the cache or via the pInvoke call.</returns>
        public string Get(int sessId)
        {
            string result = null;
            lock (this)
            {
                if (m_cache.ContainsKey(sessId))
                    result = m_cache[sessId];
            }

            if (result == null)
            {
                // Try to get the username from the pInvoke call
                result = GetUserName(sessId);
                if (result != null)
                    this.Add(sessId, result);
            }

            return result;
        }

        public void Clear()
        {
            lock (this)
            {
                m_cache.Clear();
            }
        }

        /// <summary>
        /// Tries to get the username for a session ID by calling the pInvoke:
        /// WTSQuerySessionInformation.  If that fails, returns null.
        /// </summary>
        /// <param name="sessionId">The session ID.</param>
        /// <returns>The username or null if not found.</returns>
        public static string GetUserName(int sessionId)
        {
            try
            {
                string result = pInvokes.GetUserName(sessionId);
                if (string.IsNullOrEmpty(result.Trim()))
                    return null;
                return result;
            }
            catch (Win32Exception)
            {
                return null;
            }
        }

        public static string TryHardToGetUserName( int sessionId, int tries, int delay )
        {
            string result = null;
            for (int i = 0; i < tries; i++)
            {
                m_logger.DebugFormat("Trying to get username for session ID {0}, attempt {1}", sessionId, i + 1);
                result = GetUserName(sessionId);
                if (result != null)
                {
                    m_logger.DebugFormat("Got username {0}", result);
                    return result;
                }
                Thread.Sleep(delay);
            }
            return result;
        }
    }
}
