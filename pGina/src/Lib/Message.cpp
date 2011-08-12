#include <assert.h>
#include "Message.h"
#include "MessageProperty.h"
#include "BinaryReader.h"
#include "BinaryWriter.h"

#define CURRENT_MESSAGE_FORMAT_VERSION 0x01

namespace pGina
{
	namespace Messaging
	{
		Message::Message()
		{
		}

		Message::~Message()
		{
			for(PropertyMap::iterator itr = m_properties.begin(); itr != m_properties.end(); ++itr)
			{
				// Release the property object we own
				delete itr->second;
			}

			m_properties.clear();
		}
							
		/* static */
		Message * Message::Demarshal(pGina::Memory::Buffer &buffer)
		{
			// Cannot be empty
			if(buffer.Raw() == 0 || buffer.Length() == 0)
				return 0;
			
			pGina::Memory::BinaryReader reader(buffer);

			unsigned char messageFormatVersion = reader.ReadByte();
			if(messageFormatVersion != CURRENT_MESSAGE_FORMAT_VERSION)
				return 0;

			Message * msg = new Message();
			while(!reader.EndOfBuffer())
			{
				std::wstring propertyName = reader.ReadUnicodeString();
				PropertyType propertyType = static_cast<PropertyType>(reader.ReadByte());
				
				switch(propertyType)
				{
				case Boolean:
					msg->Property<bool>(propertyName, reader.ReadBool(), Boolean);
					break;
				case Byte:
					msg->Property<unsigned char>(propertyName, reader.ReadByte(), Byte);
					break;
				case EmptyString:
					msg->Property<std::wstring>(propertyName, L"", String);
					break;
				case Integer:
					msg->Property<int>(propertyName, reader.ReadInt32(), Integer);
					break;
				case String:
					msg->Property<std::wstring>(propertyName, reader.ReadUnicodeString(), String);
					break;
				}
			}			
			return msg;
		}
		
		/* static */
		pGina::Memory::Buffer *  Message::Marshal(Message *msg)
		{
			int length = MarshalToBuffer(msg, 0);
			pGina::Memory::Buffer * buffer = new pGina::Memory::Buffer(length);
			
			if(MarshalToBuffer(msg, buffer) != length)
				assert(0);

			return buffer;
		}

		/* static */
		int Message::MarshalToBuffer(Message * msg, pGina::Memory::Buffer * buffer)
		{
			pGina::Memory::BinaryWriter writer(buffer);

			writer.Write((unsigned char) CURRENT_MESSAGE_FORMAT_VERSION);

			PropertyMap& properties = msg->Properties();

			for(Message::PropertyMap::iterator itr = properties.begin(); itr != properties.end(); ++itr)
			{
				// Property name
				writer.Write(itr->first);

				PropertyBase * propBase = itr->second;

				// Special case, if its a string, and its empty, we want to write
				//	nothing for value
				if(propBase->Type() == String)
				{
					pGina::Messaging::Property<std::wstring> * prop = static_cast<pGina::Messaging::Property<std::wstring> *>(propBase);
					if(prop->Value().empty())
						propBase->Type(EmptyString);
				}

				// Property type
				writer.Write((unsigned char)propBase->Type());

				// now work out type/value				
				switch(propBase->Type())
				{
				case Boolean:				
					{
						pGina::Messaging::Property<bool> * prop = static_cast<pGina::Messaging::Property<bool> *>(propBase);
						writer.Write(prop->Value());
					}
					break;
				case Byte:
					{
						pGina::Messaging::Property<unsigned char> * prop = static_cast<pGina::Messaging::Property<unsigned char> *>(propBase);
						writer.Write(prop->Value());
					}
					break;
				case EmptyString:	
					// Do nothing, no value here
					break;
				case Integer:					
					{
						pGina::Messaging::Property<int> * prop = static_cast<pGina::Messaging::Property<int> *>(propBase);
						writer.Write(prop->Value());
					}
					break;
				case String:					
					{
						pGina::Messaging::Property<std::wstring> * prop = static_cast<pGina::Messaging::Property<std::wstring> *>(propBase);
						writer.Write(prop->Value());
					}
					break;
				}
			}

			return writer.BytesWritten();			
		}
	}
}