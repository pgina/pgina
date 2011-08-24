#pragma once

#include <Windows.h>
#include <string>

namespace pGina
{
	namespace Messaging
	{
		typedef enum PropertyType
		{
			Byte            = 0x00,
            Integer         = 0x01,
            Boolean         = 0x02,
            String          = 0x03,
            EmptyString     = 0x04,
		};

		class PropertyBase
		{
		public:
			std::wstring const& Name() { return m_name; }
			void                Name(std::wstring const& v) { m_name = v; }

			PropertyType        Type() { return m_type; }
			void				Type(PropertyType t) { m_type = t; }
			
		protected:
			std::wstring m_name;					
			PropertyType m_type;

			PropertyBase() {}	// Common folk shouldn't be creating me..		
			virtual void DummyVirtualForDynamicCastRequirement() {}
		};

		template <typename ValueType>
		class Property : public PropertyBase
		{
		public:
			Property(std::wstring const& name, ValueType const& value, PropertyType type) :
				m_value(value)
				{
					m_name = name;				
					m_type = type;
				}
			  			
			ValueType const&    Value() { return m_value; }
			void			    Value(ValueType const& v) { m_value = v; }

		private:			
			ValueType m_value;
		};
	}
}