#include "StdAfx.h"
#include "ThreadCallTree.h"


map<ThreadID, shared_ptr<ThreadCallTree>> ThreadCallTree::_threadCallTreeMap;
CriticalSection ThreadCallTree::_criticalSection;
__declspec(thread) HANDLE ThreadCallTree::_OSThreadHandle;

ThreadCallTree::ThreadCallTree(ThreadID threadId):_threadId(threadId){
	_pActiveCallTreeElem = & _rootCallTreeElem;	
}

void ThreadCallTree::FunctionEnter(FunctionID functionId){
	ThreadCallTreeElem * prevActiveElem = _pActiveCallTreeElem;
	ThreadCallTreeElem * nextActiveElem = _pActiveCallTreeElem->GetChildTreeElem(functionId);

	UpdateUserAndKernelMode(prevActiveElem, nextActiveElem);			
	_timer.GetElapsedTimeIn100NanoSeconds(&nextActiveElem->LastEnterTimeStampHns);
	nextActiveElem->EnterCount++;

	_pActiveCallTreeElem = nextActiveElem;
}

void ThreadCallTree::FunctionLeave(FunctionID functionId){
	ThreadCallTreeElem * prevActiveElem = _pActiveCallTreeElem;
	ThreadCallTreeElem * nextActiveElem = _pActiveCallTreeElem->pParent;

	ULONGLONG actualTimeStamp;
	_timer.GetElapsedTimeIn100NanoSeconds(&actualTimeStamp);
	ULONGLONG funcitonDuration = actualTimeStamp - prevActiveElem->LastEnterTimeStampHns;
	prevActiveElem->WallClockDurationHns += funcitonDuration;
	prevActiveElem->LeaveCount++;

	UpdateUserAndKernelMode(prevActiveElem, nextActiveElem);

	_pActiveCallTreeElem = nextActiveElem;
}

void ThreadCallTree::UpdateUserAndKernelMode(ThreadCallTreeElem * prevActiveElem, ThreadCallTreeElem* nextActiveElem){
	FILETIME dummy;
	GetThreadTimes(_OSThreadHandle,&dummy, &dummy, &nextActiveElem->LastEnterKernelModeTimeStamp, &nextActiveElem->LastEnterUserModeTimeStamp);
	SubtractFILETIMESAndAddToResult( &nextActiveElem->LastEnterKernelModeTimeStamp, &prevActiveElem->LastEnterKernelModeTimeStamp, &prevActiveElem->KernelModeDurationHns);
	SubtractFILETIMESAndAddToResult( &nextActiveElem->LastEnterUserModeTimeStamp,   &prevActiveElem->LastEnterUserModeTimeStamp,   &prevActiveElem->UserModeDurationHns);
}

ThreadCallTreeElem * ThreadCallTree::GetRealRootCallTreeElem(){
	bool noManagedCallTree = _rootCallTreeElem.GetChildrenMap()->size() == 0;
	if(noManagedCallTree)
		return NULL;

	bool unexpectedSize = _rootCallTreeElem.GetChildrenMap()->size() != 1;
	if(unexpectedSize)
		CheckError(false);
	ThreadCallTreeElem * pRealRootCallTreeElem  =_rootCallTreeElem.GetChildrenMap()->begin()->second.get();
	return pRealRootCallTreeElem;
}

ThreadTimer * ThreadCallTree::GetTimer(){
	return &_timer;
}

ThreadID ThreadCallTree::GetThreadId(){
	return _threadId;
}

void ThreadCallTree::SetOSThreadHandle(HANDLE osThreadHandle){
	_OSThreadHandle = osThreadHandle;
}

HANDLE ThreadCallTree::GetOSThreadHandle(){
	return _OSThreadHandle;
}

wstring ThreadCallTree::ToString(){
	wstringstream wsout;
	wsout << "Thread Id = " << _threadId;
	ThreadCallTreeElem * pRealRootCallTreeElem = GetRealRootCallTreeElem();
	if(pRealRootCallTreeElem != NULL){
		wsout << endl <<  pRealRootCallTreeElem->ToString();
	}

	return wsout.str();
}

ThreadCallTree * ThreadCallTree::AddThread(ThreadID threadId){
	shared_ptr<ThreadCallTree> pThreadCallTree(new ThreadCallTree(threadId));
	_criticalSection.Enter();
	_threadCallTreeMap[threadId] = pThreadCallTree;
	_criticalSection.Leave();
	return pThreadCallTree.get();
}

ThreadCallTree * ThreadCallTree::GetThreadCallTree(ThreadID threadId){
	_criticalSection.Enter();
	shared_ptr<ThreadCallTree> pThreadCallTree = _threadCallTreeMap[threadId];
	_criticalSection.Leave();
	return pThreadCallTree.get();
}

map<ThreadID, shared_ptr<ThreadCallTree>> * ThreadCallTree::GetThreadCallTreeMap(){
	return &_threadCallTreeMap;
}