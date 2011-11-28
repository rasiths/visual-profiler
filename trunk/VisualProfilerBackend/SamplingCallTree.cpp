#include "StdAfx.h"
#include "SamplingCallTree.h"
#include <iostream>

SamplingCallTree::SamplingCallTree(ThreadID threadId):
CallTreeBase<SamplingCallTree, SamplingCallTreeElem>(threadId),
	OsThreadHandle(0), KernelModeDurationHns(0), UserModeDurationHns(0){
		CreationUserModeTimeStamp.dwHighDateTime=0;
		CreationUserModeTimeStamp.dwLowDateTime=0;
		CreationKernelModeTimeStamp.dwHighDateTime=0;
		CreationKernelModeTimeStamp.dwLowDateTime=0;
}

void printFT(FILETIME * ft){
	cout << ft->dwHighDateTime << ft->dwLowDateTime << endl;
}

void SamplingCallTree::ProcessSamples(vector<FunctionID> * functionIdsSnapshot, ICorProfilerInfo3 * pProfilerInfo){
	SamplingCallTreeElem * treeElem = &_rootCallTreeElem;
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
	
		if(pMethodMetadata->GetDefiningAssembly()->IsProfilingEnabled()){
			isProfilingEnabledOnElem = true;
			treeElem = treeElem->GetChildTreeElem(functionId);
		}else{
			isProfilingEnabledOnElem = false;
		}
	}

#pragma region waitJoinSleep
/*
		FILETIME dummy, userNow, kernelNow;
			GetThreadTimes(OsThreadHandle,&dummy,&dummy,&userNow,&kernelNow);
			bool kernelChanged =false, userchanged = false;
	
			if(user.dwHighDateTime != userNow.dwHighDateTime && user.dwLowDateTime != userNow.dwLowDateTime){
				user = userNow;
				userchanged = true;
		}
			
			if(kernel.dwHighDateTime != kernelNow.dwHighDateTime && kernel.dwLowDateTime != kernelNow.dwLowDateTime){
				kernel = kernelNow;
				kernelChanged = true;
		}

			ULONG64 ticks;
			QueryThreadCycleTime(OsThreadHandle, &ticks);
			cout << "threadID=" << _threadId << ", ticks=" << ticks << endl;
			printFT(&userNow);
			printFT(&kernelNow);
			cout << "--------------" <<endl;
	if(!(userchanged || kernelChanged))
		return;
	}*/
#pragma endregion
	bool wasProfilingEnabledOnStackTopFrame = isProfilingEnabledOnElem;
	if(wasProfilingEnabledOnStackTopFrame){
		treeElem->StackTopOccurrenceCount++;
	}else{
		treeElem->LastProfiledFrameInStackCount++;
	}
}

void SamplingCallTree::SetOsThreadInfo(DWORD osThreadId){
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

void SamplingCallTree::UpdateUserAndKernelModeDurations(){
	FILETIME dummy;
	FILETIME currentUserModeTimeStamp;
	FILETIME currentKernelModeTimeStamp;

	BOOL success = GetThreadTimes(OsThreadHandle,&dummy, &dummy,&currentKernelModeTimeStamp, &currentUserModeTimeStamp);
	CheckError2(success);
	SubtractFILETIMESAndAddToResult(&currentUserModeTimeStamp, &CreationUserModeTimeStamp, &UserModeDurationHns);
	SubtractFILETIMESAndAddToResult(&currentKernelModeTimeStamp, &CreationKernelModeTimeStamp, &KernelModeDurationHns);

}

void SamplingCallTree::ToString(wstringstream & wsout){
	wsout << "Thread Id = " << _threadId << ", Number of stack divisions = " << _rootCallTreeElem.GetChildrenMap()->size() <<  endl ;

	double durationSec = _timer.GetElapsedTimeIn100NanoSeconds()/1e7;
	double userModeSec = UserModeDurationHns/1e7;
	double kernelModeSec = KernelModeDurationHns/1e7;
	wsout << L"Twc=" << durationSec << L"s,Tum=" << userModeSec << L"s,Tkm=" << kernelModeSec << "s" << endl;

	_rootCallTreeElem.ToString(wsout);
}

void SamplingCallTree::Serialize(SerializationBuffer * buffer){
	buffer->SerializeProfilingDataTypes(ProfilingDataTypes_Sampling);
	buffer->SerializeThreadId(_threadId);
	
	ULONGLONG wallClockTime;
	_timer.GetElapsedTimeIn100NanoSeconds(&wallClockTime);
	buffer->SerializeULONGLONG(wallClockTime);
	
	UpdateUserAndKernelModeDurations();
	buffer->SerializeULONGLONG(KernelModeDurationHns);
	buffer->SerializeULONGLONG(UserModeDurationHns);

	SerializeCallTreeElem(&_rootCallTreeElem, buffer);
}

void SamplingCallTree::SerializeCallTreeElem(SamplingCallTreeElem * elem, SerializationBuffer * buffer){
	buffer->SerializeFunctionId(elem->FunctionId);
	buffer->SerializeUINT(elem->StackTopOccurrenceCount);
	buffer->SerializeUINT(elem->LastProfiledFrameInStackCount);
	
	map<FunctionID, shared_ptr<SamplingCallTreeElem>> * childrenMap =  elem->GetChildrenMap();
	UINT childrenSize = childrenMap->size();
	buffer->SerializeUINT(childrenSize);

	map<FunctionID, shared_ptr<SamplingCallTreeElem>>::iterator it = childrenMap->begin();
	
	for(; it != childrenMap->end(); it++){
		SamplingCallTreeElem * childElem = it->second.get();
		SerializeCallTreeElem(childElem, buffer);	
	}
}