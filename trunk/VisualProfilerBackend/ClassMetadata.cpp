#include "StdAfx.h"
#include "ClassMetadata.h"


ClassMetadata::ClassMetadata(ClassID classId, ICorProfilerInfo3 & profilerInfo, IMetaDataImport2* pMetadataImport): ClassId(classId)
{
	HRESULT hr;
	ModuleID moduleId;
	ClassID parrentClassId;
	hr = profilerInfo.GetClassIDInfo(classId, &moduleId, &this->ClassMdToken);
	
	CheckError(hr);
	
	WCHAR name[NAME_BUFFER_SIZE];
	hr = pMetadataImport->GetTypeDefProps(this->ClassMdToken, name, NAME_BUFFER_SIZE, 0, 0, 0);
	CheckError(hr);
	this->Name.append(name);
}



wstring ClassMetadata::ToString(){
	bool isNonGenericType = this->TypeArgs.size() == 0;
	if(isNonGenericType)
		return this->Name;
	
	wstring wholeName(this->Name);
	wholeName.append(L"<");
	for ( vector<shared_ptr<ClassMetadata>>::iterator it = this->TypeArgs.begin(); it < this->TypeArgs.end(); it++ ){
		shared_ptr<ClassMetadata> typeArgClassMetadata = *it;
		wholeName.append(typeArgClassMetadata->ToString());
		
		bool lastElement = (++it)-- == this->TypeArgs.end();
		if(lastElement)
			break;
		wholeName.append(L", ");
	}

	if(this->_typeArgsListIncomplete)
		wholeName.append(L", ...");
	
	wholeName.append(L">");

	return wholeName;
}

void ClassMetadata::PopulateTypeArgs(ClassID typeArgsArray[], ULONG32 typeArgsCount, ICorProfilerInfo3 & profilerInfo){
	if(typeArgsCount != 0){
		this->_typeArgsListIncomplete = ENUM_ARRAY_SIZE < typeArgsCount;
		for(int i=0; i < typeArgsCount; i++){
			ClassID typeArgClassId = typeArgsArray[i];
			shared_ptr<ClassMetadata> pTypeArg = ClassMetadata::AddMetadata(typeArgClassId,profilerInfo);
			this->TypeArgs.push_back(pTypeArg);
		}
	}
}


