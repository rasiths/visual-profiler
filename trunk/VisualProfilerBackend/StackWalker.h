#pragma once
#include <cor.h>
#include <corprof.h>
#include <set>
#include "CriticalSection.h"
#include <vector>
#include <map>
#include "CallTreeElemBase.h"
#include "CallTreeBase.h"

class StatisticalCallTreeElem : public CallTreeElemBase<StatisticalCallTreeElem>{
public:
	UINT TopStackOccurrenceCount;

	StatisticalCallTreeElem(FunctionID functionId = 0, StatisticalCallTreeElem * pParent = NULL)
		:CallTreeElemBase<StatisticalCallTreeElem>(functionId, pParent),TopStackOccurrenceCount(0){}

	void ToString(wstringstream & wsout, wstring indentation = L"", wstring indentationString = L"   "){
		if(!IsRootElem()){
			MethodMetadata * pMethodMd = MethodMetadata::GetById(this->FunctionId).get();
			wsout << indentation << pMethodMd->ToString() << L",TopCount=" << TopStackOccurrenceCount ;
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
	StatisticalCallTree(ThreadID threadId):CallTreeBase<StatisticalCallTree, StatisticalCallTreeElem>(threadId){}

	void ProcessSamples(vector<FunctionID> * functionIdsSnapshot, ICorProfilerInfo3 * pProfilerInfo){

		StatisticalCallTreeElem * treeElem = &_rootCallTreeElem;
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
				treeElem = treeElem->GetChildTreeElem(functionId);
			}
		}
		StatisticalCallTreeElem * stackTopTreeElem = treeElem;
		stackTopTreeElem->TopStackOccurrenceCount++;
	}
};

class StackWalker
{
public:
	StackWalker(ICorProfilerInfo3 * pProfilerInfo, UINT samplingPeriodMs = 0):_pProfilerInfo(pProfilerInfo), _samplingPeriodMs(samplingPeriodMs){

	}

	void RegisterThread(ThreadID threadId){
		_criticalSection.Enter();
		{
			_registeredThreadIds.insert(threadId);
		}
		_criticalSection.Leave();
		StatisticalCallTree::AddThread(threadId);
	}

	void DeregisterThread(ThreadID threadId){
		_criticalSection.Enter();
		_registeredThreadIds.erase(threadId);
		_criticalSection.Leave();
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

