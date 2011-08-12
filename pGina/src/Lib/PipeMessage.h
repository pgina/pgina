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
			Message();
			~Message();

			static Message * Demarshal(pGina::Memory::Buffer &buffer);
			static pGina::Memory::Buffer *  Marshal(Message *);

			int  Integer(std::wstring const& propertyName);
			void Integer(std::wstring const& propertyName, int value);

			std::wstring String(std::wstring const& propertyName);
			void         String(std::wstring const& propertyName, std::wstring const& value);

			unsigned char Byte(std::wstring const& propertyName);
			void          Byte(std::wstring const& propertyName, unsigned char value);

			bool		  Bool(std::wstring const& propertyName);
			void		  Bool(std::wstring const& propertyName, bool value);
						
		private:			
			typedef std::map<std::wstring, PropertyBase *> PropertyMap;
			PropertyMap m_properties;			
		};
	}
}