#pragma once
#include <map>
#include <cor.h>
#include <corprof.h>
#include "ThreadTimer.h"
#include "CriticalSection.h"
#include "ThreadCallTreeElem.h"
#include <string>
#include <sstream>
#include <iostream>
#include "Utils.h"

using namespace std;

class ThreadCallTree
{
private:
	static map<ThreadID, shared_ptr<ThreadCallTree>> _threadCallTreeMap;
	static CriticalSection _criticalSection;
	__declspec(thread) static HANDLE _OSThreadHandle;
	ThreadCallTreeElem _rootCallTreeElem;
	ThreadCallTreeElem * _pActiveCallTreeElem;
	ThreadTimer _timer;
	ThreadID _threadId;

public:
	ThreadCallTree(ThreadID threadId);
	void FunctionEnter(FunctionID functionId);
	void FunctionLeave(FunctionID functionId);

	ThreadCallTreeElem * GetRealRootCallTreeElem();
	ThreadTimer * GetTimer();
	ThreadID GetThreadId();
	void SetOSThreadHandle(HANDLE osThreadHandle);
	HANDLE GetOSThreadHandle();
	wstring ToString();
	static ThreadCallTree * AddThread(ThreadID threadId);
	static ThreadCallTree * GetThreadCallTree(ThreadID threadId);
	static map<ThreadID, shared_ptr<ThreadCallTree>> * GetThreadCallTreeMap();

private:
	void UpdateUserAndKernelMode(ThreadCallTreeElem * prevActiveElem, ThreadCallTreeElem* nextActiveElem);
};
