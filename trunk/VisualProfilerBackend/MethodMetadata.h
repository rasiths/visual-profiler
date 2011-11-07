#pragma once
#include "stdafx.h"
#include <cor.h>
#include <corprof.h>
#include <vector>
#include <string>
#include <map>
#include "AssemblyMetadata.h"
#include "TypeMetadata.h"

using namespace std;
class MethodMetadata
{
public:
	FunctionID FunctionId;
	mdMethodDef MethodMdToken;
	wstring Name;
	vector<wstring> Parameters;
	
	
	MethodMetadata(FunctionID functionId, ICorProfilerInfo3 & profilerInfo);
	~MethodMetadata(void);

	AssemblyMetadata & GetDefiningAssembly();

	//static MethodMetadata & GetMethodMetadataByFunctionId(FunctionID functionID);

private:
	TypeMetadata * _pContainingTypeMetadata;
	//std::map<FunctionID, MethodMetadata&> _functionIDToMethodMetadataMap;

};

