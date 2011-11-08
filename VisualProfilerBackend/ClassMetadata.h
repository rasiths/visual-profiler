#pragma once
#include <cor.h>
#include <corprof.h>
#include <string>
#include <vector>
#include "MetadataBase.h"

using namespace std;

class ClassMetadata : public MetadataBase<ClassID, ClassMetadata>
{
public:
	ClassID ClassId;
	wstring Name;
	mdTypeDef ClassMdToken;
	vector<shared_ptr<ClassMetadata>> TypeArgs;

	ClassMetadata(ClassID classId, ICorProfilerInfo3 & profilerInfo, IMetaDataImport2* pMetadataImport);
	ClassMetadata(mdTypeDef classMdToken);
	wstring ToString();

private:
	bool _typeArgsListIncomplete;
	void PopulateTypeArgs(ClassID typeArgsArray[], ULONG32 typeArgsCount, ICorProfilerInfo3 & profilerInfo);
};

