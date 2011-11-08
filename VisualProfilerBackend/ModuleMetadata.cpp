#include "StdAfx.h"
#include "ModuleMetadata.h"
#include "MetadataBase.h"

ModuleMetadata::ModuleMetadata(ModuleID moduleId, ICorProfilerInfo3 & profilerInfo, IMetaDataImport2* pMetadataImport):ModuleId(moduleId)
{
	HRESULT hr;
	WCHAR fileName[NAME_BUFFER_SIZE];
	hr = profilerInfo.GetModuleInfo(moduleId, &this->BaseLoadAddress,NAME_BUFFER_SIZE,0,fileName, &this->assemblyId);
	
	CheckError(hr);

	this->FileName.append(fileName);
}


ModuleMetadata::~ModuleMetadata(void)
{
}
