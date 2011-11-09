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

	static void AddMetadata(TId id, shared_ptr<TMetadata> pMetadata){
		//Beep(200,200);
		if(!ContainsCache(id)){
			_cacheMap[id] = pMetadata;
		}
	}

	static bool ContainsCache(TId id){
		bool contains = _cacheMap[id] != NULL;
		return contains;
	}

	static shared_ptr<TMetadata> GetById(TId id){
		shared_ptr<TMetadata> pMetadata = _cacheMap[id];
		return pMetadata;
	}

	static int CacheSize(){
		int cacheSize = _cacheMap.size();
		return cacheSize;
	}
	
protected:
	static map<TId, shared_ptr<TMetadata>> _cacheMap;
};


template <class TId, class TMetadata>
map<TId, shared_ptr<TMetadata>> MetadataBase<TId, TMetadata>::_cacheMap;