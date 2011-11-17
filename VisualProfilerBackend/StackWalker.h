#pragma once
#include <cor.h>
#include <corprof.h>
#include <set>
#include "CriticalSection.h"
#include <vector>
#include <map>
#include "CallTreeElemBase.h"
#include "CallTreeBase.h"
#include <iostream>
#include "ThreadTimer.h"


class StatisticalCallTreeElem : public CallTreeElemBase<StatisticalCallTreeElem>{
public:
	UINT StackTopOccurrenceCount;
	UINT LastProfiledFrameInStackCount;

	StatisticalCallTreeElem(FunctionID functionId = 0, StatisticalCallTreeElem * pParent = NULL)
		:CallTreeElemBase<StatisticalCallTreeElem>(functionId, pParent),StackTopOccurrenceCount(0), LastProfiledFrameInStackCount(0){}

	void ToString(wstringstream & wsout, wstring indentation = L"", wstring indentationString = L"   "){
		if(!IsRootElem()){
			MethodMetadata * pMethodMd = MethodMetadata::GetById(this->FunctionId).get();
			wsout << indentation << pMethodMd->ToString() << L"TopCount=" << StackTopOccurrenceCount << ",LastProfiledCount=" << LastProfiledFrameInStackCount ;
		}

		int stackDivisionCount = 0;
		for(map<FunctionID,shared_ptr<StatisticalCallTreeElem>>::iterator it = _pChildrenMap.begin(); it != _pChildrenMap.end(); it ++){
			if(IsRootElem()){
				wsout << endl << indentation << "-------------- Stack division "<< stackDivisionCount++ <<"--------------";
			}
			wsout << endl ;
			it->second->ToString(wsout,indentation + indentationString);
		}
	}
};

class StatisticalCallTree : public CallTreeBase<StatisticalCallTree, StatisticalCallTreeElem> {
public:
	FILETIME CreationUserModeTimeStamp;
	FILETIME CreationKernelModeTimeStamp;

	ULONGLONG KernelModeDurationHns;
	ULONGLONG UserModeDurationHns;

	HANDLE OsThreadHandle;
	DWORD OsThreadId;

	StatisticalCallTree(ThreadID threadId):
	CallTreeBase<StatisticalCallTree, StatisticalCallTreeElem>(threadId),
		OsThreadHandle(0), KernelModeDurationHns(0), UserModeDurationHns(0){
			CreationUserModeTimeStamp.dwHighDateTime=0;
			CreationUserModeTimeStamp.dwLowDateTime=0;
			CreationKernelModeTimeStamp.dwHighDateTime=0;
			CreationKernelModeTimeStamp.dwLowDateTime=0;
	}

	void ProcessSamples(vector<FunctionID> * functionIdsSnapshot, ICorProfilerInfo3 * pProfilerInfo){
		StatisticalCallTreeElem * treeElem = &_rootCallTreeElem;
		bool isProfilingEnabledOnElem = false;
		for(vector<FunctionID>::reverse_iterator it = functionIdsSnapshot->rbegin(); it < functionIdsSnapshot->rend(); it++){
			FunctionID functionId = *it;
			if(functionId == 0)
				continue;

			shared_ptr<MethodMetadata> pMethodMetadata = MethodMetadata::GetById(functionId);
			if(pMethodMetadata == NULL){
				pMethodMetadata = shared_ptr<MethodMetadata>(new MethodMetadata(functionId, pProfilerInfo));
				MethodMetadata::AddMetadata(functionId, pMethodMetadata);
			}

			if(pMethodMetadata->GetDefiningAssembly()->IsProfilingEnabled){
				isProfilingEnabledOnElem = true;
				treeElem = treeElem->GetChildTreeElem(functionId);
			}else{
				isProfilingEnabledOnElem = false;
			}
		}

		bool wasProfilingEnabledOnStackTopFrame = isProfilingEnabledOnElem;
		if(wasProfilingEnabledOnStackTopFrame){
			treeElem->StackTopOccurrenceCount++;
		}else{
			treeElem->LastProfiledFrameInStackCount++;
		}
	}

	void SetOsThreadInfo(DWORD osThreadId){
		HANDLE osThreadHandle = OpenThread(THREAD_QUERY_INFORMATION,false,osThreadId);
		if(osThreadHandle == NULL){
			CheckError(false);
		}

		OsThreadHandle = osThreadHandle;
		OsThreadId = osThreadId;

		FILETIME dummy;
		BOOL success = GetThreadTimes(OsThreadHandle,&dummy, &dummy,&CreationKernelModeTimeStamp, &CreationUserModeTimeStamp);
		CheckError2(success);
	}

