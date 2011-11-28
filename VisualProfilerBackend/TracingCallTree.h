#pragma once

#include "TracingCallTreeElem.h"
#include "CallTreeBase.h"

using namespace std;

class TracingCallTree : public CallTreeBase<TracingCallTree, TracingCallTreeElem>
{
private:
	HANDLE _OSThreadHandle;
	TracingCallTreeElem * _pActiveCallTreeElem;

public:
	TracingCallTree(ThreadID threadId);
	void FunctionEnter(FunctionID functionId);
	void FunctionLeave();
	TracingCallTreeElem * GetActiveCallTreeElem();
	void SetOSThreadHandle(HANDLE osThreadHandle);
	HANDLE GetOSThreadHandle();
	virtual void Serialize(SerializationBuffer * buffer);

private:
	void UpdateUserAndKernelMode(TracingCallTreeElem * prevActiveElem, TracingCallTreeElem* nextActiveElem);

protected:
	void SerializeCallTreeElem(TracingCallTreeElem * elem, SerializationBuffer * buffer);
};
