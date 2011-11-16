#pragma once
#include <cor.h>
#include <corprof.h>
#include <set>
#include "CriticalSection.h"
#include <vector>
#include <map>

class StatisticalCallTreeElem{
public:
	FunctionID FunctionId;
	StatisticalCallTreeElem * Parent;
	UINT TopOfStackCount;
	UINT BelowTopOfStackCount;

	private:
	map<FunctionID,shared_ptr<StatisticalCallTreeElem>> _pChildrenMap;
};

class StatisticalCallTree{


};

class StackWalker
{
public:
	StackWalker(ICorProfilerInfo3 * pProfilerInfo, UINT samplingPeriodMs = 0):_pProfilerInfo(pProfilerInfo), _samplingPeriodMs(samplingPeriodMs){

	}

	void RegisterThread(ThreadID threadId){
		_criticalSection.Enter();
		_registeredThreadIds.insert(threadId);
		_criticalSection.Leave();
	}

	void DeregisterThread(ThreadID threadId){
		_criticalSection.Enter();
		_registeredThreadIds.erase(threadId);
		_criticalSection.Leave();
	}
private:
	static DWORD WINAPI Sample(void * data){
		StackWalker * pThis = (StackWalker *) data;
		vector<FunctionID> functionIds;
		HRESULT hr = 0;

		while(pThis->_continueSampling){
			pThis->_criticalSection.Enter();
			{
				for(set<ThreadID>::iterator it = pThis->_registeredThreadIds.begin(); it != pThis->_registeredThreadIds.end(); it++){
					ThreadID threadId = *it;
					functionIds.clear();
					hr = pThis->_pProfilerInfo->DoStackSnapshot(threadId,&MakeFrameWalk,0, &functionIds,0,0);
					if(SUCCEEDED(hr)){
						pThis->ProcessSamples(&functionIds, threadId);
					}
				}
			}

			pThis->_criticalSection.Leave();

			if(!pThis->_continueSampling) break;
			Sleep(pThis->_samplingPeriodMs);
		}
	}

	static HRESULT __stdcall  MakeFrameWalk(FunctionID functionId, UINT_PTR ip, COR_PRF_FRAME_INFO frameInfo, ULONG32 contextSize, BYTE context[], void *clientData){
		vector<FunctionID>  * functionIds = (vector<FunctionID>  *) clientData;
		functionIds->push_back(functionId);
		return S_OK;
	}

	void ProcessSamples(vector<FunctionID> * functionIds, ThreadID threadId){

	}

public:
	void StartSampling(){
		bool alreadyStarted = _continueSampling == true;
		if( alreadyStarted) return;
		_continueSampling = true;
		_samplingThreadHandle = CreateThread(NULL, 0, &Sample,this, 0, &_samplingThreadId);
	}

	void StopSampling(){
		_continueSampling = false;
		WaitForSingleObject(_samplingThreadHandle, INFINITE);
	}


private:
	bool _continueSampling;
	CriticalSection _criticalSection;
	ICorProfilerInfo3 * _pProfilerInfo;
	UINT _samplingPeriodMs;
	set<ThreadID> _registeredThreadIds;
	DWORD _samplingThreadId;
	HANDLE _samplingThreadHandle;
	//map<ThreadID, shared_ptr<ThreadCallTree>> _threadCallTreeMap;
};

