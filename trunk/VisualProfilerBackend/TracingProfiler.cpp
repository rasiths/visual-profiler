// TracingProfiler.cpp : Implementation of CTracingProfiler

#include "stdafx.h"
#include "TracingProfiler.h"
#include <iostream>
#define NAME_BUFFER_SIZE 1024

CTracingProfiler * tracingProfiler;

void __stdcall FunctionEnterGlobal(FunctionIDOrClientID functionIDOrClientID){
	BOOL zkurvenaPromena;
	CTracingProfiler::FunctionMapper(functionIDOrClientID.functionID, &zkurvenaPromena);
}



void _declspec(naked)  FunctionEnter3Naked(FunctionIDOrClientID functionIDOrClientID)
{

	__asm
	{
		push    ebp                 // Create a frame
			mov     ebp,esp
			pushad                      // Save registers
			mov     eax,[ebp+0x08]      // functionIDOrClientID
		push    eax;
		call    FunctionEnterGlobal
			popad                       // Restore registers
			pop     ebp                 // Restore EBP
			ret     4					// Return to caller and ESP = ESP+4 to clean the argument
	}
}

void _declspec(naked) FunctionLeave3Naked(FunctionIDOrClientID functionIDOrClientID)
{

	__asm
	{
		int 3
			ret
	}
}

void _declspec(naked) FunctionTailcall3Naked(FunctionIDOrClientID functionIDOrClientID)
{

	__asm
	{
		int 3
			ret    
	}
}

HRESULT STDMETHODCALLTYPE CTracingProfiler::Initialize( /* [in] */ IUnknown *pICorProfilerInfoUnk) {
	CorProfilerCallbackBase::Initialize(pICorProfilerInfoUnk);
	DWORD mask = COR_PRF_MONITOR_ENTERLEAVE;      
	_pICorProfilerInfo3->SetEventMask(mask);

	FunctionEnter3* enterFunction = &FunctionEnter3Naked;
	FunctionLeave3* leaveFunction = &FunctionLeave3Naked;
	FunctionTailcall3* tailcallFuntion = &FunctionTailcall3Naked;
	_pICorProfilerInfo3->SetEnterLeaveFunctionHooks3(enterFunction, NULL , NULL);
	_pICorProfilerInfo3->SetFunctionIDMapper(&FunctionMapper);

	tracingProfiler = this;
	Beep(1000, 500);
	return S_OK;
}

UINT_PTR CTracingProfiler::FunctionMapper(FunctionID functionID, BOOL *pbHookFunction)
{
	FunctionID id = functionID;
	IMetaDataImport2* pIMetaDataImport = 0;
	HRESULT hr = S_OK;
	mdToken funcToken = 0;
	WCHAR szFunction[NAME_BUFFER_SIZE];
	WCHAR szClass[NAME_BUFFER_SIZE];
	hr = tracingProfiler->_pICorProfilerInfo3->GetTokenAndMetaDataFromFunction(id,IID_IMetaDataImport2,(LPUNKNOWN *) &pIMetaDataImport, &funcToken);
	if(SUCCEEDED(hr))
	{
		mdTypeDef classTypeDef;
		ULONG cchFunction;
		ULONG cchClass;

		// retrieve the function properties based on the token
		hr = pIMetaDataImport->GetMethodProps(funcToken, &classTypeDef, szFunction, NAME_BUFFER_SIZE, &cchFunction, 0, 0, 0, 0, 0);
		if (SUCCEEDED(hr))
		{
			// get the function name
			hr = pIMetaDataImport->GetTypeDefProps(classTypeDef, szClass, NAME_BUFFER_SIZE, &cchClass, 0, 0);
			if (SUCCEEDED(hr))
			{
				// create the fully qualified name
				std::wcout << szClass << "." <<szFunction << std::endl;
			}
		}
		// release our reference to the metadata
		pIMetaDataImport->Release();
	}

	return S_OK;
}

