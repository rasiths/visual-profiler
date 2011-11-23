#include "StdAfx.h"
#include "ThreadCallTreeElem.h"


ThreadCallTreeElem::ThreadCallTreeElem(FunctionID functionId , ThreadCallTreeElem * pParent)
	:CallTreeElemBase<ThreadCallTreeElem>(functionId, pParent),  EnterCount(0), LeaveCount(0), 
	WallClockDurationHns(0), LastEnterTimeStampHns(0), UserModeDurationHns(0),KernelModeDurationHns(0) {
		LastEnterKernelModeTimeStamp.dwHighDateTime = 0;
		LastEnterKernelModeTimeStamp.dwLowDateTime = 0;
		LastEnterUserModeTimeStamp.dwHighDateTime = 0;
		LastEnterUserModeTimeStamp.dwLowDateTime = 0;
};

void ThreadCallTreeElem::ToString(wstringstream & wsout, wstring indentation, wstring indentationString){
	if(!IsRootElem()){
		MethodMetadata * pMethodMd = MethodMetadata::GetById(this->FunctionId).get();
		/*double durationSec = this->WallClockDurationHns/1e7;
		double userModeSec = this->UserModeDurationHns/1e7;
		double kernelModeSec = this->KernelModeDurationHns/1e7;*/	
		ULONGLONG durationSec = this->WallClockDurationHns;//1e7;
		ULONGLONG userModeSec = this->UserModeDurationHns;//1e7;
		ULONGLONG kernelModeSec = this->KernelModeDurationHns;//1e7;
		wsout << indentation << pMethodMd->ToString() << L",Twc=" << durationSec << L"s,Tum=" << userModeSec << L"s,Tkm=" << kernelModeSec << L"s,Ec=" << this->EnterCount << L",Lc=" << this->LeaveCount;
	}
	
	int stackDivisionCount = 0;
	for(map<FunctionID,shared_ptr<ThreadCallTreeElem>>::iterator it = _pChildrenMap.begin(); it != _pChildrenMap.end(); it ++){
		if(IsRootElem()){
			wsout << endl << indentation << "-------------- Stack division "<< stackDivisionCount++ <<" --------------";
		}
		wsout << endl ;
		it->second->ToString(wsout,indentation + indentationString);
	}
}

void ThreadCallTreeElem::Serialize(SerializationBuffer * buffer){
	buffer->SerializeFunctionId(FunctionId);
	buffer->SerializeUINT(EnterCount);
	buffer->SerializeUINT(LeaveCount);
	buffer->SerializeULONGLONG(WallClockDurationHns); 
	buffer->SerializeULONGLONG(KernelModeDurationHns);
	buffer->SerializeULONGLONG(UserModeDurationHns);
	
	UINT childrenSize = _pChildrenMap.size();
	buffer->SerializeUINT(childrenSize);

	map<FunctionID, shared_ptr<ThreadCallTreeElem>>::iterator it = _pChildrenMap.begin();
	ThreadCallTreeElem** ppThreadCallTreeElemArray = new ThreadCallTreeElem*[childrenSize];
	for(UINT i = 0; i < childrenSize; i++){
		ThreadCallTreeElem * elem = it->second.get();
		ppThreadCallTreeElemArray[i] = elem;
		it++;
	}

	for(UINT i = 0; i < childrenSize; i++){
		ppThreadCallTreeElemArray[i]->Serialize(buffer);
	}

	delete[] ppThreadCallTreeElemArray;
}

