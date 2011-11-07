#include "StdAfx.h"
#include "MethodMetadata.h"
#define ENUM_ARRAY_SIZE 20
#define NAME_BUFFER_SIZE 1024

MethodMetadata::MethodMetadata(FunctionID functionId, ICorProfilerInfo3 & profilerInfo)
{
	HRESULT hr = S_OK;
	IMetaDataImport2* pMetaDataImport = 0;
	hr = profilerInfo.GetTokenAndMetaDataFromFunction(functionId,IID_IMetaDataImport2,(LPUNKNOWN *) &pMetaDataImport, &this->MethodMdToken);
	if(SUCCEEDED(hr)){
		
		WCHAR methodName[NAME_BUFFER_SIZE];
		mdTypeDef definingTypeToken;
		hr = pMetaDataImport->GetMethodProps(this->MethodMdToken, &definingTypeToken, methodName, NAME_BUFFER_SIZE, 0, 0, 0, 0, 0, 0);
		
		if(SUCCEEDED(hr)){
			Name.append(methodName);
			
			HCORENUM paramsEnum = 0;
			ULONG realCount = 0;
			mdParamDef paramArray[ENUM_ARRAY_SIZE];
			do{
				hr = pMetaDataImport->EnumParams(&paramsEnum, this->MethodMdToken, paramArray,ENUM_ARRAY_SIZE,&realCount);
				if(SUCCEEDED(hr)){
					for(int i = 0; i < realCount; i++){
						mdParamDef paramMdToken;
						ULONG paramsPosition;
						pMetaDataImport->GetParamProps(paramMdToken,0,&paramsPosition
					}
				}
			
			}while(actualCount > 0);
		}
	}

}


MethodMetadata::~MethodMetadata(void)
{
	if(_pContainingTypeMetadata != NULL)
		delete _pContainingTypeMetadata;
}
