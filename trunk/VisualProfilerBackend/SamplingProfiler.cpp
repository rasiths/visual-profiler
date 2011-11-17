// SamplingProfiler.cpp : Implementation of CSamplingProfiler

#include "stdafx.h"
#include "SamplingProfiler.h"


HRESULT STDMETHODCALLTYPE CSamplingProfiler::Initialize(IUnknown *pICorProfilerInfoUnk) {
	CorProfilerCallbackBase::Initialize(pICorProfilerInfoUnk);

	DWORD mask =  COR_PRF_MONITOR_THREADS |  COR_PRF_ENABLE_STACK_SNAPSHOT ;
	pProfilerInfo->SetEventMask(mask);
	Beep(2000, 200);
	_stackWalker = shared_ptr<StackWalker>(new StackWalker(pProfilerInfo, 2));
	_stackWalker->StartSampling();
	return S_OK;
}

HRESULT STDMETHODCALLTYPE CSamplingProfiler::Shutdown(){
	//DWORD osThreadId = GetCurrentThreadId();
	//cout << "Shutting down from osThreadId=" << osThreadId << endl;
	_stackWalker->StopSampling();
	return S_OK;
}

HRESULT STDMETHODCALLTYPE  CSamplingProfiler::ThreadCreated(ThreadID threadId) {
	/*	DWORD osThreadId = GetCurrentThreadId();
		cout << "Thread id = " << threadId << " created" << ", osThreadId=" << osThreadId << endl;
	HANDLE threadHandle = GetCurrentThread();*/
	//GetThreadTimes(
	_stackWalker->RegisterThread(threadId);
	return S_OK;
}

HRESULT STDMETHODCALLTYPE  CSamplingProfiler::ThreadDestroyed(ThreadID threadId) {
	/*	DWORD osThreadId = GetCurrentThreadId();
	cout << "Thread id = " << threadId << " destroyed" << ", osThreadId=" << osThreadId << endl;*/
	_stackWalker->DeregisterThread(threadId);

	return S_OK;
}

HRESULT STDMETHODCALLTYPE  CSamplingProfiler::ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId) {
	/*DWORD myOsThreadId;
	pProfilerInfo->GetThreadInfo(managedThreadId, &myOsThreadId);
	DWORD realId = GetCurrentThreadId();
	HANDLE threadHandle = GetCurrentThread();*/
	return S_OK;
}


