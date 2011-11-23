#include "StdAfx.h"
#include "ThreadCallTree.h"

ThreadCallTree::ThreadCallTree(ThreadID threadId):CallTreeBase<ThreadCallTree, ThreadCallTreeElem>(threadId){
	_pActiveCallTreeElem = & _rootCallTreeElem;	
}

void ThreadCallTree::FunctionEnter(FunctionID functionId){

	ThreadCallTreeElem * prevActiveElem = _pActiveCallTreeElem;
	ThreadCallTreeElem * nextActiveElem = _pActiveCallTreeElem->GetChildTreeElem(functionId);

	UpdateUserAndKernelMode(prevActiveElem, nextActiveElem);			
	_timer.GetElapsedTimeIn100NanoSeconds(&nextActiveElem->LastEnterTimeStampHns);
	nextActiveElem->EnterCount++;

	_pActiveCallTreeElem = nextActiveElem;
	RefreshCallTreeBuffer();
}

void ThreadCallTree::FunctionLeave(){
	ThreadCallTreeElem * prevActiveElem = _pActiveCallTreeElem;
	ThreadCallTreeElem * nextActiveElem = _pActiveCallTreeElem->pParent;


	ULONGLONG actualTimeStamp;
	_timer.GetElapsedTimeIn100NanoSeconds(&actualTimeStamp);
	ULONGLONG funcitonDuration = actualTimeStamp - prevActiveElem->LastEnterTimeStampHns;
	prevActiveElem->WallClockDurationHns += funcitonDuration;
	prevActiveElem->LeaveCount++;

	UpdateUserAndKernelMode(prevActiveElem, nextActiveElem);

	_pActiveCallTreeElem = nextActiveElem;

	RefreshCallTreeBuffer();
}

void ThreadCallTree::UpdateUserAndKernelMode(ThreadCallTreeElem * prevActiveElem, ThreadCallTreeElem* nextActiveElem){
	FILETIME dummy;
	GetThreadTimes(_OSThreadHandle,&dummy, &dummy, &nextActiveElem->LastEnterKernelModeTimeStamp, &nextActiveElem->LastEnterUserModeTimeStamp);
	SubtractFILETIMESAndAddToResult( &nextActiveElem->LastEnterKernelModeTimeStamp, &prevActiveElem->LastEnterKernelModeTimeStamp, &prevActiveElem->KernelModeDurationHns);
	SubtractFILETIMESAndAddToResult( &nextActiveElem->LastEnterUserModeTimeStamp,   &prevActiveElem->LastEnterUserModeTimeStamp,   &prevActiveElem->UserModeDurationHns);
}

ThreadCallTreeElem * ThreadCallTree::GetActiveCallTreeElem(){
	return _pActiveCallTreeElem;
}

void ThreadCallTree::SetOSThreadHandle(HANDLE osThreadHandle){
	_OSThreadHandle = osThreadHandle;
}

HANDLE ThreadCallTree::GetOSThreadHandle(){
	return _OSThreadHandle;
}

void ThreadCallTree::Serialize(SerializationBuffer * buffer){
	buffer->SerializeProfilingDataTypes(ProfilingDataTypes_Tracing);
	buffer->SerializeThreadId(_threadId);
	_rootCallTreeElem.Serialize(buffer);
}


