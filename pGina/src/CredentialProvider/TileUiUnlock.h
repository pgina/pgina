#pragma once

#include "TileUiTypes.h"

namespace pGina
{
	namespace CredProv
	{
		// Fields for unlock and logon:
		typedef enum LOCKED_UI_FIELD_ID
		{
			LOIFI_TILEIMAGE       = 0,
			LOIFI_LOCKED          = 1,
			LOIFI_USERNAME        = 2,
			LOIFI_PASSWORD        = 3,
			LOIFI_SUBMIT          = 4, 
			LOIFI_NUM_FIELDS      = 5,  
		};

		static const UI_FIELDS s_unlockFields =
		{
			LOIFI_NUM_FIELDS,		// Number of fields total
			LOIFI_PASSWORD,			// Field index which submit button should be adjacent to
			LOIFI_USERNAME,			// Username field index value
			LOIFI_PASSWORD,			// Password field index value
			{
				//  when to display,               style,             field id,        type,               name
				{ { CPFS_DISPLAY_IN_BOTH,          CPFIS_NONE },    { LOIFI_TILEIMAGE, CPFT_TILE_IMAGE,    L"Image" } },	// LOIFI_TILEIMAGE
				{ { CPFS_DISPLAY_IN_BOTH,          CPFIS_NONE },    { LOIFI_LOCKED,    CPFT_LARGE_TEXT,    L"Locked" }, L"Locked" },   // LOIFI_LOCKED
				{ { CPFS_DISPLAY_IN_SELECTED_TILE, CPFIS_FOCUSED }, { LOIFI_USERNAME,  CPFT_EDIT_TEXT,     L"Username" } },	// LOIFI_USERNAME
				{ { CPFS_DISPLAY_IN_SELECTED_TILE, CPFIS_NONE },	{ LOIFI_PASSWORD,  CPFT_PASSWORD_TEXT, L"Password" } }, // LOIFI_PASSWORD
				{ { CPFS_DISPLAY_IN_SELECTED_TILE, CPFIS_NONE },    { LOIFI_SUBMIT,    CPFT_SUBMIT_BUTTON, L"Submit" } },   // LOIFI_SUBMIT
			}
		};
	}
}