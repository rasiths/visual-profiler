#pragma once
#include "MethodMetadata.h"
#include "CallTreeElemBase.h"


class ThreadCallTreeElem: public CallTreeElemBase<ThreadCallTreeElem> { 
public:
	UINT EnterCount;
	UINT LeaveCount;
	ULONGLONG WallClockDurationHns; //100 nanoseconds
	ULONGLONG LastEnterTimeStampHns; //100 nanoseconds
	ULONGLONG UserModeDurationHns; //100 nanoseconds
	ULONGLONG KernelModeDurationHns; //100 nanoseconds
	FILETIME LastEnterUserModeTimeStamp;
	FILETIME LastEnterKernelModeTimeStamp;

	ThreadCallTreeElem(FunctionID functionId = 0, ThreadCallTreeElem * pParent = NULL);
	void ToString(wstringstream & wsout, wstring indentation = L"", wstring indentationString = L"   ");
	virtual void Serialize(SerializationBuffer * buffer);
};
