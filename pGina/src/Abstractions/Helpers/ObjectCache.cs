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
using System.Threading;
using System.Diagnostics;

namespace Abstractions.Helpers
{
    public class ObjectCache<KeyType, ValueType> : IDisposable
    {
        protected class CacheEntry
        {
            public KeyType Key;
            public ValueType Value;            
        }

        protected Dictionary<KeyType, CacheEntry> m_cache = new Dictionary<KeyType, CacheEntry>();
        
        public ObjectCache()
        {            
        }

        protected virtual CacheEntry WrapValue(KeyType key, ValueType value)
        {
            return new CacheEntry()
            {
                Key = key,
                Value = value,
            };
        }
        
        public virtual void Add(KeyType key, ValueType value)
        {
            CacheEntry ce = WrapValue(key, value);

            lock (m_cache)
            {
                if (m_cache.ContainsKey(key))
                    m_cache[key] = ce;
                else
                    m_cache.Add(key, ce);
            }
        }

        public virtual ValueType Get(KeyType key)
        {
            return m_cache[key].Value;
        }
        
        public virtual void Remove(KeyType key)
        {
            lock (m_cache)
            {
                if (m_cache.ContainsKey(key))
                {
                    m_cache.Remove(key);
                }
            }
        }

        public virtual bool Exists(KeyType key)
        {
            lock (m_cache)
            {
                return m_cache.ContainsKey(key);
            }
        }

        private bool m_disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!m_disposed)
            {
                m_disposed = true;
                if (disposing)
                {
                    m_cache.Clear();
                }                
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);            
        }

        ~ObjectCache()
        {
            Dispose(false);
        }
    }
}
