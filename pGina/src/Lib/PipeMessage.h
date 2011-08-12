#pragma once

#include <Windows.h>
#include <iostream>
#include <map>

#include "MessageProperty.h"
#include "Buffer.h"

namespace pGina
{
	namespace Pipes
	{				
		class Message
		{
		public:	
			typedef std::map<std::wstring, PropertyBase *> PropertyMap;

			Message();
			~Message();

			static Message * Demarshal(pGina::Memory::Buffer &buffer);
			static pGina::Memory::Buffer *  Marshal(Message *);

			template<typename T>
			bool Exists(std::wstring const& propertyName)
			{
				PropertyMap::iterator itr = m_properties.find(propertyName);
				if(itr != m_properties.end())
				{
					Property<T> * prop = dynamic_cast<Property<T> *>(itr->second);
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
					Property<T> * prop = static_cast<Property<T> *>(itr->second);
					return prop->Value();
				}

				return T();
			}

			template<typename T>
			void Property(std::wstring const& propertyName, T const& value, PropertyType type)
			{				
				// Create a new property to be inserted
				pGina::Pipes::Property<T> * prop = new pGina::Pipes::Property<T>(propertyName, value, type);

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