using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Shared.Types
{
    public class SessionProperties
    {
        private Dictionary<string, object> m_sessionDictionary = new Dictionary<string, object>();
        private Guid m_sessionId = Guid.Empty;
        public Guid Id
        {
            get { return m_sessionId; }
            set { m_sessionId = value; }
        }

        public SessionProperties(Guid sessionId)
        {
            m_sessionId = sessionId;
            AddTrackedObject("SessionId", new Guid(m_sessionId.ToString()));
        }

        public void AddTracked<T>(string name, T value)
        {
            AddTrackedObject(name, value);
        }

        public void AddTrackedSingle<T>(T value)
        {
            AddTrackedObject(typeof(T).ToString(), value);
        }

        public void AddTrackedObject(string name, object value)
        {
            lock(this)
            {
                if (!m_sessionDictionary.ContainsKey(name))
                    m_sessionDictionary.Add(name, value);
                else
                    m_sessionDictionary[name] = value;
            }
        }

        public T GetTracked<T>(string name)
        {
            return (T)GetTrackedObject(name);
        }

        public T GetTrackedSingle<T>()
        {
            return GetTracked<T>(typeof(T).ToString());
        }

        public object GetTrackedObject(string name)
        {
            lock(this)
            {
                if(!m_sessionDictionary.ContainsKey(name))
                    throw new KeyNotFoundException(string.Format("No tracked variable {0} in session {1}", name, m_sessionId));

                return m_sessionDictionary[name];
            }
        }
    }
}
