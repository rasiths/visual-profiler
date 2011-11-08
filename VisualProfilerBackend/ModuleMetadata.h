#pragma once
#include "stdafx.h"
#include <cor.h>
#include <corprof.h>
#include <string>
#include <map>
#include "MetadataBase.h"

using namespace std;

class ModuleMetadata : public MetadataBase<ModuleID, ModuleMetadata>
{
public:
	ModuleID ModuleId;
	wstring FileName;
	LPCBYTE BaseLoadAddress;  

	ModuleMetadata(ModuleID moduleId, ICorProfilerInfo3 & profilerInfo, IMetaDataImport2* pMetadataImport);
	~ModuleMetadata(void);

private:
	AssemblyID assemblyId;
};

