// TracingProfiler.cpp : Implementation of CTracingProfiler

#include "stdafx.h"
#include "TracingProfiler.h"
#include <iostream>
#include "MethodMetadata.h"
#include "AssemblyMetadata.h"

#define NAME_BUFFER_SIZE 1024

CTracingProfiler * tracingProfiler;
int calls = 0;
void __stdcall FunctionEnterGlobal(FunctionIDOrClientID functionIDOrClientID){
	if(functionIDOrClientID.functionID == 0)
		return;
	/*calls++;
	cout << calls << endl;*/
	shared_ptr<MethodMetadata> pMethodMetadata =  MethodMetadata::GetById(functionIDOrClientID.functionID);  
	std::wcout << L"Entering method " << pMethodMetadata->ToString() << std::endl;
	
}

void  _declspec(naked)  FunctionEnter3Naked(FunctionIDOrClientID functionIDOrClientID)
{
	__asm
	{
		push    ebp                 // Create a frame
		mov     ebp,esp
		pushad                      // Save registers
		mov     eax,[ebp+0x08]      // functionIDOrClientID
		push    eax;
		call    FunctionEnterGlobal
		popad                       // Restore registers
		pop     ebp                 // Restore EBP
		ret     4					// Return to caller and ESP = ESP+4 to clean the argument
	}
}

void _declspec(naked) FunctionLeave3Naked(FunctionIDOrClientID functionIDOrClientID)
{

	__asm
	{
		//int 3
			ret 4
	}
}

void _declspec(naked) FunctionTailcall3Naked(FunctionIDOrClientID functionIDOrClientID)
{

	__asm
	{
		//int 3
			ret    4
	}
}

HRESULT STDMETHODCALLTYPE CTracingProfiler::Initialize( /* [in] */ IUnknown *pICorProfilerInfoUnk) {
	CorProfilerCallbackBase::Initialize(pICorProfilerInfoUnk);
	DWORD mask = COR_PRF_MONITOR_ENTERLEAVE ;      
	_pICorProfilerInfo3->SetEventMask(mask);

	FunctionEnter3* enterFunction = &FunctionEnter3Naked;
	FunctionLeave3* leaveFunction = &FunctionLeave3Naked;
	FunctionTailcall3* tailcallFuntion = &FunctionTailcall3Naked;
	_pICorProfilerInfo3->SetFunctionIDMapper2(&FunctionMapper, this);
	_pICorProfilerInfo3->SetEnterLeaveFunctionHooks3(enterFunction, leaveFunction , tailcallFuntion);
	

	tracingProfiler = this;
	Beep(1000, 500);
	return S_OK;
}

void AssemblyMetadataFCE(mdTypeDef classTypeDef, IMetaDataAssemblyImport* pIMetaDataAssemblyImport ){
	mdAssembly  assemblyToken = 0;
	HRESULT hr = pIMetaDataAssemblyImport->GetAssemblyFromScope(&assemblyToken);
	
	WCHAR assemblyName[NAME_BUFFER_SIZE];
	hr = pIMetaDataAssemblyImport->GetAssemblyProps(assemblyToken,0,0,0, assemblyName,NAME_BUFFER_SIZE, 0, 0,0);
	

	IMetaDataImport2* pIMetaDataImport = 0;
	hr = pIMetaDataAssemblyImport->QueryInterface(IID_IMetaDataImport2,(void **)&pIMetaDataImport);
	std::wcout<<L"assembly:: "<<assemblyName<<std::endl;
	
	 BYTE *  pVal;
	 ULONG  cbVal;
	hr = pIMetaDataImport->GetCustomAttributeByName(assemblyToken, L"SandBox.ProfileMeAttribute", (const void**)&pVal, &cbVal);
	  if (hr == S_OK)
            {
				BYTE IsProfilingEnabledByte = pVal[2];
				bool isProfilingEnabled = IsProfilingEnabledByte > 0;
				std::wcout<< assemblyName << std::endl;
            }
	//return;

	HCORENUM attributeEnum = 0;
	mdCustomAttribute attributes[100];
	ULONG actualCout = 0;
	do{
		hr = pIMetaDataImport->EnumCustomAttributes(&attributeEnum,assemblyToken,0,attributes, 100, &actualCout);
		for(int i = 0; i < actualCout; i++){


			mdCustomAttribute attribute = attributes[i];

			mdToken             ptkObj;
			mdMemberRef        ptkType;
			const BYTE *  pVal;
			ULONG  cbVal;
			hr = pIMetaDataImport->GetCustomAttributeProps(attribute,  &ptkObj, &ptkType,(const void**)&pVal, &cbVal);

			WCHAR  szMember[NAME_BUFFER_SIZE];
			 mdTypeRef definingType;
			 hr =pIMetaDataImport->GetMemberRefProps( ptkType, &definingType, szMember, NAME_BUFFER_SIZE,0,0,0);
			 //std::cout<<szMember<<std::endl;
			 
			 WCHAR  szdefinigType[NAME_BUFFER_SIZE];
			 mdToken definingScope;
			 hr = pIMetaDataImport->GetTypeRefProps(definingType,&definingScope, szdefinigType, NAME_BUFFER_SIZE, 0);
			  
			  if(wcscmp(L"SandBox.ProfileMeAttribute", szdefinigType) == 0){
				  BYTE IsProfilingEnabledByte = pVal[2];
				bool isProfilingEnabled = IsProfilingEnabledByte > 0;
				  std::wcout<<szdefinigType<<std::endl;
			  }
			
		}														  

	}while(actualCout > 0);
	
}

