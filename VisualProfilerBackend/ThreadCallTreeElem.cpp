#include "StdAfx.h"
#include "ThreadCallTreeElem.h"


ThreadCallTreeElem::ThreadCallTreeElem(FunctionID functionId , ThreadCallTreeElem * pParent)
	:FunctionId(functionId), pParent(pParent), EnterCount(0), LeaveCount(0), 
	WallClockDurationHns(0), LastEnterTimeStampHns(0), UserModeDurationHns(0),KernelModeDurationHns(0) {
		LastEnterKernelModeTimeStamp.dwHighDateTime = 0;
		LastEnterKernelModeTimeStamp.dwLowDateTime = 0;
		LastEnterUserModeTimeStamp.dwHighDateTime = 0;
		LastEnterUserModeTimeStamp.dwLowDateTime = 0;
};


bool ThreadCallTreeElem::IsRootElem(){
	return this->FunctionId == 0 && this->pParent == NULL;
}

ThreadCallTreeElem * ThreadCallTreeElem::GetChildTreeElem(FunctionID functionId){
	ThreadCallTreeElem * childElem = _pChildrenMap[functionId].get();
	bool missingInMap = childElem == NULL;
	if(missingInMap){
		childElem = new ThreadCallTreeElem(functionId, this);
		_pChildrenMap[functionId] = shared_ptr<ThreadCallTreeElem>(childElem);
	}
	return childElem;
}

wstring ThreadCallTreeElem::ToString(wstring indentation, wstring indentationString){
	wstringstream wsout;
	MethodMetadata * pMethodMd = MethodMetadata::GetById(this->FunctionId).get();
	double durationSec = this->WallClockDurationHns/1e7;
	double userModeSec = this->UserModeDurationHns/1e7;
	double kernelModeSec = this->KernelModeDurationHns/1e7;
	wsout << indentation << pMethodMd->ToString() << L",Twc=" << durationSec << L"s,Tum=" << userModeSec << L"s,Tkm=" << kernelModeSec << L"s,Ec=" << this->EnterCount << L",Lc=" << this->LeaveCount;
	for(map<FunctionID,shared_ptr<ThreadCallTreeElem>>::iterator it = _pChildrenMap.begin(); it != _pChildrenMap.end(); it ++){
		wstring childText = it->second->ToString(indentation + indentationString);
		wsout << endl << childText;
	}

	return wsout.str();
}


map<FunctionID,shared_ptr<ThreadCallTreeElem>> * ThreadCallTreeElem:: GetChildrenMap(){
	return &this->_pChildrenMap;
	}