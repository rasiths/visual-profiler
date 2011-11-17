#pragma once

#include "ThreadCallTreeElem.h"
#include "CallTreeBase.h"

using namespace std;

class ThreadCallTree : public CallTreeBase<ThreadCallTree, ThreadCallTreeElem>
{
private:
	__declspec(thread) static HANDLE _OSThreadHandle;
	ThreadCallTreeElem * _pActiveCallTreeElem;

public:
	ThreadCallTree(ThreadID threadId);
	void FunctionEnter(FunctionID functionId);
	void FunctionLeave();
	ThreadCallTreeElem * GetActiveCallTreeElem();
	void SetOSThreadHandle(HANDLE osThreadHandle);
	HANDLE GetOSThreadHandle();

private:
	void UpdateUserAndKernelMode(ThreadCallTreeElem * prevActiveElem, ThreadCallTreeElem* nextActiveElem);
};
