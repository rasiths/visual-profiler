#include "StdAfx.h"
#include "TracingCallTree.h"

TracingCallTree::TracingCallTree(ThreadID threadId):CallTreeBase<TracingCallTree, TracingCallTreeElem>(threadId){
	_pActiveCallTreeElem = & _rootCallTreeElem;	
}

void TracingCallTree::FunctionEnter(FunctionID functionId){

	TracingCallTreeElem * prevActiveElem = _pActiveCallTreeElem;
	TracingCallTreeElem * nextActiveElem = _pActiveCallTreeElem->GetChildTreeElem(functionId);

	UpdateUserAndKernelMode(prevActiveElem, nextActiveElem);			
	_timer.GetElapsedTimeIn100NanoSeconds(&nextActiveElem->LastEnterTimeStampHns);
	
	nextActiveElem->EnterCount++;

	_pActiveCallTreeElem = nextActiveElem;
	RefreshCallTreeBuffer();
}

void TracingCallTree::FunctionLeave(){
	TracingCallTreeElem * prevActiveElem = _pActiveCallTreeElem;
	TracingCallTreeElem * nextActiveElem = _pActiveCallTreeElem->pParent;
	
	ULONGLONG actualTimeStamp;
	_timer.GetElapsedTimeIn100NanoSeconds(&actualTimeStamp);
	ULONGLONG funcitonDuration = actualTimeStamp - prevActiveElem->LastEnterTimeStampHns;
	prevActiveElem->WallClockDurationHns += funcitonDuration;
	
	prevActiveElem->LeaveCount++;

	UpdateUserAndKernelMode(prevActiveElem, nextActiveElem);

	_pActiveCallTreeElem = nextActiveElem;

	RefreshCallTreeBuffer();
}

void TracingCallTree::UpdateUserAndKernelMode(TracingCallTreeElem * prevActiveElem, TracingCallTreeElem* nextActiveElem){
	FILETIME dummy;
	GetThreadTimes(_OSThreadHandle,&dummy, &dummy, &nextActiveElem->LastEnterKernelModeTimeStamp, &nextActiveElem->LastEnterUserModeTimeStamp);
	SubtractFILETIMESAndAddToResult( &nextActiveElem->LastEnterKernelModeTimeStamp, &prevActiveElem->LastEnterKernelModeTimeStamp, &prevActiveElem->KernelModeDurationHns);
	SubtractFILETIMESAndAddToResult( &nextActiveElem->LastEnterUserModeTimeStamp,   &prevActiveElem->LastEnterUserModeTimeStamp, &prevActiveElem->UserModeDurationHns);
}

TracingCallTreeElem * TracingCallTree::GetActiveCallTreeElem(){
	return _pActiveCallTreeElem;
}

void TracingCallTree::SetOSThreadHandle(HANDLE osThreadHandle){
	_OSThreadHandle = osThreadHandle;
}

HANDLE TracingCallTree::GetOSThreadHandle(){
	return _OSThreadHandle;
}

void TracingCallTree::Serialize(SerializationBuffer * buffer){
	buffer->SerializeProfilingDataTypes(ProfilingDataTypes_Tracing);
	buffer->SerializeThreadId(_threadId);
	SerializeCallTreeElem(&_rootCallTreeElem, buffer);
}

void TracingCallTree::SerializeCallTreeElem(TracingCallTreeElem * elem, SerializationBuffer * buffer){
	buffer->SerializeFunctionId(elem->FunctionId);
	buffer->SerializeUINT(elem->EnterCount);
	buffer->SerializeUINT(elem->LeaveCount);

	bool functionNotFinished = elem->EnterCount != elem->LeaveCount;
	if(functionNotFinished){
		ULONGLONG actualTimeStamp;
		_timer.GetElapsedTimeIn100NanoSeconds(&actualTimeStamp);
		ULONGLONG funcitonDuration = actualTimeStamp - elem->LastEnterTimeStampHns + elem->WallClockDurationHns;
		buffer->SerializeULONGLONG(actualTimeStamp); 
	}else{
		buffer->SerializeULONGLONG(elem->WallClockDurationHns);
	}
	buffer->SerializeULONGLONG(elem->KernelModeDurationHns);
	buffer->SerializeULONGLONG(elem->UserModeDurationHns);
	
	map<FunctionID, shared_ptr<TracingCallTreeElem>> * pChildrenMap = elem->GetChildrenMap();
	UINT childrenSize = pChildrenMap->size();
	buffer->SerializeUINT(childrenSize);

	map<FunctionID, shared_ptr<TracingCallTreeElem>>::iterator it = pChildrenMap->begin();
	for(; it != pChildrenMap->end(); it++){
		TracingCallTreeElem * childElem = it->second.get();
		SerializeCallTreeElem(childElem, buffer);
	}
}
