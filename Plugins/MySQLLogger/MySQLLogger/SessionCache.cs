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
            if (m_cache.ContainsKey(sessId))
                return m_cache[sessId];
            return "";
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