HRESULT CTracingProfiler::GetMethodAndDefininingAssemblyForFunctionID(FunctionID functionID, mdAssembly * assembly, mdMethodDef* method){
//	HRESULT hr = _pICorProfilerInfo3->GetTokenAndMetaDataFromFunction(id,IID_IMetaDataImport2,(LPUNKNOWN *) &pIMetaDataImport, &funcToken);

	return S_OK;
}

UINT_PTR FunctionMapper2(FunctionID functionId, void * clientData, BOOL *pbHookFunction){
	
	return S_OK;
}

UINT_PTR CTracingProfiler::FunctionMapper(FunctionID functionId, void * clientData, BOOL *pbHookFunction)
{
	HRESULT hr;
	CorProfilerCallbackBase * profilerBase = (CorProfilerCallbackBase *) clientData;
	ICorProfilerInfo3 * pProfilerInfo = profilerBase->_pICorProfilerInfo3;

	shared_ptr<MethodMetadata> pMethodMetadata;
	if(MethodMetadata::ContainsCache(functionId)){
		pMethodMetadata = MethodMetadata::GetById(functionId);
	}else{
		pMethodMetadata = shared_ptr<MethodMetadata>(new MethodMetadata(functionId, pProfilerInfo));
		MethodMetadata::AddMetadata(functionId, pMethodMetadata);
	}
		
	//wstring methodName = pMethodMetadata->ToString();
//	std::wcout << L"Mapping method:: " << methodName << endl;
	
		   
	
	*pbHookFunction = pMethodMetadata->GetDefiningAssembly()->IsProfilingEnabled;
	return functionId;
#pragma region	hide me
	
	IMetaDataImport2* pIMetaDataImport = 0;

	mdToken funcToken = 0;
	WCHAR szFunction[NAME_BUFFER_SIZE];
	WCHAR szClass[NAME_BUFFER_SIZE];
	hr = tracingProfiler->_pICorProfilerInfo3->GetTokenAndMetaDataFromFunction(functionId,IID_IMetaDataImport2,(LPUNKNOWN *) &pIMetaDataImport, &funcToken);


	IMetaDataAssemblyImport* pIMetaDataAssemblyImport = 0;
	hr = tracingProfiler->_pICorProfilerInfo3->GetTokenAndMetaDataFromFunction(functionId,IID_IMetaDataAssemblyImport,(LPUNKNOWN *) &pIMetaDataAssemblyImport, 0);
	
	if(SUCCEEDED(hr))
	{
		mdTypeDef classTypeDef;
		ULONG cchFunction;
		ULONG cchClass;

		// retrieve the function properties based on the token
		hr = pIMetaDataImport->GetMethodProps(funcToken, &classTypeDef, szFunction, NAME_BUFFER_SIZE, &cchFunction, 0, 0, 0, 0, 0);
		if (SUCCEEDED(hr))
		{
			AssemblyMetadataFCE(classTypeDef, pIMetaDataAssemblyImport );
			// get the function name
			hr = pIMetaDataImport->GetTypeDefProps(classTypeDef, szClass, NAME_BUFFER_SIZE, &cchClass, 0, 0);
			if (SUCCEEDED(hr))
			{
				// create the fully qualified name
				std::wcout << szClass << "." <<szFunction << std::endl;
			}
		}
		// release our reference to the metadata
		mdModule moduleToken = 0;
		HRESULT moduleRes = pIMetaDataImport->GetModuleFromScope(&moduleToken);
		MDUTF8CSTR moduleName; 
		moduleRes = pIMetaDataImport->GetNameFromToken(classTypeDef, &moduleName);
		std::cout<< moduleName << std::endl;
		pIMetaDataImport->Release();
	}

			return S_OK;
#pragma endregion
}

