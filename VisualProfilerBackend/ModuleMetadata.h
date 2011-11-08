#pragma once
#include "stdafx.h"
#include <cor.h>
#include <corprof.h>
#include <string>
#include <map>

using namespace std;

class ModuleMetadata
{
public:
	ModuleID ModuleId;
	mdModule ModuleMdToken;
	wstring FileName;
	LPCBYTE BaseLoadAddress;  

	ModuleMetadata(ModuleID moduleId, ICorProfilerInfo3 & profilerInfo);
	~ModuleMetadata(void);

private:
	AssemblyID assemblyId;
};

