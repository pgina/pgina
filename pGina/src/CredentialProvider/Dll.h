#pragma once

#include <Windows.h>

extern HINSTANCE g_dllHandle;	// Globally available hinstance to self

// Instances of classes in this dll should inc/dec our
//	reference count to avoid the dll being unloaded beneath them.
void AddDllReference();
void ReleaseDllReference();