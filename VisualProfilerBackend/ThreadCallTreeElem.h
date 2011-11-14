#pragma once
#include <map>
#include <cor.h>
#include <corprof.h>
#include <string>
#include <sstream>
#include <iostream>
#include "MethodMetadata.h"

class ThreadCallTreeElem{
public:
	ThreadCallTreeElem * pParent;
	FunctionID FunctionId;
    UINT EnterCount;
	UINT LeaveCount;
	ULONGLONG WallClockDurationHns; //100 nanoseconds
	ULONGLONG LastEnterTimeStampHns; //100 nanoseconds
	ULONGLONG UserModeDurationHns; //100 nanoseconds
	ULONGLONG KernelModeDurationHns; //100 nanoseconds
	FILETIME LastEnterUserModeTimeStamp;
	FILETIME LastEnterKernelModeTimeStamp;
	
	ThreadCallTreeElem(FunctionID functionId = 0, ThreadCallTreeElem * pParent = NULL);
	bool IsRootElem();
	ThreadCallTreeElem * GetChildTreeElem(FunctionID functionId);
	wstring ToString(wstring indentation = L"", wstring indentationString = L"   ");
	map<FunctionID,shared_ptr<ThreadCallTreeElem>> * GetChildrenMap();

private:
	map<FunctionID,shared_ptr<ThreadCallTreeElem>> _pChildrenMap;
};
