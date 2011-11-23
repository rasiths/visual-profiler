#include "StdAfx.h"
#include "StatisticalCallTree.h"
#include <iostream>

StatisticalCallTree::StatisticalCallTree(ThreadID threadId):
CallTreeBase<StatisticalCallTree, StatisticalCallTreeElem>(threadId),
	OsThreadHandle(0), KernelModeDurationHns(0), UserModeDurationHns(0){
		CreationUserModeTimeStamp.dwHighDateTime=0;
		CreationUserModeTimeStamp.dwLowDateTime=0;
		CreationKernelModeTimeStamp.dwHighDateTime=0;
		CreationKernelModeTimeStamp.dwLowDateTime=0;
}

void printFT(FILETIME * ft){
	cout << ft->dwHighDateTime << ft->dwLowDateTime << endl;
}

void StatisticalCallTree::ProcessSamples(vector<FunctionID> * functionIdsSnapshot, ICorProfilerInfo3 * pProfilerInfo){
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



void StatisticalCallTree::SetOsThreadInfo(DWORD osThreadId){
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

void StatisticalCallTree::UpdateUserAndKernelModeDurations(){
	FILETIME dummy;
	FILETIME currentUserModeTimeStamp;
	FILETIME currentKernelModeTimeStamp;

	BOOL success = GetThreadTimes(OsThreadHandle,&dummy, &dummy,&currentKernelModeTimeStamp, &currentUserModeTimeStamp);
	CheckError2(success);
	SubtractFILETIMESAndAddToResult(&currentUserModeTimeStamp, &CreationUserModeTimeStamp, &UserModeDurationHns);
	SubtractFILETIMESAndAddToResult(&currentKernelModeTimeStamp, &CreationKernelModeTimeStamp, &KernelModeDurationHns);

}

void StatisticalCallTree::ToString(wstringstream & wsout){
	wsout << "Thread Id = " << _threadId << ", Number of stack divisions = " << _rootCallTreeElem.GetChildrenMap()->size() <<  endl ;

	double durationSec = _timer.GetElapsedTimeIn100NanoSeconds()/1e7;
	double userModeSec = UserModeDurationHns/1e7;
	double kernelModeSec = KernelModeDurationHns/1e7;
	wsout << L"Twc=" << durationSec << L"s,Tum=" << userModeSec << L"s,Tkm=" << kernelModeSec << "s" << endl;

	_rootCallTreeElem.ToString(wsout);
}

void StatisticalCallTree::Serialize(SerializationBuffer * buffer){
	buffer->SerializeProfilingDataTypes(ProfilingDataTypes_Sampling);
	buffer->SerializeThreadId(_threadId);
	
	ULONGLONG wallClockTime;
	_timer.GetElapsedTimeIn100NanoSeconds(&wallClockTime);
	buffer->SerializeULONGLONG(wallClockTime);
	
	UpdateUserAndKernelModeDurations();
	buffer->SerializeULONGLONG(KernelModeDurationHns);
	buffer->SerializeULONGLONG(UserModeDurationHns);

	_rootCallTreeElem.Serialize(buffer);
}