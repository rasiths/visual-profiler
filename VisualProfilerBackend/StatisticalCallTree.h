#pragma once

#include "CallTreeBase.h"
#include "StatisticalCallTreeElem.h"

class StatisticalCallTree : public CallTreeBase<StatisticalCallTree, StatisticalCallTreeElem> {
public:
	FILETIME CreationUserModeTimeStamp;
	FILETIME CreationKernelModeTimeStamp;

	ULONGLONG KernelModeDurationHns;
	ULONGLONG UserModeDurationHns;

	HANDLE OsThreadHandle;
	DWORD OsThreadId;

	StatisticalCallTree(ThreadID threadId);
	void ProcessSamples(vector<FunctionID> * functionIdsSnapshot, ICorProfilerInfo3 * pProfilerInfo);
	void SetOsThreadInfo(DWORD osThreadId);
	void UpdateUserAndKernelModeDurations();
	virtual void ToString(wstringstream & wsout);

private:
	#pragma region waitJoinSleep
	//FILETIME user;
	//FILETIME kernel;
	#pragma endregion

};