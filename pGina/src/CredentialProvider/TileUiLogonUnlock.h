#pragma once

#include "TileUiTypes.h"

namespace pGina
{
	namespace CredProv
	{
		// Fields for unlock and logon:
		typedef enum LOGON_UI_FIELD_ID
		{
			LUIFI_TILEIMAGE       = 0,
			LUIFI_USERNAME        = 1,
			LUIFI_PASSWORD        = 2,
			LUIFI_SUBMIT          = 3, 
			LUIFI_NUM_FIELDS      = 4,  
		};

		static const UI_FIELDS s_logonFields =
		{
			LUIFI_NUM_FIELDS,		// Number of fields total
			LUIFI_PASSWORD,			// Field index which submit button should be adjacent to
			{
				//  when to display,               style,             field id,        type,               name
				{ { CPFS_DISPLAY_IN_BOTH,          CPFIS_NONE },    { LUIFI_TILEIMAGE, CPFT_TILE_IMAGE,    L"Image" } },	// LUIFI_TILEIMAGE
				{ { CPFS_DISPLAY_IN_SELECTED_TILE, CPFIS_FOCUSED }, { LUIFI_USERNAME,  CPFT_EDIT_TEXT,     L"Username" } },	// LUIFI_USERNAME
				{ { CPFS_DISPLAY_IN_SELECTED_TILE, CPFIS_NONE },	{ LUIFI_PASSWORD,  CPFT_PASSWORD_TEXT, L"Password" } }, // LUIFI_PASSWORD
				{ { CPFS_DISPLAY_IN_SELECTED_TILE, CPFIS_NONE },    { LUIFI_SUBMIT,    CPFT_SUBMIT_BUTTON, L"Submit" } },   // LUIFI_SUBMIT
			}
		};
	}
}