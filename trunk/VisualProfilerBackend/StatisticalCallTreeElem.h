#pragma once
#include "CallTreeElemBase.h"

class StatisticalCallTreeElem : public CallTreeElemBase<StatisticalCallTreeElem>{
public:
	UINT StackTopOccurrenceCount;
	UINT LastProfiledFrameInStackCount;

	StatisticalCallTreeElem(FunctionID functionId = 0, StatisticalCallTreeElem * pParent = NULL);
	void ToString(wstringstream & wsout, wstring indentation = L"", wstring indentationString = L"   ");

	void Serialize(SerializationBuffer * buffer);
};
