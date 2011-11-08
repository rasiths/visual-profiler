#pragma once
#include "stdafx.h"
#include <cor.h>
#include <corprof.h>
#include <vector>
#include <string>
#include <map>
#include "AssemblyMetadata.h"
#include "TypeMetadata.h"
#include "MetadataBase.h"

using namespace std;
class MethodMetadata : public MetadataBase<FunctionID, MethodMetadata>
{
public:
	FunctionID FunctionId;
	mdMethodDef MethodMdToken;
	wstring Name;
	vector<wstring> Parameters;
		
	MethodMetadata(FunctionID functionId, ICorProfilerInfo3 & profilerInfo);
	~MethodMetadata(void);
	AssemblyMetadata & GetDefiningAssembly();
	wstring ToString();

private:
	shared_ptr<TypeMetadata> _pContainingTypeMetadata;
	IMetaDataImport2* _pMetaDataImport ;
		
	mdTypeDef GetContainingTypeMdTokenAndSetMethodProps(ICorProfilerInfo3 & profilerInfo);
	void SetParameters();
};

