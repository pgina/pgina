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

#include <windows.h>
#include <credentialprovider.h>
#include <string>

// Define the fields and field states for our tiles.  Heavily modified/borrowed from sample
// as their struct layout makes pretty good sense.

#pragma warning(push)
#pragma warning(disable : 4200)	// Cannot generate copy-ctor or copy-assignment operator when UDT contains a zero-sized array

namespace pGina
{
	namespace CredProv
	{		
		// Where to get text for a text field/label
		typedef enum PGINA_FIELD_DATA_SOURCE
		{
			SOURCE_NONE,		// Use the built in text
			SOURCE_DYNAMIC,     // Call the service
			SOURCE_CALLBACK,    // Call a function
			SOURCE_STATUS,		// Service status text
		};

		typedef std::wstring (*LABEL_TEXT_CALLBACK_FUNC)(LPWSTR fieldName, int fieldId);

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
			PGINA_FIELD_DATA_SOURCE fieldDataSource;
			union
			{
				PWSTR wstr;				
			};
			LABEL_TEXT_CALLBACK_FUNC labelCallback;
		};		

		struct UI_FIELDS
		{
			DWORD fieldCount;
			DWORD submitAdjacentTo;
			DWORD usernameFieldIdx;
			DWORD passwordFieldIdx;
			DWORD statusFieldIdx;
			UI_FIELD fields[];	// Note: Warning 4200 - compiler cannot generate copy ctor, no doing UI_FIELDS x = UI_FIELDS y!
		};
	}
}

#pragma warning(pop)


