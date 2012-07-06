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
#pragma once

#include <Windows.h>
#include <iostream>
#include <map>

#include "MessageProperty.h"
#include "Buffer.h"

namespace pGina
{
	namespace Messaging
	{				
		class Message
		{
		public:	
			typedef std::map<std::wstring, PropertyBase *> PropertyMap;

			Message();
			~Message();

			static Message * Demarshal(pGina::Memory::Buffer &buffer);
			static Message * Demarshal(pGina::Memory::Buffer *buffer);
			static pGina::Memory::Buffer *  Marshal(Message *);

			template<typename T>
			bool Exists(std::wstring const& propertyName)
			{
				PropertyMap::iterator itr = m_properties.find(propertyName);
				if(itr != m_properties.end())
				{
					pGina::Messaging::Property<T> * prop = dynamic_cast<pGina::Messaging::Property<T> *>(itr->second);
					return prop != 0;
				}

				return false;
			}

			template<typename T>
			T    Property(std::wstring const& propertyName)
			{
				PropertyMap::iterator itr = m_properties.find(propertyName);
				if(itr != m_properties.end())
				{
					pGina::Messaging::Property<T> * prop = static_cast<pGina::Messaging::Property<T> *>(itr->second);
					return prop->Value();
				}

				return T();
			}

			template<typename T>
			void Property(std::wstring const& propertyName, T const& value, PropertyType type)
			{				
				// Create a new property to be inserted
				pGina::Messaging::Property<T> * prop = new pGina::Messaging::Property<T>(propertyName, value, type);

				// If we already have a property by this name, we need to clean it up
				PropertyMap::iterator itr = m_properties.find(propertyName);
				if(itr != m_properties.end())
				{
					PropertyBase * base = itr->second;
					delete base;
					m_properties.erase(itr);
				}

				// Insert our new property
				m_properties[propertyName] = prop;
			}	

			PropertyMap& Properties() { return m_properties; }
						
		private:			
			static int MarshalToBuffer(Message *msg, pGina::Memory::Buffer * buffer);			
			PropertyMap m_properties;			
		};
	}
}