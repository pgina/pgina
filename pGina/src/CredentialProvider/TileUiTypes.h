#pragma once

#include <credentialprovider.h>

// Define the fields and field states for our tiles.  Heavily modified/borrowed from sample
// as their struct layout makes pretty good sense.

#pragma warning(push)
#pragma warning(disable : 4200)	// Cannot generate copy-ctor or copy-assignment operator when UDT contains a zero-sized array

namespace pGina
{
	namespace CredProv
	{		
		// The first value indicates when the tile is displayed (selected, not selected)
		// the second indicates things like whether the field is enabled, whether it has key focus, etc.
		struct FIELD_STATE_PAIR
		{
			CREDENTIAL_PROVIDER_FIELD_STATE fieldState;
			CREDENTIAL_PROVIDER_FIELD_INTERACTIVE_STATE fieldInteractiveState;		
		};

		// Describes the field overall, both its state pair and its field descriptor
		//  as well as a union representing the fields current value
		struct UI_FIELD
		{
			FIELD_STATE_PAIR fieldStatePair;
			CREDENTIAL_PROVIDER_FIELD_DESCRIPTOR fieldDescriptor;
			union
			{
				PWSTR wstr;				
			};
		};		

		struct UI_FIELDS
		{
			DWORD fieldCount;
			DWORD submitAdjacentTo;
			UI_FIELD fields[];	// Note: Warning 4200 - compiler cannot generate copy ctor, no doing UI_FIELDS x = UI_FIELDS y!
		};
	}
}

#pragma warning(pop)


