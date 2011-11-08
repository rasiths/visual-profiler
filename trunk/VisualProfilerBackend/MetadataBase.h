#pragma once
#include <memory>
#include <map>
using namespace std;

template <class TId, class TMetadata>
class MetadataBase
{
public:
	

	//shared_ptr<TMetadata> GetMethodMetadataBy(TId id);
	//void AddMethodMetadata(TId id, shared_ptr<TMetadata> pMethodMetadata);
	//bool Contains(TId id);



static shared_ptr<TMetadata> GetMethodMetadataBy(TId id){
	shared_ptr<TMetadata> pMetadata = MetadataBase::_cacheMap[id];
	return pMetadata;
}


static void AddMethodMetadata(TId id, shared_ptr<TMetadata> pMethodMetadata){
		
	MetadataBase::_cacheMap[id] = pMethodMetadata;
}


static bool Contains(TId id){
	bool contains = true; MetadataBase::_cacheMap[id] != NULL;
	return contains;
}
protected:
	static map<TId, shared_ptr<TMetadata>> _cacheMap;

};

template <class TId, class TMetadata>
map<TId, shared_ptr<TMetadata>> MetadataBase<TId, TMetadata>::_cacheMap;