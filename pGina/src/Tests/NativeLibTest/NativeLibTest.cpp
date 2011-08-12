// NativeLibTest.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include <pGinaNativeLib.h>


int _tmain(int argc, _TCHAR* argv[])
{		
	std::wstring pipeName = pGina::Registry::GetString(L"ServicePipeName", L"Unknown");
	return 0;
}

