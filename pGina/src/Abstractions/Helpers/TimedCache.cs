using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;

namespace Abstractions.Helpers
{
    public class TimedCache<KeyType, ValueType> : IDisposable
    {
        internal class CacheEntry
        {
            public KeyType Key;
            public ValueType Value;
            public TimeSpan InsertionTime;
        }

        private Dictionary<KeyType, CacheEntry> m_cache = new Dictionary<KeyType, CacheEntry>();
        private TimeSpan m_entryTtl = TimeSpan.FromMinutes(5);
        private Timer m_timer = null;
        private Stopwatch m_stopwatch = new Stopwatch(); 

        public TimedCache()
        {
            InitTimer();
        }

        public TimedCache(TimeSpan ttl)
        {
            m_entryTtl = ttl;
            InitTimer();
        }

        public void Add(KeyType key, ValueType value)
        {
            CacheEntry ce = new CacheEntry()
            {
                Key = key,
                Value = value,
                InsertionTime = m_stopwatch.Elapsed                
            };

            lock (m_cache)
            {
                if (m_cache.ContainsKey(key))
                    m_cache[key] = ce;
                else
                    m_cache.Add(key, ce);
            }
        }

        public ValueType Get(KeyType key)
        {
            return Get(key, false);
        }

        public ValueType Get(KeyType key, bool refreshTimer)
        {
            lock (m_cache)
            {
                if (refreshTimer)                
                    m_cache[key].InsertionTime = m_stopwatch.Elapsed;
                return m_cache[key].Value;
            }
        }

        public void Remove(KeyType key)
        {
            lock (m_cache)
            {
                if (m_cache.ContainsKey(key))
                {
                    m_cache.Remove(key);
                }
            }
        }

        public bool Exists(KeyType key)
        {
            lock (m_cache)
            {
                return m_cache.ContainsKey(key);
            }
        }

        private void InitTimer()
        {
            m_stopwatch.Start();
            m_timer = new Timer(new TimerCallback(ExpireTime), null, m_entryTtl, TimeSpan.FromMilliseconds(-1));
        }

        private void ExpireTime(object timer)
        {
            lock (m_cache)
            {
                List<KeyType> expirations = new List<KeyType>();
                foreach (KeyValuePair<KeyType, CacheEntry> kv in m_cache)
                {
                    if (m_stopwatch.Elapsed - kv.Value.InsertionTime > m_entryTtl)
                    {
                        expirations.Add(kv.Key);
                    }
                }

                foreach (KeyType key in expirations)
                {
                    CacheEntry ce = m_cache[key];                    
                    if (ce.Value is IDisposable)
                    {
                        IDisposable disposableValue = (IDisposable)ce.Value;
                        disposableValue.Dispose();
                    }
                    m_cache.Remove(key);
                }
            }

            m_timer.Change(m_entryTtl, TimeSpan.FromMilliseconds(-1));
        }

        private bool m_disposed = false;
        public void Dispose()
        {
            if (!m_disposed)
            {                
                m_timer.Dispose();
                m_stopwatch.Stop();
                m_disposed = true;
            }
        }
    }
}
