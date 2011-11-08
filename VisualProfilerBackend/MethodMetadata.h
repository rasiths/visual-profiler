#pragma once
#include "stdafx.h"
#include <cor.h>
#include <corprof.h>
#include <vector>
#include <string>
#include <map>
#include "AssemblyMetadata.h"
#include "ClassMetadata.h"
#include "MetadataBase.h"

using namespace std;
class MethodMetadata : public MetadataBase<FunctionID, MethodMetadata>
{
public:
	FunctionID FunctionId;
	mdMethodDef MethodMdToken;
	wstring Name;
	vector<wstring> Parameters;
	shared_ptr<ClassMetadata> pContainingTypeMetadata;
		
	MethodMetadata(FunctionID functionId, ICorProfilerInfo3 & profilerInfo, IMetaDataImport2* pMetadataImport);
	~MethodMetadata(void);
	AssemblyMetadata & GetDefiningAssembly();
	wstring ToString();

private:
	
	IMetaDataImport2* _pMetaDataImport ;
		
	void InitializeFields(ICorProfilerInfo3 & profilerInfo);
	void PopulateParameters();
};

