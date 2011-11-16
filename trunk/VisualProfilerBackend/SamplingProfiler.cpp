// SamplingProfiler.cpp : Implementation of CSamplingProfiler

#include "stdafx.h"
#include "SamplingProfiler.h"


bool CSamplingProfiler::_continueSampling = true;
vector<FunctionID> CSamplingProfiler::_functionIds;

set<ThreadID> _threadIds;
CriticalSection _criticalSection;
ICorProfilerInfo3 * _pProfilerInfo ;
UINT_PTR STDMETHODCALLTYPE FunctionMapper(FunctionID functionId, void * clientData, BOOL *pbHookFunction);
void  Callback( void *arg );



HRESULT STDMETHODCALLTYPE CSamplingProfiler::Initialize( /* [in] */ IUnknown *pICorProfilerInfoUnk) {
	CorProfilerCallbackBase::Initialize(pICorProfilerInfoUnk);

	DWORD mask =  COR_PRF_MONITOR_THREADS | COR_PRF_DISABLE_INLINING | COR_PRF_ENABLE_STACK_SNAPSHOT ;
	this->pProfilerInfo->SetEventMask(mask);
	Beep(2000, 100);
	_pProfilerInfo = pProfilerInfo;
	HANDLE hdl = (HANDLE)_beginthread(Callback, 0, pProfilerInfo);

	return S_OK;
}

HRESULT __stdcall StackSnapshot(FunctionID functionId, UINT_PTR ip, COR_PRF_FRAME_INFO frameInfo, ULONG32 contextSize, BYTE context[], void *clientData){
	vector<FunctionID>  * functionIds = (vector<FunctionID>  *) clientData;
	functionIds->push_back(functionId);
	return S_OK;
}

void  Callback( void *arg )
{
	ICorProfilerInfo3 * pProfilerInfo = (ICorProfilerInfo3*) arg;

	//	profInfo->DoStackSnapshot(
	while(CSamplingProfiler::_continueSampling){
		_criticalSection.Enter();
		vector<FunctionID> functionIds;
		for(set<ThreadID>::iterator it = _threadIds.begin(); it != _threadIds.end(); it++){
			ThreadID threadId = *it;
			functionIds.clear();
			HRESULT hr = pProfilerInfo->DoStackSnapshot(threadId, StackSnapshot, COR_PRF_SNAPSHOT_DEFAULT, &functionIds, NULL, 0);
			if(hr == CORPROF_E_STACKSNAPSHOT_UNMANAGED_CTX){
				cout << endl << "!!!!!!!!!!!! !!!!!!!!!!!!!! Unamaged frame" << endl;
			}
			if(SUCCEEDED(hr)){
				for(vector<FunctionID>::iterator it2 =functionIds.begin(); it2 != functionIds.end(); it2++){
					FunctionID functionId = *it2;
					if(functionId == 0)
						continue;
					shared_ptr<MethodMetadata> pMethodMetadata = MethodMetadata::GetById(functionId);
					if(pMethodMetadata == NULL){
						pMethodMetadata = shared_ptr<MethodMetadata>(new MethodMetadata(functionId, pProfilerInfo));
						MethodMetadata::AddMetadata(functionId, pMethodMetadata);
					}
					if(pMethodMetadata->GetDefiningAssembly()->IsProfilingEnabled){
						CSamplingProfiler::_functionIds.push_back(functionId);
					}
				}
			}
		}
		
		_criticalSection.Leave();
	//	Sleep(5);
	}

}

HRESULT STDMETHODCALLTYPE  CSamplingProfiler::ThreadCreated(ThreadID threadId) {
	_criticalSection.Enter();
	_threadIds.insert(threadId);
	_criticalSection.Leave();

	return S_OK;
}

HRESULT STDMETHODCALLTYPE  CSamplingProfiler::ThreadDestroyed(ThreadID threadId) {
	_criticalSection.Enter();
	_threadIds.erase(threadId);
	_criticalSection.Leave();

	return S_OK;
}

HRESULT STDMETHODCALLTYPE  CSamplingProfiler::ThreadAssignedToOSThread(ThreadID managedThreadId, DWORD osThreadId) {
	return S_OK;
}


