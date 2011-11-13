#pragma once
#include <map>
#include <unordered_map>
#include <cor.h>
#include <corprof.h>
#include "ThreadTimer.h"
#include "CriticalSection.h"
#include <string>
#include <sstream>
#include <iostream>

using namespace std;

class ThreadCallTreeElem{
public:
	ThreadCallTreeElem * pParent;
	FunctionID FunctionId;
    UINT EnterCount;
	UINT LeaveCount;
	ULONGLONG WallClockDurationHns; //100 nanoseconds
	ULONGLONG LastEnterTimeStampHns; //100 nanoseconds
	unordered_map<FunctionID,shared_ptr<ThreadCallTreeElem>> pChildrenMap;
		
	ThreadCallTreeElem(FunctionID functionId = 0, ThreadCallTreeElem * pParent = NULL)
		:FunctionId(functionId), pParent(pParent), EnterCount(0), LeaveCount(0), WallClockDurationHns(0), LastEnterTimeStampHns(0) {};
	
	bool IsRootElem(){
		return this->FunctionId == 0 && this->pParent == NULL;
	}

	ThreadCallTreeElem * GetChildTreeElem(FunctionID functionId){
		ThreadCallTreeElem * childElem = pChildrenMap[functionId].get();
		bool missingInMap = childElem == NULL;
		if(missingInMap){
			childElem = new ThreadCallTreeElem(functionId, this);
			pChildrenMap[functionId] = shared_ptr<ThreadCallTreeElem>(childElem);
		}
		return childElem;
	}

	wstring ToString(wstring indentation = L"", wstring indentationString = L"   "){
		wstringstream wsout;
		MethodMetadata * pMethodMd = MethodMetadata::GetById(this->FunctionId).get();
		double durationMs = this->WallClockDurationHns/1e7;
		wsout << indentation << pMethodMd->ToString() << L", t=" << durationMs << L" ms, Ec=" << this->EnterCount << L", Lc=" << this->LeaveCount;
		for(unordered_map<FunctionID,shared_ptr<ThreadCallTreeElem>>::iterator it = pChildrenMap.begin(); it != pChildrenMap.end(); it ++){
			wstring childText = it->second->ToString(indentation + indentationString);
			wsout << endl << childText;
		}

		return wsout.str();
	}
};

class ThreadCallTree
{
public:
	ThreadCallTreeElem RootCallTreeElem;
	ThreadCallTreeElem * pActiveCallTreeElem;
	ThreadTimer Timer;
	ThreadID ThreadId;

	ThreadCallTree(ThreadID threadId):ThreadId(threadId){
		this->pActiveCallTreeElem = & this->RootCallTreeElem;	
	}

	void FunctionEnter(FunctionID functionId){
		this->pActiveCallTreeElem  = this->pActiveCallTreeElem->GetChildTreeElem(functionId);
		this->Timer.GetElapsedTimeIn100NanoSeconds(&pActiveCallTreeElem->LastEnterTimeStampHns);
		pActiveCallTreeElem->EnterCount++;
	}

	void FunctionLeave(FunctionID functionId){
		bool inconsistent = this->pActiveCallTreeElem->FunctionId != functionId;
		if(inconsistent) 
			CheckError(false);

		ULONGLONG actualTimeStamp;
		this->Timer.GetElapsedTimeIn100NanoSeconds(&actualTimeStamp);
		ULONGLONG funcitonDuration = actualTimeStamp - this->pActiveCallTreeElem->LastEnterTimeStampHns;
		this->pActiveCallTreeElem->WallClockDurationHns += funcitonDuration;

		this->pActiveCallTreeElem->LeaveCount++;
		this->pActiveCallTreeElem = this->pActiveCallTreeElem->pParent;
	}

	ThreadCallTreeElem * GetRealRootCallTreeElem(){
		bool noManagedCallTree = this->RootCallTreeElem.pChildrenMap.size() == 0;
		if(noManagedCallTree)
			return NULL;

		bool unexpectedSize = this->RootCallTreeElem.pChildrenMap.size() != 1;
		if(unexpectedSize)
			CheckError(false);
		ThreadCallTreeElem * pRealRootCallTreeElem  =this->RootCallTreeElem.pChildrenMap.begin()->second.get();
		return pRealRootCallTreeElem;
	}

	wstring ToString(){
		wstringstream wsout;
		wsout << "Thread Id = " << this->ThreadId;
		ThreadCallTreeElem * pRealRootCallTreeElem = GetRealRootCallTreeElem();
		if(pRealRootCallTreeElem != NULL){
			wsout << endl <<  pRealRootCallTreeElem->ToString();
		}
				
		return wsout.str();
	}

	
	
	static ThreadCallTree * AddThread(ThreadID threadId){
		shared_ptr<ThreadCallTree> pThreadCallTree(new ThreadCallTree(threadId));
		_criticalSection.Enter();
		_threadCallTreeMap[threadId] = pThreadCallTree;
		_criticalSection.Leave();
		return pThreadCallTree.get();
	}

	static ThreadCallTree * GetThreadCallTree(ThreadID threadId){
		_criticalSection.Enter();
		shared_ptr<ThreadCallTree> pThreadCallTree = _threadCallTreeMap[threadId];
		_criticalSection.Leave();
		return pThreadCallTree.get();
	}

	static map<ThreadID, shared_ptr<ThreadCallTree>> * GetThreadCallTreeMap(){
		return &_threadCallTreeMap;
	}


private:
	static map<ThreadID, shared_ptr<ThreadCallTree>> _threadCallTreeMap;
	static CriticalSection _criticalSection;
};

 map<ThreadID, shared_ptr<ThreadCallTree>> ThreadCallTree::_threadCallTreeMap;
 CriticalSection ThreadCallTree::_criticalSection;