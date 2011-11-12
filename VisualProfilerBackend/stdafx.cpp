// stdafx.cpp : source file that includes just the standard includes
// VisualProfilerBackend.pch will be the pre-compiled header
// stdafx.obj will contain the pre-compiled type information

#include "stdafx.h"

void CheckError(HRESULT hr){
	if(SUCCEEDED(hr)){
		return;
	}

	throw hr;
}

void CheckError(bool succeeded){
	if(succeeded){
		return;
	}

	throw "CheckError encountered false";
}

void HandleError(wstring message){
	Beep(3333,1000);
	__asm{
		int 3
	}
}