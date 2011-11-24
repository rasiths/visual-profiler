#pragma once

#include "ThreadCallTreeElem.h"
#include "CallTreeBase.h"

using namespace std;

class ThreadCallTree : public CallTreeBase<ThreadCallTree, ThreadCallTreeElem>
{
private:
	HANDLE _OSThreadHandle;
	ThreadCallTreeElem * _pActiveCallTreeElem;

public:
	ThreadCallTree(ThreadID threadId);
	void FunctionEnter(FunctionID functionId);
	void FunctionLeave();
	ThreadCallTreeElem * GetActiveCallTreeElem();
	void SetOSThreadHandle(HANDLE osThreadHandle);
	HANDLE GetOSThreadHandle();
	virtual void Serialize(SerializationBuffer * buffer);

private:
	void UpdateUserAndKernelMode(ThreadCallTreeElem * prevActiveElem, ThreadCallTreeElem* nextActiveElem);

protected:
	void SerializeCallTreeElem(ThreadCallTreeElem * elem, SerializationBuffer * buffer);
};
