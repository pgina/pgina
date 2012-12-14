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

namespace pGina.Plugin.SessionLimit
{
    class SessionCache
    {
        Dictionary<int, DateTime> m_cache;

        public SessionCache()
        {
            m_cache = new Dictionary<int, DateTime>();
        }

        public void Add(int sessId)
        {
            lock (this)
            {
                if (m_cache.ContainsKey(sessId))
                    m_cache[sessId] = DateTime.Now;
                else
                    m_cache.Add(sessId, DateTime.Now);
            }
        }

        public void Remove(int sessId)
        {
            lock (this)
            {
                m_cache.Remove(sessId);
            }
        }

        private TimeSpan LoggedInTimeSpan(int sessId)
        {
            if (m_cache.ContainsKey(sessId))
                return DateTime.Now - m_cache[sessId];
            else
                return TimeSpan.Zero;
        }

        public List<int> SessionsLoggedOnLongerThan(TimeSpan span)
        {
            lock (this)
            {
                List<int> sessionList = new List<int>();

                foreach (int sessId in m_cache.Keys)
                {
                    TimeSpan sessSpan = LoggedInTimeSpan(sessId);
                    if (sessSpan >= span)
                    {
                        sessionList.Add(sessId);
                    }
                }
                return sessionList;
            }
        }
    }
}
