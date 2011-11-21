#include "StdAfx.h"
#include "StatisticalCallTreeElem.h"

StatisticalCallTreeElem::StatisticalCallTreeElem(FunctionID functionId, StatisticalCallTreeElem * pParent)
	:CallTreeElemBase<StatisticalCallTreeElem>(functionId, pParent),StackTopOccurrenceCount(0), LastProfiledFrameInStackCount(0){}

void StatisticalCallTreeElem::ToString(wstringstream & wsout, wstring indentation, wstring indentationString){
	if(!IsRootElem()){
		MethodMetadata * pMethodMd = MethodMetadata::GetById(this->FunctionId).get();
		wsout << indentation << pMethodMd->ToString() << L",TopFrameCount=" << StackTopOccurrenceCount << ",LastProfiledFrameCount=" << LastProfiledFrameInStackCount ;
	}

	int stackDivisionCount = 0;
	for(map<FunctionID,shared_ptr<StatisticalCallTreeElem>>::iterator it = _pChildrenMap.begin(); it != _pChildrenMap.end(); it ++){
		if(IsRootElem()){
			wsout << endl << indentation << "-------------- Stack division "<< stackDivisionCount++ <<"--------------";
		}
		wsout << endl ;
		it->second->ToString(wsout,indentation + indentationString);
	}
}

void StatisticalCallTreeElem::Serialize(SerializationBuffer * buffer){
	buffer->SerializeFunctionId(FunctionId);
	buffer->SerializeUINT(StackTopOccurrenceCount);
	buffer->SerializeUINT(LastProfiledFrameInStackCount);
	
	UINT childrenSize = _pChildrenMap.size();
	buffer->SerializeUINT(childrenSize);

	map<FunctionID, shared_ptr<StatisticalCallTreeElem>>::iterator it = _pChildrenMap.begin();
	StatisticalCallTreeElem** ppElemArray = new StatisticalCallTreeElem*[childrenSize];
	for(UINT i = 0; i < childrenSize; i++){
		StatisticalCallTreeElem * elem = it->second.get();
		ppElemArray[i] = elem;
		it++;
	}

	for(UINT i = 0; i < childrenSize; i++){
		ppElemArray[i]->Serialize(buffer);
	}

	delete[] ppElemArray;
}