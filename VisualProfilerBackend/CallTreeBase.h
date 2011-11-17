#pragma once
#include <map>
#include <cor.h>
#include <corprof.h>
#include "ThreadTimer.h"
#include "CriticalSection.h"
#include <string>
#include <sstream>
#include "Utils.h"

using namespace std;

template<class TCallTree, class TTreeElem>
class CallTreeBase{
protected :
	static map<ThreadID, shared_ptr<TCallTree>> _callTreeMap;
	static CriticalSection _criticalSection;

	TTreeElem _rootCallTreeElem;
	ThreadID _threadId;
	ThreadTimer _timer;
public:

	CallTreeBase(ThreadID threadId):_threadId(threadId){};

	ThreadID GetThreadId(){
		return _threadId;
	}

	ThreadTimer * GetTimer(){
		return &_timer;
	}

	virtual void ToString(wstringstream & wsout){
		wsout << "Thread Id = " << _threadId << ", Number of stack divisions = " << _rootCallTreeElem.GetChildrenMap()->size() <<  endl ;
		_rootCallTreeElem.ToString(wsout);
	}

	static TCallTree * AddThread(ThreadID threadId){
		shared_ptr<TCallTree> pCallTree(new TCallTree(threadId));
		_criticalSection.Enter();
		{
			_callTreeMap[threadId] = pCallTree;
		}
		_criticalSection.Leave();
		return pCallTree.get();
	}

	static TCallTree * GetCallTree(ThreadID threadId){
		shared_ptr<TCallTree> pCallTree;
		_criticalSection.Enter();
		{
			pCallTree = _callTreeMap[threadId];
		}
		_criticalSection.Leave();
		return pCallTree.get();
	}

	static map<ThreadID, shared_ptr<TCallTree>> * GetCallTreeMap(){
		return &_callTreeMap;
	}

};
template<class TCallTree, class TTreeElem>
map<ThreadID, shared_ptr<TCallTree>> CallTreeBase<TCallTree, TTreeElem>::_callTreeMap;

template<class TCallTree, class TTreeElem>
CriticalSection CallTreeBase<TCallTree, TTreeElem>::_criticalSection;