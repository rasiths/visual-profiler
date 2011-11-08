#pragma once
#include <memory>
#include <map>
#include <cor.h>
#include <corprof.h>
using namespace std;

template <class TId, class TMetadata>
class MetadataBase
{
public:

	static shared_ptr<TMetadata> GetMethodMetadataBy(TId id){
		shared_ptr<TMetadata> pMetadata = _cacheMap[id];
		return pMetadata;
	}

	static shared_ptr<TMetadata> AddMetadata(TId id, ICorProfilerInfo3 & profilerInfo, IMetaDataImport2* pMetadataImport = NULL){
		shared_ptr<TMetadata> pMetadata;
		if(!Contains(id)){
			pMetadata = shared_ptr<TMetadata>(new TMetadata(id, profilerInfo, pMetadataImport));
			_cacheMap[id] = pMetadata;
		}else{
			pMetadata = _cacheMap[id];
		}
		return pMetadata;
	}

	static bool Contains(TId id){
		bool contains = _cacheMap[id] != NULL;
		return contains;
	}

protected:
	static map<TId, shared_ptr<TMetadata>> _cacheMap;
};


template <class TId, class TMetadata>
map<TId, shared_ptr<TMetadata>> MetadataBase<TId, TMetadata>::_cacheMap;