	void UpdateUserAndKernelModeDurations(){
		FILETIME dummy;
		FILETIME currentUserModeTimeStamp;
		FILETIME currentKernelModeTimeStamp;

		BOOL success = GetThreadTimes(OsThreadHandle,&dummy, &dummy,&currentKernelModeTimeStamp, &currentUserModeTimeStamp);
		CheckError2(success);
		SubtractFILETIMESAndAddToResult(&currentUserModeTimeStamp, &CreationUserModeTimeStamp, &UserModeDurationHns);
		SubtractFILETIMESAndAddToResult(&currentKernelModeTimeStamp, &CreationKernelModeTimeStamp, &KernelModeDurationHns);
	}

	

	virtual void ToString(wstringstream & wsout){
		wsout << "Thread Id = " << _threadId << ", Number of stack divisions = " << _rootCallTreeElem.GetChildrenMap()->size() <<  endl ;

		double durationSec = _timer.GetElapsedTimeIn100NanoSeconds()/1e7;
		double userModeSec = UserModeDurationHns/1e7;
		double kernelModeSec = KernelModeDurationHns/1e7;
		wsout << L",Twc=" << durationSec << L"s,Tum=" << userModeSec << L"s,Tkm=" << kernelModeSec << endl;
		
		_rootCallTreeElem.ToString(wsout);
	}

};

class StackWalker
{
public:
	StackWalker(ICorProfilerInfo3 * pProfilerInfo, UINT samplingPeriodMs = 0):_pProfilerInfo(pProfilerInfo), _samplingPeriodMs(samplingPeriodMs){}

	void RegisterThread(ThreadID threadId){
		_criticalSection.Enter();
		{
			_registeredThreadIds.insert(threadId);
		}
		_criticalSection.Leave();
		StatisticalCallTree * callTree = StatisticalCallTree::AddThread(threadId);

		DWORD osThreadId;
		_pProfilerInfo->GetThreadInfo(threadId, &osThreadId);
		callTree->SetOsThreadInfo(osThreadId);

		callTree->GetTimer()->Start();
	}

	void DeregisterThread(ThreadID threadId){
		_criticalSection.Enter();
		{
			_registeredThreadIds.erase(threadId);
		}
		_criticalSection.Leave();

		StatisticalCallTree * callTree = StatisticalCallTree::GetCallTree(threadId);
		callTree->GetTimer()->Stop();
		callTree->UpdateUserAndKernelModeDurations();
	}

private:
	static DWORD WINAPI Sample(void * data){
		StackWalker * pThis = (StackWalker *) data;
		vector<FunctionID> functionIdsSnapshot;
		HRESULT hr = 0;

		while(pThis->_continueSampling){
			pThis->_criticalSection.Enter();
			{
				for(set<ThreadID>::iterator it = pThis->_registeredThreadIds.begin(); it != pThis->_registeredThreadIds.end(); it++){
					ThreadID threadId = *it;
					functionIdsSnapshot.clear();
					hr = pThis->_pProfilerInfo->DoStackSnapshot(threadId,&MakeFrameWalk,0, &functionIdsSnapshot,0,0);
					if(SUCCEEDED(hr)){
						StatisticalCallTree * pCallTree = StatisticalCallTree::GetCallTree(threadId);
						pCallTree->ProcessSamples(&functionIdsSnapshot, pThis->_pProfilerInfo);
					}
				}
			}

			pThis->_criticalSection.Leave();

			if(!pThis->_continueSampling) break;
			Sleep(pThis->_samplingPeriodMs);
		}

		DWORD success = 1;
		return success;
	}

	void DeregisterAllRegisteredThreads(){
		UINT registeredThreadIdsCount = _registeredThreadIds.size();
		ThreadID * threadIds = new ThreadID[registeredThreadIdsCount];
		
		UINT i = 0;
		for(set<ThreadID>::iterator it = _registeredThreadIds.begin(); it != _registeredThreadIds.end(); it++){
			ThreadID threadId = *it;
			threadIds[i++] = threadId;
		}

		for(i = 0; i < registeredThreadIdsCount; i++){
			ThreadID threadId = threadIds[i];
			DeregisterThread(threadId);
		}

	}

	static HRESULT __stdcall  MakeFrameWalk(FunctionID functionId, UINT_PTR ip, COR_PRF_FRAME_INFO frameInfo, ULONG32 contextSize, BYTE context[], void *clientData){
		vector<FunctionID>  * functionIds = (vector<FunctionID>  *) clientData;
		functionIds->push_back(functionId);
		return S_OK;
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
		DeregisterAllRegisteredThreads();
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

};

