using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Plugin.MySqlLogger
{
    class SessionCache
    {
        private Dictionary<int, string> m_cache;

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

        public string Get(int sessId)
        {
            string result = "--Unknown--";
            lock (this)
            {
                if (m_cache.ContainsKey(sessId))
                    result = m_cache[sessId];
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
    }
}
