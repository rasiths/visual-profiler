#pragma once

#include "ThreadCallTreeElem.h"
#include "CallTreeBase.h"

using namespace std;

class ThreadCallTree : public CallTreeBase<ThreadCallTree, ThreadCallTreeElem>
{
private:
	__declspec(thread) static HANDLE _OSThreadHandle;
	ThreadCallTreeElem * _pActiveCallTreeElem;
	ThreadTimer _timer;

public:
	ThreadCallTree(ThreadID threadId);
	void FunctionEnter(FunctionID functionId);
	void FunctionLeave();
	ThreadCallTreeElem * GetActiveCallTreeElem();
	ThreadTimer * GetTimer();
	void SetOSThreadHandle(HANDLE osThreadHandle);
	HANDLE GetOSThreadHandle();

private:
	void UpdateUserAndKernelMode(ThreadCallTreeElem * prevActiveElem, ThreadCallTreeElem* nextActiveElem);
};
