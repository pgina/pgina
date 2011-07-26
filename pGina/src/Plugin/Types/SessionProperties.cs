using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pGina.Shared.Types
{
    public static class SessionProperties
    {
        private static Dictionary<Guid, Dictionary<string, object>> s_sessionDictionary = new Dictionary<Guid, Dictionary<string, object>>();
        private static object s_mutex = new object();

        // Called by pGina before the IAuth* plugins are called
        public static void StartSessionTracking(Guid newGuid)
        {
            lock(s_mutex)
            {
                if(!s_sessionDictionary.ContainsKey(newGuid))
                    s_sessionDictionary.Add(newGuid, new Dictionary<string,object>());
                else
                    s_sessionDictionary[newGuid].Clear();            
            }
        }

        // Called by pGina after all plugins are called
        public static void StopSessionTracking(Guid newGuid)
        {
            lock(s_mutex)
            {
                if(s_sessionDictionary.ContainsKey(newGuid))
                    s_sessionDictionary.Remove(newGuid);
            }
        }

        public static void AddTrackedObject(Guid trackedGuid, string name, object value)
        {
            lock(s_mutex)
            {            
                if(!s_sessionDictionary.ContainsKey(trackedGuid))
                    throw new KeyNotFoundException(string.Format("No tracked session for: {0}", trackedGuid));

                if(!s_sessionDictionary[trackedGuid].ContainsKey(name))
                    s_sessionDictionary[trackedGuid].Add(name, value);
                else
                    s_sessionDictionary[trackedGuid][name] = value;
            }
        }

        public static object GetTrackedObject(Guid trackedGuid, string name)
        {
            lock(s_mutex)
            {
                if(!s_sessionDictionary.ContainsKey(trackedGuid))
                    throw new KeyNotFoundException(string.Format("No tracked session for: {0}", trackedGuid));

                if(!s_sessionDictionary[trackedGuid].ContainsKey(name))
                    throw new KeyNotFoundException(string.Format("No tracked variable {0} in session {1}", name, trackedGuid));

                return s_sessionDictionary[trackedGuid][name];
            }
        }
    }
}
