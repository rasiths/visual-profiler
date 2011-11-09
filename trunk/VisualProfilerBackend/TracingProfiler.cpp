// TracingProfiler.cpp : Implementation of CTracingProfiler

#include "stdafx.h"
#include "TracingProfiler.h"
#include <iostream>



#define NAME_BUFFER_SIZE 1024

CTracingProfiler * tracingProfiler;
int calls = 0;
void __stdcall FunctionEnterGlobal(FunctionIDOrClientID functionIDOrClientID){

	//CTracingProfiler::InspectThreadIds();
	if(functionIDOrClientID.functionID == 0)
		return;
	/*calls++;
	cout << calls << endl;*/
	shared_ptr<MethodMetadata> pMethodMetadata =  MethodMetadata::GetById(functionIDOrClientID.functionID);  
	
	std::wcout << L"Entering method " << pMethodMetadata->ToString() << std::endl;
	if(L"ComputeSqrtSum" == pMethodMetadata->Name){
		Beep(2000, 200);
		Sleep(10000);
	}
	
}

void  _declspec(naked)  FunctionEnter3Naked(FunctionIDOrClientID functionIDOrClientID)
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
		//int 3
			ret 4
	}
}

void _declspec(naked) FunctionTailcall3Naked(FunctionIDOrClientID functionIDOrClientID)
{

	__asm
	{
		//int 3
			ret    4
	}
}

HRESULT STDMETHODCALLTYPE CTracingProfiler::Initialize( /* [in] */ IUnknown *pICorProfilerInfoUnk) {
	CorProfilerCallbackBase::Initialize(pICorProfilerInfoUnk);
	DWORD mask = COR_PRF_MONITOR_ENTERLEAVE;      
	this->pProfilerInfo->SetEventMask(mask);

	FunctionEnter3* enterFunction = &FunctionEnter3Naked;
	FunctionLeave3* leaveFunction = &FunctionLeave3Naked;
	FunctionTailcall3* tailcallFuntion = &FunctionTailcall3Naked;
	this->pProfilerInfo->SetFunctionIDMapper2(&FunctionMapper, this);
	this->pProfilerInfo->SetEnterLeaveFunctionHooks3(enterFunction, leaveFunction , tailcallFuntion);
	

	tracingProfiler = this;
	Beep(1000, 500);
	return S_OK;
}

UINT_PTR STDMETHODCALLTYPE CTracingProfiler::FunctionMapper(FunctionID functionId, void * clientData, BOOL *pbHookFunction)
{
	InspectThreadIds();
	CTracingProfiler * profilerBase = (CTracingProfiler *) clientData;
	
	shared_ptr<MethodMetadata> pMethodMetadata;
	if(MethodMetadata::ContainsCache(functionId)){
		pMethodMetadata = MethodMetadata::GetById(functionId);
	}else{
		pMethodMetadata = shared_ptr<MethodMetadata>(new MethodMetadata(functionId, profilerBase->pProfilerInfo));
		MethodMetadata::AddMetadata(functionId, pMethodMetadata);
	}

	//if(wcscmp(L"ComputeSqrtSum", pMethodMetadata->Name)==0){
	/*if(L"ComputeSqrtSum" == pMethodMetadata->Name){
		Beep(2000, 200);
		Sleep(10000);
	}
		*/
	//wstring methodName = pMethodMetadata->ToString();
//	std::wcout << L"Mapping method:: " << methodName << endl;
	   
	
	*pbHookFunction = pMethodMetadata->GetDefiningAssembly()->IsProfilingEnabled;
	return functionId;
}

map<ThreadID, set<DWORD>> CTracingProfiler::ThreadIdsMap;

void CTracingProfiler::InspectThreadIds(){
//	cout<< endl ;
	
	
	HRESULT hr;
	ThreadID threadId;
	hr  = tracingProfiler->pProfilerInfo->GetCurrentThreadID(&threadId);

  //  cout<< "managed threadId = " << threadId << endl ;
	if(hr == CORPROF_E_NOT_MANAGED_THREAD){
		cout << "\t" << "CORPROF_E_NOT_MANAGED_THREAD" <<endl;
	}
	
	DWORD osThreadId;
	hr = tracingProfiler->pProfilerInfo->GetThreadInfo(threadId, &osThreadId);

	//cout<< "     os threadId = " << osThreadId << endl ;
	
	ThreadIdsMap[threadId].insert(osThreadId);

}