#include "StdAfx.h"
#include "MethodMetadata.h"

MethodMetadata::MethodMetadata(FunctionID functionId, ICorProfilerInfo3 & profilerInfo)
{
	this->FunctionId = functionId;
	mdTypeDef definingTypeToken = this->GetContainingTypeMdTokenAndSetMethodProps(profilerInfo);
	this->SetParameters();
}

mdTypeDef MethodMetadata::GetContainingTypeMdTokenAndSetMethodProps(ICorProfilerInfo3 & profilerInfo){
	HRESULT hr;
	
	hr = profilerInfo.GetTokenAndMetaDataFromFunction(this->FunctionId,IID_IMetaDataImport2,(LPUNKNOWN *) &this->_pMetaDataImport, &this->MethodMdToken);
	CheckError(hr);

	WCHAR methodName[NAME_BUFFER_SIZE];
	mdTypeDef containingTypeMdToken;
	hr = _pMetaDataImport->GetMethodProps(this->MethodMdToken, &containingTypeMdToken, methodName, NAME_BUFFER_SIZE, 0, 0, 0, 0, 0, 0);
	
	CheckError(hr);

	this->Name.append(methodName);
	return containingTypeMdToken;
}

void MethodMetadata::SetParameters(){
	HRESULT hr;

	HCORENUM paramsEnum = 0;
	ULONG reaEnumlCount = 0;
	mdParamDef paramMdTokenArray[ENUM_ARRAY_SIZE];
	do{
		hr = _pMetaDataImport->EnumParams(&paramsEnum, this->MethodMdToken, paramMdTokenArray,ENUM_ARRAY_SIZE,&reaEnumlCount);
		
		CheckError(hr);
		
		for(unsigned int i = 0; i < reaEnumlCount; i++){
			mdParamDef paramMdToken = paramMdTokenArray[i];
			WCHAR paramName[NAME_BUFFER_SIZE];
			hr = _pMetaDataImport->GetParamProps(paramMdToken,0,0, paramName, NAME_BUFFER_SIZE,0,0,0,0,0);
			
			CheckError(hr);
			
			wstring paramNameString;
			paramNameString.append(paramName);
			this->Parameters.push_back(paramNameString);
		}
	}while(reaEnumlCount > 0);
}

wstring MethodMetadata::ToString(){
	wstring wholeName;
	wholeName.append(this->Name);
	wholeName.append(L"(");
	for ( vector<wstring>::iterator it = this->Parameters.begin(); it < this->Parameters.end(); it++ ){
		wholeName.append(*it);
		
		bool lastElement = (++it)-- == this->Parameters.end();
		if(lastElement)
			break;
		wholeName.append(L", ");
	}
	wholeName.append(L")");

    return wholeName;
}


MethodMetadata::~MethodMetadata(void)
{
	if(_pMetaDataImport != NULL){
		_pMetaDataImport->Release();
	}
}
