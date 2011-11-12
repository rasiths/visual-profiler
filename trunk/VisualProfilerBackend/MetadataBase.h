#pragma once
#include <memory>
#include <map>
#include <cor.h>
#include <corprof.h>
#include "CriticalSection.h"
using namespace std;

template <class TId, class TMetadata>
class MetadataBase
{
public:

	static void AddMetadata(TId id, shared_ptr<TMetadata> pMetadata){
		if(!ContainsCache(id)){
			_criticalSection.Enter();
			_cacheMap[id] = pMetadata;
			_criticalSection.Leave();
		}
	}

	static bool ContainsCache(TId id){
		_criticalSection.Enter();
		bool contains = _cacheMap[id] != NULL;
		_criticalSection.Leave();
		return contains;
	}

	static shared_ptr<TMetadata> GetById(TId id){
		_criticalSection.Enter();
		shared_ptr<TMetadata> pMetadata = _cacheMap[id];
		_criticalSection.Leave();
		return pMetadata;
	}

	static int CacheSize(){
		_criticalSection.Enter();
		int cacheSize = _cacheMap.size();
		_criticalSection.Leave();
		return cacheSize;
	}
	
	static int Count;
private:
	static map<TId, shared_ptr<TMetadata>> _cacheMap;
	
	static CriticalSection _criticalSection;
};


template <class TId, class TMetadata>
map<TId, shared_ptr<TMetadata>> MetadataBase<TId, TMetadata>::_cacheMap;

template <class TId, class TMetadata>
int MetadataBase<TId, TMetadata>::Count = 0;

template <class TId, class TMetadata>
CriticalSection MetadataBase<TId, TMetadata>::_criticalSection;