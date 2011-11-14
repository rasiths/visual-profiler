// TracingProfiler.cpp : Implementation of CTracingProfiler

#include "stdafx.h"
#include "TracingProfiler.h"
#include <iostream>
#include "ThreadCallTree.h"

CTracingProfiler * tracingProfiler;

__declspec(thread)  ThreadCallTree * CTracingProfiler::_pThreadCallTree = 0;

void __stdcall CTracingProfiler::FunctionEnterHook(FunctionIDOrClientID functionIDOrClientID){
	_pThreadCallTree->FunctionEnter(functionIDOrClientID.functionID);
}

void __stdcall CTracingProfiler::FunctionLeaveHook(FunctionIDOrClientID functionIDOrClientID){
	_pThreadCallTree->FunctionLeave(functionIDOrClientID.functionID);
}

void  _declspec(naked) FunctionEnter3Naked(FunctionIDOrClientID functionIDOrClientID)
{
	__asm
	{
		push    ebp                 // Create a frame
			mov     ebp,esp
			pushad                      // Save registers
			mov     eax,[ebp+0x08]      // pass functionIDOrClientID parameter to a function
		push    eax;
		call    CTracingProfiler::FunctionEnterHook // call a function
			popad                       // Restore registers
			pop     ebp                 // Restore EBP
			ret     4					// Return to caller and ESP = ESP+4 to clean the argument
	}
}

void _declspec(naked) FunctionLeave3Naked(FunctionIDOrClientID functionIDOrClientID)
{

	__asm
	{
		push    ebp                 // Create a frame
			mov     ebp,esp
			pushad                      // Save registers
			mov     eax,[ebp+0x08]      // pass functionIDOrClientID parameter to a function
		push    eax;
		call    CTracingProfiler::FunctionLeaveHook // call a function
			popad                       // Restore registers
			pop     ebp                 // Restore EBP
			ret     4					// Return to caller and ESP = ESP+4 to clean the argument
	}
}

void _declspec(naked) FunctionTailcall3Naked(FunctionIDOrClientID functionIDOrClientID)
{

	__asm
	{
		int 3
			ret    4
	}
}

HRESULT STDMETHODCALLTYPE CTracingProfiler::Initialize( IUnknown *pICorProfilerInfoUnk) {
	CorProfilerCallbackBase::Initialize(pICorProfilerInfoUnk);
	DWORD mask = COR_PRF_MONITOR_ENTERLEAVE |  COR_PRF_MONITOR_THREADS  | COR_PRF_DISABLE_INLINING;      
	this->pProfilerInfo->SetEventMask(mask);

	FunctionEnter3* enterFunction = &FunctionEnter3Naked;
	FunctionLeave3* leaveFunction = &FunctionLeave3Naked;
	FunctionTailcall3* tailcallFuntion = &FunctionTailcall3Naked;
	this->pProfilerInfo->SetFunctionIDMapper2(&FunctionMapper, this);
	this->pProfilerInfo->SetEnterLeaveFunctionHooks3(enterFunction, leaveFunction , tailcallFuntion);

	tracingProfiler = this;
	Beep(4000, 100);
	return S_OK;
}

UINT_PTR STDMETHODCALLTYPE CTracingProfiler::FunctionMapper(FunctionID functionId, void * clientData, BOOL *pbHookFunction)
{	 
	CTracingProfiler * profilerBase = (CTracingProfiler *) clientData;

	shared_ptr<MethodMetadata> pMethodMetadata;
	if(MethodMetadata::ContainsCache(functionId)){
		pMethodMetadata = MethodMetadata::GetById(functionId);
	}else{
		pMethodMetadata = shared_ptr<MethodMetadata>(new MethodMetadata(functionId, profilerBase->pProfilerInfo));
		MethodMetadata::AddMetadata(functionId, pMethodMetadata);
	}

	/*	wstring methodName = pMethodMetadata->ToString();
	std::wcout << L"Mapping method:: " << methodName << endl;
	*/   

	*pbHookFunction = pMethodMetadata->GetDefiningAssembly()->IsProfilingEnabled;
	//*pbHookFunction = true;
	UINT_PTR internalCLRFunctionKey = functionId;
	return internalCLRFunctionKey;
}

HRESULT STDMETHODCALLTYPE CTracingProfiler::ThreadCreated(ThreadID threadId){
	_pThreadCallTree = ThreadCallTree::AddThread(threadId);
	_pThreadCallTree->GetTimer()->Start();
	HANDLE osThreadHandle = GetCurrentThread();
	_pThreadCallTree->SetOSThreadHandle(osThreadHandle);
	
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CTracingProfiler::ThreadDestroyed(ThreadID threadId){
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CTracingProfiler::RuntimeThreadSuspended(ThreadID threadId){
	_pThreadCallTree->GetTimer()->Stop();
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CTracingProfiler::RuntimeThreadResumed(ThreadID threadId){
	_pThreadCallTree->GetTimer()->Start();
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CTracingProfiler::ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId){
	bool needOSThreadInitialization = _pThreadCallTree == NULL || _pThreadCallTree->GetThreadId() != managedThreadId;
	if(needOSThreadInitialization){
		_pThreadCallTree =  ThreadCallTree::GetThreadCallTree(managedThreadId);
	}

	HANDLE osThreadHandle = GetCurrentThread();
	_pThreadCallTree->SetOSThreadHandle(osThreadHandle);

	//update thread kernel and user mode time stamps when a new os thread is assigned to the managed thread 
	ThreadCallTreeElem * pCallTreeElem = _pThreadCallTree->GetRealRootCallTreeElem();
	if(pCallTreeElem != NULL){
		while(!pCallTreeElem->IsRootElem()){
			FILETIME dummy;
			GetThreadTimes(_pThreadCallTree->GetOSThreadHandle(),&dummy, &dummy, &pCallTreeElem->LastEnterKernelModeTimeStamp, &pCallTreeElem->LastEnterUserModeTimeStamp);
		}
	}
	
	return S_OK;
}