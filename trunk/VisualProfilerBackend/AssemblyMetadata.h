#pragma once
#include "stdafx.h"
#include <cor.h>
#include <corprof.h>
#include <map>


class AssemblyMetadata
{
private:
	IMetaDataAssemblyImport* metadataAssemblyImport;
public:
	mdAssembly metadataToken;
	WCHAR name;
	bool IsMarkedForProfiling;

	AssemblyMetadata(IMetaDataAssemblyImport* metadataAssemblyImport);
	~AssemblyMetadata(void);
};

