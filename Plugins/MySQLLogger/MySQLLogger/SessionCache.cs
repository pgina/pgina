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
